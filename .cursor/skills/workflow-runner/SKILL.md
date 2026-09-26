---
name: workflow-runner
description: >-
  Execute SafeMining VR QA workflows (WF1–WF4) step by step with MCP-backed verification.
  Reads checklists from .agent-docs/workflows/, runs each item against live project state via
  Unity and Meta VR MCP tools, and produces a structured PASS/FAIL/BLOCKED report.
  Use when the user says "run QA", "run workflow", "validate slice", "smoke test",
  "run WF1", "run WF4", "validate dual-mode", "validate LOTO", "check build", or similar.
---

# Workflow Runner

## Purpose

Interpret and execute the QA workflow checklists in `.agent-docs/workflows/` against the live project state, using MCP tools for objective verification wherever possible. Produce a structured report with per-item verdicts and actionable remediation for failures.

## Workflow registry

| Shorthand | File | Scope |
|-----------|------|-------|
| `WF1` | `.agent-docs/workflows/wf-dual-mode-testing.md` | PC Playmode + Quest 3S input/ergonomics |
| `WF2` | `.agent-docs/workflows/wf-loto-step-compliance.md` | S1–S3 on `LotoGameManager` + `ScoreManager`, branding |
| `WF3` | `.agent-docs/workflows/wf-network-auth-telemetry.md` | Offline-first sync, DTOs, API contracts |
| `WF4` | `.agent-docs/workflows/wf-build-performance.md` | Build settings, lighting, script perf, 72 FPS |
| `all` | All four in order: WF4 → WF1 → WF2 → WF3 | Full QA pass |

Slice shortcuts (run specific phases within a workflow):

| Shorthand | Resolves to |
|-----------|-------------|
| `S1`, `slice S1` | WF2 Phase B (S1 EPP) |
| `S2`, `slice S2` | WF2 Phase C (S2 Hazards) |
| `S3`, `slice S3` | WF2 Phase D (S3 LOTO Sequence) |
| `scoring` | WF2 Phase E (Scoring Validation) |
| `dual-mode` | WF1 (all phases) |
| `smoke test` | WF4 Phase A (Build Settings) + WF1 Phase A (PC Playmode) |
| `device`, `quest` | WF1 Phase B (Quest 3S Device) + WF4 Phase E (Runtime Perf) |
| `build` | WF4 Phase A–D (everything except device runtime) |
| `network`, `sync` | WF3 (all phases) |
| `brand`, `palette` | WF2 Phase A3 (Branding) + WF2 Phase F (Localization) |

## Procedure

### Step 1 — Resolve target

Parse the user's request and match it to one or more entries from the registry above.

If ambiguous, ask the user:

```
Which workflow should I run?
- WF1: Dual-Mode Testing & Ergonomics
- WF2: LOTO Serious Gaming & Step Compliance
- WF3: HTTP Network, Auth & Telemetry Pipeline
- WF4: Build & Performance Verification
- all: Full QA pass (WF4 → WF1 → WF2 → WF3)
```

### Step 2 — Read the workflow file

Read the full workflow markdown from `.agent-docs/workflows/`. Parse it into phases and individual checklist items.

### Step 3 — Pre-flight gate

Before running any checklist items:

1. `Unity_ManageEditor` → `GetProjectRoot` must return `C:/AGU/Unity/Projects/SafeMiningVR`.
2. `Unity_ReadConsole` (Errors) → record baseline error count (should be 0).
3. For device workflows (WF1-B, WF4-E): check `metavr_device` → `action: "list"`. If no device, mark those phases `BLOCKED`.

### Step 4 — Execute checklist

Process each phase and each item within it sequentially. For every item:

#### Items with MCP verification

When the checklist item references a specific MCP call, execute it:

| Checklist pattern | MCP action |
|-------------------|------------|
| "Unity_ReadConsole" or "console errors" | `Unity_GetConsoleLogs` → `logTypes: "Error"` |
| "build settings" or "platform" | `Unity_ManageEditor` → `Action: "GetState"` |
| "scene hierarchy" or "light count" | `Unity_ManageScene` → `Action: "GetHierarchy"` |
| "find GameObject" or "component on" | `Unity_ManageGameObject` → `action: "find"` or `"get_components"` |
| "build scenes" or "build index" | `Unity_ManageScene` → `Action: "GetBuildSettings"` |
| "device logcat" | `get_device_logcat` → `tag: "Unity", level: "E"` |
| "device connected" | `metavr_device` → `action: "list"` |
| "FPS" or "metrics" | `capture_perfetto_trace` + `analyze_trace` |

