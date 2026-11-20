# Account setup: Infisical and Azure (ticket #9)

One-time setup on your Windows machine, before running `tools/bootstrap/*.sh`. About 30 minutes. Every command runs in **Git Bash**.

## 0. Tools

```bash
npm install -g @infisical/cli          # Infisical CLI (Node is already installed for the frontend)
winget install -e --id Microsoft.AzureCLI
winget install -e --id Gitleaks.Gitleaks   # already there if you set up the pre-commit hook (#1)
```

Close and reopen Git Bash, then check:

```bash
infisical --version && az version --query '"azure-cli"' -o tsv && gitleaks version && openssl version
```

## 1. Infisical (EU cloud)

1. Sign up at **https://eu.infisical.com** (not app.infisical.com: that is the US region, data would leave the EU).
2. **MFA first**: avatar > Personal settings > Security > enable two-factor authentication.
3. Create an organization (any name, e.g. `kiilse`).
4. Create a project: **Secrets Management**, name `daily-system`.
5. Project settings > Environments:
	- keep `Development` (slug **`dev`**) and `Production` (slug **`prod`**)
	- delete `Staging` (the free plan allows 3 environments, we use 2, ADR-018)
6. Project settings > General: copy the **Project ID**.
7. Log the CLI in:

```bash
infisical login --domain https://eu.infisical.com   # opens the browser
export INFISICAL_PROJECT_ID=<the project ID>
```

Not now: the machine identity for GitHub Actions and the Railway sync come with tickets #10 and #11.

## 2. Azure

1. Create the account at **https://azure.microsoft.com/pricing/purchase-options/pay-as-you-go**. A card is required even if the bill stays at a few cents. Prefer pay-as-you-go to the free trial: when the trial ends, its resources are disabled until you upgrade, and Key Vault would stop working.
2. **MFA**: https://account.microsoft.com > Security > Advanced security options > two-step verification.
3. Log the CLI in and pick the subscription:

```bash
az login                                   # browser
az account list -o table
az account set --subscription "<subscription name>"
```

4. **Budget alert** (portal, 2 minutes): Cost Management > Budgets > Add: scope = the subscription, monthly, **2 EUR**, alerts at 80 % and 100 % to your email.

You are Owner of your own subscription, so you can create app registrations and role assignments: nothing else to configure.

## 3. Run the bootstrap

```bash
cd <repo>
./tools/bootstrap/1-infisical-secrets.sh dev
./tools/bootstrap/2-azure-keyvault.sh dev          # if the vault name is taken: VAULT_SUFFIX=vc ./tools/...
./tools/bootstrap/3-verify.sh dev

./tools/bootstrap/1-infisical-secrets.sh prod
./tools/bootstrap/2-azure-keyvault.sh prod
OTHER_VAULT=kv-dailysys-dev ./tools/bootstrap/3-verify.sh prod
```

`3-verify.sh` must end with `0 failed` for both environments. Check in the Infisical UI that `/core`, `/bff`, `/worker` and `/ci` exist in `dev` and `prod`, and that you never saw a secret value in the terminal.

## Troubleshooting

| Symptom | Cause | Fix |
|---|---|---|
| `VaultAlreadyExists` / name not available | Key Vault names are global | `VAULT_SUFFIX=<2-3 letters>` and re-run, the same suffix for every later run |
| `Could not create data-kek (role not applied yet?)` | Azure role assignments take a few minutes | wait 2 minutes, re-run step 2 |
| `MissingSubscriptionRegistration` for Microsoft.KeyVault | new subscription | step 2 registers it; if it still fails: `az provider register --namespace Microsoft.KeyVault --wait` |
| `infisical: not logged in` or 403 | CLI logged in to the US region, or wrong project ID | `infisical login --domain https://eu.infisical.com`, check `INFISICAL_PROJECT_ID` |
| Paths like `C:/Program Files/Git/core` in errors | Git Bash path conversion | the scripts set `MSYS_NO_PATHCONV=1`; run them with `./tools/...`, not through PowerShell |
| `3-verify.sh` FAIL (unexpected error) right after step 2 | role assignment not applied yet | wait 5 minutes, re-run step 3 |
