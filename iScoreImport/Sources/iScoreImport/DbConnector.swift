//
//  DbConnector.swift
//  iScoreImport
//
//  Created by Patrick Eschenfeldt on 6/27/24.
//

import Foundation
import SQLKit

enum ConnectorError: Error {
    case connectionRequired
}

protocol DbConnector {
    
    mutating func getDb() async throws -> SQLDatabase
    
    mutating func connect() async throws
    
    mutating func close() async throws
    
}
