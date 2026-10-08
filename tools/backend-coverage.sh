#!/usr/bin/env bash
# Backend coverage gate (ADR-017): merges the Cobertura files written, from src/backend, by
#   dotnet test --solution Ecosystem.slnx -c Release -- --coverlet --coverlet-output-format cobertura --results-directory "$PWD/TestResults/coverage"
# and fails when line coverage of Domain and Features code (endpoint mapping excluded) is below the threshold.
# Same command in CI and on your machine. Usage, from anywhere: tools/backend-coverage.sh [threshold, default 80]
# Locally, delete src/backend/TestResults first: files from an older run would be counted.
set -euo pipefail

threshold="${1:-80}"
backend="$(cd "$(dirname "$0")/../src/backend" && pwd)"
raw="$backend/TestResults/coverage"
report="$backend/TestResults/coverage-report"

cd "$backend"
# One file per test project. Fewer means a run failed to write (or two runs collided on the same
# timestamped name), more means leftovers from an older run: either way the percentage would lie.
projects=$(find tests -name '*.csproj' | wc -l)
reports=$(find "$raw" -maxdepth 1 -name 'coverage.cobertura*.xml' 2>/dev/null | wc -l)
if [ "$reports" -ne "$projects" ]; then
  echo "Found $reports coverage files in $raw for $projects test projects." >&2
  echo "Delete src/backend/TestResults and run the tests again with --results-directory \"\$PWD/TestResults/coverage\"." >&2
  exit 1
fi

rm -rf "$report"
dotnet tool run reportgenerator \
  "-reports:$raw/coverage.cobertura*.xml" \
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
