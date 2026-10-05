#!/usr/bin/env bash
# One-time setup for a fresh clone: enables the versioned git hooks and checks the tools they need.
set -euo pipefail

repo_root="$(git rev-parse --show-toplevel)"
cd "$repo_root"

git config core.hooksPath .githooks
echo "Git hooks enabled from .githooks/"

if command -v gitleaks >/dev/null 2>&1; then
  echo "gitleaks $(gitleaks version) found"
else
  echo "gitleaks is missing: every commit will be blocked until it is installed." >&2
  echo "See https://github.com/gitleaks/gitleaks#installing" >&2
  exit 1
fi
