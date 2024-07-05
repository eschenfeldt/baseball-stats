//
//  GameLoader.swift
//  iScoreImport
//
//  Created by Patrick Eschenfeldt on 7/4/24.
//

import Foundation
import SQLKit

struct GameLoader {
    
    let db: SQLDatabase
    static let zeroTime = Date(timeIntervalSince1970: 0)
    
    func loadGames() async throws -> [Game] {
        // columns from "game"
        var query: SQLSelectBuilder = db.select()
            .column(SQLColumn("guid", table: "game"), as: "ExternalId")
            .column(SQLFunction("TRIM", args: SQLColumn("game_nm")), as: "Name")
            .column("scheduled_dt", as: "ScheduledTime")
            .column("start_dt", as: "StartTime")
            .column("end_dt", as: "EndTime")
            .column("location", as: "Location")
        
        // columns from teams
        query = query.column(SQLColumn("team_guid", table: "home_team_game"), as: "HomeTeamGuid")
            .column(SQLColumn("team_nm", table: "home_team_game"), as: "HomeTeamName")
            .column(SQLColumn("team_guid", table: "away_team_game"), as: "AwayTeamGuid")
            .column(SQLColumn("team_nm", table: "away_team_game"), as: "AwayTeamName")
        
        // winning pitcher
        query = query.column(SQLColumn("first_nm", table: "win_p"), as: "WinningPitcherFirstName")
            .column(SQLColumn("last_nm", table: "win_p"), as: "WinningPitcherLastName")
            .column(SQLFunction("CONCAT", args: SQLColumn("first_nm", table: "win_p"), SQLLiteral.string(" "), SQLColumn("last_nm", table: "win_p")), as: "WinningPitcherName")
            .column(SQLColumn("guid", table: "win_p"), as: "WinningPitcherGuid")
        
        // losing pitcher
        query = query.column(SQLColumn("first_nm", table: "lose_p"), as: "LosingPitcherFirstName")
            .column(SQLColumn("last_nm", table: "lose_p"), as: "LosingPitcherLastName")
            .column(SQLFunction("CONCAT", args: SQLColumn("first_nm", table: "lose_p"), SQLLiteral.string(" "), SQLColumn("last_nm", table: "lose_p")), as: "LosingPitcherName")
            .column(SQLColumn("guid", table: "lose_p"), as: "LosingPitcherGuid")
        
        // saving pitcher
        query = query.column(SQLColumn("first_nm", table: "save_p"), as: "SavingPitcherFirstName")
            .column(SQLColumn("last_nm", table: "save_p"), as: "SavingPitcherLastName")
            .column(SQLFunction("CONCAT", args: SQLColumn("first_nm", table: "save_p"), SQLLiteral.string(" "), SQLColumn("last_nm", table: "save_p")), as: "SavingPitcherName")
            .column(SQLColumn("guid", table: "save_p"), as: "SavingPitcherGuid")
        
        // game and team joins
        query = query.from("game")
            .join(SQLAlias("team_game", as: "home_team_game"), on: SQLColumn("home_game_guid"), .equal, SQLColumn("guid", table:"home_team_game"))
            .join(SQLAlias("team_game", as: "away_team_game"), on: SQLColumn("visitor_game_guid"), .equal, SQLColumn("guid", table:"away_team_game"))
        
        // pitcher joins
        query = query.join(SQLAlias("player_game", as: "win_pg"), method: SQLJoinMethod.left, on: SQLColumn("pitcher_win"), .equal, SQLColumn("guid", table:"win_pg"))
            .join(SQLAlias("player", as: "win_p"), method: SQLJoinMethod.left, on: SQLColumn("player_guid", table: "win_pg"), .equal, SQLColumn("guid", table: "win_p"))
            .join(SQLAlias("player_game", as: "lose_pg"), method: SQLJoinMethod.left, on: SQLColumn("pitcher_lose"), .equal, SQLColumn("guid", table:"lose_pg"))
            .join(SQLAlias("player", as: "lose_p"), method: SQLJoinMethod.left, on: SQLColumn("player_guid", table: "lose_pg"), .equal, SQLColumn("guid", table: "lose_p"))
            .join(SQLAlias("player_game", as: "save_pg"), method: SQLJoinMethod.left, on: SQLColumn("pitcher_save"), .equal, SQLColumn("guid", table:"save_pg"))
            .join(SQLAlias("player", as: "save_p"), method: SQLJoinMethod.left, on: SQLColumn("player_guid", table: "save_pg"), .equal, SQLColumn("guid", table: "save_p"))
        
        // where not deleted
        query = query.where(SQLFunction.coalesce(SQLColumn("is_deleted", table: "game"), SQLLiteral.numeric("0")), .equal, SQLLiteral.numeric("0"))
        
//        var blah = SQLSerializer(database: db)
//        query.query.serialize(to: &blah)
//        let string = blah.sql
        
        return try await query.all()
            .asyncMap {
                let game = try sqlRowToGame(row: $0)
                return try await loadBoxScore(game: game)
            }
    }
                
