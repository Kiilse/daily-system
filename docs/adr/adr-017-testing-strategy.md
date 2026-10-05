---
tags:
  - adr
  - architecture
adr: 017
status: Accepted
date: 2026-09-25
related: "[Ecosystem SaaS - Technical Specification](../specs/technical-specification.md)"
---

# ADR-017: Testing strategy

## Status

Accepted (2026-09-25)

## Context

Agile practice with unit tests, TDD requested, continuous delivery. No test exists yet.

## Decision

- **TDD** on Domain and Application code.
- Backend: xUnit v3, NSubstitute, Shouldly, `WebApplicationFactory` + **Testcontainers** PostgreSQL for integration tests, **ArchUnitNET** for architecture rules.
- Frontend: Vitest + Angular Testing Library; **Playwright** E2E against dev after each deploy.
- **Coverage gate 80 %** on Domain and Application (Coverlet), not on generated or glue code.
- Every sharing or authorization rule has a negative test ("user B cannot read user A's data").
- Encryption building block: round-trip, tampering detection, wrong key, key rotation tests.

## Alternatives Considered

| Option | Why not |
|---|---|
| EF Core InMemory provider | Doesn't behave like PostgreSQL (constraints, RLS, transactions) |
| FluentAssertions | v8 requires a paid license for commercial use |
| Global 80 % coverage | Pushes toward tests of trivial code |

## Consequences

**Positive**
- Enough confidence to deploy continuously to dev and quickly to prod.

**Negative**
- Integration tests need Docker in CI (available on GitHub-hosted runners) and add a few minutes.

## Key Concepts

- **Test pyramid**: many fast unit tests, fewer integration tests, very few E2E tests.
- **Testcontainers**: a library that starts real services (PostgreSQL) in throwaway Docker containers during tests.
- **Architecture test**: a test that fails when code breaks a structural rule (e.g. Menu referencing Calendar's internals).

## Related

- Specs: [Ecosystem SaaS - Technical Specification](../specs/technical-specification.md) · [Ecosystem SaaS - Functional Specification](../specs/functional-specification.md)
- Backlog: [daily-system - Backlog](https://github.com/Kiilse/daily-system/issues)
- Related ADRs: [ADR-003](adr-003-github-flow-with-short-lived-feature-branches-and-feature-flags.md) · [ADR-004](adr-004-ci-cd-with-build-once-deploy-many-and-manual-production-approval.md) · [ADR-016](adr-016-security-scanning-pipeline.md)
- Tickets implementing this decision: [#3](https://github.com/Kiilse/daily-system/issues/3) · [#6](https://github.com/Kiilse/daily-system/issues/6) · [#22](https://github.com/Kiilse/daily-system/issues/22)
