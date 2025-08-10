#!/bin/bash

echo "Building Spotify Overlay..."

# Build the Swift package
swift build -c release

if [ $? -eq 0 ]; then
    echo "Build successful!"
    echo "Running the overlay..."
    ./.build/release/SpotifyOverlay
else
    echo "Build failed!"
    exit 1
fi