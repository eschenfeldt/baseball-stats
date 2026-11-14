// swift-tools-version: 6.0

import PackageDescription

let package = Package(
    name: "iScoreImport",
    platforms: [.macOS(.v14)],
    dependencies: [
        .package(url: "https://github.com/vapor/postgres-kit.git", from: "2.13.5"),
        .package(url: "https://github.com/vapor/sqlite-kit.git", from: "4.5.2"),
        .package(url: "https://github.com/vapor/sql-kit.git", from: "3.31.1"),
        .package(url: "https://github.com/apple/swift-log.git", from: "1.6.1"),
        .package(url: "https://github.com/apple/swift-argument-parser.git", from: "1.4.0"),
    ],
    targets: [
        .executableTarget(
            name: "iScoreImport",
            dependencies: [
                .product(name: "PostgresKit", package: "postgres-kit"),
                .product(name: "SQLiteKit", package: "sqlite-kit"),
                .product(name: "SQLKit", package: "sql-kit"),
                .product(name: "Logging", package: "swift-log"),
                .product(name: "ArgumentParser", package: "swift-argument-parser"),
            ],
            path: "Sources/iScoreImport",
            swiftSettings: [.swiftLanguageMode(.v5)]
        ),
        .testTarget(
            name: "iScoreImportTests",
            dependencies: ["iScoreImport"],
            path: "Tests/iScoreImportTests",
            resources: [.process("Resources")],
            swiftSettings: [.swiftLanguageMode(.v5)]
        ),
    ]
)
