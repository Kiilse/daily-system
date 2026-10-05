---
tags:
  - adr
  - architecture
adr: 015
status: Accepted
date: 2026-09-25
related: "[Ecosystem SaaS - Technical Specification](../specs/technical-specification.md)"
---

# ADR-015: Angular SPA single frontend

## Status

Accepted (2026-09-25)

## Context

Frontend in TypeScript. Angular chosen for the job market. The apps are behind a login and don't need search engine indexing. One frontend in v1.

## Decision

- **Angular 22**, standalone components, signals, zoneless change detection, strict TypeScript.
- **SPA** (no server-side rendering).
- **One Angular application** with one library per product (`feature-menu`, `feature-calendar`, ...), plus `core`, `ui` and `api-clients`. Libraries respect the same boundaries as backend modules.
- Vitest for unit tests (Angular default), Playwright for E2E.
- Angular `autoCsp` for a strict Content Security Policy.

## Alternatives Considered

| Option | Why not |
|---|---|
| Server-side rendering (Angular SSR) | Useful for SEO and first paint of public pages; not needed behind a login, and it needs a Node server to host and pay for |
| One frontend per product | More builds and deployments; can be done later since features are already in separate libraries |
| React | More offers overall, but Angular fits the targeted offers and .NET-like structure |

## Consequences

**Positive**
- Static hosting on a free CDN. Structure familiar to a C# developer (DI, strong typing).

**Negative**
- A public marketing page, if needed later, should be a separate static site.

## Key Concepts

- **SPA (single-page application)**: the browser downloads the app once, then JavaScript renders pages and calls the API. The server only serves files.
- **SSR (server-side rendering)**: the server renders HTML for each page; better for SEO, but needs a running server.
- **Signals**: Angular's reactive primitive that tells the framework exactly what changed.
- **Zoneless**: change detection triggered by signals instead of patching every browser event with Zone.js.

## Related

- Specs: [Ecosystem SaaS - Technical Specification](../specs/technical-specification.md) · [Ecosystem SaaS - Functional Specification](../specs/functional-specification.md)
- Backlog: [daily-system - Backlog](https://github.com/Kiilse/daily-system/issues)
- Related ADRs: [ADR-009](adr-009-bff-pattern-with-asp-net-core-and-yarp.md) · [ADR-014](adr-014-openapi-contract-and-generated-typescript-client.md)
- Tickets implementing this decision: [#5](https://github.com/Kiilse/daily-system/issues/5) · [#17](https://github.com/Kiilse/daily-system/issues/17)
