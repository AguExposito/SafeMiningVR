---
name: review-bugbot
description: >-
  Run SafeMining VR Bugbot review on branch or uncommitted changes. Use when the
  user says /review-bugbot, before merging cloud agent PRs, or for regression
  checks on LOTO/sync/XR code. Launches the bugbot subagent.
disable-model-invocation: true
---

# Review Bugbot — SafeMining VR

Use when the user asks for `/review-bugbot` or before merging cloud/background agent PRs.

## Launch

Invoke exactly one native subagent:

```
/review-bugbot
```

Or Task tool with subagent reading `.cursor/agents/bugbot.md`.

### Prompt shape

```text
Full Repository Path: C:/AGU/Unity/Projects/SafeMiningVR
Diff: branch changes
Custom Instructions: Review for logic bugs, regressions, async pitfalls, LOTO gate violations, and sync queue correctness. Sort findings by severity.
```

Use `uncommitted changes` when user requests local-only review.

If diff cannot be computed, use `natural language` with per-file change description.

## After review

Summarize as markdown table: **Severity | Location (file:line) | Finding**

- No issues → "Bugbot: no bugs found"
- CRITICAL/HIGH → list and recommend fix before merge

## Pair with security review

When diff touches Adapters, SyncQueue, or DTOs, also run `/review-security`.

## Cloud agent merge gate

Per `.agent-docs/cloud-agent-playbook.md`, Bugbot is required before merging any cloud agent PR.
