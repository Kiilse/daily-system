# Frontend

Angular SPA and its libraries ([ADR-015](../../docs/adr/adr-015-angular-spa-single-frontend.md)). Local commands: [local setup runbook](../../docs/runbooks/local-setup.md#frontend).

| Path | Import as | Role |
|---|---|---|
| `src/app` | | The application: shell, routes |
| `projects/core` | `@daily-system/core` | Auth state, HTTP interceptors, error handling |
| `projects/ui` | `@daily-system/ui` | Shared components |
| `projects/api-clients` | `@daily-system/api-clients` | Generated API client (#21) |
| `projects/feature-*` | `@daily-system/feature-*` | One library per product |

Libraries are never published: the `@daily-system/*` aliases in `tsconfig.json` point at their sources.

## Boundaries

Enforced by `npm run lint` (`boundaries/dependencies` in `eslint.config.js`), tested by `tools/boundaries.test.mjs`:

- `feature-*` may import `core`, `ui`, `api-clients`, never another `feature-*`
- `core` may import `api-clients` only, `ui` and `api-clients` import no other library
- the app may import everything

## CI

`.github/workflows/ci-frontend.yml` runs on every PR; its `frontend` job is required to merge into `main`. Same commands locally, from `src/frontend/`:

```bash
npm ci && npm run lint && npx tsc -b --noEmit && npm run test:coverage && node --test tools/*.test.mjs
npm run build -- --configuration production
../../tools/scan-bundle.test.sh && ../../tools/scan-bundle.sh dist/   # needs gitleaks on the PATH
```

The bundle scan fails on any gitleaks finding, private key, JWT, `client_secret`, bearer token, AWS or Azure key, and on any source map: everything in `dist/` ships to every visitor.
