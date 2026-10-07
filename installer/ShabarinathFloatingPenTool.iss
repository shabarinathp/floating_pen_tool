#define MyAppName "Shabarinath Floating Pen Tool"
#define MyAppVersion "5.2.0"
#define MyAppPublisher "Shabarinath"
#define MyAppExeName "ShabarinathFloatingPenTool.exe"

[Setup]
AppId={{A9C45B55-8A22-4D93-9EFA-AF7AD2BA8771}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\Shabarinath Floating Pen Tool
DefaultGroupName=Shabarinath Floating Pen Tool
OutputDir=..\installer_output
OutputBaseFilename=Shabarinath_Floating_Pen_Tool_V5_2_Setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=lowest

[Files]
Source: "..\publish\ShabarinathFloatingPenTool.exe"; DestDir: "{app}"; Flags: ignoreversion

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional icons:"; Flags: unchecked

[Icons]
Name: "{autoprograms}\Shabarinath Floating Pen Tool"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\Shabarinath Floating Pen Tool"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch Shabarinath Floating Pen Tool"; Flags: nowait postinstall skipifsilent