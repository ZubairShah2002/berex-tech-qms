import ExcelJS from 'exceljs';
import { parse as parseCsv } from 'csv-parse/sync';
import type { Queryable, Tx } from './db.js';
import type { AuthUser } from './auth.js';
import { badRequest } from './errors.js';
import { clean } from './text.js';
import {
  createProduct, findByCode, loadContent, parseProductInput, updateProduct, writeAudit,
  type ProductInput,
} from './products.js';

/**
 * Bulk import from Excel (.xlsx) or CSV.
 *
 * Columns are matched by header name (see HEADER_MAP). Columns that are not
 * recognised become specification fields named exactly after their header.
 * Values are imported exactly as they appear (only surrounding whitespace is
 * trimmed). Nothing is written until the user confirms the preview.
 */

type Target =
  | 'productCode' | 'productName' | 'description' | 'size' | 'variant' | 'model' | 'material'
  | 'category' | 'unit' | 'notes' | 'supplier' | 'supplierCode' | 'supplierPartNumber' | 'ignore';

const HEADER_MAP: Record<string, Target> = {
  itemcode: 'productCode', productcode: 'productCode', code: 'productCode', partno: 'productCode',
  partnumber: 'productCode', materialcode: 'productCode', itemno: 'productCode', sku: 'productCode',
  materialname: 'productName', productname: 'productName', name: 'productName', itemname: 'productName', partname: 'productName',
  description: 'description', desc: 'description', productdescription: 'description',
  size: 'size',
  variant: 'variant',
  model: 'model', applicated: 'model', application: 'model', applied: 'model', appliedto: 'model', series: 'model',
  material: 'material',
  group: 'category', category: 'category', materialgroup: 'category',
  unit: 'unit', uom: 'unit',
  notes: 'notes', note: 'notes', remark: 'notes', remarks: 'notes',
  supplier: 'supplier', supplier1: 'supplier', supplier2: 'supplier', supplier3: 'supplier', supplie3: 'supplier',
  suppliername: 'supplier', vendor: 'supplier', vendorname: 'supplier',
  suppliercode: 'supplierCode', vendorcode: 'supplierCode',
  supplierpartnumber: 'supplierPartNumber', supplierpartno: 'supplierPartNumber', vendorpartnumber: 'supplierPartNumber',
  no: 'ignore', sn: 'ignore', sno: 'ignore', srno: 'ignore', serialno: 'ignore', '': 'ignore',
};

const headerKey = (h: string) => h.toLowerCase().replace(/[^a-z0-9]/g, '');

interface Column { index: number; header: string; target: Target | 'spec' }

interface SourceRow { row: number; values: (string | null)[] }

export interface PreviewItem {
  productCode: string;
  productName: string;
  status: 'new' | 'existing';
  existingId: string | null;
  existingStatus: string | null;
  rows: number[];
  mergedRows: number;
  input: ProductInput;
}

export interface PreviewProblem { row: number; productCode: string | null; reason: string }

export interface ImportPreview {
  batchId?: string;
  fileName: string;
  sheetName: string | null;
  headerRow: number;
  columns: { header: string; target: string }[];
  totals: {
    totalRows: number; newProducts: number; existingProducts: number;
    missingCodes: number; invalidRows: number; mergedRows: number; ignoredRows: number;
  };
  items: PreviewItem[];
  problems: PreviewProblem[];
}

// ---------------------------------------------------------------------------
// File reading
// ---------------------------------------------------------------------------

function cellText(v: ExcelJS.CellValue): string | null {
  if (v === null || v === undefined) return null;
  if (typeof v === 'string') return v;
  if (typeof v === 'number' || typeof v === 'boolean') return String(v);
  if (v instanceof Date) return v.toISOString().slice(0, 10);
  if (typeof v === 'object') {
    if ('richText' in v) return v.richText.map((t) => t.text).join('');
    if ('formula' in v || 'sharedFormula' in v) {
      const result = (v as { result?: ExcelJS.CellValue }).result;
      return result === undefined ? null : cellText(result);
    }
    if ('text' in v) return String((v as { text: unknown }).text);
    if ('error' in v) return null;
  }
  return null;
}

