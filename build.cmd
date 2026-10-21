@echo off
rem Builds Leaf Markdown (Release, win-x64) into .\dist and, if Inno Setup is installed, the installer into .\installer\Output
setlocal
cd /d "%~dp0"
dotnet publish src\LeafMarkdown\LeafMarkdown.csproj -c Release -o dist || exit /b 1

set "ISCC="
if exist "%LOCALAPPDATA%\Programs\Inno Setup 6\ISCC.exe" set "ISCC=%LOCALAPPDATA%\Programs\Inno Setup 6\ISCC.exe"
if exist "%ProgramFiles(x86)%\Inno Setup 6\ISCC.exe" set "ISCC=%ProgramFiles(x86)%\Inno Setup 6\ISCC.exe"
if defined ISCC ("%ISCC%" /Q installer\LeafMarkdown.iss || exit /b 1) else echo Inno Setup not found, skipping installer.

echo.
echo Done: %~dp0dist\LeafMarkdown.exe
