//
//  GameImporter.swift
//  iScoreImport
//
//  Created by Patrick Eschenfeldt on 7/4/24.
//

import Foundation
import SQLKit

enum GameImportError : Error {
    case incompleteTeam(team: Team)
    case playerInsertIssue(player: Player)
    case boxScoreInsertIssue(game: Game)
    case gameInsertIssue(game: Game)
    case unimportedPlayer(externalId: UUID)
}

struct GameImporter {
    
    let db: SQLDatabase
    let teams: TeamImporter
    let players: PlayerImporter
    
    init(db: SQLDatabase) {
        self.db = db
        self.teams = TeamImporter(db: db)
        self.players = PlayerImporter(db: db)
    }
    
    func insertOrUpdateGame(game: Game) async throws {
        var existingGameId: Int? = nil
        if let externalId = game.ExternalId {
            existingGameId = try await getGameId(externalId: externalId)
        }
        if existingGameId == nil {
            existingGameId = try await getGameId(name: game.Name)
        }
        if let existingGameId {
            try await updateGame(game: game, gameId: existingGameId)
        } else {
            try await insertGame(game: game)
        }
    }
    
    private func insertGame(game: Game) async throws {
        let home = try await getTeamId(team: game.HomeTeam)
        let away = try await getTeamId(team: game.AwayTeam)
        let winningTeamId = try await getOptionalTeamId(team: game.WinningTeam)
        let losingTeamId = try await getOptionalTeamId(team: game.LosingTeam)
        let winningPitcherId = try await getOptionalPlayerId(player: game.WinningPitcher)
        let losingPitcherId = try await getOptionalPlayerId(player: game.LosingPitcher)
        let savingPitcherId = try await getOptionalPlayerId(player: game.SavingPitcher)
        try await db.insert(into: "Games")
            .columns(
                "ExternalId",
                "Name",
                "Date",
                "HomeId",
                "AwayId",
                "ScheduledTime",
                "StartTime",
                "EndTime",
                "HomeScore",
                "AwayScore",
                "WinningTeamId",
                "LosingTeamId",
                "WinningPitcherId",
                "LosingPitcherId",
                "SavingPitcherId"
            )
            .values(
                game.ExternalId ?? UUID(uuidString: "00000000-0000-0000-0000-000000000000"),
                game.Name,
                game.Date,
                home,
                away,
                game.ScheduledTime,
                game.StartTime,
                game.EndTime,
                game.HomeScore,
                game.AwayScore,
                winningTeamId,
                losingTeamId,
                winningPitcherId,
                losingPitcherId,
                savingPitcherId
            )
            .run()
        
        guard let gameId = try await getGameId(name: game.Name) else {
            throw GameImportError.gameInsertIssue(game: game)
        }
        
        try await setBoxScoreIdsIfNecessary(game: game, gameId: gameId)
        try await insertOrUpdatePlayerGames(game: game, gameId: gameId)
    }

    
    private func updateGame(game: Game, gameId: Int) async throws {
        let home = try await getTeamId(team: game.HomeTeam)
        let away = try await getTeamId(team: game.AwayTeam)
        let winningTeamId = try await getOptionalTeamId(team: game.WinningTeam)
        let losingTeamId = try await getOptionalTeamId(team: game.LosingTeam)
        let winningPitcherId = try await getOptionalPlayerId(player: game.WinningPitcher)
        let losingPitcherId = try await getOptionalPlayerId(player: game.LosingPitcher)
        let savingPitcherId = try await getOptionalPlayerId(player: game.SavingPitcher)
        
        var updateStatement = db.update("Games")
            .set("Name", to: game.Name)
            .set("Date", to: game.Date)
            .set("HomeId", to: home)
            .set("AwayId", to: away)
            .set("ScheduledTime", to: game.ScheduledTime)
            .set("StartTime", to: game.StartTime)
            .set("EndTime", to: game.EndTime)
            .set("HomeScore", to: game.HomeScore)
            .set("AwayScore", to: game.AwayScore)
            .set("WinningTeamId", to: winningTeamId)
            .set("LosingTeamId", to: losingTeamId)
            .set("WinningPitcherId", to: winningPitcherId)
            .set("LosingPitcherId", to: losingPitcherId)
            .set("SavingPitcherId", to: savingPitcherId)
        
        if let externalId = game.ExternalId {
            updateStatement = updateStatement
                .set("ExternalId", to: externalId)
        }
        
        try await updateStatement.run()
        try await setBoxScoreIdsIfNecessary(game: game, gameId: gameId)
        try await insertOrUpdatePlayerGames(game: game, gameId: gameId)
    }
    
