---
tags:
  - adr
  - architecture
adr: 008
status: Accepted
date: 2026-09-25
related: "[Ecosystem SaaS - Technical Specification](../specs/technical-specification.md)"
---

# ADR-008: Managed identity provider Auth0

## Status

Accepted (2026-09-25)

## Context

One Global account authenticates users across the four products. The application must be as secure as possible, with MFA, EU data, and a managed service rather than a self-built one for v1.

## Decision

- **Auth0**, free plan, **one tenant per environment** (dev, prod), created in the **EU region**.
- OpenID Connect authorization code flow with PKCE, used only by the BFF.
- Tenant default domain until a domain name is bought, then custom domain `auth.<domain>` (configuration only).
- MFA with authenticator app (TOTP): **mandatory for the admin tier and when the user activates FinTrack**, strongly suggested for everyone else. Breached password detection and brute-force protection on for all.
- Roles and tiers are **not** stored in Auth0 (not available on the free plan) but in `account_db`.
- Only standard OIDC is used, so the provider can be swapped (Entra External ID, Zitadel, Keycloak) by changing configuration.

## Alternatives Considered

| Option | Why not |
|---|---|
| Microsoft Entra External ID (50,000 MAU free) | Serious alternative, relevant for a .NET CV. More complex setup; kept as plan B |
| Zitadel Cloud (100 DAU free) | Good security, EU region, but custom domain only on the $100/month plan |
| Keycloak self-hosted | Uses paid RAM, and you maintain a security-critical service yourself |
| Custom identity with OpenIddict | Maximum learning, maximum risk. Not for v1 with real data |

## Consequences

**Positive**
- Login, MFA, password reset, account protection handled by specialists.
- 25,000 MAU on the free plan.

**Negative**
- Free plan: 1-day log retention, basic MFA factors, community support.
- Vendor dependency, mitigated by using standard OIDC only.

## Key Concepts

- **OIDC (OpenID Connect)**: a standard layer on top of OAuth 2.0 that says who the user is (ID token) in addition to what the app may access (access token).
- **PKCE**: a protection for the code flow that prevents a stolen authorization code from being exchanged by someone else.
- **MAU / DAU**: monthly / daily active users, how identity providers count usage.

## Related

- Specs: [Ecosystem SaaS - Technical Specification](../specs/technical-specification.md) · [Ecosystem SaaS - Functional Specification](../specs/functional-specification.md)
- Backlog: [daily-system - Backlog](https://github.com/Kiilse/daily-system/issues)
- Related ADRs: [ADR-009](adr-009-bff-pattern-with-asp-net-core-and-yarp.md) · [ADR-010](adr-010-field-level-envelope-encryption-with-azure-key-vault.md)
- Tickets implementing this decision: [#13](https://github.com/Kiilse/daily-system/issues/13) · [#20](https://github.com/Kiilse/daily-system/issues/20) · [#44](https://github.com/Kiilse/daily-system/issues/44)
