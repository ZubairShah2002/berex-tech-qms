import crypto from 'node:crypto';
import { z } from 'zod';
import type { Queryable, Tx } from './db.js';
import type { AuthUser } from './auth.js';
import { badRequest, conflict, notFound } from './errors.js';
import { clean, codeKey, normalizeForSearch, revisionLabel } from './text.js';

// ---------------------------------------------------------------------------
// Content model: everything about a product that is under revision control.
// ---------------------------------------------------------------------------

export const PRODUCT_FIELDS = [
  ['productCode', 'product_code', 'Product Code'],
  ['productName', 'product_name', 'Product Name'],
  ['description', 'description', 'Description'],
  ['size', 'size', 'Size'],
  ['variant', 'variant', 'Variant'],
  ['model', 'model', 'Model / Application'],
  ['material', 'material', 'Material'],
  ['category', 'category', 'Group'],
  ['unit', 'unit', 'Unit'],
  ['notes', 'notes', 'Notes'],
] as const;

export type ProductFieldKey = (typeof PRODUCT_FIELDS)[number][0];

export interface SpecRow { id: string; name: string; value: string | null; unit: string | null; tolerance: string | null }
export interface InspectionRow { id: string; checkPoint: string; specification: string | null; tolerance: string | null; method: string | null }
export interface PackagingRow { id: string; item: string; requirement: string | null }
export interface SupplierRow { id: string; supplierId?: string; supplierName: string; supplierCode: string | null; partNumber: string | null; notes: string | null }

export type ProductContent = Record<ProductFieldKey, string | null> & {
  productCode: string;
  productName: string;
  specifications: SpecRow[];
  inspection: InspectionRow[];
  packaging: PackagingRow[];
  suppliers: SupplierRow[];
};

export interface FileMeta {
  id: string; kind: string; caption: string | null; fileName: string; contentType: string;
  sizeBytes: number; uploadedAt: string; uploadedBy: string | null;
}

export interface Change { section: string; item: string; field: string; oldValue: string | null; newValue: string | null }

// ---------------------------------------------------------------------------
// Input validation
// ---------------------------------------------------------------------------

const optText = (max: number) =>
  z.union([z.string(), z.number(), z.null()]).optional()
    .transform((v) => clean(v))
    .refine((v) => v === null || v.length <= max, { message: `Must be at most ${max} characters` });
const reqText = (max: number, label: string) =>
  z.union([z.string(), z.number()])
    .transform((v) => clean(v) ?? '')
    .refine((v) => v.length > 0, { message: `${label} is required` })
    .refine((v) => v.length <= max, { message: `Must be at most ${max} characters` });
const rowId = z.string().max(64).optional().nullable();

export const productInputSchema = z.object({
  productCode: reqText(64, 'Product code').refine((v) => !/[\u0000-\u001f\u007f]/.test(v), { message: 'Product code contains invalid characters' }),
  productName: reqText(300, 'Product name'),
  description: optText(4000),
  size: optText(200),
  variant: optText(200),
  model: optText(300),
  material: optText(500),
  category: optText(200),
  unit: optText(50),
  notes: optText(10000),
  specifications: z.array(z.object({
    id: rowId, name: reqText(200, 'Specification name'), value: optText(1000), unit: optText(50), tolerance: optText(200),
  })).max(500).default([]),
  inspection: z.array(z.object({
    id: rowId, checkPoint: reqText(200, 'Check point'), specification: optText(1000), tolerance: optText(200), method: optText(500),
  })).max(500).default([]),
  packaging: z.array(z.object({
    id: rowId, item: reqText(200, 'Packaging item'), requirement: optText(2000),
  })).max(200).default([]),
  suppliers: z.array(z.object({
    id: rowId, supplierName: reqText(300, 'Supplier name'), supplierCode: optText(100), partNumber: optText(200), notes: optText(2000),
  })).max(50).default([]),
});

export type ProductInput = z.infer<typeof productInputSchema>;

export const revisionMetaSchema = z.object({
  revisionLabel: optText(40),
  changeSummary: optText(1000),
});

