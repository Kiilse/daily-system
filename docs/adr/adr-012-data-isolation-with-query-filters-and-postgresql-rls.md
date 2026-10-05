---
tags:
  - adr
  - architecture
adr: 012
status: Accepted
date: 2026-09-25
related: "[Ecosystem SaaS - Technical Specification](../specs/technical-specification.md)"
---

# ADR-012: Data isolation with query filters and PostgreSQL RLS

## Status

Accepted (2026-09-25)

## Context

Multi-user application with sharing between users. A single forgotten `WHERE user_id = ...` would expose another user's data. FinTrack holds financial data.

## Decision

- **All modules**: EF Core global query filters on the owner ID, plus explicit authorization checks in handlers for shared data, plus integration tests that try to read another user's data.
- **FinTrack**: in addition, **PostgreSQL Row-Level Security** on every user-owned table. The app sets `app.current_user_id` at the start of each transaction; policies compare it to the row owner. The runtime role is not the table owner and RLS is `FORCE`d.
- Menu and Calendar may adopt RLS later if sharing rules grow.

## Alternatives Considered

| Option | Why not |
|---|---|
| Query filters only everywhere | A raw SQL query or a filter bypass (`IgnoreQueryFilters`) would leak data with nothing behind |
| RLS everywhere from v1 | More setup for modules whose data is less sensitive, in a two-week window |

## Consequences

**Positive**
- For FinTrack, the database itself refuses to return another user's rows, even with a bug in the code.

**Negative**
- RLS needs care with connection pooling (the setting must be scoped to the transaction with `SET LOCAL`).
- Slightly harder debugging.

## Key Concepts

- **Row-Level Security (RLS)**: PostgreSQL rules that filter rows per query according to a policy, applied by the database regardless of the query written.
- **Global query filter**: an EF Core filter automatically added to every query on an entity.

## Related

- Specs: [Ecosystem SaaS - Technical Specification](../specs/technical-specification.md) · [Ecosystem SaaS - Functional Specification](../specs/functional-specification.md)
- Backlog: [daily-system - Backlog](https://github.com/Kiilse/daily-system/issues)
- Related ADRs: [ADR-006](adr-006-postgresql-with-one-database-per-product.md) · [ADR-010](adr-010-field-level-envelope-encryption-with-azure-key-vault.md) · [ADR-020](adr-020-market-data-providers.md)
- Tickets implementing this decision: [#35](https://github.com/Kiilse/daily-system/issues/35) · [#36](https://github.com/Kiilse/daily-system/issues/36) · [#39](https://github.com/Kiilse/daily-system/issues/39)
