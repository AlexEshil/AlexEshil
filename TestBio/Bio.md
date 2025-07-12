# BiogenomAPI

Простой API для хранения и получения результатов нутрициональной оценки.

---

## Описание

API реализован на .NET 7 с использованием Entity Framework Core и PostgreSQL.  
Подключение к базе данных происходит через Docker-контейнер с PostgreSQL.

---

## Быстрый старт на MacOS через Docker

1. Запустите PostgreSQL в Docker:  
```bash
docker run --name biogenom-postgres -e POSTGRES_PASSWORD=yourpassword -p 5432:5432 -d postgres:15
```

2. Создайте базу данных, подключившись к контейнеру через любой клиент (psql, pgAdmin):  
```sql
CREATE DATABASE biogenom_db;
```

3. Проверьте строку подключения в `appsettings.json`:  
```json
"DefaultConnection": "Host=localhost;Port=5432;Database=biogenom_db;Username=postgres;Password=yourpassword"
```

4. Запустите API:  
```bash
dotnet run
```

5. API доступен по адресу:  
```
http://localhost:5000
```

---

## Доступные эндпоинты

- **GET** `/api/NutritionAssessment/last` — получить последний сохранённый результат.

- **POST** `/api/NutritionAssessment` — сохранить новый результат (при сохранении старые удаляются).

---

## Примечания

- Миграции в базе данных не используются, таблица создаётся вручную.  
- Swagger UI доступен по адресу:  
```
http://localhost:5000/swagger/index.html
```

---

## Структура проекта

- **Controllers** — контроллеры API  
- **Data** — контекст базы данных  
- **Models** — модели данных

Проект написан на .NET 7.0, который официально вышел из поддержки (End of Support).
Рекомендуется использовать .NET 7 для совместимости с текущим кодом, но с пониманием, что обновления безопасности и исправления уже не выходят.