export function parseProductInput(body: unknown): ProductInput {
  const parsed = productInputSchema.safeParse(body);
  if (!parsed.success) {
    const issues = parsed.error.issues.map((i) => ({ path: i.path.join('.'), message: i.message }));
    throw badRequest(issues[0]?.message ?? 'Invalid product data.', issues);
  }
  const seen = new Set<string>();
  for (const s of parsed.data.specifications) {
    const key = s.name.toLowerCase();
    if (seen.has(key)) throw badRequest(`Specification "${s.name}" is listed more than once. Edit the existing row instead.`);
    seen.add(key);
  }
  return parsed.data;
}

// ---------------------------------------------------------------------------
// Reading
// ---------------------------------------------------------------------------

export async function loadContent(db: Queryable, productId: string, forUpdate = false): Promise<(ProductContent & {
  id: string; status: string; revisionNumber: number; currentRevision: string;
}) | null> {
  const p = await db.query(
    `SELECT * FROM products WHERE id = $1 ${forUpdate ? 'FOR UPDATE' : ''}`, [productId]);
  const row = p.rows[0];
  if (!row) return null;
  // Sequential on purpose: db may be a single transaction client.
  const specs = await db.query('SELECT id, spec_name, value, unit, tolerance FROM product_specifications WHERE product_id = $1 ORDER BY display_order, spec_name', [productId]);
  const insp = await db.query('SELECT id, check_point, specification, tolerance, inspection_method FROM product_inspection_requirements WHERE product_id = $1 ORDER BY display_order', [productId]);
  const pack = await db.query('SELECT id, item, requirement FROM product_packaging_requirements WHERE product_id = $1 ORDER BY display_order', [productId]);
  const sup = await db.query(`SELECT ps.id, s.id AS supplier_id, s.supplier_name, s.supplier_code, ps.supplier_part_number, ps.notes
                FROM product_suppliers ps JOIN suppliers s ON s.id = ps.supplier_id
               WHERE ps.product_id = $1 ORDER BY ps.display_order`, [productId]);
  const content = {
    id: row.id as string,
    status: row.status as string,
    revisionNumber: row.revision_number as number,
    currentRevision: row.current_revision as string,
  } as ProductContent & { id: string; status: string; revisionNumber: number; currentRevision: string };
  for (const [key, col] of PRODUCT_FIELDS) (content as Record<string, unknown>)[key] = row[col] ?? null;
  content.specifications = specs.rows.map((r) => ({ id: r.id, name: r.spec_name, value: r.value, unit: r.unit, tolerance: r.tolerance }));
  content.inspection = insp.rows.map((r) => ({ id: r.id, checkPoint: r.check_point, specification: r.specification, tolerance: r.tolerance, method: r.inspection_method }));
  content.packaging = pack.rows.map((r) => ({ id: r.id, item: r.item, requirement: r.requirement }));
  content.suppliers = sup.rows.map((r) => ({ id: r.id, supplierId: r.supplier_id, supplierName: r.supplier_name, supplierCode: r.supplier_code, partNumber: r.supplier_part_number, notes: r.notes }));
  return content;
}

export async function loadFiles(db: Queryable, productId: string): Promise<FileMeta[]> {
  const { rows } = await db.query(
    `SELECT f.id, f.kind, f.caption, f.file_name, f.content_type, f.size_bytes, f.uploaded_at,
            u.user_id AS uploaded_by
       FROM product_files f LEFT JOIN users u ON u.id = f.uploaded_by
      WHERE f.product_id = $1 AND f.deleted_at IS NULL
      ORDER BY CASE f.kind WHEN 'photo' THEN 0 WHEN 'drawing' THEN 1 WHEN 'dimension_drawing' THEN 2 ELSE 3 END, f.uploaded_at`,
    [productId]);
  return rows.map((r) => ({
    id: r.id, kind: r.kind, caption: r.caption, fileName: r.file_name, contentType: r.content_type,
    sizeBytes: r.size_bytes, uploadedAt: r.uploaded_at.toISOString(), uploadedBy: r.uploaded_by,
  }));
}

