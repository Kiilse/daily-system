---
tags:
  - adr
  - architecture
adr: 001
status: Accepted
date: 2026-09-25
related: "[Ecosystem SaaS - Technical Specification](../specs/technical-specification.md)"
---

# ADR-001: Modular monolith with co-hosted modules in v1

## Status

Accepted (2026-09-25)

## Context

The ecosystem has four products plus a Global account. The previous FinTrack plan targeted microservices behind an API gateway. Constraints: one developer, 20 €/month hosting, two weeks for the first release, real user data from day one. Railway bills every running service for its RAM and CPU, per environment.

## Decision

- Each product is a **module** with hard boundaries: own database, own code, public `*.Contracts` project, no access to another module's internals or database.
- In v1, the Account, Menu and Calendar modules are **co-hosted** in one process (`core`). The BFF is a separate process.
- FinTrack will run in its own process (`fintrack-api`) from its first release, because its data is the most sensitive.
- Boundaries are enforced by architecture tests, so splitting a module out later is a deployment change, not a rewrite.

## Alternatives Considered

| Option | Why not |
|---|---|
| Microservices (one service per product or per bounded context) | 10 to 20 containers across two environments: over budget, and distributed-system problems (network failures, data consistency, tracing) before any feature exists |
| Classic monolith (one project, no module boundaries) | Cheapest, but boundaries erode quickly and splitting later becomes a rewrite. Contradicts "separate products" |
| One container per product from day one | Cleaner isolation, about double the hosting cost, and more deployment work in week 1 |

## Consequences

**Positive**
- Hosting fits the budget. One deployment pipeline for three modules.
- Calls between Menu and Calendar are in-process: fast, no network failure modes.
- Clear path to split: move a module into its own host, replace the in-process contract implementation with an HTTP client.

**Negative**
- A crash or memory leak in one module takes down the other two.
- Modules share the same runtime version and deployment rhythm.
- Discipline is required: without architecture tests, boundaries would erode.

**Follow-ups**
- Architecture tests in Sprint 0 (see [ADR-017 Testing strategy](adr-017-testing-strategy.md)).
- Re-evaluate co-hosting when a module needs to scale or deploy independently.

## Key Concepts

- **Modular monolith**: one deployable application, internally split into modules that behave like separate services (own data, explicit contracts). You get most of the design benefits of microservices without the operational cost.
- **Bounded context**: a part of the business with its own vocabulary and rules. Here, each product is one.
- **Composition root**: the single place (`Host.Core/Program.cs`) where modules are registered and wired together.

## Related

- Specs: [Ecosystem SaaS - Technical Specification](../specs/technical-specification.md) · [Ecosystem SaaS - Functional Specification](../specs/functional-specification.md)
- Backlog: [daily-system - Backlog](https://github.com/Kiilse/daily-system/issues)
- Related ADRs: [ADR-002](adr-002-monorepo.md) · [ADR-007](adr-007-internal-architecture-per-module.md) · [ADR-013](adr-013-inter-module-communication.md) · [ADR-017](adr-017-testing-strategy.md)
- Tickets implementing this decision: [#2](https://github.com/Kiilse/daily-system/issues/2) · [#3](https://github.com/Kiilse/daily-system/issues/3) · [#39](https://github.com/Kiilse/daily-system/issues/39) · [#43](https://github.com/Kiilse/daily-system/issues/43)
