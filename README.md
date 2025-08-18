# Spotify Overlay

A retro-styled floating overlay that displays your currently playing Spotify track on macOS.

## Features

- 🎵 Real-time Spotify track display
- 🎨 Retro terminal aesthetic with cyan-green text
- 📍 Customizable overlay position (9 different positions)
- 🔤 Custom font support - use your own TTF/OTF fonts
- ⏱️ Adjustable display duration
- 🖥️ Multi-monitor support
- 🎛️ Easy-to-use settings interface

## Installation

1. Clone this repository
2. Run `swift build` to compile
3. Run `./.build/debug/SpotifyOverlay` to start

## Usage

### Basic Usage
- The overlay automatically appears when you start playing a song in Spotify
- It shows for a few seconds then disappears
- The overlay follows your mouse cursor to the active screen

### Settings
Access settings through the menu bar icon (music note):

#### Position Settings
Choose from 10 different overlay positions:
- Top Left, Top Center, Top Right
- Center Left, Center, Center Right  
- Bottom Left, Bottom Center, Bottom Right
- **Random** - Picks a different position each time a song plays

#### Font Settings
- **Font Size**: Adjust from 12pt to 36pt
- **Custom Font**: Upload your own TTF or OTF font files
  - Click "Choose Font..." to browse for font files
  - Supports .ttf, .otf, and other standard font formats
  - Falls back to system font for unsupported characters

#### Display Settings
- **Display Duration**: How long the overlay stays visible (1-10 seconds)

### Custom Fonts
To use your own font:
1. Open Settings from the menu bar
2. Click "Choose Font..." in the Font Settings section
3. Select your .ttf or .otf font file
4. The overlay will immediately use your new font

The app includes a default retro font, but you can replace it with any font you prefer!

## Requirements

- macOS 13.0 or later
- Spotify app installed and running
- Swift 5.9 or later (for building from source)

## How It Works

The app uses:
- **ScriptingBridge** to communicate with Spotify
- **SwiftUI** for the modern UI and settings interface
- **AppKit** for window management and system integration
- **Core Text** for custom font loading
- **UserDefaults** for persistent settings storage

## Troubleshooting

- **Overlay not showing**: Make sure Spotify is running and playing music
- **Custom font not working**: Ensure the font file is valid and accessible
- **Position not updating**: Try restarting the app after changing position settings