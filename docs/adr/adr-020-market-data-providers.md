---
tags:
  - adr
  - architecture
adr: 020
status: Proposed (FinTrack phase)
date: 2026-09-25
related: "[Ecosystem SaaS - Technical Specification](../specs/technical-specification.md)"
---

# ADR-020: Market data providers

## Status

Proposed (FinTrack phase) (2026-09-25)

## Context

FinTrack needs exchange rates for fiat currencies, crypto and stocks. The functional spec says "real-time rates". The actual goal is complete tracking with charts (net worth over time, evolution per space and currency). Budget is 20 €/month, and commercialization is possible later.

## Decision

- **Fiat**: **Frankfurter** (no key, no quota, commercial use allowed, rates from central banks, updated about daily; self-hostable with Docker).
- **Crypto**: **CoinGecko Demo** plan (10,000 calls/month, 100 per minute, attribution required, **non-commercial**). A paid plan or another provider is required before commercialization.
- **Stocks**: deferred. Free tiers found so far (Twelve Data Basic: 800 credits/day, internal non-display use only) don't allow displaying data to users.
- All providers behind `IMarketDataProvider`, called only by scheduled jobs, never per user request.
- **Rate history**: one rate per currency pair per day in `exchange_rate_history`. Fiat history backfilled from Frankfurter.
- **Balance snapshots**: one row per space per day in `balance_snapshot` (native currency + user's reference currency). Charts read snapshots instead of recomputing history.
- Crypto current value refreshed frequently (e.g. every 15 minutes) for display; only the daily closing value is stored in history.
- Functional spec change: "real-time rates" becomes "daily rate history for fiat and crypto, frequent refresh of the current crypto value".

## Alternatives Considered

| Option | Why not |
|---|---|
| Real-time paid feeds | Cost with no real value for budget tracking |
| Scraping | Legally and technically fragile |

## Consequences

**Positive**
- Free for fiat, cheap to switch providers thanks to the port.

**Negative**
- Crypto display for a commercial product will have a cost.
- Stocks are not covered for now.
- Snapshot tables grow by one row per space per day: small, but to include in the retention policy.

## Key Concepts

- **Port and adapter**: the domain defines an interface (port) for what it needs; each provider is an adapter implementing it. Swapping providers doesn't touch business code.
- **Snapshot**: a stored copy of a computed value at a point in time. It trades a little storage for fast, stable charts: history doesn't change if a past transaction is recomputed differently later.

## Related

- Specs: [Ecosystem SaaS - Technical Specification](../specs/technical-specification.md) · [Ecosystem SaaS - Functional Specification](../specs/functional-specification.md)
- Backlog: [daily-system - Backlog](https://github.com/Kiilse/daily-system/issues)
- Related ADRs: [ADR-007](adr-007-internal-architecture-per-module.md) · [ADR-012](adr-012-data-isolation-with-query-filters-and-postgresql-rls.md)
- Tickets implementing this decision: [#41](https://github.com/Kiilse/daily-system/issues/41)
