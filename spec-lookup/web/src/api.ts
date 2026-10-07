// Typed client for the Spec Lookup API. All requests go to the same origin.

export interface User {
  id: string;
  userId: string;
  displayName: string;
  role: 'admin' | 'qc';
  canImport: boolean;
}

export interface Meta { siteName: string; publicRevisionHistory: boolean; user: User | null }

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
  exactCode?: boolean;
}

export interface SearchResult { query: string; hits: SearchHit[]; fuzzy: boolean }

export interface SpecRow { id?: string; name: string; value: string | null; unit: string | null; tolerance: string | null }
export interface InspectionRow { id?: string; checkPoint: string; specification: string | null; tolerance: string | null; method: string | null }
export interface PackagingRow { id?: string; item: string; requirement: string | null }
export interface SupplierRow { id?: string; supplierName: string; supplierCode: string | null; partNumber: string | null; notes: string | null }

export interface ProductContent {
  productCode: string;
  productName: string;
  description: string | null;
  size: string | null;
  variant: string | null;
  model: string | null;
  material: string | null;
  category: string | null;
  unit: string | null;
  notes: string | null;
  specifications: SpecRow[];
  inspection: InspectionRow[];
  packaging: PackagingRow[];
  suppliers: SupplierRow[];
}

export interface ProductFile {
  id: string; kind: FileKind; caption: string | null; fileName: string; contentType: string;
  sizeBytes: number; uploadedAt: string; uploadedBy: string | null;
}

export type FileKind = 'photo' | 'drawing' | 'dimension_drawing' | 'reference';

export interface Product extends ProductContent {
  id: string;
  status: 'active' | 'archived';
  revisionNumber: number;
  currentRevision: string;
  files: ProductFile[];
  createdAt: string;
  updatedAt: string;
  archivedAt: string | null;
  createdBy: string | null;
  updatedBy: string | null;
}

export interface Change { section: string; item: string; field: string; oldValue: string | null; newValue: string | null }

export interface Revision {
  id: string; revisionNumber: number; revisionLabel: string; changedBy: string; changedAt: string;
  changeSummary: string; changes: Change[];
}

export interface ProductList { items: (SearchHit & { updatedAt?: string })[]; total: number; page: number; pageSize: number; fuzzy: boolean }

export interface AdminUser extends User { status: 'active' | 'disabled'; createdAt: string; lastLoginAt: string | null }

export interface AuditEntry { id: number; at: string; userName: string; action: string; productId: string | null; productCode: string | null; detail: string }

export interface ImportPreview {
  batchId: string;
  fileName: string;
  sheetName: string | null;
  headerRow: number;
  columns: { header: string; target: string }[];
  totals: { totalRows: number; newProducts: number; existingProducts: number; missingCodes: number; invalidRows: number; mergedRows: number; ignoredRows: number };
  items: {
    productCode: string; productName: string; status: 'new' | 'existing'; existingId: string | null; existingStatus: string | null;
    rows: number[]; mergedRows: number; description: string | null; model: string | null; unit: string | null;
    suppliers: string[]; specCount: number; hasMergeNotes: boolean;
  }[];
  problems: { row: number; productCode: string | null; reason: string }[];
}

export interface ImportResult { created: number; updated: number; unchanged: number; skipped: number; failed: { productCode: string; reason: string }[] }

export class ApiError extends Error {
  constructor(public status: number, public code: string, message: string, public details?: unknown) {
    super(message);
  }
}

async function request<T>(method: string, url: string, body?: unknown): Promise<T> {
  const init: RequestInit = { method, credentials: 'same-origin', headers: {} };
  if (body instanceof FormData) {
    init.body = body;
  } else if (body !== undefined) {
    init.body = JSON.stringify(body);
    (init.headers as Record<string, string>)['Content-Type'] = 'application/json';
  }
  let res: Response;
  try {
    res = await fetch(url, init);
  } catch {
    throw new ApiError(0, 'NETWORK', 'Cannot reach the server. Check your connection.');
  }
  if (res.status === 204) return undefined as T;
  const text = await res.text();
  let data: unknown = null;
  try { data = text ? JSON.parse(text) : null; } catch { /* not JSON */ }
  if (!res.ok) {
    const err = (data as { error?: { code: string; message: string; details?: unknown } } | null)?.error;
    throw new ApiError(res.status, err?.code ?? 'ERROR', err?.message ?? `Request failed (${res.status}).`, err?.details);
  }
  return data as T;
}

