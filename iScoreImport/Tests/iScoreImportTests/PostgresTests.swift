//
//  PostgresTests.swift
//  iScoreImportTests
//
//  Created by Patrick Eschenfeldt on 6/28/24.
//

import Foundation
import SQLKit
import Testing

@testable import iScoreImport

@Suite("Postgres Tests", .enabled(if: TestPostgres.isConfigured))
struct PostgresTests {

    @Test("Connection Test")
    func testConnection() async throws {
        try await TestPostgres.withScratchDatabase { connector in
            let db = try await connector.getDb()
            try await connector.insertOrUpdateTeam(team: Team(City: "Test City", Name: "Test Team"))

            let result = try await db.select()
                .column("Name")
                .from("Teams")
                .limit(1)
                .all()
            try #require(result.count == 1)
            let decoded = try result[0].decode(column: "Name", as: String.self)
            #expect(decoded == "Test Team")
        }
    }

    @Test("Get Team ID")
    func testGetTeamId() async throws {
        try await TestPostgres.withScratchDatabase { connector in
            let db = try await connector.getDb()
            try await connector.insertOrUpdateTeam(team: Team(City: "Test City", Name: "Test Team"))

            let inserted = try await db.select()
                .column("Id")
                .from("Teams")
                .first()
            let insertedId = try #require(inserted).decode(column: "Id", as: Int.self)

            let testTeamId = try await connector.getTeamId(city: "Test City", name: "Test Team")
            #expect(testTeamId == insertedId)
        }
    }

    @Test("Team Insert")
    func testTeamInsert() async throws {
        try await TestPostgres.withScratchDatabase { connector in
            let db = try await connector.getDb()
            let city = "Test City"
            let name = "Testers"
            try await connector.insertOrUpdateTeam(team: Team(City: city, Name: name))

            let savedTeamId = try #require(await connector.getTeamId(city: city, name: name))
            try await deleteTeam(db: db, teamId: savedTeamId)
            let deletedTeamId = try await connector.getTeamId(city: city, name: name)
            #expect(deletedTeamId == nil)
        }
    }

    @Test("Team Update")
    func testTeamUpdate() async throws {
        try await TestPostgres.withScratchDatabase { connector in
            let db = try await connector.getDb()
            let city = "Test City"
            let name = "Testers"
            let externalId = UUID()
            try await connector.insertOrUpdateTeam(team: Team(ExternalId: externalId, City: city, Name: name))

            let savedTeamId = try #require(await connector.getTeamId(city: city, name: name))

            // Same ExternalId with a new name should update the row, not add one.
            let updatedName = "New Testers"
            try await connector.insertOrUpdateTeam(team: Team(ExternalId: externalId, City: city, Name: updatedName))

            let savedNameRaw = try await db.select()
                .column("Name")
                .from("Teams")
                .where("Id", .equal, savedTeamId)
                .first()
            let savedName = try #require(savedNameRaw).decode(column: "Name", as: String.self)
            #expect(savedName == updatedName)

            try await deleteTeam(db: db, teamId: savedTeamId)
            let deletedTeamId = try await connector.getTeamId(city: city, name: updatedName)
            #expect(deletedTeamId == nil)
        }
    }

    private func deleteTeam(db: SQLDatabase, teamId: Int) async throws {
        try await db.delete(from: "Teams")
            .where("Id", .equal, teamId)
            .run()
    }

    @Test("Player Insert")
    func testPlayerInsert() async throws {
        try await TestPostgres.withScratchDatabase { connector in
            let db = try await connector.getDb()
            let firstName = "A.J"
            let lastName = "Test"
            let name = "\(firstName) \(lastName)"
            let player = Player(Name: name, ExternalId: nil, DateOfBirth: nil, FirstName: firstName, MiddleName: nil, LastName: lastName, Suffix: nil)
            try await connector.insertOrUpdatePlayer(player: player)

            let savedPlayerId = try #require(await connector.getPlayerId(name: name))
            try await deletePlayer(db: db, playerId: savedPlayerId)
            let deletedPlayerId = try await connector.getPlayerId(name: name)
            #expect(deletedPlayerId == nil)
        }
    }

    @Test("Player Update")
    func testPlayerUpdate() async throws {
        try await TestPostgres.withScratchDatabase { connector in
            let db = try await connector.getDb()
            let firstName = "A.J"
            let lastName = "Test"
            let name = "\(firstName) \(lastName)"
            let externalId = UUID()
            let player = Player(Name: name, ExternalId: externalId, DateOfBirth: nil, FirstName: firstName, MiddleName: nil, LastName: lastName, Suffix: nil)
            try await connector.insertOrUpdatePlayer(player: player)

            let savedPlayerId = try #require(await connector.getPlayerId(name: name))

            let updatedLastName = "New Test"
            let updatedName = "\(firstName) \(updatedLastName)"
            let updatedPlayer = Player(
                Name: updatedName, ExternalId: externalId, DateOfBirth: nil, FirstName: firstName, MiddleName: nil, LastName: updatedLastName, Suffix: nil)
            try await connector.insertOrUpdatePlayer(player: updatedPlayer)

            let savedNameRaw = try await db.select()
                .column("Name")
                .from("Players")
                .where("Id", .equal, savedPlayerId)
                .first()
            let savedName = try #require(savedNameRaw).decode(column: "Name", as: String.self)
            #expect(savedName == updatedName)

            try await deletePlayer(db: db, playerId: savedPlayerId)
            let deletedPlayerId = try await connector.getPlayerId(name: updatedName)
            #expect(deletedPlayerId == nil)
        }
    }

    private func deletePlayer(db: SQLDatabase, playerId: Int) async throws {
        try await db.delete(from: "Players")
            .where("Id", .equal, playerId)
            .run()
    }
}
