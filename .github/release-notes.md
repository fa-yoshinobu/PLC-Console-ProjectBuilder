## Download
- [Windows x64 ZIP]({{DOWNLOAD_URL}})
- [SHA-256 checksum]({{CHECKSUM_URL}})

## Getting started
Extract the ZIP file, then run `PLCConsoleProjectBuilder.exe`.

Requirements: Windows x64. The .NET runtime is included.

## Changes
### Changed
- Validate offline device addresses using shared syntax and Int32 limits instead of model-specific device ranges; connected mobile apps validate the actual PLC catalog.
- Reject 32-bit values whose second word would exceed the shared address limit.
- Updated regression tests for valid KEYENCE addresses and invalid number, bit-bank and multiword ranges.
- Standardized Windows ZIP distribution, checksums and release documentation.

### Fixed
- Wait for VirusTotal analysis to complete and preserve unrelated release notes when updating scan results.

<!-- virustotal:start -->
{{VIRUSTOTAL_CONTENT}}
<!-- virustotal:end -->
