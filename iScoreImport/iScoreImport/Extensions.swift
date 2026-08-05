//
//  Extensions.swift
//  iScoreImport
//
//  Created by Patrick Eschenfeldt on 7/4/24.
//

import Foundation

extension TimeZone {
    /// Park time zones are stored using .NET's Windows identifiers, which Foundation doesn't
    /// recognize, so translate the ones we can before falling back to an IANA lookup.
    init?(parkIdentifier: String) {
        self.init(identifier: TimeZone.ianaIdentifiers[parkIdentifier] ?? parkIdentifier)
    }

    private static let ianaIdentifiers = [
        "Eastern Standard Time": "America/New_York",
        "Central Standard Time": "America/Chicago",
        "Mountain Standard Time": "America/Denver",
        "US Mountain Standard Time": "America/Phoenix",
        "Pacific Standard Time": "America/Los_Angeles",
        "Alaskan Standard Time": "America/Anchorage",
        "Hawaiian Standard Time": "Pacific/Honolulu",
        "Atlantic Standard Time": "America/Halifax",
        "Central Standard Time (Mexico)": "America/Mexico_City",
        "GMT Standard Time": "Europe/London",
        "Tokyo Standard Time": "Asia/Tokyo",
        "Korea Standard Time": "Asia/Seoul"
    ]
}

// from https://www.swiftbysundell.com/articles/async-and-concurrent-forEach-and-map/
extension Sequence {
    func asyncMap<T> (
        _ transform: (Element) async throws -> T
    ) async rethrows -> [T] {
        var values = [T]()
        
        for element in self {
            try await values.append(transform(element))
        }
        
        return values
    }
}
