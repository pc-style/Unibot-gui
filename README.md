# Unibot - C# Edition

**⭐ Modern C# implementation of the advanced gaming assistant tool**

---

<img src=https://i.imgur.com/c55L14T.png alt="Unibot logo" width="250"> 

## About

This is a complete rewrite of Unibot in C# with a modern WPF interface. The application provides the same powerful features as the original Python version but with a sleek, professional dark-themed GUI.

### Key Features

- **🎯 Aim Assist** - Advanced color-based target detection with smooth aim interpolation
- **🔫 Triggerbot** - Automatic shooting when crosshair is on target
- **🔥 Rapid Fire** - High-speed clicking for semi-automatic weapons  
- **🎮 Recoil Control** - Intelligent recoil compensation with multiple modes
- **📊 Real-time Preview** - Live visual feedback of target detection
- **⚙️ Modern Interface** - Compact tabbed interface with real-time configuration

### Interface Design

The application features a sleek, modern, and professional-looking interface with:
- **Dark Theme** - Dark gray background with vibrant blue accents
- **Tabbed Organization** - Settings organized into logical categories:
  - Screen & Detection - Color ranges, FOV, capture settings
  - Aim Settings - Speed, smoothing, targeting preferences  
  - Recoil & Trigger - Recoil compensation and triggerbot configuration
  - Communication - Hardware connection options
  - Debug & Preview - Live target detection preview
- **Compact Design** - Scaled to fit normal screen resolutions
- **Real-time Status** - Live indicators for aim, trigger, and recoil states

### Communication Methods

Supports multiple input methods just like the original:
- **WinAPI** - Direct Windows mouse events (default)
- **Interception Driver** - Low-level input interception
- **Serial (COM Port)** - Arduino Leonardo/Pi Pico hardware
- **Socket (Network)** - Wi-Fi/Ethernet connected devices

### Advanced Features

- **HSV Color Detection** - Precise target identification using OpenCV
- **Sub-pixel Accuracy** - Smooth mouse movement with remainder tracking
- **Rate Limiting** - Configurable click speeds and delays
- **Randomized Timing** - Human-like input patterns
- **Hot-reload Configuration** - F1 to reload settings without restart
- **Multi-threaded Processing** - Smooth 60+ FPS operation

## Building from Source

To build the project from source, you'll need the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) installed.

### Manual Build

1. **Clone the repository:**
   ```sh
   git clone https://github.com/your-repo/Unibot-gui.git
   cd Unibot-gui
   ```
2. **Restore dependencies and build the project:**
   ```sh
   dotnet build --configuration Release
   ```
3. **Run the application:**
   The executable will be located in `Unibot/bin/Release/net8.0-windows/Unibot.exe`.

### Automated Builds (GitHub Actions)

This repository includes automated build workflows:

#### 🔄 **Continuous Integration** 
- **Triggers**: Push to `main`/`develop` branches, Pull Requests to `main`
- **Process**: Automatically builds, tests, and uploads artifacts on every code change
- **Artifacts**: Build outputs available for download for 30 days
- **Status**: ![Build Status](https://github.com/your-repo/Unibot-gui/workflows/Build%20Unibot/badge.svg)

#### 🚀 **Automated Releases**
- **Triggers**: Version tags (e.g., `v1.0.0`, `v1.0.1-beta`)
- **Process**: Creates distributable packages automatically
- **Outputs**:
  - `Unibot-v1.0.0-win-x64-standalone.zip` - Self-contained (no .NET required)
  - `Unibot-v1.0.0-framework-dependent.zip` - Requires .NET 8 Runtime
- **Features**: Automatic changelog generation, GitHub releases

#### 📦 **Creating a Release**

To create a new release:

1. **Tag your commit:**
   ```sh
   git tag v1.0.0
   git push origin v1.0.0
   ```

2. **Automatic Process:**
   - GitHub Actions builds both standalone and framework-dependent versions
   - Creates a GitHub release with download links
   - Generates changelog with installation instructions
   - Uploads artifacts for long-term storage

#### 🛠️ **Build Configurations**

| Build Type | Description | Requirements | Size |
|------------|-------------|--------------|------|
| **Standalone** | Single executable with .NET embedded | None | ~150MB |
| **Framework-Dependent** | Requires .NET 8 Desktop Runtime | .NET 8 | ~15MB |

#### 📋 **Workflow Files**

- `.github/workflows/build.yml` - Continuous integration workflow
- `.github/workflows/release.yml` - Release automation workflow

Both workflows use Windows runners to ensure compatibility with WPF applications.

## Technical Implementation

Built with modern .NET 8 and WPF, utilizing:
- **OpenCV Sharp** - Advanced computer vision processing
- **Modern WPF UI** - Contemporary interface components
- **Async/Await** - Non-blocking operations for smooth performance
- **MVVM Pattern** - Clean separation of UI and logic
- **Dependency Injection** - Modular, testable architecture

## Requirements

- Windows 10/11
- .NET 8 Runtime
- Screen resolution: 800x600 minimum (optimized for 1920x1080)

## Installation

1. Download the latest release
2. Extract to desired folder
3. Run `Unibot.exe`
4. Configure your settings and start

## Usage

1. **Configure Detection** - Set HSV color ranges for your targets
2. **Adjust Aim Settings** - Tune speed, smoothing, and targeting
3. **Set Communication** - Choose input method (WinAPI recommended)
4. **Test with Preview** - Use debug preview to verify detection
5. **Start Unibot** - Click Start when ready

### Default Hotkeys

- **F1** - Reload configuration
- **F2** - Toggle aim assist  
- **F3** - Toggle recoil control
- **F4** - Exit application
- **Mouse 4** - Triggerbot activation
- **Mouse 5** - Rapid fire
- **Mouse 1/2** - Aim activation keys

## Performance

Optimized for high performance with:
- Multi-threaded image processing
- Efficient memory management  
- Configurable frame rates (30-240 FPS)
- Minimal CPU overhead when idle

## Disclaimer

This software is intended for educational and testing purposes only. The authors do not condone cheating in competitive gaming environments. Use responsibly and in accordance with game terms of service.

## License

```
Unibot C# Edition
Copyright (C) 2025

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.
```

---

**Built with ❤️ using modern C# and WPF**
