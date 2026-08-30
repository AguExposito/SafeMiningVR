---
name: merge-gate
description: >-
  Run Phases E/F automatically at end of substantive SafeMining VR tasks:
  merge_gate.py, MCP validation, Bugbot and Security Review. Escalate to user
  only with explicit "Tu intervención" instructions.
---

# Merge Gate — Phases E & F

Execute at **task completion** without waiting for user request.

## Phase E

```bash
python tools/ci/merge_gate.py
```

- Exit non-zero → fix failures, re-run until PASS
- If C# changed: `Unity_ReadConsole` (Errors) = 0, `Unity_ValidateScript` per file
- Use suggested PR title/body from script output when user opens PR

## Phase F

| Trigger | Action |
|---------|--------|
| Any `.cs` in diff | `/review-bugbot` on `branch changes` |
| `Code/Adapters/`, `SyncQueueManager`, `Code/Data/`, `IBManager` | `/review-security` on `branch changes` |
| CRITICAL/HIGH finding | Stop; report and fix before claiming done |

## User escalation template

Only when automation cannot finish:

```markdown
## ⚠️ Tu intervención
1. <acción> — <comando o paso exacto>
```

Common cases:
- Unpushed branch → `git push -u origin <branch>` + crear PR en GitHub
- Unity MCP unavailable → abrir Editor con proyecto SafeMiningVR
- Merge → aprobar PR tras CI y reviews verdes

If fully automated: **Sin intervención requerida.**

## Task not complete until

- `merge_gate.py` exit 0
- Unity 0 errors (if applicable)
- Reviews clear (if applicable)
