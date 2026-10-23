# Výřezy obrazovky

Lehká aplikace pro Windows 10/11, která běží u hodin. Vybranou část obrazovky otevře v Malování nebo zkopíruje do schránky. Podporuje více monitorů, angličtinu a češtinu. [English documentation](README.md).

**[Stáhnout aplikaci pro Windows (.exe)](https://github.com/MarekFolwarczny/marks-printscreen/releases/latest/download/PrintScreen.exe)**

## Stažení a spuštění

Stáhněte `PrintScreen.exe` z odkazu výše a spusťte ho ve Windows. Instalace není potřeba. EXE se publikuje v [Releases](https://github.com/MarekFolwarczny/marks-printscreen/releases); repozitář obsahuje zdrojový kód. Pro sestavení ze zdrojů spusťte ve Windows `Build-and-run.cmd`. Skript použije dostupný kompilátor .NET Framework, soubory `Program.cs`, `SettingsDialog.cs`, `Brand.ico` a `PrintScreen.manifest`. Další balíčky nejsou potřeba. Před přestavbou ukončete běžící aplikaci přes ikonu u hodin. Původní název `Sestavit-a-spustit.cmd` zůstává jako zástupce.

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

Pro nové uživatele je výchozí angličtina. V nabídce **Language → Čeština** lze přepnout na češtinu a přes **Jazyk → English** zase zpět. Volba se uloží pro příští spuštění.

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

[DEVELOPERS.cs.md](DEVELOPERS.cs.md) popisuje stavbu aplikace, tok pořízení výřezu a ukládané nastavení. Zdrojový kód je poskytován pod [licencí MIT](LICENSE). Ikona `Brand.ico` vychází z dodaného modrého symbolu značky.
