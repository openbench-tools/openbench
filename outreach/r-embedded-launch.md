# r/embedded launch post — Modbus Simulator v0.1.0

Draft for posting to https://reddit.com/r/embedded by the maintainer.
Reddit renders Markdown.

Before posting:
- r/embedded is strict about self-promo. Check the current rules. If there's a
  live "What are you working on?" sticky, post there first / instead — a tool
  you built to solve your own problem is welcome, a launch announcement less so.
- Lead with the debugging angle, not the product. This crowd builds Modbus
  *devices* and needs something on the other end of the wire.
- Post from an account with history. Answer comments.

---

## Title options (pick one)

1. I wrote an open-source Modbus simulator (TCP + RTU) to test my own device firmware — MIT, shows raw ADU bytes
2. Free Modbus master/slave simulator with a raw-frame view, for debugging your device's Modbus stack
3. Made a from-scratch Modbus simulator (no NModbus/libmodbus dependency) — TCP + RTU, MIT

*(Recommend #1.)*

---

## Body

If you've written a Modbus slave into device firmware, you know the testing
loop: you need a master on the other end that you trust, that shows you
exactly what went on the wire, and that you can run on whatever machine is on
the bench that day. The commercial options are per-seat licensed and
closed; wiring up a second micro as a test master is its own rabbit hole.

I built **OpenBench Modbus Simulator** for this and released it under MIT.

**Useful for embedded work specifically:**

- **Master mode** to exercise a device you're building: pick function code,
  address, quantity, poll interval; watch responses and exceptions come back.
- **Slave mode** to stand in for a device your code talks to, so you can
  develop the master side before hardware exists.
- **Raw ADU bytes** on every frame in the log, next to the decoded view — so
  when your CRC is wrong or your byte count is off by one, you can see it.
- **RTU framing** is gap-based with a configurable inter-frame timeout, which
  matters when you're on a slow USB-serial adapter.
- The protocol layer is written from scratch (no NModbus, no libmodbus) —
  ~600 lines, if you want to read exactly what it does or lift a piece of it.
  FC 01–06, 0F, 10; proper exception responses; CRC-16/MODBUS.
- Single self-contained binary, no runtime install. Windows x64 for now;
  it's .NET + Avalonia so `dotnet publish -r linux-x64` works if you want it
  on your Linux box (I just haven't published that build yet).

**Caveats:** RTU passes loopback tests but hasn't been beaten on against a
wide range of real devices — reports welcome. Not code-signed. TCP slave on
port 502 needs admin on Windows; use a high port.

**Links:**

- Download / tool page: https://openbench.dev/tools/modbus-simulator
- Source (MIT): https://github.com/openbench-tools/openbench
- Docs: https://openbench.dev/tools/modbus-simulator/docs

Side project, local tool stays free. Feedback and framing-edge-case bug
reports are the most useful thing right now.

---

## Cross-post / follow-up targets

- r/PLC (separate, more application-focused draft — different framing)
- r/SCADA
- Hacker News "Show HN" once the domain is live and there's a bit of a
  track record (stars, a couple of resolved issues)
- The `libmodbus` / `pymodbus` issue trackers are NOT the place — don't.
