//
//  Fielder.swift
//  iScoreImport
//
//  Created by Patrick Eschenfeldt on 7/4/24.
//

import Foundation

struct Fielder: Codable {
    let PlayerExternalId: UUID
    var Player: Player?
    let TeamExternalId: UUID
    
    let Number: Int
    let Games: Int
    let ErrorsThrowing: Int
    let ErrorsFielding: Int
    let Putouts: Int
    let Assists: Int
    let StolenBaseAttempts: Int
    let CaughtStealing: Int
    let DoublePlays: Int
    let TriplePlays: Int
    let PassedBalls: Int
    let PickoffFailed: Int
    let PickoffSuccess: Int
}
