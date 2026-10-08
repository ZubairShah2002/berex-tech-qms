import { config } from './config.js';
import os from 'node:os';
import { openDatabase, migrate } from './db.js';
import { ensureInitialAdmin } from './auth.js';
import { buildApp } from './app.js';

const pool = await openDatabase();
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

// Show the addresses other PCs and phones on the network can use.
const addresses = Object.values(os.networkInterfaces()).flat()
  .filter((a) => a && a.family === 'IPv4' && !a.internal)
  .map((a) => `http://${a!.address}:${config.port}`);
console.log('');
console.log('  Product Specification Lookup is running.');
console.log(`  On this PC:            http://localhost:${config.port}`);
for (const a of addresses) console.log(`  From other PCs/phones: ${a}`);
console.log(config.databaseUrl ? '  Database: PostgreSQL server' : `  Data folder: ${config.dataDir}`);
console.log('  Keep this window open. Close it to stop the app.');
console.log('');
