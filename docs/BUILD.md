# Build And Development

This document is for developers. The README is focused on app usage.

Reviewed against the current implementation: 2026-08-26.

## .NET WPF App

The production app is the .NET WPF implementation under `dotnet/`.

```powershell
cd dotnet
dotnet run --project src\PLCConsoleProjectBuilder.Wpf
```

WPF requires Windows Desktop SDK support.

## Tests

```powershell
dotnet test dotnet\PLCConsoleProjectBuilder.sln --no-restore
```

## Build Single-File EXE

```powershell
.\build-dotnet-onefile.bat
```

The executable is written to:

```text
dotnet\publish\win-x64\PLCConsoleProjectBuilder.exe
```

Language resources are published to `dotnet\publish\win-x64\Languages\*.json`
so UI text can be edited without rebuilding. The same resources are also
embedded in the executable as a fallback when the external language files are
missing.

## GitHub Release Package

The GitHub Release asset is `PLCConsoleProjectBuilder-win-x64.zip`. The archive
contains only `PLCConsoleProjectBuilder.exe`; the release workflow does not add
the external `Languages` directory, DLL files, configuration files, or an
installer. The executable uses the embedded Japanese and English language
resources.

## Distribution Maintenance

The Windows package is distributed without code signing or an installer.
Build outputs under `dotnet/publish/` and `artifacts/` are excluded from Git.
When updating the package, verify that the release ZIP contains only
`PLCConsoleProjectBuilder.exe` and that it starts on a clean Windows PC.

When changing manual URLs or the app's help links, verify both destinations:

- Help menu: <https://plc-console.fa-labo.com/>
- Header link: <https://plc-console.fa-labo.com/projectbuilder/projectbuilder.html>
