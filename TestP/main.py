from __future__ import annotations
import os, ssl, smtplib, json, time, hashlib, hmac, threading
import urllib.request, urllib.parse
from dataclasses import dataclass
from typing import Optional, Protocol, Tuple, Dict, List

# Модель пользователя
@dataclass
class User:
    id: str
    email: Optional[str] = None
    phone: Optional[str] = None
    telegram_id: Optional[str] = None
    channel_order: Optional[List[str]] = None  # например: ["email", "telegram", "sms"]
# Результат доставки
@dataclass
class DeliveryResult:
    ok: bool
    provider_status: str
    details: Optional[str] = None
# Контракт
class Channel(Protocol):
    name: str
    def available_for(self, user: User) -> bool: ...
    def send(self, user: User, subject: str, body: str, timeout_sec: float = 5.0) -> DeliveryResult: ...
# Email
class EmailChannel:
    name = "email"
    def __init__(self) -> None:
        self.host = os.getenv("SMTP_HOST", "")
        self.port = int(os.getenv("SMTP_PORT", "587"))
        self.user = os.getenv("SMTP_USER", "")
        self.password = os.getenv("SMTP_PASS", "")
        self.sender = os.getenv("SMTP_SENDER", self.user or "no-reply@localhost")
    def available_for(self, user: User) -> bool:
        return bool(user.email and self.host and self.user and self.password)
    def send(self, user: User, subject: str, body: str, timeout_sec: float = 8.0) -> DeliveryResult:
        if not self.available_for(user):
            return DeliveryResult(False, "email_not_configured_or_missing_recipient")
        msg = (
            f"From: {self.sender}\r\n"
            f"To: {user.email}\r\n"
            f"Subject: {subject}\r\n"
            f"Content-Type: text/plain; charset=utf-8\r\n"
            f"\r\n{body}\r\n"
        ).encode("utf-8")
        context = ssl.create_default_context()
        try:
            with smtplib.SMTP(self.host, self.port, timeout=timeout_sec) as server:
                server.starttls(context=context)
                server.login(self.user, self.password)
                server.sendmail(self.sender, [user.email], msg)
            return DeliveryResult(True, "email_sent")
        except Exception as e:
            return DeliveryResult(False, "email_error", details=str(e))
# Telegram (Bot API)
class TelegramChannel:
    name = "telegram"
    def __init__(self) -> None:
        self.token = os.getenv("TELEGRAM_BOT_TOKEN", "")
    def available_for(self, user: User) -> bool:
        return bool(self.token and user.telegram_id)
    def send(self, user: User, subject: str, body: str, timeout_sec: float = 5.0) -> DeliveryResult:
        if not self.available_for(user):
            return DeliveryResult(False, "telegram_not_configured_or_missing_recipient")
        text = f"*{subject}*\n{body}"
        url = f"https://api.telegram.org/bot{self.token}/sendMessage"
        data = urllib.parse.urlencode({
            "chat_id": user.telegram_id,
            "text": text,
            "parse_mode": "Markdown"
        }).encode("utf-8")
        req = urllib.request.Request(url, data=data, method="POST")
        try:
            with urllib.request.urlopen(req, timeout=timeout_sec) as resp:
                payload = resp.read().decode("utf-8")
                jd = json.loads(payload)
                if jd.get("ok"):
                    return DeliveryResult(True, "telegram_sent")
                return DeliveryResult(False, "telegram_api_error", details=payload)
        except Exception as e:
            return DeliveryResult(False, "telegram_http_error", details=str(e))
