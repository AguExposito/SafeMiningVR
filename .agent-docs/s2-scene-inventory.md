# VR-LOTO-S2S3 — Scene Inventory & Contract (S2 focus)

Living inventory for S2 (HU-222) inside the unified S2+S3 build scene. Spec still names the logical scenario `VR-LOTO-S2`; disk/build asset is `VR-LOTO-S2S3.unity`.

**KISS:** [loto-game-board.md](loto-game-board.md). Warehouse art is **frozen**. HUD may change. Do not recreate `LOTO_S2Root` / `HazardInspectionManager` / `LotoSequenceManager` as visual hubs. Interactables notify `LotoGameEventRelay` / `LotoGameManager`.

## A. Metadata

| Field | Value |
|-------|-------|
| Date | 2026-09-12 (Fase 5) |
| Scene | `Assets/SafeMining/LOTOSimulation/Scenes/VR-LOTO-S2S3.unity` |
| Build index | 2 |
| Unity | 6000.3.10f1 |
| MCP | E2E: `F5_E2E_PASS` + gate in-scene + abandon PASS; runner hardened for skipBriefing |
| Legacy | `VR-LOTO-S2.unity` removed; `VR-LOTO-S3.unity` on disk, not in Build Settings |

## B. Root map

| Root | Role | Policy |
|------|------|--------|
| `abandoned_warehouse_-_interior_scene` | Art — warehouse + conveyor kit + props | **NO DELETE** / do not relocate meshes (**frozen**) |
| `LotoGameEventRelay` | Scene UnityEvent API (empty GO) | KEEP; board persists from S1 |
| `Lights` | Scene lighting | KEEP |
| `LOTO_S2Root` | Legacy logic hub if present | **Do not recreate.** Art frozen. SoT is relay + board |
| `PREF_S2_InspectionYard` | Hazard domain + nested conveyor station | KEEP |
| `PREF_LotoPlayerHUD` / `PREF_LotoBriefingPanel` | World-space UI | KEEP |
| `PlayerMR` | XR rig (hands/controllers, locomotion) | KEEP |
| `XR Interaction Manager` / `EventSystem` | XR + UI input | KEEP |
| `PREF_LotoPostProcessVolume` | URP volume | KEEP |

### Logic under `LOTO_S2Root` (historical MCP 2026-09-12 — may be absent)

Do **not** recreate this hub to restore old managers. Current SoT is `LotoGameEventRelay` + persisted `LotoGameManager`. Art rows below stay as **frozen** catalog.

| GO | Components (MCP) | Notes |
|----|------------------|-------|
| `LOTO_S2Root` | `PlayerHudManager`, `LotoScenarioSceneBootstrap` (scenarioIndex=2, HUD+briefing+s2+s3), `LotoScenario2Controller`, `LotoScenario3Controller` | **activeSelf=true** |
| `PREF_S2_InspectionYard` | `HazardInspectionManager` (legacy if present) | Not the S2 gate |
| `LotoScenario2Controller` | zone=yard, manager=yard, conveyorMode=`PREF_S3_ConveyorStation` | Wired |
| `LotoScenario3Controller` | zone/sequence/mode on `PREF_S3_ConveyorStation` | In-scene S3 |
| `Hazard_*` ×4 | `XRSimpleInteractable` + `ref_HazardSpot` + `HazardReportInteractable` + **`LotoDwellReticle`** | Types 0–3 |
| `Decoy_*` ×3 | same stack + **`LotoDwellReticle`** | Types 4–6 |
| `ExitDoor` | `XRSimpleInteractable` + `LotoS2ContinueInteractable` + **`LotoDwellReticle`**; `interactable` wired | PASS |
| `YardFloor` | `TeleportationArea` | PASS |
| `PREF_S3_ConveyorStation` | `LotoSequenceManager` (legacy if present) | Board: `NotifyStop` … `CheckStartButton` |

