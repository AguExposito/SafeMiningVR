---
name: safemining-orchestrator
description: >-
  Master orchestration skill for SafeMining VR LOTO simulation development.
  Acts as a unified router that classifies any task into its technical domain,
  activates the correct rules + skills + workflows + subagent, enforces MCP gates,
  and reports sources consulted. Use when receiving any development task,
  when unsure which skill or subagent to invoke, or when the user says "orchestrate".
---

# SafeMining Orchestrator

## Purpose

Single entry point that transforms a raw task into a fully-routed, gate-checked, compliance-verified execution pipeline. Eliminates guesswork about which rules, skills, workflows, and MCP tools apply.

## Pipeline

Every task follows four sequential phases. Do not skip phases.

```
┌─────────────────────────────────────────────────────┐
│ Phase 1 — CLASSIFY                                  │
│ Parse task → match domain → resolve ambiguity       │
├─────────────────────────────────────────────────────┤
│ Phase 2 — GATE                                      │
│ MCP checks + doc reads before touching code         │
├─────────────────────────────────────────────────────┤
│ Phase 3 — EXECUTE                                   │
│ Read skill → apply rules → implement or delegate    │
├─────────────────────────────────────────────────────┤
│ Phase 4 — VERIFY & REPORT                           │
│ Console check + sources declaration + deliverable   │
└─────────────────────────────────────────────────────┘
```

---

## Phase 1 — CLASSIFY

Read the task and assign it to **one primary domain** (or two if it genuinely spans boundaries):

| Domain ID | Keywords / signals | Primary rule | Skill | Subagent | Workflow |
|-----------|--------------------|-------------|-------|----------|----------|
| `xr` | interactable, grab, hand tracking, controller, dwell, reticle, outline hint, poke, socket, teleport, XRI, modality | `40-vr-interaction` | `meta-xr-interaction` | `agent-vr-usability` | WF1 |
| `playtest` | PC test, Editor play, mouse click, hotkey, cheat, simulator, camera rig, dual-mode | `40-vr-interaction` | `dual-mode-playtest` | `agent-vr-usability` | WF1 |
| `network` | backend, adapter, sync, queue, DTO, payload, JWT, auth, token, REST, endpoint, IBManager, offline | `70-backend-sync` | `network-session-pipeline` | `agent-network-security` | WF3 |
| `compliance` | LOTO step, EPP, hazard, lock color, tag, scoring, objective, gate, threshold, branding, palette, localization, TranslatableText, pedagogical, serious game | `30-loto-domain` + `50-hud-ui-brand` | `safemining-compliance` | `agent-serious-games` | WF2 |
| `perf` | FPS, frame time, lighting, LOD, draw call, batching, Update loop, GC, build settings, IL2CPP, Vulkan, occlusion, URP | `60-quest-performance` + `10-unity-architecture` | — | `agent-unity-performance` | WF4 |
| `arch` | ControlCenter, LotoGameManager, Relay, ScoreManager, SDB, ref_, Initialize, singleton, HUD bind, KISS | `10-unity-architecture` + `15-code-style` | — | `agent-unity-performance` | — |

If the task spans two domains (e.g., "add a new interactable and verify scoring"), assign a primary and a secondary. Execute primary first, then secondary.

If classification is ambiguous, ask the user with a structured question listing the candidate domains.

---

## Phase 2 — GATE

Execute these checks **before writing any code or modifying any scene**. Skip only if the check is provably irrelevant to the classified domain.

### Always (every domain)

1. Read the primary rule file(s) from the classification table.
2. Read the primary skill file from the classification table.
3. Consult the source-of-truth hierarchy in `AGENTS.md` if any requirement is unclear.

### If domain is `xr` or `playtest`

4. `meta_docs_search` → verify the XRI / Meta OpenXR API being used is current.
5. `Unity_ManageGameObject` → inspect the target GameObject's components before modifying.

### If domain is `network`

4. Read `.agent-docs/integration-vr-backend.md` → confirm API contract.
5. Read `LotoObjectiveIds.cs` → confirm scoring weights if payload includes scores.

### If domain is `compliance`

4. Read `Doc_SMVR/SafeMining_VR_v1.1.md` sections relevant to the scenario (RN, HU).
5. Read `.agent-docs/loto-objectives.md` → confirm objective IDs and weights.

### If domain is `perf` or `arch`

4. `Unity_ManageScene` → get scene hierarchy for the target scene.
5. `Unity_Profiler_*` → capture frame data if investigating a performance issue.