    func sqlRowToGame(row: SQLRow) throws -> Game {
        let homeTeam = Team(
            ExternalId: try row.decode(column: "HomeTeamGuid", as: UUID.self),
            CombinedName: try row.decode(column: "HomeTeamName", as: String.self)
        )
        let awayTeam = Team(
            ExternalId: try row.decode(column: "AwayTeamGuid", as: UUID.self),
            CombinedName: try row.decode(column: "AwayTeamName", as: String.self)
        )
        let location = Park(Name: try row.decode(column: "Location", as: String.self))
        let startTime = try decodeDatetime(row: row, colName: "StartTime")
        let scheduledTime = try decodeDatetime(row: row, colName: "ScheduledTime")
        let dateBaseTime = scheduledTime ?? startTime
        return Game(
            Name: try row.decode(column: "Name", as: String.self),
            ExternalId: try row.decode(column: "ExternalId", as: UUID.self),
            Date: Calendar.current.startOfDay(for: dateBaseTime!),
            HomeTeam: Team.withParsedName(team: homeTeam),
            AwayTeam: Team.withParsedName(team: awayTeam),
            ScheduledTime: scheduledTime,
            StartTime: startTime,
            EndTime: try decodeDatetime(row: row, colName: "EndTime"),
            Location: location,
            HomeScore: nil,
            AwayScore: nil,
            WinningTeam: nil,
            LosingTeam: nil,
            WinningPitcher: try? playerFromRow(row: row, prefix: "WinningPitcher"),
            LosingPitcher: try? playerFromRow(row: row, prefix: "LosingPitcher"),
            SavingPitcher: try? playerFromRow(row: row, prefix: "SavingPitcher"),
            HomeBoxScore: nil
        )
    }
    
    private func playerFromRow(row: SQLRow, prefix: String) throws -> Player? {
        let name = try? row.decode(column: "\(prefix)Name", as: String.self)
        guard let name else {
            return nil
        }
        guard name != " " else {
            return nil
        }
        return Player(Name: name,
                      ExternalId: try row.decode(column: "\(prefix)Guid", as: UUID.self),
                      DateOfBirth: nil,
                      FirstName: try row.decode(column: "\(prefix)FirstName", as: String.self),
                      MiddleName: nil,
                      LastName: try row.decode(column: "\(prefix)LastName", as: String.self),
                      Suffix: nil)
    }
    
    private func decodeDatetime(row: SQLRow, colName: String) throws -> Date? {
        let time = try row.decode(column: colName, as: Date.self)
        if (time == GameLoader.zeroTime) {
            return nil
        } else {
            return time
        }
    }
    
    private func loadBoxScore(game: Game) async throws -> Game {
        let homeBatters = try await getBatters(home: true)
        let awayBatters = try await getBatters(home: false)
        let homePitchers = try await getPitchers(home: true)
        let awayPitchers = try await getPitchers(home: false)
        let homeFielders = try await getFielders(home: true)
        let awayFielders = try await getFielders(home: false)
        
        var returnGame = game
        
        returnGame.AwayScore = awayBatters.reduce(0) {
            $0 + $1.Runs
        }
        returnGame.HomeScore = homeBatters.reduce(0) {
            $0 + $1.Runs
        }
        returnGame.assignWinner()
        
        returnGame.HomeBoxScore = BoxScore(
            batters: homeBatters,
            pitchers: homePitchers,
            fielders: homeFielders
        )
        returnGame.AwayBoxScore = BoxScore(
            batters: awayBatters,
            pitchers: awayPitchers,
            fielders: awayFielders
        )
        
        return returnGame
    }
    