async function readTable(buffer: Buffer, fileName: string): Promise<{ sheetName: string | null; rows: SourceRow[] }[]> {
  const lower = fileName.toLowerCase();
  if (lower.endsWith('.csv') || lower.endsWith('.txt')) {
    let records: string[][];
    try {
      records = parseCsv(buffer, { bom: true, relax_column_count: true, skip_empty_lines: false, relax_quotes: true });
    } catch (err) {
      throw badRequest(`Could not read the CSV file: ${(err as Error).message}`);
    }
    return [{ sheetName: null, rows: records.map((values, i) => ({ row: i + 1, values: values.map((v) => v ?? null) })) }];
  }
  if (!lower.endsWith('.xlsx') && !lower.endsWith('.xlsm')) {
    throw badRequest('Unsupported file type. Upload an Excel workbook (.xlsx) or a CSV file. For old .xls files, save as .xlsx first.');
  }
  // .xlsx is a zip archive.
  if (buffer.length < 4 || buffer.readUInt32LE(0) !== 0x04034b50) throw badRequest('The file is not a valid .xlsx workbook.');
  const wb = new ExcelJS.Workbook();
  try {
    await wb.xlsx.load(buffer as unknown as ArrayBuffer);
  } catch {
    throw badRequest('The Excel file could not be read. Check that it is a valid .xlsx workbook.');
  }
  return wb.worksheets.map((ws) => {
    const rows: SourceRow[] = [];
    const maxRow = Math.min(ws.rowCount, 50_000);
    for (let r = 1; r <= maxRow; r++) {
      const row = ws.getRow(r);
      const values: (string | null)[] = [];
      for (let c = 1; c <= Math.min(ws.columnCount, 200); c++) values.push(cellText(row.getCell(c).value));
      rows.push({ row: r, values });
    }
    return { sheetName: ws.name, rows };
  });
}

function findHeader(rows: SourceRow[]): { headerIndex: number; columns: Column[] } | null {
  for (let i = 0; i < Math.min(rows.length, 25); i++) {
    const keys = rows[i].values.map((v) => headerKey(v ?? ''));
    if (!keys.some((k) => HEADER_MAP[k] === 'productCode')) continue;
    const columns: Column[] = [];
    let codeSeen = false;
    let nameSeen = false;
    rows[i].values.forEach((raw, index) => {
      const header = clean(raw) ?? '';
      const key = headerKey(header);
      let target: Column['target'] = key in HEADER_MAP ? HEADER_MAP[key] : 'spec';
      if (header === '') target = 'ignore';
      // Only the first code / name column is used; later ones become specs.
      if (target === 'productCode') { if (codeSeen) target = 'spec'; codeSeen = true; }
      if (target === 'productName') { if (nameSeen) target = 'spec'; nameSeen = true; }
      columns.push({ index, header, target });
    });
    return { headerIndex: i, columns };
  }
  return null;
}

// ---------------------------------------------------------------------------
// Preview
// ---------------------------------------------------------------------------

const FIELD_TARGETS = ['productName', 'description', 'size', 'variant', 'model', 'material', 'category', 'unit', 'notes'] as const;
const FIELD_LABELS: Record<string, string> = {
  productName: 'Name', description: 'Description', size: 'Size', variant: 'Variant', model: 'Model / Application',
  material: 'Material', category: 'Group', unit: 'Unit', notes: 'Notes',
};

interface ParsedRow {
  row: number;
  code: string | null;
  fields: Partial<Record<(typeof FIELD_TARGETS)[number], string | null>>;
  suppliers: { supplierName: string; supplierCode: string | null; partNumber: string | null }[];
  specs: { name: string; value: string }[];
  hasData: boolean;
}

function parseRow(src: SourceRow, columns: Column[]): ParsedRow {
  const out: ParsedRow = { row: src.row, code: null, fields: {}, suppliers: [], specs: [], hasData: false };
  let supplierCode: string | null = null;
  let partNumber: string | null = null;
  for (const col of columns) {
    const v = clean(src.values[col.index]);
    if (v === null || col.target === 'ignore') continue;
    out.hasData = true;
    switch (col.target) {
      case 'productCode': out.code = v; break;
      case 'supplier': if (!out.suppliers.some((s) => s.supplierName === v)) out.suppliers.push({ supplierName: v, supplierCode: null, partNumber: null }); break;
      case 'supplierCode': supplierCode = v; break;
      case 'supplierPartNumber': partNumber = v; break;
      case 'spec': out.specs.push({ name: col.header, value: v }); break;
      default: out.fields[col.target] = v;
    }
  }
  if (out.suppliers[0]) { out.suppliers[0].supplierCode = supplierCode; out.suppliers[0].partNumber = partNumber; }
  return out;
}

/**
 * Rows sharing a product code are merged into one product: the first complete
 * row supplies the product fields, every distinct supplier is kept, and any
 * value in a later row that differs from the first is recorded word-for-word
 * in Notes so no source data is lost.
 */
