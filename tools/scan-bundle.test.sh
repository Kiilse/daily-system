#!/usr/bin/env bash
# Tests for tools/scan-bundle.sh (ticket #7): a clean bundle passes, each kind of secret or a source map fails.
# The fake secrets are assembled at run time so this file itself never matches a secret scanner.
set -euo pipefail

scan="$(dirname "$0")/scan-bundle.sh"
work=$(mktemp -d)
trap 'rm -rf "$work"' EXIT
failures=0

# expect <passes|fails> <name> <file name> <content> [text the scan output must contain]
expect() {
  local dir="$work/$2"
  mkdir -p "$dir"
  printf '%s\n' "$4" > "$dir/$3"
  if "$scan" "$dir" > "$dir.log" 2>&1; then actual=passes; else actual=fails; fi
  if [[ "$actual" == "$1" ]] && grep -qF -- "${5:-}" "$dir.log"; then
    echo "ok   $2 $1"
  else
    echo "FAIL $2: expected the scan to $1${5:+ reporting '$5'}, it $actual"; sed 's/^/     /' "$dir.log"
    failures=$((failures + 1))
  fi
}

b64=ABCDEFGHIJKLMNOPQRSTUVWXYZabcdef

expect passes clean           main.js 'const api="/api/menu";function f(a){return a.headers}'
expect fails  private-key     main.js "const k=\"-----BEGIN RSA PRIVATE"" KEY-----\\nMIIEow\";" 'Possible private key'
expect fails  jwt             main.js "const t=\"eyJ${b64}.eyJ${b64}.${b64}\";" 'Possible JWT'
expect fails  client-secret   main.js "const c={client_""secret:\"x\"};" 'Possible client_secret'
expect fails  bearer          main.js "h.set(\"Authorization\",\"Bearer ${b64}${b64}\");" 'Possible bearer token'
expect fails  aws-key         main.js "const a=\"AKI""AZ7Q4XWERTYUIOPLK\";" 'Possible AWS access key'
expect fails  azure-secret    main.js "const s=\"abc8Q""~${b64}xyz12\";" 'Possible Azure client secret'
expect fails  azure-storage   main.js "const s=\"DefaultEndpointsProtocol=https;Account""Key=${b64}${b64}==\";" 'Possible Azure storage key'
expect fails  source-map      main.js.map '{"version":3}' 'Source maps must not be deployed'

if (( failures > 0 )); then
  echo "$failures scan-bundle test(s) failed"
  exit 1
fi
echo "All scan-bundle tests passed"
