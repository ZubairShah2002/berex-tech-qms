import type { FastifyInstance } from 'fastify';
import { z } from 'zod';
import { searchProducts } from '../search.js';
import { findByCode, getProductView } from '../products.js';
import { getSettings } from '../settings.js';
import { badRequest, notFound, unauthorized } from '../errors.js';

const UUID = z.string().uuid();

export function parseId(value: unknown, what = 'Product'): string {
  const r = UUID.safeParse(value);
  if (!r.success) throw notFound(what);
  return r.data;
}

/** Read-only endpoints. These are open to everyone, no login required. */
export async function publicRoutes(app: FastifyInstance) {
  const db = app.db;

  app.get('/api/meta', async (req) => {
    const s = await getSettings(db);
    return { siteName: s.siteName, publicRevisionHistory: s.publicRevisionHistory, user: req.user };
  });

  app.get('/api/search', async (req) => {
    const q = z.object({ q: z.string().max(200).default(''), limit: z.coerce.number().int().min(1).max(100).default(25) })
      .safeParse(req.query);
    if (!q.success) throw badRequest('Invalid search.');
    return searchProducts(db, q.data.q, { limit: q.data.limit });
  });

  app.get('/api/products', async (req) => {
    const parsed = z.object({
      q: z.string().max(200).default(''),
      status: z.enum(['active', 'archived']).default('active'),
      page: z.coerce.number().int().min(1).max(100000).default(1),
      pageSize: z.coerce.number().int().min(1).max(200).default(50),
      sort: z.enum(['code', 'name', 'updated']).default('code'),
    }).safeParse(req.query);
    if (!parsed.success) throw badRequest('Invalid list parameters.');
    const { q, status, page, pageSize, sort } = parsed.data;
    if (status === 'archived' && !req.user) throw unauthorized();

    if (q.trim()) {
      const result = await searchProducts(db, q, { limit: 100, includeArchived: status === 'archived' });
      const hits = result.hits.filter((h) => h.status === status);
      return { items: hits, total: hits.length, page: 1, pageSize: hits.length, fuzzy: result.fuzzy };
    }
    const order = sort === 'name' ? 'p.product_name, p.product_code' : sort === 'updated' ? 'p.updated_at DESC' : 'p.product_code';
    const [list, count] = await Promise.all([
      db.query(
        `SELECT p.id, p.product_code, p.product_name, p.description, p.size, p.variant, p.model, p.category,
                p.current_revision, p.status, p.updated_at,
                (SELECT string_agg(s.supplier_name, ', ' ORDER BY ps.display_order)
                   FROM product_suppliers ps JOIN suppliers s ON s.id = ps.supplier_id WHERE ps.product_id = p.id) AS suppliers
           FROM products p WHERE p.status = $1 ORDER BY ${order} LIMIT $2 OFFSET $3`,
        [status, pageSize, (page - 1) * pageSize]),
      db.query('SELECT count(*)::int AS n FROM products WHERE status = $1', [status]),
    ]);
    return {
      items: list.rows.map((r) => ({
        id: r.id, productCode: r.product_code, productName: r.product_name, description: r.description,
        size: r.size, variant: r.variant, model: r.model, category: r.category, suppliers: r.suppliers,
        currentRevision: r.current_revision, status: r.status, updatedAt: r.updated_at.toISOString(),
      })),
      total: count.rows[0].n, page, pageSize, fuzzy: false,
    };
  });

  app.get('/api/products/by-code/:code', async (req) => {
    const { code } = req.params as { code: string };
    const found = await findByCode(db, code.slice(0, 64));
    if (!found) throw notFound();
    return found;
  });

  app.get('/api/products/:id', async (req) => {
    const id = parseId((req.params as { id: string }).id);
    const product = await getProductView(db, id);
    if (!product) throw notFound();
    return product;
  });

  const assertHistoryVisible = async (req: { user: unknown }) => {
    if (req.user) return;
    const s = await getSettings(db);
    if (!s.publicRevisionHistory) throw unauthorized();
  };

  app.get('/api/products/:id/revisions', async (req) => {
    const id = parseId((req.params as { id: string }).id);
    await assertHistoryVisible(req);
    const { rows } = await db.query(
      `SELECT r.id, r.revision_number, r.revision_label, r.changed_by_name, r.changed_at, r.change_summary,
              COALESCE(json_agg(json_build_object('section', c.section, 'item', c.item, 'field', c.field,
                                                  'oldValue', c.old_value, 'newValue', c.new_value) ORDER BY c.id)
                       FILTER (WHERE c.id IS NOT NULL), '[]') AS changes
         FROM product_revisions r LEFT JOIN product_revision_changes c ON c.revision_id = r.id
        WHERE r.product_id = $1
        GROUP BY r.id ORDER BY r.revision_number DESC`, [id]);
    return rows.map((r) => ({
      id: r.id, revisionNumber: r.revision_number, revisionLabel: r.revision_label, changedBy: r.changed_by_name,
      changedAt: r.changed_at.toISOString(), changeSummary: r.change_summary, changes: r.changes,
    }));
  });

  app.get('/api/revisions/:id', async (req) => {
    const id = parseId((req.params as { id: string }).id, 'Revision');
    await assertHistoryVisible(req);
    const { rows } = await db.query(
      `SELECT r.id, r.product_id, r.revision_number, r.revision_label, r.changed_by_name, r.changed_at, r.change_summary, r.snapshot,
              p.revision_number AS current_number
         FROM product_revisions r JOIN products p ON p.id = r.product_id WHERE r.id = $1`, [id]);
    const r = rows[0];
    if (!r) throw notFound('Revision');
    return {
      id: r.id, productId: r.product_id, revisionNumber: r.revision_number, revisionLabel: r.revision_label,
      changedBy: r.changed_by_name, changedAt: r.changed_at.toISOString(), changeSummary: r.change_summary,
      isCurrent: r.revision_number === r.current_number, snapshot: r.snapshot,
    };
  });

  app.get('/api/files/:id', async (req, reply) => {
    const id = parseId((req.params as { id: string }).id, 'File');
    const { rows } = await db.query(
      'SELECT file_name, content_type, data, sha256 FROM product_files WHERE id = $1 AND deleted_at IS NULL', [id]);
    const f = rows[0];
    if (!f) throw notFound('File');
    const download = (req.query as { download?: string }).download === '1';
    reply
      .header('Content-Type', f.content_type)
      .header('Content-Disposition', `${download ? 'attachment' : 'inline'}; filename*=UTF-8''${encodeURIComponent(f.file_name)}`)
      .header('Cache-Control', 'public, max-age=31536000, immutable')
      .header('ETag', `"${f.sha256}"`)
      .header('X-Content-Type-Options', 'nosniff');
    if (f.content_type.startsWith('image/')) {
      reply.header('Content-Security-Policy', "default-src 'none'; img-src 'self'; style-src 'unsafe-inline'; sandbox");
    }
    return reply.send(f.data);
  });
}
