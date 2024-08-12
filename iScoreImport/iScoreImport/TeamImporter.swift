//
//  TeamImporter.swift
//  iScoreImport
//
//  Created by Patrick Eschenfeldt on 7/4/24.
//

import Foundation
import SQLKit

struct TeamImporter {
    let db: SQLDatabase
    
    func insertOrUpdateTeam(team: Team) async throws {
        var existingTeamId: Int? = nil
        if let externalId = team.ExternalId {
            existingTeamId = try await getTeamId(externalId: externalId)
        }
        if existingTeamId == nil, let city = team.City, let name = team.Name {
            existingTeamId = try await getTeamId(city: city, name: name)
        }
        if let existingTeamId {
            try await updateTeam(team: team, id: existingTeamId)
        } else {
            try await insertTeam(team: team)
        }
    }
    
    private func getTeamId(externalId: UUID) async throws -> Int? {
        let result = try await db.select()
            .column("Id")
            .from("Teams")
            .where("ExternalId", .equal, externalId)
            .first()
        return try result?.decode(column: "Id", as: Int.self)
    }
    
    func getTeamId(city: String, name: String) async throws -> Int? {
        let result = try await db.select()
            .column("Id")
            .from("Teams")
            .where("City", .equal, city)
            .where("Name", .equal, name)
            .first()
        return try result?.decode(column: "Id", as: Int.self)
    }
    
    private func insertTeam(team: Team) async throws {
        try await db.insert(into: "Teams")
            .columns("ExternalId", "City", "Name")
            .values(team.ExternalId ?? UUID(uuidString: "00000000-0000-0000-0000-000000000000"), team.City, team.Name)
            .run()
    }
    
    private func updateTeam(team: Team, id: Int) async throws {
        try await db.update("Teams")
            .set("ExternalId", to: team.ExternalId)
            .set("City", to: team.City)
            .set("Name", to: team.Name)
            .set("ColorHex", to: team.ColorHex)
            .where("Id", .equal, id)
            .run()
    }
}