    private static let batterColumnMap = [
        "bat_games": "Games",
        "bat_pa": "PlateAppearances",
        "bat_ab": "AtBats",
        "bat_runs": "Runs",
        "bat_bunt_singles": "BuntSingles",
        "bat_1b": "Singles",
        "bat_2b": "Doubles",
        "bat_3b": "Triples",
        "bat_hr": "Homeruns",
        "bat_rbi": "RunsBattedIn",
        "bat_bb": "Walks",
        "bat_ko_looking": "StrikeoutsCalled",
        "bat_ko_swinging": "StrikeoutsSwinging",
        "bat_hbp": "HitByPitch",
        "bat_sb": "StolenBases",
        "bat_cs": "CaughtStealing",
        "bat_scb": "SacrificeBunts",
        "bat_sf": "SacrificeFlies",
        "bat_roe": "ReachedOnError",
        "bat_fc": "FieldersChoices",
        "bat_ci": "CatchersInterference",
        "bat_gidp": "GroundedIntoDoublePlay",
        "bat_gitp": "GroundedIntoTriplePlay"
    ]
    
    private func getBatters(home: Bool) async throws -> [Batter] {
        let joinCol = home ? "home_game_guid" : "visitor_game_guid"
        var query = db.select()
            .column(SQLColumn("team_guid", table: "team_game"), as: "TeamExternalId")
            .column(SQLColumn("player_guid", table: "player_game"), as: "PlayerExternalId")
            .column(SQLFunction("CONCAT", args: SQLColumn("first_nm"), SQLLiteral.string(" "), SQLColumn("last_nm")), as: "PlayerName")
            .column("player_number", as: "Number")
        for kvp in GameLoader.batterColumnMap {
            query = query.column(
                SQLFunction("COALESCE", args: SQLColumn(kvp.key), SQLLiteral.numeric("0")),
                as: kvp.value)
        }
        
        return try await query.from("game")
            .join("team_game", on: SQLColumn(joinCol, table: "game"), .equal, SQLColumn("guid", table: "team_game"))
            .join("player_game", on: SQLColumn("guid", table: "team_game"), .equal, SQLColumn("team_game_guid", table: "player_game"))
            .join("stat_summary", on: SQLColumn("player_guid", table: "player_game"), .equal, SQLColumn("player_game_guid", table: "stat_summary"))
            .join("player", on: SQLColumn("player_guid", table: "player_game"), .equal, SQLColumn("guid", table: "player"))
            .where(SQLColumn("game_guid", table:"stat_summary"), .equal, SQLColumn("guid", table: "game"))
            .where("bat_games", .greaterThan, 0)
            .all(decoding: Batter.self)
    }
    
