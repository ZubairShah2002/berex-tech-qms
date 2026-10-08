import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useQueryClient } from '@tanstack/react-query';
import { api, type ImportPreview, type ImportResult } from '../api';
import { useToast } from '../components/Toast';

const TARGET_LABEL: Record<string, string> = {
  productCode: 'Product Code', productName: 'Product Name', description: 'Description', size: 'Size', variant: 'Variant',
  model: 'Model / Application', material: 'Material', category: 'Group', unit: 'Unit', notes: 'Notes',
  supplier: 'Supplier', supplierCode: 'Supplier Code', supplierPartNumber: 'Supplier Part No.',
  spec: 'Specification field', ignore: 'Not imported',
};

type Filter = 'all' | 'new' | 'existing' | 'merged';

export function ImportPage() {
  const toast = useToast();
  const qc = useQueryClient();
  const [file, setFile] = useState<File | null>(null);
  const [inputKey, setInputKey] = useState(0);
  const [preview, setPreview] = useState<ImportPreview | null>(null);
  const [existingAction, setExistingAction] = useState<'skip' | 'update'>('skip');
  const [result, setResult] = useState<ImportResult | null>(null);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [filter, setFilter] = useState<Filter>('all');

  const reset = () => { setPreview(null); setResult(null); setFile(null); setInputKey((k) => k + 1); setError(null); setFilter('all'); };

  const runPreview = async () => {
    if (!file) return;
    setBusy(true); setError(null); setResult(null);
    try { setPreview(await api.importPreview(file)); } catch (e) { setError((e as Error).message); } finally { setBusy(false); }
  };

  const commit = async () => {
    if (!preview) return;
    setBusy(true); setError(null);
    try {
      const r = await api.importCommit(preview.batchId, existingAction);
      setResult(r);
      setPreview(null);
      void qc.invalidateQueries();
      toast(`Import complete: ${r.created} added, ${r.updated} updated.`);
    } catch (e) { setError((e as Error).message); } finally { setBusy(false); }
  };

  const cancel = async () => {
    if (preview) await api.importCancel(preview.batchId).catch(() => undefined);
    reset();
    toast('Import cancelled. Nothing was changed.');
  };

  const items = preview?.items.filter((i) =>
    filter === 'all' ? true : filter === 'merged' ? i.mergedRows > 0 : i.status === filter) ?? [];

  return (
    <div className="page">
      <div className="page-head"><h1>Import Products (Excel / CSV)</h1></div>

      {error && <div className="alert alert-error" role="alert">{error}</div>}

      {result && (
        <div className="alert alert-success">
          <strong>Import complete.</strong> {result.created} product{result.created === 1 ? '' : 's'} added,
          {' '}{result.updated} updated, {result.unchanged} unchanged, {result.skipped} skipped
          {result.failed.length > 0 && <>, {result.failed.length} failed</>}.
          {result.failed.length > 0 && (
            <ul>{result.failed.map((f) => <li key={f.productCode}><span className="mono">{f.productCode}</span>: {f.reason}</li>)}</ul>
          )}
          <div className="btn-row" style={{ marginTop: 8 }}>
            <Link className="btn" to="/products?sort=updated">View products</Link>
            <button className="btn" onClick={reset}>Import another file</button>
          </div>
        </div>
      )}

      {!preview && !result && (
        <section className="panel">
          <div className="panel-head"><h2>1. Choose file</h2></div>
          <div className="panel-body">
            <p style={{ marginTop: 0 }}>
              Upload an Excel workbook (.xlsx) or CSV file. The first sheet with a <strong>Product Code</strong> / <strong>Item Code</strong> column is used.
              Recognised columns: Product Code / Item Code, Product Name / Material Name, Description, Size, Variant, Model / Applicated,
              Material, Group, Unit, Supplier (Supplier1–3), Supplier Code, Supplier Part Number, Notes.
              Any other column is imported as a specification field with the column heading as its name.
            </p>
            <p className="muted">Values are imported exactly as written. Nothing is saved until you confirm the preview.</p>
            <div className="btn-row">
              <input key={inputKey} type="file" className="input" style={{ maxWidth: 420, paddingTop: 5 }}
                accept=".xlsx,.xlsm,.csv,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,text/csv"
                onChange={(e) => setFile(e.target.files?.[0] ?? null)} />
              <button className="btn btn-primary" onClick={runPreview} disabled={!file || busy}>{busy ? 'Reading file…' : 'Preview Import'}</button>
            </div>
          </div>
        </section>
      )}

      {preview && (
        <>
          <section className="panel">
            <div className="panel-head"><h2>2. Import Preview — <span className="as-is">{preview.fileName}</span></h2></div>
            <div className="panel-body">
              <p style={{ marginTop: 0 }} className="muted">
                {preview.sheetName ? <>Sheet <strong>{preview.sheetName}</strong>, </> : null}header on row {preview.headerRow}.
              </p>
              <div className="stat-grid">
                <div className="stat"><div className="n">{preview.totals.totalRows}</div><div className="l">Total rows</div></div>
                <div className="stat"><div className="n">{preview.totals.newProducts}</div><div className="l">New products</div></div>
                <div className="stat"><div className="n">{preview.totals.existingProducts}</div><div className="l">Existing product codes</div></div>
                <div className="stat"><div className="n">{preview.totals.mergedRows}</div><div className="l">Rows merged (repeated code)</div></div>
                <div className="stat"><div className="n">{preview.totals.missingCodes}</div><div className="l">Missing product codes</div></div>
                <div className="stat"><div className="n">{preview.totals.invalidRows}</div><div className="l">Invalid rows</div></div>
              </div>
              {preview.totals.mergedRows > 0 && (
                <p className="muted" style={{ marginBottom: 0 }}>
                  Rows that repeat a product code are combined into one product: all suppliers are kept, and any value that
                  differs from the first row is copied word-for-word into the product's Notes so nothing is lost.
                </p>
              )}
            </div>
          </section>

          <section className="panel">
            <div className="panel-head"><h2>Column mapping</h2></div>
            <div className="table-wrap">
              <table className="grid">
                <thead><tr><th>Column in file</th><th>Imported as</th></tr></thead>
                <tbody>{preview.columns.map((c, i) => <tr key={i}><td>{c.header}</td><td>{TARGET_LABEL[c.target] ?? c.target}</td></tr>)}</tbody>
              </table>
            </div>
          </section>

          {preview.problems.length > 0 && (
            <section className="panel">
              <div className="panel-head"><h2>Rows not imported ({preview.problems.length})</h2></div>
              <div className="table-wrap">
                <table className="grid stack">
                  <thead><tr><th>Row</th><th>Product Code</th><th>Reason</th></tr></thead>
                  <tbody>{preview.problems.map((p, i) => (
                    <tr key={i}><td data-label="Row">{p.row}</td><td data-label="Product Code" className="mono">{p.productCode ?? '—'}</td><td data-label="Reason">{p.reason}</td></tr>
                  ))}</tbody>
                </table>
              </div>
            </section>
          )}

          {preview.totals.existingProducts > 0 && (
            <section className="panel">
              <div className="panel-head"><h2>Existing product codes ({preview.totals.existingProducts})</h2></div>
              <div className="panel-body" style={{ display: 'grid', gap: 8 }}>
                <p style={{ margin: 0 }}>These product codes already exist. Choose what to do with them:</p>
                <label className="check"><input type="radio" name="existing" checked={existingAction === 'skip'} onChange={() => setExistingAction('skip')} />
                  Skip — leave existing products unchanged</label>
                <label className="check"><input type="radio" name="existing" checked={existingAction === 'update'} onChange={() => setExistingAction('update')} />
                  Update existing — fill in values from the file, add new specifications and suppliers (nothing is deleted; a new revision is recorded)</label>
              </div>
            </section>
          )}

          <section className="panel">
            <div className="panel-head"><h2>Products in file ({preview.items.length})</h2></div>
            <div className="tabs" style={{ margin: 0, padding: '0 8px' }}>
              {(['all', 'new', 'existing', 'merged'] as Filter[]).map((f) => (
                <button key={f} className={filter === f ? 'active' : ''} onClick={() => setFilter(f)}>
                  {f === 'all' ? 'All' : f === 'new' ? 'New' : f === 'existing' ? 'Existing' : 'Merged rows'}
                </button>
              ))}
            </div>
            <div className="table-wrap">
              <table className="grid stack">
                <thead><tr><th>Product Code</th><th>Product Name / Description</th><th>Status</th><th>Rows</th><th>Supplier</th><th>Unit</th></tr></thead>
                <tbody>
                  {items.slice(0, 500).map((i) => (
                    <tr key={i.productCode}>
                      <td className="col-code stack-title">{i.productCode}</td>
                      <td>{i.productName}{i.description && i.description !== i.productName && <div className="result-desc">{i.description}</div>}
                        {i.hasMergeNotes && <div className="result-desc" style={{ color: 'var(--warning-fg)' }}>Repeated rows differ — details kept in Notes</div>}</td>
                      <td data-label="Status">
                        {i.status === 'new' ? <span className="badge badge-new">New</span> : (
                          <><span className="badge badge-existing">Exists</span>{i.existingStatus === 'archived' && <span className="faint"> (archived)</span>}
                            {i.existingId && <> <Link to={`/products/${i.existingId}`} target="_blank">view</Link></>}</>
                        )}
                      </td>
                      <td data-label="Source rows">{i.rows.join(', ')}</td>
                      <td data-label="Supplier">{i.suppliers.join('; ') || <span className="ns">Not specified</span>}</td>
                      <td data-label="Unit">{i.unit ?? ''}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
              {items.length > 500 && <div className="panel-empty muted">Showing the first 500 of {items.length}.</div>}
            </div>
          </section>

          <div className="form-actions">
            <span className="grow muted" style={{ fontSize: 13 }}>
              {preview.totals.newProducts} new{preview.totals.existingProducts > 0 && `, ${preview.totals.existingProducts} existing will be ${existingAction === 'skip' ? 'skipped' : 'updated'}`}
            </span>
            <button className="btn" onClick={cancel} disabled={busy}>Cancel Import</button>
            <button className="btn btn-primary" onClick={commit}
              disabled={busy || (preview.totals.newProducts === 0 && (existingAction === 'skip' || preview.totals.existingProducts === 0))}>
              {busy ? 'Importing…' : 'Confirm Import'}
            </button>
          </div>
        </>
      )}
    </div>
  );
}
