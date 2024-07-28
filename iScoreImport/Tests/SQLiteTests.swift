//
//  Tests.swift
//  Tests
//
//  Created by Patrick Eschenfeldt on 6/27/24.
//
import Foundation
import Testing
import SQLKit
import SQLiteKit

@Suite("SQLite Tests")
final class SQLiteTests {
    
    let fileName = "Test_iScore_baseball"
    let filePath: String?
    
    init() {
        let testBundle = Bundle(for: type(of: self))
        guard let resourceURL = testBundle.url(forResource: fileName, withExtension: "sqlite") else {
            filePath = nil
            return
        }
        filePath = resourceURL.absoluteString
    }
   
    @Test("Connector Test")
    func testConnection() async throws {
        #expect(filePath != nil)
        guard let filePath else { return }
        var connector = SQLiteConnector(filePath: filePath)
        let db = try await connector.getDb()
        let result = try await db.select()
            .column("game_nm", as: "Game Name")
            .from("game")
            .limit(1)
            .all()
        #expect(result.count == 1)
        let decoded = try result[0].decode(column: "Game Name", as: String.self)
        #expect(decoded == "8/26/11 Chicago Cubs at Milwaukee Brewers ")
        try await connector.close()
    }
    
    @Test("Team Load")
    func testLoadingTeams() async throws {
        #expect(filePath != nil)
        guard let filePath else { return }
        var connector = SQLiteConnector(filePath: filePath)
        do {
            try await connector.connect()
            let teams = try await connector.getTeams()
            #expect(teams.count == 40)
            let bandits = teams.first() {
                $0.CombinedName == "Quad Cities River Bandits"
            }
            #expect(bandits != nil)
            guard let bandits else {
                try await connector.close()
                return
            }
            #expect(bandits.City == "Quad Cities")
            #expect(bandits.Name == "River Bandits")
            #expect(bandits.ExternalId == UUID(uuidString: "6EFA1F3C-2282-4945-8972-28CF51FAC8A1"))
        } catch {
            #expect(error == nil)
        }
        try await connector.close()
    }
    
    @Test("Player Load")
    func testLoadingPlayers() async throws {
        #expect(filePath != nil)
        guard let filePath else { return }
        var connector = SQLiteConnector(filePath: filePath)
        do {
            try await connector.connect()
            let players = try await connector.getPlayers(take:20)
            #expect(players.count == 20)
            let ankiel = players.first() {
                $0.Name == "Rick Ankiel"
            }
            #expect(ankiel != nil)
            guard let ankiel else {
                try await connector.close()
                return
            }
            #expect(ankiel.FirstName == "Rick")
            #expect(ankiel.LastName == "Ankiel")
            #expect(ankiel.ExternalId == UUID(uuidString: "019543AF-9B13-4BC0-9C5A-F58804A65BF8"))
        } catch {
            #expect(error == nil)
        }
        try await connector.close()
    }
    
    @Test("Game Load")
    func testLoadingGame() async throws {
        #expect(filePath != nil)
        guard let filePath else { return }
        var connector = SQLiteConnector(filePath: filePath)
        do {
            try await connector.connect()
            let games = try await connector.getGames()
            let fullGame = try await games.first {
                $0.Name == "8/26/11 Chicago Cubs at Milwaukee Brewers"
            }
            #expect(fullGame != nil)
            guard let fullGame else {
                try await connector.close()
                return
            }
            #expect(fullGame.ExternalId == UUID(uuidString: "8280516F-8E5F-4BD3-A96F-3620CB19751A"))
            let dateFormatter = DateFormatter()
            dateFormatter.dateFormat = "yyyy-MM-dd"
            #expect(fullGame.Date == dateFormatter.date(from: "2011-08-26"))
            let datetimeFormatter = DateFormatter()
            datetimeFormatter.dateFormat = "yyyy-MM-dd HH:mm:ss"
            #expect(fullGame.ScheduledTime == nil)
            #expect(fullGame.StartTime == datetimeFormatter.date(from: "2011-08-26 20:10:40"))
            #expect(fullGame.EndTime == datetimeFormatter.date(from: "2011-08-26 23:12:00"))
            #expect(fullGame.AwayScore == 2)
            #expect(fullGame.AwayTeamName == "Chicago Cubs") // team name at the time of the game, possibly different than current name
            #expect(fullGame.AwayTeam.CombinedName == "Chicago Cubs")
            #expect(fullGame.HomeScore == 5)
            #expect(fullGame.HomeTeamName == "Milwaukee Brewers")
            #expect(fullGame.HomeTeam.CombinedName == "Milwaukee Brewers")
            #expect(fullGame.Location?.Name == "Miller Park")
            #expect(fullGame.WinningPitcher?.Name == "Randy Wolf")
            #expect(fullGame.LosingPitcher?.Name == "Rodrigo Lopez")
            #expect(fullGame.SavingPitcher?.Name == "John Axford")
            #expect(fullGame.WinningTeam?.CombinedName == "Milwaukee Brewers")
            #expect(fullGame.LosingTeam?.CombinedName == "Chicago Cubs")
            #expect(fullGame.HomeBoxScore != nil)
            guard let homeBoxScore = fullGame.HomeBoxScore, let awayBoxScore = fullGame.AwayBoxScore else {
                try await connector.close()
                return
            }
            #expect(homeBoxScore.batters.count == 14)
            #expect(homeBoxScore.fielders.count == 13)
            #expect(homeBoxScore.pitchers.count == 4)
            #expect(awayBoxScore.batters.count == 14)
            #expect(awayBoxScore.fielders.count == 12)
            #expect(awayBoxScore.pitchers.count == 3)
            #expect(fullGame.LosingPitcher?.ExternalId != nil)
            let lopezId = fullGame.LosingPitcher?.ExternalId
            let lopezBat = awayBoxScore.batters.first {
                $0.PlayerExternalId == lopezId
            }
            #expect(lopezBat != nil)
            let lopezPit = awayBoxScore.pitchers.first {
                $0.PlayerExternalId == lopezId
            }
            #expect(lopezPit != nil)
            let lopezFld = awayBoxScore.fielders.first {
                $0.PlayerExternalId == lopezId
            }
            #expect(lopezFld != nil)
            #expect(lopezBat?.Player?.Name == "Rodrigo Lopez")
            #expect(lopezBat?.Games == 1)
            #expect(lopezBat?.AtBats == 2)
            #expect(lopezBat?.PlateAppearances == 2)
            #expect(lopezBat?.StrikeoutsCalled == 1)
            #expect(lopezBat?.Singles == 0)
            #expect(lopezPit?.Player?.Name == "Rodrigo Lopez")
            #expect(lopezPit?.Games == 1)
            #expect(lopezPit?.ThirdInningsPitched == 18)
            #expect(lopezPit?.BattersFaced == 27)
            #expect(lopezPit?.EarnedRuns == 2)
            #expect(lopezPit?.Runs == 4)
            #expect(lopezFld?.Player?.Name == "Rodrigo Lopez")
            #expect(lopezFld?.Games == 1)
            #expect(lopezFld?.Assists == 0)
            #expect(lopezFld?.Putouts == 1)
        } catch {
            #expect(error == nil)
        }
        try await connector.close()
    }
}
