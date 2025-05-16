import Foundation

struct Quote: Codable, Identifiable {
    let id = UUID()       // фиксированный ID при создании
    let text: String
    let author: String
}
