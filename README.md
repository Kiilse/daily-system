# daily-system

An ecosystem of four interconnected SaaS products: a menu and grocery manager, a shareable calendar, FinTrack (personal finance and budgeting), and a bento-box hub that aggregates modules from the other three. Each product works on its own, and the value comes from the connections the user explicitly turns on. The backend is a .NET modular monolith behind an ASP.NET Core BFF, and the frontend is a single Angular SPA.

## Documentation

- [Functional specification](docs/specs/functional-specification.md)
- [Technical specification](docs/specs/technical-specification.md)
- [Architecture Decision Records](docs/README.md)

`docs/adr` and `docs/specs` are mirrored from the author's Obsidian vault with `tools/sync-vault-docs.py`; don't edit them by hand.

## Repository layout

| Path | Content |
|---|---|
| `src/backend` | .NET solution: BFF, core host, modules, building blocks, tests |
| `src/frontend` | Angular workspace: the SPA and its libraries |
| `infra` | Docker Compose for local development, Railway configuration |
| `docs` | ADRs and specifications |
| `tools` | Developer and maintenance scripts |

## Contributing

Work follows GitHub Flow ([ADR-003](docs/adr/adr-003-github-flow-with-short-lived-feature-branches-and-feature-flags.md)): one issue, one branch, one pull request, squash merged into a protected `main`.

After cloning, install [gitleaks](https://github.com/gitleaks/gitleaks#installing), then run:

```bash
tools/setup-dev.sh
```

It enables the pre-commit hook that blocks any commit containing a secret. To report a vulnerability, see [SECURITY.md](SECURITY.md).

## License

No open-source license. The code is public so it can be read, but all rights are reserved: copying, redistributing or reusing it requires the author's written permission.
