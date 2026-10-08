/**
 * Schema migrations, applied in order inside the configured schema
 * (the connection's search_path puts that schema first).
 * Never edit a released migration: add a new one.
 */
export const migrations: { version: number; sql: string }[] = [
  {
    version: 1,
    sql: `
CREATE TABLE users (
  id             uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id        text NOT NULL,
  display_name   text NOT NULL DEFAULT '',
  password_hash  text NOT NULL,
  role           text NOT NULL CHECK (role IN ('admin', 'qc')),
  can_import     boolean NOT NULL DEFAULT false,
  status         text NOT NULL DEFAULT 'active' CHECK (status IN ('active', 'disabled')),
  created_at     timestamptz NOT NULL DEFAULT now(),
  updated_at     timestamptz NOT NULL DEFAULT now(),
  last_login_at  timestamptz
);
CREATE UNIQUE INDEX ux_users_user_id ON users (lower(user_id));

CREATE TABLE sessions (
  id            text PRIMARY KEY,           -- sha256 of the cookie token
  user_id       uuid NOT NULL REFERENCES users (id) ON DELETE CASCADE,
  created_at    timestamptz NOT NULL DEFAULT now(),
  last_seen_at  timestamptz NOT NULL DEFAULT now(),
  expires_at    timestamptz NOT NULL
);
CREATE INDEX ix_sessions_user ON sessions (user_id);

CREATE TABLE suppliers (
  id             uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  supplier_name  text NOT NULL,
  supplier_code  text,
  notes          text,
  created_at     timestamptz NOT NULL DEFAULT now(),
  updated_at     timestamptz NOT NULL DEFAULT now()
);
-- Exact (case-sensitive) names: supplier terminology is kept as entered.
CREATE UNIQUE INDEX ux_suppliers_name ON suppliers (btrim(supplier_name));

CREATE TABLE products (
  id               uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  product_code     text NOT NULL,
  code_key         text NOT NULL,             -- uppercase, alphanumerics only; for fast code lookup
  product_name     text NOT NULL,
  description      text,
  size             text,
  variant          text,
  model            text,
  material         text,
  category         text,
  unit             text,
  notes            text,
  status           text NOT NULL DEFAULT 'active' CHECK (status IN ('active', 'archived')),
  revision_number  int  NOT NULL DEFAULT 1,
  current_revision text NOT NULL DEFAULT 'Rev. 01',
  search_text      text NOT NULL DEFAULT '',
  created_at       timestamptz NOT NULL DEFAULT now(),
  created_by       uuid REFERENCES users (id),
  updated_at       timestamptz NOT NULL DEFAULT now(),
  updated_by       uuid REFERENCES users (id),
  archived_at      timestamptz,
  archived_by      uuid REFERENCES users (id)
);
-- Product codes are unique regardless of case or surrounding whitespace, archived or not.
CREATE UNIQUE INDEX ux_products_code ON products (upper(btrim(product_code)));
CREATE INDEX ix_products_code_key ON products (code_key text_pattern_ops);
CREATE INDEX ix_products_status ON products (status);
CREATE INDEX ix_products_name ON products (lower(product_name));
CREATE INDEX ix_products_search_trgm ON products USING gin (search_text gin_trgm_ops);

CREATE TABLE product_specifications (
  id             uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  product_id     uuid NOT NULL REFERENCES products (id) ON DELETE CASCADE,
  spec_name      text NOT NULL,
  value          text,
  unit           text,
  tolerance      text,
  display_order  int NOT NULL DEFAULT 0
);
CREATE INDEX ix_specs_product ON product_specifications (product_id, display_order);

CREATE TABLE product_inspection_requirements (
  id                 uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  product_id         uuid NOT NULL REFERENCES products (id) ON DELETE CASCADE,
  check_point        text NOT NULL,
  specification      text,
  tolerance          text,
  inspection_method  text,
  display_order      int NOT NULL DEFAULT 0
);
CREATE INDEX ix_inspection_product ON product_inspection_requirements (product_id, display_order);

CREATE TABLE product_packaging_requirements (
  id             uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  product_id     uuid NOT NULL REFERENCES products (id) ON DELETE CASCADE,
  item           text NOT NULL,
  requirement    text,
  display_order  int NOT NULL DEFAULT 0
);
CREATE INDEX ix_packaging_product ON product_packaging_requirements (product_id, display_order);

CREATE TABLE product_suppliers (
  id                    uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  product_id            uuid NOT NULL REFERENCES products (id) ON DELETE CASCADE,
  supplier_id           uuid NOT NULL REFERENCES suppliers (id),
  supplier_part_number  text,
  notes                 text,
  display_order         int NOT NULL DEFAULT 0
);
CREATE INDEX ix_product_suppliers_product ON product_suppliers (product_id, display_order);
CREATE INDEX ix_product_suppliers_supplier ON product_suppliers (supplier_id);

CREATE TABLE product_files (
  id            uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  product_id    uuid NOT NULL REFERENCES products (id) ON DELETE CASCADE,
  kind          text NOT NULL CHECK (kind IN ('photo', 'drawing', 'dimension_drawing', 'reference')),
  caption       text,
  file_name     text NOT NULL,
  content_type  text NOT NULL,
  size_bytes    int  NOT NULL,
  sha256        text NOT NULL,
  data          bytea NOT NULL,
  uploaded_by   uuid REFERENCES users (id),
  uploaded_at   timestamptz NOT NULL DEFAULT now(),
  deleted_at    timestamptz,
  deleted_by    uuid REFERENCES users (id)
);
CREATE INDEX ix_files_product ON product_files (product_id) WHERE deleted_at IS NULL;

CREATE TABLE product_revisions (
  id               uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  product_id       uuid NOT NULL REFERENCES products (id) ON DELETE CASCADE,
  revision_number  int  NOT NULL,
  revision_label   text NOT NULL,
  changed_by       uuid REFERENCES users (id),
  changed_by_name  text NOT NULL,
  changed_at       timestamptz NOT NULL DEFAULT now(),
  change_summary   text NOT NULL,
  snapshot         jsonb NOT NULL,
  UNIQUE (product_id, revision_number)
);

CREATE TABLE product_revision_changes (
  id           bigserial PRIMARY KEY,
  revision_id  uuid NOT NULL REFERENCES product_revisions (id) ON DELETE CASCADE,
  section      text NOT NULL,
  item         text NOT NULL,
  field        text NOT NULL,
  old_value    text,
  new_value    text
);
CREATE INDEX ix_revision_changes_revision ON product_revision_changes (revision_id);

CREATE TABLE audit_log (
  id            bigserial PRIMARY KEY,
  at            timestamptz NOT NULL DEFAULT now(),
  user_id       uuid REFERENCES users (id),
  user_name     text NOT NULL,
  action        text NOT NULL,
  product_id    uuid REFERENCES products (id) ON DELETE SET NULL,
  product_code  text,
  detail        text NOT NULL,
  data          jsonb
);
CREATE INDEX ix_audit_at ON audit_log (at DESC);
CREATE INDEX ix_audit_product ON audit_log (product_id, at DESC);

CREATE TABLE settings (
  key    text PRIMARY KEY,
  value  jsonb NOT NULL
);
INSERT INTO settings (key, value) VALUES
  ('publicRevisionHistory', 'true'::jsonb),
  ('siteName', '"Berex Tech"'::jsonb);

CREATE TABLE import_batches (
  id            uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  created_by    uuid NOT NULL REFERENCES users (id),
  created_at    timestamptz NOT NULL DEFAULT now(),
  file_name     text NOT NULL,
  payload       jsonb NOT NULL,
  committed_at  timestamptz,
  result        jsonb
);
`,
  },
  {
    // The header no longer shows a company name unless an admin sets one.
    version: 2,
    sql: `UPDATE settings SET value = '""'::jsonb WHERE key = 'siteName' AND value = '"Berex Tech"'::jsonb;`,
  },
];
