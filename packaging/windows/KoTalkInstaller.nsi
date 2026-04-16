Unicode True
ManifestDPIAware True
RequestExecutionLevel user
SetCompressor /SOLID lzma

!include "MUI2.nsh"

!ifndef APP_NAME
  !define APP_NAME "KoTalk"
!endif

!ifndef APP_COMPANY
  !define APP_COMPANY "PHYSIA"
!endif

!ifndef APP_VERSION
  !define APP_VERSION "0.1.0"
!endif

!ifndef SOURCE_DIR
  !error "SOURCE_DIR define is required"
!endif

!ifndef OUTPUT_FILE
  !error "OUTPUT_FILE define is required"
!endif

!ifndef APP_ICON
  !define APP_ICON "branding/ico/kotalk.ico"
!endif

!define MUI_ABORTWARNING
!define MUI_ICON "${APP_ICON}"
!define MUI_UNICON "${APP_ICON}"
!define MUI_FINISHPAGE_RUN "$INSTDIR\KoTalk.exe"
!define MUI_FINISHPAGE_RUN_TEXT "KoTalk 실행"

Name "${APP_NAME}"
OutFile "${OUTPUT_FILE}"
InstallDir "$LOCALAPPDATA\KoTalk"
InstallDirRegKey HKCU "Software\${APP_COMPANY}\${APP_NAME}" "InstallDir"

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

!insertmacro MUI_LANGUAGE "Korean"

Section "Install"
  SetOutPath "$INSTDIR"
  File /r "${SOURCE_DIR}/*"
  WriteUninstaller "$INSTDIR\Uninstall KoTalk.exe"

  WriteRegStr HKCU "Software\${APP_COMPANY}\${APP_NAME}" "InstallDir" "$INSTDIR"
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "DisplayName" "${APP_NAME}"
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "Publisher" "${APP_COMPANY}"
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "DisplayVersion" "${APP_VERSION}"
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "InstallLocation" "$INSTDIR"
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "DisplayIcon" "$INSTDIR\KoTalk.exe"
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "UninstallString" "$INSTDIR\Uninstall KoTalk.exe"
  WriteRegDWORD HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "NoModify" 1
  WriteRegDWORD HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}" "NoRepair" 1

  CreateDirectory "$SMPROGRAMS\KoTalk"
  CreateShortcut "$SMPROGRAMS\KoTalk\KoTalk.lnk" "$INSTDIR\KoTalk.exe"
  CreateShortcut "$SMPROGRAMS\KoTalk\KoTalk 제거.lnk" "$INSTDIR\Uninstall KoTalk.exe"
  CreateShortcut "$DESKTOP\KoTalk.lnk" "$INSTDIR\KoTalk.exe"
SectionEnd

Section "Uninstall"
  Delete "$DESKTOP\KoTalk.lnk"
  Delete "$SMPROGRAMS\KoTalk\KoTalk.lnk"
  Delete "$SMPROGRAMS\KoTalk\KoTalk 제거.lnk"
  RMDir "$SMPROGRAMS\KoTalk"

  Delete "$INSTDIR\Uninstall KoTalk.exe"
  RMDir /r "$INSTDIR"

  DeleteRegKey HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_NAME}"
  DeleteRegKey HKCU "Software\${APP_COMPANY}\${APP_NAME}"
SectionEnd
