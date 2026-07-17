# 🎮 GameBarMixr - Audio & Bluetooth Mixer Widget for Xbox Game Bar

**GameBarMixr** is a native widget designed for **Xbox Game Bar (Windows 10/11)** that solves the hassle of losing game focus when switching Bluetooth headphones or adjusting audio output volumes during gameplay.

With GameBarMixr, simply press **`Win + G`** over any game (windowed or exclusive fullscreen) to seamlessly control audio endpoints and Bluetooth devices with a single click.

---

## ✨ Key Features

- 🎧 **Quick Bluetooth Headset Switcher**: Connect and disconnect Bluetooth headphones, earbuds (AirPods, Galaxy Buds, Sony, etc.), and soundbars without opening Windows Settings.
- 🎛️ **Audio Endpoint & App Mixer**: Instantly switch the default audio playback device (Speakers / Headset) and control master volume or individual app levels (Cyberpunk, Spotify, Discord, etc.).
- ⚡ **No Loss of Game Focus**: Runs as a lightweight native Xbox Game Bar overlay, keeping your game active and focused.
- 📌 **Pin Widget Support**: Pin the overlay to any screen corner while gaming.
- 🏪 **Microsoft Store Ready**: Includes full MSIX packaging structure, manifest capabilities, and asset setup ready for Microsoft Partner Center publication.

---

## ⚡ Quick 1-Click Installation on Windows

To install and test the widget on your Windows machine:

1. Clone or download this repository:
   ```bash
   git clone https://github.com/your-username/GameBarMixr.git
   cd GameBarMixr
   ```
2. Open **PowerShell** on Windows and run the quick installer script:
   ```powershell
   .\Scripts\install_widget.ps1
   ```
3. Press on your keyboard: **`Win + G`**
4. Open the Widget Menu (top-left bar) and click on **Audio & Bluetooth Mixer (GameBarMixr)**!

---

## 🏪 Building the Package for Microsoft Store

To publish **GameBarMixr** to the *Microsoft Partner Center*:

1. Run the MSIX build script:
   ```powershell
   .\Scripts\build_msix.ps1 -Configuration Release -Platform x64
   ```
2. The script will generate the `.msixbundle` file inside `GameBarMixr\bin\MSIX\`.
3. Go to [Microsoft Partner Center](https://partner.microsoft.com/dashboard), create a new app submission, upload the `.msixbundle`, and set the category to **Games > Utilities**.

---

## 🌐 Interactive Web Preview

To test and preview the widget UI and interactions directly in your browser:

1. Open [`web_preview/index.html`](file:///Users/joaovictorbatista/GameBarMixr/web_preview/index.html) in any browser (Edge, Chrome, Firefox).
2. Try switching between the "Audio" and "Bluetooth" tabs, adjusting volume sliders, and toggling mock Bluetooth devices.

---

## 🛠️ Project Architecture

- **Language & Framework**: C# / WinUI 3 / UWP (.NET 8)
- **SDK**: `Microsoft.Gaming.XboxGameBar.SDK` (v1.6.2)
- **Native APIs**: `Windows.Devices.Bluetooth`, `Windows.Devices.Enumeration`, Windows CoreAudio / MMDeviceApi
- **Manifest**: `Package.appxmanifest` configured with the `windows.xboxGameBarWidget` extension category.

---

## 📜 License

Distributed under the MIT License. See `LICENSE` for details.
