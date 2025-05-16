import Foundation

class QuoteLoader {
    static func loadQuotes() -> [Quote] {
        guard let url = Bundle.main.url(forResource: "quotes", withExtension: "json") else {
            print("quotes.json не найден")
            return []
        }

        do {
            let data = try Data(contentsOf: url)
            let quotes = try JSONDecoder().decode([Quote].self, from: data)
            print("Загружено цитат: \(quotes.count)")
            return quotes
        } catch {
            print("Ошибка при загрузке цитат: \(error)")
            return []
        }
    }
}