    private static let pitcherColumnMap = [
        "pit_games": "Games",
        "pit_win": "Wins",
        "pit_loss": "Losses",
        "pit_save": "Saves",
        "pit_outs": "ThirdInningsPitched",
        "pit_bf": "BattersFaced",
        "pit_balls": "Balls",
        "pit_strikes": "Strikes",
        "pit_runs": "Runs",
        "pit_er": "EarnedRuns",
        "pit_hits": "Hits",
        "pit_walks": "Walks",
        "pit_int_walks": "IntentionalWalks",
        "pit_strikeouts": "Strikeouts",
        "pit_strikeouts_looking": "StrikeoutsCalled",
        "pit_strikeouts_swinging": "StrikeoutsSwinging",
        "pit_hit_batters": "HitByPitch",
        "pit_balks": "Balks",
        "pit_wild_pitch": "WildPitches",
        "pit_homeruns": "Homeruns",
        "pit_ground_outs": "GroundOuts",
        "pit_air_outs": "AirOuts",
        "pit_first_strikes": "FirstPitchStrikes",
        "pit_first_balls": "FirstPitchBalls",
    ]
    
    
    private func getPitchers(home: Bool) async throws -> [Pitcher] {
        let joinCol = home ? "home_game_guid" : "visitor_game_guid"
        var query = db.select()
            .column(SQLColumn("team_guid", table: "team_game"), as: "TeamExternalId")
            .column(SQLColumn("player_guid", table: "player_game"), as: "PlayerExternalId")
            .column(SQLFunction("CONCAT", args: SQLColumn("first_nm"), SQLLiteral.string(" "), SQLColumn("last_nm")), as: "PlayerName")
            .column("player_number", as: "Number")
        for kvp in GameLoader.pitcherColumnMap {
            query = query.column(
                SQLFunction("COALESCE", args: SQLColumn(kvp.key), SQLLiteral.numeric("0")),
                as: kvp.value)
        }
        
        return try await query.from("game")
            .join("team_game", on: SQLColumn(joinCol, table: "game"), .equal, SQLColumn("guid", table: "team_game"))
            .join("player_game", on: SQLColumn("guid", table: "team_game"), .equal, SQLColumn("team_game_guid", table: "player_game"))
            .join("stat_summary", on: SQLColumn("player_guid", table: "player_game"), .equal, SQLColumn("player_game_guid", table: "stat_summary"))
            .join("player", on: SQLColumn("player_guid", table: "player_game"), .equal, SQLColumn("guid", table: "player"))
            .where(SQLColumn("game_guid", table:"stat_summary"), .equal, SQLColumn("guid", table: "game"))
            .where("pit_games", .greaterThan, 0)
            .all(decoding: Pitcher.self)
    }
    
    private static let fielderColumnMap = [
        "fld_games": "Games",
        "fld_throw_errors": "ErrorsThrowing",
        "fld_field_errors": "ErrorsFielding",
        "fld_putouts": "Putouts",
        "fld_assists": "Assists",
        "fld_caught_stealing": "CaughtStealing",
        "fld_double_plays": "DoublePlays",
        "fld_triple_plays": "TriplePlays",
        "fld_passed_balls": "PassedBalls",
        "fld_pickoff_failed": "PickoffFailed",
        "fld_pickoff_success": "PickoffSuccess",
    ]
    
    private func getFielders(home: Bool) async throws -> [Fielder] {
        let joinCol = home ? "home_game_guid" : "visitor_game_guid"
        var query = db.select()
            .column(SQLColumn("team_guid", table: "team_game"), as: "TeamExternalId")
            .column(SQLColumn("player_guid", table: "player_game"), as: "PlayerExternalId")
            .column(SQLFunction("CONCAT", args: SQLColumn("first_nm"), SQLLiteral.string(" "), SQLColumn("last_nm")), as: "PlayerName")
            .column("player_number", as: "Number")
            .column("fld_steals_allowed + fld_caught_stealing" as SQLQueryString, as: "StolenBaseAttempts")
        for kvp in GameLoader.fielderColumnMap {
            query = query.column(
                SQLFunction("COALESCE", args: SQLColumn(kvp.key), SQLLiteral.numeric("0")),
                as: kvp.value)
        }
        
        return try await query.from("game")
            .join("team_game", on: SQLColumn(joinCol, table: "game"), .equal, SQLColumn("guid", table: "team_game"))
            .join("player_game", on: SQLColumn("guid", table: "team_game"), .equal, SQLColumn("team_game_guid", table: "player_game"))
            .join("stat_summary", on: SQLColumn("player_guid", table: "player_game"), .equal, SQLColumn("player_game_guid", table: "stat_summary"))
            .join("player", on: SQLColumn("player_guid", table: "player_game"), .equal, SQLColumn("guid", table: "player"))
            .where(SQLColumn("game_guid", table:"stat_summary"), .equal, SQLColumn("guid", table: "game"))
            .where("fld_games", .greaterThan, 0)
            .all(decoding: Fielder.self)
    }
}
