#!/usr/bin/env bash
# Ticket #9, step 2: Key Vault, master keys and least-privilege identities for one environment.
# Creates: resource group, Key Vault (RBAC, purge protection), keys data-kek and dp-kek,
# one app registration for core (data-kek only) and one for bff (dp-kek only).
# Client secrets go straight to Infisical, never to the terminal.
# Prerequisites: `az login` on the subscription to use, step 1 done.
# Usage: INFISICAL_PROJECT_ID=... ./tools/bootstrap/2-azure-keyvault.sh dev
source "$(dirname "$0")/common.sh"
require az infisical

az account show -o none 2>/dev/null || az login -o none
TENANT_ID="$(az account show --query tenantId -o tsv)"
ME="$(az ad signed-in-user show --query id -o tsv)"
echo "== Subscription: $(az account show --query name -o tsv)"

echo "== Resource provider Microsoft.KeyVault (needed once on a new subscription)"
az provider register --namespace Microsoft.KeyVault --wait -o none

echo "== Resource group $RESOURCE_GROUP ($LOCATION)"
az group create --name "$RESOURCE_GROUP" --location "$LOCATION" -o none

echo "== Key Vault $VAULT_NAME"
if ! az keyvault show --name "$VAULT_NAME" -o none 2>/dev/null; then
  # Purge protection cannot be turned off later: a deleted key stays recoverable for 90 days,
  # and so does the vault name.
  az keyvault create --name "$VAULT_NAME" --resource-group "$RESOURCE_GROUP" --location "$LOCATION" \
    --enable-rbac-authorization true --enable-purge-protection true --retention-days 90 \
    --sku standard -o none
fi
VAULT_ID="$(az keyvault show --name "$VAULT_NAME" --query id -o tsv)"

echo "== You: Key Vault Crypto Officer on the vault (to create keys)"
az role assignment create --assignee-object-id "$ME" --assignee-principal-type User \
  --role "Key Vault Crypto Officer" --scope "$VAULT_ID" -o none 2>/dev/null || true

echo "== Keys (RSA 2048, wrap/unwrap only)"
for key in data-kek dp-kek; do
  if az keyvault key show --vault-name "$VAULT_NAME" --name "$key" -o none 2>/dev/null; then
    echo "   $key already exists"; continue
  fi
  for attempt in 1 2 3 4 5 6; do # role assignments take up to a few minutes to apply
    if az keyvault key create --vault-name "$VAULT_NAME" --name "$key" --kty RSA --size 2048 \
         --ops wrapKey unwrapKey -o none 2>/dev/null; then
      echo "   $key created"; break
    fi
    [[ $attempt == 6 ]] && { echo "Could not create $key (role not applied yet?). Re-run the script." >&2; exit 1; }
    sleep 20
  done
done

create_identity() { # app key folder
  local app="$1" key="$2" folder="$3" name="daily-system-$1-$ENVIRONMENT" app_id secret
  echo "== Identity $name -> $key only"
  app_id="$(az ad app list --display-name "$name" --query '[0].appId' -o tsv)"
  if [[ -z "$app_id" ]]; then
    app_id="$(az ad app create --display-name "$name" --query appId -o tsv)"
  fi
  az ad sp show --id "$app_id" -o none 2>/dev/null || az ad sp create --id "$app_id" -o none
  # Scope = one key, not the vault: core cannot use dp-kek, bff cannot use data-kek.
  az role assignment create --assignee "$app_id" --role "Key Vault Crypto Service Encryption User" \
    --scope "$VAULT_ID/keys/$key" -o none
  secret="$(az ad app credential reset --id "$app_id" --display-name "infisical-$(date +%Y%m%d)" \
    --years 1 --query password -o tsv 2>/dev/null)"
  infisical_set "$folder" "AZURE_TENANT_ID=$TENANT_ID" "AZURE_CLIENT_ID=$app_id" \
    "AZURE_CLIENT_SECRET=$secret" "KEY_VAULT_URI=$VAULT_URI" "KEY_VAULT_KEY_NAME=$key"
  unset secret
  echo "   credentials stored in Infisical $folder (secret expires in 1 year)"
}
create_identity core data-kek /core
create_identity bff dp-kek /bff

echo
echo "Done for $ENVIRONMENT. Next: ./tools/bootstrap/3-verify.sh $ENVIRONMENT"
echo "Manual: Azure portal > Cost Management > Budgets: monthly budget 2 EUR with an email alert."
