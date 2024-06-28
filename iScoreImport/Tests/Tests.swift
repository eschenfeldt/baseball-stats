//
//  Tests.swift
//  Tests
//
//  Created by Patrick Eschenfeldt on 6/27/24.
//
import Foundation
import Testing
import SQLKit
import SQLiteKit

struct Tests {

    @Suite("SQLite Tests")
    final class SQLiteTests {
        
        let fileName = "Test_iScore_baseball"
        let filePath: String?
        
        init() {
            let testBundle = Bundle(for: type(of: self))
            guard let resourceURL = testBundle.url(forResource: fileName, withExtension: "sqlite") else {
                filePath = nil
                return
            }
            filePath = resourceURL.absoluteString
        }
       
        @Test("Connector Test")
        func testConnection() async throws {
            #expect(filePath != nil)
            guard let filePath else { return }
            var connector = SQLiteConnector(filePath: filePath)
            let db = try await connector.getDb()
            let result = try await db.select()
                .column("game_nm", as: "Game Name")
                .from("game")
                .limit(1)
                .all()
            #expect(result.count == 1)
            let decoded = try result[0].decode(column: "Game Name", as: String.self)
            #expect(decoded == "8/26/11 Chicago Cubs at Milwaukee Brewers ")
            try await connector.close()
        }
    }

}
