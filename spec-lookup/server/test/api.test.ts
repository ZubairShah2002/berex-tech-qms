/**
 * API integration tests. Needs a PostgreSQL database:
 *   TEST_DATABASE_URL=postgres://user:pass@localhost:5432/spec_test npm test
 * Each run uses its own throwaway schema, dropped afterwards.
 */
import { after, before, describe, it } from 'node:test';
import assert from 'node:assert/strict';
import type { FastifyInstance } from 'fastify';
import type pg from 'pg';
import { createPool, migrate } from '../src/db.js';
import { buildApp } from '../src/app.js';
import { hashPassword } from '../src/auth.js';
import { normalizeForSearch, codeKey } from '../src/text.js';

const url = process.env.TEST_DATABASE_URL;
if (!url) {
  console.error('Set TEST_DATABASE_URL to run the API tests.');
  process.exit(1);
}
const schema = `t_${Date.now().toString(36)}_${Math.floor(Math.random() * 1e6)}`;

let pool: pg.Pool;
let app: FastifyInstance;
const cookies: Record<string, string> = {};

const PNG = Buffer.from('89504e470d0a1a0a0000000d4948445200000001000000010806000000' + '1f15c4890000000d49444154789c6360000002000154a24f5d0000000049454e44ae426082', 'hex');

async function req(method: string, path: string, opts: { as?: string; body?: unknown; headers?: Record<string, string>; payload?: Buffer } = {}) {
  const headers: Record<string, string> = { ...(opts.headers ?? {}) };
  if (opts.as) headers.cookie = `sl_session=${cookies[opts.as]}`;
  const res = await app.inject({
    method: method as 'GET', url: path, headers,
    ...(opts.payload ? { payload: opts.payload } : opts.body !== undefined ? { payload: opts.body as object } : {}),
  });
  let json: any = null;
  try { json = res.json(); } catch { /* not json */ }
  return { status: res.statusCode, json, headers: res.headers, raw: res };
}

function multipart(fields: Record<string, string>, file: { name: string; data: Buffer; type: string }) {
  const boundary = '----test' + Math.random().toString(16).slice(2);
  const parts: Buffer[] = [];
  for (const [k, v] of Object.entries(fields)) {
    parts.push(Buffer.from(`--${boundary}\r\nContent-Disposition: form-data; name="${k}"\r\n\r\n${v}\r\n`));
  }
  parts.push(Buffer.from(`--${boundary}\r\nContent-Disposition: form-data; name="file"; filename="${file.name}"\r\nContent-Type: ${file.type}\r\n\r\n`));
  parts.push(file.data, Buffer.from(`\r\n--${boundary}--\r\n`));
  return { payload: Buffer.concat(parts), headers: { 'content-type': `multipart/form-data; boundary=${boundary}` } };
}

async function login(userId: string, password: string) {
  const res = await app.inject({ method: 'POST', url: '/api/auth/login', payload: { userId, password } });
  assert.equal(res.statusCode, 200, res.body);
  const c = res.cookies.find((x) => x.name === 'sl_session');
  assert.ok(c?.httpOnly, 'session cookie must be HttpOnly');
  cookies[userId] = c!.value;
}

const baseProduct = (code: string, extra: Record<string, unknown> = {}) => ({
  productCode: code, productName: 'Latex (D75) 9 Zone', size: 'King',
  specifications: [
    { name: 'Density', value: 'D75', unit: null, tolerance: null },
    { name: 'Length', value: '1880', unit: 'mm', tolerance: '±5' },
    { name: 'Width', value: '1800', unit: 'mm', tolerance: '±5' },
  ],
  suppliers: [{ supplierName: 'Maus', supplierCode: null, partNumber: null, notes: null }],
  ...extra,
});

