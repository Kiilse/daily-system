#!/usr/bin/env bash
# Ticket #9, step 1: folders and generated secrets in Infisical for one environment.
# Prerequisites: `infisical login --domain https://eu.infisical.com`, project created with dev and prod.
# Usage: INFISICAL_PROJECT_ID=... ./tools/bootstrap/1-infisical-secrets.sh dev
source "$(dirname "$0")/common.sh"
require infisical openssl

echo "== Infisical folders ($ENVIRONMENT)"
for folder in bff core ci worker; do
  infisical secrets folders create --name="$folder" --path=/ --env="$ENVIRONMENT" \
    --projectId="$INFISICAL_PROJECT_ID" --domain="$INFISICAL_DOMAIN" --silent >/dev/null 2>&1 \
    || echo "   /$folder already exists"
done

echo "== Generated secrets ($ENVIRONMENT)"
# Never regenerate an existing value: BLIND_INDEX_ROOT_KEY changes would break every blind index.
if infisical_get /core BLIND_INDEX_ROOT_KEY >/dev/null 2>&1; then
  echo "   BLIND_INDEX_ROOT_KEY already set, kept"
else
  infisical_set /core "BLIND_INDEX_ROOT_KEY=$(openssl rand -base64 32)"
  echo "   BLIND_INDEX_ROOT_KEY created in /core"
fi

if infisical_get /bff EDGE_SHARED_SECRET >/dev/null 2>&1; then
  echo "   EDGE_SHARED_SECRET already set, kept"
else
  edge="$(openssl rand -base64 32)"
  infisical_set /bff "EDGE_SHARED_SECRET=$edge"
  infisical_set /worker "EDGE_SHARED_SECRET=$edge"
  unset edge
  echo "   EDGE_SHARED_SECRET created in /bff and /worker (same value)"
fi
echo "Done. No secret value was printed."
