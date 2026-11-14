//
//  SQLiteTests.swift
//  iScoreImportTests
//
//  Created by Patrick Eschenfeldt on 6/27/24.
//
import Foundation
import Testing
import SQLKit
import SQLiteKit
@testable import iScoreImport

@Suite("SQLite Tests")
struct SQLiteTests {

    @Test("Connector Test")
    func testConnection() async throws {
        try await TestFixtures.withiScoreReader { reader in
            let db = try await reader.getDb()
            let result = try await db.select()
                .column("game_nm", as: "Game Name")
                .from("game")
                .limit(1)
                .all()
            #expect(result.count == 1)
            let decoded = try result[0].decode(column: "Game Name", as: String.self)
            #expect(decoded == "8/26/11 Chicago Cubs at Milwaukee Brewers ")
        }
    }

    @Test("Team Load")
    func testLoadingTeams() async throws {
        try await TestFixtures.withiScoreReader { reader in
            let teams = try await reader.getTeams()
            #expect(teams.count == 40)
            let bandits = try #require(teams.first { $0.CombinedName == "Quad Cities River Bandits" })
            #expect(bandits.City == "Quad Cities")
            #expect(bandits.Name == "River Bandits")
            #expect(bandits.ExternalId == UUID(uuidString: "6EFA1F3C-2282-4945-8972-28CF51FAC8A1"))
            #expect(bandits.ColorHex == "E10F00")
            #expect(bandits.Abbreviation == "QC")
        }
    }

    @Test("Player Load")
    func testLoadingPlayers() async throws {
        try await TestFixtures.withiScoreReader { reader in
            let players = try await reader.getPlayers(take: 20)
            #expect(players.count == 20)
            let ankiel = try #require(players.first { $0.Name == "Rick Ankiel" })
            #expect(ankiel.FirstName == "Rick")
            #expect(ankiel.LastName == "Ankiel")
            #expect(ankiel.ExternalId == UUID(uuidString: "019543AF-9B13-4BC0-9C5A-F58804A65BF8"))
        }
    }

    @Test("Game Load")
    func testLoadingGame() async throws {
        try await TestFixtures.withiScoreReader { reader in
            let games = try await reader.getGames()
            let fullGame = try #require(await games.first {
                $0.Name == "8/26/11 Chicago Cubs at Milwaukee Brewers"
            })
            #expect(fullGame.ExternalId == UUID(uuidString: "8280516F-8E5F-4BD3-A96F-3620CB19751A"))

            // iScore stores these as absolute instants (Unix epoch seconds), so the
            // expectations are pinned to UTC rather than to whatever zone the machine
            // running the tests happens to be in.
            let datetimeFormatter = DateFormatter()
            datetimeFormatter.dateFormat = "yyyy-MM-dd HH:mm:ss"
            datetimeFormatter.locale = Locale(identifier: "en_US_POSIX")
            datetimeFormatter.timeZone = TimeZone(secondsFromGMT: 0)
            #expect(fullGame.ScheduledTime == nil)
            #expect(fullGame.StartTime == datetimeFormatter.date(from: "2011-08-27 00:10:40"))
            #expect(fullGame.EndTime == datetimeFormatter.date(from: "2011-08-27 03:12:00"))

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

            let homeBoxScore = try #require(fullGame.HomeBoxScore)
            let awayBoxScore = try #require(fullGame.AwayBoxScore)
            #expect(homeBoxScore.batters.count == 14)
            #expect(homeBoxScore.fielders.count == 13)
            #expect(homeBoxScore.pitchers.count == 4)
            #expect(awayBoxScore.batters.count == 14)
            #expect(awayBoxScore.fielders.count == 12)
            #expect(awayBoxScore.pitchers.count == 3)

            let lopezId = try #require(fullGame.LosingPitcher?.ExternalId)
            let lopezBat = try #require(awayBoxScore.batters.first { $0.PlayerExternalId == lopezId })
            let lopezPit = try #require(awayBoxScore.pitchers.first { $0.PlayerExternalId == lopezId })
            let lopezFld = try #require(awayBoxScore.fielders.first { $0.PlayerExternalId == lopezId })
            #expect(lopezBat.Player?.Name == "Rodrigo Lopez")
            #expect(lopezBat.Games == 1)
            #expect(lopezBat.AtBats == 2)
            #expect(lopezBat.PlateAppearances == 2)
            #expect(lopezBat.StrikeoutsCalled == 1)
            #expect(lopezBat.Singles == 0)
            #expect(lopezPit.Player?.Name == "Rodrigo Lopez")
            #expect(lopezPit.Games == 1)
            #expect(lopezPit.ThirdInningsPitched == 18)
            #expect(lopezPit.BattersFaced == 27)
            #expect(lopezPit.EarnedRuns == 2)
            #expect(lopezPit.Runs == 4)
            #expect(lopezFld.Player?.Name == "Rodrigo Lopez")
            #expect(lopezFld.Games == 1)
            #expect(lopezFld.Assists == 0)
            #expect(lopezFld.Putouts == 1)
        }
    }
}