Note: Do not rebuild nested scenario prefabs to “fix” logic. World art frozen. HUD may change.

## C. Hazard catalog (prefab contract)

| Object | Kind | Type | Remediation |
|--------|------|------|-------------|
| `Hazard_Spill` | Real | SpilledMaterial | Hide puddle; show cones |
| `Hazard_Tools` | Real | AbandonedTools | Move wrench to rack |
| `Hazard_Signage` | Real | FallenSignage | Upright board |
| `Hazard_Extinguisher` | Real | ObstructedExtinguisher | Hide blocking boxes |
| `Decoy_CleanFloor` | Decoy | — | Penalize only |
| `Decoy_StoredTools` | Decoy | — | Penalize only |
| `Decoy_VisibleSign` | Decoy | — | Penalize only |
| `ExitDoor` | Gate | — | Continue when `IsS2Complete` |
| `YardFloor` | Locomotion | — | `TeleportationArea` |
| Labels | Loc | `loto_s2_label_*` / `loto_s2_exit` | `LotoWorldLabel` |

Nested: `PREF_S3_ConveyorStation` + `LotoConveyorInspectionMode` (disabled during S2 inspection).

## D. Action matrix

| Action | Items |
|--------|-------|
| **NO DELETE** | Warehouse art hierarchy; Lights; `PlayerMR`; existing hazard meshes/props |
| **KEEP** | HUD/briefing (HUD may change); frozen warehouse art; dwell on existing interactables |
| **WIRE / FIX** | Roadmap S2 complete — maintain only |
| **CREATE gameplay scripts** | None (KISS) |

## E. Interaction contract (frozen — HU-222)

1. Arrive from S1 into build scene `VR-LOTO-S2S3` (logical S2).
2. Free locomotion via XRI teleport / continuous movement around stopped conveyor.
3. Report hazards via select/dwell → `LotoGameManager.NotifyTools/Sign/Puddle/Extinguisher` (or relay). Remediación visual may still live on leftover presentation scripts.
4. Decoy report → `loto_s2_false_positive`, score penalty, no remediación, does not block S3.
5. Gate: `LotoGameManager.IsS2Complete` (tools 3, sign 1, puddle 2, extinguisher 1).
6. When complete: continue → in-scene S3 briefing.
7. Objective `loto_obj_hazard_report` (25%) via `ScoreManager` when CC exists; FP reduce slice via `LotoSessionScore`.
8. Anti-arcade: no urgency timers, pop-ups, or combos.
9. Feedback: localized keys + `LotoTrainingFeedback` only.

## F. Gaps → phase

| ID | Gap | Phase | Status |
|----|-----|-------|--------|
| S2-DOC-01 | Tools/docs pointed at deleted `VR-LOTO-S2.unity` | 0 | **CLOSED** |
| S2-DOC-02 | Missing live inventory | 0 | **CLOSED** |
| S2-FLOW-00 | `LOTO_S2Root` inactive in authored scene (bootstrap may not Start) | 1 | **CLOSED** |
| S2-FLOW-01 | Full MCP verify all 4 hazards + 3 decoys + ExitDoor + teleport | 1 | **CLOSED** |
| S2-INT-01 | Add `LotoDwellReticle` on hazards + ExitDoor | 2 | **CLOSED** |
| S2-INT-02 | Remediation E2E on report | 2 | **CLOSED** |
| S2-RULE-01 | `LotoConveyorInspectionMode.Revert` + `ResetSocketsAfterInspectionMode` (no blanket tag sockets) | 3 | **CLOSED** |
| S2-RULE-02 | LockReporting + score gate Play verify | 3 | **CLOSED** |
| S2-PED-01 | Outlines, timeout hint, world labels E2E | 4 | **CLOSED** |
| S2-E2E-01 | PC menu→S1→S2→S3 + negative gate | 5 | **CLOSED** |

## G. Fase 0 exit

