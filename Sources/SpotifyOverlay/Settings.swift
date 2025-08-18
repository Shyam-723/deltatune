import Foundation
import SwiftUI

enum OverlayPosition: String, CaseIterable {
    case topLeft = "Top Left"
    case topRight = "Top Right"
    case topCenter = "Top Center"
    case bottomLeft = "Bottom Left"
    case bottomRight = "Bottom Right"
    case bottomCenter = "Bottom Center"
    case centerLeft = "Center Left"
    case centerRight = "Center Right"
    case center = "Center"
    case random = "Random"
}

class Settings: ObservableObject {
    static let shared = Settings()
    
    @Published var overlayPosition: OverlayPosition {
        didSet {
            UserDefaults.standard.set(overlayPosition.rawValue, forKey: "overlayPosition")
        }
    }
    
    @Published var customFontPath: String? {
        didSet {
            UserDefaults.standard.set(customFontPath, forKey: "customFontPath")
        }
    }
    
    @Published var fontSize: Double {
        didSet {
            UserDefaults.standard.set(fontSize, forKey: "fontSize")
        }
    }
    
    @Published var displayDuration: Double {
        didSet {
            UserDefaults.standard.set(displayDuration, forKey: "displayDuration")
        }
    }
    
    private init() {
        self.overlayPosition = OverlayPosition(rawValue: UserDefaults.standard.string(forKey: "overlayPosition") ?? "Top Right") ?? .topRight
        self.customFontPath = UserDefaults.standard.string(forKey: "customFontPath")
        self.fontSize = UserDefaults.standard.double(forKey: "fontSize") != 0 ? UserDefaults.standard.double(forKey: "fontSize") : 18.0
        self.displayDuration = UserDefaults.standard.double(forKey: "displayDuration") != 0 ? UserDefaults.standard.double(forKey: "displayDuration") : 4.0
    }
    
    func getWindowFrame(for screenFrame: NSRect, windowSize: NSSize) -> NSRect {
        let margin: CGFloat = 20
        
        let actualPosition = overlayPosition == .random ? getRandomPosition() : overlayPosition
        
        switch actualPosition {
        case .topLeft:
            return NSRect(
                x: screenFrame.origin.x + margin,
                y: screenFrame.maxY - windowSize.height - margin,
                width: windowSize.width,
                height: windowSize.height
            )
        case .topRight:
            return NSRect(
                x: screenFrame.maxX - windowSize.width - margin,
                y: screenFrame.maxY - windowSize.height - margin,
                width: windowSize.width,
                height: windowSize.height
            )
        case .topCenter:
            return NSRect(
                x: screenFrame.midX - windowSize.width / 2,
                y: screenFrame.maxY - windowSize.height - margin,
                width: windowSize.width,
                height: windowSize.height
            )
        case .bottomLeft:
            return NSRect(
                x: screenFrame.origin.x + margin,
                y: screenFrame.origin.y + margin,
                width: windowSize.width,
                height: windowSize.height
            )
        case .bottomRight:
            return NSRect(
                x: screenFrame.maxX - windowSize.width - margin,
                y: screenFrame.origin.y + margin,
                width: windowSize.width,
                height: windowSize.height
            )
        case .bottomCenter:
            return NSRect(
                x: screenFrame.midX - windowSize.width / 2,
                y: screenFrame.origin.y + margin,
                width: windowSize.width,
                height: windowSize.height
            )
        case .centerLeft:
            return NSRect(
                x: screenFrame.origin.x + margin,
                y: screenFrame.midY - windowSize.height / 2,
                width: windowSize.width,
                height: windowSize.height
            )
        case .centerRight:
            return NSRect(
                x: screenFrame.maxX - windowSize.width - margin,
                y: screenFrame.midY - windowSize.height / 2,
                width: windowSize.width,
                height: windowSize.height
            )
        case .center:
            return NSRect(
                x: screenFrame.midX - windowSize.width / 2,
                y: screenFrame.midY - windowSize.height / 2,
                width: windowSize.width,
                height: windowSize.height
            )
        case .random:
            // This case should never be reached due to getRandomPosition() above
            return getWindowFrame(for: screenFrame, windowSize: windowSize)
        }
    }
    
    private func getRandomPosition() -> OverlayPosition {
        let nonRandomPositions = OverlayPosition.allCases.filter { $0 != .random }
        return nonRandomPositions.randomElement() ?? .topRight
    }
}