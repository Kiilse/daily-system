# Secrets and encryption keys

Where every secret lives, who uses it and how to rotate it ([ADR-010](../adr/adr-010-field-level-envelope-encryption-with-azure-key-vault.md), [ADR-011](../adr/adr-011-secrets-management-with-infisical.md)).

**Rule**: Infisical (EU cloud) is the only place where a secret is written by hand. Railway, GitHub Actions and Cloudflare receive copies through sync or the deploy pipeline. Nothing secret is ever committed, printed in CI logs or pasted in a chat.

## Inventory

| Secret | Infisical folder | Used by | Created by | Rotation |
|---|---|---|---|---|
| `BLIND_INDEX_ROOT_KEY` | `/core` | core (HKDF root of blind indexes, #18) | `tools/bootstrap/1-infisical-secrets.sh` | **Never** without a re-index migration: changing it breaks every lookup by email |
| `EDGE_SHARED_SECRET` | `/bff` and `/worker` (same value) | Cloudflare Worker adds it, BFF checks it (#15) | `1-infisical-secrets.sh` | Yearly or on suspicion: update both folders, then redeploy Worker and BFF |
| `AZURE_TENANT_ID`, `AZURE_CLIENT_ID`, `AZURE_CLIENT_SECRET` | `/core` | core, to wrap and unwrap user data keys with `data-kek` (#19) | `tools/bootstrap/2-azure-keyvault.sh` | Client secret expires after 1 year: re-run step 2 (it resets the credential) |
| `AZURE_TENANT_ID`, `AZURE_CLIENT_ID`, `AZURE_CLIENT_SECRET` | `/bff` | gateway-bff, to protect Data Protection keys with `dp-kek` (#14) | `2-azure-keyvault.sh` | Same as above |
| `KEY_VAULT_URI`, `KEY_VAULT_KEY_NAME` | `/core`, `/bff` | same services | `2-azure-keyvault.sh` | Not secret, kept next to the credentials for convenience |
| Auth0 domain and client secret | `/bff` | gateway-bff (#13, #14) | ticket #13 | On suspicion; Auth0 allows two secrets during rotation |
| Railway and Cloudflare deploy tokens | `/ci` | deploy pipeline (#11) | ticket #10 and #11 | Yearly |
| Database connection strings | `/core`, `/bff`, `/ci` (migrator) | services and migrator (#10) | ticket #10 | On suspicion |
| Grafana Cloud and Sentry tokens | `/core`, `/bff`, `/ci` | telemetry (#12) | ticket #12 | Yearly, write-only tokens |

## Azure Key Vault (one vault per environment)

| | dev | prod |
|---|---|---|
| Resource group | `rg-daily-system-dev` | `rg-daily-system-prod` |
| Vault | `kv-dailysys-dev` | `kv-dailysys-prod` |
| Keys | `data-kek`, `dp-kek` (RSA 2048, wrap/unwrap only) | same |
| Identities | `daily-system-core-dev` on `data-kek` only, `daily-system-bff-dev` on `dp-kek` only | `-prod` equivalents |

- Role of the identities: **Key Vault Crypto Service Encryption User**, scoped to **one key**: read key metadata, wrap, unwrap. No decrypt of arbitrary data, no key creation or deletion.
- Purge protection is on: a deleted key stays recoverable 90 days. It protects encrypted data against an attacker or a mistake; it also means a vault name stays reserved 90 days after deletion.
- **KEK rotation**: create a new key version in Key Vault, then run the re-wrap command of #19 (`RewrapAllAsync`). Data is not re-encrypted, only the user data keys are re-wrapped.

## Bootstrap (ticket #9)

Run from Git Bash at the repository root, once per environment:

```bash
infisical login --domain https://eu.infisical.com
az login
export INFISICAL_PROJECT_ID=...   # Infisical, project settings
./tools/bootstrap/1-infisical-secrets.sh dev
./tools/bootstrap/2-azure-keyvault.sh dev        # VAULT_SUFFIX=xyz if the vault name is taken
./tools/bootstrap/3-verify.sh dev                # OTHER_VAULT=kv-dailysys-prod once prod exists
```

The scripts are idempotent: re-running them keeps existing values (except `2-azure-keyvault.sh`, which issues a new client secret).

## If a secret leaks

1. Rotate it first (table above), then investigate.
2. If it was committed: rotating is mandatory, rewriting Git history is not enough (the repository is public).
3. Note the incident and the date in this file.
