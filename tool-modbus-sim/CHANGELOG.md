# Changelog

All notable changes to the OpenBench Modbus Simulator are recorded here.
Format follows [Keep a Changelog](https://keepachangelog.com/); versions follow
[SemVer](https://semver.org/).

## v0.1.0 — 2026-08-28

First public build. Windows x64, single self-contained `.exe` (~50 MB, no
.NET runtime required).

### Added

- **Slave mode** — emulate a Modbus device that answers requests from an
  editable register table.
- **Master mode** — poll a remote device on an interval; results are mirrored
  into the register table live, and edits are pushed back as writes.
- **Modbus TCP** (client and server) and **Modbus RTU over serial** (client
  and server), selectable at runtime.
- Full 0–65535 address space for all four tables: coils, discrete inputs,
  holding registers, input registers.
- Function codes `0x01`–`0x06`, `0x0F`, `0x10`, with spec-correct exception
  responses (illegal function / data address / data value).
- **Live traffic log** — every request and response with timestamp,
  direction, function, address, values, and raw ADU bytes.
- **CSV export** of the traffic log.
- Configurable serial parameters (baud, parity, data bits, stop bits) and
  unit/slave ID; configurable poll interval and response timeout for master
  mode.

### Notes

- No license key, no telemetry, no network activity of the tool's own.
- The RTU/serial path has been exercised only in loopback tests, not yet
  against physical hardware — feedback welcome via GitHub issues.
- Linux build is possible from source (`-r linux-x64`) but not yet published.

### Verify your download

```
sha256sum -c SHA256SUMS.txt
```
