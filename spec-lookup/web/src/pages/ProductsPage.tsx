import { useEffect, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { keepPreviousData, useQuery } from '@tanstack/react-query';
import { api } from '../api';
import { loginPath, useAuth } from '../auth';
import { ProductResults } from '../components/ProductResults';

export function ProductsPage() {
  const { user } = useAuth();
  const [params, setParams] = useSearchParams();
  const status = user && params.get('status') === 'archived' ? 'archived' : 'active';
  const page = Math.max(1, Number(params.get('page')) || 1);
  const sort = params.get('sort') ?? 'code';
  const q = params.get('q') ?? '';
  const [filter, setFilter] = useState(q);
  useEffect(() => setFilter(q), [q]);

  const update = (patch: Record<string, string | null>) => {
    const next = new URLSearchParams(params);
    for (const [k, v] of Object.entries(patch)) { if (v) next.set(k, v); else next.delete(k); }
    setParams(next, { replace: true });
  };

  const list = useQuery({
    queryKey: ['products', status, page, sort, q],
    queryFn: () => api.listProducts({ status, page, pageSize: 50, sort, q }),
    placeholderData: keepPreviousData,
  });
  const pages = list.data ? Math.max(1, Math.ceil(list.data.total / list.data.pageSize)) : 1;

  return (
    <div className="page">
      <div className="page-head">
        <h1>{status === 'archived' ? 'Archived Products' : 'Products'}</h1>
        <div className="btn-row">
          <Link className="btn" to={user ? '/products/new' : loginPath('/products/new')}>+ Add Product</Link>
        </div>
      </div>

      {user && (
        <div className="tabs">
          <button className={status === 'active' ? 'active' : ''} onClick={() => update({ status: null, page: null })}>Active</button>
          <button className={status === 'archived' ? 'active' : ''} onClick={() => update({ status: 'archived', page: null })}>Archived</button>
        </div>
      )}

      <form className="toolbar" onSubmit={(e) => { e.preventDefault(); update({ q: filter.trim() || null, page: null }); }}>
        <label htmlFor="pf" className="sr-only">Filter</label>
        <input id="pf" className="input" type="search" placeholder="Filter by code, name, keyword…" value={filter} onChange={(e) => setFilter(e.target.value)} />
        <button className="btn" type="submit">Filter</button>
        {!q && (
          <>
            <label htmlFor="sort" className="muted" style={{ marginLeft: 'auto' }}>Sort</label>
            <select id="sort" className="select" style={{ width: 'auto' }} value={sort} onChange={(e) => update({ sort: e.target.value, page: null })}>
              <option value="code">Product code</option>
              <option value="name">Product name</option>
              <option value="updated">Last updated</option>
            </select>
          </>
        )}
      </form>

      {list.isError && <div className="alert alert-error">{(list.error as Error).message}</div>}
      {list.isPending ? <div className="loading">Loading…</div> : list.data && (
        list.data.items.length === 0 ? (
          <div className="panel"><div className="panel-empty muted">{q ? `No products match "${q}".` : status === 'archived' ? 'No archived products.' : 'No products yet.'}</div></div>
        ) : (
          <>
            <div className="results-meta">
              {q ? `${list.data.total} result${list.data.total === 1 ? '' : 's'} for "${q}"` : `${list.data.total} product${list.data.total === 1 ? '' : 's'}`}
            </div>
            <ProductResults hits={list.data.items} showStatus />
            {!q && pages > 1 && (
              <div className="pager">
                <button className="btn btn-sm" disabled={page <= 1} onClick={() => update({ page: String(page - 1) })}>Previous</button>
                <span className="muted">Page {page} of {pages}</span>
                <button className="btn btn-sm" disabled={page >= pages} onClick={() => update({ page: String(page + 1) })}>Next</button>
              </div>
            )}
          </>
        )
      )}
    </div>
  );
}
