//
//  PostgresConnector.swift
//  iScoreImport
//
//  Created by Patrick Eschenfeldt on 6/28/24.
//

import Foundation
import PostgresKit

enum PostgresConnectionError: Error {
    case invalidConfigPath(path: String)
    case invalidConfig(data: Data)
    case invalidMethodCall
}

struct PostgresConfig : Codable {
    var hostname: String
    var database: String
    var username: String
    var password: String
}

struct PostgresConnector : DbConnector {
    
    var configFilePath: String
    /// Determines file load behavior
    let pathIsLocalFile: Bool
    private var config: SQLPostgresConfiguration?
    var logger: Logger
    private var connection: PostgresConnection?
    private var db: SQLDatabase?
    
    init(configFilePath: String, logger: Logger = Logger.init(label: "Postgres"), pathIsLocalFile: Bool = true) {
        self.configFilePath = configFilePath
        self.logger = logger
        self.pathIsLocalFile = pathIsLocalFile
    }
    
    private func getConfigURL() -> URL? {
        if pathIsLocalFile {
            return URL(fileURLWithPath: configFilePath)
        } else {
            return URL(string: configFilePath)
        }
    }
    
    private mutating func getConfig() throws {
        guard let url = getConfigURL() else {
            throw PostgresConnectionError.invalidConfigPath(path: configFilePath)
        }
        let data: Data
        do {
            data = try Data(contentsOf: url)
        } catch {
            throw PostgresConnectionError.invalidConfigPath(path: configFilePath)
        }
        
        let decoder = JSONDecoder()
        
        if let decoded = try? decoder.decode(PostgresConfig.self, from: data) {
            config = SQLPostgresConfiguration(hostname: decoded.hostname, username: decoded.username, password: decoded.password, database: decoded.database, tls: .disable)
        } else {
            throw PostgresConnectionError.invalidConfig(data: data)
        }
    }
    
    private mutating func getConnection() async throws {
        guard let config else {
            throw PostgresConnectionError.invalidMethodCall
        }
        let newConnection = try await PostgresConnection.connect(configuration: config.coreConfiguration, id: 1, logger: logger)
        connection = newConnection
    }
    
    mutating func connect() async throws {
        _ = try await getDb()
    }
    
    mutating func getDb() async throws -> SQLDatabase {
        if config == nil {
            try self.getConfig()
            return try await getDbFromConfig()
        }
        if connection == nil {
            return try await getDbFromConfig()
        }
        guard let db else {
            return try await getDbFromConnection()
        }
        
        return db
    }
    
    private mutating func getDbFromConfig() async throws -> SQLDatabase {
        if config == nil {
            throw PostgresConnectionError.invalidMethodCall
        }
        try await getConnection()
        return try await getDbFromConnection()
    }
    
    private mutating func getDbFromConnection() async throws -> SQLDatabase {
        guard let connection else {
            throw PostgresConnectionError.invalidMethodCall
        }
        let newDb = connection.sql()
        db = newDb
        return newDb
    }
    
    mutating func close() async throws {
        guard let connection else {
            return
        }
        try await connection.close()
        self.connection = nil;
    }
    
    func insertOrUpdateTeam(team: Team) async throws {
        guard db != nil else {
            throw ConnectorError.connectionRequired
        }
        var existingTeamId: Int? = nil
        if let externalId = team.ExternalId {
            existingTeamId = try await getTeamId(externalId: externalId)
        }
        if existingTeamId == nil, let city = team.City, let name = team.Name {
            existingTeamId = try await getTeamId(city: city, name: name)
        }
        if let existingTeamId {
            try await updateTeam(team: team, id: existingTeamId)
        } else {
            try await insertTeam(team: team)
        }
    }
    
    private func getTeamId(externalId: UUID) async throws -> Int? {
        guard let db else {
            throw ConnectorError.connectionRequired
        }
        let result = try await db.select()
            .column("Id")
            .from("Teams")
            .where("ExternalId", .equal, externalId)
            .first()
        return try result?.decode(column: "Id", as: Int.self)
    }
    
    func getTeamId(city: String, name: String) async throws -> Int? {
        guard let db else {
            throw ConnectorError.connectionRequired
        }
        let result = try await db.select()
            .column("Id")
            .from("Teams")
            .where("City", .equal, city)
            .where("Name", .equal, name)
            .first()
        return try result?.decode(column: "Id", as: Int.self)
    }
    
    private func insertTeam(team: Team) async throws {
        guard let db else {
            throw ConnectorError.connectionRequired
        }
        try await db.insert(into: "Teams")
            .columns("ExternalId", "City", "Name")
            .values(team.ExternalId ?? UUID(uuidString: "00000000-0000-0000-0000-000000000000"), team.City, team.Name)
            .run()
    }
    
    private func updateTeam(team: Team, id: Int) async throws {
        guard let db else {
            throw ConnectorError.connectionRequired
        }
        try await db.update("Teams")
            .set("ExternalId", to: team.ExternalId)
            .set("City", to: team.City)
            .set("Name", to: team.Name)
            .where("Id", .equal, id)
            .run()
    }
    
    func insertOrUpdatePlayer(player: Player) async throws {
        guard db != nil else {
            throw ConnectorError.connectionRequired
        }
        var existingPlayerId: Int? = nil
        if let externalId = player.ExternalId {
            existingPlayerId = try await getPlayerId(externalId: externalId)
        }
        if existingPlayerId == nil {
            existingPlayerId = try await getPlayerId(name: player.Name)
        }
        if let existingPlayerId {
            try await updatePlayer(player: player, id: existingPlayerId)
        } else {
            try await insertPlayer(player: player)
        }
    }
    
    private func getPlayerId(externalId: UUID) async throws -> Int? {
        guard let db else {
            throw ConnectorError.connectionRequired
        }
        let result = try await db.select()
            .column("Id")
            .from("Players")
            .where("ExternalId", .equal, externalId)
            .first()
        return try result?.decode(column: "Id", as: Int.self)
    }
    
    func getPlayerId(name: String) async throws -> Int? {
        guard let db else {
            throw ConnectorError.connectionRequired
        }
        let result = try await db.select()
            .column("Id")
            .from("Players")
            .where("Name", .equal, name)
            .first()
        return try result?.decode(column: "Id", as: Int.self)
    }
    
    private func insertPlayer(player: Player) async throws {
        guard let db else {
            throw ConnectorError.connectionRequired
        }
        try await db.insert(into: "Players")
            .columns("ExternalId", "Name", "FirstName", "LastName")
            .values(player.ExternalId ?? UUID(uuidString: "00000000-0000-0000-0000-000000000000"),
                    player.Name, player.FirstName, player.LastName)
            .run()
    }
    
    private func updatePlayer(player: Player, id: Int) async throws {
        guard let db else {
            throw ConnectorError.connectionRequired
        }
        try await db.update("Players")
            .set("ExternalId", to: player.ExternalId)
            .set("FirstName", to: player.FirstName)
            .set("LastName", to: player.LastName)
            .set("Name", to: player.Name)
            .where("Id", .equal, id)
            .run()
    }
    
    func insertOrUpdateGame(game: Game) async throws {
        guard let db else {
            throw ConnectorError.connectionRequired
        }
        let importer = GameImporter(db: db)
        try await importer.insertOrUpdateGame(game: game)
    }
}
