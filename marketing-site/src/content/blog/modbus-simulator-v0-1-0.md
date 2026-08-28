---
title: "Modbus Simulator v0.1.0 — first release"
description: >
  The first public build of the OpenBench Modbus Simulator is out: TCP and RTU,
  slave and master, an editable register table and a live traffic log. Free,
  open source, no license key.
pubDate: 2026-08-28
tags: ["release", "modbus-simulator"]
tool: "modbus-simulator"
---

The first OpenBench tool is live. The **Modbus Simulator** is a free,
open-source utility for exercising Modbus TCP and RTU without real hardware —
either as a simulated device (slave) or as a polling client (master).

## Why build another Modbus simulator

The commercial ones work. They're also closed-source and licensed per seat,
which is real friction when you just need to confirm a register map before a
site visit, or when your company's security policy wants a tool audited
before it goes on an engineering workstation. This does the same job, free,
with the source open.

## What's in v0.1.0

- **Slave mode** — the app answers requests from an editable register table.
  Type a value in, and a connected client reads it back; incoming writes land
  in the table and the log.
- **Master mode** — point it at a device, pick a table and address range, and
  it polls on an interval. Results mirror into the table live; editing a value
  sends a write.
- **Modbus TCP and Modbus RTU over serial**, client and server, switchable at
  runtime.
- All four tables (coils, discrete inputs, holding & input registers) across
  the full 0–65535 address space.
- Function codes `0x01`–`0x06`, `0x0F`, `0x10`, with spec-correct exception
  responses.
- A **live traffic log** — every frame with timestamp, direction, function,
  decoded values, and raw ADU bytes — exportable as CSV.

No license key, no telemetry, no network calls of its own.

## Get it

A single self-contained Windows executable, no installer or .NET runtime
required. [Download it from the tool page](/tools/modbus-simulator), or grab
it straight from the
[GitHub release](https://github.com/openbench-tools/openbench/releases/latest).
The [documentation](/tools/modbus-simulator/docs) covers install and quick
starts for both TCP and RTU.

## Known limitations

- The RTU/serial path has been exercised in loopback tests only, not yet
  against physical hardware. If you run it on a real bus,
  [feedback is welcome](https://github.com/openbench-tools/openbench/issues).
- Windows x64 only for now; a Linux build works from source.
- Not code-signed, so SmartScreen will warn on first run.

## What's next

More testing against real devices, a configurable RTU inter-frame gap, and —
depending on what people actually ask for — either a serial-line terminal or
a TCP/IP tester as the second tool. If you have opinions, the
[GitHub Discussions](https://github.com/openbench-tools/openbench/discussions)
are open.
