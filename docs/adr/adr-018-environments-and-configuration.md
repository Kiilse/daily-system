---
tags:
  - adr
  - architecture
adr: 018
status: Accepted
date: 2026-09-25
related: "[Ecosystem SaaS - Technical Specification](../specs/technical-specification.md)"
---

# ADR-018: Environments and configuration

## Status

Accepted (2026-09-25)

## Context

Each product needs a dev and a prod environment. Real user data in prod from day one.

## Decision

- Three levels: **local** (`docker compose`), **dev** (Railway env + Cloudflare + Auth0 dev tenant, auto-deployed from `main`), **prod** (same, manual approval).
- No PR preview environments in v1 (budget).
- **No production data outside production**. Dev and local use generated data (seed scripts with Bogus).
- Configuration: `appsettings.json` for non-secret defaults, environment variables for everything environment-specific, secrets only from Infisical.
- Each environment has its own Auth0 tenant, Key Vault, database, and domain names.

## Alternatives Considered

| Option | Why not |
|---|---|
| PR preview environments | Useful, but costs Railway usage per open PR |
| Staging environment | A third paid environment; dev plays that role in v1 |

## Consequences

**Positive**
- A mistake in dev can't touch prod keys or data.

**Negative**
- Bugs that only appear with real data volumes may reach prod; mitigated by realistic generated datasets.

## Key Concepts

- **Environment parity**: dev and prod run the same images and topology, only configuration differs.
- **Bogus**: a .NET library that generates realistic fake data.

## Related

- Specs: [Ecosystem SaaS - Technical Specification](../specs/technical-specification.md) · [Ecosystem SaaS - Functional Specification](../specs/functional-specification.md)
- Backlog: [daily-system - Backlog](https://github.com/Kiilse/daily-system/issues)
- Related ADRs: [ADR-004](adr-004-ci-cd-with-build-once-deploy-many-and-manual-production-approval.md) · [ADR-005](adr-005-hosting-topology-and-budget.md) · [ADR-011](adr-011-secrets-management-with-infisical.md)
- Tickets implementing this decision: [#4](https://github.com/Kiilse/daily-system/issues/4) · [#10](https://github.com/Kiilse/daily-system/issues/10)
