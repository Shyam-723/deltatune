import SwiftUI
import AppKit
import UniformTypeIdentifiers

struct SettingsView: View {
    @ObservedObject var settings = Settings.shared
    @State private var showingFontPicker = false
    @State private var previewFont: Font?
    
    var body: some View {
        VStack(alignment: .leading, spacing: 20) {
            Text("Spotify Overlay Settings")
                .font(.title)
                .fontWeight(.bold)
            
            Divider()
            
            // Position Settings
            VStack(alignment: .leading, spacing: 10) {
                Text("Overlay Position")
                    .font(.headline)
                
                Picker("Position", selection: $settings.overlayPosition) {
                    ForEach(OverlayPosition.allCases, id: \.self) { position in
                        Text(position.rawValue).tag(position)
                    }
                }
                .pickerStyle(MenuPickerStyle())
                .frame(maxWidth: 200)
            }
            
            Divider()
            
            // Font Settings
            VStack(alignment: .leading, spacing: 10) {
                Text("Font Settings")
                    .font(.headline)
                
                HStack {
                    Text("Font Size:")
                    Slider(value: $settings.fontSize, in: 12...36, step: 1)
                    Text("\(Int(settings.fontSize))pt")
                        .frame(width: 40)
                }
                
                HStack {
                    Text("Custom Font:")
                    if let fontPath = settings.customFontPath {
                        Text(URL(fileURLWithPath: fontPath).lastPathComponent)
                            .foregroundColor(.secondary)
                        Button("Remove") {
                            settings.customFontPath = nil
                        }
                        .buttonStyle(.bordered)
                    } else {
                        Text("Using default font")
                            .foregroundColor(.secondary)
                    }
                    
                    Spacer()
                    
                    Button("Choose Font...") {
                        showFontPicker()
                    }
                    .buttonStyle(.bordered)
                }
            }
            
            Divider()
            
            // Display Settings
            VStack(alignment: .leading, spacing: 10) {
                Text("Display Settings")
                    .font(.headline)
                
                HStack {
                    Text("Display Duration:")
                    Slider(value: $settings.displayDuration, in: 1...10, step: 0.5)
                    Text("\(settings.displayDuration, specifier: "%.1f")s")
                        .frame(width: 40)
                }
            }
            
            Divider()
            
            // Preview
            VStack(alignment: .leading, spacing: 10) {
                Text("Preview")
                    .font(.headline)
                
                HStack {
                    Text("♪ Artist - Song Title")
                        .font(previewFont ?? Font.system(size: settings.fontSize, weight: .bold, design: .monospaced))
                        .foregroundColor(Color(red: 0.0, green: 1.0, blue: 0.8))
                        .padding(.horizontal, 12)
                        .padding(.vertical, 6)
                        .background(
                            Rectangle()
                                .fill(Color.black.opacity(0.6))
                        )
                    
                    Spacer()
                }
            }
            
            Spacer()
            
            HStack {
                Spacer()
                Button("Close") {
                    NSApplication.shared.keyWindow?.close()
                }
                .buttonStyle(.borderedProminent)
            }
        }
        .padding(20)
        .frame(width: 500, height: 400)
        .onAppear {
            updatePreviewFont()
        }
        .onChange(of: settings.customFontPath) { _ in
            updatePreviewFont()
        }
        .onChange(of: settings.fontSize) { _ in
            updatePreviewFont()
        }
    }
    
    private func showFontPicker() {
        let panel = NSOpenPanel()
        panel.allowedContentTypes = [UTType.font, UTType("public.truetype-ttf-font")!, UTType("public.opentype-font")!]
        panel.allowsMultipleSelection = false
        panel.canChooseDirectories = false
        panel.canChooseFiles = true
        panel.title = "Choose a Font File"
        panel.message = "Select a .ttf, .otf, or other font file"
        
        if panel.runModal() == .OK {
            if let url = panel.url {
                settings.customFontPath = url.path
            }
        }
    }
    
    private func updatePreviewFont() {
        var fontURL: URL?
        
        // First try user's custom font
        if let customPath = settings.customFontPath {
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
            // Register the font if it's not already registered
            CTFontManagerRegisterFontsForURL(fontURL as CFURL, .process, nil)
            
            // Try to get the actual font family name from the TTF file
            if let fontDescriptors = CTFontManagerCreateFontDescriptorsFromURL(fontURL as CFURL) as? [CTFontDescriptor],
               let fontDescriptor = fontDescriptors.first,
               let fontName = CTFontDescriptorCopyAttribute(fontDescriptor, kCTFontFamilyNameAttribute) as? String {
                previewFont = Font.custom(fontName, size: settings.fontSize)
            } else {
                // Try common variations of the font name
                let fontName = fontURL.deletingPathExtension().lastPathComponent
                previewFont = Font.custom(fontName, size: settings.fontSize)
            }
        } else {
            previewFont = Font.system(size: settings.fontSize, weight: .bold, design: .monospaced)
        }
    }
}

struct SettingsView_Previews: PreviewProvider {
    static var previews: some View {
        SettingsView()
    }
}