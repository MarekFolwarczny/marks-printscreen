@echo off
setlocal
cd /d "%~dp0"

set "CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%CSC%" set "CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe"
if not exist "%CSC%" (
    echo Kompilator .NET Framework nebyl nalezen. / .NET Framework compiler not found.
    pause
    exit /b 1
)

"%CSC%" /nologo /target:winexe /optimize+ /out:"PrintScreen.exe" /r:System.dll /r:System.Windows.Forms.dll /r:System.Drawing.dll /win32manifest:"PrintScreen.manifest" /win32icon:"Brand.ico" "Program.cs" "SettingsDialog.cs"
if errorlevel 1 (
    echo Sestaveni se nezdarilo. / Build failed.
    pause
    exit /b 1
)

start "" "%~dp0PrintScreen.exe"
echo Aplikace je spustena. Ikonu najdete u hodin. / The app is running in the tray.
echo Spousteni po prihlaseni lze zmenit v nabidce ikony. / Change sign-in startup from the tray menu.
pause
