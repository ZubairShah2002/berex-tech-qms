# ADR-0003: PostgreSQL Multi-Tenancy via Row-Level Security

## Status

Accepted

## Context

Berex Tech QMS serves multiple manufacturing tenants from a single deployment. Each tenant's data must be strictly isolated — quality records, audit trails, and compliance documents must never leak across tenant boundaries. The system must support this without requiring separate database instances per tenant.

## Decision

Use PostgreSQL Row-Level Security (RLS) with a shared database, shared schema approach:

1. Every tenant-scoped table includes a `tenant_id UUID NOT NULL` column
2. RLS policies enforce tenant isolation at the database level via `current_setting('app.current_tenant_id')`
3. The application sets the session variable on each request via the tenant context middleware
4. Per-module schemas (`identity`, `inspection`, `nonconformance`, `capa`, `document_control`, `audit_management`, `supplier_quality`, `calibration`, `training`, `product_catalog`, `spc`, `ai_engine`) organize tables by bounded context
5. A shared schema (`shared`) holds cross-cutting tables (audit_log, domain_events_outbox)

**Why RLS over schema-per-tenant or database-per-tenant:**
- Schema-per-tenant complicates migrations (must apply to each schema)
- Database-per-tenant increases operational overhead and connection pool fragmentation
- RLS provides defense-in-depth — even application bugs cannot read another tenant's data

## Consequences

- Strong tenant isolation enforced at the database level
- Single migration path for all tenants
- RLS adds a small query planning overhead (negligible with proper indexes)
- Must ensure `app.current_tenant_id` is set before any query — forgetting it blocks rather than leaks (policies default to deny)
- Cross-tenant queries (for admin/reporting) require explicitly setting the session variable or using BYPASSRLS role
