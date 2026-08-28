---
name: "Modbus Simulator"
tagline: "Simulate Modbus RTU and TCP devices without touching real hardware"
description: >
  A free, open-source Modbus slave and master simulator for testing SCADA
  systems, PLC programs, and integrations before they ever touch a real
  device. Built by a test & automation engineer who needed this on the bench
  every week and got tired of licensing friction for a tool this simple.
category: "engineering"
status: "live"
version: "0.1.0"
releaseDate: 2026-08-28
protocol: "Modbus RTU / Modbus TCP"
features:
  - "Slave (server) and master (client) simulation modes"
  - "Configurable holding registers, coils, and discrete inputs"
  - "Live register read/write log, exportable as CSV"
  - "Modbus RTU over serial and Modbus TCP in one tool"
  - "No license key, no telemetry, source available for audit"
requirements:
  - "Windows 10/11 (Linux build planned post-launch)"
  - "A serial port or virtual COM port for RTU mode"
sourceUrl: "https://github.com/openbench-tools/openbench"
docsUrl: "/tools/modbus-simulator/docs"
downloadUrl: "https://github.com/openbench-tools/openbench/releases/latest"
---

## Why this exists

Commercial Modbus simulators are capable but closed-source and licensed per
seat — which is a real barrier when you just need to check that a register
map is right before a site visit. OpenBench's Modbus Simulator does the same
job, free, with the source code open for anyone whose company requires
audit-ability before installing new tools on an engineering workstation.
