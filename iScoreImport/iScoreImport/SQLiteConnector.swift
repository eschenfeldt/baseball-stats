//
//  SqliteConnector.swift
//  iScoreImport
//
//  Created by Patrick Eschenfeldt on 6/27/24.
//

import Foundation
import SQLiteKit

struct SQLiteConnector: DbConnector {
    
    var filePath: String
    var logger: Logger
    private var connection: SQLiteConnection?
    private var db: SQLDatabase?
    
    init(filePath: String, logger: Logger = Logger.init(label: "SQLite")) {
        self.filePath = filePath
        self.logger = logger
    }
    
    func getTeams() async throws -> [Team] {
        guard let db else {
            throw ConnectorError.connectionRequired
        }
        return try await db.select()
            .column("guid", as: "ExternalId")
            .column("team_nm", as: "CombinedName")
            .column("team_color", as: "ColorHex")
            .from("team")
            .all(decoding: Team.self)
            .map() {
                Team.withParsedName(team: $0)
            }
    }
    
    func getPlayers(skip: Int? = nil, take: Int? = nil, lastName: String? = nil) async throws -> [Player] {
        guard let db else {
            throw ConnectorError.connectionRequired
        }
        var query = db.select()
            .column("guid", as: "ExternalId")
            .column("first_nm", as: "FirstName")
            .column("last_nm", as: "LastName")
            .column(SQLFunction("CONCAT", args: SQLColumn("first_nm"), SQLLiteral.string(" "), SQLColumn("last_nm")), as: "Name")
            .from("player")
        
        if let lastName {
            query = query.where("last_nm", .equal, SQLLiteral.string(lastName))
        }
            
        return try await query.orderBy(SQLLiteral.string("ROWID"))
            .offset(skip)
            .limit(take)
            .all(decoding: Player.self)
    }
    
    func getGames() async throws -> AsyncThrowingStream<Game, Error> {
        guard let db else {
            throw ConnectorError.connectionRequired
        }
        let loader = GameLoader(db: db)
        return try await loader.loadGames()
    }
    
    mutating func connect() async throws {
        _ = try await getDb()
    }
    
    mutating func getDb() async throws -> SQLDatabase {
        guard let connection else {
            let newConnection = try await SQLiteConnection.open(
                storage: .file(path: filePath),
                logger: logger
            );
            connection = newConnection
            let newDb = newConnection.sql()
            db = newDb
            return newDb
        }
        guard let db else {
            let newDb = connection.sql()
            db = newDb
            return newDb
        }
        
        return db
    }
    
    mutating func close() async throws {
        guard let connection else {
            return
        }
        try await connection.close()
        self.connection = nil;
    }
}
