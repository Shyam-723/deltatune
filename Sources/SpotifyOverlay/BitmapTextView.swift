import SwiftUI
import AppKit

struct BitmapTextView: NSViewRepresentable {
    let text: String
    let font: BitmapFont
    let color: NSColor
    
    init(text: String, font: BitmapFont, color: NSColor = .white) {
        self.text = text
        self.font = font
        self.color = color
    }
    
    func makeNSView(context: Context) -> NSImageView {
        let imageView = NSImageView()
        imageView.imageScaling = .scaleNone
        imageView.imageAlignment = .alignLeft
        updateView(imageView)
        return imageView
    }
    
    func updateNSView(_ nsView: NSImageView, context: Context) {
        updateView(nsView)
    }
    
    private func updateView(_ imageView: NSImageView) {
        if let textImage = font.createTextImage(text, color: color) {
            imageView.image = textImage
        }
    }
}

// Singleton font manager
class FontManager {
    static let shared = FontManager()
    private var musicTitleFont: BitmapFont?
    
    private init() {
        loadFonts()
    }
    
    private func loadFonts() {
        // Try to load from current directory Resources folder
        let currentDir = FileManager.default.currentDirectoryPath
        let fntPath = currentDir + "/Resources/MusicTitleFont.fnt"
        let pngPath = currentDir + "/Resources/MusicTitleFont.png"
        
        print("Looking for fonts at:")
        print("FNT: \(fntPath)")
        print("PNG: \(pngPath)")
        
        if FileManager.default.fileExists(atPath: fntPath) && 
           FileManager.default.fileExists(atPath: pngPath) {
            musicTitleFont = BitmapFont(fontFile: fntPath, textureFile: pngPath)
            print("✅ Loaded MusicTitleFont successfully")
        } else {
            print("❌ Font files not found")
            print("FNT exists: \(FileManager.default.fileExists(atPath: fntPath))")
            print("PNG exists: \(FileManager.default.fileExists(atPath: pngPath))")
        }
    }
    
    func getMusicTitleFont() -> BitmapFont? {
        return musicTitleFont
    }
}