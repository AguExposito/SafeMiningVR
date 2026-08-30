# Audit Alignment Baseline — Fase 0

> **S1 scene wiring (post-Cabin art):** Prefer [`s1-scene-inventory.md`](s1-scene-inventory.md) (2026-08-30). It supersedes S1-03 / S1-04 and related VR-LOTO-S1 PASS rows below that reflected prefab/code — live MCP shows Continue / Reject / Outline absent and `rejectZoneAnchor` null.

## Metadata

| Field | Value |
|-------|-------|
| Date | 2026-08-29 |
| Commit | `ede4e6250af7a525803e882a37cf374d356080ac` |
| Unity | 6000.3.10f1 (`D:/Unity/Editors/6000.3.10f1/Editor/Unity.exe`) |
| Project root | `D:/Unity/Projects/SafeMiningVR` |
| Auditor | Agent (read-only Fase 0) |
| Active scene at close | `VR-LOTO-S3` |

## Executive summary

| Metric | Count |
|--------|-------|
| PASS | 62 |
| GAP | 28 |
| BLOCKED | 6 |
| N/A | 4 |

**Top 5 P0 (start Fase 1–3 early):**

1. ~~**PERF-S1-LIGHT**~~ — CLOSED Fase 5 (3 lights/scene S1–S3).
2. **VR-01** — Sin componente dwell-time reticle en triggers (regla 40-vr-interaction).
3. **ARCH-03 / ARCH-02** — `LotoS*EnvironmentBuilder` + `LotoHubBootstrap.cs` código legacy sin uso en escenas build.
4. **BRAND-01** — 0 instancias `BrandApplicator`; colores HUD hardcodeados en prefab (visualmente correctos pero no vía `Brand.*`).
5. **LOC-S2-LABELS** — `loto_s2_report_signage` / `loto_s2_false_positive` en labels estáticos del yard (misassigned).

**Distribución GAP por fase target:**

| Fase | GAP count |
|------|-----------|
| Fase 1 — Arquitectura | 8 |
| Fase 2 — Brand/Loc | 7 |
| Fase 3 — VR | 3 |
| Fase 4 — Escenas | 6 |
| Fase 5 — Assets/Perf | 4 |

**Hipótesis roadmap confirmadas:** dual bootstrap inactivo en build ✓; EnvironmentBuilder muerto ✓; BrandApplicator ausente ✓; dwell reticle ausente ✓; loop gameplay offline jugable ✓.

**Hipótesis refutadas:** bootstrap no presente en escenas (está en `LOTO_S1/2/3Root`) ✓; prefabs finales no usados (S1–S3 usan `PREF_S*`) ✓.

---

## CI & MCP gate results

| Gate | Result |
|------|--------|
| `Unity_ManageEditor` GetProjectRoot | PASS — `D:/Unity/Projects/SafeMiningVR` |
| `Unity_GetConsoleLogs` Errors (idle) | PASS — 0 errors |
| `python tools/ci/run_all.py` | PASS — workspace, LOTO matrix (5 obj, 100%, threshold 60), C# sanity (59 files) |
| Build settings order | PASS — Menu(0), S1(1), S2(2), S3(3); Placeholder not in build |
| Quest device / adb | PARTIAL — metavr OK (`340YC20G8W04C3`); shell adb not in PATH; Perfetto needs adb path |
| APK build (WF4 A4) | PARTIAL — APK on disk; fresh build via `LotoFase6QuestBuildTool` recommended |

---

## WF4 results (A/B/C/D/F)

### A — Build settings

| Setting | Required | Actual | Estado |
|---------|----------|--------|--------|
| Application ID | `com.safeminingvr.sim` | `com.safeminingvr.sim` | PASS |
| Min API | 32 | 32 | PASS |
| Target API | 34 | 34 | PASS |
| Android arch | ARM64 | `AndroidTargetArchitectures: 2` (ARM64) | PASS |
| Scripting backend | IL2CPP | `Android: 1` (IL2CPP) | PASS |
| Build scenes | 4 ordered | 4 ordered | PASS |
| Placeholder in build | No | No | PASS |
| Vulkan / SPI | Vulkan | `m_APIs: 15000000` (Vulkan on Android) | PASS |
| OpenXR render mode | Single Pass Instanced | Not verified via MCP (manual ProjectSettings) | BLOCKED |

