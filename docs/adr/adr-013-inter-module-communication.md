---
tags:
  - adr
  - architecture
adr: 013
status: Accepted (v1), async part Proposed
date: 2026-09-25
related: "[Ecosystem SaaS - Technical Specification](../specs/technical-specification.md)"
---

# ADR-013: Inter-module communication

## Status

Accepted (v1), async part Proposed (2026-09-25)

## Context

Menu and Calendar exchange data both ways (diners per day, planned meals). Menu will push forecast expenses to FinTrack. The functional spec requires graceful degradation when a product doesn't answer. Chosen direction: synchronous first, asynchronous (RabbitMQ or similar) later.

## Decision

- **v1 (co-hosted modules)**: synchronous calls through `*.Contracts` interfaces, implemented in-process. Same request/response semantics as HTTP, without the network.
- **When a module is split** (FinTrack from its first release): the contract implementation becomes a typed HTTP client, with timeout, retry and circuit breaker (`Microsoft.Extensions.Http.Resilience`).
- **Later (Proposed)**: domain events published through the **Outbox pattern** to **RabbitMQ**, for flows that don't need an immediate answer (Menu forecast expense to FinTrack, notifications).
- Every consumer handles absence of data (default values, "not connected" state).

## Alternatives Considered

| Option | Why not |
|---|---|
| HTTP between co-hosted modules | Calling yourself over the network adds latency and failure modes for nothing |
| RabbitMQ from v1 | Another always-on service to pay and operate before any flow needs it |
| Shared database tables between modules | Breaks product separation |

## Consequences

**Positive**
- Simple and fast in v1, with a clear evolution path.

**Negative**
- Synchronous calls couple availability: if Calendar is slow, Menu is slow. Acceptable while co-hosted.

## Key Concepts

- **Outbox pattern**: the event is saved in an `outbox` table in the same database transaction as the business change, then a background job publishes it. No event is lost, and no event is sent for a change that was rolled back.
- **Circuit breaker**: after repeated failures, calls stop for a while instead of piling up on a failing service.

## Related

- Specs: [Ecosystem SaaS - Technical Specification](../specs/technical-specification.md) · [Ecosystem SaaS - Functional Specification](../specs/functional-specification.md)
- Backlog: [daily-system - Backlog](https://github.com/Kiilse/daily-system/issues)
- Related ADRs: [ADR-001](adr-001-modular-monolith-with-co-hosted-modules-in-v1.md) · [ADR-014](adr-014-openapi-contract-and-generated-typescript-client.md)
- Tickets implementing this decision: [#28](https://github.com/Kiilse/daily-system/issues/28) · [#29](https://github.com/Kiilse/daily-system/issues/29) · [#30](https://github.com/Kiilse/daily-system/issues/30) · [#35](https://github.com/Kiilse/daily-system/issues/35) · [#38](https://github.com/Kiilse/daily-system/issues/38) · [#42](https://github.com/Kiilse/daily-system/issues/42)