    private func insertOrUpdatePlayerGames(game: Game, gameId: Int) async throws {
        guard let homeBoxScoreId = try await getBoxScoreId(gameId: gameId, home: true),
              let awayBoxScoreId = try await getBoxScoreId(gameId: gameId, home: false),
              let homeBoxScore = game.HomeBoxScore,
              let awayBoxScore = game.AwayBoxScore else {
            throw GameImportError.boxScoreInsertIssue(game: game)
        }
        try await insertOrUpdatePlayerGames(game: game, boxScoreId: homeBoxScoreId, boxScore: homeBoxScore)
        try await insertOrUpdatePlayerGames(game: game, boxScoreId: awayBoxScoreId, boxScore: awayBoxScore)
    }
    
    private func insertOrUpdatePlayerGames(game: Game, boxScoreId: Int, boxScore: BoxScore) async throws {
        for batter in boxScore.batters {
            let playerId = try await getPlayerId(externalId: batter.PlayerExternalId, player: batter.Player)
            let model = BatterInsertModel(boxScoreId: boxScoreId, playerId: playerId, batter: batter)
            if let batterId = try await getPlayerGameId(playerId: playerId, boxScoreId: boxScoreId, baseTable: "Batters") {
                try await updateBatter(batterId: batterId, model: model)
            } else {
                try await insertBatter(model: model)
            }
        }
    }
    
    private func getPlayerId(externalId: UUID, player: Player?) async throws -> Int {
        if let playerId = try await players.getPlayerId(externalId: externalId) {
            // already stored by external id
            return playerId
        }
        guard let player else {
            throw GameImportError.unimportedPlayer(externalId: externalId)
        }
        try await players.insertOrUpdatePlayer(player: player)
        guard let playerId = try await players.getPlayerId(externalId: externalId) else {
            throw GameImportError.playerInsertIssue(player: player)
        }
        return playerId
    }
    
    private func insertBatter(model: BatterInsertModel) async throws {
        try await db.insert(into: "Batters")
            .model(model)
            .run()
    }
    
    private func updateBatter(batterId: Int, model: BatterInsertModel) async throws {
        try await db.update("Batters")
            .set(model: model)
            .where("Id", .equal, batterId)
            .run()
    }
    
    private func getPlayerGameId(playerId: Int, boxScoreId: Int, baseTable: String) async throws -> Int? {
        let rawResult = try await db.select()
            .column(SQLColumn("Id", table: baseTable))
            .from(baseTable)
            .where("BoxScoreId", .equal, boxScoreId)
            .where("PlayerId", .equal, playerId)
            .first()
        return try rawResult?.decode(column: "Id", as: Int.self)
    }
    
    private func setBoxScoreIdsIfNecessary(game: Game, gameId: Int) async throws {
        let homeBoxScoreId = try await insertBoxScoreIfNecessary(game: game, gameId: gameId, home: true)
        let awayBoxScoreId = try await insertBoxScoreIfNecessary(game: game, gameId: gameId, home: false)
        
        try await db.update("Games")
            .set("HomeBoxScoreId", to: homeBoxScoreId)
            .set("AwayBoxScoreId", to: awayBoxScoreId)
            .where("Id", .equal, gameId)
            .run()
    }
    
    private func insertBoxScoreIfNecessary(game: Game, gameId: Int, home: Bool) async throws -> Int {
        let team = home ? game.HomeTeam : game.AwayTeam
        let teamId = try await getTeamId(team: team)
        if let boxScoreId = try await getBoxScoreId(gameId: gameId, teamId: teamId) {
            return boxScoreId
        } else {
            return try await insertBoxScore(game: game, gameId: gameId, teamId: teamId)
        }
    }
    
    private func insertBoxScore(game: Game, gameId: Int, teamId: Int) async throws -> Int {
        try await db.insert(into: "BoxScores")
            .columns("GameId", "TeamId")
            .values(gameId, teamId)
            .run()
        guard let boxScoreId = try await getBoxScoreId(gameId: gameId, teamId: teamId) else {
            throw GameImportError.boxScoreInsertIssue(game: game)
        }
        return boxScoreId
    }
    
    private func getOptionalTeamId(team: Team?) async throws -> Int? {
        if let team {
            return try await getTeamId(team: team)
        } else {
            return nil
        }
    }
    
    private func getTeamId(team: Team) async throws -> Int {
        guard let city = team.City, let name = team.Name else {
            throw GameImportError.incompleteTeam(team: team)
        }
        guard let teamId = try await teams.getTeamId(city: city, name: name) else {
            try await teams.insertOrUpdateTeam(team: team)
            guard let insertedTeamId = try await teams.getTeamId(city: city, name: name) else {
                throw GameImportError.incompleteTeam(team: team)
            }
            return insertedTeamId
        }
        return teamId
    }
    
    private func getOptionalPlayerId(player: Player?) async throws -> Int? {
        if let player {
            return try await getPlayerId(player: player)
        } else {
            return nil
        }
    }
    
