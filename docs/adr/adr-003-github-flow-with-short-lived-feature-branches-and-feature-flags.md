---
tags:
  - adr
  - architecture
adr: 003
status: Accepted
date: 2026-09-25
related: "[Ecosystem SaaS - Technical Specification](../specs/technical-specification.md)"
---

# ADR-003: GitHub Flow with short-lived feature branches and feature flags

## Status

Accepted (2026-09-25)

## Context

The project follows agile principles: feature-by-feature development from spec tickets, and one feature equals one Git branch. It also targets continuous delivery, which requires `main` to be releasable at all times.

## Decision

- **GitHub Flow**: `main` is always deployable. Work happens in branches created from `main` and merged back through a PR.
- One GitHub Issue (feature-spec template) = one branch = one PR. Naming: `feature/<issue>-<slug>`, `fix/<issue>-<slug>`, `chore/<slug>`.
- Branch lifetime target: 3 days maximum. Bigger features are split into several issues; unfinished work merged to `main` is hidden behind a **feature flag** (`Microsoft.FeatureManagement`).
- Conventional Commits, squash merge, linear history.
- `main` protection: PR required, all checks green, branch up to date, no direct or force push.
- GitHub Issues are the execution backlog. Notion holds specs and roadmap, synced with the Notion GitHub integration.

## Alternatives Considered

| Option | Why not |
|---|---|
| GitFlow (develop, release, hotfix branches) | Designed for scheduled releases and several maintained versions. Heavy for one developer doing continuous delivery |
| Trunk-based without branches | Contradicts "one feature = one branch" and loses the PR as a gate for automated checks |
| Long-lived feature branches | Painful merges and no continuous integration: the code is not integrated until the end |

## Consequences

**Positive**
- Every change goes through the same automated gate.
- Git history maps to issues, issues map to the functional spec.

**Negative**
- Feature flags add conditional code that must be cleaned up once a feature is released (tracked as a `chore` issue).

## Key Concepts

- **Feature flag**: a configuration switch that enables or disables a feature at runtime, without redeploying. It lets you merge unfinished code safely.
- **Conventional Commits**: a commit message format (`feat(menu): add ingredient import`) that tools can read to generate changelogs.
- **Squash merge**: all commits of a PR become one commit on `main`, so `main` has one commit per feature.

## Related

- Specs: [Ecosystem SaaS - Technical Specification](../specs/technical-specification.md) · [Ecosystem SaaS - Functional Specification](../specs/functional-specification.md)
- Backlog: [daily-system - Backlog](https://github.com/Kiilse/daily-system/issues)
- Related ADRs: [ADR-017](adr-017-testing-strategy.md)
- Tickets implementing this decision: [#1](https://github.com/Kiilse/daily-system/issues/1)
