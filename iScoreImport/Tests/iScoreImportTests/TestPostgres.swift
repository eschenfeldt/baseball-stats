//
//  TestPostgres.swift
//  iScoreImportTests
//
//  Everything the Postgres-backed tests know about their environment lives here.
//  Each test gets a private, freshly-migrated scratch database, so the suites are
//  independent of each other, of run order, and of whatever happens to be sitting
//  in a developer's local database.
//

import Foundation
import PostgresKit
@testable import iScoreImport

enum TestPostgresError: Error {
    /// No password could be resolved, so we can't talk to a server at all.
    case notConfigured
    case missingSchemaResource
}

/// Where to find a Postgres server, and who to connect as.
///
/// The defaults describe the `db` service in the repo's `compose.yaml` as published on
/// the host, so `docker compose --profile test up -d db` followed by `swift test` works
/// with no configuration at all. Every field can be overridden by an environment
/// variable, which is how the containerized CI run points at the `db` service directly.
struct TestPostgresSettings {
    var hostname: String
    var port: Int
    var username: String
    var password: String
    /// An existing database to connect to while creating and dropping scratch databases.
    var adminDatabase: String

    static func fromEnvironment() -> TestPostgresSettings? {
        let env = ProcessInfo.processInfo.environment
        guard let password = resolvePassword(env) else {
            return nil
        }
        return TestPostgresSettings(
            hostname: env["ISCORE_TEST_PG_HOST"] ?? "localhost",
            port: env["ISCORE_TEST_PG_PORT"].flatMap(Int.init) ?? 5433,
            username: env["ISCORE_TEST_PG_USER"] ?? "baseball owner",
            password: password,
            adminDatabase: env["ISCORE_TEST_PG_ADMIN_DB"] ?? "postgres"
        )
    }

    private static func resolvePassword(_ env: [String: String]) -> String? {
        if let password = env["ISCORE_TEST_PG_PASSWORD"], !password.isEmpty {
            return password
        }
        let path = env["ISCORE_TEST_PG_PASSWORD_FILE"] ?? defaultPasswordFilePath()
        guard let path,
              let contents = try? String(contentsOfFile: path, encoding: .utf8) else {
            return nil
        }
        let trimmed = contents.trimmingCharacters(in: .whitespacesAndNewlines)
        return trimmed.isEmpty ? nil : trimmed
    }

    /// The `baseball_password.txt` at the repo root, which compose already uses as the
    /// `db` service's password. Anyone set up to run the .NET tests can run these too.
    private static func defaultPasswordFilePath() -> String? {
        var directory = URL(fileURLWithPath: #filePath).deletingLastPathComponent()
        while directory.path != "/" {
            let candidate = directory.appendingPathComponent("baseball_password.txt")
            if FileManager.default.fileExists(atPath: candidate.path) {
                return candidate.path
            }
            directory = directory.deletingLastPathComponent()
        }
        return nil
    }
}

enum TestPostgres {

    static let settings = TestPostgresSettings.fromEnvironment()

    /// Drives `.enabled(if:)` on the database-backed suites so an unconfigured machine
    /// skips them instead of reporting a wall of failures.
    static var isConfigured: Bool { settings != nil }

