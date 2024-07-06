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
    var HomeScore: Int?
    var AwayScore: Int?
    let HomeTeamName: String
    let AwayTeamName: String
    var WinningTeam: Team?
    var LosingTeam: Team?
    let WinningPitcher: Player?
    let LosingPitcher: Player?
    let SavingPitcher: Player?
    var HomeBoxScore: BoxScore?
    var AwayBoxScore: BoxScore?
    
    mutating func assignWinner() {
        guard let homeScore = self.HomeScore, let awayScore = self.AwayScore else { return }
        if homeScore > awayScore {
            self.WinningTeam = self.HomeTeam
            self.LosingTeam = self.AwayTeam
        } else if awayScore > homeScore {
            self.WinningTeam = self.AwayTeam
            self.LosingTeam = self.HomeTeam
        }
    }
}
