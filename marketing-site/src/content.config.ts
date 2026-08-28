import { defineCollection, z } from 'astro:content';
import { glob } from 'astro/loaders';

const tools = defineCollection({
  loader: glob({ pattern: '**/*.md', base: './src/content/tools' }),
  schema: z.object({
    name: z.string(),
    tagline: z.string(),
    description: z.string(),
    category: z.enum(['engineering']), // widen later (e.g. 'seo-dev') only under a separate brand — see business plan Section 5
    status: z.enum(['live', 'coming-soon']).default('coming-soon'),
    version: z.string().optional(),
    releaseDate: z.date().optional(),
    features: z.array(z.string()),
    requirements: z.array(z.string()),
    downloadUrl: z.string().optional(),
    sourceUrl: z.string(),
    docsUrl: z.string().optional(),
    screenshot: z.string().optional(), // path under /public, e.g. "/screenshots/foo.png"
    screenshotAlt: z.string().optional(),
    protocol: z.string().optional(), // e.g. "Modbus RTU/TCP" — shown as a spec-sheet row
  }),
});

const blog = defineCollection({
  loader: glob({ pattern: '**/*.md', base: './src/content/blog' }),
  schema: z.object({
    title: z.string(),
    description: z.string(),
    pubDate: z.date(),
    updatedDate: z.date().optional(),
    tags: z.array(z.string()).default([]),
    tool: z.string().optional(), // slug of a related tool, e.g. "modbus-simulator"
    draft: z.boolean().default(false),
  }),
});

export const collections = { tools, blog };