/** Full product view, as returned to the product page. */
export async function getProductView(db: Queryable, productId: string) {
  const content = await loadContent(db, productId);
  if (!content) return null;
  const [files, meta] = await Promise.all([
    loadFiles(db, productId),
    db.query(
      `SELECT p.created_at, p.updated_at, p.archived_at,
              cu.user_id AS created_by,
              uu.user_id AS updated_by
         FROM products p
         LEFT JOIN users cu ON cu.id = p.created_by
         LEFT JOIN users uu ON uu.id = p.updated_by
        WHERE p.id = $1`, [productId]),
  ]);
  const m = meta.rows[0];
  return {
    ...content,
    files,
    createdAt: m.created_at.toISOString(),
    updatedAt: m.updated_at.toISOString(),
    archivedAt: m.archived_at ? m.archived_at.toISOString() : null,
    createdBy: m.created_by,
    updatedBy: m.updated_by,
  };
}

export async function findByCode(db: Queryable, code: string): Promise<{ id: string; productCode: string; productName: string; status: string } | null> {
  const { rows } = await db.query(
    'SELECT id, product_code, product_name, status FROM products WHERE upper(btrim(product_code)) = upper(btrim($1))', [code]);
  const r = rows[0];
  return r ? { id: r.id, productCode: r.product_code, productName: r.product_name, status: r.status } : null;
}

// ---------------------------------------------------------------------------
// Diffing (what changed between two revisions)
// ---------------------------------------------------------------------------

const withUnit = (value: string | null, unit: string | null) =>
  value === null ? null : unit ? `${value} ${unit}` : value;

function diffRows<T extends { id: string }>(
  section: string,
  before: T[], after: T[],
  label: (r: T) => string,
  fields: [string, (r: T) => string | null][],
  changes: Change[],
) {
  const beforeById = new Map(before.map((r) => [r.id, r]));
  const afterIds = new Set(after.map((r) => r.id));
  for (const r of before) {
    if (!afterIds.has(r.id)) {
      changes.push({ section, item: label(r), field: 'Removed', oldValue: fields.map(([, f]) => f(r)).filter(Boolean).join(' | ') || null, newValue: null });
    }
  }
  for (const r of after) {
    const old = beforeById.get(r.id);
    if (!old) {
      changes.push({ section, item: label(r), field: 'Added', oldValue: null, newValue: fields.map(([, f]) => f(r)).filter(Boolean).join(' | ') || null });
      continue;
    }
    if (label(old) !== label(r)) changes.push({ section, item: label(r), field: 'Name', oldValue: label(old), newValue: label(r) });
    for (const [name, f] of fields) {
      if (f(old) !== f(r)) changes.push({ section, item: label(r), field: name, oldValue: f(old), newValue: f(r) });
    }
  }
  const commonBefore = before.filter((r) => afterIds.has(r.id)).map((r) => r.id);
  const commonAfter = after.filter((r) => beforeById.has(r.id)).map((r) => r.id);
  if (commonBefore.join() !== commonAfter.join()) {
    changes.push({
      section, item: 'Order', field: 'Order',
      oldValue: before.filter((r) => afterIds.has(r.id)).map(label).join(', '),
      newValue: after.filter((r) => beforeById.has(r.id)).map(label).join(', '),
    });
  }
}