    /// Runs `body` against a scratch database containing the API's schema and nothing
    /// else. The database is dropped afterwards whether or not `body` throws.
    static func withScratchDatabase<T>(
        _ body: (inout PostgresConnector) async throws -> T
    ) async throws -> T {
        guard let settings else {
            throw TestPostgresError.notConfigured
        }
        let databaseName = "iscore_test_\(UUID().uuidString.replacingOccurrences(of: "-", with: "").lowercased())"

        try await onAdminConnection(settings) { admin in
            _ = try await admin.simpleQuery(#"CREATE DATABASE "\#(databaseName)""#).get()
        }

        do {
            let result = try await withScratchConnector(settings, databaseName, body)
            if keepAllDatabases {
                reportKept(settings, databaseName, reason: "ISCORE_TEST_KEEP_DB is set")
            } else {
                try await dropDatabase(settings, databaseName)
            }
            return result
        } catch {
            // PostgresNIO redacts its errors' plain descriptions, which is all Swift Testing
            // shows for a thrown error; the server message and query are only in the
            // reflecting form.
            print("[\(databaseName)] \(String(reflecting: error))")
            // Scratch databases hold nothing but test data, so keep the evidence.
            reportKept(settings, databaseName, reason: "the test threw")
            throw error
        }
    }

    /// Keeps every scratch database, not just those whose test threw. Failed `#expect`s
    /// don't throw, so this is the way to inspect the state behind one.
    private static var keepAllDatabases: Bool {
        let value = ProcessInfo.processInfo.environment["ISCORE_TEST_KEEP_DB"] ?? ""
        return !value.isEmpty && value != "0"
    }

    private static func reportKept(
        _ settings: TestPostgresSettings,
        _ databaseName: String,
        reason: String
    ) {
        print("""
            Kept scratch database \(databaseName) (\(reason)). Inspect with:
              psql -h \(settings.hostname) -p \(settings.port) -U "\(settings.username)" \(databaseName)
            Drop it when done with:
              DROP DATABASE \(databaseName);
            """)
    }

    private static func withScratchConnector<T>(
        _ settings: TestPostgresSettings,
        _ databaseName: String,
        _ body: (inout PostgresConnector) async throws -> T
    ) async throws -> T {
        try await applySchema(settings, databaseName)

        // Go through a real config file so the tests exercise the same loading path
        // production imports use.
        let configPath = try writeConfigFile(settings, databaseName)
        defer { try? FileManager.default.removeItem(atPath: configPath) }

        var connector = PostgresConnector(configFilePath: configPath)
        do {
            let result = try await body(&connector)
            try await connector.close()
            return result
        } catch {
            try? await connector.close()
            throw error
        }
    }

    private static func applySchema(
        _ settings: TestPostgresSettings,
        _ databaseName: String
    ) async throws {
        let statements = SQLScript.statements(in: try loadSchemaScript())
        try await onConnection(settings, database: databaseName) { connection in
            for statement in statements {
                _ = try await connection.simpleQuery(statement).get()
            }
        }
    }

    private static func loadSchemaScript() throws -> String {
        guard let url = Bundle.module.url(forResource: "schema", withExtension: "sql") else {
            throw TestPostgresError.missingSchemaResource
        }
        let script = try String(contentsOf: url, encoding: .utf8)
        // `dotnet ef migrations script` writes a UTF-8 BOM, which Postgres won't accept
        // as the first character of a statement.
        return script.hasPrefix("\u{FEFF}") ? String(script.dropFirst()) : script
    }

    private static func writeConfigFile(
        _ settings: TestPostgresSettings,
        _ databaseName: String
    ) throws -> String {
        let config = PostgresConfig(hostname: settings.hostname,
                                    database: databaseName,
                                    username: settings.username,
                                    password: settings.password,
                                    port: settings.port)
        let url = FileManager.default.temporaryDirectory
            .appendingPathComponent("\(databaseName).postgres_config")
        try JSONEncoder().encode(config).write(to: url)
        return url.path
    }

    private static func dropDatabase(
        _ settings: TestPostgresSettings,
        _ databaseName: String
    ) async throws {
        try await onAdminConnection(settings) { admin in
            // FORCE terminates any connection the test left open.
            _ = try await admin.simpleQuery(#"DROP DATABASE IF EXISTS "\#(databaseName)" WITH (FORCE)"#).get()
        }
    }

    private static func onAdminConnection(
        _ settings: TestPostgresSettings,
        _ body: (PostgresConnection) async throws -> Void
    ) async throws {
        try await onConnection(settings, database: settings.adminDatabase, body)
    }

    private static func onConnection(
        _ settings: TestPostgresSettings,
        database: String,
        _ body: (PostgresConnection) async throws -> Void
    ) async throws {
        let configuration = PostgresConnection.Configuration(
            host: settings.hostname,
            port: settings.port,
            username: settings.username,
            password: settings.password,
            database: database,
            tls: .disable
        )
        let connection = try await PostgresConnection.connect(
            configuration: configuration,
            id: 0,
            logger: Logger(label: "TestPostgres")
        )
        do {
            try await body(connection)
            try await connection.close()
        } catch {
            try? await connection.close()
            throw error
        }
    }
}
