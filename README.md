# Výřezy obrazovky

Lehká aplikace pro Windows 10/11, která běží u hodin. Vybranou část obrazovky otevře v Malování nebo zkopíruje do schránky. Podporuje více monitorů, češtinu a angličtinu. [English documentation](README.en.md).

## Sestavení a spuštění

Ve Windows spusťte `Sestavit-a-spustit.cmd`. Skript sestaví `PrintScreen.exe` ze souborů `Program.cs` a `SettingsDialog.cs` pomocí kompilátoru .NET Framework, který musí být v systému dostupný. Využívá také `Brand.ico` a `PrintScreen.manifest`; žádné další balíčky nepotřebuje. Pokud aplikace už běží, ukončete ji přes ikonu u hodin, aby se EXE dalo přepsat. Hotový `PrintScreen.exe` lze pak spouštět přímo.

Při prvním spuštění se zapne automatické spouštění **po přihlášení aktuálního uživatele**. V nabídce ikony lze volbu **Spouštět po přihlášení** vypnout. Aplikace nevyžaduje oprávnění správce.

## Použití

| Zkratka | Akce |
| --- | --- |
| **Ctrl + W** | Tažením levého tlačítka vyberte oblast. Výřez se uloží jako PNG a otevře v Malování. |
| **Ctrl + Shift + W** | Vyberte oblast. Obrázek se vloží do schránky bez uložení souboru. |
| **Esc** během výběru | Zruší výřez. |

Zkratky fungují v celém systému. Výchozí **Ctrl + W** tak může převzít funkci zavření karty v jiných aplikacích; v takovém případě ji změňte.

### Nastavení zkratek a jazyka

Pravým tlačítkem klikněte na ikonu u hodin a zvolte **Nastavení zkratek…**. U každé akce zaškrtněte Ctrl, Shift nebo Alt a **vyberte klávesu v seznamu**. Pro **Ctrl + R** nechte Ctrl zaškrtnuté a v seznamu zvolte **R**. Stisk samotné kombinace do pole nic nezadá. Samostatně lze použít také F1–F24. Obě akce musí mít různé kombinace. Po kliknutí na **Uložit** aplikace ověří, zda je Windows dokáže zaregistrovat; obsazenou kombinaci odmítne a obnoví původní zkratky.

V nabídce **Jazyk → English** lze přepnout uživatelské texty. Volba se uloží pro příští spuštění.

## Obrázky a nastavení

- Pro Malování vznikne soubor `%TEMP%\PrintScreenAplikace\yyyy-MM-dd_HH-mm-ss-fff.png` podle místního data a času pořízení. Při shodě názvů se přidá číselná přípona. Jde o dočasné úložiště; úpravy, které chcete uchovat, uložte z Malování pomocí **Uložit jako** do vlastní složky.
- Při kopírování do schránky aplikace žádný soubor nevytváří. Další kopírování může obsah schránky přepsat.
- Zkratky, jazyk a volba spuštění se ukládají pod `HKEY_CURRENT_USER\Software\PrintScreenAplikace`. Automatické spuštění používá hodnotu `PrintScreenAplikace` pod `HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run` s cestou k EXE. Pokud EXE přesunete, spusťte ho z nového umístění, aby se cesta aktualizovala.

Aplikace neposílá snímky po síti. Obrazovku snímá až po stisku zkratky.

## Potíže

- **Zkratka nereaguje:** Ověřte, že aplikace běží u hodin. Jiná aplikace může kombinaci používat; v nastavení zkuste jinou.
- **Po stisku Ctrl + R se v poli nic nezmění:** Zaškrtněte Ctrl a vyberte R v rozbalovacím seznamu.
- **Po sestavení stále běží stará verze:** Ukončete ji přes nabídku ikony, spusťte sestavovací skript znovu a zkontrolujte jeho výstup.
- **Výřez se neotevře v Malování:** Aplikace zobrazí chybu, pokud Windows nedokáže spustit `mspaint.exe`. Zkontrolujte dostupnost Malování.

## Pro vývojáře

[DEVELOPERS.md](DEVELOPERS.md) popisuje stavbu aplikace, tok pořízení výřezu a ukládané nastavení. Zdrojový kód je poskytován pod [licencí MIT](LICENSE). Ikona `Brand.ico` vychází z dodaného modrého symbolu značky.