before(async () => {
  pool = createPool(url, schema);
  await migrate(pool, schema);
  await pool.query(`INSERT INTO users (user_id, display_name, password_hash, role, can_import) VALUES
    ('admin', 'Admin', $1, 'admin', true), ('QC01', 'QC One', $1, 'qc', false), ('QC02', 'QC Two', $1, 'qc', true)`,
  [await hashPassword('Password123')]);
  app = await buildApp({ db: pool, webDist: null, logger: false });
  await login('admin', 'Password123');
  await login('QC01', 'Password123');
  await login('QC02', 'Password123');
});

after(async () => {
  await app?.close();
  await pool?.query(`DROP SCHEMA IF EXISTS ${schema} CASCADE`);
  await pool?.end();
});

describe('text normalization', () => {
  it('normalizes digit grouping, case and punctuation for search only', () => {
    assert.equal(normalizeForSearch('NPC450, 4mm x 1,810mm x 1,880mm'), 'npc450 4mm x 1810mm x 1880mm');
    assert.equal(normalizeForSearch('Latex (D75) 9-Zone, King'), 'latex d75 9 zone king');
    assert.equal(normalizeForSearch('2.00mm SWC-72B'), '2.00mm swc 72b');
    assert.equal(codeKey(' 2mlt 3101a '), '2MLT3101A');
  });
});

describe('authorization', () => {
  it('rejects every write without login', async () => {
    const p = baseProduct('AUTH-1', { productName: 'Auth test item', size: null });
    assert.equal((await req('POST', '/api/products', { body: { product: p } })).status, 401);
    const created = await req('POST', '/api/products', { as: 'QC01', body: { product: p } });
    assert.equal(created.status, 201);
    const id = created.json.id;
    assert.equal((await req('PUT', `/api/products/${id}`, { body: { product: p, expectedRevision: 1 } })).status, 401);
    assert.equal((await req('POST', `/api/products/${id}/archive`)).status, 401);
    assert.equal((await req('POST', `/api/products/${id}/restore`)).status, 401);
    const mp = multipart({ kind: 'photo' }, { name: 'a.png', data: PNG, type: 'image/png' });
    assert.equal((await req('POST', `/api/products/${id}/files`, { payload: mp.payload, headers: mp.headers })).status, 401);
    const imp = multipart({}, { name: 'a.csv', data: Buffer.from('Product Code,Product Name\nX1,Y\n'), type: 'text/csv' });
    assert.equal((await req('POST', '/api/import/preview', { payload: imp.payload, headers: imp.headers })).status, 401);
    assert.equal((await req('GET', '/api/users')).status, 401);
    assert.equal((await req('POST', '/api/users', { body: { userId: 'x1', role: 'admin', password: 'Password123' } })).status, 401);
    assert.equal((await req('PUT', '/api/settings', { body: { siteName: 'x', publicRevisionHistory: false } })).status, 401);
    // Data unchanged
    const view = await req('GET', `/api/products/${id}`);
    assert.equal(view.json.revisionNumber, 1);
  });

  it('keeps user management and import behind roles', async () => {
    assert.equal((await req('GET', '/api/users', { as: 'QC01' })).status, 403);
    assert.equal((await req('PUT', '/api/settings', { as: 'QC01', body: { siteName: 'x', publicRevisionHistory: true } })).status, 403);
    const imp = multipart({}, { name: 'a.csv', data: Buffer.from('Product Code,Product Name\nX1,Y\n'), type: 'text/csv' });
    assert.equal((await req('POST', '/api/import/preview', { as: 'QC01', payload: imp.payload, headers: imp.headers })).status, 403);
    assert.equal((await req('POST', '/api/import/preview', { as: 'QC02', payload: imp.payload, headers: imp.headers })).status, 200);
  });

  it('blocks cross-site writes and bad logins', async () => {
    const r = await req('POST', '/api/products', { as: 'admin', body: { product: baseProduct('XS-1') }, headers: { origin: 'https://evil.example' } });
    assert.equal(r.status, 403);
    const bad = await app.inject({ method: 'POST', url: '/api/auth/login', payload: { userId: 'admin', password: 'wrong' } });
    assert.equal(bad.statusCode, 401);
    assert.equal(bad.json().error.message, 'Incorrect user ID or password.');
  });
});