function mergeGroup(rows: ParsedRow[]): { input: ProductInput; mergedRows: number } {
  const master = rows[0];
  const suppliers: ParsedRow['suppliers'] = [];
  const addSupplier = (s: ParsedRow['suppliers'][number]) => {
    if (!suppliers.some((x) => x.supplierName === s.supplierName)) suppliers.push(s);
  };
  master.suppliers.forEach(addSupplier);
  const specs = [...master.specs];
  const extraNotes: string[] = [];
  let merged = 0;
  for (const r of rows.slice(1)) {
    merged++;
    const diffs: string[] = [];
    for (const f of FIELD_TARGETS) {
      const v = r.fields[f] ?? null;
      if (v !== null && v !== (master.fields[f] ?? null)) diffs.push(`${FIELD_LABELS[f]}: ${v}`);
    }
    for (const s of r.specs) {
      const existing = specs.find((x) => x.name === s.name);
      if (!existing) specs.push(s);
      else if (existing.value !== s.value) diffs.push(`${s.name}: ${s.value}`);
    }
        r.suppliers.forEach(addSupplier);
    if (diffs.length) {
      const sup = r.suppliers.length ? `; Supplier: ${r.suppliers.map((s) => s.supplierName).join(', ')}` : '';
      extraNotes.push(`Source row ${r.row} — ${diffs.join('; ')}${sup}`);
    }
  }
  const notes = [master.fields.notes ?? null, extraNotes.length ? `Additional entries in import file:\n${extraNotes.join('\n')}` : null]
    .filter(Boolean).join('\n\n') || null;

  const input = parseProductInput({
    productCode: master.code,
    productName: master.fields.productName,
    description: master.fields.description ?? null,
    size: master.fields.size ?? null,
    variant: master.fields.variant ?? null,
    model: master.fields.model ?? null,
    material: master.fields.material ?? null,
    category: master.fields.category ?? null,
    unit: master.fields.unit ?? null,
    notes,
    specifications: specs.map((s) => ({ name: s.name, value: s.value, unit: null, tolerance: null })),
    inspection: [],
    packaging: [],
    suppliers: suppliers.map((s) => ({ supplierName: s.supplierName, supplierCode: s.supplierCode, partNumber: s.partNumber, notes: null })),
  });
  return { input, mergedRows: merged };
}

export async function buildPreview(db: Queryable, buffer: Buffer, fileName: string): Promise<ImportPreview> {
  const tables = await readTable(buffer, fileName);
  let chosen: { sheetName: string | null; rows: SourceRow[]; header: NonNullable<ReturnType<typeof findHeader>> } | null = null;
  for (const t of tables) {
    const header = findHeader(t.rows);
    if (header) { chosen = { ...t, header }; break; }
  }
  if (!chosen) {
    throw badRequest('No product code column found. The first rows must contain a header such as "Item Code" or "Product Code".');
  }
  const { columns, headerIndex } = chosen.header;
  if (!columns.some((c) => c.target === 'productName')) {
    throw badRequest('No product name column found. Add a column named "Product Name" or "Material Name".');
  }

  const problems: PreviewProblem[] = [];
  const groups = new Map<string, ParsedRow[]>();
  let totalRows = 0;
  let missingCodes = 0;
  let invalidRows = 0;
  let ignoredRows = 0;
  const codeOnlyRows: ParsedRow[] = [];

  for (const src of chosen.rows.slice(headerIndex + 1)) {
    const r = parseRow(src, columns);
    if (!r.hasData) continue;
    totalRows++;
    if (!r.code) {
      missingCodes++;
      problems.push({ row: r.row, productCode: null, reason: 'Missing product code' });
      continue;
    }
    if (r.code.length > 64 || /[\u0000-\u001f\u007f]/.test(r.code)) {
      invalidRows++;
      problems.push({ row: r.row, productCode: r.code, reason: 'Product code is too long or contains invalid characters' });
      continue;
    }
    if (!r.fields.productName) { codeOnlyRows.push(r); continue; }
    const key = r.code.toUpperCase();
    const g = groups.get(key);
    if (g) g.push(r); else groups.set(key, [r]);
  }
  // A row with only a code is invalid unless another row supplies that product's data.
  for (const r of codeOnlyRows) {
    const g = groups.get(r.code!.toUpperCase());
    if (g) {
      if (r.suppliers.length || r.specs.length || Object.keys(r.fields).length) g.push(r);
      else {
        ignoredRows++;
        problems.push({ row: r.row, productCode: r.code, reason: 'Row has no data besides the code; ignored (product data taken from another row)' });
      }
    } else {
      invalidRows++;
      problems.push({ row: r.row, productCode: r.code, reason: 'Missing product name' });
    }
  }

  const items: PreviewItem[] = [];
  let mergedRows = 0;
  for (const rows of groups.values()) {
    try {
      const { input, mergedRows: m } = mergeGroup(rows);
      mergedRows += m;
      const existing = await findByCode(db, input.productCode);
      items.push({
        productCode: input.productCode, productName: input.productName,
        status: existing ? 'existing' : 'new', existingId: existing?.id ?? null, existingStatus: existing?.status ?? null,
        rows: rows.map((r) => r.row), mergedRows: m, input,
      });
    } catch (err) {
      invalidRows += rows.length;
      problems.push({ row: rows[0].row, productCode: rows[0].code, reason: (err as Error).message });
    }
  }
  problems.sort((a, b) => a.row - b.row);

  return {
    fileName,
    sheetName: chosen.sheetName,
    headerRow: chosen.rows[headerIndex].row,
    columns: columns.filter((c) => c.header !== '').map((c) => ({ header: c.header, target: c.target })),
    totals: {
      totalRows,
      newProducts: items.filter((i) => i.status === 'new').length,
      existingProducts: items.filter((i) => i.status === 'existing').length,
      missingCodes,
      invalidRows,
      mergedRows,
      ignoredRows,
    },
    items,
    problems,
  };
}

