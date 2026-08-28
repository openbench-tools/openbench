# Product Hunt launch — OpenBench Modbus Simulator

Submit at https://www.producthunt.com/products/new (needs an account with
some history — a brand-new account launching a product looks like spam).

## Set expectations first

Product Hunt's audience is startup / general-tech, not industrial automation.
A Modbus simulator is niche here and will **not** top the leaderboard. That's
fine — what you actually get:

- a permanent, indexed listing (decent backlink + SEO)
- a spike of dev traffic on launch day
- a bit of third-party credibility ("it's on Product Hunt")
- occasional long-tail referrals

Treat it as one more distribution channel, not the launch. Do it **after**
r/PLC and r/embedded, once the domain is live and there are a few GitHub
stars / a resolved issue or two to point at.

## Timing & mechanics

- Launch at 12:01 AM Pacific. Tuesday–Thursday. Avoid days with a big-name launch.
- Self-hunt (you don't need an external hunter anymore).
- Be present in the comments all day — reply to every one.
- Line up a handful of people beforehand who'll genuinely try it and comment
  (not "great launch!" — actual first impressions). PH down-ranks vote rings;
  real comments are what move it.

---

## Listing fields

### Name

OpenBench Modbus Simulator

### Tagline (60 char max — pick one)

1. Free, open-source Modbus simulator — no license key
2. Test Modbus TCP & RTU devices without the hardware
3. The Modbus simulator you can actually audit and share

*(Recommend #2 — says what it does to someone who doesn't know the space;
#1 as backup.)*

### Description (~260 char)

A desktop tool for testing Modbus TCP and RTU systems without real hardware.
Emulate a device (slave) from an editable register table, or poll a real one
(master) and watch it live. Live traffic log with raw bytes, CSV export.
Single .exe, MIT-licensed, no telemetry.

### Topics

Developer Tools, Open Source, Hardware, Engineering, Productivity

### Links

- Website: https://openbench.dev/tools/modbus-simulator
- GitHub: https://github.com/openbench-tools/openbench
- (Docs link goes in the first comment)

### Gallery

First image is the thumbnail — make it count.

- [ ] `marketing-site/public/screenshots/modbus-simulator.png` (have this;
      crop/pad to ~1270×760 for PH's aspect ratio)
- [ ] TODO: a second shot — master mode polling a device
- [ ] TODO: a short GIF — type a value in the register table, watch a client
      read it back in the log (10–15s, this is the one that sells it)
- [ ] Optional: the traffic log with an exception response highlighted

The GIF matters more than the stills here. Worth recording before launch.

---

## Maker's first comment (post immediately after the listing goes live)

Hey Product Hunt 👋

I do test & automation engineering, and Modbus is everywhere in that world —
PLCs, sensors, drives, building systems. When you're integrating or testing
one of these, you constantly need to fake the other end of the connection:
pretend to be a device your software expects, or pretend to be the software
so you can check a device.

The tools that do this are solid but closed-source and licensed per seat,
which is friction for something you might use for twenty minutes before a
site visit — and a real blocker when a colleague needs one on an unlicensed
machine, or when IT wants any new tool audited before it goes on an
engineering workstation.

So I built OpenBench Modbus Simulator and put it out under MIT:

- Acts as a **slave** (fake device, editable register table) or **master**
  (polls a real device, shows values live)
- **Modbus TCP and RTU over serial**, both roles, switch at runtime
- **Live traffic log** with decoded values *and* raw frame bytes — handy when
  you're chasing a CRC or byte-count bug
- Single self-contained .exe, no installer, no runtime, no account, no
  telemetry
- Source is ~600 lines of protocol code you can actually read

It's v0.1.0 and honest about it: Windows x64 only for now (Linux builds from
source), RTU is tested in loopback but not yet against a wide range of real
devices, and it's not code-signed. Feedback — especially "I ran it on a real
bus and X broke" — is the most useful thing right now.

The local tool stays free. If a hosted/team version ever happens it'll be a
separate opt-in, not a paywall on this.

Docs: https://openbench.dev/tools/modbus-simulator/docs
Happy to answer anything.

---

## After launch

- Add the PH badge/embed to the tool page or README ("Featured on Product Hunt").
- Log the result in `README.md` (upvotes, comments, referral traffic in Plausible).
- Whatever the number, the listing is now a permanent asset — don't sweat the rank.
