# OpenBench Modbus Simulator

Free, open-source Modbus RTU/TCP slave and master simulator — no license
key, no telemetry. Built for testing SCADA systems, PLC programs, and
integrations without touching real hardware.

**Status:** In active development — no release yet. Watch this repo for the
first build.

[Website](https://openbench.dev/tools/modbus-simulator) ·
[Report an issue](../../issues)

## Why this exists

Commercial Modbus simulators work fine but are closed-source and licensed
per seat — real friction for something you might use for twenty minutes
before a site visit. This does the same job, free, with source available
for teams whose security policy requires audit-ability before a new tool
touches an engineering workstation.

## Features (planned for v0.1)

- Slave (server) and master (client) simulation modes
- Configurable holding registers, coils, and discrete inputs
- Live register read/write log, exportable as CSV
- Modbus RTU over serial and Modbus TCP in one tool

## Building from source

_Build instructions will be added once the initial implementation lands._

## License

MIT — see [LICENSE](./LICENSE).

## Support the project

OpenBench tools are free and always will be for local use. If this tool
saves you time, consider [buying a coffee](https://ko-fi.com/openbench) or
[sponsoring on GitHub](https://github.com/sponsors/openbench).
