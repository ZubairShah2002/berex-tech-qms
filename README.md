# Berex Tech QMS

Enterprise Quality Management System for discrete manufacturing, built as a modular monolith with Clean Architecture and Domain-Driven Design.

## Quick Deploy to Render (Free)

[![Deploy to Render](https://render.com/images/deploy-to-render-button.svg)](https://render.com/deploy?repo=https://github.com/ZubairShah2002/berex-tech-qms)

> After deployment, login with: `admin@berextech.com` / `Admin@123456`

## Tech Stack

- **Backend**: .NET 8 (ASP.NET Core Web API), Entity Framework Core 8, MediatR (CQRS)
- **Frontend**: React 18 + TypeScript + Vite
- **Database**: PostgreSQL 16 with Row-Level Security (multi-tenancy)
- **Cache**: Redis 7
- **Object Storage**: MinIO (S3-compatible)

## 12 Bounded Contexts

| Module | Description |
|--------|-------------|
| Identity | Users, roles, permissions, tenants, JWT auth |
| Product Catalog | Parts, revisions, specifications |
| Quality Inspection | Inspections, sampling plans, measurements |
| Non-Conformance (NCR) | Non-conformance reports, dispositions |
| CAPA | Corrective & preventive actions |
| Document Control | Document lifecycle, versioning, approvals |
| Audit Management | Internal/external audits, findings |
| Supplier Quality | Supplier assessments, scorecards |
| Calibration | Equipment, calibration records |
| Training | Courses, assignments, competencies |
| SPC | Statistical process control, control charts |
| AI Engine | AI-powered quality insights, recommendations |

## Local Development

### Prerequisites

- Docker Desktop (or Docker Engine + Docker Compose v2)
- .NET 8 SDK (for backend development)
- Node.js 22 (for frontend development)

### One-Command Start

```bash
./deploy-preview.sh
```

Opens at http://localhost:3000

### Manual Setup

```bash
# Start infrastructure
docker compose -f docker/docker-compose.yml up -d

# Backend
dotnet build BerexQms.sln
dotnet run --project src/BerexQms.Api

# Frontend
cd src/BerexQms.Web
npm ci
npm run dev
```

### Testing

```bash
dotnet test                      # 265 tests across 4 projects
cd src/BerexQms.Web && npm run lint   # Frontend linting
cd src/BerexQms.Web && npx tsc --noEmit  # Type checking
```

## Architecture

```
src/
├── BerexQms.SharedKernel/    # Base classes, value objects, interfaces
├── BerexQms.Domain/          # Entities, aggregates, domain events
├── BerexQms.Application/     # CQRS handlers, pipeline behaviors
├── BerexQms.Infrastructure/  # EF Core, Redis, MinIO implementations
├── BerexQms.Api/             # ASP.NET Core host, middleware, controllers
└── BerexQms.Web/             # React 18 + TypeScript frontend
```

See `docs/Berex_Tech_QMS_Architecture_Blueprint_v1.1_Frozen.md` for the full architectural specification.
