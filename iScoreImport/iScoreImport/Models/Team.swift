//
//  Team.swift
//  iScoreImport
//
//  Created by Patrick Eschenfeldt on 6/29/24.
//

import Foundation

struct Team : Codable {
    let ExternalId: UUID?
    var City: String?
    var Name: String?
    let CombinedName: String?
    let HomePark: Park?
    let ColorHex: String?
    
    init(ExternalId: UUID? = nil, City: String? = nil, Name: String? = nil, CombinedName: String? = nil, HomePark: Park? = nil, ColorHex: String? = nil) {
        self.ExternalId = ExternalId
        self.City = City
        self.Name = Name
        self.CombinedName = CombinedName
        self.HomePark = HomePark
        self.ColorHex = ColorHex
    }
    
    static let compoundCityStarts = Set<String>([
        "Los", "Las", "San", "St.", "Great", "New", "Quad", "Lake", "Kane", "Tampa"
    ])
    
    static func withParsedName(team: Team) -> Team {
        guard let combinedName = team.CombinedName else {
            return team
        }
        let components = combinedName.split(separator: " ")
        var city: String? = nil
        var name: String? = nil
        if components.count == 2 {
            city = String(components[0])
            name = String(components[1])
        } else if components.count >= 3 {
            let first = String(components[0])
            if Team.compoundCityStarts.contains(first) {
                // Compound city name (assumes no 3-word cities)
                city = components.prefix(through: 1).joined(separator: " ")
                name = components.dropFirst(2).joined(separator: " ")
            } else {
                city = String(components[0])
                name = components.dropFirst().joined(separator: " ")
            }
        } else {
            name = combinedName
        }
        return Team(ExternalId: team.ExternalId, City: city, Name: name, CombinedName: combinedName, HomePark: team.HomePark, ColorHex: team.ColorHex)
    }
}
