# Windows System Tray Voice Input Method (WinUI 3)

A refined Windows 10/11 system tray voice input method application, replicating the fluid, lightweight, and eye-catching user experience of macOS's built-in dictation services.

## Core Features

-   **Global Voice Dictation**: Press and hold the **Right Alt** (AltGr) key to talk. Release to immediately transcribe and automatically inject the text into any active cursor/input field.
-   **Acrylic HUD Overlays**: An elegant capsule HUD appears above the taskbar when dictating, complete with an animated visualizer responsive to voice levels. No footprint is left when idle.
-   **Lyric Recognition (LRC)**: Upload/drag any `.wav` or `.mp3` audio file (up to 10MB) to generate timestamped `.lrc` lyrics with real-time progress indicators.
-   **Real-time Subtitles**: Run continuous streaming transcriptions with real-time colored timestamp displays, full history logging, and one-click clipboard copying.
-   **Secure Storage (DPAPI)**: API credentials are fully encrypted using Windows Data Protection API (DPAPI) and stored safely within the registry (`HKEY_CURRENT_USER\Software\VoiceInput`).
-   **Mica Backgrounds**: Gorgeous Windows 11 Mica backdrop integration for a native desktop application aesthetic.

---

## Technical Specifications

| Parameter | Value / Detail |
| :--- | :--- |
| **Framework** | WinUI 3 (Windows App SDK 1.4+) |
| **Runtime** | .NET 8.0 (Windows Desktop SDK) |
| **ASR API** | MiMo-V2.5-ASR API |
| **Audio Capture** | NAudio (16kHz, Mono, 16-bit PCM) |
| **Keyboard Listening** | Global Low-level Hook (`WH_KEYBOARD_LL`) |
| **Text Injection** | Clipboard simulation via simulated `Ctrl+V` key events |
| **DPI Awareness** | PerMonitorV2 High-DPI scaling support |

---

## System Requirements

-   **Operating System**: Windows 10 (Build 1809 / 17763 or later) or Windows 11.
-   **SDK/Runtime**: [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0).
-   **Device**: Active recording microphone.

---

## Installation & Build Instructions

### Prerequisites
1. Install Visual Studio 2022 with the **.NET Desktop Development** and **Windows Application Development** workloads.
2. Install the **Windows App SDK** component.

### Build via dotnet CLI
```bash
# Clone the repository
git clone https://github.com/your-username/VoiceInput.git
cd VoiceInput

# Restore NuGet packages
dotnet restore

# Build the applet in Release mode
dotnet build -c Release
```

---

## Architecture & Class Guide

-   **`Program.cs`**: Handles Single-Instance execution via local system `Mutex` flags and instantiates the main application context.
-   **`KeyboardHook.cs`**: Low-level hook using Win32 API `SetWindowsHookEx` to intercept and suppress the Right Alt modifier key natively.
-   **`SecureStorage.cs`**: Integrates DPAPI `ProtectedData.Protect` / `Unprotect` to encrypt API keys before storing them under `HKCU\Software\VoiceInput`.
-   **`ClipboardInjector.cs`**: Manages the clipboard state safely, injects unicode characters into active screens, and restores original user clipboards seamlessly.
-   **`DpiHelper.cs`**: Accesses `GetDpiForWindow` and `GetDpiForMonitor` to perfectly calculate screen scale values across multi-monitor setups.
-   **`TrayIcon.cs`**: Complete custom wrapper around Win32 `Shell_NotifyIcon` to control taskbar status without introducing obsolete WinForms dependencies.

---

## Security & Privacy

This application values security and privacy.
- **Local Decryption Only**: Your MiMo API Keys are encrypted locally using the Windows user credentials system (DPAPI). No third-party servers ever receive your raw API key.
- **Direct ASR Connectivity**: Transcription requests are transmitted directly from your machine to the official MiMo servers over secure HTTPS channels. No analytical tracking or middleware tracking is performed.

---

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
