# Builds Leaf Markdown and its LeafSetup.exe installer.
#
#   .\build.ps1                 Build the app and the installer (dist\LeafSetup.exe).
#   .\build.ps1 -SkipInstaller  Build the app only (build\app\LeafMarkdown.exe).
#
# Requirements: .NET 10 SDK and Inno Setup 6.

param(
    [switch]$SkipInstaller
)

$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$appBuild = Join-Path $root 'build\app'
$dist = Join-Path $root 'dist'

# --- LeafMarkdown.exe (Release, win-x64, single file) ----------------------------

Write-Host 'Building Leaf Markdown'
Remove-Item -Recurse -Force $appBuild -ErrorAction SilentlyContinue
dotnet publish (Join-Path $root 'src\LeafMarkdown\LeafMarkdown.csproj') -c Release -o $appBuild
if ($LASTEXITCODE) { throw 'dotnet publish failed' }

# --- Installer -------------------------------------------------------------------

if (-not $SkipInstaller) {
    $iscc = @("${env:LOCALAPPDATA}\Programs\Inno Setup 6\ISCC.exe", "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe") |
        Where-Object { Test-Path $_ } | Select-Object -First 1
    if (-not $iscc) { throw 'Inno Setup 6 was not found. Install it with: winget install JRSoftware.InnoSetup' }
    New-Item -ItemType Directory -Force $dist | Out-Null
    & $iscc /Q "/O$dist" (Join-Path $root 'installer\LeafMarkdown.iss')
    if ($LASTEXITCODE) { throw 'Inno Setup failed' }
    Write-Host "Installer: $dist\LeafSetup.exe"
}