const q = (params: Record<string, string | number | undefined>) => {
  const s = new URLSearchParams();
  for (const [k, v] of Object.entries(params)) if (v !== undefined && v !== '') s.set(k, String(v));
  return s.toString();
};

export const api = {
  meta: () => request<Meta>('GET', '/api/meta'),
  search: (query: string, limit = 25) => request<SearchResult>('GET', `/api/search?${q({ q: query, limit })}`),
  listProducts: (p: { q?: string; status?: string; page?: number; pageSize?: number; sort?: string }) =>
    request<ProductList>('GET', `/api/products?${q(p)}`),
  product: (id: string) => request<Product>('GET', `/api/products/${encodeURIComponent(id)}`),
  productByCode: (code: string) =>
    request<{ id: string; productCode: string; productName: string; status: string }>('GET', `/api/products/by-code/${encodeURIComponent(code)}`),
  revisions: (id: string) => request<Revision[]>('GET', `/api/products/${encodeURIComponent(id)}/revisions`),
  createProduct: (product: ProductContent, revisionLabel?: string, changeSummary?: string) =>
    request<Product>('POST', '/api/products', { product, revisionLabel, changeSummary }),
  updateProduct: (id: string, product: ProductContent, expectedRevision: number, revisionLabel?: string, changeSummary?: string) =>
    request<{ changed: boolean; currentRevision: string; product: Product }>('PUT', `/api/products/${id}`, { product, expectedRevision, revisionLabel, changeSummary }),
  archive: (id: string) => request<Product>('POST', `/api/products/${id}/archive`),
  restore: (id: string) => request<Product>('POST', `/api/products/${id}/restore`),
  restoreRevision: (id: string, revisionId: string) =>
    request<{ currentRevision: string; product: Product }>('POST', `/api/products/${id}/revisions/${revisionId}/restore`),
  uploadFile: (id: string, file: File, kind: FileKind, caption: string) => {
    const fd = new FormData();
    fd.append('kind', kind);
    fd.append('caption', caption);
    fd.append('file', file);
    return request<Product>('POST', `/api/products/${id}/files`, fd);
  },
  deleteFile: (id: string, fileId: string) => request<Product>('DELETE', `/api/products/${id}/files/${fileId}`),
  suppliers: () => request<{ id: string; supplierName: string; supplierCode: string | null }[]>('GET', '/api/suppliers'),

  login: (userId: string, password: string) => request<User>('POST', '/api/auth/login', { userId, password }),
  logout: () => request<{ ok: true }>('POST', '/api/auth/logout'),
  changePassword: (currentPassword: string, newPassword: string) =>
    request<{ ok: true }>('POST', '/api/auth/change-password', { currentPassword, newPassword }),

  importPreview: (file: File) => {
    const fd = new FormData();
    fd.append('file', file);
    return request<ImportPreview>('POST', '/api/import/preview', fd);
  },
  importCommit: (batchId: string, existingAction: 'skip' | 'update') =>
    request<ImportResult>('POST', `/api/import/${batchId}/commit`, { existingAction }),
  importCancel: (batchId: string) => request<void>('DELETE', `/api/import/${batchId}`),

  users: () => request<AdminUser[]>('GET', '/api/users'),
  createUser: (u: { userId: string; displayName: string; role: string; canImport: boolean; password: string }) =>
    request<AdminUser>('POST', '/api/users', u),
  updateUser: (id: string, patch: Partial<{ displayName: string; role: string; canImport: boolean; status: string; password: string }>) =>
    request<AdminUser>('PATCH', `/api/users/${id}`, patch),
  settings: () => request<{ siteName: string; publicRevisionHistory: boolean }>('GET', '/api/settings'),
  saveSettings: (s: { siteName: string; publicRevisionHistory: boolean }) => request<typeof s>('PUT', '/api/settings', s),
  audit: (p: { productId?: string; page?: number; pageSize?: number }) => request<AuditEntry[]>('GET', `/api/audit?${q(p)}`),
};

export const FILE_KIND_LABEL: Record<FileKind, string> = {
  photo: 'Product photo',
  drawing: 'Technical drawing',
  dimension_drawing: 'Dimension drawing',
  reference: 'Reference image',
};
