# Security policy

## Reporting a vulnerability

Please report vulnerabilities **privately** through GitHub private vulnerability reporting:
[Security tab > Report a vulnerability](https://github.com/Kiilse/daily-system/security/advisories/new).

Do not open a public issue, pull request or discussion for a security problem.

You can expect an acknowledgement within 7 days. Once the issue is confirmed and fixed, the advisory is published with credit to the reporter, unless you prefer to stay anonymous.

## Supported versions

Only the latest version of `main` (deployed to production) receives security fixes.

## Secrets

This repository is public. If a secret is ever committed, it is **rotated immediately**: removing it from Git history is not enough, because public commits are scraped by bots within minutes.
