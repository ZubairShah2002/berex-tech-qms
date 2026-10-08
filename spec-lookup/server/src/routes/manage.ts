import crypto from 'node:crypto';
import type { FastifyInstance, FastifyRequest } from 'fastify';
import { z } from 'zod';
import { withTx } from '../db.js';
import { requireAdmin, requireAuth, requireImport } from '../auth.js';
import { config } from '../config.js';
import { badRequest, conflict, notFound } from '../errors.js';
import {
  contentToInput, createProduct, getProductView, parseProductInput, recordFileRevision,
  revisionMetaSchema, updateProduct, writeAudit, actorName,
} from '../products.js';
import { buildPreview, commitImport, type ImportPreview } from '../importer.js';
import { parseId } from './public.js';

const FILE_KINDS = ['photo', 'drawing', 'dimension_drawing', 'reference'] as const;
const KIND_LABEL: Record<string, string> = {
  photo: 'product photo', drawing: 'technical drawing', dimension_drawing: 'dimension drawing', reference: 'reference image',
};

/** Detects the real file type from its first bytes; the browser-supplied type is not trusted. */
export function sniffFileType(buf: Buffer): string | null {
  if (buf.length >= 3 && buf[0] === 0xff && buf[1] === 0xd8 && buf[2] === 0xff) return 'image/jpeg';
  if (buf.length >= 8 && buf.subarray(0, 8).equals(Buffer.from([0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a]))) return 'image/png';
  if (buf.length >= 12 && buf.toString('ascii', 0, 4) === 'RIFF' && buf.toString('ascii', 8, 12) === 'WEBP') return 'image/webp';
  if (buf.length >= 6 && (buf.toString('ascii', 0, 6) === 'GIF87a' || buf.toString('ascii', 0, 6) === 'GIF89a')) return 'image/gif';
  if (buf.length >= 5 && buf.toString('ascii', 0, 5) === '%PDF-') return 'application/pdf';
  return null;
}

const EXT_FOR: Record<string, string> = { 'image/jpeg': '.jpg', 'image/png': '.png', 'image/webp': '.webp', 'image/gif': '.gif', 'application/pdf': '.pdf' };

