/**
 * Runtime configuration, read once from environment variables.
 * See spec-lookup/.env.example for documentation of each variable.
 */

function str(name: string, fallback?: string): string {
  const v = process.env[name];
  if (v === undefined || v.trim() === '') {
    if (fallback === undefined) throw new Error(`Missing required environment variable ${name}`);
    return fallback;
  }
  return v.trim();
}

function int(name: string, fallback: number): number {
  const v = process.env[name];
  if (!v) return fallback;
  const n = Number.parseInt(v, 10);
  if (!Number.isFinite(n) || n <= 0) throw new Error(`Environment variable ${name} must be a positive integer`);
  return n;
}

function bool(name: string, fallback: boolean): boolean {
  const v = process.env[name]?.trim().toLowerCase();
  if (!v) return fallback;
  return v === '1' || v === 'true' || v === 'yes';
}

const isProduction = (process.env.NODE_ENV ?? '').toLowerCase() === 'production';

export const config = {
  isProduction,
  host: str('HOST', '0.0.0.0'),
  port: int('PORT', 8080),
  databaseUrl: str('DATABASE_URL', 'postgres://postgres:postgres@localhost:5432/spec_lookup'),
  /** Postgres schema holding all tables. Lets the app share a database with Berex Tech QMS. */
  dbSchema: str('DB_SCHEMA', 'spec_lookup'),
  /** "false" (default), "true" (verify certificate) or "no-verify" (encrypt, skip verification). */
  dbSsl: str('DATABASE_SSL', 'false').toLowerCase(),
  sessionTtlHours: int('SESSION_TTL_HOURS', 12),
  cookieSecure: bool('COOKIE_SECURE', isProduction),
  adminUserId: str('ADMIN_USER_ID', 'admin'),
  adminPassword: process.env.ADMIN_PASSWORD?.trim() || '',
  maxFileMb: int('MAX_FILE_MB', 10),
  maxImportMb: int('MAX_IMPORT_MB', 10),
  webDist: str('WEB_DIST', new URL('../../web/dist', import.meta.url).pathname),
  logLevel: str('LOG_LEVEL', isProduction ? 'info' : 'debug'),
};

if (!/^[a-z_][a-z0-9_]*$/.test(config.dbSchema)) {
  throw new Error('DB_SCHEMA may only contain lowercase letters, digits and underscores');
}
