import Foundation
import AppKit

class SpotifyMonitor {
    private var timer: Timer?
    private let updateCallback: (TrackInfo?) -> Void
    private var lastTrackInfo: TrackInfo?
    
    init(updateCallback: @escaping (TrackInfo?) -> Void) {
        self.updateCallback = updateCallback
    }
    
    func startMonitoring() {
        print("🎵 Starting Spotify monitoring...")
        
        // Check every 4 seconds for Spotify updates (good balance of responsiveness vs efficiency)
        timer = Timer.scheduledTimer(withTimeInterval: 4.0, repeats: true) { [weak self] _ in
            self?.checkSpotifyStatus()
        }
        
        // Initial check
        checkSpotifyStatus()
    }
    
    func stopMonitoring() {
        timer?.invalidate()
        timer = nil
    }
    
    private func checkSpotifyStatus() {
        getCurrentTrack { [weak self] trackInfo in
            // Only update if track info changed
            if let trackInfo = trackInfo,
               trackInfo.title != self?.lastTrackInfo?.title ||
               trackInfo.artist != self?.lastTrackInfo?.artist ||
               trackInfo.isPlaying != self?.lastTrackInfo?.isPlaying {
                
                print("🎵 Now playing: \(trackInfo.artist) - \(trackInfo.title)")
                self?.lastTrackInfo = trackInfo
                self?.updateCallback(trackInfo.isPlaying ? trackInfo : nil)
            } else if trackInfo == nil && self?.lastTrackInfo != nil {
                print("🔇 Music stopped")
                self?.lastTrackInfo = nil
                self?.updateCallback(nil)
            }
        }
    }
    
    private func getCurrentTrack(completion: @escaping (TrackInfo?) -> Void) {
        // Use AppleScript to get current Spotify track
        let script = """
        try
            tell application "Spotify"
                if player state is playing then
                    set trackName to name of current track
                    set artistName to artist of current track
                    return trackName & "|" & artistName & "|playing"
                else if player state is paused then
                    set trackName to name of current track
                    set artistName to artist of current track
                    return trackName & "|" & artistName & "|paused"
                else
                    return "||stopped"
                end if
            end tell
        on error errMsg
            return "ERROR:" & errMsg
        end try
        """
        
        let appleScript = NSAppleScript(source: script)
        var error: NSDictionary?
        
        DispatchQueue.global(qos: .background).async {
            let result = appleScript?.executeAndReturnError(&error)
            
            DispatchQueue.main.async {
                if let error = error {
                    // Only print errors if they're not permission-related
                    let errorString = "\(error)"
                    if !errorString.contains("Not authorized") {
                        print("AppleScript error: \(error)")
                    }
                    completion(nil)
                    return
                }
                
                guard let resultString = result?.stringValue else {
                    completion(nil)
                    return
                }
                
                let components = resultString.components(separatedBy: "|")
                guard components.count >= 3 else {
                    completion(nil)
                    return
                }
                
                let title = components[0].trimmingCharacters(in: .whitespacesAndNewlines)
                let artist = components[1].trimmingCharacters(in: .whitespacesAndNewlines)
                let state = components[2].trimmingCharacters(in: .whitespacesAndNewlines)
                
                if state == "playing" && !title.isEmpty && !artist.isEmpty {
                    let trackInfo = TrackInfo(
                        title: title,
                        artist: artist,
                        isPlaying: true
                    )
                    completion(trackInfo)
                } else {
                    completion(nil)
                }
            }
        }
    }
    
    deinit {
        stopMonitoring()
    }
}