# Claude Code Development Playbook — OpenBench

*A working, phase-by-phase build guide. Locked decisions: brand = OpenBench, first and only Wave 1 tool = Modbus simulator, license = MIT, time budget = 14 hrs/week (2 hrs/day). Each phase lists: goal, concrete tasks, suggested folder/file structure, example Claude Code prompts, and a "done when" checklist.*

**Scope discipline reminder:** Wave 1 is the Modbus simulator only. Do not start the serial terminal, TCP/IP tester, OPC UA client, SEO tools, or the Pro tier until the Month 3 checkpoint in the business plan (Section 11) has been reviewed. At 14 hrs/week, scope is the main risk to this plan — hold the line here.

---

## How to use this document
Work through phases in order inside Claude Code. For each phase:
1. Open a fresh Claude Code session in your project folder (or continue the same one).
2. Paste the phase goal + tasks as context, or reference this file directly (`Read the DEVELOPMENT_PLAN.md and let's start Phase 0`).
3. Don't jump ahead — Phase 3's validation gate is deliberate; skipping it is the single biggest time-waste risk for a side-hustle project.

Save this file as `DEVELOPMENT_PLAN.md` in your project root so Claude Code can reference it directly in every session.

---

## Phase 0 — Foundations & Repo Setup
**Goal:** A deployed, empty-but-real marketing site skeleton and properly licensed GitHub repos.

