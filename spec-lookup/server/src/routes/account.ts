import type { FastifyInstance } from 'fastify';
import { z } from 'zod';
import {
  SESSION_COOKIE, clearLoginFailures, clearSessionCookie, createSession, deleteSession, hashPassword,
  loginLockedFor, passwordProblem, recordLoginFailure, requireAdmin, requireAuth, setSessionCookie, verifyPassword,
} from '../auth.js';
import { AppError, badRequest, conflict, notFound } from '../errors.js';
import { writeAudit } from '../products.js';
import { getSettings, saveSettings, settingsSchema } from '../settings.js';
import { parseId } from './public.js';

// Used to keep failed-login timing similar whether or not the user exists.
const DUMMY_HASH = 'scrypt$32768$8$1$AAAAAAAAAAAAAAAAAAAAAA==$' + Buffer.alloc(64).toString('base64');

const userIdSchema = z.string().trim().min(2, 'User ID must be at least 2 characters').max(50)
  .regex(/^[A-Za-z0-9._@-]+$/, 'User ID may only contain letters, digits and . _ @ -');

function toUser(r: Record<string, any>) {
  return {
    id: r.id, userId: r.user_id, displayName: r.display_name, role: r.role, canImport: r.role === 'admin' || r.can_import,
    status: r.status, createdAt: r.created_at.toISOString(), lastLoginAt: r.last_login_at ? r.last_login_at.toISOString() : null,
  };
}

