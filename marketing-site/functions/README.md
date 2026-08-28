# Cloudflare Pages Functions

## `api/notify.ts` — Pro-interest email capture

`POST /api/notify` with JSON `{ email, note?, tool? }` (or form-encoded).
Stores one row per email in a D1 table (`pro_interest`), upserting on repeat
submissions. Honeypot field: `company` (must be empty). Returns
`{ ok: true }` on success, `{ ok: false, error }` otherwise. Returns `503`
with `error: "not_configured"` if the `DB` binding is missing — the form then
shows a "not live yet" message instead of an error.

### One-time setup

1. **Create the database** — Cloudflare dashboard → *Storage & databases → D1*
   → *Create database* → name it `openbench`.
2. **Create the table** — open the database → *Console* tab → paste the
   contents of [`../migrations/0001_pro_interest.sql`](../migrations/0001_pro_interest.sql)
   → *Execute*.
3. **Bind it** — *Workers & Pages → openbench (Pages project) → Settings →
   Bindings* (or *Functions → D1 database bindings*) → *Add* →
   variable name **`DB`**, database **`openbench`** → *Save*.
4. **Redeploy** — *Deployments* → retry the latest, or push any commit.

### Reading the signups

D1 → `openbench` → *Console*:

```sql
SELECT email, tool, note, created_at
FROM pro_interest
ORDER BY created_at DESC;
```