describe('products', () => {
  let id = '';

  it('creates a product and keeps values exactly as entered', async () => {
    const r = await req('POST', '/api/products', { as: 'QC01', body: { product: baseProduct('2MLT3101A', { description: '  D75, 1820mm x 1890mm x 25mm  ' }) } });
    assert.equal(r.status, 201);
    id = r.json.id;
    assert.equal(r.json.productCode, '2MLT3101A');
    assert.equal(r.json.description, 'D75, 1820mm x 1890mm x 25mm');
    assert.equal(r.json.currentRevision, 'Rev. 01');
    assert.deepEqual(r.json.specifications.map((s: any) => [s.name, s.value, s.unit, s.tolerance]), [
      ['Density', 'D75', null, null], ['Length', '1880', 'mm', '±5'], ['Width', '1800', 'mm', '±5'],
    ]);
    assert.equal(r.json.material, null);
  });

  it('rejects duplicate product codes regardless of case and does not overwrite', async () => {
    const r = await req('POST', '/api/products', { as: 'QC01', body: { product: baseProduct(' 2mlt3101a ', { productName: 'Other' }) } });
    assert.equal(r.status, 409);
    assert.equal(r.json.error.code, 'DUPLICATE_CODE');
    assert.equal(r.json.error.message, 'Product code already exists.');
    assert.equal(r.json.error.details.existing.id, id);
    const view = await req('GET', `/api/products/${id}`);
    assert.equal(view.json.productName, 'Latex (D75) 9 Zone');
  });

  it('finds products without login by code, name, keyword and dimensions', async () => {
    const byCode = await req('GET', '/api/search?q=2MLT3101A');
    assert.equal(byCode.json.hits[0].id, id);
    assert.equal(byCode.json.hits[0].exactCode, true);
    const spaced = await req('GET', '/api/search?q=' + encodeURIComponent('  2mlt 3101a '));
    assert.equal(spaced.json.hits[0].id, id);
    const byName = await req('GET', '/api/search?q=' + encodeURIComponent('Latex D75 9 Zone King'));
    assert.equal(byName.json.hits[0].id, id);
    const bySpec = await req('GET', '/api/search?q=' + encodeURIComponent('1880 x 1800'));
    assert.ok(bySpec.json.hits.some((h: any) => h.id === id));
    const bySupplier = await req('GET', '/api/search?q=maus');
    assert.ok(bySupplier.json.hits.some((h: any) => h.id === id));
    const typo = await req('GET', '/api/search?q=2MLT311A');
    assert.equal(typo.json.fuzzy, true);
    assert.equal(typo.json.hits[0].id, id);
  });

  it('creates a revision with before/after values on edit', async () => {
    const view = (await req('GET', `/api/products/${id}`)).json;
    const product = { ...view, specifications: view.specifications.map((s: any) => (s.name === 'Width' ? { ...s, value: '1810' } : s)) };
    const r = await req('PUT', `/api/products/${id}`, { as: 'QC01', body: { product, expectedRevision: 1 } });
    assert.equal(r.status, 200);
    assert.equal(r.json.changed, true);
    assert.equal(r.json.currentRevision, 'Rev. 02');
    assert.deepEqual(r.json.changes, [{ section: 'Specification', item: 'Width', field: 'Value', oldValue: '1800 mm', newValue: '1810 mm' }]);

    const revs = (await req('GET', `/api/products/${id}/revisions`)).json;
    assert.deepEqual(revs.map((x: any) => x.revisionLabel), ['Rev. 02', 'Rev. 01']);
    assert.equal(revs[0].changedBy, 'QC01');
    assert.equal(revs[0].changeSummary, 'Updated Width');

    const audit = (await req('GET', `/api/audit?productId=${id}`, { as: 'QC01' })).json;
    assert.match(audit[0].detail, /QC01 updated 2MLT3101A: Width from 1800 mm to 1810 mm/);

    // Saving the same content again creates no revision.
    const again = await req('PUT', `/api/products/${id}`, { as: 'QC01', body: { product: r.json.product, expectedRevision: 2 } });
    assert.equal(again.json.changed, false);
  });

  it('refuses to overwrite a newer revision', async () => {
    const view = (await req('GET', `/api/products/${id}`)).json;
    const r = await req('PUT', `/api/products/${id}`, { as: 'admin', body: { product: { ...view, notes: 'x' }, expectedRevision: 1 } });
    assert.equal(r.status, 409);
    assert.equal(r.json.error.code, 'REVISION_CONFLICT');
  });

  it('supports adding, removing and reordering specification rows', async () => {
    const view = (await req('GET', `/api/products/${id}`)).json;
    const [density, length, width] = view.specifications;
    const product = { ...view, specifications: [width, length, { name: 'Hardness', value: '65', unit: 'N', tolerance: '±10%' }] };
    void density;
    const r = await req('PUT', `/api/products/${id}`, { as: 'QC01', body: { product, expectedRevision: view.revisionNumber } });
    assert.equal(r.status, 200);
    const fields = r.json.changes.map((c: any) => `${c.item}:${c.field}`);
    assert.ok(fields.includes('Density:Removed'));
    assert.ok(fields.includes('Hardness:Added'));
    assert.ok(fields.includes('Order:Order'));
    assert.deepEqual(r.json.product.specifications.map((s: any) => s.name), ['Width', 'Length', 'Hardness']);
  });

  it('rejects a duplicate specification name and missing required fields', async () => {
    const view = (await req('GET', `/api/products/${id}`)).json;
    const dup = await req('PUT', `/api/products/${id}`, { as: 'QC01', body: { product: { ...view, specifications: [...view.specifications, { name: 'width', value: '1' }] }, expectedRevision: view.revisionNumber } });
    assert.equal(dup.status, 400);
    const noName = await req('POST', '/api/products', { as: 'QC01', body: { product: { productCode: 'NEW-1', productName: '  ' } } });
    assert.equal(noName.status, 400);
  });

  it('lets an admin restore an earlier revision as a new revision', async () => {
    const revs = (await req('GET', `/api/products/${id}/revisions`)).json;
    const first = revs.find((r: any) => r.revisionNumber === 1);
    assert.equal((await req('POST', `/api/products/${id}/revisions/${first.id}/restore`, { as: 'QC01' })).status, 403);
    const r = await req('POST', `/api/products/${id}/revisions/${first.id}/restore`, { as: 'admin' });
    assert.equal(r.status, 200);
    assert.deepEqual(r.json.product.specifications.map((s: any) => `${s.name}=${s.value}`), ['Density=D75', 'Length=1880', 'Width=1800']);
    assert.equal(r.json.product.revisionNumber, revs[0].revisionNumber + 1);
  });

  it('validates uploaded files by content and records a revision', async () => {
    const before = (await req('GET', `/api/products/${id}`)).json.revisionNumber;
    const fake = multipart({ kind: 'photo' }, { name: 'evil.png', data: Buffer.from('<script>alert(1)</script>'), type: 'image/png' });
    assert.equal((await req('POST', `/api/products/${id}/files`, { as: 'QC01', payload: fake.payload, headers: fake.headers })).status, 400);
    const svg = multipart({ kind: 'drawing' }, { name: 'd.svg', data: Buffer.from('<svg xmlns="http://www.w3.org/2000/svg"/>'), type: 'image/svg+xml' });
    assert.equal((await req('POST', `/api/products/${id}/files`, { as: 'QC01', payload: svg.payload, headers: svg.headers })).status, 400);
    const ok = multipart({ kind: 'drawing', caption: 'Top view' }, { name: 'drawing.png', data: PNG, type: 'application/octet-stream' });
    const r = await req('POST', `/api/products/${id}/files`, { as: 'QC01', payload: ok.payload, headers: ok.headers });
    assert.equal(r.status, 201);
    assert.equal(r.json.files.length, 1);
    assert.equal(r.json.files[0].contentType, 'image/png');
    assert.equal(r.json.revisionNumber, before + 1);
    const file = await req('GET', `/api/files/${r.json.files[0].id}`);
    assert.equal(file.status, 200);
    assert.equal(file.headers['content-type'], 'image/png');
    assert.equal(file.headers['x-content-type-options'], 'nosniff');
    const del = await req('DELETE', `/api/products/${id}/files/${r.json.files[0].id}`, { as: 'QC01' });
    assert.equal(del.json.files.length, 0);
    assert.equal((await req('GET', `/api/files/${r.json.files[0].id}`)).status, 404);
  });

  it('archives (hidden from search) and restores', async () => {
    assert.equal((await req('POST', `/api/products/${id}/archive`, { as: 'QC01' })).json.status, 'archived');
    assert.equal((await req('GET', '/api/search?q=2MLT3101A')).json.hits.length, 0);
    assert.equal((await req('GET', '/api/products?status=archived')).status, 401);
    const archived = await req('GET', '/api/products?status=archived', { as: 'QC01' });
    assert.ok(archived.json.items.some((p: any) => p.id === id));
    assert.equal((await req('GET', `/api/products/${id}`)).json.status, 'archived');
    const restored = await req('POST', `/api/products/${id}/restore`, { as: 'QC01' });
    assert.equal(restored.json.status, 'active');
    assert.equal((await req('GET', '/api/search?q=2MLT3101A')).json.hits[0].id, id);
  });

  it('hides revision history from the public when disabled', async () => {
    await req('PUT', '/api/settings', { as: 'admin', body: { siteName: 'Berex Tech', publicRevisionHistory: false } });
    assert.equal((await req('GET', `/api/products/${id}/revisions`)).status, 401);
    assert.equal((await req('GET', `/api/products/${id}/revisions`, { as: 'QC01' })).status, 200);
    await req('PUT', '/api/settings', { as: 'admin', body: { siteName: 'Berex Tech', publicRevisionHistory: true } });
  });
});

