//
//  PlayerImporter.swift
//  iScoreImport
//
//  Created by Patrick Eschenfeldt on 7/4/24.
//

import Foundation
import SQLKit

struct PlayerImporter {
    let db: SQLDatabase
    
    func insertOrUpdatePlayer(player: Player) async throws {
        var existingPlayerId: Int? = nil
        if let externalId = player.ExternalId {
            existingPlayerId = try await getPlayerId(externalId: externalId)
        }
        if existingPlayerId == nil {
            existingPlayerId = try await getPlayerId(name: player.Name)
        }
        if let existingPlayerId {
            try await updatePlayer(player: player, id: existingPlayerId)
        } else {
            try await insertPlayer(player: player)
        }
    }
    
    func getPlayerId(externalId: UUID) async throws -> Int? {
        let result = try await db.select()
            .column("Id")
            .from("Players")
            .where("ExternalId", .equal, externalId)
            .first()
        return try result?.decode(column: "Id", as: Int.self)
    }
    
    func getPlayerId(name: String) async throws -> Int? {
        let result = try await db.select()
            .column("Id")
            .from("Players")
            .where("Name", .equal, name)
            .first()
        return try result?.decode(column: "Id", as: Int.self)
    }
    
    private func insertPlayer(player: Player) async throws {
        try await db.insert(into: "Players")
            .columns("ExternalId", "Name", "FirstName", "LastName")
            .values(player.ExternalId ?? UUID(uuidString: "00000000-0000-0000-0000-000000000000"),
                    player.Name, player.FirstName, player.LastName)
            .run()
    }
    
    private func updatePlayer(player: Player, id: Int) async throws {
        try await db.update("Players")
            .set("ExternalId", to: player.ExternalId)
            .set("FirstName", to: player.FirstName)
            .set("LastName", to: player.LastName)
            .set("Name", to: player.Name)
            .where("Id", .equal, id)
            .run()
    }
}
