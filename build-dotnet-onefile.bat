@echo off
setlocal

set "ROOT=%~dp0"
set "PROJECT=%ROOT%dotnet\src\PLCConsoleProjectBuilder.Wpf\PLCConsoleProjectBuilder.Wpf.csproj"
set "OUT=%ROOT%artifacts\release"

if not exist "%PROJECT%" (
  echo Project file not found: %PROJECT%
  exit /b 1
)


dotnet publish "%PROJECT%" ^
  -c Release ^
  -r win-x64 ^
  --self-contained true ^
  -o "%OUT%" ^
  /p:PublishSingleFile=true ^
  /p:IncludeNativeLibrariesForSelfExtract=true ^
  /p:EnableCompressionInSingleFile=true ^
  /p:DebugType=None ^
  /p:DebugSymbols=false

if errorlevel 1 exit /b %errorlevel%

powershell -NoProfile -File "%ROOT%.github\scripts\package_release.ps1" -OutputDirectory "%OUT%"
if errorlevel 1 exit /b 1

echo.
echo Single-file EXE:
dir /b "%OUT%\*.exe"
echo.
echo Output folder: %OUT%
