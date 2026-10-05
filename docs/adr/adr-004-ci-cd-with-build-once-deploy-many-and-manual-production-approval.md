---
tags:
  - adr
  - architecture
adr: 004
status: Accepted
date: 2026-09-25
related: "[Ecosystem SaaS - Technical Specification](../specs/technical-specification.md)"
---

# ADR-004: CI-CD with build once deploy many and manual production approval

## Status

Accepted (2026-09-25)

## Context

The project wants CI/CD and continuous deployment. With a single developer and a test suite that doesn't exist yet, automatic production deploys on every merge would ship bugs to real users with real financial data.

## Decision

- **Continuous delivery**: every merge to `main` is built, tested and deployed automatically to **dev**. Production deployment requires a manual approval in a GitHub Environment (`production`, required reviewer: owner).
- **Build once, deploy many**: images are built once per commit, tagged with the commit SHA, pushed to GitHub Container Registry, and the same image is promoted to prod.
- Database migrations run as an EF Core migration bundle, in a pipeline step, before the new image starts. Expand/contract pattern for breaking schema changes.
- Rollback = redeploy the previous SHA.
- Actions pinned by commit SHA. Secrets injected from Infisical.

## Alternatives Considered

| Option | Why not |
|---|---|
| Continuous deployment to prod | Requires a mature test suite and monitoring. Can be switched on later by removing the approval step |
| Rebuild per environment | The image running in prod would not be the one tested in dev |
| Migrations at app startup (`Database.Migrate()`) | Needs DDL rights at runtime, and several instances may race to migrate |

## Consequences

**Positive**
- Prod only runs artifacts already validated in dev.
- Rollback in minutes without rebuilding.

**Negative**
- A manual click is needed for each release.
- Expand/contract makes some schema changes take two releases.

## Key Concepts

- **Continuous integration**: merging often and checking every change automatically.
- **Continuous delivery**: every change is ready to go to prod; a human decides when.
- **Continuous deployment**: every change that passes checks goes to prod automatically.
- **Expand/contract**: add the new column first (expand), migrate code and data, remove the old column in a later release (contract), so the previous version still works during rollback.

## Related

- Specs: [Ecosystem SaaS - Technical Specification](../specs/technical-specification.md) · [Ecosystem SaaS - Functional Specification](../specs/functional-specification.md)
- Backlog: [daily-system - Backlog](https://github.com/Kiilse/daily-system/issues)
- Related ADRs: [ADR-016](adr-016-security-scanning-pipeline.md) · [ADR-017](adr-017-testing-strategy.md) · [ADR-018](adr-018-environments-and-configuration.md)
- Tickets implementing this decision: [#6](https://github.com/Kiilse/daily-system/issues/6) · [#7](https://github.com/Kiilse/daily-system/issues/7) · [#11](https://github.com/Kiilse/daily-system/issues/11) · [#31](https://github.com/Kiilse/daily-system/issues/31)
