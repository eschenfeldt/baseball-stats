//
//  IntegrationTests.swift
//  Tests
//
//  Created by Patrick Eschenfeldt on 6/30/24.
//

import Foundation
import Testing
import SQLKit

final class IntegrationTests {
    
    // SQLite
    let fileName = "Test_iScore_baseball"
    let filePath: String?
    
    // Postgres
    let configFileName = "test"
    let configFilePath: String?
    
    init() {
        let testBundle = Bundle(for: type(of: self))
        guard let resourceURL = testBundle.url(forResource: fileName, withExtension: "sqlite") else {
            filePath = nil
            configFilePath = nil
            return
        }
        filePath = resourceURL.absoluteString
        guard let configUrl = testBundle.url(forResource: configFileName, withExtension: "postgres_config") else {
            configFilePath = nil
            return
        }
        configFilePath = configUrl.absoluteString
    }

    @Test("Import Teams")
    func testImportTeams() async throws {
        #expect(filePath != nil)
        #expect(configFilePath != nil)
        guard let filePath, let configFilePath else { return }
        var reader = SQLiteConnector(filePath: filePath)
        var writer = PostgresConnector(configFilePath: configFilePath, pathIsLocalFile: false)
        do {
            try await reader.connect()
            try await writer.connect()
            let resultDb = try await writer.getDb()
            let initialCount = try await countTeams(db: resultDb)
            #expect(initialCount >= 1) // We always keep one test team in the db
            
            let teams = try await reader.getTeams()
            #expect(teams.count == 40)
            for team in teams {
                try await writer.insertOrUpdateTeam(team: team)
            }
            let teamCount = try await countTeams(db: resultDb)
            #expect(teamCount == initialCount + teams.count)
            // update again to be sure the count doesn't change
            for team in teams {
                try await writer.insertOrUpdateTeam(team: team)
            }
            let updatedTeamCount = try await countTeams(db: resultDb)
            #expect(teamCount == updatedTeamCount)
            // delete these teams
            for team in teams {
                #expect(team.City != nil)
                #expect(team.Name != nil)
                let teamId = try await writer.getTeamId(city: team.City!, name: team.Name!)
                #expect(teamId != nil)
                if let teamId {
                    try await deleteTeam(db: resultDb, teamId: teamId)
                }
            }
            let postDeleteCount = try await countTeams(db: resultDb)
            #expect(postDeleteCount == initialCount)
        }
        catch {
            print(String(reflecting: error))
            #expect(error == nil)
        }
        try await reader.close()
        try await writer.close()
    }

    private func countTeams(db: SQLDatabase) async throws -> Int {
        let teamCountRaw = try await db.select()
            .column(SQLFunction("COUNT", args: SQLLiteral.all), as: "Team Count")
            .from("Teams")
            .first()
        #expect(teamCountRaw != nil)
        return try teamCountRaw!.decode(column: "Team Count", as: Int.self)
    }
    
    private func deleteTeam(db: SQLDatabase, teamId: Int) async throws {
        try await db.delete(from: "Teams")
            .where("Id", .equal, teamId)
            .run()
    }
}
