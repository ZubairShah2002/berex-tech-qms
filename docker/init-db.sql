-- Berex Tech QMS - Database Initialization
-- Creates schemas for each bounded context per the frozen blueprint

-- Module schemas
CREATE SCHEMA IF NOT EXISTS identity;
CREATE SCHEMA IF NOT EXISTS inspection;
CREATE SCHEMA IF NOT EXISTS ncr;
CREATE SCHEMA IF NOT EXISTS capa;
CREATE SCHEMA IF NOT EXISTS document;
CREATE SCHEMA IF NOT EXISTS audit;
CREATE SCHEMA IF NOT EXISTS supplier;
CREATE SCHEMA IF NOT EXISTS calibration;
CREATE SCHEMA IF NOT EXISTS training;
CREATE SCHEMA IF NOT EXISTS catalog;
CREATE SCHEMA IF NOT EXISTS spc;
CREATE SCHEMA IF NOT EXISTS ai_engine;
CREATE SCHEMA IF NOT EXISTS shared;

-- Extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";
CREATE EXTENSION IF NOT EXISTS "btree_gist";

-- Audit log table (shared schema, append-only)
CREATE TABLE IF NOT EXISTS shared.audit_log (
    id              BIGSERIAL PRIMARY KEY,
    tenant_id       UUID NOT NULL,
    user_id         UUID NOT NULL,
    timestamp       TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    entity_type     VARCHAR(256) NOT NULL,
    entity_id       UUID NOT NULL,
    action          VARCHAR(64) NOT NULL,
    old_value       JSONB,
    new_value       JSONB,
    source_ip       VARCHAR(45),
    correlation_id  VARCHAR(64),
    module_name     VARCHAR(128) NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_audit_log_tenant_entity
    ON shared.audit_log (tenant_id, entity_type, entity_id);

CREATE INDEX IF NOT EXISTS ix_audit_log_timestamp
    ON shared.audit_log (timestamp DESC);

CREATE INDEX IF NOT EXISTS ix_audit_log_correlation
    ON shared.audit_log (correlation_id);

-- Domain events outbox table
CREATE TABLE IF NOT EXISTS shared.domain_events_outbox (
    id              BIGSERIAL PRIMARY KEY,
    event_id        UUID NOT NULL UNIQUE,
    event_type      VARCHAR(512) NOT NULL,
    tenant_id       UUID NOT NULL,
    aggregate_type  VARCHAR(256) NOT NULL,
    aggregate_id    UUID NOT NULL,
    payload         JSONB NOT NULL,
    occurred_on     TIMESTAMPTZ NOT NULL,
    processed_on    TIMESTAMPTZ,
    error           TEXT,
    retry_count     INT NOT NULL DEFAULT 0
);

CREATE INDEX IF NOT EXISTS ix_outbox_unprocessed
    ON shared.domain_events_outbox (occurred_on)
    WHERE processed_on IS NULL;

-- Revoke destructive operations on audit_log from application role
REVOKE UPDATE, DELETE ON shared.audit_log FROM berexqms_app;

-- Row-Level Security helper function
CREATE OR REPLACE FUNCTION shared.current_tenant_id()
RETURNS UUID AS $$
BEGIN
    RETURN current_setting('app.current_tenant_id', true)::UUID;
EXCEPTION
    WHEN OTHERS THEN
        RETURN NULL;
END;
$$ LANGUAGE plpgsql STABLE;

-- =============================================================================
-- Identity Module Tables
-- =============================================================================

-- Tenants
CREATE TABLE IF NOT EXISTS identity.tenants (
    id              UUID PRIMARY KEY,
    tenant_id       UUID NOT NULL,
    name            VARCHAR(200) NOT NULL,
    code            VARCHAR(20) NOT NULL,
    is_active       BOOLEAN NOT NULL DEFAULT TRUE,
    contact_email   VARCHAR(254),
    timezone        VARCHAR(50) DEFAULT 'UTC',
    created_by      VARCHAR(100) NOT NULL,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    modified_by     VARCHAR(100),
    modified_at     TIMESTAMPTZ
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_tenants_code
    ON identity.tenants (code);

-- Users
CREATE TABLE IF NOT EXISTS identity.users (
    id                      UUID PRIMARY KEY,
    tenant_id               UUID NOT NULL,
    email                   VARCHAR(254) NOT NULL,
    first_name              VARCHAR(100) NOT NULL,
    last_name               VARCHAR(100) NOT NULL,
    display_name            VARCHAR(201) NOT NULL,
    password_hash           VARCHAR(256) NOT NULL,
    status                  VARCHAR(20) NOT NULL DEFAULT 'Active',
    phone_number            VARCHAR(20),
    department              VARCHAR(100),
    job_title               VARCHAR(100),
    last_login_at           TIMESTAMPTZ,
    failed_login_attempts   INT NOT NULL DEFAULT 0,
    lockout_end_utc         TIMESTAMPTZ,
    refresh_token           VARCHAR(256),
    refresh_token_expiry_utc TIMESTAMPTZ,
    created_by              VARCHAR(100) NOT NULL,
    created_at              TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    modified_by             VARCHAR(100),
    modified_at             TIMESTAMPTZ
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_users_email
    ON identity.users (email);

CREATE INDEX IF NOT EXISTS ix_users_tenant_id
    ON identity.users (tenant_id);

CREATE INDEX IF NOT EXISTS ix_users_refresh_token
    ON identity.users (refresh_token) WHERE refresh_token IS NOT NULL;

-- Roles
CREATE TABLE IF NOT EXISTS identity.roles (
    id              UUID PRIMARY KEY,
    tenant_id       UUID NOT NULL,
    name            VARCHAR(100) NOT NULL,
    description     VARCHAR(500),
    is_system_role  BOOLEAN NOT NULL DEFAULT FALSE,
    created_by      VARCHAR(100) NOT NULL,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    modified_by     VARCHAR(100),
    modified_at     TIMESTAMPTZ
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_roles_tenant_name
    ON identity.roles (tenant_id, name);

-- Permissions
CREATE TABLE IF NOT EXISTS identity.permissions (
    id              UUID PRIMARY KEY,
    tenant_id       UUID NOT NULL,
    module          VARCHAR(100) NOT NULL,
    action          VARCHAR(100) NOT NULL,
    description     VARCHAR(500)
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_permissions_tenant_module_action
    ON identity.permissions (tenant_id, module, action);

-- User-Role junction
CREATE TABLE IF NOT EXISTS identity.user_roles (
    user_id         UUID NOT NULL REFERENCES identity.users(id) ON DELETE CASCADE,
    role_id         UUID NOT NULL REFERENCES identity.roles(id) ON DELETE RESTRICT,
    assigned_at     TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    assigned_by     VARCHAR(100) NOT NULL,
    PRIMARY KEY (user_id, role_id)
);

-- Role-Permission junction
CREATE TABLE IF NOT EXISTS identity.role_permissions (
    role_id         UUID NOT NULL REFERENCES identity.roles(id) ON DELETE CASCADE,
    permission_id   UUID NOT NULL REFERENCES identity.permissions(id) ON DELETE RESTRICT,
    PRIMARY KEY (role_id, permission_id)
);

-- RLS policies on identity tables
ALTER TABLE identity.users ENABLE ROW LEVEL SECURITY;
ALTER TABLE identity.roles ENABLE ROW LEVEL SECURITY;
ALTER TABLE identity.permissions ENABLE ROW LEVEL SECURITY;

CREATE POLICY tenant_isolation_users ON identity.users
    USING (tenant_id = shared.current_tenant_id());

CREATE POLICY tenant_isolation_roles ON identity.roles
    USING (tenant_id = shared.current_tenant_id());

CREATE POLICY tenant_isolation_permissions ON identity.permissions
    USING (tenant_id = shared.current_tenant_id());

-- =============================================================================
-- Product Catalog Module Tables
-- =============================================================================

-- Parts (master data)
CREATE TABLE IF NOT EXISTS catalog.parts (
    id                  UUID PRIMARY KEY,
    tenant_id           UUID NOT NULL,
    part_number         VARCHAR(50) NOT NULL,
    name                VARCHAR(200) NOT NULL,
    description         VARCHAR(2000),
    product_family      VARCHAR(100),
    category            VARCHAR(100),
    serialization_mode  VARCHAR(20) NOT NULL DEFAULT 'None',
    status              VARCHAR(20) NOT NULL DEFAULT 'Active',
    unit_of_measure     VARCHAR(20),
    created_by          VARCHAR(100) NOT NULL,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    modified_by         VARCHAR(100),
    modified_at         TIMESTAMPTZ
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_parts_tenant_part_number
    ON catalog.parts (tenant_id, part_number);

CREATE INDEX IF NOT EXISTS ix_parts_tenant_id
    ON catalog.parts (tenant_id);

CREATE INDEX IF NOT EXISTS ix_parts_product_family
    ON catalog.parts (product_family);

CREATE INDEX IF NOT EXISTS ix_parts_status
    ON catalog.parts (status);

-- Part Revisions
CREATE TABLE IF NOT EXISTS catalog.part_revisions (
    id                  UUID PRIMARY KEY,
    tenant_id           UUID NOT NULL,
    part_id             UUID NOT NULL REFERENCES catalog.parts(id) ON DELETE CASCADE,
    revision_code       VARCHAR(20) NOT NULL,
    status              VARCHAR(20) NOT NULL DEFAULT 'Draft',
    description         VARCHAR(2000),
    change_reason       VARCHAR(1000),
    released_at         TIMESTAMPTZ,
    released_by         VARCHAR(100),
    obsoleted_at        TIMESTAMPTZ,
    created_by          VARCHAR(100) NOT NULL,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    modified_by         VARCHAR(100),
    modified_at         TIMESTAMPTZ
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_part_revisions_part_code
    ON catalog.part_revisions (part_id, revision_code);

CREATE INDEX IF NOT EXISTS ix_part_revisions_tenant_id
    ON catalog.part_revisions (tenant_id);

CREATE INDEX IF NOT EXISTS ix_part_revisions_status
    ON catalog.part_revisions (status);

-- Specification Parameters
CREATE TABLE IF NOT EXISTS catalog.specification_parameters (
    id                  UUID PRIMARY KEY,
    tenant_id           UUID NOT NULL,
    part_revision_id    UUID NOT NULL REFERENCES catalog.part_revisions(id) ON DELETE CASCADE,
    name                VARCHAR(200) NOT NULL,
    type                VARCHAR(20) NOT NULL,
    unit                VARCHAR(50),
    nominal_value       NUMERIC(18,6),
    upper_tolerance     NUMERIC(18,6),
    lower_tolerance     NUMERIC(18,6),
    text_value          VARCHAR(500),
    is_critical         BOOLEAN NOT NULL DEFAULT FALSE,
    sort_order          INT NOT NULL DEFAULT 0
);

CREATE INDEX IF NOT EXISTS ix_spec_params_tenant_id
    ON catalog.specification_parameters (tenant_id);

CREATE INDEX IF NOT EXISTS ix_spec_params_revision_id
    ON catalog.specification_parameters (part_revision_id);

-- BOM References
CREATE TABLE IF NOT EXISTS catalog.bom_references (
    id                  UUID PRIMARY KEY,
    tenant_id           UUID NOT NULL,
    parent_part_id      UUID NOT NULL REFERENCES catalog.parts(id) ON DELETE CASCADE,
    child_part_id       UUID NOT NULL REFERENCES catalog.parts(id) ON DELETE RESTRICT,
    quantity            NUMERIC(18,4) NOT NULL,
    reference_designator VARCHAR(100),
    sort_order          INT NOT NULL DEFAULT 0
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_bom_refs_parent_child
    ON catalog.bom_references (parent_part_id, child_part_id);

CREATE INDEX IF NOT EXISTS ix_bom_refs_tenant_id
    ON catalog.bom_references (tenant_id);

CREATE INDEX IF NOT EXISTS ix_bom_refs_child_part_id
    ON catalog.bom_references (child_part_id);

-- RLS policies on catalog tables
ALTER TABLE catalog.parts ENABLE ROW LEVEL SECURITY;
ALTER TABLE catalog.part_revisions ENABLE ROW LEVEL SECURITY;
ALTER TABLE catalog.specification_parameters ENABLE ROW LEVEL SECURITY;
ALTER TABLE catalog.bom_references ENABLE ROW LEVEL SECURITY;

CREATE POLICY tenant_isolation_parts ON catalog.parts
    USING (tenant_id = shared.current_tenant_id());

CREATE POLICY tenant_isolation_part_revisions ON catalog.part_revisions
    USING (tenant_id = shared.current_tenant_id());

CREATE POLICY tenant_isolation_spec_params ON catalog.specification_parameters
    USING (tenant_id = shared.current_tenant_id());

CREATE POLICY tenant_isolation_bom_refs ON catalog.bom_references
    USING (tenant_id = shared.current_tenant_id());

-- =============================================================================
-- Seed Data: Default tenant, roles, and system admin
-- =============================================================================

-- Default tenant
INSERT INTO identity.tenants (id, tenant_id, name, code, is_active, contact_email, timezone, created_by, created_at)
VALUES (
    '00000000-0000-0000-0000-000000000001',
    '00000000-0000-0000-0000-000000000001',
    'Berex Tech Manufacturing',
    'BEREX',
    TRUE,
    'admin@berextech.com',
    'UTC',
    'system',
    NOW()
) ON CONFLICT (code) DO NOTHING;

-- Default system roles
INSERT INTO identity.roles (id, tenant_id, name, description, is_system_role, created_by, created_at) VALUES
    ('10000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001', 'System Administrator', 'Platform-wide administration: tenant management, system configuration, user management', TRUE, 'system', NOW()),
    ('10000000-0000-0000-0000-000000000002', '00000000-0000-0000-0000-000000000001', 'Quality Manager', 'Tenant-wide quality operations, approval authority, AI capability management, report access', TRUE, 'system', NOW()),
    ('10000000-0000-0000-0000-000000000003', '00000000-0000-0000-0000-000000000001', 'Quality Supervisor', 'Department/area: inspection approval, NC disposition, team workload management', TRUE, 'system', NOW()),
    ('10000000-0000-0000-0000-000000000004', '00000000-0000-0000-0000-000000000001', 'Quality Engineer', 'Tenant-wide quality data: RCA/CAPA ownership, SPC management, AI interaction', TRUE, 'system', NOW()),
    ('10000000-0000-0000-0000-000000000005', '00000000-0000-0000-0000-000000000001', 'Quality Inspector', 'Assigned inspection types: inspection execution, defect reporting, measurement recording', TRUE, 'system', NOW()),
    ('10000000-0000-0000-0000-000000000006', '00000000-0000-0000-0000-000000000001', 'SQE', 'Supplier quality scope: supplier management, SCAR management, scorecard review', TRUE, 'system', NOW()),
    ('10000000-0000-0000-0000-000000000007', '00000000-0000-0000-0000-000000000001', 'Internal Auditor', 'Audit scope: audit execution, finding recording, report generation', TRUE, 'system', NOW()),
    ('10000000-0000-0000-0000-000000000008', '00000000-0000-0000-0000-000000000001', 'Calibration Technician', 'Calibration scope: equipment management, calibration recording, certificate upload', TRUE, 'system', NOW()),
    ('10000000-0000-0000-0000-000000000009', '00000000-0000-0000-0000-000000000001', 'Training Manager', 'Training scope: course management, assignment, qualification management', TRUE, 'system', NOW()),
    ('10000000-0000-0000-0000-000000000010', '00000000-0000-0000-0000-000000000001', 'Operator', 'Limited: defect reporting only, no approval authority', TRUE, 'system', NOW()),
    ('10000000-0000-0000-0000-000000000011', '00000000-0000-0000-0000-000000000001', 'Supplier Portal User', 'Own supplier data only: view own scorecards, respond to own SCARs, upload certificates', TRUE, 'system', NOW()),
    ('10000000-0000-0000-0000-000000000012', '00000000-0000-0000-0000-000000000001', 'Read-Only Viewer', 'Configurable scope: dashboard and report viewing only, no data modification', TRUE, 'system', NOW())
ON CONFLICT DO NOTHING;

-- Default system admin user (password: Admin@123456)
-- BCrypt hash for "Admin@123456" with work factor 12
INSERT INTO identity.users (id, tenant_id, email, first_name, last_name, display_name, password_hash, status, department, job_title, created_by, created_at)
VALUES (
    '20000000-0000-0000-0000-000000000001',
    '00000000-0000-0000-0000-000000000001',
    'admin@berextech.com',
    'System',
    'Administrator',
    'System Administrator',
    '$2a$12$LJ3m4ys3Gzl7v2VBKwmdxOYBNGTmN9pkLFNcHXNO5z7r5W5qR5d2W',
    'Active',
    'IT',
    'System Administrator',
    'system',
    NOW()
) ON CONFLICT DO NOTHING;

-- Assign System Administrator role to default admin user
INSERT INTO identity.user_roles (user_id, role_id, assigned_at, assigned_by)
VALUES (
    '20000000-0000-0000-0000-000000000001',
    '10000000-0000-0000-0000-000000000001',
    NOW(),
    'system'
) ON CONFLICT DO NOTHING;