| Check | Result |
|-------|--------|
| Inventory published | PASS — this file |
| C# tools retargeted to `VR-LOTO-S2S3.unity` | PASS |
| Operational docs scene name aligned | PASS |
| Spec `Doc_SMVR` left as logical `VR-LOTO-S2` | intentional |

## G2. Fase 1 exit (historical snapshot)

`LOTO_S2Root` / `HazardInspectionManager` rows below are **not** the current gate. Assert `LotoGameEventRelay` + `LotoGameManager.IsS2Complete`.

| Check | Result |
|-------|--------|
| `LOTO_S2Root.activeSelf` | PASS (true, scene saved) |
| Wiring matrix 4+3+ExitDoor+YardFloor+inspection mode | PASS |
| ExitDoor `interactable` serialized | PASS |
| Smoke MainMenu → S1 E2E → `VR-LOTO-S2S3` | PASS |
| Play Mode verify root+bootstrap+HazardInspectionManager+Hazard_Spill | PASS (`F1 smoke PASS`) |

## G3. Fase 2 exit (historical)

`IsReadyToAdvance` / 4/4 rows below are leftover-manager snapshots. Current gate: `LotoGameManager.IsS2Complete`.

| Check | Result |
|-------|--------|
| `LotoDwellReticle` on 4 hazards + 3 decoys + ExitDoor | PASS (`F2 dwell PASS`) |
| Prefab yard dwell count (incl. nested S3 simples) | 15 after Fase 3 tool |
| Decoy FP (`FalsePositiveCount>=1`, visual unchanged) | PASS |
| Spill remediación (Visual off, cones on) | PASS |
| Tools remediación (wrench → ToolsSafeAnchor) | PASS |
| Signage remediación (FallenBoard upright) | PASS |
| Extinguisher remediación (BlockBoxA/B off) | PASS |
| `IsReadyToAdvance` after 4/4 | PASS (`F2 remediation PASS`) |

## G4. Fase 3 exit

| Check | Result |
|-------|--------|
| `Revert` re-enables socket components | PASS |
| `ResetSocketsAfterInspectionMode` keeps tag sockets inactive | PASS (Bugbot clear) |
| Negative gate without 4/4 | PASS |
| LockReporting after ContinueS2 | PASS |
| Locked TryReport blocks FP | PASS |
| Sockets enabled after Revert / sequence-ready | PASS (`F3 RULE PASS`) |
| Bugbot | clear |

## G5. Fase 4 exit

| Check | Result |
|-------|--------|
| World labels Spill/Tools/Signage/Extinguisher/ExitDoor keys | PASS (5/5) |
| `LotoOutlineHint` after Begin + Outline on pending hazard | PASS (`Hazard_Spill`) |
| Outline switches to `ExitDoor` after 4/4 | PASS (`hazardOutlineEnabled=0`) |
| Timeout hint `loto_s2_timeout_hint` @ `timeScale=20` | PASS (`F4_TimeoutMarker_PASS`) |
| Anti-arcade (no combo/urgency in manager) | PASS |
| Play summary | PASS (`F4 PED PASS`) |

## H. Fase 5 E2E results

| Check | Result |
|-------|--------|
| Gate S1 without 5/5 | PASS |
| Gate S2 without 4/4 (in-scene, not Briefing) | PASS (live assert + runner) |
| Decoy FP before 4/4 | PASS (`A3.3b`) |
| Report 4/4 → `IsReadyToAdvance` | PASS |
| Handoff S2→S3 Active + `LockReporting` | PASS (`skipExperimentalBriefing`) |
| Scenario index 3 + HUD | PASS |
| LOTO happy path → SessionSummary | PASS |
| Return to MainMenu | PASS (`F5_E2E_PASS`) |
| Abandon mid-S2 → menu | PASS (`F5_ABANDON_PASS`) |
| Menu | `SafeMining/LOTO/Run Fase 5 S2 Full E2E` |