export function diffContent(before: ProductContent | null, after: ProductContent): Change[] {
  const changes: Change[] = [];
  if (!before) return changes;
  for (const [key, , label] of PRODUCT_FIELDS) {
    if ((before[key] ?? null) !== (after[key] ?? null)) {
      changes.push({ section: 'Product', item: label, field: label, oldValue: before[key] ?? null, newValue: after[key] ?? null });
    }
  }
  diffRows('Specification', before.specifications, after.specifications, (r) => r.name, [
    ['Value', (r) => withUnit(r.value, r.unit)],
    ['Tolerance', (r) => r.tolerance],
  ], changes);
  diffRows('Inspection', before.inspection, after.inspection, (r) => r.checkPoint, [
    ['Specification', (r) => r.specification],
    ['Tolerance', (r) => r.tolerance],
    ['Inspection Method', (r) => r.method],
  ], changes);
  diffRows('Packaging', before.packaging, after.packaging, (r) => r.item, [
    ['Requirement', (r) => r.requirement],
  ], changes);
  diffRows('Supplier', before.suppliers, after.suppliers, (r) => r.supplierName, [
    ['Supplier Code', (r) => r.supplierCode],
    ['Supplier Part Number', (r) => r.partNumber],
    ['Supplier Notes', (r) => r.notes],
  ], changes);
  return changes;
}

const SECTION_NOUN: Record<string, string> = {
  Specification: '', Inspection: 'inspection check ', Packaging: 'packaging item ', Supplier: 'supplier ', File: 'file ',
};

export function summarizeChanges(changes: Change[]): string {
  const parts: string[] = [];
  for (const c of changes) {
    let s: string;
    if (c.section === 'Product') s = `Updated ${c.item}`;
    else if (c.field === 'Added') s = `Added ${SECTION_NOUN[c.section] ?? ''}${c.item}`;
    else if (c.field === 'Removed') s = `Removed ${SECTION_NOUN[c.section] ?? ''}${c.item}`;
    else if (c.field === 'Order') s = `Reordered ${c.section.toLowerCase()} rows`;
    else s = `Updated ${SECTION_NOUN[c.section] ?? ''}${c.item}`;
    if (!parts.includes(s)) parts.push(s);
  }
  if (parts.length > 6) return `${parts.slice(0, 6).join('; ')}; and ${parts.length - 6} more`;
  return parts.join('; ');
}

/** One human-readable line per change, used in the audit log. */
export function describeChange(c: Change): string {
  const q = (v: string | null) => (v === null ? '(blank)' : v);
  if (c.field === 'Added') return `added ${c.section.toLowerCase()} ${c.item}${c.newValue ? ` (${c.newValue})` : ''}`;
  if (c.field === 'Removed') return `removed ${c.section.toLowerCase()} ${c.item}${c.oldValue ? ` (${c.oldValue})` : ''}`;
  if (c.field === 'Order') return `reordered ${c.section.toLowerCase()} rows`;
  if (c.section === 'Product') return `${c.item} from ${q(c.oldValue)} to ${q(c.newValue)}`;
  const field = c.field === 'Value' ? '' : ` ${c.field.toLowerCase()}`;
  return `${c.item}${field} from ${q(c.oldValue)} to ${q(c.newValue)}`;
}

// ---------------------------------------------------------------------------
// Writing
// ---------------------------------------------------------------------------

const UUID_RE = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

/** Gives each input row a stable id: keeps ids that already belong to the product, new uuid otherwise. */
export function assignRowIds(input: ProductInput, before: ProductContent | null): ProductContent {
  const known = new Set<string>([
    ...(before?.specifications ?? []), ...(before?.inspection ?? []), ...(before?.packaging ?? []), ...(before?.suppliers ?? []),
  ].map((r) => r.id));
  const seen = new Set<string>();
  const idFor = (id: string | null | undefined) => {
    const ok = id && UUID_RE.test(id) && known.has(id) && !seen.has(id);
    const out = ok ? id! : crypto.randomUUID();
    seen.add(out);
    return out;
  };
  return {
    productCode: input.productCode,
    productName: input.productName,
    description: input.description, size: input.size, variant: input.variant, model: input.model,
    material: input.material, category: input.category, unit: input.unit, notes: input.notes,
    specifications: input.specifications.map((r) => ({ id: idFor(r.id), name: r.name, value: r.value, unit: r.unit, tolerance: r.tolerance })),
    inspection: input.inspection.map((r) => ({ id: idFor(r.id), checkPoint: r.checkPoint, specification: r.specification, tolerance: r.tolerance, method: r.method })),
    packaging: input.packaging.map((r) => ({ id: idFor(r.id), item: r.item, requirement: r.requirement })),
    suppliers: input.suppliers.map((r) => ({ id: idFor(r.id), supplierName: r.supplierName, supplierCode: r.supplierCode, partNumber: r.partNumber, notes: r.notes })),
  };
}

