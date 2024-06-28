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
    var logger: Logger = Logger.init(label: "Sqlite")
    var connection: SQLiteConnection?
    var db: SQLDatabase?
    
    mutating func getDb() async throws -> SQLDatabase {
        guard let connection else {
            let newConnection = try await SQLiteConnection.open(
                storage: .file(path: filePath),
                logger: logger
            );
            connection = newConnection
            return newConnection.sql()
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