    private func getPlayerId(player: Player) async throws -> Int {
        guard let playerId = try await players.getPlayerId(name: player.Name) else {
            try await players.insertOrUpdatePlayer(player: player)
            guard let insertedPlayerId = try await players.getPlayerId(name: player.Name) else {
                throw GameImportError.playerInsertIssue(player: player)
            }
            return insertedPlayerId
        }
        return playerId
    }
    
    private func getGameId(externalId: UUID) async throws -> Int? {
        let result = try await db.select()
            .column("Id")
            .from("Games")
            .where("ExternalId", .equal, externalId)
            .first()
        return try result?.decode(column: "Id", as: Int.self)
    }
    
    func getGameId(name: String) async throws -> Int? {
        let result = try await db.select()
            .column("Id")
            .from("Games")
            .where("Name", .equal, name)
            .first()
        return try result?.decode(column: "Id", as: Int.self)
    }
    
    private func getBoxScoreId(gameId: Int, teamId: Int) async throws -> Int? {
        let result = try await db.select()
            .column("Id")
            .from("BoxScores")
            .where("GameId", .equal, gameId)
            .where("TeamId", .equal, teamId)
            .first()
        return try result?.decode(column: "Id", as: Int.self)
    }
    
    private func getBoxScoreId(gameId: Int, home: Bool) async throws -> Int? {
        let column = home ? "HomeBoxScoreId" : "AwayBoxScoreId"
        let result = try await db.select()
            .column(SQLColumn("Id", table: "BoxScores"))
            .from("BoxScores")
            .join("Games", on: SQLColumn("GameId", table: "BoxScores"), .equal, SQLColumn("Id", table: "Games"))
            .where("GameId", .equal, gameId)
            .where(column, .equal, SQLColumn("Id", table: "BoxScores"))
            .first()
        return try result?.decode(column: "Id", as: Int.self)
    }
    
    private struct BatterInsertModel: Encodable {
        let BoxScoreId: Int
        let PlayerId: Int
        let Number: Int
        let Games: Int
        let PlateAppearances: Int
        let AtBats: Int
        let Runs: Int
        let Hits: Int
        let BuntSingles: Int
        let Singles: Int
        let Doubles: Int
        let Triples: Int
        let Homeruns: Int
        let RunsBattedIn: Int
        let Walks: Int
        let Strikeouts: Int
        let StrikeoutsCalled: Int
        let StrikeoutsSwinging: Int
        let HitByPitch: Int
        let StolenBases: Int
        let CaughtStealing: Int
        let SacrificeBunts: Int
        let SacrificeFlies: Int
        let Sacrifices: Int
        let ReachedOnError: Int
        let FieldersChoices: Int
        let CatchersInterference: Int
        let GroundedIntoDoublePlay: Int
        let GroundedIntoTriplePlay: Int
        let AtBatsWithRunnersInScoringPosition: Int
        let HitsWithRunnersInScoringPosition: Int
        
        init(boxScoreId: Int, playerId: Int, batter: Batter) {
            self.BoxScoreId = boxScoreId
            self.PlayerId = playerId
            
            self.Number = batter.Number
            self.Games = batter.Games
            self.PlateAppearances = batter.PlateAppearances
            self.AtBats = batter.AtBats
            self.Runs = batter.Runs
            self.Hits = batter.Singles + batter.Doubles + batter.Triples + batter.Homeruns
            self.BuntSingles = batter.BuntSingles
            self.Singles = batter.Singles
            self.Doubles = batter.Doubles
            self.Triples = batter.Triples
            self.Homeruns = batter.Homeruns
            self.RunsBattedIn = batter.RunsBattedIn
            self.Walks = batter.Walks
            self.Strikeouts = batter.StrikeoutsCalled + batter.StrikeoutsSwinging
            self.StrikeoutsCalled = batter.StrikeoutsCalled
            self.StrikeoutsSwinging = batter.StrikeoutsSwinging
            self.HitByPitch = batter.HitByPitch
            self.StolenBases = batter.StolenBases
            self.CaughtStealing = batter.CaughtStealing
            self.SacrificeBunts = batter.SacrificeBunts
            self.SacrificeFlies = batter.SacrificeFlies
            self.Sacrifices = batter.SacrificeBunts + batter.SacrificeFlies
            self.ReachedOnError = batter.ReachedOnError
            self.FieldersChoices = batter.FieldersChoices
            self.CatchersInterference = batter.CatchersInterference
            self.GroundedIntoDoublePlay = batter.GroundedIntoDoublePlay
            self.GroundedIntoTriplePlay = batter.GroundedIntoTriplePlay
            self.AtBatsWithRunnersInScoringPosition = 0
            self.HitsWithRunnersInScoringPosition = 0
        }
    }
}