### B — Lighting budget

| Scene | Light components | Budget 1+2 | Estado |
|-------|------------------|------------|--------|
| VR-LOTO-S1 | 3 | PASS | **PASS** (Fase 5: was 9) |
| VR-LOTO-S2 | 3 | PASS | **PASS** (Fase 5: was 4) |
| VR-LOTO-S3 | 3 | PASS | **PASS** (Fase 5: was 4) |

### C — Script performance (SafeMining LOTO Code)

| Check | Result |
|-------|--------|
| Empty `Update`/`FixedUpdate` | PASS — none empty in LOTO Code |
| `FindObjectOfType` in hot paths | PASS — only Debug, Start, bootstrap, continue interactables |
| `ControlCenter.Instance` per-frame | PASS — not in Update in SafeMining LOTO |
| `GetComponent` in Update | PASS — none in Update loops |
| `PlayerHudManager.Update` | N/A — XR Menu; timer/subtitle (acceptable) |

### D — Asset optimization

| Check | Result |
|-------|--------|
| `PREF_PH_*` in build scenes | PASS — 0 references in `.unity` build scenes |
| `PREF_PH_*` on disk | PASS — retired Fase 5 (0 refs in build scenes) |
| Duplicates Scenery2 | PASS — `Binary_35 - copia` removed Fase 5 |
| Textures >2048 in LOTOSimulation | Not exhaustively scanned | BLOCKED |
| HUD canvas scale | GAP minor — `0.0007` vs spec ~`0.001` |

### E — Runtime FPS (Quest)

BLOCKED — no Quest 3S / adb.

### F — Console hygiene (Editor idle)

PASS — 0 errors before Play Mode.

---

## WF1 results

### Phase A — PC Play Mode

| Test | Result | Evidence |
|------|--------|----------|
| A1 Menu launch | BLOCKED | Not Play-tested from menu in this session |
| A2 Full flow menu→S1 | BLOCKED | Requires manual carousel + CC bootstrap |
| A3 Cheat Numpad 1 briefing | PARTIAL | Direct S1: orchestrator null → dismiss no-op |
| A3 Cheat 2 equip 5/5 | **PASS** | `EppEquipmentManager`: `EquippedMandatoryCount=5`, `IsReadyToAdvance=true` |
| A3 Cheat 3 continue S1→S2 | BLOCKED | Needs `ControlCenter` + orchestrator scene load |
| A3 Cheats 4–7 | BLOCKED | Same CC dependency for transitions/summary |
| A6 Direct S1 launch | PASS | Scene loads; expected CC null warning |
| S3 `forceIncompleteStart` cheat | PASS (code path) | `TryStartVerification` → `loto_s3_alert_incomplete` |
| S3 `runLotoHappyPath` cheat | PASS (invoked) | No exceptions; full subtitle path needs CC |

Play Mode errors during tests: **0**.

Pre-existing warnings: XR missing scripts, ARFoundation subsystems, `[LOTO] ControlCenter.Instance is null` on direct scene launch (expected).

### Phase B — Quest 3S

PARTIAL (2026-08-29) — Quest 3S `340YC20G8W04C3` via metavr; APK install+launch PASS; logcat Unity E=0; B3–B10 in-headset pending; Perfetto blocked (adb path).

---

## WF2 results

### Phase A — Philosophy

| Check | Result |
|-------|--------|
| Countdown / combos / leaderboard | PASS — no matches in SafeMining code |
| Pedagogical errors via `loto_*` | PASS — managers use `Translate("loto_*")` |
| Camera shake on errors | PASS — not implemented |

### Phase B — S1 EPP (prefab + runtime cheat)

| Check | Result |
|-------|--------|
| 5 mandatory types in locker | PASS — helmet, glasses, vest, gloves, boots (good + defect slots) |
| Defect variants | PASS — helmet, glasses, gloves, boots defect prefabs wired |
| Optional items | PASS — respirator, hearing in interactable prefabs |
| Reject bin | PASS — `RejectBin` anchor on `EppEquipmentManager` |
| Gate 5/5 continue | PASS — `IsReadyToAdvance` after cheat |
| Wear marks (no green=good) | PASS — defect cues via builder/prefab |

### Phase C — S2 Hazards

