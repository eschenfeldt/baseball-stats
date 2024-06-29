//
//  Player.swift
//  iScoreImport
//
//  Created by Patrick Eschenfeldt on 6/29/24.
//

import Foundation

struct Player : Codable {
    let Name: String
    let ExternalId: UUID?
    let DateOfBirth: Date?
    let FirstName: String
    let MiddleName: String?
    let LastName: String
    let Suffix: String?
}
