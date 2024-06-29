//
//  PostgresTests.swift
//  Tests
//
//  Created by Patrick Eschenfeldt on 6/28/24.
//

import Testing

struct PostgresTests {
    
    let configFilePath = "test"

    @Test("Connection Test")
    func testConnection() async throws {
        var connector = PostgresConnector(configFileName: configFilePath)
        do {
            let db = try await connector.getDb()
            let result = try await db.select()
                .column("Name")
                .from("Teams")
                .limit(1)
                .all()
            try #require(result.count == 1)
            let decoded = try result[0].decode(column: "Name", as: String.self)
            #expect(decoded == "Test Team")
        } catch {
            print(String(reflecting: error))
            throw error
        }
        try await connector.close()
    }

}
