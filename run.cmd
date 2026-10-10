@echo off
set "APP=%~dp0dist\MorningBuddy.exe"
if not exist "%APP%" (
  echo App not built yet. Run build.cmd first.
  pause
  exit /b 1
)
start "" "%APP%"
