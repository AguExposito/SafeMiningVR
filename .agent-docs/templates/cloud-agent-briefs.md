# Cloud Agent Brief Templates

Copy-paste templates for `/in-cloud` or background subagents. Replace `{placeholders}`.

---

## Template A — WF2 Full LOTO Compliance Audit

```text
/in-cloud

## Objective
Execute WF2 (LOTO Serious Gaming & Step Compliance) end-to-end audit on branch `{branch}` and produce a structured PASS/FAIL report with remediation list for any failures.

## Branch
`audit/wf2-loto-compliance-{date}`

## Orchestrator
/safemining-orchestrator — classify as compliance, delegate to /loto-compliance

## Sources (read before auditing)
- @.agent-docs/workflows/wf-loto-step-compliance.md (primary checklist — all phases A–F)
- @.cursor/rules/30-loto-domain.mdc
- @.cursor/rules/50-hud-ui-brand.mdc
- @.agent-docs/INDEX.md → RN01, RN02, HU-220–223 line ranges in Doc_SMVR/SafeMining_VR_v1.1.md
- @.agent-docs/loto-objectives.md
- @Propuesta Simulacion LOTO.txt

## Scope
IN: S1 EPP, S2 hazards, S3 LOTO sequence, scoring, localization, HUD
OUT: Backend adapters, rendering perf (unless blocking compliance UX)

## MCP gates
1. Unity_ManageEditor → GetProjectRoot = D:/Unity/Projects/SafeMiningVR
2. Unity_ManageScene → inspect VR-LOTO-S1, S2, S3 hierarchies as needed
3. Unity_ReadConsole (Errors) → 0 before and after any fixes
4. Do NOT hand-edit .unity/.prefab YAML

## Constraints
- Industrial safety training — no arcade mechanics
- Approval threshold 60%, weights sum 100%
- S3 strict order: STOP → breaker OFF → red lock → PELIGRO NO OPERAR → START verify

## Deliverables
1. Sign-off matrix (Phases A–F) with PASS/FAIL per checklist item
2. List of code/scene gaps with file paths and RN/HU IDs
3. Optional: minimal fixes on branch with separate commits per scenario
4. Final summary: total PASS count, CRITICAL failures, recommended merge decision

## Done when
All Phase A–F items audited with evidence. CRITICAL failures documented. Console 0 errors if fixes applied.
```

---

## Template B — S3 Interactable Mass Refactor

```text
/in-cloud

## Objective
Refactor S3 LOTO interactables to unified XRI 3.3.1 patterns (grab, dwell, socket, outline hints) on branch `{branch}`.

## Branch
`refactor/s3-interactables-xri-{date}`

## Subagent
/vr-usability-interaction (primary) — /loto-compliance for pedagogical validation after implementation

## Sources
- @.cursor/rules/40-vr-interaction.mdc
- @.cursor/skills/meta-xr-interaction/SKILL.md
- @.cursor/skills/dual-mode-playtest/SKILL.md
- @.cursor/rules/30-loto-domain.mdc (S3 sequence only)
- @.agent-docs/workflows/wf-dual-mode-testing.md

## Scope
IN:
- Assets/SafeMining/LOTOSimulation/Code/Interactables/Loto*.cs
- Assets/SafeMining/LOTOSimulation/Code/Interactables/*S3*
- Scene VR-LOTO-S3 (via Unity MCP only)

OUT: Scoring weights, backend sync, lighting/perf (unless regression)

## MCP gates
1. meta_docs_search before ANY XRI API change (scope: unity, mode: verify)
2. Unity_ManageGameObject → get_components before each GO modification
3. Unity_ValidateScript after each script change
4. Unity_ReadConsole (Errors) → 0 after each batch

## Implementation requirements
- Grabs = XRGrabInteractable (no dwell on grabs)
- Buttons/hazards = dwell reticle 0.8–1.2s, Secondary #F97316
- Every interactable: ActivateFromPc() for PC playtest
- Configure(ManagerType) injection — no Awake self-wiring
- Hand tracking + controllers both work

## Deliverables
1. File change list with rationale per interactable
2. Dual-modality checklist (WF1 subset) — hands, controllers, PC mouse
3. S3 LOTO sequence still enforces strict order (coordinate with /loto-compliance)
4. MCP verification log

## Done when
All S3 interactables refactored. WF1 dual-mode checks pass. Unity_ReadConsole = 0. No XRI deprecation warnings.
```

