---
tags:
  - adr
  - architecture
adr: 019
status: Accepted
date: 2026-09-25
related: "[Ecosystem SaaS - Technical Specification](../specs/technical-specification.md)"
---

# ADR-019: Observability with OpenTelemetry Grafana Cloud and Sentry

## Status

Accepted (2026-09-25)

## Context

Railway logs alone are not enough to debug a distributed flow (browser, BFF, core, database) or to be alerted on errors.

## Decision

- **OpenTelemetry** SDK in `gateway-bff` and `core` (traces, metrics, logs), exported via OTLP to **Grafana Cloud** (free tier).
- **Sentry** (free tier) for exceptions in backend and Angular, source maps uploaded in CI and not served publicly.
- Correlation ID propagated from the BFF.
- Health checks `/health/live` and `/health/ready`.
- No C2/C3 data in any telemetry.

## Alternatives Considered

| Option | Why not |
|---|---|
| Railway logs only | No traces, no alerting, short retention |
| Self-hosted Grafana stack | Uses paid RAM |
| Datadog / New Relic | Free tiers more limited or paid quickly |

## Consequences

**Positive**
- A slow request can be followed from the browser to the SQL query.
- Vendor-neutral: OpenTelemetry can export elsewhere later.

**Negative**
- Telemetry export keeps dev services awake; export interval tuned on dev.
- Free tier quotas to watch.

## Key Concepts

- **OpenTelemetry**: an open standard and SDK to produce traces, metrics and logs, independent from the tool that stores them.
- **Trace**: the full path of one request across services, made of spans.

## Related

- Specs: [Ecosystem SaaS - Technical Specification](../specs/technical-specification.md) · [Ecosystem SaaS - Functional Specification](../specs/functional-specification.md)
- Backlog: [daily-system - Backlog](https://github.com/Kiilse/daily-system/issues)
- Related ADRs: [ADR-005](adr-005-hosting-topology-and-budget.md)
- Tickets implementing this decision: [#12](https://github.com/Kiilse/daily-system/issues/12)
