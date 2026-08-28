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

**Single-file Windows binary:** `dotnet publish -r win-x64` now produces one
self-contained ~50 MB `.exe` (compression + native-lib self-extract on;
Skia/HarfBuzz `.pdb`s trimmed by an `AfterTargets="Publish"` target; knobs
live in `ModbusSim.App.csproj` under a `RuntimeIdentifier != ''` condition so
plain build/test is unaffected). Built `OpenBench-ModbusSimulator-0.1.0-win-x64.exe`
+ `SHA256SUMS.txt` locally under `publish/win-x64/` (gitignored); smoke-tested
(launches). No GitHub Release yet — no repo/remote exists.

**Shipped:**
- GitHub: org `openbench-tools`, repo `openbench-tools/openbench` (monorepo,
  both folders). Pushed.
- **Release `v0.1.0`** cut with `OpenBench-ModbusSimulator-0.1.0-win-x64.exe`
  + `SHA256SUMS.txt`.
- Docs page live at `/tools/modbus-simulator/docs` (install, TCP/RTU quick
  starts, register table, traffic log, troubleshooting, build).
- Tool entry flipped to `status: live`, `version: 0.1.0`. `DownloadButton`
  now resolves the actual `.exe` asset from the GitHub Releases API at build
  time (static fallback to the releases page), shows version + size.
  `ToolLayout` JSON-LD gained `softwareVersion` / `datePublished` /
  `downloadUrl` / `license`.

- **App screenshot** captured (`public/screenshots/modbus-simulator.png`) —
  running as a TCP slave with a populated register table + live traffic log.
  Rendered as a band under the tool-page hero; also in the JSON-LD.

- **Deployed** to **Cloudflare Pages** (Vercel account was flagged): project
  `openbench`, root dir `marketing-site`, build `npm run build` → `dist`,
  auto-deploys on push to `main`. Live at **https://openbench-3ub.pages.dev**.
  All 6 routes 200; download button, screenshot, JSON-LD, sitemap, robots all
  verified on the deployed site. `astro.config` `site:` still points at
  `https://openbench.dev` (correct once the domain is attached; canonical /
  OG / sitemap URLs use it in the meantime).

---

## Phase 2 — Marketing Push (started)

- **Blog scaffolded** (commit `73d5080`): `blog` content collection, `/blog`
  index (date-sorted), `/blog/[slug]` via `BlogPostLayout`, `/rss.xml`
  (`@astrojs/rss`), `<link rel=alternate>` + Blog nav/footer links. First
  post: *"Modbus Simulator v0.1.0 — first release"*. Also defined the
  `.prose-ob` markdown styles in `global.css` (were referenced, never
  defined — also improves the tool-page overview slot). Live & verified on
  the pages.dev deploy.

**Not done yet:**
- Plausible custom events on the `data-event` hooks (download / donate / docs
  / source clicks) — Phase 2 core deliverable.
- Email capture ("notify me about Pro features") on the tool page.
- Directory / community submissions (r/PLC, r/embedded, AlternativeTo, PH).
- Register `openbench.dev` + attach it to the Pages project (Cloudflare
  Registrar — same dashboard).
- Plausible (or Umami) account → then uncomment the analytics line in
  `BaseLayout.astro`.
- Validate structured data in Google's Rich Results Test (needs a stable URL;
  can run against the pages.dev URL now).
- GIF / short demo video (still just the one static screenshot).
- Serial (RTU) path is untested against real hardware — no COM port here.
- Blog / release-notes post (Phase 2).
- `.exe` is not code-signed (SmartScreen warning on first run).
