//
//  SQLiteTests.swift
//  iScoreImportTests
//
//  Created by Patrick Eschenfeldt on 6/27/24.
//
import Foundation
import SQLKit
import SQLiteKit
import Testing

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
            let fullGame = try #require(
                await games.first {
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
            #expect(fullGame.AwayTeamName == "Chicago Cubs")  // team name at the time of the game, possibly different than current name
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
            #expect(lopezBat.LineupPosition == 9)
            #expect(lopezBat.AppearanceOrder == 1)
            #expect(lopezBat.Games == 1)
            #expect(lopezBat.AtBats == 2)
            #expect(lopezBat.PlateAppearances == 2)
            #expect(lopezBat.StrikeoutsCalled == 1)
            #expect(lopezBat.Singles == 0)
            #expect(lopezPit.Player?.Name == "Rodrigo Lopez")
            #expect(lopezPit.AppearanceOrder == 1)
            #expect(lopezPit.Games == 1)
            #expect(lopezPit.ThirdInningsPitched == 18)
            #expect(lopezPit.BattersFaced == 27)
            #expect(lopezPit.EarnedRuns == 2)
            #expect(lopezPit.Runs == 4)
            #expect(lopezFld.Player?.Name == "Rodrigo Lopez")
            #expect(lopezFld.Games == 1)
            #expect(lopezFld.Assists == 0)
            #expect(lopezFld.Putouts == 1)

            // Pull a relief pitcher to validate substitute lineup position and appearance order
            let axfordId = try #require(fullGame.SavingPitcher?.ExternalId)
            let axfordBat = try #require(
                homeBoxScore.batters.first { $0.PlayerExternalId == axfordId })
            #expect(axfordBat.Player?.Name == "John Axford")
            #expect(axfordBat.LineupPosition == 9)
            #expect(axfordBat.AppearanceOrder == 5)  // 5th batter in the 9 spot
            let axfordPit = try #require(
                homeBoxScore.pitchers.first { $0.PlayerExternalId == axfordId })
            #expect(axfordPit.Player?.Name == "John Axford")
            #expect(axfordPit.AppearanceOrder == 4)  // 4th Brewers pitcher

            // Also pull a pinch hitter
            let dewittBat = try #require(
                awayBoxScore.batters.first { $0.Player?.Name == "Blake DeWitt" })
            #expect(dewittBat.LineupPosition == 9)
            #expect(dewittBat.AppearanceOrder == 5)  // 5th batter in the 9 spot

            // With play-by-play, everyone should be placed
            for boxScore in [homeBoxScore, awayBoxScore] {
                #expect(boxScore.batters.allSatisfy { $0.LineupPosition != nil })
                #expect(boxScore.batters.allSatisfy { $0.AppearanceOrder != nil })
                #expect(
                    boxScore.pitchers.compactMap(\.AppearanceOrder).sorted()
                        == Array(1...boxScore.pitchers.count))
            }
        }
    }

    @Test("Pitcher batting 8th")
    func testPitcherBattingEighth() async throws {
        try await TestFixtures.withiScoreReader { reader in
            let game = try await loadGame(reader, "4/5/15 St. Louis Cardinals at Chicago Cubs")
            let cubs = try #require(game.HomeBoxScore)

            // Lester's player_game.batting_order is 7 and La Stella's is 8, so the pitcher
            // can't be assumed to hit 9th. The double switch also puts relievers into the 7 spot.
            #expect(
                battingCard(cubs) == [
                    "1.1 Dexter Fowler", "2.1 Jorge Soler", "3.1 Anthony Rizzo",
                    "4.1 Starlin Castro", "5.1 Chris Coghlan", "6.1 Mike Olt",
                    "7.1 David Ross", "7.2 Jason Motte", "7.3 Neil Ramirez",
                    "7.4 Arismendy Alcantara",
                    "8.1 Jon Lester", "8.2 Phil Coke", "8.3 Miguel Montero",
                    "9.1 Tommy La Stella", "9.2 Pedro Strop", "9.3 Hector Rondon",
                ])
            #expect(
                pitchingCard(cubs) == [
                    "1 Jon Lester", "2 Phil Coke", "3 Jason Motte", "4 Neil Ramirez",
                    "5 Pedro Strop", "6 Hector Rondon",
                ])

            // The Cardinals' pitcher hit 9th in the same game
            let cardinals = try #require(game.AwayBoxScore)
            #expect(
                battingCard(cardinals) == [
                    "1.1 Matt Carpenter", "2.1 Jason Heyward", "3.1 Matt Holliday",
                    "3.2 Peter Bourjos", "4.1 Jhonny Peralta", "5.1 Matt Adams",
                    "6.1 Yadier Molina", "7.1 Kolten Wong", "8.1 Jon Jay",
                    "9.1 Adam Wainwright", "9.2 Carlos Martinez", "9.3 Mark Reynolds",
                    "9.4 Jordan Walden", "9.5 Trevor Rosenthal",
                ])
        }
    }

    @Test("Lineup edited after first pitch")
    func testEditedLineup() async throws {
        try await TestFixtures.withiScoreReader { reader in
            let game = try await loadGame(reader, "5/23/25 Los Angeles Dodgers at New York Mets")
            let mets = try #require(game.HomeBoxScore)

            // The LINEUP play-by-play event still has the original lineup (Vientos 2nd, Marte 5th,
            // no Acuña). The edit only reached player_game, so this checks that we read the lineup from there.
            #expect(
                battingCard(mets) == [
                    "1.1 Francisco Lindor", "2.1 Starling Marte", "3.1 Juan Soto",
                    "4.1 Pete Alonso", "4.2 José Azocar", "4.3 Luis Torrens",
                    "5.1 Brandon Nimmo", "5.2 Jeff McNeil", "6.1 Tyrone Taylor",
                    "7.1 Francisco Alvarez", "8.1 Brett Baty", "9.1 Luisangel Acuña",
                ])
            #expect(!mets.batters.contains { $0.Player?.Name == "Mark Vientos" })
            #expect(
                pitchingCard(mets) == [
                    "1 Griffin Canning", "2 Max Kranick", "3 José Buttó", "4 José Castillo",
                    "5 Ryne Stanek", "6 Génesis Cabrera", "7 Edwin Diaz", "8 Reed Garrett",
                    "9 Huascar Brazobán",
                ])

            // DH game: no Dodgers pitcher gets a batting slot
            let dodgers = try #require(game.AwayBoxScore)
            let dodgerPitchers = Set(dodgers.pitchers.map(\.PlayerExternalId))
            #expect(!dodgers.batters.contains { dodgerPitchers.contains($0.PlayerExternalId) })
            #expect(dodgers.pitchers.first { $0.AppearanceOrder == 1 }?.Player?.Name == "Clayton Kershaw")
        }
    }

    private func loadGame(_ reader: SQLiteConnector, _ name: String) async throws -> Game {
        let games = try await reader.getGames()
        return try #require(await games.first { $0.Name == name })
    }

    /// "slot.appearance Name" in scorecard order
    private func battingCard(_ boxScore: BoxScore) -> [String] {
        boxScore.batters
            .sorted {
                ($0.LineupPosition ?? .max, $0.AppearanceOrder ?? .max)
                    < ($1.LineupPosition ?? .max, $1.AppearanceOrder ?? .max)
            }
            .map {
                "\($0.LineupPosition.map(String.init) ?? "?").\($0.AppearanceOrder.map(String.init) ?? "?") \($0.Player?.Name ?? "?")"
            }
    }

    /// "appearance Name" in pitching order
    private func pitchingCard(_ boxScore: BoxScore) -> [String] {
        boxScore.pitchers
            .sorted { ($0.AppearanceOrder ?? .max) < ($1.AppearanceOrder ?? .max) }
            .map { "\($0.AppearanceOrder.map(String.init) ?? "?") \($0.Player?.Name ?? "?")" }
    }
}
