//
//  Game.swift
//  iScoreImport
//
//  Created by Patrick Eschenfeldt on 6/29/24.
//

import Foundation

struct Game : Codable {
    let Name: String
    let ExternalId: UUID?
    let Date: Date
    let HomeTeam: Team
    let AwayTeam: Team
    let ScheduledTime: Date?
    let StartTime: Date?
    let EndTime: Date?
    let Location: Park?
    let HomeScore: Int?
    let AwayScore: Int?
    let WinningTeam: Team?
    let LosingTeam: Team?
    let WinningPitcher: Player?
    let LosingPitcher: Player?
    let SavingPitcher: Player?
}
