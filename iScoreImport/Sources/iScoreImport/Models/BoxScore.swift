//
//  BoxScore.swift
//  iScoreImport
//
//  Created by Patrick Eschenfeldt on 7/4/24.
//

import Foundation

struct BoxScore: Codable {
    let batters: [Batter]
    let pitchers: [Pitcher]
    let fielders: [Fielder]
}
