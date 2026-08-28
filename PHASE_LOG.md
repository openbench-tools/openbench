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

**Not done yet (next session):**
- Actual Modbus simulator application code (still zero — this session only built the marketing site and repo skeleton).
- Real domain registration, hosting deployment (Vercel/Cloudflare Pages), Plausible account.
- Screenshots/GIFs for the tool page (currently text-only).
- `sitemap.xml` / `robots.txt` generation.
- GitHub org/repos actually created and pushed (built locally only so far).
