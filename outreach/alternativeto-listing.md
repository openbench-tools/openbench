# AlternativeTo submission — OpenBench Modbus Simulator

Submit at https://alternativeto.net/manage/new/ (needs a free account).
Listings are moderator-reviewed — keep the description factual and neutral,
not promotional. Do NOT submit until https://openbench.dev is live (the form
requires a working homepage URL and reviewers check it).

---

## Application name

OpenBench Modbus Simulator

## Homepage URL

https://openbench.dev/tools/modbus-simulator

## Short description (one line)

Free, open-source Modbus TCP and RTU simulator — acts as a slave (device) or
master (client), with an editable register table and a live traffic log.

## Full description

OpenBench Modbus Simulator is a desktop tool for testing Modbus systems
without physical hardware. It can emulate a Modbus device (slave/server) that
answers requests from an editable register table, or act as a Modbus client
(master) that polls a real device and mirrors its registers live.

It supports Modbus TCP and Modbus RTU over serial, in both roles, switchable
at runtime. All four data tables (coils, discrete inputs, holding registers,
input registers) span the full 0–65535 address space. Supported function
codes: read/write coils and registers (01–06, 0F, 10), with standard Modbus
exception responses.

Every request and response is shown in a live traffic log with timestamp,
direction, function, decoded values, and raw ADU bytes, and can be exported
to CSV.

Distributed as a single self-contained Windows executable (no installer or
runtime required). The source is MIT-licensed; Linux builds are possible from
source. No license key, no telemetry, no network activity of its own.

## License

Open Source — MIT

## Pricing

Free

## Platforms

- Windows
- Linux (from source)

## Tags / categories

modbus, scada, plc, industrial-automation, serial-port, protocol-analyzer,
network-testing, rs-485, iot, engineering

## "Alternative to" — suggest linking against these existing entries

- Modbus Poll (Witte Software)
- Modbus Slave (Witte Software)
- QModMaster
- ModbusTool
- Simply Modbus
- Modbus Mechanic

## Features to list (AlternativeTo lets you add feature bullets)

- Modbus TCP
- Modbus RTU (serial)
- Slave / server simulation
- Master / client polling
- Editable register map
- Live traffic log
- CSV export
- Raw frame / ADU view
- Portable (single executable)
- No account required
- Open source

## Screenshot

marketing-site/public/screenshots/modbus-simulator.png
(1709×1092 PNG — upload this in the submission form)

---

## After it's listed

- Ask a couple of people who actually use it to click "Like" on the listing
  and add a short review — new listings with zero engagement get buried.
- Add the GitHub repo link in the listing's "Links" section.
- Revisit in a month to add the v0.2 features once they ship.
