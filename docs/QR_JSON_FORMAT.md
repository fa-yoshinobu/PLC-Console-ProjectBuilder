# QR And Project JSON Format

This document describes the app-compatible QR payload and project JSON shape.
The README is intentionally kept as a user guide.

Reviewed against the current implementation: 2026-08-26.

## Project JSON

The generated JSON uses the shared `plc-console-project` schema v2 consumed
by Android and iOS.

Generated project JSON includes only shared schema v2 fields. UI-only
preferences and runtime observation values are not emitted.

ProjectBuilder emits shared address metadata in `deviceMeta`. Comments are
stored there once per address, and referenced addresses store their data type
there as well. `deviceList`, `timeChart`, and `traps` store membership and trap
settings only.

Top-level fields:

- `schema`
- `schemaVersion`
- `exportInfo`
- `projectId`
- `projectName`
- `plc`
- `deviceList`
- `timeChart`
- `deviceMeta`
- `traps`
- `updatedAtEpochMs`

Exporters should emit fields in this order for diff readability. Importers must
still treat JSON object order as non-semantic.

`exportInfo` is an output memo overwritten by the exporting app. It contains
`source` (`PROJECT_BUILDER`, `ANDROID`, or `IOS`) and `version` (the exporter app
version). Importers must not treat this as project identity.

`deviceList` and `timeChart` entries contain only `address`. A `deviceMeta`
entry whose normalized address is referenced by `deviceList`, `timeChart`, or
`traps` must contain `address` and `dataType`; `comment` is optional. An
unreferenced, comment-only entry must contain `address` and a non-empty
`comment`, and must omit `dataType`. Importers reject a missing referenced
`dataType`, a comment-only `dataType`, or a blank comment-only `comment` without
guessing or fallback. ProjectBuilder generation also rejects an explicit data
type on a comment-only row instead of silently discarding it. Comments are
normalized to one line and must be 1024 characters or fewer.
Trap entries contain `id`, `enabled`, `address`, `condition`, and
`comparisonValue`; trap data types are resolved through `deviceMeta`.

MELSEC routing uses decimal `networkNo` / `stationNo` values and a canonical
`moduleIo` target name. Remote passwords are never emitted in JSON or QR
payloads; the mobile apps store them in device-local secure storage after the
user enters them.

`plc.cpuModel` is the mobile app canonical connection model key. ProjectBuilder
keeps friendly labels such as `MELSEC iQ-R (built-in)`, `KEYENCE KV-8000`, and
`KEYENCE KV-8000 (XYM)` in the UI, but schema v2 JSON emits values such as
`melsec:iq-r`, `melsec:iq-r:rj71en71`, `melsec:qcpu:qj71e71-100`,
`keyence:kv-8000`, and `keyence:kv-8000-xym`.
ProjectBuilder must not emit a separate `plcProfile` field in schema v2.

ProjectBuilder enforces the mobile app registration limits: up to 1,000 List
devices, 20 Time Chart targets, and 20 trap definitions. The normalized union
of addresses referenced by those three sections may contain up to 1,040
entries. In addition, `deviceMeta` may contain up to 100,000 unreferenced,
comment-only addresses. Project JSON is limited to 5 MiB (5,242,880 bytes) as
the final total-size protection. Polling is limited to 100–10,000 ms and timeout
to 250–10,000 ms; timeout may be shorter than the polling interval.

## Value Sets

- `plc.vendor`: `MELSEC`, `KEYENCE`
- `plc.cpuModel`: mobile app canonical model key
  - MELSEC: `melsec:iq-r`, `melsec:iq-r:rj71en71`, `melsec:iq-f`, `melsec:iq-l`, `melsec:mx-r`, `melsec:mx-r:rj71en71`, `melsec:mx-f`, `melsec:qnudv`, `melsec:qnudv:qj71e71-100`, `melsec:qnu`, `melsec:qnu:qj71e71-100`, `melsec:qcpu:qj71e71-100`, `melsec:lcpu`, `melsec:lcpu:lj71e71-100`
  - KEYENCE: `keyence:kv-nano`, `keyence:kv-nano-xym`, `keyence:kv-3000`, `keyence:kv-3000-xym`, `keyence:kv-5000`, `keyence:kv-5000-xym`, `keyence:kv-7000`, `keyence:kv-7000-xym`, `keyence:kv-8000`, `keyence:kv-8000-xym`, `keyence:kv-x500`, `keyence:kv-x500-xym`
- `plc.connection.mode`: `REAL`, `DEMO_MOCK`
- `plc.connection.transport`: `TCP`, `UDP`
- `traps.condition`: `RISING_EDGE`, `FALLING_EDGE`, `CHANGE`, `GREATER_OR_EQUAL`, `LESS_OR_EQUAL`, `EQUAL`, `NOT_EQUAL`
- referenced `deviceMeta.dataType`: `BIT`, `INT16`, `UINT16`, `INT32`, `UINT32`, `FLOAT32`

## QR Payload

Each QR contains:

```text
PLCIOC1|ZSTD|<session>|<index>|<total>|<sha256>|<payload-chunk>
```

- `index` is 1-based.
- `sha256` is calculated from the minified JSON bytes after decompression.
- `payload-chunk` is a slice of base64url-encoded Zstd-compressed JSON without padding.
- QR count is not fixed, but is limited to 4,096 pages.
- Pages may be read in any order. Readers arrange chunks by `index`, join all
  chunks, then decompress the combined compressed bytes.

## Compression Requirements

`PLCIOC1|ZSTD` uses a Zstandard frame and requires Zstd support in the importing
app.

Readers and writers limit the compressed QR payload to 1 MiB and the
decompressed project JSON to 5 MiB. A complete import must contain every index
from 1 through `total` exactly once; missing or duplicate indexes are rejected.
Session identifiers are limited to 128 characters.

## Compatibility Policy

Only the `plc-console-project` schema identifier with schema version 2 is
accepted. Do not add silent fallback, alias conversion, or compatibility
normalization for unsupported or invalid values. Invalid data should fail
visibly so QR/JSON bugs are caught early.
