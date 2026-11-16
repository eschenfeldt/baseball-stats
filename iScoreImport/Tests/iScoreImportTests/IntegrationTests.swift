//
//  IntegrationTests.swift
//  iScoreImportTests
//
//  Created by Patrick Eschenfeldt on 6/30/24.
//

import Foundation
import SQLKit
import Testing

@testable import iScoreImport

@Suite("Integration Tests", .enabled(if: TestPostgres.isConfigured))
struct IntegrationTests {

    @Test("Import Teams")
    func testImportTeams() async throws {
        try await TestPostgres.withScratchDatabase { writer in
            try await TestFixtures.withiScoreReader { reader in
                let resultDb = try await writer.getDb()

                let initialCount = try await countTeams(db: resultDb)
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
                    let city = try #require(team.City)
                    let name = try #require(team.Name)
                    let teamId = try #require(await writer.getTeamId(city: city, name: name))
                    try await deleteTeam(db: resultDb, teamId: teamId)
                }
                let postDeleteCount = try await countTeams(db: resultDb)
                #expect(postDeleteCount == initialCount)
            }
        }
    }

    private func countTeams(db: SQLDatabase) async throws -> Int {
        let teamCountRaw = try await db.select()
            .column(SQLFunction("COUNT", args: SQLLiteral.all), as: "Team Count")
            .from("Teams")
            .first()
        return try #require(teamCountRaw).decode(column: "Team Count", as: Int.self)
    }

    private func deleteTeam(db: SQLDatabase, teamId: Int) async throws {
        try await db.delete(from: "Teams")
            .where("Id", .equal, teamId)
            .run()
    }

    @Test("Import Players")
    func testImportPlayers() async throws {
        try await TestPostgres.withScratchDatabase { writer in
            try await TestFixtures.withiScoreReader { reader in
                let resultDb = try await writer.getDb()

                let initialCount = try await countPlayers(db: resultDb)

                let take = 100
                let players = try await reader.getPlayers(skip: 100, take: take)
                #expect(players.count == take)
                for player in players {
                    try await writer.insertOrUpdatePlayer(player: player)
                }
                let playerCount = try await countPlayers(db: resultDb)
                #expect(playerCount == initialCount + players.count)

                // update again to be sure the count doesn't change
                for player in players {
                    try await writer.insertOrUpdatePlayer(player: player)
                }
                let updatedPlayerCount = try await countPlayers(db: resultDb)
                #expect(playerCount == updatedPlayerCount)

                // delete these players
                for player in players {
                    let playerId = try #require(await writer.getPlayerId(name: player.Name))
                    try await deletePlayer(db: resultDb, playerId: playerId)
                }
                let postDeleteCount = try await countPlayers(db: resultDb)
                #expect(postDeleteCount == initialCount)
            }
        }
    }

    private func countPlayers(db: SQLDatabase) async throws -> Int {
        let playerCountRaw = try await db.select()
            .column(SQLFunction("COUNT", args: SQLLiteral.all), as: "Player Count")
            .from("Players")
            .first()
        return try #require(playerCountRaw).decode(column: "Player Count", as: Int.self)
    }

    private func deletePlayer(db: SQLDatabase, playerId: Int) async throws {
        try await db.delete(from: "Players")
            .where("Id", .equal, playerId)
            .run()
    }

    @Test("Import Game")
    func testImportGame() async throws {
        try await TestPostgres.withScratchDatabase { writer in
            try await TestFixtures.withiScoreReader { reader in
                // get one player we know is used in this game so we can add them to the writer first
                let players = try await reader.getPlayers(lastName: "Soriano")
                #expect(players.count >= 1)
                let soriano = try #require(players.first { $0.Name == "Alfonso Soriano" })

                let resultDb = try await writer.getDb()
                let games = try await reader.getGames()
                let fullGame = try #require(
                    await games.first {
                        $0.Name == "8/26/11 Chicago Cubs at Milwaukee Brewers"
                    })

                // insert Soriano
                let preSorianoPlayerCount = try await countPlayers(db: resultDb)
                try await writer.insertOrUpdatePlayer(player: soriano)

                let initialCount = try await countGames(db: resultDb)
                let initialPlayerCount = try await countPlayers(db: resultDb)
                #expect(initialPlayerCount == preSorianoPlayerCount + 1)

                try await writer.insertOrUpdateGame(game: fullGame)
                let count = try await countGames(db: resultDb)
                let playerCount = try await countPlayers(db: resultDb)
                #expect(initialCount + 1 == count)
                #expect(playerCount == initialPlayerCount + 27)

                let gameId = try #require(await writer.getGameId(name: fullGame.Name))
                try await validateGame(db: resultDb, gameId: gameId)

                // importing the same game again should update rather than duplicate
                try await writer.insertOrUpdateGame(game: fullGame)
                let afterUpdateCount = try await countGames(db: resultDb)
                #expect(count == afterUpdateCount)
                try await validateGame(db: resultDb, gameId: gameId)

                let playersToDelete = try await getPlayersToDelete(db: resultDb, gameId: gameId)
                let teamsToDelete = try await getTeamsToDelete(db: resultDb, gameId: gameId)
                try await deleteGame(db: resultDb, gameId: gameId)
                let postDeleteCount = try await countGames(db: resultDb)
                #expect(postDeleteCount == initialCount)
                for toDeleteId in playersToDelete {
                    try await deletePlayer(db: resultDb, playerId: toDeleteId)
                }
                for toDeleteId in teamsToDelete {
                    try await deleteTeam(db: resultDb, teamId: toDeleteId)
                }
                let postDeletePlayerCount = try await countPlayers(db: resultDb)
                #expect(postDeletePlayerCount == preSorianoPlayerCount)
            }
        }
    }

    private func countGames(db: SQLDatabase) async throws -> Int {
        let gameCountRaw = try await db.select()
            .column(SQLFunction("COUNT", args: SQLLiteral.all), as: "Game Count")
            .from("Games")
            .first()
        return try #require(gameCountRaw).decode(column: "Game Count", as: Int.self)
    }

    private struct GameSummary: Codable {
        let Name: String
        let HomeScore: Int
        let AwayScore: Int
        let WinningPitcherName: String?
        let LosingPitcherName: String?
        let SavingPitcherName: String?
        let HomeTeamName: String?
        let AwayTeamName: String?
    }

    private struct BatterSummary: Codable {
        let ExternalId: UUID
        let BatterCount: Int
        let BatterRuns: Int
        let BatterAtBats: Int
        let BatterLineupPositions: Int
        let BatterAppearanceOrders: Int
    }

    private struct PitcherSummary: Codable {
        let ExternalId: UUID
        let PitcherCount: Int
        let PitcherRuns: Int
        let PitcherAppearanceOrders: Int
    }

    private struct FielderSummary: Codable {
        let ExternalId: UUID
        let FielderCount: Int
    }

    private func validateGame(db: SQLDatabase, gameId: Int) async throws {
        let gameSummaries = try await db.select()
            .column(SQLColumn("Name", table: "Games"))
            .column("HomeScore")
            .column("AwayScore")
            .column("HomeTeamName")
            .column("AwayTeamName")
            .column(SQLColumn("Name", table: "wp"), as: "WinningPitcherName")
            .column(SQLColumn("Name", table: "lp"), as: "LosingPitcherName")
            .column(SQLColumn("Name", table: "sp"), as: "SavingPitcherName")
            .from("Games")
            .join(SQLAlias("Players", as: "wp"), method: SQLJoinMethod.left, on: SQLColumn("WinningPitcherId"), .equal, SQLColumn("Id", table: "wp"))
            .join(SQLAlias("Players", as: "lp"), method: SQLJoinMethod.left, on: SQLColumn("LosingPitcherId"), .equal, SQLColumn("Id", table: "lp"))
            .join(SQLAlias("Players", as: "sp"), method: SQLJoinMethod.left, on: SQLColumn("SavingPitcherId"), .equal, SQLColumn("Id", table: "sp"))
            .all(decoding: GameSummary.self)
        let batterSummaries = try await db.select()
            .column("ExternalId")
            .column(SQLFunction("COUNT", args: SQLColumn("Id", table: "Batters")), as: "BatterCount")
            .column(SQLFunction("SUM", args: SQLColumn("Runs", table: "Batters")), as: "BatterRuns")
            .column(SQLFunction("SUM", args: SQLColumn("AtBats", table: "Batters")), as: "BatterAtBats")
            // COUNT(column) only counts non-null values
            .column(SQLFunction("COUNT", args: SQLColumn("LineupPosition", table: "Batters")), as: "BatterLineupPositions")
            .column(SQLFunction("COUNT", args: SQLColumn("AppearanceOrder", table: "Batters")), as: "BatterAppearanceOrders")
            .from("Games")
            .join("BoxScores", on: SQLColumn("Id", table: "Games"), .equal, SQLColumn("GameId", table: "BoxScores"))
            .join("Batters", method: SQLJoinMethod.left, on: SQLColumn("Id", table: "BoxScores"), .equal, SQLColumn("BoxScoreId", table: "Batters"))
            .where(SQLColumn("Id", table: "Games"), .equal, SQLLiteral.numeric("\(gameId)"))
            .groupBy("ExternalId")
            .all(decoding: BatterSummary.self)
        let pitcherSummaries = try await db.select()
            .column("ExternalId")
            .column(SQLFunction("COUNT", args: SQLColumn("Id", table: "Pitchers")), as: "PitcherCount")
            .column(SQLFunction("SUM", args: SQLFunction.coalesce(SQLColumn("Runs", table: "Pitchers"), SQLLiteral.numeric("0"))), as: "PitcherRuns")
            .column(SQLFunction("COUNT", args: SQLColumn("AppearanceOrder", table: "Pitchers")), as: "PitcherAppearanceOrders")
            .from("Games")
            .join("BoxScores", on: SQLColumn("Id", table: "Games"), .equal, SQLColumn("GameId", table: "BoxScores"))
            .join("Pitchers", method: SQLJoinMethod.left, on: SQLColumn("Id", table: "BoxScores"), .equal, SQLColumn("BoxScoreId", table: "Pitchers"))
            .where(SQLColumn("Id", table: "Games"), .equal, SQLLiteral.numeric("\(gameId)"))
            .groupBy("ExternalId")
            .all(decoding: PitcherSummary.self)
        let fielderSummaries = try await db.select()
            .column("ExternalId")
            .column(SQLFunction("COUNT", args: SQLColumn("Id", table: "Fielders")), as: "FielderCount")
            .from("Games")
            .join("BoxScores", on: SQLColumn("Id", table: "Games"), .equal, SQLColumn("GameId", table: "BoxScores"))
            .join("Fielders", method: SQLJoinMethod.left, on: SQLColumn("Id", table: "BoxScores"), .equal, SQLColumn("BoxScoreId", table: "Fielders"))
            .where(SQLColumn("Id", table: "Games"), .equal, SQLLiteral.numeric("\(gameId)"))
            .groupBy("ExternalId")
            .all(decoding: FielderSummary.self)
        #expect(gameSummaries.count == 1)
        let gameSummary = gameSummaries.first
        #expect(gameSummary?.AwayScore == 2)
        #expect(gameSummary?.AwayTeamName == "Chicago Cubs")
        #expect(gameSummary?.HomeScore == 5)
        #expect(gameSummary?.HomeTeamName == "Milwaukee Brewers")
        #expect(gameSummary?.WinningPitcherName == "Randy Wolf")
        #expect(gameSummary?.LosingPitcherName == "Rodrigo Lopez")
        #expect(gameSummary?.SavingPitcherName == "John Axford")

        #expect(batterSummaries.count == 1)
        #expect(pitcherSummaries.count == 1)
        #expect(fielderSummaries.count == 1)
        let batterSummary = batterSummaries.first
        let pitcherSummary = pitcherSummaries.first
        let fielderSummary = fielderSummaries.first
        #expect(batterSummary?.ExternalId != nil)
        #expect(pitcherSummary?.ExternalId != nil)
        #expect(fielderSummary?.ExternalId != nil)
        #expect(batterSummary?.ExternalId == pitcherSummary?.ExternalId)
        #expect(pitcherSummary?.ExternalId == fielderSummary?.ExternalId)
        #expect(batterSummary?.BatterCount == 28)
        #expect(fielderSummary?.FielderCount == 25)
        #expect(pitcherSummary?.PitcherCount == 7)
        #expect(batterSummary?.BatterRuns == 7)
        #expect(pitcherSummary?.PitcherRuns == 7)
        #expect(batterSummary?.BatterAtBats == 63)

        // every batter and pitcher has their order set
        #expect(batterSummary?.BatterLineupPositions == 28)
        #expect(batterSummary?.BatterAppearanceOrders == 28)
        #expect(pitcherSummary?.PitcherAppearanceOrders == 7)

        // and the values make it through to the right rows
        let axford = try await db.select()
            .column(SQLColumn("LineupPosition", table: "Batters"))
            .column(SQLColumn("AppearanceOrder", table: "Batters"), as: "BatterAppearanceOrder")
            .column(SQLColumn("AppearanceOrder", table: "Pitchers"), as: "PitcherAppearanceOrder")
            .from("BoxScores")
            .join("Batters", on: SQLColumn("Id", table: "BoxScores"), .equal, SQLColumn("BoxScoreId", table: "Batters"))
            .join("Pitchers", on: SQLColumn("Id", table: "BoxScores"), .equal, SQLColumn("BoxScoreId", table: "Pitchers"))
            .join("Players", on: SQLColumn("Id", table: "Players"), .equal, SQLColumn("PlayerId", table: "Batters"))
            .where(SQLColumn("GameId", table: "BoxScores"), .equal, SQLLiteral.numeric("\(gameId)"))
            .where(SQLColumn("PlayerId", table: "Pitchers"), .equal, SQLColumn("PlayerId", table: "Batters"))
            .where(SQLColumn("Name", table: "Players"), .equal, SQLLiteral.string("John Axford"))
            .first()
        let axfordRow = try #require(axford)
        #expect(try axfordRow.decode(column: "LineupPosition", as: Int.self) == 9)
        #expect(try axfordRow.decode(column: "BatterAppearanceOrder", as: Int.self) == 5)
        #expect(try axfordRow.decode(column: "PitcherAppearanceOrder", as: Int.self) == 4)
    }

    private func getPlayersToDelete(db: SQLDatabase, gameId: Int) async throws -> [Int] {
        let resultsRaw = try await db.select()
            .distinct()
            .column(SQLColumn("PlayerId", table: "Batters"))
            .from("BoxScores")
            .join("Batters", on: SQLColumn("Id", table: "BoxScores"), .equal, SQLColumn("BoxScoreId", table: "Batters"))
            .join("Players", on: SQLColumn("Id", table: "Players"), .equal, SQLColumn("PlayerId", table: "Batters"))
            .where("GameId", .equal, SQLLiteral.numeric("\(gameId)"))
            .all()
        return try resultsRaw.map {
            try $0.decode(column: "PlayerId", as: Int.self)
        }
    }

    private func getTeamsToDelete(db: SQLDatabase, gameId: Int) async throws -> [Int] {
        let homeTeamRaw = try await db.select()
            .column(SQLColumn("HomeId"))
            .from("Games")
            .where("Id", .equal, SQLLiteral.numeric("\(gameId)"))
            .all()
        let homeTeam = try homeTeamRaw.map {
            try $0.decode(column: "HomeId", as: Int.self)
        }
        let awayTeamRaw = try await db.select()
            .column(SQLColumn("AwayId"))
            .from("Games")
            .where("Id", .equal, SQLLiteral.numeric("\(gameId)"))
            .all()
        let awayTeam = try awayTeamRaw.map {
            try $0.decode(column: "AwayId", as: Int.self)
        }
        var teams = homeTeam
        teams.append(contentsOf: awayTeam)
        return teams
    }

    private func deleteGame(db: SQLDatabase, gameId: Int) async throws {
        try await db.delete(from: "Games")
            .where("Id", .equal, gameId)
            .run()
    }
}
