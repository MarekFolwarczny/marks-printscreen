# Screen Snips

A lightweight Windows 10/11 tray app that opens a selected screen area in Paint or copies it to the clipboard. It supports multiple monitors and English and Czech UI text. [Česká dokumentace](README.cs.md).

**[Download Screen Snips for Windows (.exe)](https://github.com/MarekFolwarczny/marks-printscreen/releases/latest/download/PrintScreen.exe)**

## Download and run

Download `PrintScreen.exe` from the link above and run it on Windows. No installer is required. The EXE is distributed through [Releases](https://github.com/MarekFolwarczny/marks-printscreen/releases); the repository contains the source code. To build from source, run `Build-and-run.cmd` on Windows. It compiles `Program.cs` and `SettingsDialog.cs` with the .NET Framework compiler available on the system, embedding `Brand.ico` and `PrintScreen.manifest`. No additional packages are needed. Exit a running instance from the tray menu before rebuilding so the EXE can be replaced. The original `Sestavit-a-spustit.cmd` name remains as an alias.

On first launch, the app enables startup **when the current user signs in**. Right-click its tray icon and toggle **Start when I sign in** to change this. It needs no administrator privileges.

## Use

| Shortcut | Action |
| --- | --- |
| **Ctrl + W** | Drag with the left mouse button to select an area. The snip is saved as PNG and opened in Paint. |
| **Ctrl + Shift + W** | Select an area. The image is copied to the clipboard without creating a file. |
| **Esc** while selecting | Cancel the snip. |

Shortcuts are system-wide. The default **Ctrl + W** may take over Close Tab in other apps; change it if this interferes with your workflow.

### Change shortcuts and language

Right-click the tray icon and open **Shortcut settings…**. For each action, check Ctrl, Shift or Alt and **choose a key from the drop-down list**. For **Ctrl + R**, leave Ctrl checked and select **R**. Pressing the combination itself does not enter it. F1–F24 can also be used without a modifier. The two actions need different shortcuts. Click **Save**; the app checks whether Windows can register both combinations and restores the previous ones if one is unavailable.

English is the default for new users. Choose **Language → Čeština** in the tray menu to switch to Czech, or **Language → English** to switch back. The choice is saved for future launches.

## Images and settings

- Paint snips are saved under `%TEMP%\PrintScreenAplikace\yyyy-MM-dd_HH-mm-ss-fff.png`, named with the local capture time. A numeric suffix prevents a duplicate name. This is a temporary location: use **Save as** in Paint to keep edits in a folder of your choice.
- Clipboard snips create no file. Copying something else may replace the clipboard image.
- Shortcuts, language and startup preference are stored in `HKEY_CURRENT_USER\Software\PrintScreenAplikace`. Startup uses the `PrintScreenAplikace` value in `HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run`, pointing to the current EXE. If you move the EXE, launch it from its new location to update that path.

The app does not send screenshots over the network. It captures the screen only after a shortcut is pressed.

## Troubleshooting

- **A shortcut does nothing:** Check that the app is running in the tray. Try another combination if another app has reserved it.
- **Pressing Ctrl + R changes nothing in the list:** Check Ctrl and choose R from the drop-down list.
- **The old version still runs after a rebuild:** Exit it from the tray menu, run the build script again and check its output.
- **The snip does not open in Paint:** The app reports an error if Windows cannot start `mspaint.exe`. Check that Paint is available.

## Development

[DEVELOPERS.md](DEVELOPERS.md) describes the app structure, capture flow and stored settings. The source is released under the [MIT license](LICENSE). `Brand.ico` was derived from the supplied blue brand symbol.
