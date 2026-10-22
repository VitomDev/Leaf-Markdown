#define AppName "Leaf Markdown"
#define AppVersion "1.0.0"
#define AppExe "LeafMarkdown.exe"
#define ProgId "LeafMarkdown.Markdown"

[Setup]
AppId={{92BAB738-C41A-4CE8-A2FF-CF8A9F70723D}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher=VitomDev
AppPublisherURL=https://github.com/VitomDev/Leaf-Markdown
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
DisableDirPage=yes
DisableReadyPage=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
ChangesAssociations=yes
SetupIconFile=..\src\LeafMarkdown\app.ico
UninstallDisplayIcon={app}\{#AppExe}
LicenseFile=..\LICENSE
OutputDir=Output
OutputBaseFilename=LeafMarkdown-Setup-{#AppVersion}
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern

[Tasks]
Name: "assoc"; Description: "Open .md files with Leaf Markdown"; GroupDescription: "{cm:AdditionalIcons}"
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\dist\{#AppExe}"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\LICENSE"; DestDir: "{app}"; DestName: "LICENSE.txt"; Flags: ignoreversion
Source: "..\THIRD-PARTY-NOTICES.md"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExe}"; Tasks: desktopicon

[Registry]
Root: HKCU; Subkey: "Software\Classes\{#ProgId}"; ValueType: string; ValueData: "Markdown Document"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\{#ProgId}\DefaultIcon"; ValueType: string; ValueData: """{app}\{#AppExe}"",0"
Root: HKCU; Subkey: "Software\Classes\{#ProgId}\shell\open\command"; ValueType: string; ValueData: """{app}\{#AppExe}"" ""%1"""
Root: HKCU; Subkey: "Software\Classes\Applications\{#AppExe}"; ValueType: string; ValueName: "FriendlyAppName"; ValueData: "{#AppName}"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\Applications\{#AppExe}\shell\open\command"; ValueType: string; ValueData: """{app}\{#AppExe}"" ""%1"""
Root: HKCU; Subkey: "Software\Classes\Applications\{#AppExe}\SupportedTypes"; ValueType: string; ValueName: ".md"; ValueData: ""
Root: HKCU; Subkey: "Software\Classes\Applications\{#AppExe}\SupportedTypes"; ValueType: string; ValueName: ".markdown"; ValueData: ""
Root: HKCU; Subkey: "Software\Classes\.md\OpenWithProgids"; ValueType: string; ValueName: "{#ProgId}"; ValueData: ""; Flags: uninsdeletevalue
Root: HKCU; Subkey: "Software\Classes\.markdown\OpenWithProgids"; ValueType: string; ValueName: "{#ProgId}"; ValueData: ""; Flags: uninsdeletevalue
; Default handler for .md, only when the user leaves the option checked. Windows may still ask once.
Root: HKCU; Subkey: "Software\Classes\.md"; ValueType: string; ValueData: "{#ProgId}"; Tasks: assoc; Flags: uninsdeletevalue
Root: HKCU; Subkey: "Software\Classes\.markdown"; ValueType: string; ValueData: "{#ProgId}"; Tasks: assoc; Flags: uninsdeletevalue
Root: HKCU; Subkey: "Software\LeafMarkdown\Capabilities"; ValueType: string; ValueName: "ApplicationName"; ValueData: "{#AppName}"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\LeafMarkdown\Capabilities"; ValueType: string; ValueName: "ApplicationDescription"; ValueData: "Minimal Markdown viewer"
Root: HKCU; Subkey: "Software\LeafMarkdown\Capabilities\FileAssociations"; ValueType: string; ValueName: ".md"; ValueData: "{#ProgId}"
Root: HKCU; Subkey: "Software\LeafMarkdown\Capabilities\FileAssociations"; ValueType: string; ValueName: ".markdown"; ValueData: "{#ProgId}"
Root: HKCU; Subkey: "Software\RegisteredApplications"; ValueType: string; ValueName: "LeafMarkdown"; ValueData: "Software\LeafMarkdown\Capabilities"; Flags: uninsdeletevalue

[Run]
Filename: "{app}\{#AppExe}"; Description: "{cm:LaunchProgram,{#AppName}}"; Flags: nowait postinstall skipifsilent

[Code]
function HasDesktopRuntime10: Boolean;
var
  Rec: TFindRec;
begin
  Result := FindFirst(ExpandConstant('{commonpf64}\dotnet\shared\Microsoft.WindowsDesktop.App\10.*'), Rec);
  if Result then
    FindClose(Rec);
end;

function InitializeSetup: Boolean;
var
  Code: Integer;
begin
  Result := True;
  if not HasDesktopRuntime10 then
    if MsgBox('Leaf Markdown needs the .NET 10 Desktop Runtime (x64), which is not installed.' + #13#10#13#10 +
              'Open the download page? You can continue the setup and install it afterwards.',
              mbConfirmation, MB_YESNO) = IDYES then
      ShellExec('open', 'https://dotnet.microsoft.com/download/dotnet/10.0', '', '', SW_SHOWNORMAL, ewNoWait, Code);
end;
