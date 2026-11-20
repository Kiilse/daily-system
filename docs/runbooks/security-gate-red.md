# Security gate red on a PR that changes no dependency

The `images`, `npm-audit` and `dotnet-vulnerable` checks scan everything on the branch, not only the diff ([ADR-016](../adr/adr-016-security-scanning-pipeline.md)). When they fail on a PR that does not touch dependencies, `main` itself is vulnerable. Every PR stays blocked until `main` is fixed.

## 1. Confirm it comes from main

- Open the failing job log and note the package, the installed version and the fixed version.
- Run the security workflow on `main` (Actions, `security`, Run workflow). If it fails the same way, the cause is on `main`.

## 2. Fix on main first, in its own PR

| Failing check | Usual cause | Fix |
|---|---|---|
| `images` | .NET runtime in the base image has a fixed CVE | Update the `sdk` and `aspnet` digests in **every** Dockerfile in one commit. Read `DOTNET_VERSION` and `ASPNET_VERSION` from `docker image inspect` to check the patch level. Align `src/backend/global.json` with `DOTNET_SDK_VERSION` |
| `npm-audit` | A production npm dependency has a fixed advisory | `npm audit --omit=dev` names the package. Update the direct dependency that pins it (`npm ls PACKAGE` shows which one), not an `overrides` entry, unless no upstream release exists yet |
| `dotnet-vulnerable` | A NuGet package has a fixed advisory | Update it in `Directory.Packages.props` and refresh the lock files (`dotnet restore --force-evaluate`) |

Never add a CVE that has a fix to `.trivyignore`, `.gitleaksignore` or an audit exception to get green.

## 3. Then unblock the other PRs

- Rebase open PRs on the fixed `main` (for Dependabot: comment `@dependabot rebase`).
- Close Dependabot PRs made obsolete by the fix.

## 4. Transient failures

A `curl` error (`504`, `connection reset`) while installing Gitleaks or Trivy is not a finding. Downloads already retry 3 times; if it still fails, re-run the job.
