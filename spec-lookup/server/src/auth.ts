import crypto from 'node:crypto';
import fs from 'node:fs';
import path from 'node:path';
import { promisify } from 'node:util';
import type { FastifyReply, FastifyRequest } from 'fastify';
import type { Queryable } from './db.js';
import { config } from './config.js';
import { forbidden, unauthorized } from './errors.js';

const scrypt = promisify(crypto.scrypt) as (
  password: string, salt: Buffer, keylen: number, options: crypto.ScryptOptions,
) => Promise<Buffer>;

export const SESSION_COOKIE = 'sl_session';

export type Role = 'admin' | 'qc';

export interface AuthUser {
  id: string;
  userId: string;
  displayName: string;
  role: Role;
  canImport: boolean;
}

declare module 'fastify' {
  interface FastifyRequest {
    user: AuthUser | null;
  }
}

// ---- Password hashing (scrypt, per-user random salt) ----

const SCRYPT = { N: 32768, r: 8, p: 1, keylen: 64 };

export async function hashPassword(password: string): Promise<string> {
  const salt = crypto.randomBytes(16);
  const key = await scrypt(password, salt, SCRYPT.keylen, { N: SCRYPT.N, r: SCRYPT.r, p: SCRYPT.p, maxmem: 64 * 1024 * 1024 });
  return `scrypt$${SCRYPT.N}$${SCRYPT.r}$${SCRYPT.p}$${salt.toString('base64')}$${key.toString('base64')}`;
}

export async function verifyPassword(password: string, stored: string): Promise<boolean> {
  const parts = stored.split('$');
  if (parts.length !== 6 || parts[0] !== 'scrypt') return false;
  const [, n, r, p, saltB64, keyB64] = parts;
  const expected = Buffer.from(keyB64, 'base64');
  const key = await scrypt(password, Buffer.from(saltB64, 'base64'), expected.length, {
    N: Number(n), r: Number(r), p: Number(p), maxmem: 64 * 1024 * 1024,
  });
  return key.length === expected.length && crypto.timingSafeEqual(key, expected);
}

/** Minimum password policy. Returns an error message or null. */
export function passwordProblem(password: string): string | null {
  if (password.length < 8) return 'Password must be at least 8 characters.';
  if (password.length > 200) return 'Password is too long.';
  return null;
}

// ---- Sessions (opaque random token in an HttpOnly cookie; only its hash is stored) ----

const hashToken = (token: string) => crypto.createHash('sha256').update(token).digest('hex');

export async function createSession(db: Queryable, userId: string): Promise<{ token: string; expiresAt: Date }> {
  const token = crypto.randomBytes(32).toString('base64url');
  const expiresAt = new Date(Date.now() + config.sessionTtlHours * 3600_000);
  await db.query('INSERT INTO sessions (id, user_id, expires_at) VALUES ($1, $2, $3)', [hashToken(token), userId, expiresAt]);
  return { token, expiresAt };
}

export async function deleteSession(db: Queryable, token: string): Promise<void> {
  await db.query('DELETE FROM sessions WHERE id = $1', [hashToken(token)]);
}

export async function userFromSession(db: Queryable, token: string): Promise<AuthUser | null> {
  const { rows } = await db.query(
    `UPDATE sessions s
        SET last_seen_at = now(),
            expires_at = GREATEST(s.expires_at, now() + make_interval(hours => $2))
       FROM users u
      WHERE s.id = $1 AND s.expires_at > now() AND u.id = s.user_id AND u.status = 'active'
     RETURNING u.id, u.user_id, u.display_name, u.role, u.can_import`,
    [hashToken(token), config.sessionTtlHours],
  );
  const u = rows[0];
  if (!u) return null;
  return { id: u.id, userId: u.user_id, displayName: u.display_name || u.user_id, role: u.role, canImport: u.role === 'admin' || u.can_import };
}

export function setSessionCookie(reply: FastifyReply, token: string, expiresAt: Date): void {
  reply.setCookie(SESSION_COOKIE, token, {
    httpOnly: true,
    secure: config.cookieSecure,
    sameSite: 'lax',
    path: '/',
    expires: expiresAt,
  });
}

export function clearSessionCookie(reply: FastifyReply): void {
  reply.clearCookie(SESSION_COOKIE, { path: '/' });
}

// ---- Route guards (always enforced server-side) ----

export async function requireAuth(req: FastifyRequest): Promise<void> {
  if (!req.user) throw unauthorized();
}

export async function requireAdmin(req: FastifyRequest): Promise<void> {
  if (!req.user) throw unauthorized();
  if (req.user.role !== 'admin') throw forbidden('Administrator access is required.');
}

export async function requireImport(req: FastifyRequest): Promise<void> {
  if (!req.user) throw unauthorized();
  if (!req.user.canImport) throw forbidden('Your account is not permitted to import products. Ask an administrator.');
}

// ---- Login throttling (per IP + user ID, in memory) ----

const failures = new Map<string, { count: number; first: number; lockedUntil: number }>();
const WINDOW_MS = 15 * 60_000;
const MAX_FAILURES = 5;
const LOCK_MS = 15 * 60_000;

export function loginLockedFor(key: string): number {
  const f = failures.get(key);
  if (!f) return 0;
  const now = Date.now();
  if (f.lockedUntil > now) return f.lockedUntil - now;
  if (now - f.first > WINDOW_MS) failures.delete(key);
  return 0;
}

export function recordLoginFailure(key: string): void {
  const now = Date.now();
  const f = failures.get(key);
  if (!f || now - f.first > WINDOW_MS) {
    failures.set(key, { count: 1, first: now, lockedUntil: 0 });
    return;
  }
  f.count += 1;
  if (f.count >= MAX_FAILURES) f.lockedUntil = now + LOCK_MS;
  if (failures.size > 10_000) failures.clear(); // bound memory under abuse
}

export function clearLoginFailures(key: string): void {
  failures.delete(key);
}

/** Creates the first administrator when the users table is empty. */
export async function ensureInitialAdmin(db: Queryable, log: { warn: (m: string) => void; info: (m: string) => void }): Promise<void> {
  const { rows } = await db.query('SELECT count(*)::int AS n FROM users');
  if (rows[0].n > 0) return;
  let password = config.adminPassword;
  if (!password) {
    password = crypto.randomBytes(12).toString('base64url');
    log.warn(`No ADMIN_PASSWORD set. Created administrator "${config.adminUserId}" with one-time password: ${password} — change it after logging in.`);
    if (!config.databaseUrl) {
      // Built-in mode runs in a console window that may be closed; keep the password in a file too.
      const file = path.join(config.dataDir, 'FIRST-LOGIN.txt');
      fs.writeFileSync(file, `User ID: ${config.adminUserId}\r\nPassword: ${password}\r\n\r\nLog in, change this password (click your user ID, top right), then delete this file.\r\n`);
      log.warn(`The first login details were also saved to ${file}`);
    }
  } else {
    const problem = passwordProblem(password);
    if (problem) throw new Error(`ADMIN_PASSWORD: ${problem}`);
    log.info(`Created administrator "${config.adminUserId}" from ADMIN_PASSWORD.`);
  }
  await db.query(
    `INSERT INTO users (user_id, display_name, password_hash, role, can_import) VALUES ($1, $2, $3, 'admin', true)`,
    [config.adminUserId, 'Administrator', await hashPassword(password)],
  );
}
