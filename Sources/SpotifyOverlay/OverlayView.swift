import SwiftUI
import AppKit

struct OverlayView: View {
    let trackInfo: TrackInfo?
    private let customFont: Font?
    
    init(trackInfo: TrackInfo? = nil) {
        self.trackInfo = trackInfo
        
        // Load the custom TTF font
        var fontURL: URL?
        
        // Try to find the font in the module bundle
        if let moduleBundle = Bundle.module.url(forResource: "NewMusticTitleFont", withExtension: "ttf") {
            fontURL = moduleBundle
        } else if let moduleBundle = Bundle.module.url(forResource: "Resources/NewMusticTitleFont", withExtension: "ttf") {
            fontURL = moduleBundle
        } else if let mainBundle = Bundle.main.url(forResource: "NewMusticTitleFont", withExtension: "ttf") {
            fontURL = mainBundle
        } else if let mainBundle = Bundle.main.url(forResource: "Resources/NewMusticTitleFont", withExtension: "ttf") {
            fontURL = mainBundle
        }
        
        if let fontURL = fontURL {
            print("Found font at: \(fontURL.path)")
            CTFontManagerRegisterFontsForURL(fontURL as CFURL, .process, nil)
            
            // Try to get the actual font family name from the TTF file
            if let fontDescriptors = CTFontManagerCreateFontDescriptorsFromURL(fontURL as CFURL) as? [CTFontDescriptor],
               let fontDescriptor = fontDescriptors.first,
               let fontName = CTFontDescriptorCopyAttribute(fontDescriptor, kCTFontFamilyNameAttribute) as? String {
                print("Loaded font with family name: \(fontName)")
                self.customFont = Font.custom(fontName, size: 30)
            } else {
                // Try common variations of the font name
                print("Trying font name: NewMusticTitleFont")
                self.customFont = Font.custom("NewMusticTitleFont", size: 18)
            }
        } else {
            print("Failed to find TTF font file in any bundle")
            // Fallback to system font if TTF loading fails
            self.customFont = Font.system(size: 18, weight: .bold, design: .monospaced)
        }
    }
    
    var body: some View {
        HStack(spacing: 8) {
            if let trackInfo = trackInfo {
                Text("♪")
                    .font(customFont)
                    .foregroundColor(.white)
                
                Text("\(trackInfo.artist) - \(trackInfo.title)")
                    .font(customFont)
                    .foregroundColor(.white)
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