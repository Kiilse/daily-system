#!/usr/bin/env bash
# Backend coverage gate (ADR-017): merges the Cobertura files written by
#   dotnet test --solution Ecosystem.slnx -c Release -- --coverlet --coverlet-output-format cobertura
# and fails when line coverage of Domain and Features code (endpoint mapping excluded) is below the threshold.
# Same command in CI and on your machine. Usage, from anywhere: tools/backend-coverage.sh [threshold, default 80]
set -euo pipefail

threshold="${1:-80}"
backend="$(cd "$(dirname "$0")/../src/backend" && pwd)"
report="$backend/TestResults/coverage-report"

cd "$backend"
rm -rf "$report"
dotnet tool run reportgenerator \
  "-reports:tests/**/TestResults/coverage.cobertura*.xml" \
  "-targetdir:$report" \
  "-reporttypes:JsonSummary;MarkdownSummaryGithub" \
  "-classfilters:+*.Domain.*;+*.Features.*;-*Endpoint;-*Endpoints" \
  "-verbosity:Warning"

covered=$(jq '.summary.coveredlines' "$report/Summary.json")
coverable=$(jq '.summary.coverablelines' "$report/Summary.json")
coverage=$(jq '.summary.linecoverage // 0' "$report/Summary.json")

if [ -n "${GITHUB_STEP_SUMMARY:-}" ]; then
  {
    echo "## Backend coverage: ${coverage} % (gate ${threshold} %)"
    echo
    echo "Domain and Features classes, endpoint mapping excluded: ${covered}/${coverable} lines."
    echo
    cat "$report/SummaryGithub.md"
  } >> "$GITHUB_STEP_SUMMARY"
fi

if [ "$coverable" -eq 0 ]; then
  echo "No Domain or Features code to measure yet: gate passes."
  exit 0
fi

# Bash only compares integers: compare covered * 100 with threshold * coverable instead of percentages.
if [ $((covered * 100)) -lt $((threshold * coverable)) ]; then
  echo "Coverage ${coverage} % (${covered}/${coverable} lines) is below the ${threshold} % gate." >&2
  exit 1
fi

echo "Coverage ${coverage} % (${covered}/${coverable} lines), gate ${threshold} %: OK."