# SMS канал (заглушка или HTTP-провайдер)
class SmsChannel:
    name = "sms"
    def __init__(self) -> None:
        self.endpoint = os.getenv("SMS_ENDPOINT", "")  # например, https://api.provider/send
        self.token = os.getenv("SMS_TOKEN", "")
    def available_for(self, user: User) -> bool:
        return bool(user.phone)
    def send(self, user: User, subject: str, body: str, timeout_sec: float = 5.0) -> DeliveryResult:
        if not self.available_for(user):
            return DeliveryResult(False, "sms_missing_recipient")
        # Моковый режим (без внешних зависимостей)
        if not self.endpoint or not self.token:
            # имитация успеха
            print(f"[SMS MOCK] to={user.phone} subj={subject!r} body={body!r}")
            return DeliveryResult(True, "sms_mock_sent")
        try:
            payload = json.dumps({
                "to": user.phone,
                "text": f"{subject}\n{body}"
            }).encode("utf-8")
            req = urllib.request.Request(
                self.endpoint,
                data=payload,
                method="POST",
                headers={"Authorization": f"Bearer {self.token}", "Content-Type": "application/json"}
            )
            with urllib.request.urlopen(req, timeout=timeout_sec) as resp:
                raw = resp.read().decode("utf-8")
                # Провайдер-специфичная проверка ответа
                return DeliveryResult(True, "sms_sent", details=raw)
        except Exception as e:
            return DeliveryResult(False, "sms_error", details=str(e))
class Deduper:
    def __init__(self, ttl_sec: int = 600) -> None:
        self._store: Dict[str, float] = {}
        self._ttl = ttl_sec
        self._lock = threading.Lock()
    def _key(self, user: User, subject: str, body: str) -> str:
        h = hashlib.sha256()
        h.update(user.id.encode())
        h.update(subject.encode("utf-8"))
        sig = hmac.new(b"body-salt", body.encode("utf-8"), hashlib.sha256).hexdigest()
        h.update(sig.encode())
        return h.hexdigest()
    def seen(self, user: User, subject: str, body: str) -> bool:
        k = self._key(user, subject, body)
        now = time.time()
        with self._lock:
            # очистка протухших
            for kk, ts in list(self._store.items()):
                if now - ts > self._ttl:
                    del self._store[kk]
            if k in self._store:
                return True
            self._store[k] = now
            return False
class NotificationService:
    def __init__(
        self,
        default_order: List[str] | None = None,
        max_retries_per_channel: int = 2,
        base_retry_sleep_sec: float = 1.0,
        dedupe_ttl_sec: int = 600
    ) -> None:
        self.channels: Dict[str, Channel] = {
            "email": EmailChannel(),
            "telegram": TelegramChannel(),
            "sms": SmsChannel(),
        }
        self.default_order = default_order or ["email", "telegram", "sms"]
        self.max_retries = max_retries_per_channel
        self.base_sleep = base_retry_sleep_sec
        self.deduper = Deduper(ttl_sec=dedupe_ttl_sec)
    def notify(self, user: User, subject: str, body: str) -> Tuple[bool, str]:
        # Идемпотентность
        if self.deduper.seen(user, subject, body):
            return True, "duplicate_suppressed"

        order = user.channel_order or self.default_order
        last_err = ""
        for ch_name in order:
            ch = self.channels.get(ch_name)
            if not ch:
                continue
            if not ch.available_for(user):
                continue

            for attempt in range(self.max_retries + 1):
                res = ch.send(user, subject, body)
                if res.ok:
                    return True, f"{ch_name}:{res.provider_status}"
                last_err = f"{ch_name}:{res.provider_status} ({res.details})" if res.details else f"{ch_name}:{res.provider_status}"
                # экспоненциальный бэк-офф
                sleep_for = self.base_sleep * (2 ** attempt)
                time.sleep(min(sleep_for, 10.0))
            # если канал исчерпал ретраи — переходим к следующему
        return False, f"all_channels_failed; last={last_err or 'none'}"
# Демонстрация
def _demo():
    svc = NotificationService()
    user = User(
        id="alex",
        email=os.getenv("DEMO_EMAIL"),
        phone=os.getenv("DEMO_PHONE", "+48111111111"),
        telegram_id=os.getenv("DEMO_TG_ID"),    # chat_id (число в строке)
        channel_order=None                      # или ["telegram","email","sms"]
    )
    ok, status = svc.notify(user, subject="Проверка доставки", body="Привет! Тест уведомления.")
    print("RESULT:", ok, status)

if __name__ == "__main__":
    _demo()
