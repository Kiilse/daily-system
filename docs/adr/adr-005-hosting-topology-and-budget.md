---
tags:
  - adr
  - architecture
adr: 005
status: Accepted
date: 2026-09-25
related: "[Ecosystem SaaS - Technical Specification](../specs/technical-specification.md)"
---

# ADR-005: Hosting topology and budget

## Status

Accepted (2026-09-25)

## Context

Budget: 20 €/month. Backend and databases must be on Railway, in the EU. Two environments (dev, prod). The frontend host was undecided; Cloudflare was preferred.

## Decision

- **Railway, EU West (Amsterdam)**, one Railway project with two environments (`dev`, `prod`). Per environment: `gateway-bff` (public), `core` (private network only), PostgreSQL (private, volume with daily and weekly backups).
- **Serverless (sleep when idle)** enabled on dev application services.
- **Frontend on Cloudflare Workers with static assets** in SPA mode (`not_found_handling: single-page-application`), security headers via `_headers`. One Worker per environment.
- The same Worker **proxies `/bff/*` and `/api/*` to the Railway BFF**, so the SPA and the API share one origin (see [ADR-009 BFF pattern with ASP.NET Core and YARP](adr-009-bff-pattern-with-asp-net-core-and-yarp.md)). This keeps the architecture independent of the domain name, which is not chosen yet.
- Containers use `mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled` (non-root, no shell), pinned by digest, Workstation GC to limit RAM.
- Estimated total: ~$13 to 17/month (see technical spec), to verify after one week.

## Alternatives Considered

| Option | Why not |
|---|---|
| Cloudflare Pages | Still works, but Workers static assets is where Cloudflare adds new features, and it offers the same SPA mode and headers |
| Frontend on Railway | Uses paid RAM for serving static files that a CDN serves for free |
| Vercel / Netlify | Fine, but no advantage over Cloudflare, and Cloudflare also holds DNS and registrar |
| Other backend hosts (Fly.io, Render, Azure Container Apps) | Railway was chosen upfront; revisit only if the budget does not hold |

## Consequences

**Positive**
- Fits the budget with margin. Only one backend service is exposed to the internet.

**Negative**
- Dev services have a cold start after sleeping (first request may return 502).
- API calls go through the Worker script, which counts against the Workers free plan request limit (static assets don't). The paid plan (~$5/month) still fits the budget if needed.
- Railway has a single EU region (Amsterdam); no multi-region failover in budget.
- Encryption at rest of Railway volumes must be verified.

## Key Concepts

- **CDN**: a network of servers that caches static files close to users.
- **Chiseled image**: a minimal Ubuntu-based container image from Microsoft, without shell or package manager, which reduces what an attacker can do inside a compromised container.
- **Server GC vs Workstation GC**: two .NET garbage collector modes. Server GC favors throughput and uses more memory; Workstation GC is lighter, better for small containers.

## Related

- Specs: [Ecosystem SaaS - Technical Specification](../specs/technical-specification.md) · [Ecosystem SaaS - Functional Specification](../specs/functional-specification.md)
- Backlog: [daily-system - Backlog](https://github.com/Kiilse/daily-system/issues)
- Related ADRs: [ADR-006](adr-006-postgresql-with-one-database-per-product.md) · [ADR-009](adr-009-bff-pattern-with-asp-net-core-and-yarp.md) · [ADR-018](adr-018-environments-and-configuration.md) · [ADR-019](adr-019-observability-with-opentelemetry-grafana-cloud-and-sentry.md)
- Tickets implementing this decision: [#4](https://github.com/Kiilse/daily-system/issues/4) · [#10](https://github.com/Kiilse/daily-system/issues/10) · [#11](https://github.com/Kiilse/daily-system/issues/11) · [#45](https://github.com/Kiilse/daily-system/issues/45)
