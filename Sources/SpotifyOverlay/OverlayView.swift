import SwiftUI
import AppKit

struct OverlayView: View {
    let trackInfo: TrackInfo?
    @ObservedObject private var settings = Settings.shared
    private let customFont: Font?
    
    init(trackInfo: TrackInfo? = nil) {
        self.trackInfo = trackInfo
        
        // Load the custom font (either user's custom font or bundled font)
        var fontURL: URL?
        
        // First try user's custom font
        if let customPath = Settings.shared.customFontPath {
            let customURL = URL(fileURLWithPath: customPath)
            if FileManager.default.fileExists(atPath: customPath) {
                fontURL = customURL
            }
        }
        
        // If no custom font, try bundled font
        if fontURL == nil {
            if let moduleBundle = Bundle.module.url(forResource: "NewMusticTitleFont", withExtension: "ttf") {
                fontURL = moduleBundle
            } else if let moduleBundle = Bundle.module.url(forResource: "Resources/NewMusticTitleFont", withExtension: "ttf") {
                fontURL = moduleBundle
            } else if let mainBundle = Bundle.main.url(forResource: "NewMusticTitleFont", withExtension: "ttf") {
                fontURL = mainBundle
            } else if let mainBundle = Bundle.main.url(forResource: "Resources/NewMusticTitleFont", withExtension: "ttf") {
                fontURL = mainBundle
            }
        }
        
        if let fontURL = fontURL {
            print("Found font at: \(fontURL.path)")
            CTFontManagerRegisterFontsForURL(fontURL as CFURL, .process, nil)
            
            // Try to get the actual font family name from the TTF file
            if let fontDescriptors = CTFontManagerCreateFontDescriptorsFromURL(fontURL as CFURL) as? [CTFontDescriptor],
               let fontDescriptor = fontDescriptors.first,
               let fontName = CTFontDescriptorCopyAttribute(fontDescriptor, kCTFontFamilyNameAttribute) as? String {
                print("Loaded font with family name: \(fontName)")
                self.customFont = Font.custom(fontName, size: Settings.shared.fontSize)
            } else {
                // Try common variations of the font name
                print("Trying font name from file")
                let fontName = fontURL.deletingPathExtension().lastPathComponent
                self.customFont = Font.custom(fontName, size: Settings.shared.fontSize)
            }
        } else {
            print("No custom font found, using system font")
            self.customFont = nil
        }
    }
    
    var body: some View {
        HStack(spacing: 8) {
            if let trackInfo = trackInfo {
                Text("♪")
                    .font(customFont ?? Font.system(size: settings.fontSize, weight: .bold, design: .monospaced))
                    .foregroundColor(Color(red: 0.0, green: 1.0, blue: 0.8))
                
                Text("\(trackInfo.artist) - \(trackInfo.title)")
                    .font(customFont ?? Font.system(size: settings.fontSize, weight: .bold, design: .monospaced))
                    .foregroundColor(Color(red: 0.0, green: 1.0, blue: 0.8))
                    .lineLimit(1)
                    .truncationMode(.tail)
            }
        }
        .padding(.horizontal, 12)
        .padding(.vertical, 6)
        .background(
            // Semi-transparent dark background like in your reference
            Rectangle()
                .fill(Color.black.opacity(0.6))
        )
        .frame(maxWidth: .infinity, maxHeight: .infinity, alignment: .center)
    }
}

struct TrackInfo {
    let title: String
    let artist: String
    let isPlaying: Bool
}