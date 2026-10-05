---
tags:
  - adr
  - architecture
adr: 010
status: Accepted
date: 2026-09-25
related: "[Ecosystem SaaS - Technical Specification](../specs/technical-specification.md)"
---

# ADR-010: Field-level envelope encryption with Azure Key Vault

## Status

Accepted (2026-09-25)

## Context

User data must be encrypted in the database. The threat model covers a stolen disk or backup and a leaked database dump, not protection against the administrator. FinTrack must still compute sums by tag in SQL. If the encryption key sits in a Railway variable next to the database password, one Railway compromise gives both.

## Decision

- **At rest**: rely on the host's volume encryption (to verify) for everything.
- **Application-level encryption** of C3 fields (see data classification in the technical spec) with **AES-256-GCM**.
- **Envelope encryption**: one data key (DEK) per user per product, stored wrapped in the product database; the master key (KEK) lives in **Azure Key Vault** (one vault per environment) and never leaves it.
- Unwrapped DEKs cached in memory a few minutes.
- **Blind index** (HMAC-SHA256) for fields searched by equality (email).
- **Crypto-shredding** for account deletion: deleting a user's DEK makes their data unreadable.
- Amounts, dates, tags stay in clear text (C1) so SQL can filter and sum; they are protected by access control and RLS.

## Alternatives Considered

| Option | Why not |
|---|---|
| Key in a Railway environment variable | The key and the data would be compromised together |
| Encrypt everything, including amounts | Sums, sorting and filtering would have to happen in memory, which is slow and complex |
| PostgreSQL `pgcrypto` | Keys would travel to the database in queries and may appear in logs |
| End-to-end encryption | Protects against the admin too, but breaks server-side features and sharing |
| AWS KMS | Equivalent; Azure chosen for proximity with the .NET ecosystem |

## Consequences

**Positive**
- A database dump alone reveals no C3 data.
- GDPR erasure is simple, and KEK rotation doesn't require re-encrypting data.

**Negative**
- Encrypted fields can't be searched with `LIKE` or sorted.
- External dependency: if Key Vault is unreachable and the cache is empty, encrypted data can't be read (readiness check fails).
- The Azure credential used by `core` is itself a secret (stored in Infisical); an attacker needs both runtime secrets and Azure access.

## Key Concepts

- **AES-256-GCM**: symmetric encryption that also detects tampering (authenticated encryption).
- **Envelope encryption**: data encrypted with a data key, data key encrypted with a master key.
- **Blind index**: a keyed hash stored next to encrypted data, allowing exact-match search without decryption.
- **Crypto-shredding**: deleting data by destroying the only key that can decrypt it.

## Related

- Specs: [Ecosystem SaaS - Technical Specification](../specs/technical-specification.md) · [Ecosystem SaaS - Functional Specification](../specs/functional-specification.md)
- Backlog: [daily-system - Backlog](https://github.com/Kiilse/daily-system/issues)
- Related ADRs: [ADR-006](adr-006-postgresql-with-one-database-per-product.md) · [ADR-008](adr-008-managed-identity-provider-auth0.md) · [ADR-011](adr-011-secrets-management-with-infisical.md) · [ADR-012](adr-012-data-isolation-with-query-filters-and-postgresql-rls.md)
- Tickets implementing this decision: [#9](https://github.com/Kiilse/daily-system/issues/9) · [#14](https://github.com/Kiilse/daily-system/issues/14) · [#18](https://github.com/Kiilse/daily-system/issues/18) · [#19](https://github.com/Kiilse/daily-system/issues/19) · [#20](https://github.com/Kiilse/daily-system/issues/20) · [#26](https://github.com/Kiilse/daily-system/issues/26) · [#33](https://github.com/Kiilse/daily-system/issues/33) · [#37](https://github.com/Kiilse/daily-system/issues/37) · [#44](https://github.com/Kiilse/daily-system/issues/44)
