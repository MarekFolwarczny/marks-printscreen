# Developer notes

[Česká verze](DEVELOPERS.md)

## Files and build

| File | Purpose |
| --- | --- |
| `Program.cs` | Entry point, tray icon, hotkey registration, area selection and output. |
| `SettingsDialog.cs` | Picker for the two shortcuts and their validation. |
| `Brand.ico` | EXE, settings dialog and tray icon. |
| `PrintScreen.manifest` | Declares process DPI awareness. |
| `Sestavit-a-spustit.cmd` | Finds the 64-bit or 32-bit .NET Framework compiler, builds a WinForms EXE and starts it. |

This is a standalone .NET Framework WinForms app with no project file or external libraries. The build script runs on Windows. Its compiler command uses `/target:winexe`, `/optimize+`, references to `System.dll`, `System.Windows.Forms.dll` and `System.Drawing.dll`, and embeds the manifest and icon. Exit a running instance before rebuilding.

## Lifecycle

1. `Program.Main` initializes WinForms and obtains the `Local\PrintScreenAplikace` mutex, allowing one instance per user session.
2. `TrayApp` reads per-user settings, creates the tray icon and menu, configures sign-in startup and registers both shortcuts with `RegisterHotKey`. It does not poll the keyboard or capture the screen while idle.
3. The hidden `HotkeyWindow` receives `WM_HOTKEY` and passes the action ID to `OnHotkey`. The `capturing` flag prevents overlapping selections.
4. On exit, the app unregisters the shortcuts and disposes the icon, menu and window. Sign-in startup remains configured until the user switches it off.

## Capture flow

`OnHotkey` reads `SystemInformation.VirtualScreen`, creates a bitmap of the entire virtual desktop and copies its pixels before showing the selection window. `SelectionForm` displays this snapshot, dimmed, across the monitors. Dragging with the left mouse button selects a rectangle. `Esc` or a zero-area selection produces no output. The chosen area is cloned from the original bitmap.

- **Paint:** Save a PNG under `%TEMP%\PrintScreenAplikace`, name it `yyyy-MM-dd_HH-mm-ss-fff.png` using local time and launch `mspaint.exe` with its path. On a filename collision, append `-2`, `-3`, etc. The app does not delete temporary files itself.
- **Clipboard:** Pass the bitmap to the Windows clipboard with `Clipboard.SetDataObject`, allowing three retries at 100ms intervals. No file is created.

Selection runs synchronously on the WinForms thread. The bitmap of the entire virtual desktop stays in memory during selection, so memory use increases with the number and resolution of monitors. Capture, save and Paint launch errors are shown to the user.

## Hotkeys and settings

The defaults are **Ctrl + W** for Paint and **Ctrl + Shift + W** for the clipboard. The dialog offers A–Z, 0–9, F1–F24, Space, Insert, Delete, Home, End, Page Up, Page Down and arrow keys. Regular keys need at least one of Ctrl, Shift or Alt; F1–F24 can be used alone. `ValidKey` also rejects bare modifiers, `Esc`, `Enter` and `Tab`. The two actions need distinct shortcuts.

Opening the dialog temporarily unregisters the old hotkeys. On confirmation, the app attempts to register the new pair; if the second registration fails, it unregisters the first. Cancellation or failure restores the previous pair. Values are saved only after successful registration. Windows or another app may reserve a combination.

| Per-user registry key | Value | Meaning |
| --- | --- | --- |
| `Software\PrintScreenAplikace` | `PaintHotkey`, `ClipboardHotkey` | Numeric `System.Windows.Forms.Keys` values. |
| `Software\PrintScreenAplikace` | `Language` | `cs` or `en`; Czech is the default. |
| `Software\PrintScreenAplikace` | `StartAtLogin` | 1 or 0; 1 is the default. |
| `Software\Microsoft\Windows\CurrentVersion\Run` | `PrintScreenAplikace` | Quoted EXE path when sign-in startup is enabled. |

UI strings are selected with `T(czech, englishText)` in `TrayApp` and `SettingsDialog`. When adding a user-facing string, supply both languages and keep the two README files aligned.
