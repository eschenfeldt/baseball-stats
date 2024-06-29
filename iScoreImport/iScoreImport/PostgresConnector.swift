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
    
    var configFileName: String
    var config: SQLPostgresConfiguration?
    var logger = Logger.init(label:"Postgres")
    var connection: PostgresConnection?
    var db: SQLDatabase?
    
    class __ { } // empty class to look up current bundle
    
    private mutating func getConfig() throws {
        let bundle = Bundle(for: PostgresConnector.__.self)
        guard let resourceURL = bundle.url(forResource: configFileName, withExtension: "postgres_config"), let data = try? Data(contentsOf: resourceURL) else {
            throw PostgresConnectionError.invalidConfigPath(path: configFileName)
        }
        
        let decoder = JSONDecoder()
        
        if let decoded = try? decoder.decode(PostgresConfig.self, from: data) {
            config = SQLPostgresConfiguration(hostname: decoded.hostname, username: decoded.username, password: decoded.password, database: decoded.database, tls: .prefer(try .init(configuration: .clientDefault)))
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
    
    
    func getTeam(externalId: UUID) async throws -> Team? {
        return nil
    }
    
    func getTeam(city: String, name: String) async throws -> Team? {
        return nil
    }
    
    func getPlayer(externalId: UUID) async throws -> Player? {
        return nil
    }
    
    func getPlayer(firstName: String, lastName: String) async throws -> Player? {
        return nil
    }
}