| Hazard | Prefab `hazardType` | Result |
|--------|---------------------|--------|
| Spilled material | 0 | PASS |
| Abandoned tools | 1 | PASS |
| Fallen signage | 2 | PASS |
| Obstructed extinguisher | 3 | PASS |
| Decoys (3) | 4, 5, 6 | PASS |
| 90s hint timeout | `HazardHintTimeoutSeconds` in constants | PASS (code) |
| Decoy outline exclusion | Code in `HazardInspectionManager` | PASS (code) |

### Phase D — S3 LOTO

| Check | Result |
|-------|--------|
| STOP/START/breaker/hasp/locks/tags | PASS — prefabs + `LotoSequenceManager` wired |
| Red / blue / yellow locks | PASS — `lockType` 0/1/2 |
| Tag danger + distractors | PASS — `tagType` 0–3 |
| Malo 1 incomplete START | PASS — code + qa-quest-fase6 prior |
| Malo 2 no gloves | PASS — code `loto_s3_alert_no_gloves` |
| World labels locks/tags | GAP — no `locKey` on S3 interactable prefabs |

### Phase E — Scoring

| Check | Result |
|-------|--------|
| Weights 20/10/25/35/10 | PASS — `LotoObjectiveIds.cs` |
| Threshold 60 | PASS |
| False-positive penalty | PASS — `LotoSessionScore.ComputePartial` |

### Phase F — UI & localization

| Check | Result |
|-------|--------|
| Hardcoded Spanish in SafeMining `.cs` | PASS — 0 matches |
| es/en `loto_*` keys | PASS — 75 / 75 |
| World Space HUD | PASS — `m_RenderMode: 2` |
| Brand via `BrandApplicator` | GAP — 0 instances |

---

## Architecture & dead code inventory

| ID | Finding | Estado | Fase | P |
|----|---------|--------|------|---|
| ARCH-01 | `LotoScenarioSceneBootstrap` on `LOTO_S1/2/3Root` | PASS | — | — |
| ARCH-02 | `LotoHubBootstrap.cs` exists; 0 scene refs | GAP | 1 | P1 |
| ARCH-03 | `LotoS*EnvironmentBuilder` never `Build()` in production | GAP | 1 | P1 |
| ARCH-04 | Multi-scene orchestrator path only in build | PASS | — | — |
| ARCH-05 | Managers split SafeMining vs XR Menu; `folder-structure.md` stale | GAP | 1 | P2 |
| ARCH-06 | `ControlCenter.persistantDataManager` setter recursive | GAP | 1 | P2 |
| ARCH-07 | No `GetManager<T>()` | GAP | 1 | P2 |
| ARCH-08 | Triplicated SetObjective/EmitEvent/subtitle in S1/S2/S3 managers | GAP | 1 | P1 |
| BACK-01 | `SafeMiningBackendAdapter` stub (`IsAvailable=false`) | GAP | 7 | P3 |
| BACK-02 | `SyncQueueManager` offline queue present | PASS | — | — |

---

## Per-scene MCP inventory

### StandardMainMenu (build 0)

| Item | Estado |
|------|--------|
| `loto_simulation` translationID | PASS |
| `BrandApplicator` | GAP — not found |
| LOTO carousel → build 1 | PASS (handbook + ScoreManager) |

### VR-LOTO-S1

| GameObject | Components | Estado |
|------------|------------|--------|
| `LOTO_S1Root` | Bootstrap, S1Controller, PlayerHudManager, LotoPcPlaytestDriver | PASS |
| `PREF_S1_Locker` | EppEquipmentManager, 12 children | PASS |
| `PREF_LotoPlayerHUD` / Briefing | World space, Util_PanelFollower | PASS |
| `PlayerMR` | LotoInputModalityVisuals, XRInputModalityManager, teleport, PC drivers | PASS (modality); comfort vignette GAP — [player-rig-baseline.md](player-rig-baseline.md) |
| `TeleportFloor` | Present in hierarchy | PASS |

### VR-LOTO-S2

| GameObject | Components | Estado |
|------------|------------|--------|
| `LOTO_S2Root` | Bootstrap, S2Controller, HUD, playtest driver | PASS |
| `PREF_S2_InspectionYard` | HazardInspectionManager, 7 hazard spots | PASS |

### VR-LOTO-S3

| GameObject | Components | Estado |
|------------|------------|--------|
| `LOTO_S3Root` | Bootstrap, S3Controller, HUD, playtest driver | PASS |
| `PREF_S3_ConveyorStation` | LotoSequenceManager, belt, hasp, lock socket | PASS |

