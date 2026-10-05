---
tags:
  - adr
  - architecture
adr: 006
status: Accepted
date: 2026-09-25
related: "[Ecosystem SaaS - Technical Specification](../specs/technical-specification.md)"
---

# ADR-006: PostgreSQL with one database per product

## Status

Accepted (2026-09-25)

## Context

Primary expertise is SQL Server. Railway offers PostgreSQL as a managed template. SQL Server needs about 2 GB of RAM per instance, around $20/month on Railway alone. The functional spec requires data to stay specific to each product.

## Decision

- **PostgreSQL** through **EF Core + Npgsql**.
- **One instance per environment, one database per product** (`account_db`, `menu_db`, `calendar_db`, later `fintrack_db` on its own instance if budget allows, otherwise its own database).
- Per database: a **migration role** (DDL) and a **runtime role** (DML only, not owner of tables).
- Private network only in prod. Daily + weekly backups.
- Types: `uuid` v7 keys (time-ordered), `timestamptz` for instants, `date` for calendar days, `numeric` for money.

## Alternatives Considered

| Option | Why not |
|---|---|
| SQL Server | Over budget on RAM, and the free Express edition is limited |
| One shared database with schemas per product | Cheaper by nothing, and weaker isolation (a single role mistake exposes everything) |
| One instance per product | Stronger isolation, but multiplies the most expensive always-on service |

## Consequences

**Positive**
- Low cost, strong ecosystem, RLS available for FinTrack.
- EF Core code stays close to what you know with SQL Server.

**Negative**
- Learning curve on PostgreSQL specifics (roles, RLS, `timestamptz`).
- One instance failure affects all co-hosted products.

## Key Concepts

- **Least privilege**: each component gets only the rights it needs. The running app cannot drop a table, because it doesn't need to.
- **UUID v7**: identifiers that contain a timestamp, so they sort by creation time, which keeps indexes efficient compared to random UUID v4.
- **`timestamptz`**: a PostgreSQL timestamp stored in UTC and converted on read, which avoids timezone bugs.

## Related

- Specs: [Ecosystem SaaS - Technical Specification](../specs/technical-specification.md) · [Ecosystem SaaS - Functional Specification](../specs/functional-specification.md)
- Backlog: [daily-system - Backlog](https://github.com/Kiilse/daily-system/issues)
- Related ADRs: [ADR-005](adr-005-hosting-topology-and-budget.md) · [ADR-010](adr-010-field-level-envelope-encryption-with-azure-key-vault.md) · [ADR-012](adr-012-data-isolation-with-query-filters-and-postgresql-rls.md)
- Tickets implementing this decision: [#4](https://github.com/Kiilse/daily-system/issues/4) · [#23](https://github.com/Kiilse/daily-system/issues/23)