### Cross-cutting gate

6. `Unity_ReadConsole` (Errors) → baseline error count before changes (should be 0).

---

## Phase 3 — EXECUTE

### Direct implementation (default)

For tasks the main agent can handle directly:

1. Read the relevant skill procedure (step-by-step instructions).
2. Apply the domain rules as constraints.
3. Implement the change following project code style (`15-code-style.mdc`).
4. After each file change, call `Unity_ValidateScript` on modified scripts.

### Delegation to subagent

For complex tasks requiring focused context, launch the mapped subagent:

1. Read the subagent skill file (e.g., `.cursor/skills/agent-vr-usability/SKILL.md`).
2. Copy the system prompt template from the skill.
3. Append the specific task after the `TASK:` separator.
4. Launch via Task tool with `subagent_type: "generalPurpose"`.
5. Review the subagent's output before presenting to user.

**Delegate when:**
- The task requires modifying 3+ files across different subsystems.
- HUD prefab bindings + code changes simultaneously (world art is frozen).
- The user explicitly requests a subagent.

### Workflow execution

When the task is a QA validation or audit, delegate to the `workflow-runner` skill:

1. Read `.cursor/skills/workflow-runner/SKILL.md`.
2. Resolve the target workflow or slice from the user's request using the skill's registry.
3. Follow the workflow-runner's 7-step procedure (resolve → read → pre-flight → execute → verdict → report → post-run).
4. Present the structured PASS/FAIL/BLOCKED report to the user.

---

## Phase 4 — VERIFY & REPORT

### Post-change verification

1. `Unity_ReadConsole` (Errors) → must still be 0 entries.
2. `Unity_ValidateScript` on every new or modified `.cs` file.
3. If domain is `xr`: confirm dual-modality (hand tracking + controller) is not broken.
4. If domain is `compliance`: confirm no arcade mechanics introduced.

### Phase E/F — merge gate (automatic)

After verification, **always** run without user prompt:

1. `python tools/ci/merge_gate.py` — fix any `[FAIL]` before closing task
2. `/review-bugbot` if any `.cs` in diff
3. `/review-security` if backend paths per `80-security-review.mdc`
4. End with **"Tu intervención"** only when push, PR, Unity Editor, or merge needs the user

Rule: `.cursor/rules/85-merge-gate.mdc`

### Source declaration

For complex tasks, declare sources at the start of the response:

```
Sources consulted:
- [rule] 40-vr-interaction.mdc — dwell-time reticle spec
- [skill] meta-xr-interaction/SKILL.md — interactable wiring procedure
- [doc] .agent-docs/loto-simulation-handbook.md — S2 hazard list
- [mcp] meta_docs_search("XRSimpleInteractable hoverEntered") — API verified
- [mcp] Unity_ReadConsole(Errors) → 0 entries
```

### Deliverable format

Present the result as:

1. **Classification** — domain(s) assigned and why.
2. **Gates passed** — MCP calls and doc reads completed.
3. **Changes made** — files created/modified with brief rationale.
4. **Verification** — console clean, scripts validated, constraints respected.
5. **Merge gate** — `merge_gate.py` result, review outcomes, **Tu intervención** (if any).

---

## Quick reference — decision tree

```
Task received
│
├─ Contains XR/interaction keywords? ──────→ domain: xr
├─ Contains PC test/Editor keywords? ──────→ domain: playtest
├─ Contains backend/sync/auth keywords? ───→ domain: network
├─ Contains LOTO/scoring/brand keywords? ──→ domain: compliance
├─ Contains FPS/lighting/build keywords? ──→ domain: perf
├─ Contains Manager/CC/SDB keywords? ──────→ domain: arch
├─ Contains "run QA" / "validate" / "WF"? ─→ workflow execution
├─ Ambiguous? ─────────────────────────────→ ask user with options
└─ Multi-domain? ──────────────────────────→ primary + secondary
```

## Constraints

- This skill orchestrates — it does not override domain-specific rules or skills.
- The source-of-truth hierarchy in `AGENTS.md` always takes precedence.
- MCP gates are mandatory, not advisory. Skipping a gate requires explicit user approval.
- Never invent requirements. If the hierarchy is exhausted, ask.
- KISS: gameplay on `LotoGameManager` + relay + `ScoreManager`. Do not author world art. HUD may change. Contract: `.agent-docs/loto-game-board.md`.
