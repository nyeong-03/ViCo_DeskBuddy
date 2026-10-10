@echo off
setlocal
set "ROOT=%~dp0"
set "CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%CSC%" set "CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe"
if not exist "%CSC%" (
  echo C# compiler not found. Install Visual Studio Build Tools or .NET Framework developer tools.
  exit /b 1
)
if not exist "%ROOT%dist" mkdir "%ROOT%dist"
"%CSC%" /nologo /target:winexe /out:"%ROOT%dist\MorningBuddy.exe" /win32manifest:"%ROOT%src\morning.manifest" /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Web.Extensions.dll /reference:System.Security.dll "%ROOT%src\*.cs"
if errorlevel 1 exit /b 1
if exist "%ROOT%google-client-id.txt" copy /y "%ROOT%google-client-id.txt" "%ROOT%dist\google-client-id.txt" >nul
echo Built: %ROOT%dist\MorningBuddy.exe
endlocal
