//
//  DbConnector.swift
//  iScoreImport
//
//  Created by Patrick Eschenfeldt on 6/27/24.
//

import Foundation
import SQLKit

protocol DbConnector {
    
    mutating func getDb() async throws -> SQLDatabase
    
    mutating func close() async throws
}
