import type { Queryable } from './db.js';
import { codeKey, escapeLike, normalizeForSearch, queryTokens } from './text.js';

export interface SearchHit {
  id: string;
  productCode: string;
  productName: string;
  description: string | null;
  size: string | null;
  variant: string | null;
  model: string | null;
  category: string | null;
  suppliers: string | null;
  currentRevision: string;
  status: string;
  exactCode: boolean;
}

export interface SearchResult { query: string; hits: SearchHit[]; fuzzy: boolean }

const SELECT = `
  p.id, p.product_code, p.product_name, p.description, p.size, p.variant, p.model, p.category,
  p.current_revision, p.status,
  (SELECT string_agg(s.supplier_name, ', ' ORDER BY ps.display_order)
     FROM product_suppliers ps JOIN suppliers s ON s.id = ps.supplier_id
    WHERE ps.product_id = p.id) AS suppliers`;

const toHit = (r: Record<string, unknown>, key: string): SearchHit => ({
  id: r.id as string,
  productCode: r.product_code as string,
  productName: r.product_name as string,
  description: r.description as string | null,
  size: r.size as string | null,
  variant: r.variant as string | null,
  model: r.model as string | null,
  category: r.category as string | null,
  suppliers: r.suppliers as string | null,
  currentRevision: r.current_revision as string,
  status: r.status as string,
  exactCode: r.code_key === key,
});

/**
 * Product search. Ranking: exact product code, then code prefix, then products
 * matching every query word (anywhere in code, name, description, specs, suppliers…),
 * ordered by similarity. If nothing matches every word, falls back to a fuzzy match.
 */
export async function searchProducts(
  db: Queryable, rawQuery: string, opts: { limit?: number; includeArchived?: boolean } = {},
): Promise<SearchResult> {
  const query = rawQuery.trim().slice(0, 200);
  const limit = Math.min(Math.max(opts.limit ?? 25, 1), 100);
  const normalized = normalizeForSearch(query);
  const key = codeKey(query);
  const tokens = queryTokens(normalized);
  if (!key && tokens.length === 0) return { query, hits: [], fuzzy: false };

  const statusFilter = opts.includeArchived ? 'TRUE' : `p.status = 'active'`;
  const params: unknown[] = [key, normalized, limit];
  const tokenConds = tokens.map((t) => {
    if (t.length <= 2) {
      // Short words ("K", "Q", "9") must match a whole word, not any substring.
      params.push(`(^| )${t.replace(/[.\\]/g, '\\$&')}( |$)`);
      return `p.search_text ~ $${params.length}`;
    }
    params.push(`%${escapeLike(t)}%`);
    return `p.search_text LIKE $${params.length}`;
  });
  const allTokens = tokenConds.length ? tokenConds.join(' AND ') : 'FALSE';

  const { rows } = await db.query(
    `SELECT ${SELECT}, p.code_key,
            (CASE WHEN p.code_key = $1 THEN 1000
                  WHEN $1 <> '' AND p.code_key LIKE $1 || '%' THEN 600
                  WHEN lower(p.product_name) = $2 THEN 400
                  ELSE 0 END)
            + similarity(p.search_text, $2) * 100
            + word_similarity($2, lower(p.product_name)) * 60 AS score
       FROM products p
      WHERE ${statusFilter}
        AND (p.code_key = $1 OR ($1 <> '' AND length($1) >= 3 AND p.code_key LIKE $1 || '%') OR (${allTokens}))
      ORDER BY score DESC, p.product_code
      LIMIT $3`,
    params, // code_key holds only letters and digits, so it needs no LIKE escaping
  );
  if (rows.length > 0) return { query, hits: rows.map((r) => toHit(r, key)), fuzzy: false };

  // Fallback: tolerate typos (e.g. "2MLT311A", "latx king").
  if (normalized.length < 3) return { query, hits: [], fuzzy: false };
  const fuzzy = await db.query(
    `SELECT ${SELECT}, p.code_key,
            GREATEST(word_similarity($1, p.search_text), similarity(p.code_key, $2)) AS score
       FROM products p
      WHERE ${statusFilter} AND ($1 <% p.search_text OR p.code_key % $2)
      ORDER BY score DESC, p.product_code
      LIMIT $3`,
    [normalized, key, Math.min(limit, 15)],
  );
  return { query, hits: fuzzy.rows.map((r) => toHit(r, key)), fuzzy: fuzzy.rows.length > 0 };
}
