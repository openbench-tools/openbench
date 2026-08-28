# OpenBench Modbus Simulator

Free, open-source Modbus RTU/TCP slave and master simulator — no license
key, no telemetry. Built for testing SCADA systems, PLC programs, and
integrations without touching real hardware.

**Status:** In active development — pre-release. The core protocol engine and
a desktop UI are working; packaged binaries are not published yet.

[Website](https://openbench.dev/tools/modbus-simulator) ·
[Report an issue](../../issues)

## Why this exists

Commercial Modbus simulators work fine but are closed-source and licensed
per seat — real friction for something you might use for twenty minutes
before a site visit. This does the same job, free, with source available
for teams whose security policy requires audit-ability before a new tool
touches an engineering workstation.

## Features

- **Slave (server) and master (client)** simulation in one tool
- **Modbus TCP** and **Modbus RTU over serial**
- Configurable holding registers, input registers, coils, and discrete
  inputs across the full 0–65535 address space
- Editable register grid — change a value and connected clients see it; run
  as master and the grid mirrors a live device
- Live read/write traffic log with raw ADU bytes, exportable as CSV
- Function codes 0x01–0x06, 0x0F, 0x10, with correct exception responses
- No license key, no telemetry, no network calls of its own

## Project layout

```
tool-modbus-sim/
  src/
    ModbusSim.Core/     protocol engine — framing, PDU codec, slave/master
                        runtime, data store, CSV log (no UI dependencies)
    ModbusSim.App/      Avalonia desktop UI (MVVM)
  tests/
    ModbusSim.Core.Tests/   xUnit — protocol, framing, TCP loopback
  ModbusSim.slnx
```

## Building from source

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```
dotnet build ModbusSim.slnx -c Release
dotnet test  ModbusSim.slnx
dotnet run   --project src/ModbusSim.App -c Release
```

A self-contained single-file Windows build (single ~50 MB `.exe`, no runtime
install needed — the single-file knobs are already set in the `.csproj`):

```
dotnet publish src/ModbusSim.App -c Release -r win-x64 -o publish/win-x64
```

Swap `-r linux-x64` for a Linux build.

The `ModbusSim.Core` library targets any .NET 10 platform; the serial
transport uses `System.IO.Ports` (Windows and Linux).

## License

MIT — see [LICENSE](./LICENSE).

## Support the project

OpenBench tools are free and always will be for local use. If this tool
saves you time, consider [buying a coffee](https://ko-fi.com/openbench) or
[sponsoring on GitHub](https://github.com/sponsors/openbench).
