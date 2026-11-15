//
//  Batter.swift
//  iScoreImport
//
//  Created by Patrick Eschenfeldt on 7/4/24.
//

import Foundation

struct Batter: Codable {
    let PlayerExternalId: UUID
    var Player: Player?
    let TeamExternalId: UUID

    let Number: Int
    /// 1-based batting order slot; filled in from the lineup / play-by-play after the stats load
    var LineupPosition: Int?
    /// 1 for the starter in a lineup slot, 2 for the first substitute into that slot, etc.
    var AppearanceOrder: Int?
    let Games: Int
    let PlateAppearances: Int
    let AtBats: Int
    let Runs: Int
    let BuntSingles: Int
    let Singles: Int
    let Doubles: Int
    let Triples: Int
    let Homeruns: Int
    let RunsBattedIn: Int
    let Walks: Int
    let StrikeoutsCalled: Int
    let StrikeoutsSwinging: Int
    let HitByPitch: Int
    let StolenBases: Int
    let CaughtStealing: Int
    let SacrificeBunts: Int
    let SacrificeFlies: Int
    let ReachedOnError: Int
    let FieldersChoices: Int
    let CatchersInterference: Int
    let GroundedIntoDoublePlay: Int
    let GroundedIntoTriplePlay: Int
}
