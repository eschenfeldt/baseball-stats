//
//  PostgresTests.swift
//  Tests
//
//  Created by Patrick Eschenfeldt on 6/28/24.
//

import Foundation
import Testing
import SQLKit

struct PostgresTests {
    
    let configFilePath = "test"

    @Test("Connection Test")
    func testConnection() async throws {
        var connector = PostgresConnector(configFileName: configFilePath)
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
        var connector = PostgresConnector(configFileName: configFilePath)
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
        var connector = PostgresConnector(configFileName: configFilePath)
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
        var connector = PostgresConnector(configFileName: configFilePath)
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
}
