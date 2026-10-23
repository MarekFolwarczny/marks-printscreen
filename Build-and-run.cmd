@echo off
setlocal
cd /d "%~dp0"

set "CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%CSC%" set "CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe"
if not exist "%CSC%" (
    echo .NET Framework compiler not found. / Kompilator .NET Framework nebyl nalezen.
    pause
    exit /b 1
)

"%CSC%" /nologo /target:winexe /optimize+ /out:"PrintScreen.exe" /r:System.dll /r:System.Windows.Forms.dll /r:System.Drawing.dll /win32manifest:"PrintScreen.manifest" /win32icon:"Brand.ico" "Program.cs" "SettingsDialog.cs"
if errorlevel 1 (
    echo Build failed. / Sestaveni se nezdarilo.
    pause
    exit /b 1
)

start "" "%~dp0PrintScreen.exe"
echo The app is running in the tray. / Aplikace je spustena. Ikonu najdete u hodin.
echo Change sign-in startup from the tray menu. / Spousteni po prihlaseni lze zmenit v nabidce ikony.
pause
