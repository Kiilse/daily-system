#!/usr/bin/env bash
# Ticket #9, step 3: checks the acceptance criteria with the real identities.
# Logs in as each app registration in an isolated Azure CLI profile (your own session is untouched).
# Usage: INFISICAL_PROJECT_ID=... ./tools/bootstrap/3-verify.sh dev   (OTHER_VAULT=kv-dailysys-prod to test isolation)
source "$(dirname "$0")/common.sh"
require az infisical openssl

pass=0; fail=0
check() { if "$@" >/dev/null 2>&1; then echo "   PASS"; pass=$((pass+1)); else echo "   FAIL"; fail=$((fail+1)); fi; }
# Denied must mean "forbidden by Azure RBAC", not "not found": a 404 would hide a missing permission check.
check_denied() {
  local out
  if out="$("$@" 2>&1)"; then echo "   FAIL (allowed)"; fail=$((fail+1))
  elif grep -qiE "forbidden|not authorized|does not have" <<<"$out"; then echo "   PASS (denied)"; pass=$((pass+1))
  else echo "   FAIL (unexpected error: ${out:0:160})"; fail=$((fail+1)); fi
}

wrap_roundtrip() { # vault key
  local plain wrapped back
  plain="$(openssl rand -base64 32 | tr '+/' '-_' | tr -d '=')"
  wrapped="$(az rest --method post --resource https://vault.azure.net \
    --url "https://$1.vault.azure.net/keys/$2/wrapkey?api-version=7.4" \
    --body "{\"alg\":\"RSA-OAEP-256\",\"value\":\"$plain\"}" --query value -o tsv)" || return 1
  back="$(az rest --method post --resource https://vault.azure.net \
    --url "https://$1.vault.azure.net/keys/$2/unwrapkey?api-version=7.4" \
    --body "{\"alg\":\"RSA-OAEP-256\",\"value\":\"$wrapped\"}" --query value -o tsv)" || return 1
  [[ "$back" == "$plain" && "$wrapped" != "$plain" ]]
}

for app in core bff; do
  folder="/$app"
  key="$(infisical_get "$folder" KEY_VAULT_KEY_NAME)"
  other_key=$([[ $key == data-kek ]] && echo dp-kek || echo data-kek)
  export AZURE_CONFIG_DIR; AZURE_CONFIG_DIR="$(mktemp -d)"
  az login --service-principal --username "$(infisical_get "$folder" AZURE_CLIENT_ID)" \
    --password "$(infisical_get "$folder" AZURE_CLIENT_SECRET)" \
    --tenant "$(infisical_get "$folder" AZURE_TENANT_ID)" --allow-no-subscriptions -o none

  echo "== $app ($ENVIRONMENT): wrap and unwrap 32 random bytes with $key"
  check wrap_roundtrip "$VAULT_NAME" "$key"
  echo "== $app: cannot use $other_key"
  check_denied wrap_roundtrip "$VAULT_NAME" "$other_key"
  echo "== $app: cannot create a key"
  check_denied az keyvault key create --vault-name "$VAULT_NAME" --name must-not-exist --kty RSA --size 2048
  echo "== $app: cannot delete a key"
  check_denied az keyvault key delete --vault-name "$VAULT_NAME" --name must-not-exist
  if [[ -n "${OTHER_VAULT:-}" ]]; then
    echo "== $app: cannot use the other environment's vault ($OTHER_VAULT)"
    check_denied wrap_roundtrip "$OTHER_VAULT" "$key"
  fi
  rm -rf "$AZURE_CONFIG_DIR"; unset AZURE_CONFIG_DIR
done

echo "== Repository: no secret value committed"
check gitleaks git --redact --no-banner "$(git rev-parse --show-toplevel)"

echo
echo "Result: $pass passed, $fail failed"
[[ $fail == 0 ]]
