import { z } from 'zod';
import type { Queryable } from './db.js';

export const settingsSchema = z.object({
  publicRevisionHistory: z.boolean(),
  siteName: z.string().trim().min(1).max(100),
});

export type Settings = z.infer<typeof settingsSchema>;

const DEFAULTS: Settings = { publicRevisionHistory: true, siteName: 'Berex Tech' };

export async function getSettings(db: Queryable): Promise<Settings> {
  const { rows } = await db.query('SELECT key, value FROM settings');
  const out: Record<string, unknown> = { ...DEFAULTS };
  for (const r of rows) if (r.key in DEFAULTS) out[r.key] = r.value;
  return out as Settings;
}

export async function saveSettings(db: Queryable, s: Settings): Promise<void> {
  for (const [key, value] of Object.entries(s)) {
    await db.query(
      'INSERT INTO settings (key, value) VALUES ($1, $2) ON CONFLICT (key) DO UPDATE SET value = EXCLUDED.value',
      [key, JSON.stringify(value)]);
  }
}
