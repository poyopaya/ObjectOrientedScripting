; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "ObjectOrientedScripting Compiler"
#define MyAppVersion "0.5.1-ALPHA"
#define MyAppPublisher "X39"
#define MyAppURL "http://x39.io/?page=projects&project=ObjectOrientedScripting"
#define MyAppExeName "Wrapper.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{52A7EACD-C1EA-4328-B463-91F6AA9467F9}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
OutputBaseFilename=setup
Compression=lzma
SolidCompression=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "D:\GitHub\ObjectOrientedScripting\ObjectOrientedScripting\Wrapper\bin\Release\Wrapper.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\GitHub\ObjectOrientedScripting\CompilerDlls\Compiler-0.5.0-ALPHA.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\GitHub\ObjectOrientedScripting\ObjectOrientedScripting\Wrapper\bin\Release\Logger.dll"; DestDir: "{app}"; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"

[Code]
var
  ChangelogPage: TOutputMsgMemoWizardPage;

procedure InitializeWizard;
begin
  ChangelogPage := CreateOutputMsgMemoPage(wpWelcome, 'Changelog', 'The change history', 'Feel free to fully ignore this changelog anytime :)',
'Version 0.5.1-ALPHA                                                             ' + AnsiChar(#10) +
'    |- Wrapper:   Fixed naming of -gen param (poject.oosproj instead of         ' + AnsiChar(#10) +
'    |             project.oosproj)                                              ' + AnsiChar(#10) +
'    |- Wrapper:   Fixed "URI-Format not supported" message when not forcing     ' + AnsiChar(#10) +
'    |             a DLL (dll lookup now works as expected -.-*)                 ' + AnsiChar(#10) +
'    |- Compiler:  Fixed functions getting invalidly recognized as duplicate     ' + AnsiChar(#10) +
'Version 0.5.0-ALPHA                                                             ' + AnsiChar(#10) +
'    |- Wrapper:   Fixed -gen is not working if file is not existing             ' + AnsiChar(#10) +
'    |             (also if file was existing ... but expected error then)       ' + AnsiChar(#10) +
'    |- Compiler:  Flag /DEFINE="#whatever(arg) dosomething with arg"            ' + AnsiChar(#10) +
'    |- Compiler:  Flag /THISVAR="_thisvarname_"                                 ' + AnsiChar(#10) +
'    |- Compiler:  PreProcessor replaced non-keywords when they just contained a ' + AnsiChar(#10) +
'    |             part of the keyword (EG. keyword was FOO, FOOBAR would have   ' + AnsiChar(#10) +
'    |             been replaced with <CONTENT>BAR)                              ' + AnsiChar(#10) +
'    |- Compiler:  PreProcessor now supports "merge" operator ##                 ' + AnsiChar(#10) +
'    |             #define FOO(BAR) BAR##FOOBAR                                  ' + AnsiChar(#10) +
'    |             FOO(test) => testFOOBAR                                       ' + AnsiChar(#10) +
'    |v- Compiler: GEN2 Implementation                                           ' + AnsiChar(#10) +
'    ||-           New Syntax                                                    ' + AnsiChar(#10) +
'    ||-           New SQF ObjectStructure                                       ' + AnsiChar(#10) +
'    ||-           Type Restriction (with that all stuff that is connected to it)' + AnsiChar(#10) +
'    ||-           Interfaces (and with them virtual functions)                  ' + AnsiChar(#10) +
'    ||-           "Linker" issues with proper issue IDs                         ' + AnsiChar(#10) +
'    ||            (currently room for 4 digits (so 0001 - 9999))                ' + AnsiChar(#10) +
'    |\-           No overhead anymore                                           ' + AnsiChar(#10) +
'Version 0.4.0-ALPHA                                                             ' + AnsiChar(#10) +
'    |- Wrapper:   Now returns -1 if was not successfully                        ' + AnsiChar(#10) +
'    |- Wrapper:   Added "setFlags(string[])" function to ICompiler interface    ' + AnsiChar(#10) +
'    |- Wrapper:   Fixed compilerDLL search location                             ' + AnsiChar(#10) +
'    |             Working dir (applicationside) was checked                     ' + AnsiChar(#10) +
'    |             and not executable dir                                        ' + AnsiChar(#10) +
'    |- Compiler:  Fixed naming of functions in output config file               ' + AnsiChar(#10) +
'    |             being incorrect                                               ' + AnsiChar(#10) +
'    |- Compiler:  Added flag /CLFN with STRING value ("/CLFN=blabla.cfg")       ' + AnsiChar(#10) +
'    |             Sets how the output config will be named                      ' + AnsiChar(#10) +
'    |- Compiler:  Added flag /NFNC                                              ' + AnsiChar(#10) +
'    \             Removes the CfgFunctions class from the config file           ' + AnsiChar(#10) +
'                                                                                ' + AnsiChar(#10) +
'Version 0.3.0-ALPHA                                                             ' + AnsiChar(#10) +
'    |- Compiler:  changed block native code from:                               ' + AnsiChar(#10) +
'    |                 native <instructions> endnative                           ' + AnsiChar(#10) +
'    |             to:                                                           ' + AnsiChar(#10) +
'    |                 startnative <instructions> endnative                      ' + AnsiChar(#10) +
'    |- Compiler:  Added "native(<instructions>)" specially for expressions      ' + AnsiChar(#10) +
'    |             (will be merged at some point with the block native again)    ' + AnsiChar(#10) +
'    |- Compiler:  Added SQF Call instruction:                                   ' + AnsiChar(#10) +
'    |                SQF [ (>arg1>, <argN>) ] <instruction> [ (>arg1>, <argN>) ]' + AnsiChar(#10) +
'    |- Compiler:  Added missing detection for                                   ' + AnsiChar(#10) +
'    |             unsigned integer/double values in VALUE                       ' + AnsiChar(#10) +
'    |- Compiler:  Added missing detection for                                   ' + AnsiChar(#10) +
'    |             >, >=, <, <= operations in EXPRESSION                         ' + AnsiChar(#10) +
'    |- Compiler:  Added missing LOCALVARIABLE alternative for FORLOOP           ' + AnsiChar(#10) +
'    |- Compiler:  Fixed FORLOOP                                                 ' + AnsiChar(#10) +
'    \- Compiler:  PrettyPrint sqf output improved                               ' + AnsiChar(#10) +
'                                                                                ' + AnsiChar(#10) +
'Version 0.2.0-ALPHA                                                             ' + AnsiChar(#10) +
'    |v- Wrapper:  New Parameters                                                ' + AnsiChar(#10) +
'    ||-           "sc=<FILE>"    Used to check the syntax of some document      ' + AnsiChar(#10) +
'    ||-           "dll=<FILE>"   Forces given dll (ignores project settings)    ' + AnsiChar(#10) +
'    |\-           "log[=<FILE>]" Enables LogToFile                              ' + AnsiChar(#10) +
'    |                            (with optional file parameter)                 ' + AnsiChar(#10) +
'    |- Compiler:  Fixed TryCatch                                                ' + AnsiChar(#10) +
'    |- Compiler:  Fixed Expressions                                             ' + AnsiChar(#10) +
'    |- Compiler:  Implemented class inheritance                                 ' + AnsiChar(#10) +
'    |- Compiler:  Implemented public/private encapsulation                      ' + AnsiChar(#10) +
'    |- Compiler:  when parsing error was found the objectTree                   ' + AnsiChar(#10) +
'    |             wont get written out anymore                                  ' + AnsiChar(#10) +
'    |- Wrapper:   Fixed ArgumentDetection (foo=bar was not detected)            ' + AnsiChar(#10) +
'    \- Logger:    Disabled logToFile per default                                '
  );
end;