import SwiftUI

struct ContentView: View {
    @State private var quotes = QuoteLoader.loadQuotes()
    @State private var currentQuote: Quote?
    

    var body: some View {
        VStack(spacing: 20) {
            Spacer()
            if let quote = currentQuote {
                Text("“\(quote.text)”")
                    .font(.title2)
                    .multilineTextAlignment(.center)
                    .padding()
                Text("— \(quote.author)")
                    .font(.subheadline)
                    .foregroundColor(.gray)
                Text("Цитат загружено: \(quotes.count)")
                    .foregroundColor(.red)
                    .padding()

            } else {
                Text("Нажмите кнопку для цитаты")
                    .foregroundColor(.secondary)
            }
            Spacer()
            Button(action: {
                print("Кнопка нажата")
                currentQuote = quotes.randomElement()
            }) {
                Text("Новая цитата")
                    .padding()
                    .background(Color.blue)
                    .foregroundColor(.white)
                    .cornerRadius(10)
            }
            Spacer()
        }
        .padding()
        .onAppear {
            currentQuote = quotes.randomElement()
        }
    }
}
