# Session Brief Templates — SafeMining VR

Quick-start templates for common development sessions. Copy a template into a new Cursor chat (or Notepad) and fill the `[placeholders]`.

## How to use

1. Pick the template that matches your task.
2. Copy the block between the `---` fences.
3. Paste it as your first message in a new Cursor chat.
4. Replace every `[placeholder]` with your specifics.
5. The agent will read the referenced files and follow the MCP gates automatically.

---

## Template 1 — XR Feature / Slice

For implementing interactables, hand tracking, dwell-time reticles, outline hints, sockets, or modality switching.

```
## Session Brief: XR Feature

**Task:** [Describe the interaction to implement or modify]
**Scenario:** [S1 / S2 / S3 / All]
**Target:** [PC Editor / Quest 3S / Both]
**Requirement:** [RN-* / HU-* / None — check .agent-docs/INDEX.md]

**Context files:**
@.cursor/rules/40-vr-interaction.mdc
@.cursor/skills/meta-xr-interaction/SKILL.md
@.agent-docs/loto-simulation-handbook.md
@.agent-docs/INDEX.md

**MCP gates:**
- [ ] Unity_ManageEditor → GetProjectRoot
- [ ] meta_docs_search → verify XRI API before coding
- [ ] Unity_ReadConsole (Errors) → 0 after changes

**Constraints:**
- Hand tracking first, controller second.
- Dwell for triggers (0.8–1.2s), direct grab for physical objects.
- ActivateFromPc() on every interactable.
- No arcade mechanics. No camera shake.
```

---

## Template 2 — Backend & Telemetry

For adapters, DTOs, sync queue, auth flow, scoring payloads, or API contract work.

```
## Session Brief: Backend & Telemetry

**Task:** [Describe the backend work — adapter, DTO, sync, auth, payload]
**Phase:** [Fase 5 (offline) / Fase 7 (REST)]
**Endpoint:** [/api/vr/sessions/start | .../events | .../complete | .../sync | N/A]
**Requirement:** [RN-* / RNF-* / HU-220 / None — check .agent-docs/INDEX.md]

**Context files:**
@.cursor/rules/70-backend-sync.mdc
@.cursor/skills/network-session-pipeline/SKILL.md
@.agent-docs/integration-vr-backend.md
@.agent-docs/INDEX.md

**MCP gates:**
- [ ] Unity_ManageEditor → GetProjectRoot
- [ ] Unity_ReadConsole (Errors) → 0 after changes

**Constraints:**
- Offline-first: gameplay never blocks on network.
- IBManager legacy path preserved — do not modify.
- JsonUtility with wrapper classes for list serialization.
- No PII beyond participantId / sessionId.
- Scoring weights must sum to 100%, threshold 60%.
```

---

## Template 3 — QA & Workflow Execution

For running QA checklists, smoke tests, slice validations, or compliance audits.

```
## Session Brief: QA Workflow

**Workflow:** [WF1 / WF2 / WF3 / WF4 / all]
**Scope:** [Full workflow / Phase only (e.g., WF2-D for S3 LOTO) / Slice (e.g., S2)]
**Target:** [PC Editor / Quest 3S / Both]

**Context files:**
@.agent-docs/workflows/README.md
@.cursor/skills/workflow-runner/SKILL.md
@.agent-docs/INDEX.md

**Instruction:** Run the workflow step by step using the workflow-runner skill.
Report each item as PASS / FAIL / BLOCKED with MCP evidence.
Present the structured report at the end.

**MCP gates:**
- [ ] Unity_ManageEditor → GetProjectRoot
- [ ] Unity_ReadConsole (Errors) → baseline 0
- [ ] metavr_device → list (if device phases)
```

---

## Template 4 — Bugfix / Polish / Performance

For fixing usability issues, VR ergonomics, lighting, framerate, or code quality problems.

```
## Session Brief: Bugfix / Polish

**Issue:** [Describe the bug, performance problem, or polish task]
**Scenario:** [S1 / S2 / S3 / Menu / All]
**Target:** [PC Editor / Quest 3S / Both]
**Symptom:** [What's broken — FPS drop, visual glitch, soft-lock, wrong feedback, etc.]

**Context files:**
@.cursor/rules/60-quest-performance.mdc
@.cursor/rules/10-unity-architecture.mdc
@.cursor/rules/15-code-style.mdc
@.agent-docs/loto-simulation-handbook.md
@.agent-docs/INDEX.md

**MCP gates:**
- [ ] Unity_ManageEditor → GetProjectRoot
- [ ] Unity_ReadConsole (Errors) → 0 before and after fix
- [ ] meta_docs_search → if fix touches XR APIs
- [ ] Unity_Profiler_* → if performance issue

**Constraints:**
- 72 FPS sustained on Quest 3S (13.9ms frame budget).
- 1 key + 2 fill lights per scene max.
- No empty Update(), no FindObjectOfType in Update.
- No comments in C# code. #region grouping.
```

---

## Template 5 — LOTO Compliance Review

For auditing a scenario against the formal spec, checking procedure fidelity, scoring, or branding.

```
## Session Brief: Compliance Review

**Scope:** [S1 EPP / S2 Hazards / S3 LOTO / Scoring / Branding / Localization / Full]
**Requirement:** [RN01 / RN02 / HU-221 / HU-222 / HU-223 / All — check .agent-docs/INDEX.md]

**Context files:**
@.cursor/rules/30-loto-domain.mdc
@.cursor/rules/50-hud-ui-brand.mdc
@.cursor/skills/safemining-compliance/SKILL.md
@.agent-docs/loto-objectives.md
@.agent-docs/INDEX.md

**Instruction:** Audit the specified scope against the formal spec.
Use INDEX.md to read only the relevant RN/HU line ranges from Doc_SMVR.
Report each criterion as PASS / FAIL with evidence.

**Constraints:**
- Industrial training, not arcade. No timers, combos, or pop-ups.
- Approval threshold is 60% (not 75%).
- All strings via TranslatableText — no hardcoded Spanish.
- All colors via Brand.* accessors — no hardcoded hex.
```

---

## Template 6 — Quick Smoke Test

Minimal template for a fast validation pass before a build or merge.

```
## Session Brief: Smoke Test

**Scope:** [PC only / PC + Quest / Build settings only]

@.cursor/skills/workflow-runner/SKILL.md
@.agent-docs/INDEX.md

Run: smoke test
Report PASS / FAIL / BLOCKED for each item.
```
