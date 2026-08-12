# Sprint 19 — System Inspection Gap Analysis

## Inspection Summary

Full system inspection performed across all layers (Domain, Application, Infrastructure, API, Frontend, Database, Security) for Sprints 1–18.

## Consolidated Findings by Severity

### P0 — Security / Data Loss / Authorization (MUST FIX)

| # | Finding | Location | Required Action |
|---|---------|----------|-----------------|
| 1 | **X-Tenant-Id header overrides JWT tenant claim** — any authenticated user can access any tenant's data by setting a header | `TenantMiddleware.cs` | JWT claim takes priority; reject mismatched header |
| 2 | **No EF Core global query filters** — RepositoryBase queries across all tenants; `FindAsync` bypasses any filter | `QmsDbContext.cs`, `RepositoryBase.cs` | Add `HasQueryFilter` for TenantId on all tenant-scoped entities |
| 3 | **PostgreSQL RLS session variable never set** — `app.current_tenant_id` is never configured on DB connection | No `set_config` call anywhere | Set session variable on connection open (defense in depth) |
| 4 | **No role-based authorization on any endpoint** — all controllers use only `[Authorize]`, any authenticated user has full access | All 15 controllers | Add `[Authorize(Roles=...)]` on admin-only endpoints |

### P1 — Critical Workflow Failure (MUST FIX)

| # | Finding | Location | Required Action |
|---|---------|----------|-----------------|
| 5 | **Open registration with arbitrary role assignment** — unauthenticated callers can register as System Administrator | `AuthController.cs`, `RegisterUserCommand` | Remove RoleIds from registration; assign default role |
| 6 | **JWT expiry config key mismatch** — `ExpirationInMinutes` in config vs `ExpiryMinutes` in code; defaults to 15min | `JwtTokenService.cs:45` | Fix config key name |
| 7 | **Frontend API double-prefix bug** — 4 modules (Calibration, Training, SPC, AI Engine) have `/api/v1/api/v1/...` paths, all requests 404 | 15+ page files | Remove `/api/v1` prefix from API calls |
| 8 | **AuditRecord missing IAuditableEntity** — audit entity without audit trail fields | `AuditRecord.cs` | Add IAuditableEntity implementation |

### P2 — Major Functional Issue (SHOULD FIX)

| # | Finding | Location | Required Action |
|---|---------|----------|-----------------|
| 9 | **10 of 11 business modules have zero tests** — only AiEngine tested | `tests/` | Add integration tests for 5 required scenarios |
| 10 | **Dashboard is static** — shows "No data yet" with no API calls | `DashboardPage.tsx` | Wire up to real data |
| 11 | **Domain event outbox not wired** — events never dispatched | `QmsDbContext.cs` | Document as known limitation (not blocking) |
| 12 | **INotificationService has no implementation** | Application interfaces | Add stub implementation |
| 13 | **TenantMiddleware fails open** — no tenant context = proceeds with Guid.Empty | `TenantMiddleware.cs` | Return 400 for authenticated requests without tenant |
| 14 | **6 commands missing validators** | Identity commands | Add basic validators |
| 15 | **AuditTrailInterceptor missing CorrelationId** | `AuditTrailInterceptor.cs` | Inject IHttpContextAccessor |

### P3 — Low Severity (DOCUMENT)

| # | Finding | Location |
|---|---------|----------|
| 16 | Missing FKs for cross-module soft links (NCR→Inspection, CAPA→NCR) | `init-db.sql` |
| 17 | Breadcrumb component exists but unused across all pages | Frontend |
| 18 | Raw UUID inputs instead of entity pickers on forms | Frontend |
| 19 | Missing quarantine module (no domain entities) | Domain |
| 20 | String-typed status/enum fields instead of typed enums | Domain (systemic) |
| 21 | Duplicate events in Common/Events vs module-specific | Domain |
| 22 | No cross-module workflow orchestration (manual ID passing) | Application |
| 23 | Missing domain events for Supplier/Audit/Calibration modules | Domain |
| 24 | Western Electric Rules 2-8 not implemented in SPC | Domain |
| 25 | UserRole/RolePermission don't extend Entity<TId> | Domain |

### P4 — Cosmetic (SKIP)

| # | Finding | Location |
|---|---------|----------|
| 26 | Schema names abbreviated vs blueprint | `init-db.sql` |
| 27 | Request DTOs defined inside controller files | API controllers |
| 28 | Duplicate AddHealthChecks() calls | `Program.cs` + DI |

## Implementation Plan

**Phase 1 (P0):** Tenant isolation + Role-based authorization
**Phase 2 (P1):** Registration security + JWT fix + Frontend API paths + AuditRecord
**Phase 3 (P2):** Tests + Dashboard + Documentation
**Phase 4:** Build verification + Push
