import fs from 'node:fs';
import path from 'node:path';
import Fastify, { type FastifyInstance } from 'fastify';
import cookie from '@fastify/cookie';
import helmet from '@fastify/helmet';
import multipart from '@fastify/multipart';
import fastifyStatic from '@fastify/static';
import type pg from 'pg';
import { config } from './config.js';
import { SESSION_COOKIE, userFromSession } from './auth.js';
import { AppError } from './errors.js';
import { publicRoutes } from './routes/public.js';
import { manageRoutes } from './routes/manage.js';
import { accountRoutes } from './routes/account.js';

declare module 'fastify' {
  interface FastifyInstance {
    db: pg.Pool;
  }
}

export interface AppOptions { db: pg.Pool; webDist?: string | null; logger?: boolean }

export async function buildApp(opts: AppOptions): Promise<FastifyInstance> {
  const app = Fastify({
    logger: opts.logger === false ? false : { level: config.logLevel },
    trustProxy: true,
    bodyLimit: 2 * 1024 * 1024,
  });
  app.decorate('db', opts.db);
  app.decorateRequest('user', null);

  await app.register(helmet, {
    contentSecurityPolicy: {
      directives: {
        defaultSrc: ["'self'"],
        imgSrc: ["'self'", 'data:', 'blob:'],
        styleSrc: ["'self'", "'unsafe-inline'"],
        scriptSrc: ["'self'"],
        connectSrc: ["'self'"],
        objectSrc: ["'none'"],
        frameSrc: ["'self'"],
        workerSrc: ["'self'"],
        manifestSrc: ["'self'"],
        baseUri: ["'self'"],
        formAction: ["'self'"],
        frameAncestors: ["'self'"],
        // Over plain http (local network), upgrading requests to https would break the page.
        upgradeInsecureRequests: config.cookieSecure ? [] : null,
      },
    },
    crossOriginEmbedderPolicy: false,
    strictTransportSecurity: config.cookieSecure ? undefined : false,
  });
  await app.register(cookie);
  await app.register(multipart);

  // Identify the logged-in user (if any) and block cross-site writes.
  app.addHook('onRequest', async (req) => {
    if (!req.url.startsWith('/api/')) return;
    if (req.method !== 'GET' && req.method !== 'HEAD') {
      const origin = req.headers.origin;
      if (origin) {
        let originHost = '';
        try { originHost = new URL(origin).host; } catch { /* invalid origin */ }
        const host = (req.headers['x-forwarded-host'] as string | undefined)?.split(',')[0].trim() || req.headers.host;
        if (originHost !== host) throw new AppError(403, 'CROSS_SITE', 'Cross-site request blocked.');
      }
    }
    const token = req.cookies[SESSION_COOKIE];
    if (token && token.length < 200) req.user = await userFromSession(opts.db, token);
  });

  app.addHook('onSend', async (req, reply) => {
    if (req.url.startsWith('/api/') && !req.url.startsWith('/api/files/')) {
      reply.header('Cache-Control', 'no-store');
    }
  });

  app.setErrorHandler((err: Error & { statusCode?: number; code?: string; validation?: unknown }, req, reply) => {
    if (err instanceof AppError) {
      return reply.code(err.statusCode).send({ error: { code: err.code, message: err.message, details: err.details } });
    }
    if (err.name === 'ZodError') {
      return reply.code(400).send({ error: { code: 'VALIDATION_ERROR', message: 'Invalid data.' } });
    }
    if (err.statusCode && err.statusCode < 500) {
      const message = err.code === 'FST_ERR_CTP_BODY_TOO_LARGE' ? 'Request is too large.' : 'Invalid request.';
      return reply.code(err.statusCode).send({ error: { code: err.code ?? 'BAD_REQUEST', message } });
    }
    req.log.error(err);
    return reply.code(500).send({ error: { code: 'SERVER_ERROR', message: 'Something went wrong. Please try again.' } });
  });

  app.get('/health', async () => {
    await opts.db.query('SELECT 1');
    return { status: 'ok' };
  });

  await app.register(publicRoutes);
  await app.register(manageRoutes);
  await app.register(accountRoutes);

  // ---- Web app (built React PWA) ----
  const webDist = opts.webDist === undefined ? config.webDist : opts.webDist;
  const indexFile = webDist ? path.join(webDist, 'index.html') : null;
  if (webDist && indexFile && fs.existsSync(indexFile)) {
    await app.register(fastifyStatic, {
      root: webDist,
      wildcard: false,
      index: false,
      setHeaders: (res, filePath) => {
        if (filePath.includes(`${path.sep}assets${path.sep}`)) res.header('Cache-Control', 'public, max-age=31536000, immutable');
        else res.header('Cache-Control', 'no-cache');
      },
    });
    const indexHtml = fs.readFileSync(indexFile);
    app.setNotFoundHandler((req, reply) => {
      if (req.method === 'GET' && !req.url.startsWith('/api/') && req.headers.accept?.includes('text/html')) {
        return reply.header('Cache-Control', 'no-cache').type('text/html').send(indexHtml);
      }
      return reply.code(404).send({ error: { code: 'NOT_FOUND', message: 'Not found.' } });
    });
  } else {
    app.setNotFoundHandler((_req, reply) => reply.code(404).send({ error: { code: 'NOT_FOUND', message: 'Not found.' } }));
  }

  return app;
}
