-- D1 schema for the "notify me about a Pro / hosted tier" signup form.
-- Apply once: Cloudflare dashboard -> your D1 database -> Console -> paste -> Execute.
-- (or: npx wrangler d1 execute openbench --remote --file=migrations/0001_pro_interest.sql)

CREATE TABLE IF NOT EXISTS pro_interest (
  id          INTEGER PRIMARY KEY AUTOINCREMENT,
  email       TEXT NOT NULL UNIQUE,
  note        TEXT NOT NULL DEFAULT '',
  tool        TEXT NOT NULL DEFAULT '',
  user_agent  TEXT NOT NULL DEFAULT '',
  referer     TEXT NOT NULL DEFAULT '',
  created_at  TEXT NOT NULL,
  updated_at  TEXT
);

CREATE INDEX IF NOT EXISTS idx_pro_interest_created_at ON pro_interest (created_at);
