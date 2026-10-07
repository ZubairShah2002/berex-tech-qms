import { useEffect, useRef, useState } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { keepPreviousData, useQuery } from '@tanstack/react-query';
import { api } from '../api';
import { useAuth, loginPath } from '../auth';
import { ProductResults } from '../components/ProductResults';
import { useVoiceSearch } from '../components/useVoiceSearch';
import { addRecentSearch, cleanSpokenQuery, clearRecentSearches, getRecentSearches } from '../util';

function useDebounced<T>(value: T, ms: number): T {
  const [v, setV] = useState(value);
  useEffect(() => {
    const t = setTimeout(() => setV(value), ms);
    return () => clearTimeout(t);
  }, [value, ms]);
  return v;
}

export function HomePage() {
  const [params, setParams] = useSearchParams();
  const urlQuery = params.get('q') ?? '';
  const [text, setText] = useState(urlQuery);
  const [recent, setRecent] = useState(getRecentSearches);
  const navigate = useNavigate();
  const { user } = useAuth();
  const inputRef = useRef<HTMLInputElement>(null);
  const pendingOpen = useRef(false);

  useEffect(() => { setText(urlQuery); }, [urlQuery]);

  const debounced = useDebounced(text.trim(), 180);
  // Keep the query in the URL so Back returns to the same results.
  useEffect(() => {
    if (debounced !== urlQuery) setParams(debounced ? { q: debounced } : {}, { replace: true });
  }, [debounced]); // eslint-disable-line react-hooks/exhaustive-deps

  const search = useQuery({
    queryKey: ['search', debounced],
    queryFn: () => api.search(debounced),
    enabled: debounced.length > 0,
    placeholderData: keepPreviousData,
  });

  const openBest = async (term: string) => {
    const q = term.trim();
    if (!q) return;
    addRecentSearch(q);
    setRecent(getRecentSearches());
    const result = q === debounced && search.data ? search.data : await api.search(q).catch(() => null);
    if (!result) return;
    const top = result.hits[0];
    if (top && !result.fuzzy && (top.exactCode || result.hits.length === 1)) navigate(`/products/${top.id}`);
    else { setText(q); setParams({ q }, { replace: true }); }
  };

  const voice = useVoiceSearch((spoken, final) => {
    const q = cleanSpokenQuery(spoken);
    setText(q);
    if (final) {
      pendingOpen.current = true;
      void openBest(q).finally(() => { pendingOpen.current = false; });
    }
  });

  const hits = search.data?.hits ?? [];
  const showResults = debounced.length > 0;

  return (
    <div className="page">
      <div className="lookup">
        <h1>Product Specification Lookup</h1>
        <form
          className="searchbar"
          role="search"
          onSubmit={(e) => { e.preventDefault(); void openBest(text); }}
        >
          <label htmlFor="q" className="sr-only">Search product code, name or keyword</label>
          <input
            id="q"
            ref={inputRef}
            className="input"
            type="search"
            placeholder="Search product code / name / keyword…"
            value={text}
            onChange={(e) => { setText(e.target.value); voice.setMessage(''); }}
            autoFocus
            autoComplete="off"
            autoCapitalize="off"
            autoCorrect="off"
            spellCheck={false}
            enterKeyHint="search"
          />
          <button
            type="button"
            className={`btn mic${voice.listening ? ' listening' : ''}`}
            onClick={voice.start}
            aria-label={voice.listening ? 'Stop voice search' : 'Voice search'}
            title="Voice search"
          >
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" aria-hidden="true">
              <rect x="9" y="3" width="6" height="11" rx="3" />
              <path d="M5 11a7 7 0 0 0 14 0M12 18v3" />
            </svg>
          </button>
          <button type="submit" className="btn btn-primary btn-search">Search</button>
        </form>
        <div className="voice-msg" aria-live="polite">{voice.message}</div>

        {!showResults && (
          <>
            <div className="lookup-actions">
              <Link className="btn" to={user ? '/products/new' : loginPath('/products/new')}>+ Add Product</Link>
              <Link className="btn" to="/products">Browse all products</Link>
            </div>
            <div className="recent">
              <h2>
                Recent Searches
                {recent.length > 0 && (
                  <button className="linklike" style={{ fontSize: 12, textTransform: 'none', letterSpacing: 0 }}
                    onClick={() => { clearRecentSearches(); setRecent([]); }}>Clear</button>
                )}
              </h2>
              {recent.length === 0 ? (
                <p className="faint" style={{ margin: 0 }}>No recent searches on this device.</p>
              ) : (
                <ul>
                  {recent.map((r) => (
                    <li key={r}><Link to={`/?q=${encodeURIComponent(r)}`}>{r}</Link></li>
                  ))}
                </ul>
              )}
            </div>
          </>
        )}

        {showResults && (
          <>
            <div className="results-meta">
              {search.isError ? (
                <span className="alert alert-error" style={{ display: 'block' }}>{(search.error as Error).message}</span>
              ) : search.isPending ? 'Searching…' : search.data?.fuzzy ? (
                <>No exact match for <strong>{debounced}</strong>. Similar products:</>
              ) : hits.length === 0 ? (
                <>No products found for <strong>{debounced}</strong>.</>
              ) : (
                <>{hits.length}{hits.length === 25 ? '+' : ''} result{hits.length === 1 ? '' : 's'}</>
              )}
            </div>
            {hits.length > 0 && <ProductResults hits={hits} onOpen={() => { addRecentSearch(debounced); }} />}
            {!search.isPending && hits.length === 0 && !search.isError && (
              <div className="lookup-actions">
                <Link className="btn" to={user ? '/products/new' : loginPath('/products/new')}>+ Add Product</Link>
              </div>
            )}
          </>
        )}
      </div>
    </div>
  );
}
