@echo off
setlocal

set "EXE=%~dp0artifacts\release\PLCConsoleProjectBuilder.exe"

if not exist "%EXE%" (
  call "%~dp0build-dotnet-onefile.bat"
  if errorlevel 1 exit /b %errorlevel%
)

start "" "%EXE%"
