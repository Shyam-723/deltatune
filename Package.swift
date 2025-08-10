// swift-tools-version: 5.9
import PackageDescription

let package = Package(
    name: "SpotifyOverlay",
    platforms: [
        .macOS(.v13)
    ],
    products: [
        .executable(
            name: "SpotifyOverlay",
            targets: ["SpotifyOverlay"]
        ),
    ],
    dependencies: [
        // Add any dependencies here if needed
    ],
    targets: [
        .executableTarget(
            name: "SpotifyOverlay",
            dependencies: []
        ),
    ]
)