function sanitizeName(name: string): string {
  return name.replace(/[\\/]/g, '_').replace(/[\u0000-\u001f\u007f"<>|:*?]/g, '').trim().slice(0, 150) || 'file';
}

function safeFileName(name: string, contentType: string): string {
  const base = sanitizeName(name);
  return /\.[a-z0-9]{2,5}$/i.test(base) ? base : base + EXT_FOR[contentType];
}

async function readSingleFile(req: FastifyRequest, maxBytes: number) {
  const fields: Record<string, string> = {};
  let file: { buffer: Buffer; filename: string } | null = null;
  try {
    for await (const part of req.parts({ limits: { fileSize: maxBytes, files: 1, fields: 10, fieldSize: 2000 } })) {
      if (part.type === 'file') {
        const buffer = await part.toBuffer();
        if (part.file.truncated) throw badRequest(`File is too large. Maximum size is ${Math.round(maxBytes / 1024 / 1024)} MB.`);
        file = { buffer, filename: part.filename };
      } else {
        fields[part.fieldname] = String(part.value ?? '');
      }
    }
  } catch (err) {
    const code = (err as { code?: string }).code;
    if (code === 'FST_REQ_FILE_TOO_LARGE') throw badRequest(`File is too large. Maximum size is ${Math.round(maxBytes / 1024 / 1024)} MB.`);
    if (code === 'FST_INVALID_MULTIPART_CONTENT_TYPE') throw badRequest('Expected a file upload.');
    throw err;
  }
  if (!file || file.buffer.length === 0) throw badRequest('No file was uploaded.');
  return { file, fields };
}

/** Endpoints that change data. Every one is guarded server-side. */
export async function manageRoutes(app: FastifyInstance) {
  const db = app.db;

  // ---- Products ----

  app.post('/api/products', { preHandler: requireAuth }, async (req, reply) => {
    const body = req.body as { product?: unknown; revisionLabel?: unknown; changeSummary?: unknown };
    const input = parseProductInput(body?.product);
    const meta = revisionMetaSchema.parse({ revisionLabel: body?.revisionLabel, changeSummary: body?.changeSummary });
    const id = await withTx(db, (tx) => createProduct(tx, input, req.user!, meta));
    reply.code(201);
    return getProductView(db, id);
  });

  app.put('/api/products/:id', { preHandler: requireAuth }, async (req) => {
    const id = parseId((req.params as { id: string }).id);
    const body = req.body as { product?: unknown; expectedRevision?: unknown; revisionLabel?: unknown; changeSummary?: unknown };
    const input = parseProductInput(body?.product);
    const expected = z.number().int().positive().safeParse(body?.expectedRevision);
    if (!expected.success) throw badRequest('expectedRevision is required.');
    const meta = revisionMetaSchema.parse({ revisionLabel: body?.revisionLabel, changeSummary: body?.changeSummary });
    const result = await withTx(db, (tx) => updateProduct(tx, id, input, req.user!, expected.data, meta));
    return { ...result, product: await getProductView(db, id) };
  });

  const setStatus = (status: 'active' | 'archived') => async (req: FastifyRequest) => {
    const id = parseId((req.params as { id: string }).id);
    await withTx(db, async (tx) => {
      const { rows } = await tx.query('SELECT product_code, status FROM products WHERE id = $1 FOR UPDATE', [id]);
      if (!rows[0]) throw notFound();
      if (rows[0].status === status) return;
      if (status === 'archived') {
        await tx.query(`UPDATE products SET status = 'archived', archived_at = now(), archived_by = $2 WHERE id = $1`, [id, req.user!.id]);
      } else {
        await tx.query(`UPDATE products SET status = 'active', archived_at = NULL, archived_by = NULL WHERE id = $1`, [id]);
      }
      await writeAudit(tx, req.user!, status === 'archived' ? 'product.archive' : 'product.restore',
        { id, code: rows[0].product_code },
        `${actorName(req.user!)} ${status === 'archived' ? 'archived' : 'restored'} ${rows[0].product_code}.`);
    });
    return getProductView(db, id);
  };
  app.post('/api/products/:id/archive', { preHandler: requireAuth }, setStatus('archived'));
  app.post('/api/products/:id/restore', { preHandler: requireAuth }, setStatus('active'));

  // Restoring an old revision creates a new revision with that content (history is never rewritten).
  app.post('/api/products/:id/revisions/:revisionId/restore', { preHandler: requireAdmin }, async (req) => {
    const id = parseId((req.params as { id: string }).id);
    const revisionId = parseId((req.params as { revisionId: string }).revisionId, 'Revision');
    const result = await withTx(db, async (tx) => {
      const { rows } = await tx.query(
        'SELECT revision_label, snapshot FROM product_revisions WHERE id = $1 AND product_id = $2', [revisionId, id]);
      if (!rows[0]) throw notFound('Revision');
      const input = contentToInput(rows[0].snapshot);
      return updateProduct(tx, id, input, req.user!, null, {
        changeSummary: `Restored content of ${rows[0].revision_label}`, auditAction: 'revision.restore',
      });
    });
    if (!result.changed) throw conflict('NO_CHANGES', 'The current specification is already identical to that revision.');
    return { ...result, product: await getProductView(db, id) };
  });

  // ---- Files (photos / drawings) ----

  app.post('/api/products/:id/files', { preHandler: requireAuth }, async (req, reply) => {
    const id = parseId((req.params as { id: string }).id);
    const { file, fields } = await readSingleFile(req, config.maxFileMb * 1024 * 1024);
    const kind = (FILE_KINDS as readonly string[]).includes(fields.kind) ? fields.kind : null;
    if (!kind) throw badRequest('Choose what kind of file this is (photo, drawing, dimension drawing or reference image).');
    const contentType = sniffFileType(file.buffer);
    if (!contentType) throw badRequest('Only JPEG, PNG, WebP, GIF images and PDF drawings are allowed.');
    const caption = fields.caption?.trim().slice(0, 300) || null;
    const fileName = safeFileName(file.filename || 'file', contentType);
    const sha = crypto.createHash('sha256').update(file.buffer).digest('hex');
    await withTx(db, async (tx) => {
      const { rows } = await tx.query('SELECT 1 FROM products WHERE id = $1 FOR UPDATE', [id]);
      if (!rows[0]) throw notFound();
      await tx.query(
        `INSERT INTO product_files (product_id, kind, caption, file_name, content_type, size_bytes, sha256, data, uploaded_by)
         VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9)`,
        [id, kind, caption, fileName, contentType, file.buffer.length, sha, file.buffer, req.user!.id]);
      await recordFileRevision(tx, id, req.user!, { section: 'File', item: fileName, field: 'Added', oldValue: null, newValue: KIND_LABEL[kind] });
    });
    reply.code(201);
    return getProductView(db, id);
  });

  app.delete('/api/products/:id/files/:fileId', { preHandler: requireAuth }, async (req) => {
    const id = parseId((req.params as { id: string }).id);
    const fileId = parseId((req.params as { fileId: string }).fileId, 'File');
    await withTx(db, async (tx) => {
      const { rows } = await tx.query(
        `UPDATE product_files SET deleted_at = now(), deleted_by = $3
          WHERE id = $1 AND product_id = $2 AND deleted_at IS NULL RETURNING file_name, kind`, [fileId, id, req.user!.id]);
      if (!rows[0]) throw notFound('File');
      await recordFileRevision(tx, id, req.user!, { section: 'File', item: rows[0].file_name, field: 'Removed', oldValue: KIND_LABEL[rows[0].kind], newValue: null });
    });
    return getProductView(db, id);
  });

  // ---- Suppliers (for autocomplete in the edit form) ----

  app.get('/api/suppliers', { preHandler: requireAuth }, async () => {
    const { rows } = await db.query('SELECT id, supplier_name, supplier_code FROM suppliers ORDER BY lower(supplier_name) LIMIT 2000');
    return rows.map((r) => ({ id: r.id, supplierName: r.supplier_name, supplierCode: r.supplier_code }));
  });

  // ---- Import ----

  app.post('/api/import/preview', { preHandler: requireImport }, async (req) => {
    const { file } = await readSingleFile(req, config.maxImportMb * 1024 * 1024);
    const fileName = sanitizeName(file.filename || 'import.xlsx');
    const preview = await buildPreview(db, file.buffer, fileName);
    const { rows } = await db.query(
      'INSERT INTO import_batches (created_by, file_name, payload) VALUES ($1, $2, $3) RETURNING id',
      [req.user!.id, fileName, JSON.stringify(preview)]);
    // Old uncommitted previews are not needed after a day.
    await db.query(`DELETE FROM import_batches WHERE committed_at IS NULL AND created_at < now() - interval '1 day'`);
    const { items, ...rest } = preview;
    return {
      ...rest,
      batchId: rows[0].id,
      items: items.map(({ input, ...i }) => ({
        ...i,
        description: input.description, model: input.model, unit: input.unit,
        suppliers: input.suppliers.map((s) => s.supplierName),
        specCount: input.specifications.length,
        hasMergeNotes: (input.notes ?? '').includes('Additional entries in import file'),
      })),
    };
  });

  app.post('/api/import/:batchId/commit', { preHandler: requireImport }, async (req) => {
    const batchId = parseId((req.params as { batchId: string }).batchId, 'Import');
    const body = z.object({ existingAction: z.enum(['skip', 'update']) }).safeParse(req.body);
    if (!body.success) throw badRequest('Choose what to do with existing product codes: skip or update.');
    return withTx(db, async (tx) => {
      const { rows } = await tx.query(
        'SELECT payload, committed_at, created_by FROM import_batches WHERE id = $1 FOR UPDATE', [batchId]);
      const batch = rows[0];
      if (!batch || batch.created_by !== req.user!.id) throw notFound('Import');
      if (batch.committed_at) throw conflict('ALREADY_IMPORTED', 'This import has already been completed.');
      const result = await commitImport(tx, batch.payload as ImportPreview, body.data.existingAction, req.user!);
      await tx.query('UPDATE import_batches SET committed_at = now(), result = $2 WHERE id = $1', [batchId, JSON.stringify(result)]);
      return result;
    });
  });

  app.delete('/api/import/:batchId', { preHandler: requireImport }, async (req, reply) => {
    const batchId = parseId((req.params as { batchId: string }).batchId, 'Import');
    await db.query('DELETE FROM import_batches WHERE id = $1 AND created_by = $2 AND committed_at IS NULL', [batchId, req.user!.id]);
    reply.code(204);
  });
}
