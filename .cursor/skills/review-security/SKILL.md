---
name: review-security
description: >-
  Run SafeMining VR security review on branch or uncommitted changes. Use when
  the user says /review-security, before merging adapter/sync/auth changes, or
  after cloud agent Fase 7 work. Launches the security-review subagent.
disable-model-invocation: true
---

# Review Security — SafeMining VR

Use when the user asks for `/review-security` or when a PR touches paths in `80-security-review.mdc`.

## Pre-check

If diff may include backend paths, run locally:

```bash
python tools/ci/validate_security_surface.py
```

## Launch

Invoke exactly one native subagent:

```
/review-security
```

Or Task tool with subagent reading `.cursor/agents/security-review.md`.

### Prompt shape

```text
Full Repository Path: D:/Unity/Projects/SafeMiningVR
Diff: branch changes
Custom Instructions: Audit per .cursor/rules/80-security-review.mdc vectors V1–V6. Include merge recommendation.
```

Use `uncommitted changes` only when user requests local dirty tree review.

## After review

Summarize as markdown table: **Severity | Location (file:line) | Finding**

- 0 CRITICAL/HIGH → note "Security review: clear to merge (pending CI)"
- Any CRITICAL → "BLOCK MERGE"
- Any HIGH → "FIX REQUIRED before merge"

Do not auto-fix unless user asks.

## Path triggers (auto-suggest review)

- `Assets/SafeMining/LOTOSimulation/Code/Adapters/**`
- `Assets/SafeMining/LOTOSimulation/Code/Data/**`
- `SyncQueueManager.cs`, `SimulationSessionManager.cs`
- `IBManager.cs`, `ScoreManager.cs`