export function buildSearchText(c: ProductContent): string {
  const parts: (string | null)[] = [
    c.productCode, c.productName, c.description, c.size, c.variant, c.model, c.material, c.category, c.notes,
    ...c.specifications.flatMap((s) => [s.name, s.value, s.unit, s.tolerance]),
    ...c.inspection.flatMap((s) => [s.checkPoint, s.specification, s.method]),
    ...c.packaging.flatMap((s) => [s.item, s.requirement]),
    ...c.suppliers.flatMap((s) => [s.supplierName, s.supplierCode, s.partNumber]),
  ];
  return normalizeForSearch(parts.filter(Boolean).join(' ')).slice(0, 20000);
}

async function upsertSupplier(tx: Queryable, name: string, code: string | null, overwriteCode: boolean): Promise<string> {
  const { rows } = await tx.query(
    `INSERT INTO suppliers (supplier_name, supplier_code) VALUES ($1, $2)
     ON CONFLICT (btrim(supplier_name)) DO UPDATE
       SET supplier_code = CASE WHEN $3 THEN EXCLUDED.supplier_code ELSE COALESCE(suppliers.supplier_code, EXCLUDED.supplier_code) END,
           updated_at = now()
     RETURNING id`,
    [name, code, overwriteCode]);
  return rows[0].id;
}

/** Writes product columns and replaces all child rows with the given content. */
async function writeContent(tx: Tx, productId: string, c: ProductContent, userId: string | null, overwriteSupplierCodes: boolean) {
  const cols = PRODUCT_FIELDS.map(([, col]) => col);
  const values = PRODUCT_FIELDS.map(([key]) => c[key] ?? null);
  await tx.query(
    `UPDATE products SET ${cols.map((col, i) => `${col} = $${i + 2}`).join(', ')},
            code_key = $${cols.length + 2}, search_text = $${cols.length + 3},
            updated_at = now(), updated_by = $${cols.length + 4}
      WHERE id = $1`,
    [productId, ...values, codeKey(c.productCode), buildSearchText(c), userId]);

  await tx.query('DELETE FROM product_specifications WHERE product_id = $1', [productId]);
  await tx.query('DELETE FROM product_inspection_requirements WHERE product_id = $1', [productId]);
  await tx.query('DELETE FROM product_packaging_requirements WHERE product_id = $1', [productId]);
  await tx.query('DELETE FROM product_suppliers WHERE product_id = $1', [productId]);

  if (c.specifications.length) {
    await tx.query(
      `INSERT INTO product_specifications (id, product_id, spec_name, value, unit, tolerance, display_order)
       SELECT x.id, $1, x.name, x.value, x.unit, x.tolerance, x.ord
         FROM jsonb_to_recordset($2::jsonb) AS x(id uuid, name text, value text, unit text, tolerance text, ord int)`,
      [productId, JSON.stringify(c.specifications.map((r, i) => ({ ...r, ord: i })))]);
  }
  if (c.inspection.length) {
    await tx.query(
      `INSERT INTO product_inspection_requirements (id, product_id, check_point, specification, tolerance, inspection_method, display_order)
       SELECT x.id, $1, x."checkPoint", x.specification, x.tolerance, x.method, x.ord
         FROM jsonb_to_recordset($2::jsonb) AS x(id uuid, "checkPoint" text, specification text, tolerance text, method text, ord int)`,
      [productId, JSON.stringify(c.inspection.map((r, i) => ({ ...r, ord: i })))]);
  }
  if (c.packaging.length) {
    await tx.query(
      `INSERT INTO product_packaging_requirements (id, product_id, item, requirement, display_order)
       SELECT x.id, $1, x.item, x.requirement, x.ord
         FROM jsonb_to_recordset($2::jsonb) AS x(id uuid, item text, requirement text, ord int)`,
      [productId, JSON.stringify(c.packaging.map((r, i) => ({ ...r, ord: i })))]);
  }
  for (const [i, s] of c.suppliers.entries()) {
    const supplierId = await upsertSupplier(tx, s.supplierName, s.supplierCode, overwriteSupplierCodes);
    s.supplierId = supplierId;
    await tx.query(
      `INSERT INTO product_suppliers (id, product_id, supplier_id, supplier_part_number, notes, display_order)
       VALUES ($1, $2, $3, $4, $5, $6)`,
      [s.id, productId, supplierId, s.partNumber, s.notes, i]);
  }
}

