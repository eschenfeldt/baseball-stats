//
//  TestFixtures.swift
//  iScoreImportTests
//

import Foundation
@testable import iScoreImport

enum TestFixtureError: Error {
    case missingResource(String)
}

enum TestFixtures {
    /// A trimmed-down iScore export, committed alongside the tests.
    ///
    /// This is a plain filesystem path rather than a `file://` URL: SQLite only accepts
    /// the latter where URI filenames are compiled in, which is true of macOS's system
    /// SQLite but not something to rely on elsewhere.
    static func iScoreDatabasePath() throws -> String {
        guard let url = Bundle.module.url(forResource: "Test_iScore_baseball", withExtension: "sqlite") else {
            throw TestFixtureError.missingResource("Test_iScore_baseball.sqlite")
        }
        return url.path
    }

    /// Runs `body` against a connected reader for the bundled iScore export, closing it
    /// afterwards whether or not `body` throws.
    static func withiScoreReader<T>(
        _ body: (inout SQLiteConnector) async throws -> T
    ) async throws -> T {
        var reader = SQLiteConnector(filePath: try iScoreDatabasePath())
        try await reader.connect()
        do {
            let result = try await body(&reader)
            try await reader.close()
            return result
        } catch {
            try? await reader.close()
            throw error
        }
    }
}
