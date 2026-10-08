; ============================================
; INSTALLEUR WINDOWS - APPLICATION POINTAGE
; Version 2.1.1 - Johanes-mg
; ============================================

[Setup]
AppId={{8E7A8C4F-1234-4567-89AB-CDEF01234567}
AppName=Pointage
AppVersion=2.1.1
AppVerName=Pointage 2.1.1
AppPublisher=Johanes-mg
AppPublisherURL=https://johanes-mg.github.io
AppSupportURL=https://johanes-mg.github.io
AppUpdatesURL=https://johanes-mg.github.io
DefaultDirName={autopf}\Pointage
DefaultGroupName=Pointage
DisableProgramGroupPage=yes
OutputDir=D:\Creation\C#\Pointage\Installeur
OutputBaseFilename=PointageSetup
SetupIconFile=D:\Creation\C#\Pointage\Images\logo.ico
Compression=lzma2/ultra64
SolidCompression=yes
LZMAUseSeparateProcess=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64compatible
ArchitecturesAllowed=x64compatible
PrivilegesRequired=admin
UninstallDisplayIcon={app}\Pointage.exe
DisableDirPage=no
DisableReadyPage=no

[Languages]
Name: "french"; MessagesFile: "compiler:Languages\French.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "D:\Creation\C#\Pointage\bin\Release\net8.0-windows\win-x64\publish\Pointage.exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\Pointage"; Filename: "{app}\Pointage.exe"
Name: "{group}\{cm:UninstallProgram,Pointage}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\Pointage"; Filename: "{app}\Pointage.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\Pointage.exe"; Description: "{cm:LaunchProgram,Pointage}"; Flags: nowait postinstall skipifsilent

[Code]
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  Reponse: Integer;
  DossierDonnees: String;
begin
  if CurUninstallStep = usPostUninstall then
  begin
    DossierDonnees := ExpandConstant('{userappdata}\Pointage');
    if DirExists(DossierDonnees) then
    begin
      Reponse := MsgBox(
        'Voulez-vous egalement supprimer vos donnees de pointage ?' + #13#10 + #13#10 +
        'Dossier : ' + DossierDonnees + #13#10 + #13#10 +
        'Si vous repondez Non, vos donnees seront conservees pour une future reinstallation.',
        mbConfirmation, MB_YESNO);

      if Reponse = IDYES then
      begin
        DelTree(DossierDonnees, True, True, True);
      end;
    end;
  end;
end;