Execute the MCP call. Compare the result to the PASS/FAIL criteria defined in the workflow.

#### Items with code inspection

When the checklist item requires verifying source code (e.g., "no empty Update loops", "no hardcoded strings"):

1. Use `Grep` with the pattern specified in the workflow (or infer it).
2. Scope to `Assets/SafeMining/` unless the item specifies otherwise.
3. Compare findings to the PASS/FAIL criteria.

#### Items requiring manual/device verification

When the item requires physical interaction (e.g., "pick up Touch controller", "grab with hand tracking"):

1. Mark as `BLOCKED` with reason: "requires device interaction".
2. Record it in the report for manual follow-up.

### Step 5 — Assign verdicts

For each checklist item, assign exactly one verdict:

| Verdict | Meaning | When to use |
|---------|---------|-------------|
| `PASS` | Item meets its defined criteria | MCP/code evidence confirms compliance |
| `FAIL` | Item does not meet criteria | MCP/code evidence shows violation |
| `BLOCKED` | Cannot be verified automatically | Requires device, manual action, or external dependency |
| `SKIP` | Not applicable to the requested scope | Item belongs to a phase outside the user's request |

### Step 6 — Generate report

Present the report in this format:

```
## Workflow Report: WF4 — Build & Performance Verification
Run: 2026-08-29 20:00 UTC-3

### Phase A — Build Settings Verification
| # | Item | Verdict | Evidence |
|---|------|---------|----------|
| A1.1 | Active platform = Android | PASS | Unity_ManageEditor: platform=Android |
| A1.2 | Scripting backend = IL2CPP | PASS | Unity_ManageEditor: scriptingBackend=IL2CPP |
| A1.3 | Architecture = ARM64 | FAIL | Unity_ManageEditor: architecture=ARMv7 | 
| A2.1 | Graphics API = Vulkan | PASS | Unity_ManageEditor: graphicsAPI=Vulkan |

### Phase B — Scene Lighting Budget
| # | Item | Verdict | Evidence |
|---|------|---------|----------|
| B1.1 | S1 key light ≤1 | PASS | Unity_ManageScene: 1 Mixed Spot found |
| B1.2 | S1 fill lights ≤2 | PASS | Unity_ManageScene: 2 Mixed Point found |
| B1.3 | S1 directional sun disabled | PASS | No active Directional light |

### Summary
| Verdict | Count |
|---------|-------|
| PASS | 6 |
| FAIL | 1 |
| BLOCKED | 0 |

### Failures — Recommended Actions
| # | Item | Current | Required | Fix |
|---|------|---------|----------|-----|
| A1.3 | Architecture | ARMv7 | ARM64 | Project Settings → Player → Architecture → ARM64 |
```

### Step 7 — Post-run gate

After completing the report:

1. `Unity_ReadConsole` (Errors) → confirm no new errors were introduced during inspection.
2. If running `all`, present the combined summary across WF4 → WF1 → WF2 → WF3.

## Integration with safemining-orchestrator

The orchestrator routes to this skill when the classified domain maps to a workflow:

```
safemining-orchestrator Phase 3 → domain has workflow? → yes → workflow-runner
```

The orchestrator's Phase 4 (VERIFY & REPORT) uses this skill's report format.

## Constraints

- **Read-only by default.** This skill inspects and reports. It does not fix failures unless the user explicitly asks "fix the failures" after seeing the report.
- **MCP evidence required.** Never mark an MCP-verifiable item as PASS without executing the call. If the MCP is unavailable, mark BLOCKED.
- **No subjective verdicts.** PASS/FAIL criteria come from the workflow file and project documentation, not from the agent's judgment.
- **Partial runs are valid.** The user can request a single phase, a single slice, or a single workflow. Only report on what was requested.
