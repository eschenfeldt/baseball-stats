//
//  iScoreImport.swift
//  iScoreImport
//
//  Created by Patrick Eschenfeldt on 6/27/24.
//

import Foundation
import SQLiteKit
import ArgumentParser

@main
struct Import: AsyncParsableCommand {
    
    enum DataType: String, ExpressibleByArgument, CaseIterable {
        case Teams = "Teams"
        case Players = "Players"
        case Games = "Games"
    }
    
    @Option(name:.shortAndLong, help:"Update data of these types")
    var includedTypes: [DataType] = []
    @Option(name:[.short, .customLong("source-file")], help: "Path to source sqlite file, with extension")
    var sourceFilePath: String
    @Option(name:[.short, .customLong("target-config")], help: "Path to config file for target Postgres instance, with extension")
    var targetFilePath: String
    
    func run() async throws {
        
        guard includedTypes.count > 0 else {
            print("No types specified to import. Not doing anything")
            return
        }
        
        var reader = SQLiteConnector(filePath: sourceFilePath)
        do {
            try await reader.connect()
        } catch {
            print("Could not connect to source file \(sourceFilePath)")
            try? await reader.close()
            throw error
        }
        var writer = PostgresConnector(configFilePath: targetFilePath)
        do {
            try await writer.connect()
        } catch {
            print("Could not connect to target db based on config file \(targetFilePath)")
            try? await reader.close()
            try? await writer.close()
            print(String(reflecting: error))
            throw error
        }

        do {
            let typeLookup = Set(includedTypes)
            if typeLookup.contains(.Teams) {
                print("Updating teams")
                try await updateTeams(from: reader, to: writer)
            }
            if typeLookup.contains(.Players) {
                print("Updating players")
                try await updatePlayers(from: reader, to: writer)
            }
            if typeLookup.contains(.Games) {
                print("Updating games")
                try await updateGames(from: reader, to: writer)
            }
        } catch {
            print("Error encountered. Closing connections")
            try? await reader.close()
            try? await writer.close()
            throw error
        }
        
        try? await reader.close()
        try? await writer.close()
    }
    
    func updateTeams(from: SQLiteConnector, to: PostgresConnector) async throws {
        let teams = try await from.getTeams()
        for team in teams {
            if team.City == nil || team.Name == nil {
                print("*** Couldn't parse city and name from combined name \(team.CombinedName!)")
            } else {
                print("Updating team: City: '\(team.City!)' Name: '\(team.Name!)'")
                try await to.insertOrUpdateTeam(team: team)
            }
        }
    }
    
    func updatePlayers(from: SQLiteConnector, to: PostgresConnector) async throws {
        let players = try await from.getPlayers()
        for player in players {
            print("Updating player: '\(player.Name)'")
            try await to.insertOrUpdatePlayer(player: player)
        }
    }
    
    func updateGames(from: SQLiteConnector, to: PostgresConnector) async throws {
        let games = try await from.getGames()
        for try await game in games {
            print("Inserting or updating game: \(game.Name)")
            try await to.insertOrUpdateGame(game: game)
        }
    }
}
