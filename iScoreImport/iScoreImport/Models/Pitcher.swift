//
//  Pitcher.swift
//  iScoreImport
//
//  Created by Patrick Eschenfeldt on 7/4/24.
//

import Foundation

struct Pitcher: Codable {
    let PlayerExternalId: UUID
    let PlayerName: String
    let TeamExternalId: UUID
    
    let Number: Int
    let Games: Int
    let Wins: Int
    let Losses: Int
    let Saves: Int
    let ThirdInningsPitched: Int
    let BattersFaced: Int
    let Balls: Int
    let Strikes: Int
    let Runs: Int
    let EarnedRuns: Int
    let Hits: Int
    let Walks: Int
    let IntentionalWalks: Int
    let Strikeouts: Int
    let StrikeoutsCalled: Int
    let StrikeoutsSwinging: Int
    let HitByPitch: Int
    let Balks: Int
    let WildPitches: Int
    let Homeruns: Int
    let GroundOuts: Int
    let AirOuts: Int
    let FirstPitchStrikes: Int
    let FirstPitchBalls: Int
}
