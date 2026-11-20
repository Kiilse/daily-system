#!/usr/bin/env bash
# Shared settings for the bootstrap scripts of ticket #9. Sourced, not executed.
set -euo pipefail
# Git Bash on Windows rewrites arguments starting with "/" (Azure scopes, Infisical paths).
export MSYS_NO_PATHCONV=1

ENVIRONMENT="${1:-}"
if [[ "$ENVIRONMENT" != "dev" && "$ENVIRONMENT" != "prod" ]]; then
  echo "Usage: $0 dev|prod" >&2
  exit 1
fi

: "${INFISICAL_PROJECT_ID:?Set INFISICAL_PROJECT_ID (Infisical project settings, Project ID)}"
INFISICAL_DOMAIN="${INFISICAL_DOMAIN:-https://eu.infisical.com}"
LOCATION="${LOCATION:-francecentral}"
# Key Vault names are global across Azure: set VAULT_SUFFIX (e.g. "vc1") if the name is taken.
VAULT_SUFFIX="${VAULT_SUFFIX:-}"
RESOURCE_GROUP="rg-daily-system-${ENVIRONMENT}"
VAULT_NAME="kv-dailysys-${ENVIRONMENT}${VAULT_SUFFIX}"
VAULT_URI="https://${VAULT_NAME}.vault.azure.net/"

infisical_set() { # folder KEY=value...
  local folder="$1"; shift
  infisical secrets set "$@" --env="$ENVIRONMENT" --path="$folder" \
    --projectId="$INFISICAL_PROJECT_ID" --domain="$INFISICAL_DOMAIN" --silent >/dev/null
}

infisical_get() { # folder KEY
  infisical secrets get "$2" --env="$ENVIRONMENT" --path="$1" \
    --projectId="$INFISICAL_PROJECT_ID" --domain="$INFISICAL_DOMAIN" --plain --silent
}

require() {
  for tool in "$@"; do
    command -v "$tool" >/dev/null || { echo "Missing tool: $tool" >&2; exit 1; }
  done
}
