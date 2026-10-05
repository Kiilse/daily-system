---
tags:
  - adr
  - architecture
adr: 009
status: Accepted
date: 2026-09-25
related: "[Ecosystem SaaS - Technical Specification](../specs/technical-specification.md)"
---

# ADR-009: BFF pattern with ASP.NET Core and YARP

## Status

Accepted (2026-09-25)

## Context

No credential may be reachable from the frontend. A single-page application that stores tokens in the browser is exposed to token theft through XSS. The functional spec plans native mobile apps in v2.

## Decision

- `gateway-bff` (.NET 10) is the only backend reachable from outside, and only through the Cloudflare Worker.
- **Single origin**: the Worker serving the SPA proxies `/bff/*` and `/api/*` to the BFF. The browser sees one origin, so the session cookie is first-party whatever the domain (`*.workers.dev` today, a custom domain later), and no CORS is needed.
- The Worker adds a secret header to proxied requests (secret stored in Infisical, synced to Cloudflare and Railway). The BFF rejects requests without it.
- It performs the OIDC login with Auth0 and keeps tokens server-side, in an encrypted cookie `__Host-session` (`HttpOnly`, `Secure`, `SameSite=Strict`).
- It forwards API calls to `core` with **YARP**, attaching the access token.
- CSRF: every API call must carry the header `X-CSRF: 1`. A page on another site cannot add a custom header without a CORS preflight, and the BFF never approves cross-origin requests.
- Data Protection keys persisted in PostgreSQL and protected with Azure Key Vault.
- Built with ASP.NET Core's own cookie and OpenID Connect handlers (no Duende BFF license needed).
- `core` still validates the JWT (issuer, audience, expiry) on every request.
- **Mobile (v2)**: mobile apps will use OIDC with PKCE directly and call the API with bearer tokens; `core` already accepts them.

## Alternatives Considered

| Option | Why not |
|---|---|
| Tokens in the SPA (localStorage or memory) | Any XSS can read or use them |
| Duende BFF library | Good, but licensed for commercial use above a revenue threshold; the built-in handlers are enough |
| SPA on `*.workers.dev` calling the BFF on `*.up.railway.app` directly | Two different sites: the session cookie becomes third-party and browsers block it. Would only work after buying a domain |

## Consequences

**Positive**
- No token ever reaches JavaScript.
- A single public entry point for rate limiting, CORS and headers.

**Negative**
- One more service to run and pay for.
- Cookie-based auth needs a proper CSRF defense (handled by `SameSite` + custom header).
- Every API call makes one extra hop through Cloudflare (a few milliseconds).

## Key Concepts

- **BFF (Backend For Frontend)**: a small server dedicated to one frontend, which handles security on its behalf.
- **HttpOnly cookie**: a cookie JavaScript cannot read.
- **SameSite=Strict**: the browser only sends the cookie on requests coming from the same site.
- **CSRF**: an attack where another site makes the user's browser send a request with their cookie.
- **YARP**: Microsoft's reverse proxy library for .NET.
- **Same site vs same origin**: an origin is scheme + host + port. A site is the registrable domain (`example.com`); `app.example.com` and `api.example.com` are the same site. Suffixes like `workers.dev` and `up.railway.app` are on the Public Suffix List, so each subdomain there is its own site.

## Related

- Specs: [Ecosystem SaaS - Technical Specification](../specs/technical-specification.md) · [Ecosystem SaaS - Functional Specification](../specs/functional-specification.md)
- Backlog: [daily-system - Backlog](https://github.com/Kiilse/daily-system/issues)
- Related ADRs: [ADR-005](adr-005-hosting-topology-and-budget.md) · [ADR-008](adr-008-managed-identity-provider-auth0.md) · [ADR-015](adr-015-angular-spa-single-frontend.md)
- Tickets implementing this decision: [#10](https://github.com/Kiilse/daily-system/issues/10) · [#14](https://github.com/Kiilse/daily-system/issues/14) · [#15](https://github.com/Kiilse/daily-system/issues/15) · [#16](https://github.com/Kiilse/daily-system/issues/16) · [#17](https://github.com/Kiilse/daily-system/issues/17) · [#45](https://github.com/Kiilse/daily-system/issues/45)
