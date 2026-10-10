---
tags:
  - adr
  - architecture
adr: 016
status: Accepted
date: 2026-09-25
related: "[Ecosystem SaaS - Technical Specification](../specs/technical-specification.md)"
---

# ADR-016: Security scanning pipeline

## Status

Accepted (2026-09-25), amended 2026-10-10 (ticket #8: npm audit scope, Trivy pinning, CodeQL merge gate)

## Context

No credential may be reachable through the frontend or the APIs. The application must be as secure as possible, with a single developer and no human reviewer.

## Decision

On every PR (blocking):
- **Gitleaks** on the diff (also as a local pre-commit hook). GitHub push protection enabled.
- **Repository is public** (decided 2026-09-25): CodeQL code scanning and GitHub secret scanning with push protection are free.
- **SAST**: CodeQL for C# and TypeScript (`security-extended`). Roslyn security analyzers and `TreatWarningsAsErrors`. A `code_scanning` rule in the main ruleset blocks the merge on a high or critical CodeQL alert: without it, an alert only shows in the Security tab.
- **Dependencies**: GitHub dependency review, `dotnet list package --vulnerable --include-transitive`, `npm audit --omit=dev --audit-level=high`, Dependabot. `npm audit` has no ignore file, and on 2026-10-10 it reported high and critical findings with no real fix in dev-only tooling (`braces` and `handlebars` under `eslint-plugin-boundaries`, never shipped in the bundle). It therefore audits production dependencies only; a new dev dependency is still checked by dependency review.
- **Containers**: Trivy on built images, failing on HIGH/CRITICAL with an available fix. Trivy is installed as a binary pinned by checksum, not through `aquasecurity/trivy-action`, whose tags were rewritten in the March 2026 supply chain attack (CVE-2026-33634). Every third-party action is pinned by commit SHA.
- **Bundle scan**: Gitleaks and custom patterns on the built Angular `dist/`.

Nightly:
- **DAST**: OWASP ZAP baseline scan against dev.
- Full dependency and image audit on what runs in prod.

## Alternatives Considered

| Option | Why not |
|---|---|
| Paid platforms (Snyk, GitHub Code Security on private repo) | Over budget; free tools cover the need |
| Scans only before release | Issues found late are more expensive to fix |

## Consequences

**Positive**
- Leaks, known vulnerabilities and common code flaws are caught before merge.

**Negative**
- False positives will need triage and documented suppressions.
- CI time increases (mitigated by running jobs in parallel).
- Public repository: a committed secret is exposed instantly and scraped by bots. The pre-commit hook is not optional, and a leaked secret is always rotated, never just removed from Git history.

## Key Concepts

- **SAST**: static analysis, reads the source code to find vulnerable patterns.
- **DAST**: dynamic analysis, attacks the running application from outside.
- **SCA**: software composition analysis, checks dependencies against known vulnerability databases.

## Related

- Specs: [Ecosystem SaaS - Technical Specification](../specs/technical-specification.md) · [Ecosystem SaaS - Functional Specification](../specs/functional-specification.md)
- Backlog: [daily-system - Backlog](https://github.com/Kiilse/daily-system/issues)
- Related ADRs: [ADR-004](adr-004-ci-cd-with-build-once-deploy-many-and-manual-production-approval.md) · [ADR-011](adr-011-secrets-management-with-infisical.md) · [ADR-017](adr-017-testing-strategy.md)
- Tickets implementing this decision: [#1](https://github.com/Kiilse/daily-system/issues/1) · [#7](https://github.com/Kiilse/daily-system/issues/7) · [#8](https://github.com/Kiilse/daily-system/issues/8) · [#22](https://github.com/Kiilse/daily-system/issues/22)