---

## Asset & localization gaps

| ID | Issue | Fase | P |
|----|-------|------|---|
| LOC-01 | es/en parity 75 keys | PASS | — |
| LOC-02 | Orphan keys: `loto_placeholder_*`, `loto_simulation_summary`, `loto_s3_summary_body`, `loto_s1_step_reject` | 2 | P2 |
| LOC-S2 | Static labels use report/false-positive keys incorrectly | 2 | P1 |
| LOC-S3 | `loto_lock_*`, `loto_tag_*` in XML but not wired on prefabs | 2 | P1 |
| BRAND-01 | No `BrandApplicator`; HUD uses hardcoded RGB matching palette | 2 | P1 |
| ASSET-01 | 19 `PREF_PH_*` placeholders unused in scenes | 5 | P2 |
| ASSET-02 | `Models/Scenery` / `Scenery2` legacy, no LOTO code refs | 5 | P2 |
| ASSET-03 | Scenery2 duplicate assets untracked | 5 | P2 |

---

## Full gap matrix

| ID | Escena | Dominio | Requisito | Evidencia | Estado | Fase | P |
|----|--------|---------|-----------|-----------|--------|------|---|
| ARCH-01 | All S | Arch | Bootstrap canónico | MCP `LOTO_S*Root` | PASS | — | — |
| ARCH-02 | — | Arch | No LotoHubBootstrap in scenes | Grep scenes | PASS | — | — |
| ARCH-02b | — | Arch | Remove orphan LotoHubBootstrap.cs | File exists | GAP | 1 | P1 |
| ARCH-03 | — | Arch | No EnvironmentBuilder in prod | Grep Build() | GAP | 1 | P1 |
| ARCH-05 | — | Arch | Doc matches manager locations | folder-structure.md | GAP | 1 | P2 |
| ARCH-06 | — | Arch | CC setter not recursive | ControlCenter.cs:24 | GAP | 1 | P2 |
| ARCH-07 | — | Arch | GetManager pattern | Grep | GAP | 1 | P2 |
| ARCH-08 | — | Arch | Shared simulation bridge | 3 managers duplicate | GAP | 1 | P1 |
| VR-01 | S1/S2/S3 | VR | Dwell reticle 0.8–1.2s | LotoDwellReticle.cs + 16 prefab instances | PASS | — | — |
| VR-02 | All S | VR | Outline Secondary #F97316 w~4 | LotoOutlineHint.cs | PASS | — | — |
| VR-03 | All S | VR | Hand-first modality | PlayerMR MCP | PASS | — | — |
| VR-04 | All S | VR | HUD raycast buttons only | Handbook | PASS | — | — |
| VR-05 | S3 | VR | Malo 1/2 alerts | LotoSequenceManager keys | PASS | — | — |
| BRAND-01 | HUD | Brand | BrandApplicator on HUD/briefing | Grep prefabs | GAP | 2 | P1 |
| BRAND-02 | HUD | Brand | Colors via Brand.* | Hardcoded in prefab RGB | GAP | 2 | P1 |
| LOC-01 | — | Loc | es/en parity | 75/75 keys | PASS | — | — |
| LOC-S2 | S2 | Loc | Correct locKeys on yard labels | PREF_S2 prefab | GAP | 2 | P1 |
| LOC-S3 | S3 | Loc | lock/tag world labels | No locKey on S3 prefabs | GAP | 2 | P1 |
| SCORE-01 | — | Score | Weights 100% | LotoObjectiveIds | PASS | — | — |
| SCORE-02 | — | Score | Threshold 60 | LotoObjectiveIds | PASS | — | — |
| SCORE-03 | S2 | Score | Decoy penalty formula | LotoSessionScore | PASS | — | — |
| PERF-S1 | S1 | Perf | Light budget | 3 Light components (1 Spot + 2 Point) | PASS | — | — |
| PERF-S2 | S2 | Perf | Light budget | 3 Light components | PASS | — | — |
| PERF-S3 | S3 | Perf | Light budget | 3 Light components | PASS | — | — |
| PERF-APK | — | Perf | Quest 72 FPS | PARTIAL — install/launch OK; Perfetto adb blocked; APK rebuild pending | PARTIAL | 6 | — |
| MENU-01 | Menu | Menu | loto_simulation key | StandardMainMenu | PASS | — | — |
| S1-01 | S1 | S1 | Bootstrap wired | MCP | PASS | — | — |
| S1-02 | S1 | S1 | 5 mandatory EPP | Prefab + cheat | PASS | — | — |
| S1-03 | S1 | S1 | Defect + reject | Prefab slots | PASS | — | — |
| S1-04 | S1 | S1 | Gate continue 5/5 | IsReadyToAdvance | PASS | — | — |
| S2-01 | S2 | S2 | 4 real hazards | hazardType 0–3 | PASS | — | — |
| S2-02 | S2 | S2 | 3 decoys | hazardType 4–6 | PASS | — | — |
| S2-03 | S2 | S2 | Gate 4/4 | HazardInspectionManager | PASS | — | — |
| S3-01 | S3 | S3 | LOTO sequence manager | MCP | PASS | — | — |
| S3-02 | S3 | S3 | Wrong lock/tag rejection | Code + prefabs | PASS | — | — |
| S3-03 | S3 | S3 | Zero energy verify | LotoSequenceManager | PASS | — | — |
| WF1-FLOW | All | VR | Full menu→S3 PC cheats | LotoFase4MenuFlowRunner Editor Play | PASS | — | — |
| WF1-B | Quest | VR | Device modality/dwell/grab | Install+launch+logcat PASS; B3–B10 manual pending | PARTIAL | 6 | — |
| HU221-CA1 | S1 | S1 | Locker with EPP | PREF_S1_Locker | PASS | — | — |
| HU221-CA4 | S1 | S1 | Defect feedback | EppEquipmentManager | PASS | — | — |
| HU221-CA5 | S1 | S1 | Block without 5/5 | TryContinue gate | PASS | — | — |
| HU221-CA6 | S1 | S1 | Optional EPP non-blocking | EppTypeRules | PASS | — | — |
| HU222-CA3–6 | S2 | S2 | Report 4 hazards | Prefab types | PASS | — | — |
| HU222-CA7 | S2 | S2 | Decoy penalty | HazardInspectionManager | PASS | — | — |
| HU222-CA8 | S2 | S2 | Hint on missing hazard | 90s timeout | PASS | — | — |
| HU223-CA2–9 | S3 | S3 | LOTO order + verify | LotoSequenceManager | PASS | — | — |
| HU223-CA8 | S3 | S3 | Malo 1 START incomplete | Code + prior QA | PASS | — | — |
| HU223-CA12 | S3 | S3 | Malo 2 no gloves | Code | PASS | — | — |
| BACK-01 | — | Network | HTTP adapter | Stub only | GAP | 7 | P3 |