---

## Template C — Quest 3S Performance Audit (WF4)

```text
/in-cloud

## Objective
Audit and optimize VR-LOTO-{scene} for 72 FPS on Quest 3S on branch `{branch}`.

## Branch
`perf/{scene}-quest3s-72fps-{date}`

## Subagent
/unity-performance

## Sources
- @.agent-docs/workflows/wf-build-performance.md
- @.cursor/rules/60-quest-performance.mdc
- @.cursor/rules/10-unity-architecture.mdc
- @.agent-docs/folder-structure.md

## Scope
IN: Scene VR-LOTO-{scene}, lighting, draw calls, script hot paths
OUT: LOTO procedure logic, backend, localization strings

## MCP gates
1. Unity_ManageEditor → GetProjectRoot
2. Unity_ManageScene → GetHierarchy for target scene
3. Unity_Profiler_* for frame analysis
4. meta_docs_search before Quest-specific rendering changes
5. Optional device: capture_perfetto_trace 5–10s during gameplay

## Budget targets
- 72 FPS (13.9ms frame budget)
- ≤1 key Mixed Spot + ≤2 Mixed Point fills
- No empty Update() loops
- LOD on meshes >10k tris

## Deliverables
1. Baseline vs optimized frame time table
2. Lighting inventory (count, types, shadows)
3. List of optimizations applied with file paths
4. Perfetto trace ID or profiler summary if captured
5. Risks / follow-ups

## Done when
Scene meets lighting budget. No new console errors. Measurable frame time improvement or documented blocker.
```

---

## Template D — Fase 7 Backend Adapter (Security-sensitive)

```text
/in-cloud

## Objective
Implement HTTP adapter for Fase 7 on branch `{branch}` with offline-first preservation and security review.

## Branch
`security/fase7-http-adapter-{date}`

## Subagent
/network-session (primary) — /review-security mandatory before PR

## Sources
- @.agent-docs/integration-vr-backend.md
- @.cursor/rules/70-backend-sync.mdc
- @.cursor/rules/80-security-review.mdc
- @.agent-docs/workflows/wf-network-auth-telemetry.md
- @.agent-docs/INDEX.md → RN09, RNF08

## Scope
IN:
- Assets/SafeMining/LOTOSimulation/Code/Adapters/SafeMiningBackendAdapter.cs
- Assets/SafeMining/LOTOSimulation/Code/Data/*.cs (DTOs only if needed)
- Assets/XR Standard Menu/Code/LOTOSimulation/Managers/SyncQueueManager.cs (read-only unless bug)

OUT: IBManager core flow (must remain untouched)

## Security requirements
- No hardcoded credentials or JWT in source
- No token/password in Debug.Log
- HTTPS only (TLS 1.2+)
- Timeouts on all HTTP calls
- Payload integrity: scores computed server-side validation ready
- PII limited to participantId/sessionId

## MCP gates
1. Unity_ValidateScript on all modified scripts
2. Unity_ReadConsole → 0 errors
3. python tools/ci/validate_security_surface.py passes

## Deliverables
1. Adapter implementation with configurable base URL (no secrets in repo)
2. WF3 Phase A–D validation checklist results
3. /review-security findings table (must be 0 CRITICAL before merge)
4. PR description with threat model summary

## Done when
Adapter implements ISimulationBackendAdapter. Offline-first unchanged. Security review clean. CI green.
```

---

## Template E — PR Babysit (CI + Reviews)

```text
/babysit

## Objective
Iterate on PR `{pr-url}` until merge-ready: CI green, Bugbot clean, security review if applicable.

## Checks
1. GitHub Actions safemining-ci validate job
2. python tools/ci/run_all.py locally
3. /review-bugbot on branch changes
4. /review-security if Adapters/, SyncQueueManager, DTOs, or IBManager touched

## Done when
All CI checks pass. Zero CRITICAL/HIGH open findings. PR description updated with verification evidence.
```
