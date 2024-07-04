//
//  PostgresTests.swift
//  Tests
//
//  Created by Patrick Eschenfeldt on 6/28/24.
//

import Foundation
import Testing
import SQLKit

final class PostgresTests {
    
    let configFileName = "test"
    let configFilePath: String?
    
    init() {
        let testBundle = Bundle(for: type(of: self))
        guard let resourceURL = testBundle.url(forResource: configFileName, withExtension: "postgres_config") else {
            configFilePath = nil
            return
        }
        configFilePath = resourceURL.absoluteString
    }

    @Test("Connection Test")
    func testConnection() async throws {
        #expect(configFilePath != nil)
        guard let configFilePath else { return }
        var connector = PostgresConnector(configFilePath: configFilePath, pathIsLocalFile: false)
        do {
            let db = try await connector.getDb()
            let result = try await db.select()
                .column("Name")
                .from("Teams")
                .limit(1)
                .all()
            try #require(result.count == 1)
            let decoded = try result[0].decode(column: "Name", as: String.self)
            #expect(decoded == "Test Team")
        } catch {
            print(String(reflecting: error))
            #expect(error == nil)
        }
        try await connector.close()
    }
    
    @Test("Get Team ID")
    func testGetTeamId() async throws {
        #expect(configFilePath != nil)
        guard let configFilePath else { return }
        var connector = PostgresConnector(configFilePath: configFilePath, pathIsLocalFile: false)
        do {
            try await connector.connect()
            let testTeamId = try await connector.getTeamId(city: "Test City", name: "Test Team")
            #expect(testTeamId == 1)
        } catch {
            print(String(reflecting: error))
            #expect(error == nil)
        }
        try await connector.close()
    }

    @Test("Team Insert")
    func testTeamInsert() async throws {
        #expect(configFilePath != nil)
        guard let configFilePath else { return }
        var connector = PostgresConnector(configFilePath: configFilePath, pathIsLocalFile: false)
        let city = "Test City"
        let name = "Testers \(UUID())"
        let team = Team(City: city, Name: name)
        do {
            try await connector.connect()
            try await connector.insertOrUpdateTeam(team: team)
            let savedTeamId = try await connector.getTeamId(city: city, name: name)
            #expect(savedTeamId != nil)
            if let savedTeamId {
                let db = try await connector.getDb()
                try await deleteTeam(db: db, teamId: savedTeamId)
                let deletedTeamId = try await connector.getTeamId(city: city, name: name)
                #expect(deletedTeamId == nil)
            }
        } catch {
            print(String(reflecting: error))
            #expect(error == nil)
        }
        try await connector.close()
    }
    
    @Test("Team Update")
    func testTeamUpdate() async throws {
        #expect(configFilePath != nil)
        guard let configFilePath else { return }
        var connector = PostgresConnector(configFilePath: configFilePath, pathIsLocalFile: false)
        let city = "Test City"
        let name = "Testers \(UUID())"
        let externalId = UUID()
        let team = Team(ExternalId: externalId, City: city, Name: name)
        do {
            try await connector.connect()
            try await connector.insertOrUpdateTeam(team: team)
            let savedTeamId = try await connector.getTeamId(city: city, name: name)
            #expect(savedTeamId != nil)
            if let savedTeamId {
                let updatedName = "New Testers \(UUID())"
                let updatedTeam = Team(ExternalId: externalId, City: city, Name: updatedName)
                try await connector.insertOrUpdateTeam(team: updatedTeam)
                let db = try await connector.getDb()
                let savedNameRaw = try await db.select()
                    .column("Name")
                    .from("Teams")
                    .where("Id", .equal, savedTeamId)
                    .first()
                #expect(savedNameRaw != nil)
                let savedName = try savedNameRaw?.decode(column: "Name", as: String.self)
                #expect(savedName == updatedName)
                try await deleteTeam(db: db, teamId: savedTeamId)
                let deletedTeamId = try await connector.getTeamId(city: city, name: name)
                #expect(deletedTeamId == nil)
            }
        } catch {
            print(String(reflecting: error))
            #expect(error == nil)
        }
        try await connector.close()
    }
    
    private func deleteTeam(db: SQLDatabase, teamId: Int) async throws {
        try await db.delete(from: "Teams")
            .where("Id", .equal, teamId)
            .run()
    }
    
    
    @Test("Player Insert")
    func testPlayerInsert() async throws {
        #expect(configFilePath != nil)
        guard let configFilePath else { return }
        var connector = PostgresConnector(configFilePath: configFilePath, pathIsLocalFile: false)
        let firstName = "A.J"
        let lastName = "Test \(UUID())"
        let name = "\(firstName) \(lastName)"
        let player = Player(Name: name, ExternalId: nil, DateOfBirth: nil, FirstName: firstName, MiddleName: nil, LastName: lastName, Suffix: nil)
        do {
            try await connector.connect()
            try await connector.insertOrUpdatePlayer(player: player)
            let savedPlayerId = try await connector.getPlayerId(name: name)
            #expect(savedPlayerId != nil)
            if let savedPlayerId {
                let db = try await connector.getDb()
                try await deletePlayer(db: db, playerId: savedPlayerId)
                let deletedPlayerId = try await connector.getPlayerId(name: name)
                #expect(deletedPlayerId == nil)
            }
        } catch {
            print(String(reflecting: error))
            #expect(error == nil)
        }
        try await connector.close()
    }
    
    @Test("Player Update")
    func testPlayerUpdate() async throws {
        #expect(configFilePath != nil)
        guard let configFilePath else { return }
        var connector = PostgresConnector(configFilePath: configFilePath, pathIsLocalFile: false)
        let firstName = "A.J"
        let lastName = "Test \(UUID())"
        let name = "\(firstName) \(lastName)"
        let externalId = UUID()
        let player = Player(Name: name, ExternalId: externalId, DateOfBirth: nil, FirstName: firstName, MiddleName: nil, LastName: lastName, Suffix: nil)
        do {
            try await connector.connect()
            try await connector.insertOrUpdatePlayer(player: player)
            let savedPLayerId = try await connector.getPlayerId(name: name)
            if let savedPLayerId {
                let updatedLastName = "New Test \(UUID())"
                let updatedName = "\(firstName) \(updatedLastName)"
                let updatedPlayer = Player(Name: updatedName, ExternalId: externalId, DateOfBirth: nil, FirstName: firstName, MiddleName: nil, LastName: updatedLastName, Suffix: nil)
                try await connector.insertOrUpdatePlayer(player: updatedPlayer)
                let db = try await connector.getDb()
                let savedNameRaw = try await db.select()
                    .column("Name")
                    .from("Players")
                    .where("Id", .equal, savedPLayerId)
                    .first()
                #expect(savedNameRaw != nil)
                let savedName = try savedNameRaw?.decode(column: "Name", as: String.self)
                #expect(savedName == updatedName)
                try await deletePlayer(db: db, playerId: savedPLayerId)
                let deletedPlayerId = try await connector.getPlayerId(name: updatedName)
                #expect(deletedPlayerId == nil)
            }
        } catch {
            print(String(reflecting: error))
            #expect(error == nil)
        }
        try await connector.close()
    }
    
    private func deletePlayer(db: SQLDatabase, playerId: Int) async throws {
        try await db.delete(from: "Players")
            .where("Id", .equal, playerId)
            .run()
    }
}