---

## Prioritized backlog (P0 → P3)

### P0

1. ~~Implement dwell reticle component (Fase 3) — VR-01~~ CLOSED
2. ~~Consolidate S1 lighting to budget (Fase 5) — PERF-S1-LIGHT~~ CLOSED

### P1

3. Remove/quarantine `LotoHubBootstrap`, EnvironmentBuilders (Fase 1)
4. Extract `Util_LotoSimulationBridge` (Fase 1)
5. Wire `BrandApplicator` + fix S2/S3 locKeys (Fase 2)
6. S2/S3 light budget cleanup (Fase 5)
7. Validate full menu→S3 flow with CC in Editor (Fase 4 / WF1)

### P2

8. Update `folder-structure.md`; fix CC setter; optional `GetManager<T>` (Fase 1)
9. Retire `PREF_PH_*`, Scenery duplicates, orphan loc keys (Fase 5 / 2)
10. HUD canvas scale normalize to 0.001 (Fase 2)

### P3

11. `SafeMiningBackendAdapter` HTTP (Fase 7)
12. Rename `AchiveObjective` typos (defer)

---

## Recommended Fase 1 entry criteria

1. This document committed to `.agent-docs/audit-alignment-baseline.md`
2. P0 items assigned to Fase 3 and Fase 5 (can parallelize after Fase 1 arch cleanup)
3. No gameplay file changes in Fase 0 commit (documentation only)

---

## Sign-off

