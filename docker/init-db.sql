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
-- Quality Inspection Module Tables
-- =============================================================================

-- Inspection Records
CREATE TABLE IF NOT EXISTS inspection.inspection_records (
    id                          UUID PRIMARY KEY,
    tenant_id                   UUID NOT NULL,
    inspection_number           VARCHAR(50) NOT NULL,
    type                        VARCHAR(20) NOT NULL,
    status                      VARCHAR(20) NOT NULL DEFAULT 'Draft',
    part_id                     UUID NOT NULL REFERENCES catalog.parts(id) ON DELETE RESTRICT,
    part_revision_id            UUID,
    lot_number                  VARCHAR(100),
    lot_size                    INT,
    sample_size                 INT,
    supplier_id                 UUID,
    sampling_plan_id            UUID,
    inspector_id                VARCHAR(100) NOT NULL,
    result                      VARCHAR(20),
    notes                       VARCHAR(2000),
    completed_at                TIMESTAMPTZ,
    completed_by                VARCHAR(100),
    approved_at                 TIMESTAMPTZ,
    approved_by                 VARCHAR(100),
    rejected_at                 TIMESTAMPTZ,
    rejected_by                 VARCHAR(100),
    checklist_id                UUID,
    disposition_type            VARCHAR(30),
    disposition_justification   VARCHAR(2000),
    disposition_approved_by     VARCHAR(100),
    disposition_approved_at     TIMESTAMPTZ,
    created_by                  VARCHAR(100) NOT NULL,
    created_at                  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    modified_by                 VARCHAR(100),
    modified_at                 TIMESTAMPTZ
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_inspection_records_tenant_number
    ON inspection.inspection_records (tenant_id, inspection_number);

CREATE INDEX IF NOT EXISTS ix_inspection_records_tenant_id
    ON inspection.inspection_records (tenant_id);

CREATE INDEX IF NOT EXISTS ix_inspection_records_part_id
    ON inspection.inspection_records (part_id);

CREATE INDEX IF NOT EXISTS ix_inspection_records_status
    ON inspection.inspection_records (status);

CREATE INDEX IF NOT EXISTS ix_inspection_records_type
    ON inspection.inspection_records (type);

-- Inspection Gate Results (owned by InspectionRecord)
CREATE TABLE IF NOT EXISTS inspection.inspection_gate_results (
    id                      INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    tenant_id               UUID NOT NULL DEFAULT shared.current_tenant_id(),
    inspection_record_id    UUID NOT NULL REFERENCES inspection.inspection_records(id) ON DELETE CASCADE,
    gate_type               VARCHAR(30) NOT NULL,
    passed                  BOOLEAN NOT NULL,
    detail                  VARCHAR(500),
    checked_at              TIMESTAMPTZ NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_gate_results_inspection_id
    ON inspection.inspection_gate_results (inspection_record_id);

-- Inspection Checklists
CREATE TABLE IF NOT EXISTS inspection.inspection_checklists (
    id                  UUID PRIMARY KEY,
    tenant_id           UUID NOT NULL,
    inspection_id       UUID NOT NULL REFERENCES inspection.inspection_records(id) ON DELETE CASCADE,
    part_revision_id    UUID NOT NULL,
    revision_code       VARCHAR(20) NOT NULL,
    snapshot_at         TIMESTAMPTZ NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_inspection_checklists_inspection_id
    ON inspection.inspection_checklists (inspection_id);

-- Checklist Items
CREATE TABLE IF NOT EXISTS inspection.checklist_items (
    id                      UUID PRIMARY KEY,
    tenant_id               UUID NOT NULL,
    checklist_id            UUID NOT NULL REFERENCES inspection.inspection_checklists(id) ON DELETE CASCADE,
    characteristic_name     VARCHAR(200) NOT NULL,
    specification_limit     VARCHAR(200),
    nominal_value           NUMERIC(18,6),
    upper_limit             NUMERIC(18,6),
    lower_limit             NUMERIC(18,6),
    unit                    VARCHAR(20),
    is_critical             BOOLEAN NOT NULL DEFAULT FALSE,
    sort_order              INT NOT NULL DEFAULT 0
);

CREATE INDEX IF NOT EXISTS ix_checklist_items_checklist_id
    ON inspection.checklist_items (checklist_id);

-- Measurements
CREATE TABLE IF NOT EXISTS inspection.measurements (
    id                      UUID PRIMARY KEY,
    tenant_id               UUID NOT NULL,
    inspection_id           UUID NOT NULL REFERENCES inspection.inspection_records(id) ON DELETE CASCADE,
    checklist_item_id       UUID,
    characteristic_name     VARCHAR(200) NOT NULL,
    measured_value          NUMERIC(18,6),
    text_value              VARCHAR(500),
    unit                    VARCHAR(20),
    result                  VARCHAR(20) NOT NULL,
    equipment_id            UUID,
    operator_id             VARCHAR(100),
    recorded_at             TIMESTAMPTZ NOT NULL,
    sequence_number         INT NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_measurements_inspection_id
    ON inspection.measurements (inspection_id);

-- Sampling Plans
CREATE TABLE IF NOT EXISTS inspection.sampling_plans (
    id                  UUID PRIMARY KEY,
    tenant_id           UUID NOT NULL,
    part_id             UUID NOT NULL REFERENCES catalog.parts(id) ON DELETE RESTRICT,
    supplier_id         UUID,
    inspection_type     VARCHAR(20) NOT NULL,
    level               VARCHAR(20) NOT NULL,
    aql_value           NUMERIC(8,4) NOT NULL,
    sample_size         INT NOT NULL,
    accept_number       INT NOT NULL,
    reject_number       INT NOT NULL,
    is_active           BOOLEAN NOT NULL DEFAULT TRUE,
    created_by          VARCHAR(100) NOT NULL,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    modified_by         VARCHAR(100),
    modified_at         TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS ix_sampling_plans_tenant_id
    ON inspection.sampling_plans (tenant_id);

CREATE INDEX IF NOT EXISTS ix_sampling_plans_part_id
    ON inspection.sampling_plans (part_id);

CREATE INDEX IF NOT EXISTS ix_sampling_plans_part_type_active
    ON inspection.sampling_plans (part_id, inspection_type, is_active);

-- RLS policies on inspection tables
ALTER TABLE inspection.inspection_records ENABLE ROW LEVEL SECURITY;
ALTER TABLE inspection.inspection_gate_results ENABLE ROW LEVEL SECURITY;
ALTER TABLE inspection.inspection_checklists ENABLE ROW LEVEL SECURITY;
ALTER TABLE inspection.checklist_items ENABLE ROW LEVEL SECURITY;
ALTER TABLE inspection.measurements ENABLE ROW LEVEL SECURITY;
ALTER TABLE inspection.sampling_plans ENABLE ROW LEVEL SECURITY;

CREATE POLICY tenant_isolation_inspection_records ON inspection.inspection_records
    USING (tenant_id = shared.current_tenant_id());

CREATE POLICY tenant_isolation_inspection_gate_results ON inspection.inspection_gate_results
    USING (tenant_id = shared.current_tenant_id());

CREATE POLICY tenant_isolation_inspection_checklists ON inspection.inspection_checklists
    USING (tenant_id = shared.current_tenant_id());

CREATE POLICY tenant_isolation_checklist_items ON inspection.checklist_items
    USING (tenant_id = shared.current_tenant_id());

CREATE POLICY tenant_isolation_measurements ON inspection.measurements
    USING (tenant_id = shared.current_tenant_id());

CREATE POLICY tenant_isolation_sampling_plans ON inspection.sampling_plans
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

-- =============================================================================
-- Non-Conformance (NCR) Module Tables
-- =============================================================================

CREATE SCHEMA IF NOT EXISTS ncr;

-- Non-Conformance Records
CREATE TABLE IF NOT EXISTS ncr.non_conformance_records (
    id                              UUID PRIMARY KEY,
    tenant_id                       UUID NOT NULL,
    ncr_number                      VARCHAR(50) NOT NULL,
    status                          VARCHAR(30) NOT NULL DEFAULT 'Open',
    severity                        VARCHAR(20) NOT NULL,
    source                          VARCHAR(30) NOT NULL,
    detection_point                 VARCHAR(30) NOT NULL,
    description                     VARCHAR(4000) NOT NULL,
    part_id                         UUID NOT NULL REFERENCES catalog.parts(id) ON DELETE RESTRICT,
    part_revision_id                UUID,
    lot_number                      VARCHAR(100),
    serial_number                   VARCHAR(100),
    supplier_id                     UUID,
    supplier_lot_number             VARCHAR(100),
    work_order_number               VARCHAR(100),
    customer_id                     UUID,
    source_inspection_id            UUID,
    quantity_affected               INT NOT NULL DEFAULT 0,
    quantity_defective              INT NOT NULL DEFAULT 0,
    classification_category         VARCHAR(200),
    classification_defect_type      VARCHAR(200),
    classification_defect_code      VARCHAR(50),
    disposition_type                VARCHAR(30),
    disposition_justification       VARCHAR(4000),
    disposition_approved_by         VARCHAR(100),
    disposition_approved_at         TIMESTAMPTZ,
    impact_affected_quantity        INT,
    impact_shipped_product_affected BOOLEAN,
    impact_customer_description     VARCHAR(2000),
    assigned_to                     VARCHAR(100),
    capa_id                         UUID,
    closed_at                       TIMESTAMPTZ,
    closed_by                       VARCHAR(100),
    reopened_at                     TIMESTAMPTZ,
    reopened_by                     VARCHAR(100),
    reopen_reason                   VARCHAR(4000),
    closure_notes                   VARCHAR(4000),
    created_by                      VARCHAR(100) NOT NULL,
    created_at                      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    modified_by                     VARCHAR(100),
    modified_at                     TIMESTAMPTZ
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_ncr_records_tenant_number
    ON ncr.non_conformance_records (tenant_id, ncr_number);
CREATE INDEX IF NOT EXISTS ix_ncr_records_tenant_id
    ON ncr.non_conformance_records (tenant_id);
CREATE INDEX IF NOT EXISTS ix_ncr_records_part_id
    ON ncr.non_conformance_records (part_id);
CREATE INDEX IF NOT EXISTS ix_ncr_records_status
    ON ncr.non_conformance_records (status);
CREATE INDEX IF NOT EXISTS ix_ncr_records_severity
    ON ncr.non_conformance_records (severity);
CREATE INDEX IF NOT EXISTS ix_ncr_records_supplier_id
    ON ncr.non_conformance_records (supplier_id);
CREATE INDEX IF NOT EXISTS ix_ncr_records_created_at
    ON ncr.non_conformance_records (created_at);

-- Containment Actions
CREATE TABLE IF NOT EXISTS ncr.containment_actions (
    id                      UUID PRIMARY KEY,
    tenant_id               UUID NOT NULL,
    non_conformance_id      UUID NOT NULL REFERENCES ncr.non_conformance_records(id) ON DELETE CASCADE,
    description             VARCHAR(4000) NOT NULL,
    action_taken_by         VARCHAR(100) NOT NULL,
    action_taken_at         TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    is_verified             BOOLEAN NOT NULL DEFAULT FALSE,
    verified_by             VARCHAR(100),
    verified_at             TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS ix_containment_actions_nc_id
    ON ncr.containment_actions (non_conformance_id);
CREATE INDEX IF NOT EXISTS ix_containment_actions_tenant_id
    ON ncr.containment_actions (tenant_id);

-- Investigations
CREATE TABLE IF NOT EXISTS ncr.investigations (
    id                      UUID PRIMARY KEY,
    tenant_id               UUID NOT NULL,
    non_conformance_id      UUID NOT NULL REFERENCES ncr.non_conformance_records(id) ON DELETE CASCADE,
    investigator_id         VARCHAR(100) NOT NULL,
    methodology             VARCHAR(200),
    root_cause              VARCHAR(4000),
    findings                VARCHAR(4000),
    started_at              TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    completed_at            TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS ix_investigations_nc_id
    ON ncr.investigations (non_conformance_id);
CREATE INDEX IF NOT EXISTS ix_investigations_tenant_id
    ON ncr.investigations (tenant_id);

-- RLS Policies for NCR
ALTER TABLE ncr.non_conformance_records ENABLE ROW LEVEL SECURITY;
ALTER TABLE ncr.containment_actions ENABLE ROW LEVEL SECURITY;
ALTER TABLE ncr.investigations ENABLE ROW LEVEL SECURITY;

CREATE POLICY tenant_isolation_ncr ON ncr.non_conformance_records
    USING (tenant_id = shared.current_tenant_id());

CREATE POLICY tenant_isolation_containment ON ncr.containment_actions
    USING (tenant_id = shared.current_tenant_id());

CREATE POLICY tenant_isolation_investigations ON ncr.investigations
    USING (tenant_id = shared.current_tenant_id());

-- =============================================================================
-- Document Control Module Tables
-- =============================================================================

-- Document Masters
CREATE TABLE IF NOT EXISTS document.document_masters (
    id                  UUID PRIMARY KEY,
    tenant_id           UUID NOT NULL,
    document_number     VARCHAR(50) NOT NULL,
    title               VARCHAR(200) NOT NULL,
    description         VARCHAR(2000),
    document_type       VARCHAR(50) NOT NULL,
    owner_id            VARCHAR(200) NOT NULL,
    department          VARCHAR(100),
    is_active           BOOLEAN NOT NULL DEFAULT TRUE,
    created_by          VARCHAR(200) NOT NULL,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    modified_by         VARCHAR(200),
    modified_at         TIMESTAMPTZ
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_document_masters_tenant_number
    ON document.document_masters (tenant_id, document_number);
CREATE INDEX IF NOT EXISTS ix_document_masters_tenant_id
    ON document.document_masters (tenant_id);
CREATE INDEX IF NOT EXISTS ix_document_masters_type
    ON document.document_masters (document_type);
CREATE INDEX IF NOT EXISTS ix_document_masters_created_at
    ON document.document_masters (created_at);

-- Document Versions
CREATE TABLE IF NOT EXISTS document.document_versions (
    id                          UUID PRIMARY KEY,
    tenant_id                   UUID NOT NULL,
    document_master_id          UUID NOT NULL REFERENCES document.document_masters(id) ON DELETE CASCADE,
    version_number              VARCHAR(20) NOT NULL,
    status                      VARCHAR(30) NOT NULL DEFAULT 'Draft',
    content                     TEXT NOT NULL,
    change_description          VARCHAR(2000),
    author_id                   VARCHAR(200) NOT NULL,
    effective_date              TIMESTAMPTZ,
    expiry_date                 TIMESTAMPTZ,
    attachment_file_name        VARCHAR(255),
    attachment_content_type     VARCHAR(100),
    attachment_size_bytes       BIGINT,
    attachment_storage_path     VARCHAR(500),
    attachment_content_hash     VARCHAR(128),
    created_at                  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    released_at                 TIMESTAMPTZ,
    released_by                 VARCHAR(200)
);

CREATE INDEX IF NOT EXISTS ix_document_versions_master_id
    ON document.document_versions (document_master_id);
CREATE INDEX IF NOT EXISTS ix_document_versions_tenant_id
    ON document.document_versions (tenant_id);
CREATE INDEX IF NOT EXISTS ix_document_versions_status
    ON document.document_versions (status);

-- Approval Workflows
CREATE TABLE IF NOT EXISTS document.approval_workflows (
    id                      UUID PRIMARY KEY,
    tenant_id               UUID NOT NULL,
    document_version_id     UUID NOT NULL,
    current_step_order      INTEGER NOT NULL DEFAULT 1,
    is_complete             BOOLEAN NOT NULL DEFAULT FALSE,
    is_rejected             BOOLEAN NOT NULL DEFAULT FALSE,
    created_at              TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    completed_at            TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS ix_approval_workflows_version_id
    ON document.approval_workflows (document_version_id);
CREATE INDEX IF NOT EXISTS ix_approval_workflows_tenant_id
    ON document.approval_workflows (tenant_id);

-- Approval Steps
CREATE TABLE IF NOT EXISTS document.approval_steps (
    "Id"                    INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    approval_workflow_id    UUID NOT NULL REFERENCES document.approval_workflows(id) ON DELETE CASCADE,
    step_order              INTEGER NOT NULL,
    approver_id             VARCHAR(200) NOT NULL,
    decision                VARCHAR(30),
    comments                VARCHAR(2000),
    signature               VARCHAR(500),
    decided_at              TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS ix_approval_steps_workflow_id
    ON document.approval_steps (approval_workflow_id);

-- Distributions
CREATE TABLE IF NOT EXISTS document.distributions (
    id                      UUID PRIMARY KEY,
    tenant_id               UUID NOT NULL,
    document_version_id     UUID NOT NULL,
    recipient_id            VARCHAR(200) NOT NULL,
    distributed_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    acknowledged_at         TIMESTAMPTZ,
    compliance_deadline     TIMESTAMPTZ NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_distributions_version_id
    ON document.distributions (document_version_id);
CREATE INDEX IF NOT EXISTS ix_distributions_tenant_id
    ON document.distributions (tenant_id);
CREATE INDEX IF NOT EXISTS ix_distributions_recipient_id
    ON document.distributions (recipient_id);

-- RLS Policies for Document Control
ALTER TABLE document.document_masters ENABLE ROW LEVEL SECURITY;
ALTER TABLE document.document_versions ENABLE ROW LEVEL SECURITY;
ALTER TABLE document.approval_workflows ENABLE ROW LEVEL SECURITY;
ALTER TABLE document.distributions ENABLE ROW LEVEL SECURITY;

CREATE POLICY tenant_isolation_document_masters ON document.document_masters
    USING (tenant_id = shared.current_tenant_id());

CREATE POLICY tenant_isolation_document_versions ON document.document_versions
    USING (tenant_id = shared.current_tenant_id());

CREATE POLICY tenant_isolation_approval_workflows ON document.approval_workflows
    USING (tenant_id = shared.current_tenant_id());

CREATE POLICY tenant_isolation_distributions ON document.distributions
    USING (tenant_id = shared.current_tenant_id());

-- =============================================================================
-- CAPA Module Tables
-- =============================================================================

-- Root Cause Analyses
CREATE TABLE IF NOT EXISTS capa.root_cause_analyses (
    id                      UUID PRIMARY KEY,
    tenant_id               UUID NOT NULL,
    capa_id                 UUID NOT NULL,
    methodology             VARCHAR(30) NOT NULL,
    analysis_details        VARCHAR(4000),
    root_cause              VARCHAR(4000),
    contributing_factors    VARCHAR(4000),
    started_at              TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    completed_at            TIMESTAMPTZ,
    analyst_id              VARCHAR(100) NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_rca_capa_id
    ON capa.root_cause_analyses (capa_id);
CREATE INDEX IF NOT EXISTS ix_rca_tenant_id
    ON capa.root_cause_analyses (tenant_id);

-- CAPA Records
CREATE TABLE IF NOT EXISTS capa.capa_records (
    id                          UUID PRIMARY KEY,
    tenant_id                   UUID NOT NULL,
    capa_number                 VARCHAR(50) NOT NULL,
    title                       VARCHAR(200) NOT NULL,
    description                 VARCHAR(4000) NOT NULL,
    status                      VARCHAR(30) NOT NULL DEFAULT 'Initiated',
    priority                    VARCHAR(20) NOT NULL,
    source_type                 VARCHAR(30) NOT NULL,
    source_non_conformance_id   UUID,
    source_audit_finding_id     UUID,
    source_description          VARCHAR(4000),
    owner_id                    VARCHAR(100) NOT NULL,
    assigned_to                 VARCHAR(100),
    source_nc_id                UUID,
    root_cause_analysis_id      UUID REFERENCES capa.root_cause_analyses(id) ON DELETE SET NULL,
    target_closure_date         TIMESTAMPTZ,
    closed_at                   TIMESTAMPTZ,
    closed_by                   VARCHAR(100),
    closure_notes               VARCHAR(4000),
    created_by                  VARCHAR(100) NOT NULL,
    created_at                  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    modified_by                 VARCHAR(100),
    modified_at                 TIMESTAMPTZ
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_capa_records_tenant_number
    ON capa.capa_records (tenant_id, capa_number);
CREATE INDEX IF NOT EXISTS ix_capa_records_tenant_id
    ON capa.capa_records (tenant_id);
CREATE INDEX IF NOT EXISTS ix_capa_records_status
    ON capa.capa_records (status);
CREATE INDEX IF NOT EXISTS ix_capa_records_priority
    ON capa.capa_records (priority);
CREATE INDEX IF NOT EXISTS ix_capa_records_created_at
    ON capa.capa_records (created_at);

-- CAPA Actions
CREATE TABLE IF NOT EXISTS capa.capa_actions (
    id                      UUID PRIMARY KEY,
    tenant_id               UUID NOT NULL,
    capa_id                 UUID NOT NULL REFERENCES capa.capa_records(id) ON DELETE CASCADE,
    action_type             VARCHAR(20) NOT NULL,
    description             VARCHAR(4000) NOT NULL,
    owner_id                VARCHAR(100) NOT NULL,
    due_date                TIMESTAMPTZ NOT NULL,
    evidence_requirement    VARCHAR(2000),
    completion_notes        VARCHAR(4000),
    evidence_provided       VARCHAR(4000),
    completed_at            TIMESTAMPTZ,
    completed_by            VARCHAR(100),
    created_at              TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_capa_actions_capa_id
    ON capa.capa_actions (capa_id);
CREATE INDEX IF NOT EXISTS ix_capa_actions_tenant_id
    ON capa.capa_actions (tenant_id);
CREATE INDEX IF NOT EXISTS ix_capa_actions_due_date
    ON capa.capa_actions (due_date);

-- Effectiveness Verifications
CREATE TABLE IF NOT EXISTS capa.effectiveness_verifications (
    id                      UUID PRIMARY KEY,
    tenant_id               UUID NOT NULL,
    capa_id                 UUID NOT NULL REFERENCES capa.capa_records(id) ON DELETE CASCADE,
    scheduled_date          TIMESTAMPTZ NOT NULL,
    verification_criteria   VARCHAR(4000) NOT NULL,
    verifier_id             VARCHAR(100),
    result                  VARCHAR(4000),
    evidence                VARCHAR(4000),
    is_effective            BOOLEAN,
    verified_at             TIMESTAMPTZ,
    created_at              TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_effectiveness_verifications_capa_id
    ON capa.effectiveness_verifications (capa_id);
CREATE INDEX IF NOT EXISTS ix_effectiveness_verifications_tenant_id
    ON capa.effectiveness_verifications (tenant_id);

-- RLS Policies for CAPA
ALTER TABLE capa.capa_records ENABLE ROW LEVEL SECURITY;
ALTER TABLE capa.root_cause_analyses ENABLE ROW LEVEL SECURITY;
ALTER TABLE capa.capa_actions ENABLE ROW LEVEL SECURITY;
ALTER TABLE capa.effectiveness_verifications ENABLE ROW LEVEL SECURITY;

CREATE POLICY tenant_isolation_capa_records ON capa.capa_records
    USING (tenant_id = shared.current_tenant_id());

CREATE POLICY tenant_isolation_rca ON capa.root_cause_analyses
    USING (tenant_id = shared.current_tenant_id());

CREATE POLICY tenant_isolation_capa_actions ON capa.capa_actions
    USING (tenant_id = shared.current_tenant_id());

CREATE POLICY tenant_isolation_effectiveness_verifications ON capa.effectiveness_verifications
    USING (tenant_id = shared.current_tenant_id());

-- =====================================================
-- AUDIT MANAGEMENT SCHEMA
-- =====================================================
CREATE SCHEMA IF NOT EXISTS audit;

-- Audit Plans
CREATE TABLE IF NOT EXISTS audit.audit_plans (
    id                      UUID PRIMARY KEY,
    tenant_id               UUID NOT NULL,
    plan_name               VARCHAR(200) NOT NULL,
    year                    INTEGER NOT NULL,
    description             VARCHAR(2000),
    scope                   VARCHAR(2000),
    is_active               BOOLEAN NOT NULL DEFAULT TRUE,
    created_by              VARCHAR(200) NOT NULL,
    created_at              TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    modified_by             VARCHAR(200),
    modified_at             TIMESTAMPTZ
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_audit_plans_tenant_name_year
    ON audit.audit_plans (tenant_id, plan_name, year);
CREATE INDEX IF NOT EXISTS ix_audit_plans_tenant_id
    ON audit.audit_plans (tenant_id);
CREATE INDEX IF NOT EXISTS ix_audit_plans_year
    ON audit.audit_plans (year);
CREATE INDEX IF NOT EXISTS ix_audit_plans_created_at
    ON audit.audit_plans (created_at);

-- Audit Records
CREATE TABLE IF NOT EXISTS audit.audit_records (
    id                          UUID PRIMARY KEY,
    tenant_id                   UUID NOT NULL,
    audit_plan_id               UUID NOT NULL REFERENCES audit.audit_plans(id) ON DELETE CASCADE,
    audit_number                VARCHAR(50) NOT NULL,
    audit_type                  VARCHAR(30) NOT NULL,
    status                      VARCHAR(30) NOT NULL,
    lead_auditor_id             VARCHAR(200) NOT NULL,
    auditee_area                VARCHAR(200),
    scheduled_date              TIMESTAMPTZ NOT NULL,
    started_at                  TIMESTAMPTZ,
    completed_at                TIMESTAMPTZ,
    report_summary              VARCHAR(4000),
    report_recommendations      VARCHAR(4000),
    report_auditor_notes        VARCHAR(4000),
    report_generated_at         TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS ix_audit_records_plan_id
    ON audit.audit_records (audit_plan_id);
CREATE INDEX IF NOT EXISTS ix_audit_records_tenant_id
    ON audit.audit_records (tenant_id);
CREATE INDEX IF NOT EXISTS ix_audit_records_status
    ON audit.audit_records (status);
CREATE INDEX IF NOT EXISTS ix_audit_records_scheduled_date
    ON audit.audit_records (scheduled_date);

-- Audit Findings
CREATE TABLE IF NOT EXISTS audit.audit_findings (
    id                      UUID PRIMARY KEY,
    tenant_id               UUID NOT NULL,
    audit_record_id         UUID NOT NULL REFERENCES audit.audit_records(id) ON DELETE CASCADE,
    classification          VARCHAR(50) NOT NULL,
    clause_reference        VARCHAR(100) NOT NULL,
    description             VARCHAR(4000) NOT NULL,
    evidence                VARCHAR(4000),
    corrective_action       VARCHAR(4000),
    linked_capa_id          VARCHAR(200),
    found_at                TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_audit_findings_record_id
    ON audit.audit_findings (audit_record_id);
CREATE INDEX IF NOT EXISTS ix_audit_findings_tenant_id
    ON audit.audit_findings (tenant_id);
CREATE INDEX IF NOT EXISTS ix_audit_findings_classification
    ON audit.audit_findings (classification);

-- Audit Checklists
CREATE TABLE IF NOT EXISTS audit.audit_checklists (
    id                      UUID PRIMARY KEY,
    tenant_id               UUID NOT NULL,
    audit_record_id         UUID NOT NULL REFERENCES audit.audit_records(id) ON DELETE CASCADE,
    standard                VARCHAR(100) NOT NULL,
    clause_reference        VARCHAR(100) NOT NULL,
    requirement             VARCHAR(2000) NOT NULL,
    is_compliant            BOOLEAN NOT NULL,
    evidence                VARCHAR(4000),
    notes                   VARCHAR(2000)
);

CREATE INDEX IF NOT EXISTS ix_audit_checklists_record_id
    ON audit.audit_checklists (audit_record_id);
CREATE INDEX IF NOT EXISTS ix_audit_checklists_tenant_id
    ON audit.audit_checklists (tenant_id);

-- RLS Policies for Audit Management
ALTER TABLE audit.audit_plans ENABLE ROW LEVEL SECURITY;
ALTER TABLE audit.audit_records ENABLE ROW LEVEL SECURITY;
ALTER TABLE audit.audit_findings ENABLE ROW LEVEL SECURITY;
ALTER TABLE audit.audit_checklists ENABLE ROW LEVEL SECURITY;

CREATE POLICY tenant_isolation_audit_plans ON audit.audit_plans
    USING (tenant_id = shared.current_tenant_id());

CREATE POLICY tenant_isolation_audit_records ON audit.audit_records
    USING (tenant_id = shared.current_tenant_id());

CREATE POLICY tenant_isolation_audit_findings ON audit.audit_findings
    USING (tenant_id = shared.current_tenant_id());

CREATE POLICY tenant_isolation_audit_checklists ON audit.audit_checklists
    USING (tenant_id = shared.current_tenant_id());

-- =============================================================================
-- SUPPLIER QUALITY MODULE
-- =============================================================================

-- Suppliers (aggregate root)
CREATE TABLE IF NOT EXISTS supplier.suppliers (
    id                      UUID PRIMARY KEY,
    tenant_id               UUID NOT NULL,
    code                    VARCHAR(50) NOT NULL,
    name                    VARCHAR(200) NOT NULL,
    status                  VARCHAR(30) NOT NULL,
    risk_level              VARCHAR(20) NOT NULL,
    tier                    VARCHAR(50),
    approved_since          TIMESTAMPTZ,
    contact_name            VARCHAR(200),
    contact_role            VARCHAR(100),
    contact_email           VARCHAR(200),
    contact_phone           VARCHAR(50),
    risk_assessment_level   VARCHAR(20),
    risk_assessment_factors VARCHAR(2000),
    risk_assessed_at        TIMESTAMPTZ,
    created_by              VARCHAR(200) NOT NULL,
    created_at              TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    modified_by             VARCHAR(200),
    modified_at             TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS ix_suppliers_tenant_id
    ON supplier.suppliers (tenant_id);
CREATE UNIQUE INDEX IF NOT EXISTS ix_suppliers_tenant_code
    ON supplier.suppliers (tenant_id, code);
CREATE INDEX IF NOT EXISTS ix_suppliers_tenant_status
    ON supplier.suppliers (tenant_id, status);
CREATE INDEX IF NOT EXISTS ix_suppliers_created_at
    ON supplier.suppliers (created_at);

-- Supplier Approvals
CREATE TABLE IF NOT EXISTS supplier.supplier_approvals (
    id                      UUID PRIMARY KEY,
    tenant_id               UUID NOT NULL,
    supplier_id             UUID NOT NULL REFERENCES supplier.suppliers(id) ON DELETE CASCADE,
    scope_description       VARCHAR(2000) NOT NULL,
    approved_date           TIMESTAMPTZ NOT NULL,
    expiry_date             TIMESTAMPTZ,
    conditions              VARCHAR(2000),
    is_active               BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE INDEX IF NOT EXISTS ix_supplier_approvals_supplier_expiry
    ON supplier.supplier_approvals (supplier_id, expiry_date);
CREATE INDEX IF NOT EXISTS ix_supplier_approvals_tenant_id
    ON supplier.supplier_approvals (tenant_id);

-- Supplier Scorecards
CREATE TABLE IF NOT EXISTS supplier.scorecards (
    id                      UUID PRIMARY KEY,
    tenant_id               UUID NOT NULL,
    supplier_id             UUID NOT NULL REFERENCES supplier.suppliers(id) ON DELETE CASCADE,
    period_start            TIMESTAMPTZ NOT NULL,
    period_end              TIMESTAMPTZ NOT NULL,
    quality_score           NUMERIC(5, 2) NOT NULL,
    delivery_score          NUMERIC(5, 2) NOT NULL,
    responsiveness_score    NUMERIC(5, 2) NOT NULL,
    cost_score              NUMERIC(5, 2) NOT NULL,
    overall_score           NUMERIC(5, 2) NOT NULL,
    status                  VARCHAR(20) NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_scorecards_supplier_period
    ON supplier.scorecards (supplier_id, period_start);
CREATE INDEX IF NOT EXISTS ix_scorecards_tenant_id
    ON supplier.scorecards (tenant_id);

-- SCAR Records
CREATE TABLE IF NOT EXISTS supplier.scar_records (
    id                              UUID PRIMARY KEY,
    tenant_id                       UUID NOT NULL,
    supplier_id                     UUID NOT NULL REFERENCES supplier.suppliers(id) ON DELETE CASCADE,
    scar_number                     VARCHAR(50) NOT NULL,
    nc_id                           UUID,
    defect_description              VARCHAR(4000) NOT NULL,
    severity                        VARCHAR(50) NOT NULL,
    issued_date                     TIMESTAMPTZ NOT NULL,
    response_deadline               TIMESTAMPTZ NOT NULL,
    status                          VARCHAR(30) NOT NULL,
    response_root_cause             VARCHAR(4000),
    response_corrective_actions     VARCHAR(4000),
    response_evidence_refs          VARCHAR(4000),
    response_date                   TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS ix_scar_records_supplier_status
    ON supplier.scar_records (supplier_id, status);
CREATE INDEX IF NOT EXISTS ix_scar_records_nc_id
    ON supplier.scar_records (nc_id);
CREATE INDEX IF NOT EXISTS ix_scar_records_tenant_id
    ON supplier.scar_records (tenant_id);

-- Approved Parts
CREATE TABLE IF NOT EXISTS supplier.approved_parts (
    id                      UUID PRIMARY KEY,
    tenant_id               UUID NOT NULL,
    supplier_id             UUID NOT NULL REFERENCES supplier.suppliers(id) ON DELETE CASCADE,
    part_id                 UUID NOT NULL,
    revision_scope          VARCHAR(200),
    approval_date           TIMESTAMPTZ NOT NULL,
    is_active               BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_approved_parts_supplier_part
    ON supplier.approved_parts (supplier_id, part_id) WHERE is_active = true;
CREATE INDEX IF NOT EXISTS ix_approved_parts_tenant_id
    ON supplier.approved_parts (tenant_id);

-- RLS Policies for Supplier Quality
ALTER TABLE supplier.suppliers ENABLE ROW LEVEL SECURITY;
ALTER TABLE supplier.supplier_approvals ENABLE ROW LEVEL SECURITY;
ALTER TABLE supplier.scorecards ENABLE ROW LEVEL SECURITY;
ALTER TABLE supplier.scar_records ENABLE ROW LEVEL SECURITY;
ALTER TABLE supplier.approved_parts ENABLE ROW LEVEL SECURITY;

CREATE POLICY tenant_isolation_suppliers ON supplier.suppliers
    USING (tenant_id = shared.current_tenant_id());

CREATE POLICY tenant_isolation_supplier_approvals ON supplier.supplier_approvals
    USING (tenant_id = shared.current_tenant_id());

CREATE POLICY tenant_isolation_scorecards ON supplier.scorecards
    USING (tenant_id = shared.current_tenant_id());

CREATE POLICY tenant_isolation_scar_records ON supplier.scar_records
    USING (tenant_id = shared.current_tenant_id());

CREATE POLICY tenant_isolation_approved_parts ON supplier.approved_parts
    USING (tenant_id = shared.current_tenant_id());

-- ============================================================================
-- Calibration & Metrology Module
-- ============================================================================

CREATE TABLE IF NOT EXISTS calibration.equipment (
    id              UUID PRIMARY KEY,
    tenant_id       UUID NOT NULL,
    code            VARCHAR(50) NOT NULL,
    name            VARCHAR(200) NOT NULL,
    type            VARCHAR(100),
    manufacturer    VARCHAR(200),
    model           VARCHAR(200),
    serial_number   VARCHAR(100),
    status          VARCHAR(50) NOT NULL DEFAULT 'Active',
    location        VARCHAR(200),
    department      VARCHAR(200),
    area            VARCHAR(200),
    custodian_id    UUID,
    created_by      VARCHAR(100) NOT NULL,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    modified_by     VARCHAR(100),
    modified_at     TIMESTAMPTZ
);
CREATE UNIQUE INDEX IF NOT EXISTS ix_equipment_tenant_code
    ON calibration.equipment (tenant_id, code);
CREATE INDEX IF NOT EXISTS ix_equipment_tenant_status
    ON calibration.equipment (tenant_id, status);

CREATE TABLE IF NOT EXISTS calibration.schedules (
    id              UUID PRIMARY KEY,
    tenant_id       UUID NOT NULL,
    equipment_id    UUID NOT NULL REFERENCES calibration.equipment(id) ON DELETE CASCADE,
    interval_days   INTEGER NOT NULL,
    lead_time_days  INTEGER NOT NULL DEFAULT 0,
    lab_type        VARCHAR(100) NOT NULL,
    procedure_ref   VARCHAR(200),
    next_due_date   TIMESTAMPTZ NOT NULL
);
CREATE UNIQUE INDEX IF NOT EXISTS ix_schedules_equipment_id
    ON calibration.schedules (equipment_id);
CREATE INDEX IF NOT EXISTS ix_schedules_next_due
    ON calibration.schedules (next_due_date);
CREATE INDEX IF NOT EXISTS ix_schedules_tenant_id
    ON calibration.schedules (tenant_id);

CREATE TABLE IF NOT EXISTS calibration.calibration_records (
    id                          UUID PRIMARY KEY,
    tenant_id                   UUID NOT NULL,
    equipment_id                UUID NOT NULL REFERENCES calibration.equipment(id) ON DELETE CASCADE,
    calibration_date            TIMESTAMPTZ NOT NULL,
    result                      VARCHAR(50) NOT NULL,
    technician_id               UUID,
    procedure_ref               VARCHAR(200),
    notes                       VARCHAR(2000),
    environmental_conditions    VARCHAR(500),
    next_due_date               TIMESTAMPTZ,
    cert_issuing_lab            VARCHAR(200),
    cert_accreditation_ref      VARCHAR(200),
    cert_file_ref               VARCHAR(500),
    cert_valid_from             TIMESTAMPTZ,
    cert_valid_until            TIMESTAMPTZ
);
CREATE INDEX IF NOT EXISTS ix_cal_records_equip_date
    ON calibration.calibration_records (equipment_id, calibration_date);
CREATE INDEX IF NOT EXISTS ix_cal_records_next_due
    ON calibration.calibration_records (next_due_date);
CREATE INDEX IF NOT EXISTS ix_cal_records_tenant_id
    ON calibration.calibration_records (tenant_id);

CREATE TABLE IF NOT EXISTS calibration.gauge_rr_studies (
    id                  UUID PRIMARY KEY,
    tenant_id           UUID NOT NULL,
    equipment_id        UUID NOT NULL REFERENCES calibration.equipment(id) ON DELETE CASCADE,
    characteristic_id   UUID,
    study_date          TIMESTAMPTZ NOT NULL,
    total_grr_pct       NUMERIC(5,2) NOT NULL,
    repeatability_pct   NUMERIC(5,2) NOT NULL,
    reproducibility_pct NUMERIC(5,2) NOT NULL,
    part_variation_pct  NUMERIC(5,2),
    ndc                 INTEGER,
    result              VARCHAR(50) NOT NULL
);
CREATE INDEX IF NOT EXISTS ix_gauge_rr_equip_char
    ON calibration.gauge_rr_studies (equipment_id, characteristic_id);
CREATE INDEX IF NOT EXISTS ix_gauge_rr_tenant_id
    ON calibration.gauge_rr_studies (tenant_id);

CREATE TABLE IF NOT EXISTS calibration.impact_assessments (
    id                          UUID PRIMARY KEY,
    tenant_id                   UUID NOT NULL,
    equipment_id                UUID NOT NULL REFERENCES calibration.equipment(id) ON DELETE CASCADE,
    failed_cal_id               UUID NOT NULL REFERENCES calibration.calibration_records(id),
    affected_from               TIMESTAMPTZ NOT NULL,
    affected_to                 TIMESTAMPTZ NOT NULL,
    affected_inspection_count   INTEGER NOT NULL DEFAULT 0,
    status                      VARCHAR(50) NOT NULL DEFAULT 'Open',
    reviewed_by                 UUID,
    notes                       VARCHAR(2000)
);
CREATE INDEX IF NOT EXISTS ix_impact_equip_cal
    ON calibration.impact_assessments (equipment_id, failed_cal_id);
CREATE INDEX IF NOT EXISTS ix_impact_tenant_id
    ON calibration.impact_assessments (tenant_id);

-- Calibration RLS Policies
ALTER TABLE calibration.equipment ENABLE ROW LEVEL SECURITY;
ALTER TABLE calibration.schedules ENABLE ROW LEVEL SECURITY;
ALTER TABLE calibration.calibration_records ENABLE ROW LEVEL SECURITY;
ALTER TABLE calibration.gauge_rr_studies ENABLE ROW LEVEL SECURITY;
ALTER TABLE calibration.impact_assessments ENABLE ROW LEVEL SECURITY;

CREATE POLICY tenant_isolation_equipment ON calibration.equipment
    USING (tenant_id = shared.current_tenant_id());

CREATE POLICY tenant_isolation_cal_schedules ON calibration.schedules
    USING (tenant_id = shared.current_tenant_id());

CREATE POLICY tenant_isolation_cal_records ON calibration.calibration_records
    USING (tenant_id = shared.current_tenant_id());

CREATE POLICY tenant_isolation_gauge_rr ON calibration.gauge_rr_studies
    USING (tenant_id = shared.current_tenant_id());

CREATE POLICY tenant_isolation_impact ON calibration.impact_assessments
    USING (tenant_id = shared.current_tenant_id());

-- =================================================================
-- Training & Competency Module (Sprint 10)
-- =================================================================

-- Qualifications
CREATE TABLE IF NOT EXISTS training.qualifications (
    id                          UUID PRIMARY KEY,
    tenant_id                   UUID NOT NULL,
    code                        VARCHAR(50) NOT NULL,
    name                        VARCHAR(200) NOT NULL,
    description                 VARCHAR(2000),
    scope_product_family        VARCHAR(200),
    scope_inspection_type       VARCHAR(200),
    scope_process_area          VARCHAR(200),
    validity_months             INT NOT NULL,
    renewal_window_days         INT NOT NULL DEFAULT 0,
    is_active                   BOOLEAN NOT NULL DEFAULT TRUE,
    created_by                  VARCHAR(100) NOT NULL,
    created_at                  TIMESTAMP NOT NULL DEFAULT NOW(),
    modified_by                 VARCHAR(100),
    modified_at                 TIMESTAMP
);
CREATE UNIQUE INDEX IF NOT EXISTS ix_qualifications_tenant_code
    ON training.qualifications (tenant_id, code);
CREATE INDEX IF NOT EXISTS ix_qualifications_tenant_active
    ON training.qualifications (tenant_id, is_active);

-- Training Courses
CREATE TABLE IF NOT EXISTS training.courses (
    id                          UUID PRIMARY KEY,
    tenant_id                   UUID NOT NULL,
    code                        VARCHAR(50) NOT NULL,
    name                        VARCHAR(200) NOT NULL,
    description                 VARCHAR(4000),
    duration_hours              NUMERIC(6,2) NOT NULL,
    assessment_type             VARCHAR(100),
    pass_criteria               VARCHAR(1000),
    qualification_id            UUID REFERENCES training.qualifications(id),
    is_active                   BOOLEAN NOT NULL DEFAULT TRUE,
    created_by                  VARCHAR(100) NOT NULL,
    created_at                  TIMESTAMP NOT NULL DEFAULT NOW(),
    modified_by                 VARCHAR(100),
    modified_at                 TIMESTAMP
);
CREATE UNIQUE INDEX IF NOT EXISTS ix_courses_tenant_code
    ON training.courses (tenant_id, code);
CREATE INDEX IF NOT EXISTS ix_courses_tenant_qualification
    ON training.courses (tenant_id, qualification_id);

-- Competency Records
CREATE TABLE IF NOT EXISTS training.competency_records (
    id                          UUID PRIMARY KEY,
    tenant_id                   UUID NOT NULL,
    employee_id                 UUID NOT NULL,
    qualification_id            UUID NOT NULL REFERENCES training.qualifications(id),
    status                      VARCHAR(50) NOT NULL DEFAULT 'NotStarted',
    qualified_date              TIMESTAMP,
    expiry_date                 TIMESTAMP,
    assessor_id                 UUID,
    evidence_ref                VARCHAR(500)
);
CREATE UNIQUE INDEX IF NOT EXISTS ix_competency_records_tenant_employee_qualification
    ON training.competency_records (tenant_id, employee_id, qualification_id);
CREATE INDEX IF NOT EXISTS ix_competency_records_tenant_status
    ON training.competency_records (tenant_id, status);
CREATE INDEX IF NOT EXISTS ix_competency_records_tenant_expiry
    ON training.competency_records (tenant_id, expiry_date);

-- Training Assignments
CREATE TABLE IF NOT EXISTS training.training_assignments (
    id                          UUID PRIMARY KEY,
    tenant_id                   UUID NOT NULL,
    employee_id                 UUID NOT NULL,
    course_id                   UUID NOT NULL REFERENCES training.courses(id),
    assigned_by                 UUID NOT NULL,
    assigned_date               TIMESTAMP NOT NULL DEFAULT NOW(),
    due_date                    TIMESTAMP NOT NULL,
    status                      VARCHAR(50) NOT NULL DEFAULT 'Assigned',
    completion_date             TIMESTAMP,
    score                       NUMERIC(6,2),
    result                      VARCHAR(50),
    assessor_id                 UUID,
    evidence_ref                VARCHAR(500),
    created_by                  VARCHAR(100) NOT NULL,
    created_at                  TIMESTAMP NOT NULL DEFAULT NOW(),
    modified_by                 VARCHAR(100),
    modified_at                 TIMESTAMP
);
CREATE INDEX IF NOT EXISTS ix_training_assignments_tenant_employee
    ON training.training_assignments (tenant_id, employee_id);
CREATE INDEX IF NOT EXISTS ix_training_assignments_tenant_course
    ON training.training_assignments (tenant_id, course_id);
CREATE INDEX IF NOT EXISTS ix_training_assignments_tenant_status
    ON training.training_assignments (tenant_id, status);
CREATE INDEX IF NOT EXISTS ix_training_assignments_tenant_due
    ON training.training_assignments (tenant_id, due_date);

-- Training RLS Policies
ALTER TABLE training.qualifications ENABLE ROW LEVEL SECURITY;
ALTER TABLE training.courses ENABLE ROW LEVEL SECURITY;
ALTER TABLE training.competency_records ENABLE ROW LEVEL SECURITY;
ALTER TABLE training.training_assignments ENABLE ROW LEVEL SECURITY;

CREATE POLICY tenant_isolation_qualifications ON training.qualifications
    USING (tenant_id = shared.current_tenant_id());

CREATE POLICY tenant_isolation_courses ON training.courses
    USING (tenant_id = shared.current_tenant_id());

CREATE POLICY tenant_isolation_competency_records ON training.competency_records
    USING (tenant_id = shared.current_tenant_id());

CREATE POLICY tenant_isolation_training_assignments ON training.training_assignments
    USING (tenant_id = shared.current_tenant_id());

-- ============================================================================
-- SPC (Statistical Process Control) Schema
-- ============================================================================

CREATE TABLE IF NOT EXISTS spc.control_charts (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL REFERENCES identity.tenants(id),
    code VARCHAR(50) NOT NULL,
    name VARCHAR(200) NOT NULL,
    chart_type VARCHAR(50) NOT NULL,
    part_id UUID NOT NULL,
    characteristic_name VARCHAR(200) NOT NULL,
    subgroup_size INT NOT NULL DEFAULT 1,
    status VARCHAR(50) NOT NULL DEFAULT 'Active',
    upper_spec_limit NUMERIC(18,6),
    lower_spec_limit NUMERIC(18,6),
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    -- Control limits (flattened from ControlLimits value object)
    ucl NUMERIC(18,6),
    center_line NUMERIC(18,6),
    lcl NUMERIC(18,6),
    cl_upper_spec_limit NUMERIC(18,6),
    cl_lower_spec_limit NUMERIC(18,6),
    -- Process capability (flattened from ProcessCapability value object)
    cp NUMERIC(10,4),
    cpk NUMERIC(10,4),
    pp NUMERIC(10,4),
    ppk NUMERIC(10,4),
    cap_mean NUMERIC(18,6),
    cap_std_dev NUMERIC(18,6),
    cap_sample_size INT,
    cap_calculated_at TIMESTAMPTZ,
    -- Audit
    created_by VARCHAR(100) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    modified_by VARCHAR(100),
    modified_at TIMESTAMPTZ,
    CONSTRAINT uq_control_charts_tenant_code UNIQUE (tenant_id, code)
);

-- ix_control_charts_tenant_code is implicitly created by UNIQUE constraint uq_control_charts_tenant_code
CREATE INDEX IF NOT EXISTS ix_control_charts_tenant_part ON spc.control_charts(tenant_id, part_id);
CREATE INDEX IF NOT EXISTS ix_control_charts_tenant_status ON spc.control_charts(tenant_id, status);

CREATE TABLE IF NOT EXISTS spc.data_points (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL REFERENCES identity.tenants(id),
    control_chart_id UUID NOT NULL REFERENCES spc.control_charts(id) ON DELETE CASCADE,
    value NUMERIC(18,6) NOT NULL,
    subgroup_values VARCHAR(2000),
    sample_size INT NOT NULL DEFAULT 1,
    timestamp TIMESTAMPTZ NOT NULL,
    inspection_id UUID,
    rule_violation VARCHAR(100),
    is_out_of_control BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE INDEX IF NOT EXISTS ix_data_points_chart_timestamp ON spc.data_points(control_chart_id, timestamp);
CREATE INDEX IF NOT EXISTS ix_data_points_tenant_inspection ON spc.data_points(tenant_id, inspection_id);

-- SPC RLS Policies
ALTER TABLE spc.control_charts ENABLE ROW LEVEL SECURITY;
ALTER TABLE spc.data_points ENABLE ROW LEVEL SECURITY;

CREATE POLICY tenant_isolation_control_charts ON spc.control_charts
    USING (tenant_id = shared.current_tenant_id());

CREATE POLICY tenant_isolation_data_points ON spc.data_points
    USING (tenant_id = shared.current_tenant_id());

-- ============================================================================
-- AI Engine Schema
-- ============================================================================

CREATE TABLE IF NOT EXISTS ai_engine.ai_models (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL REFERENCES identity.tenants(id),
    name VARCHAR(200) NOT NULL,
    version VARCHAR(100) NOT NULL,
    capability VARCHAR(50) NOT NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'Training',
    description VARCHAR(1000),
    training_metrics TEXT,
    validation_metrics TEXT,
    hyper_parameters TEXT,
    data_snapshot_reference VARCHAR(500),
    training_sample_count INT,
    trained_at TIMESTAMPTZ,
    promoted_at TIMESTAMPTZ,
    retired_at TIMESTAMPTZ,
    -- Audit
    created_by VARCHAR(100) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    modified_by VARCHAR(100),
    modified_at TIMESTAMPTZ,
    CONSTRAINT uq_ai_models_tenant_name_version UNIQUE (tenant_id, name, version)
);

-- uq_ai_models_tenant_name_version implicitly creates index on (tenant_id, name, version)
CREATE INDEX IF NOT EXISTS ix_ai_models_tenant_capability ON ai_engine.ai_models(tenant_id, capability);
CREATE INDEX IF NOT EXISTS ix_ai_models_tenant_status ON ai_engine.ai_models(tenant_id, status);

CREATE TABLE IF NOT EXISTS ai_engine.ai_interactions (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL REFERENCES identity.tenants(id),
    capability VARCHAR(50) NOT NULL,
    user_id UUID NOT NULL,
    model_id VARCHAR(200),
    input_summary VARCHAR(2000),
    output_summary TEXT,
    confidence_score NUMERIC(5,4),
    confidence_level VARCHAR(50),
    source_references TEXT,
    status VARCHAR(50) NOT NULL DEFAULT 'Pending',
    user_action VARCHAR(50),
    user_justification VARCHAR(1000),
    requested_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    completed_at TIMESTAMPTZ,
    response_time_ms INT,
    -- Audit
    created_by VARCHAR(100) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    modified_by VARCHAR(100),
    modified_at TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS ix_ai_interactions_tenant_capability ON ai_engine.ai_interactions(tenant_id, capability);
CREATE INDEX IF NOT EXISTS ix_ai_interactions_tenant_user ON ai_engine.ai_interactions(tenant_id, user_id);
CREATE INDEX IF NOT EXISTS ix_ai_interactions_tenant_status ON ai_engine.ai_interactions(tenant_id, status);
CREATE INDEX IF NOT EXISTS ix_ai_interactions_requested_at ON ai_engine.ai_interactions(tenant_id, requested_at);

CREATE TABLE IF NOT EXISTS ai_engine.ai_capability_configs (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL REFERENCES identity.tenants(id),
    capability VARCHAR(50) NOT NULL,
    is_enabled BOOLEAN NOT NULL DEFAULT FALSE,
    low_confidence_threshold NUMERIC(5,4) NOT NULL DEFAULT 0.3000,
    moderate_confidence_threshold NUMERIC(5,4) NOT NULL DEFAULT 0.6000,
    high_confidence_threshold NUMERIC(5,4) NOT NULL DEFAULT 0.8500,
    -- Audit
    created_by VARCHAR(100) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    modified_by VARCHAR(100),
    modified_at TIMESTAMPTZ,
    CONSTRAINT uq_ai_capability_configs_tenant_capability UNIQUE (tenant_id, capability)
);

-- AI Engine RLS Policies
-- AI Action Logs (Enhanced AI Audit Trail — v2.0)
CREATE TABLE IF NOT EXISTS ai_engine.ai_action_logs (
    id                    UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id             UUID NOT NULL,
    user_id               UUID NOT NULL,
    user_role             VARCHAR(128) NOT NULL,
    permission_level      VARCHAR(64) NOT NULL,
    action_type           VARCHAR(128) NOT NULL,
    action_category       VARCHAR(64) NOT NULL,
    prompt                TEXT,
    reasoning_summary     TEXT,
    affected_modules      TEXT,
    affected_records      TEXT,
    risk_level            VARCHAR(32) NOT NULL,
    confirmation_status   VARCHAR(32) NOT NULL,
    requires_confirmation BOOLEAN NOT NULL DEFAULT FALSE,
    confirmed_at          TIMESTAMPTZ,
    confirmed_by          VARCHAR(256),
    execution_result      VARCHAR(64) NOT NULL,
    error_detail          TEXT,
    requested_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    completed_at          TIMESTAMPTZ,
    duration_ms           INTEGER,
    model_version         VARCHAR(128),
    confidence_score      NUMERIC(5,4),
    is_rollback_possible  BOOLEAN NOT NULL DEFAULT FALSE,
    created_by            VARCHAR(256) NOT NULL DEFAULT '',
    created_at            TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    modified_by           VARCHAR(256),
    modified_at           TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS ix_ai_action_logs_tenant_user
    ON ai_engine.ai_action_logs(tenant_id, user_id);
CREATE INDEX IF NOT EXISTS ix_ai_action_logs_tenant_action_type
    ON ai_engine.ai_action_logs(tenant_id, action_type);
CREATE INDEX IF NOT EXISTS ix_ai_action_logs_requested_at
    ON ai_engine.ai_action_logs(tenant_id, requested_at DESC);
CREATE INDEX IF NOT EXISTS ix_ai_action_logs_confirmation
    ON ai_engine.ai_action_logs(tenant_id, confirmation_status)
    WHERE confirmation_status = 'Pending';

-- AI Permission Policies (v2.0)
CREATE TABLE IF NOT EXISTS ai_engine.ai_permission_policies (
    id                 UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id          UUID NOT NULL,
    user_id            UUID NOT NULL,
    permission_level   VARCHAR(64) NOT NULL,
    is_active          BOOLEAN NOT NULL DEFAULT TRUE,
    granted_by_user_id VARCHAR(256),
    granted_at         TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    revoked_by_user_id VARCHAR(256),
    revoked_at         TIMESTAMPTZ,
    notes              TEXT,
    created_by         VARCHAR(256) NOT NULL DEFAULT '',
    created_at         TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    modified_by        VARCHAR(256),
    modified_at        TIMESTAMPTZ
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_ai_permission_policies_active_user
    ON ai_engine.ai_permission_policies(tenant_id, user_id)
    WHERE is_active = TRUE;
CREATE INDEX IF NOT EXISTS ix_ai_permission_policies_tenant_user
    ON ai_engine.ai_permission_policies(tenant_id, user_id);

-- AI Workflow Definitions (v2.0)
CREATE TABLE IF NOT EXISTS ai_engine.ai_workflow_definitions (
    id                       UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id                UUID NOT NULL,
    name                     VARCHAR(256) NOT NULL,
    description              TEXT,
    minimum_permission_level VARCHAR(64) NOT NULL,
    category                 VARCHAR(64) NOT NULL,
    is_active                BOOLEAN NOT NULL DEFAULT TRUE,
    steps_definition         JSONB NOT NULL,
    affected_modules         TEXT NOT NULL,
    created_by               VARCHAR(256) NOT NULL DEFAULT '',
    created_at               TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    modified_by              VARCHAR(256),
    modified_at              TIMESTAMPTZ
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_ai_workflow_definitions_tenant_name
    ON ai_engine.ai_workflow_definitions(tenant_id, name);

-- AI Workflow Executions (v2.0)
CREATE TABLE IF NOT EXISTS ai_engine.ai_workflow_executions (
    id                      UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id               UUID NOT NULL,
    workflow_definition_id  UUID NOT NULL REFERENCES ai_engine.ai_workflow_definitions(id),
    workflow_name           VARCHAR(256) NOT NULL,
    user_id                 UUID NOT NULL,
    status                  VARCHAR(64) NOT NULL,
    total_steps             INTEGER NOT NULL,
    completed_steps         INTEGER NOT NULL DEFAULT 0,
    failed_steps            INTEGER NOT NULL DEFAULT 0,
    step_results            JSONB,
    output                  JSONB,
    started_at              TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    completed_at            TIMESTAMPTZ,
    total_duration_ms       INTEGER,
    error_summary           TEXT,
    created_by              VARCHAR(256) NOT NULL DEFAULT '',
    created_at              TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    modified_by             VARCHAR(256),
    modified_at             TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS ix_ai_workflow_executions_tenant_user
    ON ai_engine.ai_workflow_executions(tenant_id, user_id);
CREATE INDEX IF NOT EXISTS ix_ai_workflow_executions_tenant_status
    ON ai_engine.ai_workflow_executions(tenant_id, status);
CREATE INDEX IF NOT EXISTS ix_ai_workflow_executions_definition
    ON ai_engine.ai_workflow_executions(workflow_definition_id);

-- RLS policies
ALTER TABLE ai_engine.ai_models ENABLE ROW LEVEL SECURITY;
ALTER TABLE ai_engine.ai_interactions ENABLE ROW LEVEL SECURITY;
ALTER TABLE ai_engine.ai_capability_configs ENABLE ROW LEVEL SECURITY;
ALTER TABLE ai_engine.ai_action_logs ENABLE ROW LEVEL SECURITY;
ALTER TABLE ai_engine.ai_permission_policies ENABLE ROW LEVEL SECURITY;
ALTER TABLE ai_engine.ai_workflow_definitions ENABLE ROW LEVEL SECURITY;
ALTER TABLE ai_engine.ai_workflow_executions ENABLE ROW LEVEL SECURITY;

CREATE POLICY tenant_isolation_ai_models ON ai_engine.ai_models
    USING (tenant_id = shared.current_tenant_id());

CREATE POLICY tenant_isolation_ai_interactions ON ai_engine.ai_interactions
    USING (tenant_id = shared.current_tenant_id());

CREATE POLICY tenant_isolation_ai_capability_configs ON ai_engine.ai_capability_configs
    USING (tenant_id = shared.current_tenant_id());

CREATE POLICY tenant_isolation_ai_action_logs ON ai_engine.ai_action_logs
    USING (tenant_id = shared.current_tenant_id());

CREATE POLICY tenant_isolation_ai_permission_policies ON ai_engine.ai_permission_policies
    USING (tenant_id = shared.current_tenant_id());

CREATE POLICY tenant_isolation_ai_workflow_definitions ON ai_engine.ai_workflow_definitions
    USING (tenant_id = shared.current_tenant_id());

CREATE POLICY tenant_isolation_ai_workflow_executions ON ai_engine.ai_workflow_executions
    USING (tenant_id = shared.current_tenant_id());
