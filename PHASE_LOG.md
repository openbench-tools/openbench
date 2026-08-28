# OpenBench — Phase Log

## Phase 0 — Foundations & Repo Setup
**Status:** Scaffolded in this session.

- Astro 7 project created at `marketing-site/` (TypeScript, Tailwind v4 via Vite plugin, self-hosted fonts via @fontsource — no external font CDN needed).
- Design system: "instrument panel / engineering blueprint" identity — deep blueprint blue panels, amber indicator-lamp accent, IBM Plex + Space Grotesk type. Tokens live in `marketing-site/src/styles/global.css`.
- Signature hero element: `RegisterReadout.astro` — a live-looking simulated Modbus register poll, grounding the design in the actual product rather than generic hero art.
- Pages built: home (`/`), tools index (`/tools`), dynamic tool page (`/tools/[slug]`), about (`/about`), donate (`/donate`).
- Content collection (`tools`) set up with one entry: Modbus Simulator, `status: coming-soon` (no binary exists yet).
- Components: SiteNav, SiteFooter, DownloadButton (GitHub Releases link + coming-soon state), DonateWidget (Ko-fi + GitHub Sponsors), ToolCard, ToolLayout (shared template, includes SoftwareApplication JSON-LD).
- Plausible analytics wired via `data-event` attributes + a small click-delegation script in BaseLayout (network calls to plausible.io are commented out until a real domain/Plausible account exists).
- `tool-modbus-sim/` repo skeleton created: MIT `LICENSE`, `README.md` template (reusable pattern for future tool repos).
- Build verified locally (`npm run build` succeeds, 5 static pages generated).

**Session 2 additions:**
- `@astrojs/sitemap` integration added; build emits `sitemap-index.xml` + `sitemap-0.xml` (all 5 pages).
- `public/robots.txt` added (`Allow: /`, points at `https://openbench.dev/sitemap-index.xml`).
- Git repo initialized at project root (`main` branch) with `.gitignore` (node_modules, dist, .astro, *.zip) and `.gitattributes` (eol=lf). First commit made.
- `npm install` run; `node_modules/` now present locally.

---

## Phase 1 — Free Core Launch (Modbus Simulator)

**Status:** Application build started (session 3).

Stack decision: **C# / .NET 10 + Avalonia UI**, full v0.1 scope in one pass
(slave + master, Modbus RTU + TCP). See `memory/modbus-simulator-build.md`.
.NET 10 SDK is a user install — see `memory/dotnet-sdk-setup.md`.

**`ModbusSim.Core`** (protocol engine, zero UI deps, only `System.IO.Ports`):
- `Crc16` (CRC-16/MODBUS), `BitPacker`, big-endian helpers.
- `ModbusDataStore` — thread-safe, 4 tables × full 64K address space, range
  checks per spec, `Changed` event (distinguishes wire vs local writes).
- PDU codec: `ModbusRequest` / `ModbusResponse` parse + build for FC
  0x01–0x06, 0x0F, 0x10; `SlaveEngine` turns protocol errors into exception
  responses.
- Framing: `TcpFrameCodec` (MBAP), `RtuFrameCodec` (unit id + CRC).
- Runtime: `IFrameChannel` with `StreamFrameChannel` (TCP) and
  `SerialFrameChannel` (RTU, idle-gap framing); `ModbusSlave` (TcpListener /
  serial, multi-client), `ModbusMaster` (interval polling + one-shot write
  queue, mirrors reads into the store).
- `CsvLog` / `ModbusLogEntry` / `LogFormat` for the traffic log + export.

**`ModbusSim.App`** (Avalonia MVVM): single window — connection panel
(role / transport / TCP / serial settings), editable register `DataGrid`,
live traffic log `DataGrid` with CSV export. Builds and runs.

**`ModbusSim.Core.Tests`** (xUnit): 29 tests — CRC vectors, PDU round-trips,
slave engine behaviour, framing, and 3 real-socket TCP loopback integration
tests (master↔slave read, write, exception). All green.

**Not done yet:**
- Screenshot / GIF of the app for the tool page.
- `dotnet publish` single-file binaries + GitHub Release + checksums.
- Flip `marketing-site` tool entry from `coming-soon` to `live` + download URL.
- Serial (RTU) path is untested against real hardware — no COM port here.
- Docs page (install + usage).

**Phase 0 leftovers (unchanged):**
- Real domain registration, hosting deployment, Plausible account.
- GitHub org/repos actually created and pushed (local only so far).
