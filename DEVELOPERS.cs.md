# Technické poznámky

[English version](DEVELOPERS.md)

## Soubory a sestavení

| Soubor | Úloha |
| --- | --- |
| `Program.cs` | Vstupní bod, ikona u hodin, registrace zkratek, výběr oblasti a předání výsledku. |
| `SettingsDialog.cs` | Dialog pro výběr dvou kombinací a kontrolu jejich platnosti. |
| `Brand.ico` | Ikona EXE, dialogu a oznamovací oblasti. |
| `PrintScreen.manifest` | Deklaruje podporu DPI pro proces. |
| `Build-and-run.cmd` | Vyhledá 64bitový nebo 32bitový kompilátor .NET Framework, sestaví WinForms EXE a spustí ho. |
| `Sestavit-a-spustit.cmd` | Původní název, který spouští anglicky pojmenovaný sestavovací skript. |

Jde o samostatnou aplikaci WinForms pro .NET Framework bez souboru projektu a bez externích knihoven. Sestavovací skript musí běžet ve Windows. Příkaz kompilátoru používá `/target:winexe`, `/optimize+`, referenci na `System.dll`, `System.Windows.Forms.dll` a `System.Drawing.dll` a vloží manifest i ikonu. Před přestavbou ukončete běžící EXE.

Hotový EXE se publikuje jako soubor v GitHub Releases, nikoli v repozitáři se zdroji. Před novým vydáním se znovu sestaví z odpovídajícího zdrojového kódu.

## Životní cyklus

1. `Program.Main` nastaví WinForms a mutex `Local\PrintScreenAplikace`; současně může běžet jen jedna instance v uživatelské relaci.
2. `TrayApp` načte nastavení z registru aktuálního uživatele, vytvoří ikonu a nabídku, nastaví spuštění po přihlášení a pomocí `RegisterHotKey` zaregistruje obě zkratky. Mezi stisky neprovádí opakované snímání ani dotazování klávesnice.
3. Skryté `HotkeyWindow` přijímá `WM_HOTKEY` a předává identifikátor akce metodě `OnHotkey`. Příznak `capturing` brání souběžnému otevření několika výběrů.
4. Při ukončení se zkratky odregistrují a ikona, nabídka i okno se uvolní. Spuštění po přihlášení zůstává nastavené, dokud ho uživatel nevypne.

## Pořízení výřezu

`OnHotkey` přečte hranice `SystemInformation.VirtualScreen`, vytvoří bitmapu celé virtuální obrazovky a zkopíruje do ní pixely před zobrazením výběrového okna. `SelectionForm` pak přes všechny monitory zobrazí tento snímek se ztmavením. Levým tlačítkem se označí obdélník; `Esc` nebo výběr s nulovou plochou nevytvoří výsledek. Vybraná oblast se vyřízne z původní bitmapy.

- Akce **Malování** uloží PNG do `%TEMP%\PrintScreenAplikace`, pojmenuje jej `yyyy-MM-dd_HH-mm-ss-fff.png` podle místního času a spustí `mspaint.exe` s cestou k souboru. Při kolizi názvu přidá `-2`, `-3` atd. Aplikace dočasné soubory sama nemaže.
- Akce **Schránka** předá bitmapu do Windows schránky přes `Clipboard.SetDataObject` se třemi pokusy a 100ms prodlevou. Soubor se nevytvoří.

Výběr je synchronní na vlákně WinForms. Po dobu výběru je v paměti bitmapa celé virtuální obrazovky; spotřeba paměti proto roste s rozlišením a počtem monitorů. Chyby při snímání, ukládání a spuštění Malování se zobrazí uživateli.

## Zkratky a nastavení

Výchozí hodnoty jsou **Ctrl + W** pro Malování a **Ctrl + Shift + W** pro schránku. Dialog nabízí písmena A–Z, číslice 0–9, F1–F24, mezerník, Insert, Delete, Home, End, Page Up, Page Down a šipky. Běžná klávesa vyžaduje alespoň jeden modifikátor Ctrl, Shift nebo Alt; F1–F24 mohou být samostatně. `ValidKey` také odmítá samotné modifikátory, `Esc`, `Enter` a `Tab`. Obě akce musí mít různé kombinace.

Při otevření dialogu se původní zkratky dočasně odregistrují. Po potvrzení se zkusí zaregistrovat nový pár; když druhá registrace selže, první se zase odregistruje. Při zrušení nebo neúspěchu se obnoví původní pár. Nastavení se uloží až po úspěšné registraci. Kombinaci může odmítnout Windows nebo jiný program.

| Klíč registru aktuálního uživatele | Hodnota | Význam |
| --- | --- | --- |
| `Software\PrintScreenAplikace` | `PaintHotkey`, `ClipboardHotkey` | Číselné hodnoty `System.Windows.Forms.Keys`. |
| `Software\PrintScreenAplikace` | `Language` | `en` nebo `cs`; bez uložené volby je výchozí angličtina. |
| `Software\PrintScreenAplikace` | `StartAtLogin` | 1 nebo 0; výchozí je 1. |
| `Software\Microsoft\Windows\CurrentVersion\Run` | `PrintScreenAplikace` | Cesta k EXE v uvozovkách, pokud je automatické spuštění zapnuté. |

Texty rozhraní se vybírají pomocí metody `T(czech, englishText)` v `TrayApp` a `SettingsDialog`. Při přidání dalšího uživatelského textu doplňte obě jazykové varianty a udržujte oba soubory README v souladu.
