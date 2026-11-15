//
//  GameLoader.swift
//  iScoreImport
//
//  Created by Patrick Eschenfeldt on 7/4/24.
//

import Foundation
import SQLKit

enum GameLoadError: Error {
    case noGameIdentifier
}

struct GameLoader {

    let db: SQLDatabase
    static let zeroTime = Date(timeIntervalSince1970: 0)

    func loadGames(afterDate: Date?) async throws -> AsyncThrowingStream<Game, Error> {
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
            .column(SQLColumn("team_nm", table: "home_team"), as: "CurrentHomeTeamName")
            .column(SQLColumn("team_guid", table: "away_team_game"), as: "AwayTeamGuid")
            .column(SQLColumn("team_nm", table: "away_team_game"), as: "AwayTeamName")
            .column(SQLColumn("team_nm", table: "away_team"), as: "CurrentAwayTeamName")

        // winning pitcher
        query = query.column(SQLColumn("first_nm", table: "win_p"), as: "WinningPitcherFirstName")
            .column(SQLColumn("last_nm", table: "win_p"), as: "WinningPitcherLastName")
            .column(
                SQLFunction("CONCAT", args: SQLColumn("first_nm", table: "win_p"), SQLLiteral.string(" "), SQLColumn("last_nm", table: "win_p")),
                as: "WinningPitcherName"
            )
            .column(SQLColumn("guid", table: "win_p"), as: "WinningPitcherGuid")

        // losing pitcher
        query = query.column(SQLColumn("first_nm", table: "lose_p"), as: "LosingPitcherFirstName")
            .column(SQLColumn("last_nm", table: "lose_p"), as: "LosingPitcherLastName")
            .column(
                SQLFunction("CONCAT", args: SQLColumn("first_nm", table: "lose_p"), SQLLiteral.string(" "), SQLColumn("last_nm", table: "lose_p")),
                as: "LosingPitcherName"
            )
            .column(SQLColumn("guid", table: "lose_p"), as: "LosingPitcherGuid")

        // saving pitcher
        query = query.column(SQLColumn("first_nm", table: "save_p"), as: "SavingPitcherFirstName")
            .column(SQLColumn("last_nm", table: "save_p"), as: "SavingPitcherLastName")
            .column(
                SQLFunction("CONCAT", args: SQLColumn("first_nm", table: "save_p"), SQLLiteral.string(" "), SQLColumn("last_nm", table: "save_p")),
                as: "SavingPitcherName"
            )
            .column(SQLColumn("guid", table: "save_p"), as: "SavingPitcherGuid")

        // game and team joins
        query = query.from("game")
            .join(SQLAlias("team_game", as: "home_team_game"), on: SQLColumn("home_game_guid"), .equal, SQLColumn("guid", table: "home_team_game"))
            .join(SQLAlias("team", as: "home_team"), on: SQLColumn("team_guid", table: "home_team_game"), .equal, SQLColumn("guid", table: "home_team"))
            .join(SQLAlias("team_game", as: "away_team_game"), on: SQLColumn("visitor_game_guid"), .equal, SQLColumn("guid", table: "away_team_game"))
            .join(SQLAlias("team", as: "away_team"), on: SQLColumn("team_guid", table: "away_team_game"), .equal, SQLColumn("guid", table: "away_team"))

        // pitcher joins
        query = query.join(
            SQLAlias("player_game", as: "win_pg"), method: SQLJoinMethod.left, on: SQLColumn("pitcher_win"), .equal, SQLColumn("guid", table: "win_pg")
        )
        .join(
            SQLAlias("player", as: "win_p"), method: SQLJoinMethod.left, on: SQLColumn("player_guid", table: "win_pg"), .equal,
            SQLColumn("guid", table: "win_p")
        )
        .join(SQLAlias("player_game", as: "lose_pg"), method: SQLJoinMethod.left, on: SQLColumn("pitcher_lose"), .equal, SQLColumn("guid", table: "lose_pg"))
        .join(
            SQLAlias("player", as: "lose_p"), method: SQLJoinMethod.left, on: SQLColumn("player_guid", table: "lose_pg"), .equal,
            SQLColumn("guid", table: "lose_p")
        )
        .join(SQLAlias("player_game", as: "save_pg"), method: SQLJoinMethod.left, on: SQLColumn("pitcher_save"), .equal, SQLColumn("guid", table: "save_pg"))
        .join(
            SQLAlias("player", as: "save_p"), method: SQLJoinMethod.left, on: SQLColumn("player_guid", table: "save_pg"), .equal,
            SQLColumn("guid", table: "save_p"))

        // where not deleted
        query = query.where(SQLFunction.coalesce(SQLColumn("is_deleted", table: "game"), SQLLiteral.numeric("0")), .equal, SQLLiteral.numeric("0"))

        if let afterDate {
            query = query.where(SQLColumn("start_dt", table: "game"), .greaterThan, SQLLiteral.numeric(afterDate.timeIntervalSince1970.description))
        }

        let rows = try await query.all()

        print("Found \(rows.count) games. Loading details...")

        var index = 0

        return AsyncThrowingStream {
            guard index < rows.count else { return nil }

            let row = rows[index]
            index += 1
            let game = try sqlRowToGame(row: row)
            print("Found game \(game.Name)")
            let withBoxScore = try await loadBoxScore(game: game)
            print("Loaded details for \(game.Name)")
            return withBoxScore
        }

    }

