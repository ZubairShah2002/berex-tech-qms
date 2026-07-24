# ADR-0001: Modular Monolith Architecture

## Status

Accepted

## Context

Berex Tech QMS requires twelve domain modules that interlock deliberately. The development team is four to six developers. We needed to choose between microservices, a traditional monolith, or a modular monolith.

## Decision

We adopt a modular monolith architecture — a single deployable unit composed of well-separated domain modules with enforced boundaries. Each module communicates with others exclusively through a domain event bus or defined anti-corruption layers.

## Consequences

- Single deployment pipeline simplifies operations for a small team.
- Shared transaction context enables strong consistency within bounded contexts.
- Module boundaries are enforced through separate namespaces and project structure.
- Documented extraction seams allow future decomposition into microservices when scaling demands warrant it.
- The team avoids the operational overhead of twelve independent services (distributed tracing, network partitioning, eventual consistency).
- Migration to microservices is a bounded operation rather than a full rewrite.

## Related ADRs

- ADR-0002: Clean Architecture with DDD