### Tasks
1. Register the OpenBench domain.
2. Create GitHub org (or personal account namespace) — e.g. `openbench` or `openbench-tools`.
3. Decide repo structure — recommended for solo maintenance, Wave 1 scope only:
   ```
   openbench/
     marketing-site/        (Astro site — this is the main web project)
     tool-modbus-sim/       (Modbus simulator source — the only tool repo for now)
   ```
   *(Serial terminal, TCP/IP tester, and OPC UA repos are added later in Wave 2 — don't create them yet.)*
4. Add an **MIT `LICENSE`** file to the Modbus repo.
5. Add a solid `README.md` template to reuse across tool repos (badges, screenshot, quick description, download link, build-from-source instructions).
6. Scaffold the Astro marketing site.
7. Deploy skeleton to Vercel/Cloudflare Pages, connect domain.
8. Set up Plausible or Umami analytics.

### Suggested folder structure (`marketing-site/`)
```
marketing-site/
  src/
    layouts/
      BaseLayout.astro
      ToolLayout.astro         ← shared template for every tool page
    pages/
      index.astro               ← home
      about.astro
      contact.astro
      donate.astro
      tools/
        modbus-simulator.astro  ← the only tool page for Wave 1
      blog/
        index.astro
        [slug].astro
    content/
      tools/                    ← content collection, one .md per tool
        modbus-simulator.md
      blog/
        [release-notes].md
    components/
      DownloadButton.astro
      DonateWidget.astro
      ToolCard.astro
      SiteNav.astro
      SiteFooter.astro
  astro.config.mjs
  package.json
  DEVELOPMENT_PLAN.md           ← this file, kept in repo root for reference
```

### Example Claude Code prompts
- `Scaffold a new Astro project in marketing-site/ with TypeScript, Tailwind, and content collections enabled.`
- `Create a content collection schema for "tools" with fields: name, tagline, description, screenshots (array), downloadUrl, sourceUrl, docsUrl, category, releaseDate.`
- `Build a BaseLayout.astro and ToolLayout.astro following the folder structure in DEVELOPMENT_PLAN.md Phase 0.`
- `Set up deployment config for Vercel and add a GitHub Actions workflow to deploy on push to main.`

### Done when
- [ ] Site deploys successfully at your domain, even if pages are placeholders.
- [ ] All tool repos exist, public, licensed.
- [ ] Analytics tracking confirmed working (test pageview shows up).

---

## Phase 1 — Free Core Launch (Modbus Simulator Live)
**Goal:** The Modbus simulator fully live with a working download → donate funnel. This is the only tool built in Wave 1.

### Tasks
1. Build the Modbus simulator itself (from zero — no existing source to start from).
2. Write its content entry (`content/tools/modbus-simulator.md`) — description, features list, requirements, screenshots.
3. Build the shared `ToolLayout.astro` render: hero, screenshots gallery, feature list, Download button (→ GitHub Releases URL), Source button (→ repo), Docs link, Donate widget.
4. Create the GitHub Release for that tool with binaries, checksums, and a changelog.
5. Add Ko-fi/Buy Me a Coffee embed + GitHub Sponsors link component.
6. Add SEO basics: per-page meta title/description, Open Graph image, sitemap.xml, and `SoftwareApplication` JSON-LD structured data on tool pages.
7. Write a short docs page (`/tools/modbus-simulator/docs` or a `docs/` content collection) covering installation and basic usage.
8. Manually test the full funnel: land on homepage → click tool → download → (optional) click donate.

### Example Claude Code prompts
- `Create the DownloadButton.astro component that fetches the latest release asset URL from the GitHub Releases API for a given repo, with a static fallback link.`
- `Add SoftwareApplication JSON-LD structured data to ToolLayout.astro using the tool's frontmatter fields.`
- `Generate a sitemap.xml and robots.txt for the Astro site.`
- `Build a DonateWidget.astro component that embeds a Ko-fi button and links to GitHub Sponsors.`

### Done when
- [ ] Modbus simulator page live, download works, donate widget visible.
- [ ] Docs page published.
- [ ] Structured data validates (test in Google's Rich Results Test).
- [ ] Submitted to at least one directory/community (see Phase 2 marketing tasks).

---

## Phase 2 — Marketing Push & Instrumentation (Still Wave 1 — No New Tools)
**Goal:** Drive real traffic/usage to the Modbus simulator and instrument tracking so Phase 3's checkpoint has real data to review. **Do not start building a second tool, SEO utilities, or the Pro tier in this phase** — that's the most likely place scope creep happens, and it's explicitly out of bounds until the Month 3 checkpoint passes.

### Tasks
1. Build a blog/changelog collection; publish a release-notes post for the Modbus tool.
2. Instrument event tracking (Plausible custom events) on: download button clicks, donate widget clicks, docs link clicks — this is Phase 3's demand-validation data source.
3. Add a lightweight "Notify me about Pro/remote-monitoring features" email capture on the tool page — this feeds Phase 3's Pro-interest signal, without committing to building anything yet.
4. Submit to directories/communities: AlternativeTo.net, SourceForge, Product Hunt, r/PLC, r/embedded, relevant LinkedIn groups.
5. Track qualitative signals too: GitHub stars, issues, discussion activity, any organic mentions/shares.

### Example Claude Code prompts
- `Add Plausible custom event tracking to DownloadButton.astro and DonateWidget.astro (fire a "download_click" / "donate_click" event with the tool name as a property).`
- `Build a blog index and [slug] page using a content collection, sorted by date, with an RSS feed.`
- `Add a simple email capture form component that posts to a Supabase table (or a lightweight service like a Google Form/Airtable at this stage) with fields: email, created_at, interest_note.`

### Done when
- [ ] Event tracking confirmed firing correctly (check Plausible dashboard).
- [ ] Submitted to at least 3 directories/communities.
- [ ] Email capture live on the tool page.
- [ ] Roughly 3 months have elapsed since Phase 1 launch — proceed to Phase 3's checkpoint review.

---

## Phase 3 — Month 3 Checkpoint (Hard Gate — Do Not Skip)
**Goal:** Decide, using real evidence, whether to proceed to Wave 2 (more tools + Pro tier) or extend Wave 1 marketing instead. This is the business plan's Section 11 checkpoint, applied concretely.

### Tasks
1. Pull the numbers: total downloads, GitHub stars, issues/discussion activity, email-capture signups.
2. Check each against the locked stop/reassess criteria:
   - **Downloads:** under ~50 total → weak signal.
   - **Engagement:** zero GitHub stars/issues/discussion → weak signal.
   - **Pro-interest:** zero email captures or direct asks → weak signal.
   - **Community/fan signals:** no organic mentions, shares, forum/subreddit/LinkedIn pickup → weak signal, usually a distribution problem, not a product one.
3. If most/all signals are weak: **do not start a second tool or the Pro tier.** Go back to Phase 2 marketing tasks with new/different channels for another cycle before reassessing again.
4. If signals are reasonably strong: write a one-paragraph Pro-feature spec (exact feature, e.g. remote Modbus device monitoring, exact pricing hypothesis: $9-15/mo individual, $30-50/mo team) and proceed to Phase 4.
5. Optionally, directly ask existing users (a pinned GitHub Discussion, a short survey) what they'd pay for, to sharpen the Phase 4 feature spec.

### Example Claude Code prompts
- `Write a small script to pull GitHub Issues and stars from [repo] via the GitHub API and summarize activity over the last 3 months.`
- `Pull Plausible analytics data via its API for download_click and donate_click events over the last 3 months and summarize trends.`

### Done when
- [ ] The checkpoint has been reviewed against all four criteria, not just downloads alone.
- [ ] A clear decision is made and written down: proceed to Wave 2, or extend Wave 1 marketing.
- [ ] **Do not proceed to Phase 4 unless the decision is "proceed."**

---

## Phase 4 — Pro/Cloud Tier Build
**Goal:** One working, billable Pro feature live — the actual revenue engine.

### Tasks

**4a. Backend & Auth**
1. Create Supabase project. Design schema: `users`, `subscriptions`, `devices`/`logs` (or whatever the validated feature needs), `teams` (if team-tier planned).
2. Enable Supabase Auth: email/password + GitHub OAuth provider.
3. Set up row-level security policies so users only see their own data.

**4b. Billing**
4. Set up Paddle (or Stripe) account, create product + pricing plans (individual monthly, team monthly).
5. Implement checkout flow (Paddle Checkout overlay or Stripe Checkout).
6. Implement webhook handler (serverless function) to sync subscription status into the Supabase `subscriptions` table on payment events (created, renewed, cancelled).
7. Build a "manage subscription" link to the billing provider's customer portal.

**4c. Pro Web App**
8. Scaffold a new Next.js app (`pro-app/`) separate from the marketing site.
   ```
   pro-app/
     app/
       (auth)/
         login/
         signup/
       (dashboard)/
         dashboard/
         devices/            ← or whatever the validated feature is
         settings/
         billing/
       api/
         webhooks/
           paddle/route.ts
     lib/
       supabase/
       auth/
     components/
   ```
9. Build login/signup pages using Supabase Auth UI or custom forms.
10. Build the core Pro feature end to end (e.g., a devices list + log viewer, updated via Supabase Realtime).
11. Gate access to the dashboard behind an active-subscription check.

**4d. Connect Free Desktop App → Cloud**
12. Add an opt-in "Sync to cloud" setting in the free desktop app that authenticates against the Pro backend and pushes the relevant data (only for users who choose to enable it).
13. Document this connection clearly in both the free tool's docs and the Pro app's onboarding.

**4e. Marketing Integration**
14. Add a `/pricing` page to the marketing site.
15. Add "Upgrade to Pro" CTAs to the relevant tool page and inside the desktop app itself.
16. Set up a support email/alias and note a response-time expectation for paying customers.

### Example Claude Code prompts
- `Scaffold a Next.js 14 App Router project called pro-app with Supabase auth (email + GitHub OAuth) already wired up, following the folder structure in DEVELOPMENT_PLAN.md Phase 4c.`
- `Create the Supabase schema and RLS policies for users, subscriptions, and devices tables as described in Phase 4a.`
- `Build a Paddle webhook handler at app/api/webhooks/paddle/route.ts that verifies the signature and updates the subscriptions table on subscription.created, subscription.updated, and subscription.canceled events.`
- `Build a protected /dashboard/devices page that only renders for users with an active subscription, showing realtime device data from Supabase.`
- `Add a /pricing page to the marketing site with two tiers (Individual, Team), each linking to the appropriate Paddle checkout.`

### Done when
- [ ] A user can sign up, pay, log in, and use the one validated Pro feature end to end.
- [ ] Webhook correctly updates subscription status on payment events (test with Paddle/Stripe sandbox mode first).
- [ ] Free desktop app's opt-in sync (if applicable) connects correctly to the Pro backend.
- [ ] First real paying subscriber acquired.

---

## Phase 5 — Iterate on Pro Tier
**Goal:** Grow and refine based on real subscriber behavior.

### Tasks
1. Instrument product analytics (PostHog free tier) inside `pro-app/` to see feature usage.
2. Collect direct feedback from early subscribers (a simple in-app feedback widget or just email).
3. Prioritize the next Pro feature based on actual requests, not the original roadmap.
4. Build team-tier features (multi-user seats, shared dashboards) once individual-tier traction justifies it.
5. Revisit pricing if conversion data suggests it's off.

### Example Claude Code prompts
- `Add PostHog tracking to pro-app for key events: login, dashboard_view, feature_used.`
- `Build a team invitation flow: team owner can invite members by email, invited users get a Supabase Auth magic link tied to the team's account.`

### Done when
- [ ] Steady month-over-month subscriber growth or a clear, documented reason it's plateaued.
- [ ] At least one iteration shipped based on real subscriber feedback.

---

## Phase 6 — Backlog: Additional Tools, SEO Utilities, Business Utilities, Ads (Month 9+, Only If Phase 4-5 Are Stable)
**Goal:** Expand beyond OpenBench's Wave 1-2 scope — treat everything here as backlog, not roadmap, until Pro tier has paying subscribers.

### Backlog items (do not start any of these without re-confirming demand first)
- Additional OpenBench free tools: serial terminal, TCP/IP tester, OPC UA client — add whichever the Phase 3/5 data suggests users actually want.
- SEO/dev utility tools (sitemap generator, meta tag checker, etc.) — **separate brand from OpenBench**, own landing page/site.
- Paid business-utility product (invoicing, QR/UTM tools) — **separate brand from OpenBench**, reuse the Paddle/Supabase infrastructure built in Phase 4, own paid-from-day-one flow.
- Ads (EthicalAds/Carbon Ads) on docs/blog pages — small, secondary, add only once there's meaningful traffic.

### Example Claude Code prompts (only when actually starting one of these)
- `Scaffold a new, separately-branded site for [SEO tools / business utilities], reusing the existing Paddle checkout integration from pro-app where applicable.`

### Done when
- [ ] Whichever backlog item is chosen is live with its own working funnel, kept brand-separate from OpenBench per Section 5 of the business plan.

---

## Appendix A — Things to explicitly tell Claude Code NOT to build
Reuse this list at the start of relevant sessions to keep scope tight:
- No custom license-key/DRM system for free tools.
- No self-hosted file server for downloads — GitHub Releases only.
- No custom ticketing system — email support is sufficient early on.
- No mobile app.
- No i18n/multi-language support unless traffic data justifies it.
- No custom admin panel — use Supabase's and Paddle/Stripe's built-in dashboards.
- No second tool, SEO utility, business utility, or Pro-tier build before the Phase 3 Month 3 checkpoint passes.

## Appendix B — Recommended Claude Code session structure
- Keep the marketing site and the Pro app as separate Claude Code sessions/working directories once Phase 4 starts — they're different frameworks (Astro vs Next.js) and mixing context slows things down.
- Start each new phase's first session with: `Read DEVELOPMENT_PLAN.md, we are starting Phase [N]. Confirm the goal and task list before we begin.` — this keeps Claude Code anchored to the plan rather than improvising scope.
- After finishing each phase's "Done when" checklist, commit a note in the repo (e.g., `PHASE_LOG.md`) recording what shipped and what was learned — useful both for your own memory and as context for future Claude Code sessions.
