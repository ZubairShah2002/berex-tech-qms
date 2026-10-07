import { config } from './config.js';
import { createPool, migrate } from './db.js';
import { ensureInitialAdmin } from './auth.js';
import { buildApp } from './app.js';

const pool = createPool();
await migrate(pool);
const app = await buildApp({ db: pool });
await ensureInitialAdmin(pool, app.log);

const shutdown = async (signal: string) => {
  app.log.info(`${signal} received, shutting down`);
  await app.close();
  await pool.end();
  process.exit(0);
};
process.on('SIGTERM', () => void shutdown('SIGTERM'));
process.on('SIGINT', () => void shutdown('SIGINT'));

await app.listen({ host: config.host, port: config.port });
