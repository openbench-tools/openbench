# r/PLC launch post — Modbus Simulator v0.1.0

Draft for posting to https://reddit.com/r/PLC by the maintainer.
Reddit renders Markdown. Keep it plain — r/PLC is allergic to marketing voice.

Before posting:
- Check r/PLC's current rules / whether a flair like "Project Showcase" is required.
- Post from a real account with some history, not a fresh one.
- Reply to comments for the first day or two — that's what makes or breaks it.
- Cross-post candidates afterward: r/embedded, r/SCADA, r/AskElectronics (sparingly),
  and the "what are you working on" threads rather than new posts where those exist.

---

## Title options (pick one)

1. I got tired of per-seat licensing for a Modbus simulator, so I wrote a free one (MIT, TCP + RTU, slave + master)
2. Free open-source Modbus simulator — TCP/RTU, slave/master, single .exe, no license key
3. Made a free Modbus simulator because I only needed one for 20 minutes before a site visit

*(Recommend #1 — it names the pain the sub actually feels.)*

---

## Body

Modbus Poll / Modbus Slave are fine tools. But they're closed-source and
licensed per seat, and I kept hitting the same wall: I need a simulator for
twenty minutes to check a register map before a site visit, or a colleague
needs one on a machine that doesn't have a license, or IT wants any new tool
on an engineering workstation audited first and "trust me, it's a commercial
Modbus tool" isn't good enough for them.

So I built **OpenBench Modbus Simulator** and put it out under MIT.

**What it does (v0.1.0):**

- **Slave mode** — acts as a device. Fill in a register table, point your PLC
  / SCADA / test client at it, done. Incoming writes land in the table and
  the log.
- **Master mode** — point it at a real device, pick a table and address
  range, it polls on an interval and shows you the values live. Editing a
  value sends a write.
- **Modbus TCP and Modbus RTU over serial**, client and server, switch at
  runtime.
- All four tables across the full 0–65535 range. FC 01–06, 0F, 10, with
  proper exception responses.
- Live traffic log — timestamp, direction, function, decoded values, raw ADU
  bytes — export to CSV.
- No license key, no telemetry, no phone-home. Single self-contained .exe,
  no installer, no .NET runtime to chase down.

**Where it's honest about being young:**

- Windows x64 only right now. Linux builds from source (it's .NET + Avalonia).
- The RTU/serial path passes loopback tests but I have **not** hammered it
  against a pile of real devices yet. If you run it on an actual bus I'd
  genuinely like to hear what breaks.
- Not code-signed, so SmartScreen will grumble on first run. Checksums are on
  the release.

**Links:**

- Tool page + download: https://openbench.dev/tools/modbus-simulator
- Source (MIT): https://github.com/openbench-tools/openbench
- Docs: https://openbench.dev/tools/modbus-simulator/docs

It's a side project, not a company, and the local tool stays free. If there's
interest I'll keep going — next candidates are a serial-line terminal or a
TCP/IP tester, whichever people actually want.

Happy to answer anything about how it's built or take feature requests here.

---

## First-comment (optional, post as a reply to your own thread)

Quick note on scope since someone always asks: this is deliberately a local
bench tool. No cloud, no account, no license server — and I'm not planning to
bolt any of that onto the free version. If a hosted/team thing ever happens
it'll be a separate opt-in, and this stays as-is.