// ---------------------------------------------------------------------------
// Commit
// ---------------------------------------------------------------------------

/**
 * Applies imported data to an existing product without deleting anything:
 * non-empty imported fields replace the current value, specifications are
 * matched by name, and suppliers that are not yet listed are added.
 */
async function mergeIntoExisting(tx: Tx, productId: string, incoming: ProductInput): Promise<ProductInput> {
  const current = await loadContent(tx, productId, true);
  if (!current) throw new Error('Product disappeared during import');
  const result: ProductInput = {
    ...current,
    specifications: current.specifications.map((s) => ({ ...s })),
    inspection: current.inspection,
    packaging: current.packaging,
    suppliers: current.suppliers.map(({ id, supplierName, supplierCode, partNumber, notes }) => ({ id, supplierName, supplierCode, partNumber, notes })),
  };
  for (const f of ['productName', 'description', 'size', 'variant', 'model', 'material', 'category', 'unit'] as const) {
    if (incoming[f]) (result as Record<string, unknown>)[f] = incoming[f];
  }
  if (incoming.notes && !(current.notes ?? '').includes(incoming.notes)) {
    result.notes = current.notes ? `${current.notes}\n\n${incoming.notes}` : incoming.notes;
  }
  for (const s of incoming.specifications) {
    const existing = result.specifications.find((x) => x.name.toLowerCase() === s.name.toLowerCase());
    if (existing) existing.value = s.value;
    else result.specifications.push(s);
  }
  for (const s of incoming.suppliers) {
    if (!result.suppliers.some((x) => x.supplierName === s.supplierName)) result.suppliers.push(s);
  }
  return result;
}

export interface CommitResult { created: number; updated: number; unchanged: number; skipped: number; failed: { productCode: string; reason: string }[] }

export async function commitImport(
  tx: Tx, preview: ImportPreview, existingAction: 'skip' | 'update', user: AuthUser,
): Promise<CommitResult> {
  const result: CommitResult = { created: 0, updated: 0, unchanged: 0, skipped: 0, failed: [] };
  const summary = `Imported from ${preview.fileName}`;
  for (const item of preview.items) {
    // Re-check against the database: data may have changed since the preview.
    const existing = await findByCode(tx, item.input.productCode);
    await tx.query('SAVEPOINT import_item');
    try {
      if (!existing) {
        await createProduct(tx, item.input, user, { changeSummary: summary, overwriteSupplierCodes: false, auditAction: 'import.create' });
        result.created++;
      } else if (existingAction === 'skip') {
        result.skipped++;
      } else {
        const merged = await mergeIntoExisting(tx, existing.id, item.input);
        const r = await updateProduct(tx, existing.id, merged, user, null, {
          changeSummary: `Updated from import (${preview.fileName})`, overwriteSupplierCodes: false, auditAction: 'import.update',
        });
        if (r.changed) result.updated++; else result.unchanged++;
      }
      await tx.query('RELEASE SAVEPOINT import_item');
    } catch (err) {
      await tx.query('ROLLBACK TO SAVEPOINT import_item');
      result.failed.push({ productCode: item.productCode, reason: (err as Error).message });
    }
  }
  await writeAudit(tx, user, 'import.commit', null,
    `${user.userId} imported ${preview.fileName}: ${result.created} created, ${result.updated} updated, ${result.skipped} skipped, ${result.unchanged} unchanged, ${result.failed.length} failed.`,
    result);
  return result;
}
