/**
 * Text helpers for search indexing. These only build the hidden search index:
 * the product data shown to users is always stored exactly as entered.
 */

/** Normalizes text for matching: case, digit-group commas, punctuation and spacing. */
export function normalizeForSearch(input: string | null | undefined): string {
  if (!input) return '';
  return input
    .normalize('NFKC')
    .toLowerCase()
    .replace(/(\d),(?=\d{3}(?!\d))/g, '$1') // 1,880 -> 1880
    .replace(/[×*]/g, ' x ')
    .replace(/[^\p{L}\p{N}.±%]+/gu, ' ')
    .replace(/(^|\s)\.+|\.+(?=\s|$)/g, ' ') // stray dots, keep decimals like 2.00
    .replace(/\s+/g, ' ')
    .trim();
}

/** Key used for product-code lookup: uppercase letters and digits only. */
export function codeKey(code: string): string {
  return code.normalize('NFKC').toUpperCase().replace(/[^\p{L}\p{N}]+/gu, '');
}

/** Splits a normalized query into tokens worth matching. */
export function queryTokens(normalized: string): string[] {
  const tokens = normalized.split(' ').filter((t) => t.length > 0 && t !== 'x' && t !== '.');
  return [...new Set(tokens)].slice(0, 12);
}

/** Trims a value and turns empty strings into null. Inner content is never altered. */
export function clean(value: unknown): string | null {
  if (value === null || value === undefined) return null;
  const s = String(value).trim();
  return s === '' ? null : s;
}

export function revisionLabel(n: number): string {
  return `Rev. ${String(n).padStart(2, '0')}`;
}

export function escapeLike(s: string): string {
  return s.replace(/[\\%_]/g, (m) => `\\${m}`);
}
