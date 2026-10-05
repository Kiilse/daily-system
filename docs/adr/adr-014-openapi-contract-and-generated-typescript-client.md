---
tags:
  - adr
  - architecture
adr: 014
status: Accepted
date: 2026-09-25
related: "[Ecosystem SaaS - Technical Specification](../specs/technical-specification.md)"
---

# ADR-014: OpenAPI contract and generated TypeScript client

## Status

Accepted (2026-09-25)

## Context

The backend is .NET, the frontend TypeScript. Hand-written TS models drift from the API silently.

## Decision

- Each module exposes an OpenAPI document (ASP.NET Core built-in OpenAPI support).
- The TypeScript client is **generated** into `projects/api-clients` (tool to confirm in Sprint 0 between NSwag and `openapi-typescript`), never edited by hand.
- CI fails if the generated client is not up to date with the backend (drift check).

## Alternatives Considered

| Option | Why not |
|---|---|
| Hand-written models | Drift, runtime errors discovered by users |
| GraphQL | New paradigm, more tooling, no need identified |

## Consequences

**Positive**
- An API change that breaks the frontend fails at compile time, in the PR.

**Negative**
- Generated code in the repository, and a generation step to remember (enforced by CI).

## Key Concepts

- **OpenAPI**: a standard description of an HTTP API (routes, inputs, outputs) that tools can read to generate clients and docs.

## Related

- Specs: [Ecosystem SaaS - Technical Specification](../specs/technical-specification.md) · [Ecosystem SaaS - Functional Specification](../specs/functional-specification.md)
- Backlog: [daily-system - Backlog](https://github.com/Kiilse/daily-system/issues)
- Related ADRs: [ADR-013](adr-013-inter-module-communication.md) · [ADR-015](adr-015-angular-spa-single-frontend.md)
- Tickets implementing this decision: [#21](https://github.com/Kiilse/daily-system/issues/21)
