import fs from 'node:fs';
import { PGlite, type Results } from '@electric-sql/pglite';
import { pg_trgm } from '@electric-sql/pglite/contrib/pg_trgm';
import type pg from 'pg';

/**
 * Built-in database for running on a single office PC without installing
 * PostgreSQL. PGlite is PostgreSQL compiled to WebAssembly; data is saved in
 * a normal folder. It exposes the small part of the pg.Pool API this app uses.
 *
 * PGlite has a single connection, so access is serialized: a transaction
 * (connect() … release()) holds the connection until it is released.
 */
export async function createEmbeddedPool(dataDir: string, schema: string): Promise<pg.Pool> {
  fs.mkdirSync(dataDir, { recursive: true });
  const db = await PGlite.create({ dataDir, extensions: { pg_trgm } });

  let tail: Promise<void> = Promise.resolve();
  const acquire = (): Promise<() => void> => {
    let release!: () => void;
    const next = new Promise<void>((r) => { release = r; });
    const ready = tail.then(() => release);
    tail = tail.then(() => next);
    return ready;
  };

  const toResult = (r: Results<Record<string, unknown>>) => {
    // bytea comes back as Uint8Array; the app expects Node Buffers.
    for (const row of r.rows) {
      for (const [k, v] of Object.entries(row)) if (v instanceof Uint8Array && !Buffer.isBuffer(v)) row[k] = Buffer.from(v);
    }
    return { rows: r.rows, rowCount: r.affectedRows ?? r.rows.length, fields: r.fields };
  };

  const run = async (text: string, params?: unknown[]) => {
    const body = text.trim().replace(/;\s*$/, '');
    if ((!params || params.length === 0) && body.includes(';')) {
      // Several statements (migrations): use the simple protocol.
      const results = await db.exec(text);
      return toResult(results[results.length - 1] ?? { rows: [], fields: [] });
    }
    return toResult(await db.query(text, params as unknown[]));
  };

  await db.exec(`CREATE SCHEMA IF NOT EXISTS ${schema}; SET search_path TO ${schema}, public;`);

  const pool = {
    async query(text: string, params?: unknown[]) {
      const release = await acquire();
      try { return await run(text, params); } finally { release(); }
    },
    async connect() {
      const release = await acquire();
      let released = false;
      return {
        query: (text: string, params?: unknown[]) => run(text, params),
        release: () => { if (!released) { released = true; release(); } },
      };
    },
    on() { return pool; },
    async end() { await db.close(); },
  };
  return pool as unknown as pg.Pool;
}
