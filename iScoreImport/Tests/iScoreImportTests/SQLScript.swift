//
//  SQLScript.swift
//  iScoreImportTests
//

import Foundation

/// Splits a multi-statement SQL script into individual statements.
///
/// PostgresNIO only speaks the extended query protocol — `simpleQuery` is a deprecated
/// shim over `query`, not the simple protocol — and the server rejects more than one
/// command per parse message. So the schema script has to be fed in one statement at a
/// time, which means knowing where statements actually end: `dotnet ef migrations script`
/// wraps every migration in a `DO $EF$ … $EF$` block full of semicolons.
enum SQLScript {

    static func statements(in script: String) -> [String] {
        let chars = Array(script)
        var statements: [String] = []
        var current = ""
        var i = 0

        func matches(_ token: [Character], at index: Int) -> Bool {
            guard index + token.count <= chars.count else { return false }
            return Array(chars[index ..< (index + token.count)]) == token
        }

        while i < chars.count {
            let c = chars[i]

            // -- line comment
            if matches(["-", "-"], at: i) {
                while i < chars.count && chars[i] != "\n" {
                    current.append(chars[i])
                    i += 1
                }
                continue
            }

            // /* block comment */
            if matches(["/", "*"], at: i) {
                while i < chars.count && !matches(["*", "/"], at: i) {
                    current.append(chars[i])
                    i += 1
                }
                if i < chars.count {
                    current.append("*")
                    current.append("/")
                    i += 2
                }
                continue
            }

            // 'string literal', where '' is an escaped quote
            if c == "'" {
                current.append(c)
                i += 1
                while i < chars.count {
                    if matches(["'", "'"], at: i) {
                        current.append("'")
                        current.append("'")
                        i += 2
                        continue
                    }
                    current.append(chars[i])
                    let isClosing = chars[i] == "'"
                    i += 1
                    if isClosing { break }
                }
                continue
            }

            // $tag$ dollar-quoted body $tag$
            if c == "$", let tag = dollarQuoteTag(chars, at: i) {
                let tagChars = Array(tag)
                current += tag
                i += tagChars.count
                while i < chars.count && !matches(tagChars, at: i) {
                    current.append(chars[i])
                    i += 1
                }
                if i < chars.count {
                    current += tag
                    i += tagChars.count
                }
                continue
            }

            if c == ";" {
                statements.append(current)
                current = ""
                i += 1
                continue
            }

            current.append(c)
            i += 1
        }
        statements.append(current)

        return statements
            .map { $0.trimmingCharacters(in: .whitespacesAndNewlines) }
            .filter { !$0.isEmpty }
    }

    /// Returns the full opening delimiter (`"$EF$"`, `"$$"`) if one starts at `index`.
    private static func dollarQuoteTag(_ chars: [Character], at index: Int) -> String? {
        var tag = "$"
        var i = index + 1
        while i < chars.count {
            let c = chars[i]
            if c == "$" {
                return tag + "$"
            }
            // Tags follow identifier rules: no leading digit.
            let isValid = c.isLetter || c == "_" || (c.isNumber && i > index + 1)
            guard isValid else { return nil }
            tag.append(c)
            i += 1
        }
        return nil
    }
}
