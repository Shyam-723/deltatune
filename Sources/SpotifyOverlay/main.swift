import SwiftUI
import AppKit
import Foundation

// Main entry point
let app = NSApplication.shared
let delegate = AppDelegate()
app.delegate = delegate
app.run()

class AppDelegate: NSObject, NSApplicationDelegate {
    var overlayWindow: NSWindow?
    var spotifyMonitor: SpotifyMonitor?
    
    func applicationDidFinishLaunching(_ notification: Notification) {
        // Hide dock icon and menu bar
        NSApp.setActivationPolicy(.accessory)
        
        // Create overlay window
        createOverlayWindow()
        
        // Start monitoring Spotify
        spotifyMonitor = SpotifyMonitor { [weak self] trackInfo in
            DispatchQueue.main.async {
                self?.updateOverlay(with: trackInfo)
            }
        }
        spotifyMonitor?.startMonitoring()
        
        // Spotify monitoring is now working!

    }
    
    private func createOverlayWindow() {
        // Use the screen with the mouse cursor (active screen)
        let mouseLocation = NSEvent.mouseLocation
        let activeScreen = NSScreen.screens.first { screen in
            NSMouseInRect(mouseLocation, screen.frame, false)
        } ?? NSScreen.main
        
        let screenFrame = activeScreen?.frame ?? NSRect(x: 0, y: 0, width: 1920, height: 1080)
        
        // Create window frame - positioned in top-right area like your reference
        let windowWidth: CGFloat = 600
        let windowHeight: CGFloat = 40 // Increased height for bigger text
        let windowFrame = NSRect(
            x: screenFrame.width - windowWidth - 20, // 20px from right edge
            y: screenFrame.height - windowHeight - 80, // 80px from top (more space below menu bar)
            width: windowWidth,
            height: windowHeight
        )
        
        overlayWindow = NSWindow(
            contentRect: windowFrame,
            styleMask: [.borderless],
            backing: .buffered,
            defer: false
        )
        
        // Configure window for overlay behavior
        overlayWindow?.level = .floating
        overlayWindow?.isOpaque = false
        overlayWindow?.backgroundColor = NSColor.clear
        overlayWindow?.hasShadow = false
        overlayWindow?.ignoresMouseEvents = true
        overlayWindow?.collectionBehavior = [.canJoinAllSpaces, .fullScreenAuxiliary]
        
        // Create content view
        let contentView = OverlayView()
        overlayWindow?.contentView = NSHostingView(rootView: contentView)
        
        // Initially hidden
        overlayWindow?.orderOut(nil)
    }
    
    private func updateOverlay(with trackInfo: TrackInfo?) {
        guard let window = overlayWindow,
              let hostingView = window.contentView as? NSHostingView<OverlayView> else { return }
        
        if let trackInfo = trackInfo {
            // Update the view with new track info
            let newView = OverlayView(trackInfo: trackInfo)
            hostingView.rootView = newView
            
            // Show window and animate
            window.orderFront(nil)
            showWithAnimation()
        } else {
            // Hide window
            hideWithAnimation()
        }
    }
    
    private func showWithAnimation() {
        guard let window = overlayWindow else { return }
        
        // Position on the current active screen
        let mouseLocation = NSEvent.mouseLocation
        let activeScreen = NSScreen.screens.first { screen in
            NSMouseInRect(mouseLocation, screen.frame, false)
        } ?? NSScreen.main
        
        if let screenFrame = activeScreen?.frame {
            let windowWidth: CGFloat = 600
            let windowHeight: CGFloat = 40
            let newFrame = NSRect(
                x: screenFrame.width - windowWidth - 20 + screenFrame.origin.x,
                y: screenFrame.height - windowHeight - 80 + screenFrame.origin.y,
                width: windowWidth,
                height: windowHeight
            )
            window.setFrame(newFrame, display: false)
        }
        
        // Slide in animation from the right
        let originalFrame = window.frame
        var startFrame = originalFrame
        startFrame.origin.x += 100 // Start 100px to the right (off-screen)
        
        window.setFrame(startFrame, display: false)
        window.orderFront(nil)
        
        NSAnimationContext.runAnimationGroup { context in
            context.duration = 0.4
            context.timingFunction = CAMediaTimingFunction(name: .easeOut)
            window.animator().setFrame(originalFrame, display: true)
        }
        
        // Auto-hide after 4 seconds
        DispatchQueue.main.asyncAfter(deadline: .now() + 4.0) {
            self.hideWithAnimation()
        }
    }
    
    private func hideWithAnimation() {
        guard let window = overlayWindow else { return }
        
        let originalFrame = window.frame
        var endFrame = originalFrame
        endFrame.origin.x += 100 // Slide out to the right (off-screen)
        
        NSAnimationContext.runAnimationGroup { context in
            context.duration = 0.4
            context.timingFunction = CAMediaTimingFunction(name: .easeIn)
            window.animator().setFrame(endFrame, display: true)
        } completionHandler: {
            window.orderOut(nil)
            window.setFrame(originalFrame, display: false) // Reset position
        }
    }
}