function snapshotOf(c: ProductContent, files: FileMeta[]) {
  const { productCode, productName, description, size, variant, model, material, category, unit, notes, specifications, inspection, packaging } = c;
  return {
    productCode, productName, description, size, variant, model, material, category, unit, notes,
    specifications, inspection, packaging,
    suppliers: c.suppliers.map(({ id, supplierName, supplierCode, partNumber, notes: n }) => ({ id, supplierName, supplierCode, partNumber, notes: n })),
    files: files.map(({ id, kind, caption, fileName }) => ({ id, kind, caption, fileName })),
  };
}

export const actorName = (u: AuthUser) => u.userId;

export async function writeAudit(
  tx: Queryable, user: AuthUser, action: string,
  product: { id: string; code: string } | null, detail: string, data?: unknown,
) {
  await tx.query(
    `INSERT INTO audit_log (user_id, user_name, action, product_id, product_code, detail, data)
     VALUES ($1, $2, $3, $4, $5, $6, $7)`,
    [user.id, actorName(user), action, product?.id ?? null, product?.code ?? null, detail, data === undefined ? null : JSON.stringify(data)]);
}

async function insertRevision(
  tx: Tx, productId: string, number: number, label: string, user: AuthUser,
  summary: string, snapshot: unknown, changes: Change[],
) {
  const { rows } = await tx.query(
    `INSERT INTO product_revisions (product_id, revision_number, revision_label, changed_by, changed_by_name, change_summary, snapshot)
     VALUES ($1, $2, $3, $4, $5, $6, $7) RETURNING id`,
    [productId, number, label, user.id, actorName(user), summary, JSON.stringify(snapshot)]);
  const revisionId = rows[0].id;
  if (changes.length) {
    await tx.query(
      `INSERT INTO product_revision_changes (revision_id, section, item, field, old_value, new_value)
       SELECT $1, x.section, x.item, x.field, x."oldValue", x."newValue"
         FROM jsonb_to_recordset($2::jsonb) AS x(section text, item text, field text, "oldValue" text, "newValue" text)`,
      [revisionId, JSON.stringify(changes)]);
  }
  await tx.query('UPDATE products SET revision_number = $2, current_revision = $3 WHERE id = $1', [productId, number, label]);
  return revisionId;
}

async function assertCodeFree(tx: Queryable, code: string, exceptId: string | null) {
  const existing = await findByCode(tx, code);
  if (existing && existing.id !== exceptId) {
    throw conflict('DUPLICATE_CODE', 'Product code already exists.', { existing });
  }
}

export interface SaveOptions { revisionLabel?: string | null; changeSummary?: string | null; overwriteSupplierCodes?: boolean; auditAction?: string }