describe('import', () => {
  const csv = [
    'No.,Item Code,Material Name,Description,Group,Applicated,Unit,Supplier1,Supplier2,Length',
    '1,IMP-001,Zipper K,"VS-5, 295"" (7,415mm)",Raw Material,Prime1,pcs,K.H. Zipper,,7415',
    '2,IMP-001,Zipper K,"VS-5, 295"" (7,415mm)",Raw Material,Elite Series,mtr,Soon son,,7415',
    '3,IMP-002,Felt - K,NPC450,Raw Material,PrimeLite,Pcs,BETTERFILL,Betterfill,',
    '4,,No code row,,,,,,,',
    '5,IMP-003,,,,,,,,',
    '6,2MLT3101A,Changed name,New description,,,,LSK,,1890',
  ].join('\n');

  async function preview() {
    const mp = multipart({}, { name: 'materials.csv', data: Buffer.from(csv), type: 'text/csv' });
    const r = await req('POST', '/api/import/preview', { as: 'admin', payload: mp.payload, headers: mp.headers });
    assert.equal(r.status, 200, JSON.stringify(r.json));
    return r.json;
  }

  it('previews without writing anything', async () => {
    const p = await preview();
    assert.deepEqual(p.totals, { totalRows: 6, newProducts: 2, existingProducts: 1, missingCodes: 1, invalidRows: 1, mergedRows: 1, ignoredRows: 0 });
    assert.equal((await req('GET', '/api/products/by-code/IMP-001')).status, 404);
    const mapping = Object.fromEntries(p.columns.map((c: any) => [c.header, c.target]));
    assert.equal(mapping['Applicated'], 'model');
    assert.equal(mapping['Length'], 'spec');
  });

  it('skips existing products when asked and keeps exact values', async () => {
    const p = await preview();
    const r = await req('POST', `/api/import/${p.batchId}/commit`, { as: 'admin', body: { existingAction: 'skip' } });
    assert.deepEqual(r.json, { created: 2, updated: 0, unchanged: 0, skipped: 1, failed: [] });
    const existing = await req('GET', '/api/products/by-code/2MLT3101A');
    assert.equal(existing.json.productName, 'Latex (D75) 9 Zone');

    const imp1 = (await req('GET', `/api/products/${(await req('GET', '/api/products/by-code/IMP-001')).json.id}`)).json;
    assert.equal(imp1.description, 'VS-5, 295" (7,415mm)');
    assert.equal(imp1.model, 'Prime1');
    assert.equal(imp1.unit, 'pcs');
    assert.deepEqual(imp1.suppliers.map((s: any) => s.supplierName), ['K.H. Zipper', 'Soon son']);
    assert.deepEqual(imp1.specifications.map((s: any) => [s.name, s.value]), [['Length', '7415']]);
    assert.match(imp1.notes, /Source row 3 — Model \/ Application: Elite Series; Unit: mtr; Supplier: Soon son/);

    const imp2 = (await req('GET', `/api/products/${(await req('GET', '/api/products/by-code/IMP-002')).json.id}`)).json;
    assert.deepEqual(imp2.suppliers.map((s: any) => s.supplierName), ['BETTERFILL', 'Betterfill']);

    // A batch can only be committed once.
    assert.equal((await req('POST', `/api/import/${p.batchId}/commit`, { as: 'admin', body: { existingAction: 'skip' } })).status, 409);
  });

  it('updates existing products only when chosen, never deleting data', async () => {
    const p = await preview();
    const r = await req('POST', `/api/import/${p.batchId}/commit`, { as: 'admin', body: { existingAction: 'update' } });
    assert.equal(r.json.updated, 1);
    const prod = (await req('GET', `/api/products/${(await req('GET', '/api/products/by-code/2MLT3101A')).json.id}`)).json;
    assert.equal(prod.productName, 'Changed name');
    assert.equal(prod.specifications.find((s: any) => s.name === 'Length').value, '1890');
    assert.ok(prod.specifications.some((s: any) => s.name === 'Density'), 'existing specs are kept');
    assert.deepEqual(prod.suppliers.map((s: any) => s.supplierName), ['Maus', 'LSK']);
    const revs = (await req('GET', `/api/products/${prod.id}/revisions`)).json;
    assert.match(revs[0].changeSummary, /Updated from import/);
  });

  it('rejects files that are not spreadsheets', async () => {
    const mp = multipart({}, { name: 'x.xlsx', data: Buffer.from('not a zip'), type: 'application/octet-stream' });
    assert.equal((await req('POST', '/api/import/preview', { as: 'admin', payload: mp.payload, headers: mp.headers })).status, 400);
    const pdf = multipart({}, { name: 'x.pdf', data: Buffer.from('%PDF-1.4'), type: 'application/pdf' });
    assert.equal((await req('POST', '/api/import/preview', { as: 'admin', payload: pdf.payload, headers: pdf.headers })).status, 400);
  });
});

describe('performance', () => {
  it('searches 5,000 products quickly', async () => {
    await pool.query(`
      INSERT INTO products (product_code, code_key, product_name, description, search_text)
      SELECT 'PERF' || lpad(i::text, 5, '0'), 'PERF' || lpad(i::text, 5, '0'), 'Perf item ' || i,
             'Foam D' || (i % 50) || ', ' || (1000 + i % 900) || 'mm x 1880mm',
             lower('PERF' || lpad(i::text, 5, '0') || ' perf item ' || i || ' foam d' || (i % 50) || ' ' || (1000 + i % 900) || 'mm x 1880mm')
        FROM generate_series(1, 5000) i`);
    await pool.query('ANALYZE products');
    for (const q of ['PERF04321', 'perf item 4321', '1880 x 1321', 'foam d7']) {
      const t = performance.now();
      const r = await req('GET', '/api/search?q=' + encodeURIComponent(q));
      const ms = performance.now() - t;
      assert.ok(r.json.hits.length > 0, q);
      assert.ok(ms < 300, `search "${q}" took ${ms.toFixed(0)} ms`);
    }
    assert.equal((await req('GET', '/api/search?q=PERF04321')).json.hits[0].productCode, 'PERF04321');
  });
});
