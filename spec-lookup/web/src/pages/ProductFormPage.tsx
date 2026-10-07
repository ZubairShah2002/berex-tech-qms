import { useEffect, useMemo, useRef, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import {
  api, ApiError, FILE_KIND_LABEL, type FileKind, type InspectionRow, type PackagingRow, type Product,
  type ProductContent, type SpecRow, type SupplierRow,
} from '../api';
import { Dialog } from '../components/Dialog';
import { RowsEditor } from '../components/RowsEditor';
import { useToast } from '../components/Toast';

// New rows get a temporary client id (the server assigns the real one).
const tempId = () => `new:${Math.random().toString(36).slice(2)}${Date.now().toString(36)}`;

const EMPTY: ProductContent = {
  productCode: '', productName: '', description: null, size: null, variant: null, model: null, material: null,
  category: null, unit: null, notes: null, specifications: [], inspection: [], packaging: [], suppliers: [],
};

function toForm(p: Product): ProductContent {
  const { productCode, productName, description, size, variant, model, material, category, unit, notes, specifications, inspection, packaging } = p;
  return {
    productCode, productName, description, size, variant, model, material, category, unit, notes,
    specifications, inspection, packaging,
    suppliers: p.suppliers.map(({ id, supplierName, supplierCode, partNumber, notes: n }) => ({ id, supplierName, supplierCode, partNumber, notes: n })),
  };
}

/** Text fields: empty strings are sent as null. Values are otherwise sent exactly as typed. */
function toPayload(f: ProductContent): ProductContent {
  const n = (v: string | null) => (v === null || v.trim() === '' ? null : v);
  return {
    ...f,
    description: n(f.description), size: n(f.size), variant: n(f.variant), model: n(f.model), material: n(f.material),
    category: n(f.category), unit: n(f.unit), notes: n(f.notes),
    specifications: f.specifications.map((r) => ({ ...r, value: n(r.value), unit: n(r.unit), tolerance: n(r.tolerance) })),
    inspection: f.inspection.map((r) => ({ ...r, specification: n(r.specification), tolerance: n(r.tolerance), method: n(r.method) })),
    packaging: f.packaging.map((r) => ({ ...r, requirement: n(r.requirement) })),
    suppliers: f.suppliers.map((r) => ({ ...r, supplierCode: n(r.supplierCode), partNumber: n(r.partNumber), notes: n(r.notes) })),
  };
}

function firstMissing(f: ProductContent): string | null {
  if (!f.productCode.trim()) return 'Product Code is required.';
  if (!f.productName.trim()) return 'Product Name is required.';
  if (f.specifications.some((r) => !r.name.trim())) return 'Every specification row needs a name. Delete empty rows.';
  if (f.inspection.some((r) => !r.checkPoint.trim())) return 'Every inspection row needs a check point. Delete empty rows.';
  if (f.packaging.some((r) => !r.item.trim())) return 'Every packaging row needs an item. Delete empty rows.';
  if (f.suppliers.some((r) => !r.supplierName.trim())) return 'Every supplier row needs a supplier name. Delete empty rows.';
  return null;
}

const nextLabel = (n: number) => `Rev. ${String(n).padStart(2, '0')}`;

interface Duplicate { id: string; productCode: string; productName: string; status: string }

export function ProductFormPage({ mode }: { mode: 'new' | 'edit' }) {
  const { id = '' } = useParams();
  const navigate = useNavigate();
  const toast = useToast();
  const qc = useQueryClient();

  const existing = useQuery({ queryKey: ['product', id], queryFn: () => api.product(id), enabled: mode === 'edit' });
  const suppliers = useQuery({ queryKey: ['suppliers'], queryFn: api.suppliers, staleTime: 300_000 });

  const [form, setForm] = useState<ProductContent | null>(mode === 'new' ? EMPTY : null);
  const [initial, setInitial] = useState<string>(JSON.stringify(EMPTY));
  const [revisionLabel, setRevisionLabel] = useState('');
  const [changeSummary, setChangeSummary] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [showErrors, setShowErrors] = useState(false);
  const [saving, setSaving] = useState(false);
  const [duplicate, setDuplicate] = useState<Duplicate | null>(null);
  const [codeWarning, setCodeWarning] = useState<Duplicate | null>(null);
  const [pendingFiles, setPendingFiles] = useState<{ file: File; kind: FileKind }[]>([]);
  const baseRevision = useRef<number>(0);
  const saved = useRef(false);

  useEffect(() => {
    if (mode === 'edit' && existing.data && form === null) {
      const f = toForm(existing.data);
      setForm(f);
      setInitial(JSON.stringify(f));
      baseRevision.current = existing.data.revisionNumber;
    }
  }, [mode, existing.data, form]);

  const dirty = form !== null && JSON.stringify(form) !== initial;
  useEffect(() => {
    const onBeforeUnload = (e: BeforeUnloadEvent) => {
      if (dirty && !saved.current) { e.preventDefault(); e.returnValue = ''; }
    };
    window.addEventListener('beforeunload', onBeforeUnload);
    return () => window.removeEventListener('beforeunload', onBeforeUnload);
  }, [dirty]);

  const supplierNames = useMemo(() => suppliers.data?.map((s) => s.supplierName) ?? [], [suppliers.data]);

  if (mode === 'edit' && existing.isError) {
    return <div className="page page-narrow"><div className="alert alert-error">{(existing.error as Error).message}</div></div>;
  }
  if (!form) return <div className="page loading">Loading…</div>;

  const set = <K extends keyof ProductContent>(key: K, value: ProductContent[K]) => setForm((f) => (f ? { ...f, [key]: value } : f));
  const text = (key: 'productCode' | 'productName' | 'description' | 'size' | 'variant' | 'model' | 'material' | 'category' | 'unit' | 'notes') => ({
    id: `f-${key}`,
    value: form[key] ?? '',
    onChange: (e: { target: { value: string } }) => set(key, e.target.value),
  });

  const checkCode = async () => {
    const code = form.productCode.trim();
    setCodeWarning(null);
    if (!code || (mode === 'edit' && existing.data && code.toUpperCase() === existing.data.productCode.toUpperCase())) return;
    try {
      const found = await api.productByCode(code);
      if (found.id !== id) setCodeWarning(found);
    } catch { /* not found = free */ }
  };

  const save = async () => {
    setShowErrors(true);
    const missing = firstMissing(form);
    if (missing) { setError(missing); window.scrollTo({ top: 0, behavior: 'smooth' }); return; }
    setError(null);
    setSaving(true);
    try {
      const payload = toPayload(form);
      let product: Product;
      if (mode === 'new') {
        product = await api.createProduct(payload, revisionLabel.trim() || undefined, changeSummary.trim() || undefined);
        const failed: string[] = [];
        for (const pf of pendingFiles) {
          try { product = await api.uploadFile(product.id, pf.file, pf.kind, ''); } catch (e) { failed.push(`${pf.file.name}: ${(e as Error).message}`); }
        }
        saved.current = true;
        toast('Product added successfully.');
        if (failed.length) toast(`Some files were not uploaded — ${failed.join('; ')}`, 'error');
      } else {
        const res = await api.updateProduct(id, payload, baseRevision.current, revisionLabel.trim() || undefined, changeSummary.trim() || undefined);
        product = res.product;
        saved.current = true;
        toast(res.changed ? `Saved. New revision ${res.currentRevision}.` : 'No changes to save.');
      }
      qc.setQueryData(['product', product.id], product);
      void qc.invalidateQueries({ queryKey: ['search'] });
      void qc.invalidateQueries({ queryKey: ['products'] });
      void qc.invalidateQueries({ queryKey: ['revisions', product.id] });
      void qc.invalidateQueries({ queryKey: ['suppliers'] });
      navigate(`/products/${product.id}`, { replace: true });
    } catch (e) {
      if (e instanceof ApiError && e.code === 'DUPLICATE_CODE') {
        const d = (e.details as { existing?: Duplicate } | undefined)?.existing;
        setDuplicate(d ?? { id: '', productCode: form.productCode, productName: '', status: '' });
      } else if (e instanceof ApiError && e.status === 401) {
        setError('Your session has expired. Open a new tab, log in, then save again (your changes on this page are kept).');
      } else {
        setError((e as Error).message);
        window.scrollTo({ top: 0, behavior: 'smooth' });
      }
    } finally {
      setSaving(false);
    }
  };

  const cancel = () => navigate(mode === 'edit' ? `/products/${id}` : '/');
  const nextRev = mode === 'new' ? nextLabel(1) : nextLabel(baseRevision.current + 1);

  return (
    <div className="page">
      <div className="page-head">
        <div>
          <div className="crumbs">
            {mode === 'edit' && existing.data ? <Link to={`/products/${id}`}>← {existing.data.productCode}</Link> : <Link to="/">← Search</Link>}
          </div>
          <h1>{mode === 'new' ? 'Add Product' : `Edit Product — ${existing.data?.productCode}`}</h1>
          {mode === 'edit' && existing.data && <div className="muted" style={{ fontSize: 13 }}>Current revision: {existing.data.currentRevision}. Saving creates {nextRev}.</div>}
        </div>
      </div>

      {error && <div className="alert alert-error" role="alert">{error}</div>}

      <form onSubmit={(e) => { e.preventDefault(); void save(); }} noValidate>
        <section className="panel">
          <div className="panel-head"><h2>Product Information</h2></div>
          <div className="panel-body">
            <div className="form-grid">
              <div className="field">
                <label htmlFor="f-productCode">Product Code <span className="req">*</span></label>
                <input className="input mono" {...text('productCode')} onBlur={checkCode} maxLength={64}
                  autoCapitalize="characters" autoCorrect="off" spellCheck={false}
                  aria-invalid={showErrors && !form.productCode.trim() ? true : undefined} />
                {codeWarning && (
                  <div className="alert alert-warning" style={{ margin: '6px 0 0' }}>
                    Product code already exists: <Link to={`/products/${codeWarning.id}`}>{codeWarning.productCode} — {codeWarning.productName}</Link>
                    {codeWarning.status === 'archived' && ' (archived)'}
                  </div>
                )}
              </div>
              <div className="field">
                <label htmlFor="f-productName">Product Name <span className="req">*</span></label>
                <input className="input" {...text('productName')} maxLength={300}
                  aria-invalid={showErrors && !form.productName.trim() ? true : undefined} />
              </div>
              <div className="field span-2">
                <label htmlFor="f-description">Description</label>
                <textarea className="textarea" rows={2} {...text('description')} maxLength={4000} />
              </div>
              <div className="field"><label htmlFor="f-size">Size</label><input className="input" {...text('size')} maxLength={200} /></div>
              <div className="field"><label htmlFor="f-variant">Variant</label><input className="input" {...text('variant')} maxLength={200} /></div>
              <div className="field"><label htmlFor="f-model">Model / Application</label><input className="input" {...text('model')} maxLength={300} /></div>
              <div className="field"><label htmlFor="f-material">Material</label><input className="input" {...text('material')} maxLength={500} /></div>
              <div className="field"><label htmlFor="f-category">Group</label><input className="input" {...text('category')} maxLength={200} placeholder="e.g. Raw Material" /></div>
              <div className="field"><label htmlFor="f-unit">Unit</label><input className="input" {...text('unit')} maxLength={50} placeholder="e.g. pcs, m, kg" /></div>
            </div>
          </div>
        </section>

        <section className="panel">
          <div className="panel-head"><h2>Product Specification</h2></div>
          <RowsEditor<SpecRow>
            idPrefix="spec" rows={form.specifications} showErrors={showErrors}
            onChange={(rows) => set('specifications', rows)}
            empty={() => ({ id: tempId(), name: '', value: '', unit: '', tolerance: '' })}
            addLabel="Add Specification"
            columns={[
              { key: 'name', label: 'Specification', width: 'minmax(0,1.3fr)', required: true, placeholder: 'e.g. Length' },
              { key: 'value', label: 'Value', width: 'minmax(0,1fr)', placeholder: 'e.g. 1880' },
              { key: 'unit', label: 'Unit', width: 'minmax(0,0.5fr)', placeholder: 'mm' },
              { key: 'tolerance', label: 'Tolerance', width: 'minmax(0,0.7fr)', placeholder: '±5' },
            ]}
          />
        </section>

        <section className="panel">
          <div className="panel-head"><h2>Inspection Requirements</h2></div>
          <RowsEditor<InspectionRow>
            idPrefix="insp" rows={form.inspection} showErrors={showErrors}
            onChange={(rows) => set('inspection', rows)}
            empty={() => ({ id: tempId(), checkPoint: '', specification: '', tolerance: '', method: '' })}
            addLabel="Add Inspection Checkpoint"
            columns={[
              { key: 'checkPoint', label: 'Check Point', width: 'minmax(0,1fr)', required: true, placeholder: 'e.g. Length' },
              { key: 'specification', label: 'Specification', width: 'minmax(0,1fr)', placeholder: 'e.g. 1880 mm' },
              { key: 'tolerance', label: 'Tolerance', width: 'minmax(0,0.7fr)', placeholder: '±5 mm' },
              { key: 'method', label: 'Inspection Method', width: 'minmax(0,1fr)', placeholder: 'e.g. Measuring Tape' },
            ]}
          />
        </section>

        <section className="panel">
          <div className="panel-head"><h2>Packaging Requirements</h2></div>
          <RowsEditor<PackagingRow>
            idPrefix="pack" rows={form.packaging} showErrors={showErrors}
            onChange={(rows) => set('packaging', rows)}
            empty={() => ({ id: tempId(), item: '', requirement: '' })}
            addLabel="Add Packaging Requirement"
            columns={[
              { key: 'item', label: 'Item', width: 'minmax(0,0.8fr)', required: true, placeholder: 'e.g. Carton specification' },
              { key: 'requirement', label: 'Requirement', width: 'minmax(0,2fr)', multiline: true },
            ]}
          />
        </section>

        <section className="panel">
          <div className="panel-head"><h2>Supplier Information</h2></div>
          <datalist id="supplier-names">{supplierNames.map((n) => <option key={n} value={n} />)}</datalist>
          <RowsEditor<SupplierRow>
            idPrefix="sup" rows={form.suppliers} showErrors={showErrors}
            onChange={(rows) => set('suppliers', rows)}
            empty={() => ({ id: tempId(), supplierName: '', supplierCode: '', partNumber: '', notes: '' })}
            addLabel="Add Supplier"
            columns={[
              { key: 'supplierName', label: 'Supplier Name', width: 'minmax(0,1.2fr)', required: true, list: 'supplier-names' },
              { key: 'supplierCode', label: 'Supplier Code', width: 'minmax(0,0.6fr)' },
              { key: 'partNumber', label: 'Supplier Part No.', width: 'minmax(0,0.8fr)' },
              { key: 'notes', label: 'Supplier Notes', width: 'minmax(0,1fr)' },
            ]}
          />
        </section>

        <section className="panel">
          <div className="panel-head"><h2>Notes</h2></div>
          <div className="panel-body">
            <label htmlFor="f-notes" className="sr-only">Notes</label>
            <textarea className="textarea" rows={4} {...text('notes')} maxLength={10000} />
          </div>
        </section>

        {mode === 'new' ? (
          <section className="panel">
            <div className="panel-head"><h2>Images & Drawings</h2></div>
            <div className="panel-body">
              <PendingFiles files={pendingFiles} onChange={setPendingFiles} />
            </div>
          </section>
        ) : (
          <div className="alert alert-info">Images and drawings are added or removed on the product page.</div>
        )}

        <section className="panel">
          <div className="panel-head"><h2>Revision</h2></div>
          <div className="panel-body">
            <div className="form-grid">
              <div className="field">
                <label htmlFor="f-rev">Revision</label>
                <input id="f-rev" className="input" value={revisionLabel} placeholder={nextRev} maxLength={40} onChange={(e) => setRevisionLabel(e.target.value)} />
                <span className="hint">Leave blank to use {nextRev}.</span>
              </div>
              <div className="field">
                <label htmlFor="f-summary">{mode === 'new' ? 'Remarks' : 'Reason for change'}</label>
                <input id="f-summary" className="input" value={changeSummary} maxLength={1000} onChange={(e) => setChangeSummary(e.target.value)}
                  placeholder={mode === 'new' ? 'Initial specification' : 'Leave blank to describe changes automatically'} />
              </div>
            </div>
          </div>
        </section>

        <div className="form-actions">
          <span className="grow muted" style={{ fontSize: 13 }}>{dirty ? 'Unsaved changes' : ''}</span>
          <button type="button" className="btn" onClick={cancel}>Cancel</button>
          <button type="submit" className="btn btn-primary" disabled={saving}>{saving ? 'Saving…' : mode === 'new' ? 'Save Product' : 'Save Changes'}</button>
        </div>
      </form>

      {duplicate && (
        <Dialog title="Product code already exists." onClose={() => setDuplicate(null)} actions={<>
          {duplicate.id && <Link className="btn" to={`/products/${duplicate.id}`}>View Product</Link>}
          {duplicate.id && <Link className="btn btn-primary" to={`/products/${duplicate.id}/edit`}>Edit Existing Product</Link>}
          <button className="btn" onClick={() => setDuplicate(null)}>Cancel</button>
        </>}>
          <p>
            <span className="mono"><strong>{duplicate.productCode}</strong></span>
            {duplicate.productName && <> — {duplicate.productName}</>}
            {duplicate.status === 'archived' && <> (archived)</>}
          </p>
          <p className="muted">The existing product was not changed. Use a different product code, or edit the existing product.</p>
        </Dialog>
      )}
    </div>
  );
}

function PendingFiles({ files, onChange }: { files: { file: File; kind: FileKind }[]; onChange: (f: { file: File; kind: FileKind }[]) => void }) {
  const [kind, setKind] = useState<FileKind>('photo');
  const [key, setKey] = useState(0);
  return (
    <>
      {files.length > 0 && (
        <ul style={{ margin: '0 0 10px', paddingLeft: 18 }}>
          {files.map((f, i) => (
            <li key={i}>
              {FILE_KIND_LABEL[f.kind]}: {f.file.name}{' '}
              <button type="button" className="linklike" onClick={() => onChange(files.filter((_, j) => j !== i))}>Remove</button>
            </li>
          ))}
        </ul>
      )}
      <div className="form-grid">
        <div className="field">
          <label htmlFor="pf-kind">Type</label>
          <select id="pf-kind" className="select" value={kind} onChange={(e) => setKind(e.target.value as FileKind)}>
            {Object.entries(FILE_KIND_LABEL).map(([k, l]) => <option key={k} value={k}>{l}</option>)}
          </select>
        </div>
        <div className="field">
          <label htmlFor="pf-file">File (JPEG, PNG, WebP, GIF or PDF)</label>
          <input key={key} id="pf-file" className="input" style={{ paddingTop: 5 }} type="file"
            accept="image/jpeg,image/png,image/webp,image/gif,application/pdf"
            onChange={(e) => {
              const f = e.target.files?.[0];
              if (f) { onChange([...files, { file: f, kind }]); setKey((k) => k + 1); }
            }} />
          <span className="hint">Files are uploaded when the product is saved.</span>
        </div>
      </div>
    </>
  );
}
