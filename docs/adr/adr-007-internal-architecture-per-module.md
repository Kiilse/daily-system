---
tags:
  - adr
  - architecture
adr: 007
status: Accepted
date: 2026-09-25
related: "[Ecosystem SaaS - Technical Specification](../specs/technical-specification.md)"
---

# ADR-007: Internal architecture per module

## Status

Accepted (2026-09-25)

## Context

The previous FinTrack plan used Clean Architecture. Menu and Calendar are mostly CRUD with a few rules; FinTrack has rich rules (envelope modes, tag-based tracking, history, multi-currency). Development is organized as one feature per ticket.

## Decision

- **Vertical Slice Architecture** for Account, Menu and Calendar: one folder per feature containing endpoint, request, handler, validator and tests. Rules shared by several features live in a small `Domain/` folder.
- **Clean Architecture** for FinTrack: Domain, Application, Infrastructure, Api projects.
- Common: Minimal APIs, FluentValidation, `Result` type instead of exceptions for business errors, Problem Details (RFC 9457) for HTTP errors.
- No MediatR: handlers are plain classes resolved by DI (MediatR moved to a commercial license in 2025, and it isn't needed).

## Alternatives Considered

| Option | Why not |
|---|---|
| Clean Architecture everywhere | Four projects and several layers for a CRUD feature: ceremony without benefit |
| Vertical slice everywhere | FinTrack's rules benefit from a domain layer isolated from infrastructure |

## Consequences

**Positive**
- A ticket maps to a folder: easy to review, easy to delete.
- FinTrack's rules are testable without any database.

**Negative**
- Two styles to know in the same repository. Documented here so it's a deliberate choice, not drift.

## Key Concepts

- **Vertical slice**: organize code by feature (what the user does) instead of by technical layer.
- **Clean Architecture**: dependencies point inward, toward the domain; the domain knows nothing about databases or HTTP.
- **Problem Details**: a standard JSON format for HTTP errors.

## Related

- Specs: [Ecosystem SaaS - Technical Specification](../specs/technical-specification.md) · [Ecosystem SaaS - Functional Specification](../specs/functional-specification.md)
- Backlog: [daily-system - Backlog](https://github.com/Kiilse/daily-system/issues)
- Related ADRs: [ADR-001](adr-001-modular-monolith-with-co-hosted-modules-in-v1.md) · [ADR-020](adr-020-market-data-providers.md)
- Tickets implementing this decision: [#2](https://github.com/Kiilse/daily-system/issues/2) · [#23](https://github.com/Kiilse/daily-system/issues/23) · [#24](https://github.com/Kiilse/daily-system/issues/24) · [#25](https://github.com/Kiilse/daily-system/issues/25) · [#26](https://github.com/Kiilse/daily-system/issues/26) · [#27](https://github.com/Kiilse/daily-system/issues/27) · [#32](https://github.com/Kiilse/daily-system/issues/32) · [#34](https://github.com/Kiilse/daily-system/issues/34) · [#39](https://github.com/Kiilse/daily-system/issues/39) · [#40](https://github.com/Kiilse/daily-system/issues/40)
