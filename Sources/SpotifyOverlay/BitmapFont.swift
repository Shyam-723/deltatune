import SwiftUI
import AppKit
import Foundation

class BitmapFont {
    private let fontTexture: NSImage
    private let characters: [Int: CharacterInfo]
    private let lineHeight: CGFloat
    
    struct CharacterInfo {
        let x: Int
        let y: Int
        let width: Int
        let height: Int
        let xOffset: Int
        let yOffset: Int
        let xAdvance: Int
    }
    
    init?(fontFile: String, textureFile: String) {
        // Load the texture directly from file path
        guard let texture = NSImage(contentsOfFile: textureFile) else {
            print("Failed to load font texture: \(textureFile)")
            return nil
        }
        
        self.fontTexture = texture
        
        // Parse the .fnt file directly from file path
        guard let fntContent = try? String(contentsOfFile: fontFile) else {
            print("Failed to load font file: \(fontFile)")
            return nil
        }
        
        var characters: [Int: CharacterInfo] = [:]
        var lineHeight: CGFloat = 19 // Default from the font file
        
        let lines = fntContent.components(separatedBy: .newlines)
        for line in lines {
            if line.hasPrefix("common ") {
                // Parse line height
                if let range = line.range(of: "lineHeight=") {
                    let substring = line[range.upperBound...]
                    if let spaceRange = substring.range(of: " ") {
                        let heightStr = String(substring[..<spaceRange.lowerBound])
                        lineHeight = CGFloat(Int(heightStr) ?? 19)
                    }
                }
            } else if line.hasPrefix("char ") {
                // Parse character info
                let components = line.components(separatedBy: " ")
                var charInfo: [String: Int] = [:]
                
                for component in components {
                    if component.contains("=") {
                        let parts = component.components(separatedBy: "=")
                        if parts.count == 2 {
                            charInfo[parts[0]] = Int(parts[1])
                        }
                    }
                }
                
                if let id = charInfo["id"],
                   let x = charInfo["x"],
                   let y = charInfo["y"],
                   let width = charInfo["width"],
                   let height = charInfo["height"],
                   let xOffset = charInfo["xoffset"],
                   let yOffset = charInfo["yoffset"],
                   let xAdvance = charInfo["xadvance"] {
                    
                    characters[id] = CharacterInfo(
                        x: x, y: y, width: width, height: height,
                        xOffset: xOffset, yOffset: yOffset, xAdvance: xAdvance
                    )
                }
            }
        }
        
        self.characters = characters
        self.lineHeight = lineHeight
    }
    
    func measureText(_ text: String) -> CGSize {
        var width: CGFloat = 0
        
        for char in text {
            let charCode = Int(char.asciiValue ?? 32)
            if let charInfo = characters[charCode] {
                width += CGFloat(charInfo.xAdvance)
            } else {
                width += 8 // Default width for unknown characters
            }
        }
        
        return CGSize(width: width, height: lineHeight + 8) // Add extra height for proper rendering
    }
    
    func createTextImage(_ text: String, color: NSColor = .white) -> NSImage? {
        let size = measureText(text)
        let image = NSImage(size: size)
        
        image.lockFocus()
        
        // Clear background
        NSColor.clear.setFill()
        NSRect(origin: .zero, size: size).fill()
        
        var currentX: CGFloat = 0
        let baselineY: CGFloat = 4 // Adjust baseline position
        
        for char in text {
            let charCode = Int(char.asciiValue ?? 32)
            if let charInfo = characters[charCode], charInfo.width > 0 {
                // Extract character from font texture
                let sourceRect = NSRect(
                    x: charInfo.x,
                    y: charInfo.y,
                    width: charInfo.width,
                    height: charInfo.height
                )
                
                let destRect = NSRect(
                    x: currentX + CGFloat(charInfo.xOffset),
                    y: baselineY + CGFloat(charInfo.yOffset), // Use baseline positioning
                    width: CGFloat(charInfo.width),
                    height: CGFloat(charInfo.height)
                )
                
                // Draw character with color tinting
                fontTexture.draw(in: destRect, from: sourceRect, operation: .sourceOver, fraction: 1.0)
                
                currentX += CGFloat(charInfo.xAdvance)
            } else {
                currentX += 8 // Default advance for unknown characters
            }
        }
        
        image.unlockFocus()
        return image
    }
}