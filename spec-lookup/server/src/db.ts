import pg from 'pg';
import { config } from './config.js';
import { migrations } from './migrations.js';

export type Db = pg.Pool;
export type Tx = pg.PoolClient;
/** Anything that can run a query: the pool or a transaction client. */
export type Queryable = Pick<pg.Pool, 'query'>;

/** Opens the configured database: a PostgreSQL server, or the built-in one in DATA_DIR. */
export async function openDatabase(): Promise<pg.Pool> {
  if (config.databaseUrl) return createPool(config.databaseUrl, config.dbSchema);
  const { createEmbeddedPool } = await import('./embedded.js');
  return createEmbeddedPool(config.dataDir, config.dbSchema);
}

export function createPool(databaseUrl = config.databaseUrl, schema = config.dbSchema): pg.Pool {
  const ssl =
    config.dbSsl === 'true' ? { rejectUnauthorized: true }
    : config.dbSsl === 'no-verify' ? { rejectUnauthorized: false }
    : undefined;
  const pool = new pg.Pool({
    connectionString: databaseUrl,
    ssl,
    max: 10,
    idleTimeoutMillis: 30_000,
  });
  // Set per connection with SQL rather than a startup option: some hosted
  // Postgres providers reject startup options. The query is queued ahead of
  // any query the caller sends. public stays on the path for pg_trgm.
  pool.on('connect', (client) => {
    client.query(`SET search_path TO ${schema}, public`).catch(() => undefined);
  });
  return pool;
}

export async function withTx<T>(pool: pg.Pool, fn: (tx: Tx) => Promise<T>): Promise<T> {
  const client = await pool.connect();
  try {
    await client.query('BEGIN');
    const result = await fn(client);
    await client.query('COMMIT');
    return result;
  } catch (err) {
    await client.query('ROLLBACK').catch(() => undefined);
    throw err;
  } finally {
    client.release();
  }
}

/** Creates the schema and applies pending migrations. Safe to run on every start. */
export async function migrate(pool: pg.Pool, schema = config.dbSchema): Promise<void> {
  const client = await pool.connect();
  try {
    // Serialize concurrent starts (e.g. two instances booting at once).
    await client.query('SELECT pg_advisory_lock(hashtext($1))', [`migrate:${schema}`]);
    await client.query(`CREATE SCHEMA IF NOT EXISTS ${schema}`);
    await client.query('CREATE EXTENSION IF NOT EXISTS pg_trgm WITH SCHEMA public');
    await client.query(`CREATE TABLE IF NOT EXISTS ${schema}.schema_migrations (
      version int PRIMARY KEY, applied_at timestamptz NOT NULL DEFAULT now())`);
    const { rows } = await client.query<{ version: number }>(`SELECT version FROM ${schema}.schema_migrations`);
    const applied = new Set(rows.map((r) => r.version));
    for (const m of migrations) {
      if (applied.has(m.version)) continue;
      await client.query('BEGIN');
      try {
        await client.query(m.sql);
        await client.query(`INSERT INTO ${schema}.schema_migrations (version) VALUES ($1)`, [m.version]);
        await client.query('COMMIT');
      } catch (err) {
        await client.query('ROLLBACK');
        throw err;
      }
    }
  } finally {
    await client.query('SELECT pg_advisory_unlock(hashtext($1))', [`migrate:${schema}`]).catch(() => undefined);
    client.release();
  }
}
