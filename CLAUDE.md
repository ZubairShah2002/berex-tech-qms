# Berex Tech QMS — Project Conventions

## Architecture

- **Modular Monolith** with Clean Architecture and DDD
- **12 Bounded Contexts**: Identity, Inspection, NonConformance, Capa, DocumentControl, AuditManagement, SupplierQuality, Calibration, Training, ProductCatalog, Spc, AiEngine
- **Dependency direction**: SharedKernel <- Domain <- Application <- Infrastructure <- Api
- **Blueprint**: `docs/Berex_Tech_QMS_Architecture_Blueprint_v1.1_Frozen.md` is the immutable source of truth

## Backend (.NET 8)

### Solution Structure
```
src/BerexQms.SharedKernel/   # Base classes, value objects, interfaces
src/BerexQms.Domain/          # Entities, aggregates, domain events
src/BerexQms.Application/     # CQRS handlers, pipeline behaviors, interfaces
src/BerexQms.Infrastructure/  # EF Core, Redis, MinIO implementations
src/BerexQms.Api/              # ASP.NET Core host, middleware, controllers
```

### Patterns
- **CQRS** via MediatR — `ICommand<T>` / `IQuery<T>` with handlers
- **Repository** per aggregate root — `IRepository<T>` in SharedKernel
- **Specification** pattern for queries
- **Unit of Work** via `IUnitOfWork` (implemented by DbContext)
- **Domain Events** with Outbox pattern

### Conventions
- All entities extend `Entity<TId>` or `AggregateRoot<TId>`
- Use value objects for IDs: `TenantId`, `UserId`
- Audit fields via `IAuditableEntity` — auto-set by `QmsDbContext.SetAuditFields()`
- Pipeline order: TenantContext -> Logging -> Validation -> Transaction
- Exceptions: `DomainException`, `NotFoundException`, `ValidationException`, `ForbiddenAccessException`, `ConflictException`

### Database
- PostgreSQL 16 with per-module schemas
- Multi-tenancy via Row-Level Security (RLS)
- EF Core migrations in shared schema
- Naming: snake_case for tables/columns

## Frontend (React 18 + TypeScript)

### Structure
```
src/BerexQms.Web/src/
  components/layout/    # AppShell, sidebar
  components/ui/        # Button, Input, Select, DataTable, Badge, etc.
  components/feedback/  # Toast, Alert, ConfirmDialog, ErrorBoundary
  hooks/                # Custom React hooks
  lib/                  # API client, query client, utilities
  pages/                # Route-level page components
  router/               # React Router configuration
  stores/               # Zustand state stores
  styles/               # Design tokens, reset, global CSS
```

### Design System
- CSS Modules for component styles
- Design tokens in `styles/tokens.css`
- Two density modes: **Office** (default, information-dense) and **Floor** (touch-first)
- Professional, muted color palette — enterprise-grade, not flashy
- `@/` path alias resolves to `src/`

### Libraries
- **State**: Zustand
- **Data fetching**: @tanstack/react-query
- **HTTP**: Axios with interceptors (auth, tenant, correlation ID)
- **Icons**: lucide-react
- **Routing**: react-router-dom

## Commands

### Backend
```bash
dotnet build BerexQms.sln          # Build all projects
dotnet test                         # Run all tests
dotnet build --configuration Release /p:TreatWarningsAsErrors=true
```

### Frontend
```bash
cd src/BerexQms.Web
npm run dev                         # Dev server on :5173
npm run build                       # Production build (tsc + vite)
npm run lint                        # OxLint
npx tsc --noEmit                    # Type check only
```

### Docker
```bash
docker compose -f docker/docker-compose.yml up -d
```

## Sprint Development

- Follow sprint plan sequentially (Sprint 0-19)
- No placeholder code — every deliverable must be production-ready
- Self-review against blueprint compliance before marking sprint complete
- Branch naming: `claude/berex-qms-implementation-*`
