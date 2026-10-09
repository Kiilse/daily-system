#!/usr/bin/env bash
# Scans a built frontend bundle for secrets (ticket #7, ADR-016): everything in the bundle is public.
# Fails on any gitleaks finding, any extra pattern below, or any source map. Matches are never printed,
# only the file and the kind of match: the CI logs of this public repository are public too.
# Usage: tools/scan-bundle.sh <dist directory>
set -euo pipefail

dir="${1:?usage: scan-bundle.sh <dist directory>}"
[[ -d "$dir" ]] || { echo "No such directory: $dir" >&2; exit 2; }
found=0

maps=$(find "$dir" -name '*.map')
if [[ -n "$maps" ]]; then
  echo "Source maps must not be deployed:"; echo "$maps"
  found=1
fi

if ! gitleaks dir "$dir" --no-banner --redact --exit-code 1; then
  found=1
fi

# Patterns gitleaks can miss in minified code (no keyword nearby, low entropy fake values).
patterns=(
  "private key|-----BEGIN [A-Z ]*PRIVATE KEY-----"
  "JWT|eyJ[A-Za-z0-9_-]{10,}\.[A-Za-z0-9_-]{10,}\.[A-Za-z0-9_-]{10,}"
  "client_secret|[Cc]lient_?[Ss]ecret|CLIENT_?SECRET"
  "bearer token|Bearer [A-Za-z0-9._~+/=-]{20,}"
  "AWS access key|(AKIA|ASIA)[0-9A-Z]{16}"
  "Azure client secret|[A-Za-z0-9_.-]{3}8Q~[A-Za-z0-9_.~-]{31,34}"
  "Azure storage key|AccountKey=[A-Za-z0-9+/=]{40,}"
)
for entry in "${patterns[@]}"; do
  name="${entry%%|*}"
  regex="${entry#*|}"
  # grep exits 1 on no match and 2 on error (bad regex, unreadable file): an error must fail the scan, not pass it.
  rc=0; files=$(grep -rlIE -- "$regex" "$dir") || rc=$?
  if (( rc == 0 )); then
    echo "Possible $name in:"; echo "$files"
    found=1
  elif (( rc > 1 )); then
    echo "Pattern '$name' could not be checked (grep exit $rc)"
    found=1
  fi
done

if (( found )); then
  echo "Bundle scan failed: remove the secret from the frontend, it ships to every visitor."
  exit 1
fi
echo "Bundle scan clean: $dir"
