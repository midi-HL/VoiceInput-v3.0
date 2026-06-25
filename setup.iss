; Inno Setup Script for VoiceInput
[Setup]
AppName=VoiceInput
AppVersion=3.0.0
DefaultDirName={localappdata}\VoiceInput
DefaultGroupName=VoiceInput
UninstallDisplayIcon={app}\VoiceInput.exe
Compression=lzma2
SolidCompression=yes
OutputDir=.
OutputBaseFilename=VoiceInput-Setup
PrivilegesRequired=lowest

[Files]
Source: "publish\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs

[Icons]
Name: "{group}\VoiceInput"; Filename: "{app}\VoiceInput.exe"
Name: "{commondesktop}\VoiceInput"; Filename: "{app}\VoiceInput.exe"

[Run]
Filename: "{app}\VoiceInput.exe"; Description: "Launch VoiceInput"; Flags: postinstall nowait skipifsilent
