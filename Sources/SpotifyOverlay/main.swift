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
    var settingsWindow: NSWindow?
    var statusItem: NSStatusItem?
    
    func applicationDidFinishLaunching(_ notification: Notification) {
        // Hide dock icon but keep menu bar access
        NSApp.setActivationPolicy(.accessory)
        
        // Create status bar item
        createStatusBarItem()
        
        // Create overlay window
        createOverlayWindow()
        
        // Start monitoring Spotify
        spotifyMonitor = SpotifyMonitor { [weak self] trackInfo in
            DispatchQueue.main.async {
                self?.updateOverlay(with: trackInfo)
            }
        }
        spotifyMonitor?.startMonitoring()
    }
    
    private func createStatusBarItem() {
        statusItem = NSStatusBar.system.statusItem(withLength: NSStatusItem.squareLength)
        
        if let button = statusItem?.button {
            button.image = NSImage(systemSymbolName: "music.note", accessibilityDescription: "Spotify Overlay")
            button.action = #selector(statusBarButtonClicked)
            button.target = self
        }
        
        let menu = NSMenu()
        menu.addItem(NSMenuItem(title: "Settings...", action: #selector(openSettings), keyEquivalent: ","))
        menu.addItem(NSMenuItem.separator())
        menu.addItem(NSMenuItem(title: "Quit", action: #selector(NSApplication.terminate(_:)), keyEquivalent: "q"))
        
        statusItem?.menu = menu
    }
    
    @objc private func statusBarButtonClicked() {
        // This will show the menu
    }
    
    @objc private func openSettings() {
        if settingsWindow == nil {
            let settingsView = SettingsView()
            let hostingView = NSHostingView(rootView: settingsView)
            
            settingsWindow = NSWindow(
                contentRect: NSRect(x: 0, y: 0, width: 500, height: 400),
                styleMask: [.titled, .closable],
                backing: .buffered,
                defer: false
            )
            
            settingsWindow?.title = "Spotify Overlay Settings"
            settingsWindow?.contentView = hostingView
            settingsWindow?.center()
            settingsWindow?.isReleasedWhenClosed = false
        }
        
        settingsWindow?.makeKeyAndOrderFront(nil)
        NSApp.activate(ignoringOtherApps: true)
    }
    
    private func createOverlayWindow() {
        // Use the screen with the mouse cursor (active screen)
        let mouseLocation = NSEvent.mouseLocation
        let activeScreen = NSScreen.screens.first { screen in
            NSMouseInRect(mouseLocation, screen.frame, false)
        } ?? NSScreen.main
        
        let screenFrame = activeScreen?.frame ?? NSRect(x: 0, y: 0, width: 1920, height: 1080)
        
        // Create window frame using settings
        let windowSize = NSSize(width: 600, height: 40)
        let windowFrame = Settings.shared.getWindowFrame(for: screenFrame, windowSize: windowSize)
        
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
        
        // Position on the current active screen using settings
        let mouseLocation = NSEvent.mouseLocation
        let activeScreen = NSScreen.screens.first { screen in
            NSMouseInRect(mouseLocation, screen.frame, false)
        } ?? NSScreen.main
        
        if let screenFrame = activeScreen?.frame {
            let windowSize = NSSize(width: 600, height: 40)
            let newFrame = Settings.shared.getWindowFrame(for: screenFrame, windowSize: windowSize)
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
        
        // Auto-hide after user-defined duration
        DispatchQueue.main.asyncAfter(deadline: .now() + Settings.shared.displayDuration) {
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