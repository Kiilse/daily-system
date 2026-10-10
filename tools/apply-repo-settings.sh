#!/usr/bin/env bash
# Applies the repository settings decided in ticket #1 (ADR-003, ADR-016). Safe to run again:
# every call sets the desired state, and the ruleset is updated in place when it already exists.
# Requires the GitHub CLI logged in as a repository admin (gh auth login).
set -euo pipefail

repo="${1:-Kiilse/daily-system}"
ruleset_name="protect-main"

echo "== Merge options and secret scanning"
gh api --method PATCH "repos/$repo" --silent --input - <<'JSON'
{
  "allow_squash_merge": true,
  "allow_merge_commit": false,
  "allow_rebase_merge": false,
  "squash_merge_commit_title": "PR_TITLE",
  "squash_merge_commit_message": "PR_BODY",
  "delete_branch_on_merge": true,
  "security_and_analysis": {
    "secret_scanning": { "status": "enabled" },
    "secret_scanning_push_protection": { "status": "enabled" }
  }
}
JSON

echo "== Dependabot alerts and private vulnerability reporting"
gh api --method PUT "repos/$repo/vulnerability-alerts" --silent
gh api --method PUT "repos/$repo/private-vulnerability-reporting" --silent

echo "== Ruleset $ruleset_name on the default branch"
# Required status checks: each CI ticket (#6, #7, #8) adds its job here as it lands.
# integration_id 15368 is the GitHub Actions app: only a check reported by Actions counts.
# code_scanning: a CodeQL alert only shows in the Security tab; this rule makes a high one block the merge.
ruleset=$(cat <<JSON
{
  "name": "$ruleset_name",
  "target": "branch",
  "enforcement": "active",
  "bypass_actors": [],
  "conditions": { "ref_name": { "include": ["~DEFAULT_BRANCH"], "exclude": [] } },
  "rules": [
    {
      "type": "pull_request",
      "parameters": {
        "required_approving_review_count": 0,
        "dismiss_stale_reviews_on_push": false,
        "require_code_owner_review": false,
        "require_last_push_approval": false,
        "required_review_thread_resolution": true,
        "allowed_merge_methods": ["squash"]
      }
    },
    {
      "type": "required_status_checks",
      "parameters": {
        "strict_required_status_checks_policy": false,
        "do_not_enforce_on_create": false,
        "required_status_checks": [
          { "context": "backend", "integration_id": 15368 },
          { "context": "frontend", "integration_id": 15368 },
          { "context": "secrets", "integration_id": 15368 },
          { "context": "codeql (csharp)", "integration_id": 15368 },
          { "context": "codeql (javascript-typescript)", "integration_id": 15368 },
          { "context": "dependency-review", "integration_id": 15368 },
          { "context": "dotnet-vulnerable", "integration_id": 15368 },
          { "context": "npm-audit", "integration_id": 15368 },
          { "context": "images", "integration_id": 15368 }
        ]
      }
    },
    {
      "type": "code_scanning",
      "parameters": {
        "code_scanning_tools": [
          { "tool": "CodeQL", "security_alerts_threshold": "high_or_higher", "alerts_threshold": "errors" }
        ]
      }
    },
    { "type": "required_linear_history" },
    { "type": "non_fast_forward" },
    { "type": "deletion" }
  ]
}
JSON
)
ruleset_id=$(gh api "repos/$repo/rulesets" --jq ".[] | select(.name == \"$ruleset_name\") | .id")
if [[ -n "$ruleset_id" ]]; then
  echo "$ruleset" | gh api --method PUT "repos/$repo/rulesets/$ruleset_id" --silent --input -
  echo "Updated ruleset $ruleset_id"
else
  echo "$ruleset" | gh api --method POST "repos/$repo/rulesets" --silent --input -
  echo "Created ruleset"
fi

echo "== Current state"
gh api "repos/$repo" --jq '{
  visibility, default_branch, allow_squash_merge, allow_merge_commit, allow_rebase_merge,
  squash_merge_commit_title, squash_merge_commit_message, delete_branch_on_merge,
  secret_scanning: .security_and_analysis.secret_scanning.status,
  push_protection: .security_and_analysis.secret_scanning_push_protection.status
}'
# This endpoint answers 204 when enabled and 404 when disabled
if gh api "repos/$repo/vulnerability-alerts" --silent 2>/dev/null; then
  echo "dependabot_alerts: enabled"
else
  echo "dependabot_alerts: DISABLED"
fi
gh api "repos/$repo/private-vulnerability-reporting" --jq '"private_vulnerability_reporting: \(.enabled)"'
gh api "repos/$repo/rulesets" --jq '.[] | "ruleset: \(.name) (\(.enforcement))"'