| Workflow | Result | Notes |
|----------|--------|-------|
| B — Lighting budget | Editor | 2026-08-29 | PASS — 3 lights/scene S1–S3 |
| WF1 A | PASS | Full menu→S3 flow via `LotoFase4MenuFlowRunner`; returns to menu build 0 |
| WF1 B | PARTIAL | Install+launch+logcat PASS; VR checklist manual |
| WF2 A/E/F | PASS | |
| WF2 B/C/D | PASS (code+prefab) | Runtime gates need menu flow |

**Fase 0 complete.** Ready for Plan Fase 1.

## Fase 1 applied (2026-08-29)

| ID | Change | Status |
|----|--------|--------|
| ARCH-02b | Removed `LotoHubBootstrap.cs`, `LotoHubUiBuilder.cs` | CLOSED |
| ARCH-03 | Removed `LotoS*EnvironmentBuilder`; prefabs are source of truth | CLOSED |
| ARCH-05 | Removed `VR-LOTO-Placeholder.unity` | CLOSED |
| ARCH-06 | Added `Util_LotoSimulationBridge` (SetObjective, EnqueueEvent, NotifySubtitle) | CLOSED |
| ARCH-08 | Fixed `ControlCenter.persistantDataManager` recursive setter | CLOSED |

Remaining P0 from baseline: none (Fase 5 closed lighting).

## Fase 2 applied (2026-08-29)

| ID | Change | Status |
|----|--------|--------|
| BRAND-01 | `BrandApplicator` on HUD, briefing, objective row prefabs | CLOSED |
| BRAND-02 | Colors via `Brand.*` at runtime; HUD canvas scale 0.001 | CLOSED |
| LOC-S2 | Static labels use `loto_s2_label_extinguisher` / `loto_s2_label_signage` | CLOSED |
| LOC-S3 | `LotoWorldLabel` on 7 lock/tag prefabs with `loto_lock_*` / `loto_tag_*` | CLOSED |
| LOC-02 | Removed orphan `loto_placeholder_*`, `loto_s1_step_reject`, `loto_simulation_summary`, `loto_s3_summary_body` | CLOSED |

## Fase 3 applied (2026-08-29)

| ID | Change | Status |
|----|--------|--------|
| VR-01 | `LotoDwellReticle` on trigger interactables (dwell 1s, Secondary fill, select guard) | CLOSED |
| VR-01 | Wired S1 continue, S2 hazards+continue, S3 panel/breaker/buttons | CLOSED |
| VR-01 | `LotoFase3PrefabSetupTool` — 16 reticle components across 3 scenario prefabs | CLOSED |
| VR-03 | `LotoInputModalityVisuals` on `PlayerMR` (MCP verify) | PASS (modality); TunnelingVignette provider refs null → GAP, see [player-rig-baseline.md](player-rig-baseline.md) |

### WF1 post-Fase 3

| Test | Result | Evidence |
|------|--------|----------|
| A1–A2 Menu→S1 flow | PARTIAL | CC bootstrap path unchanged; manual Play recommended |
| A3 PC cheats (Numpad 1–7) | PASS (code) | `ActivateFromPc()` bypass preserved on all 6 trigger scripts |
| A5 XR Simulator dwell | PASS (code+prefab) | `LotoDwellReticle` on 16 triggers; hover fill + select guard |
| A6 Direct S1/S2/S3 | PASS | 0 compile errors; CC null warning expected |
| B5 Dwell reticle Quest | BLOCKED | No Quest device |
| B6 Grab no dwell | PASS (code) | Editor tool excludes `XRGrabInteractable` / lock / tag / EPP |

## Fase 4 applied (2026-08-29)

| ID | Change | Status |
|----|--------|--------|
| WF1-FLOW | Full menu→S1→S2→S3→summary→menu validated in Editor Play Mode | CLOSED |
| MENU-CC | Renamed `ControlCenter` GO (removed trailing space) in StandardMainMenu | CLOSED |
| WF1-A3.7 | `CompleteSummary()` fallback → `CompleteSimulation()` (not `ConfirmExit`) | CLOSED |
| WF2-B–E | Runtime re-audit via automated flow: gates, scoring summary, scene transitions | PASS |
| F4-TOOL | `LotoFase4MenuFlowRunner` + Editor menu **SafeMining/LOTO/Run Fase 4 Menu Flow Test** | CLOSED |
| VR-01-fix | `LotoDwellReticle` runtime white sprite fallback (Unity 6 builtin UI path) | CLOSED |

