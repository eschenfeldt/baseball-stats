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
        guard let db else {
            throw ConnectorError.connectionRequired
        }
        let importer = TeamImporter(db: db)
        try await importer.insertOrUpdateTeam(team: team)
    }
    
    func getTeamId(city: String, name: String) async throws -> Int? {
        guard let db else {
            throw ConnectorError.connectionRequired
        }
        let importer = TeamImporter(db: db)
        return try await importer.getTeamId(city: city, name: name)
    }
    
    func insertOrUpdatePlayer(player: Player) async throws {
        guard let db else {
            throw ConnectorError.connectionRequired
        }
        let importer = PlayerImporter(db: db)
        return try await importer.insertOrUpdatePlayer(player: player)
    }
    
    func getPlayerId(externalId: UUID) async throws -> Int? {
        guard let db else {
            throw ConnectorError.connectionRequired
        }
        let importer = PlayerImporter(db: db)
        return try await importer.getPlayerId(externalId: externalId)
    }
    
    func getPlayerId(name: String) async throws -> Int? {
        guard let db else {
            throw ConnectorError.connectionRequired
        }
        let importer = PlayerImporter(db: db)
        return try await importer.getPlayerId(name: name)
    }
    
    func insertOrUpdateGame(game: Game) async throws {
        guard let db else {
            throw ConnectorError.connectionRequired
        }
        let importer = GameImporter(db: db)
        try await importer.insertOrUpdateGame(game: game)
    }
    
    func getGameId(name: String) async throws -> Int? {
        guard let db else {
            throw ConnectorError.connectionRequired
        }
        let importer = GameImporter(db: db)
        return try await importer.getGameId(name: name)
    }
}
