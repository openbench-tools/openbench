/// <reference types="@cloudflare/workers-types" />

// Cloudflare Pages Function — POST /api/notify
// Stores a "notify me about a Pro / hosted tier" signup in a D1 database.
// This is a demand signal for the Month-3 checkpoint, not a product commitment.
//
// Requires a D1 binding named `DB` on the Pages project (see functions/README.md).
// If the binding is missing the endpoint returns 503 and the form shows a
// "not live yet" message rather than breaking.

interface Env {
  DB?: D1Database;
}

const EMAIL_RE = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

export const onRequestPost: PagesFunction<Env> = async ({ request, env }) => {
  let payload: Record<string, unknown>;
  try {
    const ct = request.headers.get('content-type') ?? '';
    if (ct.includes('application/json')) {
      payload = (await request.json()) as Record<string, unknown>;
    } else {
      payload = Object.fromEntries((await request.formData()).entries());
    }
  } catch {
    return json({ ok: false, error: 'bad_request' }, 400);
  }

  // Honeypot: real users never fill a hidden field.
  if (typeof payload.company === 'string' && payload.company.trim() !== '') {
    return json({ ok: true });
  }

  const email = String(payload.email ?? '').trim().toLowerCase();
  const note = String(payload.note ?? '').slice(0, 500);
  const tool = String(payload.tool ?? '').slice(0, 80);

  if (!EMAIL_RE.test(email) || email.length > 200) {
    return json({ ok: false, error: 'invalid_email' }, 400);
  }

  if (!env.DB) {
    return json({ ok: false, error: 'not_configured' }, 503);
  }

  const now = new Date().toISOString();
  try {
    await env.DB.prepare(
      `INSERT INTO pro_interest (email, note, tool, user_agent, referer, created_at)
       VALUES (?1, ?2, ?3, ?4, ?5, ?6)
       ON CONFLICT(email) DO UPDATE SET
         note = excluded.note,
         tool = excluded.tool,
         updated_at = excluded.created_at`,
    )
      .bind(
        email,
        note,
        tool,
        (request.headers.get('user-agent') ?? '').slice(0, 300),
        (request.headers.get('referer') ?? '').slice(0, 300),
        now,
      )
      .run();
  } catch {
    return json({ ok: false, error: 'storage_error' }, 500);
  }

  return json({ ok: true });
};

// Other methods get an automatic 405 from Pages since only onRequestPost is exported.

function json(data: unknown, status = 200): Response {
  return new Response(JSON.stringify(data), {
    status,
    headers: { 'content-type': 'application/json; charset=utf-8' },
  });
}