    func sqlRowToGame(row: SQLRow) throws -> Game {
        let homeTeam = Team(
            ExternalId: try row.decode(column: "HomeTeamGuid", as: UUID.self),
            CombinedName: try row.decode(column: "CurrentHomeTeamName", as: String.self)
        )
        let awayTeam = Team(
            ExternalId: try row.decode(column: "AwayTeamGuid", as: UUID.self),
            CombinedName: try row.decode(column: "CurrentAwayTeamName", as: String.self)
        )
        let location = Park(Name: try row.decode(column: "Location", as: String.self))
        let startTime = try decodeDatetime(row: row, colName: "StartTime")
        let scheduledTime = try decodeDatetime(row: row, colName: "ScheduledTime")
        return Game(
            Name: try row.decode(column: "Name", as: String.self),
            ExternalId: try row.decode(column: "ExternalId", as: UUID.self),
            HomeTeam: Team.withParsedName(team: homeTeam),
            AwayTeam: Team.withParsedName(team: awayTeam),
            ScheduledTime: scheduledTime,
            StartTime: startTime,
            EndTime: try decodeDatetime(row: row, colName: "EndTime"),
            Location: location,
            HomeScore: nil,
            AwayScore: nil,
            HomeTeamName: try row.decode(column: "HomeTeamName", as: String.self),
            AwayTeamName: try row.decode(column: "AwayTeamName", as: String.self),
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
        return Player(
            Name: name,
            ExternalId: try row.decode(column: "\(prefix)Guid", as: UUID.self),
            DateOfBirth: nil,
            FirstName: try row.decode(column: "\(prefix)FirstName", as: String.self),
            MiddleName: nil,
            LastName: try row.decode(column: "\(prefix)LastName", as: String.self),
            Suffix: nil)
    }

    private func decodeDatetime(row: SQLRow, colName: String) throws -> Date? {
        let time = try row.decode(column: colName, as: Date.self)
        if time == GameLoader.zeroTime {
            return nil
        } else {
            return time
        }
    }

    private func loadBoxScore(game: Game) async throws -> Game {
        guard let gameGuid = game.ExternalId else {
            throw GameLoadError.noGameIdentifier
        }
        let order = try await loadBattingOrder(gameGuid: gameGuid)
        let homeBatters = try await getBatters(gameGuid: gameGuid, home: true, order: order.home)
        let awayBatters = try await getBatters(gameGuid: gameGuid, home: false, order: order.away)
        let homePitchers = try await getPitchers(gameGuid: gameGuid, home: true)
        let awayPitchers = try await getPitchers(gameGuid: gameGuid, home: false)
        let homeFielders = try await getFielders(gameGuid: gameGuid, home: true)
        let awayFielders = try await getFielders(gameGuid: gameGuid, home: false)

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
        "bat_gitp": "GroundedIntoTriplePlay",
    ]

    // MARK: Box score order
    //
    // How iScore stores batting order (verified against every play-by-play game in the real
    // database by replaying it and comparing the PAs credited to each player with stat_summary.bat_pa):
    //
    // - Play-by-play never names the batter. Pitch/play events give the batter's 1-based lineup
    //   slot as the first number in `bt_act` (e.g. "4:0"), and `game_update.home_at_bat` /
    //   `visitor_at_bat` is the slot due up. So iScore rebuilds "who is in slot N" from the
    //   starting lineup plus substitutions, and we do the same.
    // - Starting lineup: `player_game` rows with non-zero `isStarter` (it's usually 1, rarely 2),
    //   ordered by `batting_order`, then by rowid. Older games have `batting_order` 0 for every
    //   player, and rowid order is the lineup order there. The starting pitcher's
    //   `batting_order` can be large (e.g. 27) when he hits 9th, because it's a roster position,
    //   not a slot number, so the ranking is what matters. Players past the lineup size can also be
    //   flagged as starters (e.g. an opener in a DH game, or bench players), so only the first
    //   `lineupSize` count. `lineupSize` is the largest slot the play-by-play ever brings up.
    // - The `LINEUP` play-by-play event is NOT used. iScore doesn't update it when the lineup is
    //   edited after the game starts, but it does update `player_game`.
    // - Substitutions: `SUBOFF` events, replayed in order. Entries are "<player_game guid>:<slot>"
    //   and may be in either `bt_act` (the team batting that half-inning) or `ft_act`, so the
    //   team comes from `player_game`, not from the column.
    // - Pitcher appearance order is stored directly as `stat_summary.pit_seq`. It matches the
    //   order of `SUBDEF` to position 1 in every game, so it's selected in `getPitchers`.

    struct BattingSlot: Equatable {
        let lineupPosition: Int
        let appearanceOrder: Int
    }

    /// Batting order slot and appearance within the slot for every player on one team who got into
    /// the batting order, keyed by player guid.
    struct BattingOrder {
        private(set) var slots: [UUID: BattingSlot] = [:]
        private var slotCounts: [Int: Int] = [:]

        subscript(player: UUID) -> BattingSlot? { slots[player] }

        fileprivate mutating func add(_ player: UUID, slot: Int) {
            // A player keeps the slot they first entered; a pinch hitter staying in on defense
            // only generates SUBDEF, but guard against a repeated SUBOFF anyway.
            guard slots[player] == nil else { return }
            let appearance = (slotCounts[slot] ?? 0) + 1
            slotCounts[slot] = appearance
            slots[player] = BattingSlot(lineupPosition: slot, appearanceOrder: appearance)
        }
    }

    private struct PlayerGameRow: Decodable {
        let PlayerGameGuid: UUID
        let PlayerGuid: UUID
        let IsStarter: Int
    }

    private struct LineupSizeRow: Decodable {
        let HomeSize: Int?
        let AwaySize: Int?
    }

    private struct SubstitutionRow: Decodable {
        let BattingTeamAction: String?
        let FieldingTeamAction: String?
    }

    func loadBattingOrder(gameGuid: UUID) async throws -> (home: BattingOrder, away: BattingOrder) {
        let sizes = try await db.select()
            .column(SQLFunction("MAX", args: SQLColumn("home_at_bat")), as: "HomeSize")
            .column(SQLFunction("MAX", args: SQLColumn("visitor_at_bat")), as: "AwaySize")
            .from("game_update")
            .where("game_guid", .equal, SQLLiteral.string("\(gameGuid)"))
            .first(decoding: LineupSizeRow.self)

        var home = BattingOrder()
        var away = BattingOrder()
        // player_game.guid -> (is home team, player guid); substitution events use player_game guids
        var playerGames: [UUID: (home: Bool, player: UUID)] = [:]

        for isHome in [true, false] {
            let joinCol = isHome ? "home_game_guid" : "visitor_game_guid"
            let rows = try await db.select()
                .column(SQLColumn("guid", table: "player_game"), as: "PlayerGameGuid")
                .column(SQLColumn("player_guid", table: "player_game"), as: "PlayerGuid")
                .column(
                    SQLFunction.coalesce(SQLColumn("isStarter"), SQLLiteral.numeric("0")),
                    as: "IsStarter"
                )
                .from("game")
                .join(
                    "player_game", on: SQLColumn(joinCol, table: "game"), .equal,
                    SQLColumn("team_game_guid", table: "player_game")
                )
                .where(SQLColumn("guid", table: "game"), .equal, SQLLiteral.string("\(gameGuid)"))
                .orderBy(SQLColumn("batting_order", table: "player_game"))
                .orderBy(SQLColumn("rowid", table: "player_game"))
                .all(decoding: PlayerGameRow.self)

            var order = BattingOrder()
            for row in rows {
                playerGames[row.PlayerGameGuid] = (isHome, row.PlayerGuid)
            }
            let lineupSize = (isHome ? sizes?.HomeSize : sizes?.AwaySize) ?? 0
            let starters = rows.filter { $0.IsStarter != 0 }.prefix(lineupSize)
            for (index, starter) in starters.enumerated() {
                order.add(starter.PlayerGuid, slot: index + 1)
            }
            if isHome { home = order } else { away = order }
        }

        let substitutions = try await db.select()
            .column(SQLColumn("bt_act", table: "game_update_event"), as: "BattingTeamAction")
            .column(SQLColumn("ft_act", table: "game_update_event"), as: "FieldingTeamAction")
            .from("game_update")
            .join(
                "game_update_event", on: SQLColumn("guid", table: "game_update"), .equal,
                SQLColumn("game_update_guid", table: "game_update_event")
            )
            .where(
                SQLColumn("game_guid", table: "game_update"), .equal,
                SQLLiteral.string("\(gameGuid)")
            )
            .where(SQLColumn("type", table: "game_update_event"), .equal, SQLLiteral.string("SUBOFF"))
            .orderBy(SQLColumn("update_dt", table: "game_update"))
            .orderBy(SQLColumn("rowid", table: "game_update_event"))
            .all(decoding: SubstitutionRow.self)

        for event in substitutions {
            let entries = [event.BattingTeamAction, event.FieldingTeamAction]
                .compactMap { $0 }
                .flatMap { $0.split(separator: ",") }
                .compactMap(GameLoader.parseSubstitution)
            for (playerGameGuid, slot) in entries {
                guard let (isHome, player) = playerGames[playerGameGuid] else { continue }
                if isHome {
                    home.add(player, slot: slot)
                } else {
                    away.add(player, slot: slot)
                }
            }
        }

        return (home, away)
    }

    private static func parseSubstitution(_ entry: Substring) -> (UUID, Int)? {
        let parts = entry.split(separator: ":")
        guard parts.count == 2,
            let guid = UUID(uuidString: String(parts[0])),
            let value = Int(parts[1])
        else { return nil }
        return (guid, value)
    }

    // MARK: Stats

    private func getBatters(gameGuid: UUID, home: Bool, order: BattingOrder) async throws -> [Batter] {
        let joinCol = home ? "home_game_guid" : "visitor_game_guid"
        var query = db.select()
            .column(SQLColumn("team_guid", table: "team_game"), as: "TeamExternalId")
            .column(SQLColumn("player_guid", table: "player_game"), as: "PlayerExternalId")
            .column(SQLFunction("CONCAT", args: SQLColumn("first_nm"), SQLLiteral.string(" "), SQLColumn("last_nm")), as: "PlayerName")
            .column("player_number", as: "Number")
            // "bat_ab" seems to always be null for some reason, so compute it
            .column(
                SQLBinaryExpression(
                    SQLColumn("bat_pa"), .subtract,
                    SQLBinaryExpression(
                        SQLColumn("bat_ci"), .subtract,
                        SQLBinaryExpression(
                            SQLColumn("bat_sf"), .subtract,
                            SQLBinaryExpression(
                                SQLColumn("bat_scb"), .subtract,
                                SQLBinaryExpression(
                                    SQLColumn("bat_hbp"), .subtract,
                                    SQLColumn("bat_bb")))))),
                as: "AtBats")
        for kvp in GameLoader.batterColumnMap {
            query = query.column(
                SQLFunction("COALESCE", args: SQLColumn(kvp.key), SQLLiteral.numeric("0")),
                as: kvp.value)
        }

        let batters = try await query.from("game")
            .join("team_game", on: SQLColumn(joinCol, table: "game"), .equal, SQLColumn("guid", table: "team_game"))
            .join("player_game", on: SQLColumn("guid", table: "team_game"), .equal, SQLColumn("team_game_guid", table: "player_game"))
            .join("stat_summary", on: SQLColumn("player_guid", table: "player_game"), .equal, SQLColumn("player_game_guid", table: "stat_summary"))
            .join("player", on: SQLColumn("player_guid", table: "player_game"), .equal, SQLColumn("guid", table: "player"))
            .where(SQLColumn("game_guid", table: "stat_summary"), .equal, SQLColumn("guid", table: "game"))
            .where("bat_games", .greaterThan, 0)
            .where(SQLColumn("guid", table: "game"), .equal, SQLLiteral.string("\(gameGuid)"))
            .all(decoding: Batter.self)

        var returnBatters: [Batter] = []
        for var batter in batters {
            batter.Player = try await getPlayer(externalId: batter.PlayerExternalId)
            let slot = order[batter.PlayerExternalId]
            batter.LineupPosition = slot?.lineupPosition
            batter.AppearanceOrder = slot?.appearanceOrder
            returnBatters.append(batter)
        }
        return returnBatters
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

    private func getPitchers(gameGuid: UUID, home: Bool) async throws -> [Pitcher] {
        let joinCol = home ? "home_game_guid" : "visitor_game_guid"
        var query = db.select()
            .column(SQLColumn("team_guid", table: "team_game"), as: "TeamExternalId")
            .column(SQLColumn("player_guid", table: "player_game"), as: "PlayerExternalId")
            .column(SQLFunction("CONCAT", args: SQLColumn("first_nm"), SQLLiteral.string(" "), SQLColumn("last_nm")), as: "PlayerName")
            .column("player_number", as: "Number")
            // pit_seq is iScore's own 1-based pitching appearance order
            .column(SQLFunction("NULLIF", args: SQLColumn("pit_seq"), SQLLiteral.numeric("0")), as: "AppearanceOrder")
        for kvp in GameLoader.pitcherColumnMap {
            query = query.column(
                SQLFunction("COALESCE", args: SQLColumn(kvp.key), SQLLiteral.numeric("0")),
                as: kvp.value)
        }

        let pitchers = try await query.from("game")
            .join("team_game", on: SQLColumn(joinCol, table: "game"), .equal, SQLColumn("guid", table: "team_game"))
            .join("player_game", on: SQLColumn("guid", table: "team_game"), .equal, SQLColumn("team_game_guid", table: "player_game"))
            .join("stat_summary", on: SQLColumn("player_guid", table: "player_game"), .equal, SQLColumn("player_game_guid", table: "stat_summary"))
            .join("player", on: SQLColumn("player_guid", table: "player_game"), .equal, SQLColumn("guid", table: "player"))
            .where(SQLColumn("game_guid", table: "stat_summary"), .equal, SQLColumn("guid", table: "game"))
            .where("pit_games", .greaterThan, 0)
            .where(SQLColumn("guid", table: "game"), .equal, SQLLiteral.string("\(gameGuid)"))
            .all(decoding: Pitcher.self)

        var returnPitchers: [Pitcher] = []
        for var pitcher in pitchers {
            pitcher.Player = try await getPlayer(externalId: pitcher.PlayerExternalId)
            returnPitchers.append(pitcher)
        }
        return returnPitchers
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

    private func getFielders(gameGuid: UUID, home: Bool) async throws -> [Fielder] {
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

        let fielders = try await query.from("game")
            .join("team_game", on: SQLColumn(joinCol, table: "game"), .equal, SQLColumn("guid", table: "team_game"))
            .join("player_game", on: SQLColumn("guid", table: "team_game"), .equal, SQLColumn("team_game_guid", table: "player_game"))
            .join("stat_summary", on: SQLColumn("player_guid", table: "player_game"), .equal, SQLColumn("player_game_guid", table: "stat_summary"))
            .join("player", on: SQLColumn("player_guid", table: "player_game"), .equal, SQLColumn("guid", table: "player"))
            .where(SQLColumn("game_guid", table: "stat_summary"), .equal, SQLColumn("guid", table: "game"))
            .where("fld_games", .greaterThan, 0)
            .where(SQLColumn("guid", table: "game"), .equal, SQLLiteral.string("\(gameGuid)"))
            .all(decoding: Fielder.self)

        var returnFielders: [Fielder] = []
        for var fielder in fielders {
            fielder.Player = try await getPlayer(externalId: fielder.PlayerExternalId)
            returnFielders.append(fielder)
        }
        return returnFielders
    }

    private func getPlayer(externalId: UUID) async throws -> Player? {
        return try await db.select()
            .column("guid", as: "ExternalId")
            .column("first_nm", as: "FirstName")
            .column("last_nm", as: "LastName")
            .column(SQLFunction("CONCAT", args: SQLColumn("first_nm"), SQLLiteral.string(" "), SQLColumn("last_nm")), as: "Name")
            .from("player")
            .where("guid", .equal, externalId)
            .first(decoding: Player.self)
    }
}