### WF1-FLOW step results (Editor Play, CC active)

| Step | Result | Evidence |
|------|--------|----------|
| A2 CC + load S1 | PASS | `ControlCenter.Instance` present; build index 1 |
| A3.1 Dismiss briefing | PASS | Orchestrator → Active |
| A3.2 Equip 5/5 | PASS | `IsReadyToAdvance=true` |
| A3.3 Continue S1→S2 | PASS | build index 2, S2Controller.Begin |
| A3.4 Report 4 hazards | PASS | `HazardInspectionManager.IsReadyToAdvance` |
| A3.5 Continue S2→S3 | PASS | build index 3 |
| A3.6 LOTO happy path | PASS | SessionSummary state |
| A3.7 Return menu | PASS | build index 0 after `CompleteSimulation` |
| Gate S1 without 5/5 | PASS (code) | `NegativeGateRoutine` in flow runner |
| Gate S2 without 4/4 | PASS (code) | Scene unchanged when not ready |

### WF2 post-Fase 4 runtime

| Phase | Result | Notes |
|-------|--------|-------|
| B S1 EPP | PASS | Gate 5/5, defect reject, optional non-blocking (flow runner) |
| C S2 Hazards | PASS | 4/4 report + remediation wired on hazard type 0 prefab |
| D S3 LOTO | PASS | Happy path + summary; Malo paths unchanged (code) |
| E Scoring | PASS | `EndScene` + payload on summary; threshold 60 |
| F Brand/Loc | PASS | Fase 2 applied; reconfirmed in flow |

## Fase 5 applied (2026-08-29)

| ID | Change | Status |
|----|--------|--------|
| PERF-S1/S2/S3 | Strip excess lights + 3 canonical Mixed lights per scene | CLOSED |
| F5-TOOL | `LotoFase5LightingCleanupTool` + menu **Apply Fase 5 Lighting Cleanup S1-S3** | CLOSED |
| WF4-B | Light count S1 9→3, S2 4→3, S3 4→3 | PASS |
| ASSETS-PH | Retired 19 `PREF_PH_*` prefabs + MAT_PREF_PH materials | CLOSED |
| ASSETS-SCENERY | Removed Scenery2 duplicate png/mat | CLOSED |
| WF4-C | Script perf grep re-verify | PASS |
| WF4-D | Texture/static batching spot-check | PASS (no blockers) |
| WF4-E | Quest 72 FPS | PARTIAL | Perfetto adb blocked; rebuild APK post-Fase 5 |
| WF1-FLOW | Post-lighting smoke (logic unchanged) | PASS (code) |

### Light count before/after

| Scene | Before | After |
|-------|--------|-------|
| VR-LOTO-S1 | 9 (CabinLights + 3 canonical + directional) | 3 |
| VR-LOTO-S2 | 4 (3 canonical + directional) | 3 |
| VR-LOTO-S3 | 4 (3 canonical + directional) | 3 |

Remaining P0: none (lighting closed Fase 5).

## Fase 6 applied (2026-08-29)

| ID | Change | Status |
|----|--------|--------|
| F6-DEVICE | Quest 3S `340YC20G8W04C3` connected via metavr | PASS |
| F6-APK | Installed `Builds/SafeMiningVR-Quest3S-Dev.apk` (315 MB); launch OK | PARTIAL (APK dated 2026-08-20 — rebuild recommended) |
| F6-TOOL | `LotoFase6QuestBuildTool` + **Build Quest 3S Dev APK** menu | CLOSED |
| WF4-A4 | APK exists + install path verified | PARTIAL |
| WF4-E | Logcat E=0; Perfetto blocked (adb path) | PARTIAL |
| WF1-B | B1/B2/B11 automated PASS; B3–B10 manual pending | PARTIAL |
| OPENXR | `m_renderMode: 1` Single Pass Instanced; multiview regions off | PASS |

### Automated device evidence

| Check | Result |
|-------|--------|
| `metavr_device list` | Quest 3S panther, state=device |
| `metavr_app install` | com.safeminingvr.sim installed |
| `metavr_app launch` | App started |
| `get_device_logcat` Unity E | 0 lines |
| `capture_perfetto_trace` | BLOCKED — adb executable not in PATH/HZDB |

Remaining manual: in-headset happy path, WF1-B B3–B10, FPS ≥72 after fresh APK + adb for Perfetto.
