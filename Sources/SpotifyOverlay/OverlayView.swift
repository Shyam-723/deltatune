import SwiftUI
import AppKit

struct OverlayView: View {
    let trackInfo: TrackInfo?
    
    init(trackInfo: TrackInfo? = nil) {
        self.trackInfo = trackInfo
    }
    
    var body: some View {
        HStack(spacing: 8) {
            if let trackInfo = trackInfo {
                // Use a pixelated-style system font that looks similar
                Text("♪")
                    .font(.system(size: 14, weight: .bold, design: .monospaced))
                    .foregroundColor(.white)
                
                Text("NOW PLAYING: \(trackInfo.artist) - \(trackInfo.title)")
                    .font(.system(size: 14, weight: .bold, design: .monospaced))
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