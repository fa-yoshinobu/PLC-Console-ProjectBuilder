# PLC Console ProjectBuilder

<img src="logo.png" alt="PLC Console ProjectBuilder logo" width="240">

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![.NET CI](https://github.com/fa-yoshinobu/PLC-Console-ProjectBuilder/actions/workflows/dotnet-ci.yml/badge.svg)](https://github.com/fa-yoshinobu/PLC-Console-ProjectBuilder/actions/workflows/dotnet-ci.yml)
![Windows WPF](https://img.shields.io/badge/platform-Windows%20WPF-0078D4?logo=windows)
![Project JSON v2](https://img.shields.io/badge/project%20JSON-v2-2ea44f)
![QR Zstd](https://img.shields.io/badge/QR-Zstd-f97316)
[![License: MIT](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)

[![C#](https://img.shields.io/badge/C%23-239120?logo=csharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)

[![Release](https://github.com/fa-yoshinobu/PLC-Console-ProjectBuilder/actions/workflows/release.yml/badge.svg)](https://github.com/fa-yoshinobu/PLC-Console-ProjectBuilder/actions/workflows/release.yml)

PLC Console ProjectBuilder is a PC tool for creating FA Labo PLC Console project settings and transferring them to the mobile apps as JSON or QR codes.

Instead of entering long PLC settings on a phone, edit the project on a PC and import it from the Android/iOS app QR scanner.

## Public Manual

- Manual site: <https://plc-console.fa-labo.com/>
- ProjectBuilder manual: <https://plc-console.fa-labo.com/projectbuilder/projectbuilder.html>

## Download

Download the Windows executable package from the release Assets:

- Releases: <https://github.com/fa-yoshinobu/PLC-Console-ProjectBuilder/releases>
- Asset file: `PLCConsoleProjectBuilder-win-x64.zip`

The zip contains only the self-contained Windows x64 executable
`PLCConsoleProjectBuilder.exe`. No installer, DLL, configuration file, or
external language file is required. Unzip the package, then start the exe.

Related repositories:

- ProjectBuilder: <https://github.com/fa-yoshinobu/PLC-Console-ProjectBuilder>
- Manual site: <https://github.com/fa-yoshinobu/PLC-Console-Site>

Primary configuration areas:

- PLC connection settings
- List registration
- Device comments
- Time Chart targets
- Trap settings

This repository contains only the PC-side project builder and QR export tool.
The Android and iOS app source code is maintained in separate repositories and
is not included here.

The supported desktop implementation is the .NET WPF app.

## Usage

1. Start `PLCConsoleProjectBuilder.exe`.
2. Create a project, or use `File` > `Load JSON` to open an existing schema v2 project.
3. Enter the project and PLC settings.
   - Project name
   - Vendor
   - CPU model
   - IP address, port, and transport
4. Register monitored addresses in `List`.
   - Rows can be pasted from Excel.
   - Columns are `Address / Data type / Comment`.
5. Edit address comments in `Comment`, including comment-only addresses that are not registered in List, Time Chart, or Trap.
6. Register graph targets in `Time Chart`.
   - Pasted columns are `Address / Data type / Comment`.
   - Up to 20 channels can be imported.
7. Register trigger rules in `Trap`.
   - Pasted columns are `Address / Data type / Comment / Condition / Threshold / Enabled`.
   - Examples: rising edge, change, greater than or equal.
   - Up to 20 traps can be imported.
8. Save JSON, generate QR pages, or save the QR pages as PNG files together with the JSON.
9. In the Android/iOS app, open `QR Import` and scan every displayed QR page. Pages may be scanned in any order.

Pasting from Excel opens a paste preview dialog that validates every row before
import. Valid rows are marked OK, invalid rows show the error reason, and rows
with a blank address are marked skipped. Importing applies only the OK rows, so
one bad row no longer fails the whole paste.

Clipboard import uses canonical values only. Data type and trap condition must be
explicit; localized aliases, guessed data types, and implicit trap conditions are
not accepted. During JSON / QR generation, a blank data type is filled only when
the same address already has one explicit data type in another row. Trap
conditions that do not match the address kind must be corrected explicitly.

Because ProjectBuilder does not communicate with the PLC, it validates address
syntax and the shared signed 32-bit representation limit but does not enforce a
CPU-specific Device Range. Android and iOS retrieve that range when connecting,
then exclude unsupported addresses from display and communication without
deleting them from the project.

For multi-page QR output, import completes after the mobile app has scanned
every page once. The page order does not matter. ProjectBuilder can switch the
displayed pages manually or automatically.

Project limits are shared with the mobile apps: 1,000 List devices, 20 Time
Chart channels, 20 Trap definitions, 5 MiB project JSON, 4,096 QR pages, 1 MiB
compressed QR data, and 5 MiB decompressed QR data. Polling accepts 100–10,000
ms and timeout accepts 250–10,000 ms independently. ProjectBuilder accepts and
emits only the `plc-console-project` schema identifier with schema version 2;
other identifiers and invalid JSON are rejected without conversion.

In schema v2, `deviceMeta.dataType` is conditional. Metadata referenced by
List, Time Chart, or Trap must contain `dataType`. Truly comment-only metadata
must omit `dataType` and contain a non-empty `comment`. ProjectBuilder applies
the same rule when reading and writing JSON; it does not infer a missing type or
accept a type on a comment-only entry.

## Scanning Tips

- Display each QR as large as possible.
- Scan each page once. The page order does not matter.
- For multiple pages, use the automatic page switch when it is easier than pressing Previous / Next.
- If a device cannot read the QR reliably, reduce the QR chunk size so the tool generates more smaller QR pages.
- After import, confirm that the project name and List contents changed in the mobile app.

## Documents

- [Build and development](docs/BUILD.md)
- [QR/JSON format](docs/QR_JSON_FORMAT.md)
- [GUI requirements](docs/GUI_REQUIREMENTS.md)
- [Manual demo project](examples/projectbuilder-manual-demo.json)
- [Time Chart and Trap example](examples/manual-timechart-trap.json)

## License

| Item | Value |
| --- | --- |
| License | [MIT](LICENSE) |
| Scope | Applies only to the PC-side project builder and QR export tool in this repository. It does not include the FA Labo PLC Console mobile apps for Android/iOS. |