export async function accountRoutes(app: FastifyInstance) {
  const db = app.db;

  // ---- Authentication ----

  app.post('/api/auth/login', async (req, reply) => {
    const body = z.object({ userId: z.string().max(100), password: z.string().max(200) }).safeParse(req.body);
    if (!body.success || !body.data.userId.trim() || !body.data.password) throw badRequest('Enter your user ID and password.');
    const userId = body.data.userId.trim();
    const key = `${req.ip}|${userId.toLowerCase()}`;
    const locked = loginLockedFor(key);
    if (locked > 0) {
      throw new AppError(429, 'TOO_MANY_ATTEMPTS', `Too many failed attempts. Try again in ${Math.ceil(locked / 60000)} minutes.`);
    }
    const { rows } = await db.query('SELECT id, password_hash, status FROM users WHERE lower(user_id) = lower($1)', [userId]);
    const u = rows[0];
    const ok = await verifyPassword(body.data.password, u?.password_hash ?? DUMMY_HASH);
    if (!u || !ok || u.status !== 'active') {
      recordLoginFailure(key);
      throw new AppError(401, 'INVALID_LOGIN', 'Incorrect user ID or password.');
    }
    clearLoginFailures(key);
    await db.query('UPDATE users SET last_login_at = now() WHERE id = $1', [u.id]);
    await db.query('DELETE FROM sessions WHERE expires_at < now()');
    const session = await createSession(db, u.id);
    setSessionCookie(reply, session.token, session.expiresAt);
    const me = await db.query('SELECT * FROM users WHERE id = $1', [u.id]);
    return toUser(me.rows[0]);
  });

  app.post('/api/auth/logout', async (req, reply) => {
    const token = req.cookies[SESSION_COOKIE];
    if (token) await deleteSession(db, token);
    clearSessionCookie(reply);
    return { ok: true };
  });

  app.get('/api/auth/me', async (req) => req.user);

  app.post('/api/auth/change-password', { preHandler: requireAuth }, async (req) => {
    const body = z.object({ currentPassword: z.string().max(200), newPassword: z.string().max(200) }).safeParse(req.body);
    if (!body.success) throw badRequest('Enter your current and new password.');
    const problem = passwordProblem(body.data.newPassword);
    if (problem) throw badRequest(problem);
    const { rows } = await db.query('SELECT password_hash FROM users WHERE id = $1', [req.user!.id]);
    if (!(await verifyPassword(body.data.currentPassword, rows[0].password_hash))) throw badRequest('Current password is incorrect.');
    await db.query('UPDATE users SET password_hash = $2, updated_at = now() WHERE id = $1', [req.user!.id, await hashPassword(body.data.newPassword)]);
    // Sign out other devices.
    const token = req.cookies[SESSION_COOKIE];
    await db.query(`DELETE FROM sessions WHERE user_id = $1 AND id <> encode(sha256(convert_to($2, 'UTF8')), 'hex')`, [req.user!.id, token ?? '']);
    await writeAudit(db, req.user!, 'user.password', null, `${req.user!.userId} changed their password.`);
    return { ok: true };
  });

  // ---- User management (admin) ----

  app.get('/api/users', { preHandler: requireAdmin }, async () => {
    const { rows } = await db.query('SELECT * FROM users ORDER BY lower(user_id)');
    return rows.map(toUser);
  });

  app.post('/api/users', { preHandler: requireAdmin }, async (req, reply) => {
    const body = z.object({
      userId: userIdSchema,
      displayName: z.string().trim().max(100).default(''),
      role: z.enum(['admin', 'qc']),
      canImport: z.boolean().default(false),
      password: z.string().max(200),
    }).safeParse(req.body);
    if (!body.success) throw badRequest(body.error.issues[0]?.message ?? 'Invalid user.');
    const problem = passwordProblem(body.data.password);
    if (problem) throw badRequest(problem);
    try {
      const { rows } = await db.query(
        `INSERT INTO users (user_id, display_name, password_hash, role, can_import) VALUES ($1, $2, $3, $4, $5) RETURNING *`,
        [body.data.userId, body.data.displayName, await hashPassword(body.data.password), body.data.role, body.data.canImport]);
      await writeAudit(db, req.user!, 'user.create', null, `${req.user!.userId} created user ${body.data.userId} (${body.data.role}).`);
      reply.code(201);
      return toUser(rows[0]);
    } catch (err) {
      if ((err as { code?: string }).code === '23505') throw conflict('DUPLICATE_USER', 'That user ID already exists.');
      throw err;
    }
  });

  app.patch('/api/users/:id', { preHandler: requireAdmin }, async (req) => {
    const id = parseId((req.params as { id: string }).id, 'User');
    const body = z.object({
      displayName: z.string().trim().max(100).optional(),
      role: z.enum(['admin', 'qc']).optional(),
      canImport: z.boolean().optional(),
      status: z.enum(['active', 'disabled']).optional(),
      password: z.string().max(200).optional(),
    }).safeParse(req.body);
    if (!body.success) throw badRequest(body.error.issues[0]?.message ?? 'Invalid user.');
    const d = body.data;
    if (id === req.user!.id && (d.role === 'qc' || d.status === 'disabled')) {
      throw badRequest('You cannot remove your own administrator access or disable your own account.');
    }
    if (d.password !== undefined) {
      const problem = passwordProblem(d.password);
      if (problem) throw badRequest(problem);
    }
    const { rows: cur } = await db.query('SELECT * FROM users WHERE id = $1', [id]);
    if (!cur[0]) throw notFound('User');
    const { rows } = await db.query(
      `UPDATE users SET display_name = COALESCE($2, display_name), role = COALESCE($3, role),
              can_import = COALESCE($4, can_import), status = COALESCE($5, status),
              password_hash = COALESCE($6, password_hash), updated_at = now()
        WHERE id = $1 RETURNING *`,
      [id, d.displayName ?? null, d.role ?? null, d.canImport ?? null, d.status ?? null,
        d.password !== undefined ? await hashPassword(d.password) : null]);
    if (d.status === 'disabled' || d.password !== undefined) await db.query('DELETE FROM sessions WHERE user_id = $1', [id]);
    const what = [
      d.displayName !== undefined && 'name', d.role && `role=${d.role}`, d.canImport !== undefined && `import=${d.canImport}`,
      d.status && `status=${d.status}`, d.password !== undefined && 'password reset',
    ].filter(Boolean).join(', ');
    await writeAudit(db, req.user!, 'user.update', null, `${req.user!.userId} updated user ${cur[0].user_id}: ${what || 'no changes'}.`);
    return toUser(rows[0]);
  });

  // ---- Settings & audit log ----

  app.get('/api/settings', { preHandler: requireAdmin }, async () => getSettings(db));

  app.put('/api/settings', { preHandler: requireAdmin }, async (req) => {
    const body = settingsSchema.safeParse(req.body);
    if (!body.success) throw badRequest(body.error.issues[0]?.message ?? 'Invalid settings.');
    await saveSettings(db, body.data);
    await writeAudit(db, req.user!, 'settings.update', null, `${req.user!.userId} updated settings.`, body.data);
    return getSettings(db);
  });

  app.get('/api/audit', { preHandler: requireAuth }, async (req) => {
    const q = z.object({
      productId: z.string().uuid().optional(),
      page: z.coerce.number().int().min(1).max(100000).default(1),
      pageSize: z.coerce.number().int().min(1).max(200).default(50),
    }).safeParse(req.query);
    if (!q.success) throw badRequest('Invalid parameters.');
    // QC users may see the log for a product; the full log is for administrators.
    if (!q.data.productId && req.user!.role !== 'admin') await requireAdmin(req);
    const { productId, page, pageSize } = q.data;
    const { rows } = await db.query(
      `SELECT id, at, user_name, action, product_id, product_code, detail FROM audit_log
        WHERE ($1::uuid IS NULL OR product_id = $1) ORDER BY at DESC, id DESC LIMIT $2 OFFSET $3`,
      [productId ?? null, pageSize, (page - 1) * pageSize]);
    return rows.map((r) => ({
      id: Number(r.id), at: r.at.toISOString(), userName: r.user_name, action: r.action,
      productId: r.product_id, productCode: r.product_code, detail: r.detail,
    }));
  });
}
