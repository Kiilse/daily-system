---
tags:
  - adr
  - architecture
adr: 002
status: Accepted
date: 2026-09-25
related: "[Ecosystem SaaS - Technical Specification](../specs/technical-specification.md)"
---

# ADR-002: Monorepo

## Status

Accepted (2026-09-25)

## Context

The ecosystem has a backend (.NET), a single Angular frontend and shared infrastructure code. The initial preference was a polyrepo (one repository per product) for scalability. After the budget and deadline were fixed, the decision was revisited.

## Decision

One Git repository for the whole ecosystem, with `src/backend`, `src/frontend`, `infra`, `docs` and `tools`. CI workflows use path filters so each part only builds when it changes.

## Alternatives Considered

| Option | Why not |
|---|---|
| Polyrepo (one repository per product) | Repository structure does not affect runtime scalability. It would multiply CI pipelines, branch protections and Dependabot configs, and require internal NuGet/npm packages for shared code, for a single deployment in v1 |
| Hybrid (backend repo + frontend repo) | A change touching the API and the client would need two PRs to stay in sync, which breaks "one feature = one branch" |

## Consequences

**Positive**
- One feature = one branch = one PR, even when it touches backend, frontend and generated client together.
- Shared code (building blocks, UI library, generated clients) without publishing packages.
- One place for issues, ADRs and security scanning.

**Negative**
- CI must use path filters to stay fast.
- If a product is ever sold or given its own team, it will need to be extracted (Git history can be kept with `git filter-repo`).

## Key Concepts

- **Monorepo**: several projects in one repository, versioned together. It says nothing about how they are deployed.
- **Path filters**: CI rules that only run a job when files under a given path changed.

## Related

- Specs: [Ecosystem SaaS - Technical Specification](../specs/technical-specification.md) · [Ecosystem SaaS - Functional Specification](../specs/functional-specification.md)
- Backlog: [daily-system - Backlog](https://github.com/Kiilse/daily-system/issues)
- Related ADRs: [ADR-001](adr-001-modular-monolith-with-co-hosted-modules-in-v1.md)
- Tickets implementing this decision: [#1](https://github.com/Kiilse/daily-system/issues/1)
