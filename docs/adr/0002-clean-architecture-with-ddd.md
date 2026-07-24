# ADR-0002: Clean Architecture with Domain-Driven Design

## Status

Accepted

## Context

Berex Tech QMS is a complex enterprise system with 12 bounded contexts spanning quality management, compliance, and manufacturing operations. The domain logic is intricate — inspection workflows, CAPA escalation rules, document approval chains, calibration schedules — and must remain stable as infrastructure concerns evolve.

## Decision

Adopt Clean Architecture (Onion Architecture) with Domain-Driven Design tactical patterns.

**Layer structure:**
- **SharedKernel** — Cross-cutting value objects, base classes, interfaces
- **Domain** — Entities, aggregates, domain events, domain services (zero external dependencies)
- **Application** — Use cases via CQRS (MediatR), pipeline behaviors, application services
- **Infrastructure** — EF Core, Redis, MinIO, external integrations
- **Api** — HTTP entry point, middleware, composition root

**DDD patterns used:**
- Aggregate Root with domain event collection
- Value Objects for type safety (TenantId, Money, DateRange, EmailAddress)
- Repository pattern per aggregate
- Specification pattern for query composition
- Domain Events with Outbox pattern for reliable delivery

**Dependency rule:** Dependencies point inward only. Domain has no dependency on Infrastructure or Application. Application depends on Domain. Infrastructure implements Application interfaces.

## Consequences

- Domain logic is testable without infrastructure
- New persistence or caching strategies can be swapped without domain changes
- Verbose — more interfaces, more files, more indirection than a simpler layered approach
- Requires discipline to avoid leaking infrastructure concerns into the domain
