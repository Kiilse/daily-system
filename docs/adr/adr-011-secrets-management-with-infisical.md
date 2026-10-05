---
tags:
  - adr
  - architecture
adr: 011
status: Accepted
date: 2026-09-25
related: "[Ecosystem SaaS - Technical Specification](../specs/technical-specification.md)"
---

# ADR-011: Secrets management with Infisical

## Status

Accepted (2026-09-25)

## Context

Secrets are needed in Railway (runtime), GitHub Actions (CI/CD) and locally. Pasting them by hand in several places leads to drift and leaks. A dedicated manager was requested.

## Decision

- **Infisical Cloud (EU region)**, free plan, one project with `dev` and `prod` environments.
- Infisical **syncs** secrets to Railway variables and GitHub Actions secrets. Locally, `infisical run -- dotnet run` injects them.
- The repository only contains `.env.example` files with empty values.
- Secrets rotated at least once a year and immediately after any suspected leak.

## Alternatives Considered

| Option | Why not |
|---|---|
| Railway variables + GitHub secrets by hand | Duplication, no single audit point |
| Doppler | Comparable; Infisical is open source and has an EU cloud |
| Azure Key Vault for secrets too | Possible, but syncing to Railway and GitHub is less direct |

## Consequences

**Positive**
- One source of truth, no secret in Git.

**Negative**
- Free plan: 5 identities, 3 environments, no secret rotation automation, no audit log retention.
- One more account to protect (MFA mandatory on it).

## Key Concepts

- **Secret sync**: the manager pushes secrets to the platforms that need them, so you edit them in one place only.

## Related

- Specs: [Ecosystem SaaS - Technical Specification](../specs/technical-specification.md) · [Ecosystem SaaS - Functional Specification](../specs/functional-specification.md)
- Backlog: [daily-system - Backlog](https://github.com/Kiilse/daily-system/issues)
- Related ADRs: [ADR-010](adr-010-field-level-envelope-encryption-with-azure-key-vault.md) · [ADR-016](adr-016-security-scanning-pipeline.md) · [ADR-018](adr-018-environments-and-configuration.md)
- Tickets implementing this decision: [#9](https://github.com/Kiilse/daily-system/issues/9)
