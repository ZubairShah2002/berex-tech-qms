/** Date as DD/MM/YYYY HH:mm (local time). */
export function formatDateTime(iso: string): string {
  const d = new Date(iso);
  const p = (n: number) => String(n).padStart(2, '0');
  return `${p(d.getDate())}/${p(d.getMonth() + 1)}/${d.getFullYear()} ${p(d.getHours())}:${p(d.getMinutes())}`;
}

export function formatDate(iso: string): string {
  return formatDateTime(iso).slice(0, 10);
}

export function formatBytes(n: number): string {
  if (n < 1024) return `${n} B`;
  if (n < 1024 * 1024) return `${(n / 1024).toFixed(0)} KB`;
  return `${(n / 1024 / 1024).toFixed(1)} MB`;
}

// ---- Recent searches (kept on this device only) ----

const RECENT_KEY = 'specLookup.recentSearches';

export function getRecentSearches(): string[] {
  try {
    const v = JSON.parse(localStorage.getItem(RECENT_KEY) ?? '[]');
    return Array.isArray(v) ? v.filter((x) => typeof x === 'string').slice(0, 10) : [];
  } catch {
    return [];
  }
}

export function addRecentSearch(term: string): void {
  const t = term.trim();
  if (!t) return;
  try {
    const list = [t, ...getRecentSearches().filter((x) => x.toLowerCase() !== t.toLowerCase())].slice(0, 10);
    localStorage.setItem(RECENT_KEY, JSON.stringify(list));
  } catch { /* storage unavailable */ }
}

export function clearRecentSearches(): void {
  try { localStorage.removeItem(RECENT_KEY); } catch { /* storage unavailable */ }
}

/** Removes spoken command words so "Show me 2MLT3101A" searches for "2MLT3101A". */
export function cleanSpokenQuery(text: string): string {
  return text
    .trim()
    .replace(/^(please\s+)?((show|find|search|look\s*up|open|get|display|check)(\s+(me|for|up))*)\s+/i, '')
    .replace(/^(the\s+)?(product|item|code|spec(ification)?s?)\s+(for\s+)?/i, '')
    .replace(/[.?!]+$/, '')
    .trim();
}
