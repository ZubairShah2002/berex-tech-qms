import { useState, type ReactNode } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { api, ApiError, FILE_KIND_LABEL, type FileKind, type Product, type Revision } from '../api';
import { loginPath, useAuth } from '../auth';
import { Dialog } from '../components/Dialog';
import { useToast } from '../components/Toast';
import { formatBytes, formatDateTime } from '../util';

const NS = <span className="ns">Not specified</span>;
const show = (v: string | null | undefined): ReactNode => (v === null || v === undefined || v === '' ? NS : v);

function Panel({ title, actions, children, flush }: { title: string; actions?: ReactNode; children: ReactNode; flush?: boolean }) {
  return (
    <section className="panel">
      <div className="panel-head"><h2>{title}</h2>{actions && <div className="btn-row no-print">{actions}</div>}</div>
      <div className={flush ? 'panel-body flush' : 'panel-body'}>{children}</div>
    </section>
  );
}

export function ProductPage() {
  const { id = '' } = useParams();
  const { user, meta } = useAuth();
  const navigate = useNavigate();
  const toast = useToast();
  const qc = useQueryClient();
  const [confirmArchive, setConfirmArchive] = useState(false);

  const product = useQuery({ queryKey: ['product', id], queryFn: () => api.product(id) });

  const setProduct = (p: Product) => {
    qc.setQueryData(['product', id], p);
    void qc.invalidateQueries({ queryKey: ['revisions', id] });
    void qc.invalidateQueries({ queryKey: ['search'] });
    void qc.invalidateQueries({ queryKey: ['products'] });
  };

  const archive = useMutation({
    mutationFn: (archived: boolean) => (archived ? api.archive(id) : api.restore(id)),
    onSuccess: (p) => {
      setProduct(p);
      setConfirmArchive(false);
      toast(p.status === 'archived' ? 'Product archived.' : 'Product restored.');
    },
    onError: (e: Error) => toast(e.message, 'error'),
  });

  if (product.isPending) return <div className="page loading">Loading…</div>;
  if (product.isError) {
    const notFound = product.error instanceof ApiError && product.error.status === 404;
    return (
      <div className="page page-narrow">
        <div className="alert alert-error">{notFound ? 'Product not found.' : (product.error as Error).message}</div>
        <Link to="/" className="btn">Back to search</Link>
      </div>
    );
  }
  const p = product.data;
  const editPath = `/products/${p.id}/edit`;
  const canSeeHistory = !!user || meta?.publicRevisionHistory;

  return (
    <div className="page">
      <div className="crumbs no-print"><a href="/" onClick={(e) => { e.preventDefault(); navigate(-1); }}>← Back</a></div>

      {p.status === 'archived' && (
        <div className="alert alert-warning">
          <strong>Archived product.</strong> Not shown in search results{p.archivedAt ? ` (archived ${formatDateTime(p.archivedAt)})` : ''}.
          Do not use for current production unless confirmed.
        </div>
      )}

      <div className="product-head">
        <div>
          <div className="product-code">{p.productCode}</div>
          <div className="product-name">{p.productName}</div>
          <div className="product-sub">
            <span className="rev-tag">{p.currentRevision}</span>{' '}
            Updated {formatDateTime(p.updatedAt)}{p.updatedBy ? ` by ${p.updatedBy}` : ''}
            {p.status === 'archived' && <> <span className="badge badge-archived">Archived</span></>}
          </div>
        </div>
        <div className="btn-row no-print">
          <Link className="btn btn-primary" to={user ? editPath : loginPath(editPath)}>Edit Product</Link>
          {user && (p.status === 'active'
            ? <button className="btn btn-danger" onClick={() => setConfirmArchive(true)}>Archive</button>
            : <button className="btn" onClick={() => archive.mutate(false)} disabled={archive.isPending}>Restore</button>)}
          <button className="btn" onClick={() => window.print()}>Print</button>
        </div>
      </div>

      <Panel title="Product Information" flush>
        <dl className="kv">
          <div><dt>Product Code</dt><dd className="mono">{p.productCode}</dd></div>
          <div><dt>Product Name</dt><dd>{p.productName}</dd></div>
          <div className="wide"><dt>Description</dt><dd>{show(p.description)}</dd></div>
          <div><dt>Size</dt><dd>{show(p.size)}</dd></div>
          <div><dt>Variant</dt><dd>{show(p.variant)}</dd></div>
          <div><dt>Model / Application</dt><dd>{show(p.model)}</dd></div>
          <div><dt>Material</dt><dd>{show(p.material)}</dd></div>
          <div><dt>Group</dt><dd>{show(p.category)}</dd></div>
          <div><dt>Unit</dt><dd>{show(p.unit)}</dd></div>
          <div><dt>Supplier</dt><dd>{p.suppliers.length ? p.suppliers.map((s) => s.supplierName).join(', ') : NS}</dd></div>
          <div><dt>Current Revision</dt><dd><strong>{p.currentRevision}</strong></dd></div>
        </dl>
      </Panel>

      <Panel title="Product Specification" flush>
        {p.specifications.length === 0 ? (
          <div className="panel-empty">{NS}{user && <> — <Link to={editPath}>add specification</Link></>}</div>
        ) : (
          <table className="grid stack">
            <thead><tr><th>Specification</th><th>Value</th><th>Unit</th><th>Tolerance</th></tr></thead>
            <tbody>
              {p.specifications.map((s) => (
                <tr key={s.id}>
                  <td className="stack-title">{s.name}</td>
                  <td data-label="Value"><strong>{show(s.value)}</strong></td>
                  <td data-label="Unit">{s.unit ?? '—'}</td>
                  <td data-label="Tolerance">{s.tolerance ?? '—'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </Panel>

      <Panel title="Inspection Requirements" flush>
        {p.inspection.length === 0 ? <div className="panel-empty">{NS}</div> : (
          <table className="grid stack">
            <thead><tr><th>Check Point</th><th>Specification</th><th>Tolerance</th><th>Inspection Method</th></tr></thead>
            <tbody>
              {p.inspection.map((r) => (
                <tr key={r.id}>
                  <td className="stack-title">{r.checkPoint}</td>
                  <td data-label="Specification">{show(r.specification)}</td>
                  <td data-label="Tolerance">{r.tolerance ?? '—'}</td>
                  <td data-label="Inspection Method">{show(r.method)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </Panel>

      <Panel title="Packaging Requirements" flush>
        {p.packaging.length === 0 ? <div className="panel-empty">{NS}</div> : (
          <table className="grid stack">
            <thead><tr><th style={{ width: '30%' }}>Item</th><th>Requirement</th></tr></thead>
            <tbody>
              {p.packaging.map((r) => (
                <tr key={r.id}>
                  <td className="stack-title">{r.item}</td>
                  <td data-label="Requirement" className="pre">{show(r.requirement)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </Panel>

      <Panel title="Supplier Information" flush>
        {p.suppliers.length === 0 ? <div className="panel-empty">{NS}</div> : (
          <table className="grid stack">
            <thead><tr><th>Supplier Name</th><th>Supplier Code</th><th>Supplier Part No.</th><th>Supplier Notes</th></tr></thead>
            <tbody>
              {p.suppliers.map((s) => (
                <tr key={s.id}>
                  <td className="stack-title">{s.supplierName}</td>
                  <td data-label="Supplier Code">{show(s.supplierCode)}</td>
                  <td data-label="Supplier Part No.">{show(s.partNumber)}</td>
                  <td data-label="Supplier Notes" className="pre">{show(s.notes)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </Panel>

      <FilesPanel product={p} canEdit={!!user} onChange={setProduct} />

      <Panel title="Notes">
        {p.notes ? <div className="pre">{p.notes}</div> : NS}
      </Panel>

      {canSeeHistory && <RevisionHistory productId={p.id} currentNumber={p.revisionNumber} onRestored={setProduct} />}

      {confirmArchive && (
        <Dialog
          title="Archive product?"
          onClose={() => setConfirmArchive(false)}
          actions={<>
            <button className="btn" onClick={() => setConfirmArchive(false)}>Cancel</button>
            <button className="btn btn-danger" onClick={() => archive.mutate(true)} disabled={archive.isPending}>Archive Product</button>
          </>}
        >
          <p><span className="mono">{p.productCode}</span> will no longer appear in search results. Its data and history are kept, and it can be restored at any time.</p>
        </Dialog>
      )}
    </div>
  );
}

function FilesPanel({ product, canEdit, onChange }: { product: Product; canEdit: boolean; onChange: (p: Product) => void }) {
  const toast = useToast();
  const [kind, setKind] = useState<FileKind>('photo');
  const [caption, setCaption] = useState('');
  const [file, setFile] = useState<File | null>(null);
  const [inputKey, setInputKey] = useState(0);
  const [toDelete, setToDelete] = useState<string | null>(null);

  const upload = useMutation({
    mutationFn: () => api.uploadFile(product.id, file!, kind, caption),
    onSuccess: (p) => {
      onChange(p);
      setFile(null); setCaption(''); setInputKey((k) => k + 1);
      toast(`File uploaded. Now ${p.currentRevision}.`);
    },
    onError: (e: Error) => toast(e.message, 'error'),
  });
  const remove = useMutation({
    mutationFn: (fileId: string) => api.deleteFile(product.id, fileId),
    onSuccess: (p) => { onChange(p); setToDelete(null); toast(`File removed. Now ${p.currentRevision}.`); },
    onError: (e: Error) => toast(e.message, 'error'),
  });

  if (!canEdit && product.files.length === 0) {
    return <Panel title="Images & Drawings"><span className="ns">No image available</span></Panel>;
  }
  return (
    <Panel title="Images & Drawings">
      {product.files.length === 0 ? <span className="ns">No image available</span> : (
        <div className="files">
          {product.files.map((f) => {
            const url = `/api/files/${f.id}`;
            return (
              <div className="file-card" key={f.id}>
                <a className="thumb" href={url} target="_blank" rel="noopener">
                  {f.contentType === 'application/pdf' ? <span className="pdf">PDF</span> : <img src={url} alt={f.caption ?? f.fileName} loading="lazy" />}
                </a>
                <div className="meta">
                  <strong>{FILE_KIND_LABEL[f.kind]}</strong>
                  {f.caption && <div>{f.caption}</div>}
                  <div className="faint" style={{ wordBreak: 'break-all' }}>{f.fileName} · {formatBytes(f.sizeBytes)}</div>
                  {canEdit && (
                    <button className="linklike no-print" style={{ color: 'var(--danger)', marginTop: 4 }} onClick={() => setToDelete(f.id)}>Remove</button>
                  )}
                </div>
              </div>
            );
          })}
        </div>
      )}
      {canEdit && (
        <form className="upload-form" onSubmit={(e) => { e.preventDefault(); if (file) upload.mutate(); }}>
          <div className="field">
            <label htmlFor="file-kind">Type</label>
            <select id="file-kind" className="select" value={kind} onChange={(e) => setKind(e.target.value as FileKind)}>
              {Object.entries(FILE_KIND_LABEL).map(([k, l]) => <option key={k} value={k}>{l}</option>)}
            </select>
          </div>
          <div className="field">
            <label htmlFor="file-input">File (JPEG, PNG, WebP, GIF or PDF)</label>
            <input key={inputKey} id="file-input" className="input" style={{ paddingTop: 5 }} type="file"
              accept="image/jpeg,image/png,image/webp,image/gif,application/pdf"
              onChange={(e) => setFile(e.target.files?.[0] ?? null)} />
          </div>
          <div className="field">
            <label htmlFor="file-caption">Caption (optional)</label>
            <input id="file-caption" className="input" value={caption} maxLength={300} onChange={(e) => setCaption(e.target.value)} />
          </div>
          <button className="btn btn-primary" type="submit" disabled={!file || upload.isPending}>{upload.isPending ? 'Uploading…' : 'Upload'}</button>
        </form>
      )}
      {toDelete && (
        <Dialog title="Remove file?" onClose={() => setToDelete(null)} actions={<>
          <button className="btn" onClick={() => setToDelete(null)}>Cancel</button>
          <button className="btn btn-danger" onClick={() => remove.mutate(toDelete)} disabled={remove.isPending}>Remove File</button>
        </>}>
          <p>The file will be removed from this product. The removal is recorded in the revision history.</p>
        </Dialog>
      )}
    </Panel>
  );
}

function RevisionHistory({ productId, currentNumber, onRestored }: { productId: string; currentNumber: number; onRestored: (p: Product) => void }) {
  const { user } = useAuth();
  const toast = useToast();
  const [open, setOpen] = useState<string | null>(null);
  const [restoreRev, setRestoreRev] = useState<Revision | null>(null);
  const revisions = useQuery({ queryKey: ['revisions', productId, currentNumber], queryFn: () => api.revisions(productId) });
  const restore = useMutation({
    mutationFn: (r: Revision) => api.restoreRevision(productId, r.id),
    onSuccess: (res) => { onRestored(res.product); setRestoreRev(null); toast(`Restored. Now ${res.currentRevision}.`); },
    onError: (e: Error) => { setRestoreRev(null); toast(e.message, 'error'); },
  });

  return (
    <Panel title="Revision History" flush>
      {revisions.isPending ? <div className="loading">Loading…</div> : revisions.isError ? (
        <div className="panel-empty muted">{(revisions.error as Error).message}</div>
      ) : (
        <table className="grid stack">
          <thead><tr><th>Revision</th><th>Date</th><th>Changed By</th><th>Change</th></tr></thead>
          <tbody>
            {revisions.data.map((r) => (
              <tr key={r.id}>
                <td className="stack-title nowrap">
                  {r.revisionLabel}{r.revisionNumber === currentNumber && <span className="faint"> (current)</span>}
                </td>
                <td data-label="Date" className="nowrap">{formatDateTime(r.changedAt)}</td>
                <td data-label="Changed By">{r.changedBy}</td>
                <td data-label="Change">
                  {r.changeSummary}
                  {r.changes.length > 0 && r.revisionNumber > 1 && (
                    <> {' '}<button className="linklike no-print" onClick={() => setOpen(open === r.id ? null : r.id)}>
                      {open === r.id ? 'Hide details' : 'Details'}
                    </button></>
                  )}
                  {user?.role === 'admin' && r.revisionNumber !== currentNumber && (
                    <> · <button className="linklike no-print" onClick={() => setRestoreRev(r)}>Restore this revision</button></>
                  )}
                  {open === r.id && (
                    <div className="rev-changes table-wrap">
                      <table>
                        <thead><tr><th>Section</th><th>Item</th><th>Field</th><th>Previous</th><th>New</th></tr></thead>
                        <tbody>
                          {r.changes.map((c, i) => (
                            <tr key={i}>
                              <td>{c.section}</td><td>{c.item}</td><td>{c.field}</td>
                              <td className="old pre">{c.oldValue ?? '—'}</td><td className="new pre">{c.newValue ?? '—'}</td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                    </div>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
      {restoreRev && (
        <Dialog title={`Restore ${restoreRev.revisionLabel}?`} onClose={() => setRestoreRev(null)} actions={<>
          <button className="btn" onClick={() => setRestoreRev(null)}>Cancel</button>
          <button className="btn btn-primary" onClick={() => restore.mutate(restoreRev)} disabled={restore.isPending}>Restore</button>
        </>}>
          <p>The product information and specification from {restoreRev.revisionLabel} will be saved as a new revision.
            Nothing in the history is deleted. Images and drawings are not changed.</p>
        </Dialog>
      )}
    </Panel>
  );
}
