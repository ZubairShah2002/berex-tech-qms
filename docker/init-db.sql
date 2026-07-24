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