export async function createProduct(tx: Tx, input: ProductInput, user: AuthUser, opts: SaveOptions = {}): Promise<string> {
  await assertCodeFree(tx, input.productCode, null);
  const content = assignRowIds(input, null);
  const label = opts.revisionLabel || revisionLabel(1);
  let id: string;
  try {
    const { rows } = await tx.query(
      `INSERT INTO products (product_code, code_key, product_name, created_by, updated_by, current_revision)
       VALUES ($1, $2, $3, $4, $4, $5) RETURNING id`,
      [content.productCode, codeKey(content.productCode), content.productName, user.id, label]);
    id = rows[0].id;
  } catch (err) {
    if ((err as { code?: string }).code === '23505') throw conflict('DUPLICATE_CODE', 'Product code already exists.');
    throw err;
  }
  await writeContent(tx, id, content, user.id, opts.overwriteSupplierCodes ?? true);
  const summary = opts.changeSummary || 'Initial specification';
  await insertRevision(tx, id, 1, label, user, summary, snapshotOf(content, []), [
    { section: 'Product', item: 'Product', field: 'Created', oldValue: null, newValue: `${content.productCode} — ${content.productName}` },
  ]);
  await writeAudit(tx, user, opts.auditAction ?? 'product.create', { id, code: content.productCode },
    `${actorName(user)} created ${content.productCode} (${content.productName}), ${label}.`);
  return id;
}

export interface UpdateResult { changed: boolean; revisionNumber: number; currentRevision: string; changes: Change[] }

export async function updateProduct(
  tx: Tx, productId: string, input: ProductInput, user: AuthUser,
  expectedRevision: number | null, opts: SaveOptions = {},
): Promise<UpdateResult> {
  const before = await loadContent(tx, productId, true);
  if (!before) throw notFound();
  if (expectedRevision !== null && expectedRevision !== before.revisionNumber) {
    throw conflict('REVISION_CONFLICT',
      `This product was changed by someone else (now ${before.currentRevision}). Reload the page and apply your changes again.`);
  }
  if (input.productCode !== before.productCode) await assertCodeFree(tx, input.productCode, productId);

  const after = assignRowIds(input, before);
  const changes = diffContent(before, after);
  if (changes.length === 0) {
    return { changed: false, revisionNumber: before.revisionNumber, currentRevision: before.currentRevision, changes };
  }
  try {
    await writeContent(tx, productId, after, user.id, opts.overwriteSupplierCodes ?? true);
  } catch (err) {
    if ((err as { code?: string }).code === '23505') throw conflict('DUPLICATE_CODE', 'Product code already exists.');
    throw err;
  }
  const number = before.revisionNumber + 1;
  const label = opts.revisionLabel || revisionLabel(number);
  const summary = opts.changeSummary || summarizeChanges(changes);
  await insertRevision(tx, productId, number, label, user, summary, snapshotOf(after, await loadFiles(tx, productId)), changes);
  await writeAudit(tx, user, opts.auditAction ?? 'product.update', { id: productId, code: after.productCode },
    `${actorName(user)} updated ${before.productCode}${before.productCode !== after.productCode ? ` (now ${after.productCode})` : ''}: ${changes.map(describeChange).join('; ')}. New revision ${label}.`,
    { changes });
  return { changed: true, revisionNumber: number, currentRevision: label, changes };
}

/** Records a file addition/removal as its own revision. */
export async function recordFileRevision(tx: Tx, productId: string, user: AuthUser, change: Change): Promise<string> {
  const before = await loadContent(tx, productId, true);
  if (!before) throw notFound();
  const number = before.revisionNumber + 1;
  const label = revisionLabel(number);
  await tx.query('UPDATE products SET updated_at = now(), updated_by = $2 WHERE id = $1', [productId, user.id]);
  const summary = `${change.field} ${change.item}`;
  await insertRevision(tx, productId, number, label, user, summary, snapshotOf(before, await loadFiles(tx, productId)), [change]);
  await writeAudit(tx, user, change.field === 'Added' ? 'file.upload' : 'file.delete', { id: productId, code: before.productCode },
    `${actorName(user)} ${change.field === 'Added' ? 'uploaded' : 'removed'} ${change.newValue ?? change.oldValue} "${change.item}" on ${before.productCode}. New revision ${label}.`);
  return label;
}

/** Converts stored content back into input form (used for restoring an old revision). */
export function contentToInput(c: Omit<ProductContent, never>): ProductInput {
  return parseProductInput({
    ...c,
    specifications: c.specifications ?? [],
    inspection: c.inspection ?? [],
    packaging: c.packaging ?? [],
    suppliers: c.suppliers ?? [],
  });
}
