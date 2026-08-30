# VR-LOTO-S1 — Scene Inventory & Contract

Living inventory for S1 alignment roadmap. Supersedes stale S1 PASS rows in `audit-alignment-baseline.md` that reflected prefab/code, not the post-Cabin art scene.

## A. Metadata

| Field | Value |
|-------|-------|
| Date | 2026-08-30 (Fase 4 applied) |
| Scene | `Assets/SafeMining/LOTOSimulation/Scenes/VR-LOTO-S1.unity` |
| Build index | 1 |
| Prior inventory commit | `5c55405` (Fase 3 fix) |
| Unity | 6000.3.10f1 |
| MCP | Outline MandatoryTypes order PASS; subtitle isError pipeline; Brand Secondary outlines |

## B. Root map

| Root | Role | Policy |
|------|------|--------|
| `Cabin` (+ `cabin2`, props, `CabinLights`, doors `container_door_*`) | Art — locker room | **NO DELETE** / do not relocate meshes |
| `LOTO_S1Root` | Simulation logic hub | KEEP; modify refs only |
| `PREF_S1_Locker` | `EppEquipmentManager` + Items + sockets + Fase 1 placeholders | KEEP |
| `PREF_LotoPlayerHUD` / `PREF_LotoBriefingPanel` | World-space UI | KEEP |
| `PlayerMR` | XR rig (hands/controllers, locomotion) | KEEP |
| `TeleportFloor` | `TeleportationArea` | KEEP |
| `XR Interaction Manager` / `EventSystem` | XR + UI input | KEEP |
| `LotoKeyIndustrial` / `LotoFillA` / `LotoFillB` | Canonical Mixed lights | KEEP |

### Logic children under `LOTO_S1Root` / locker

| GO | Components (MCP) | Notes |
|----|------------------|-------|
| `LOTO_S1Root` | `PlayerHudManager`, `LotoScenarioSceneBootstrap`, `LotoScenario1Controller` | Briefing→Begin wired |
| `PREF_S1_Locker` | `EppEquipmentManager` + `LotoOutlineHint` | Ordered outline + Brand Secondary |
| `S1_RejectZone` | Reject socket; MeshRenderer off | Fase 1 |
| `S1_ContinueTrigger` | Dwell continue + `onContinue` | Outlined at 5/5 |
| `Items` | 11 EPP | — |
| `LockerSlots` / `BodyAnchors` | Activated at Begin | Fase 3 |

## C. EPP catalog (11)

5 good mandatory + 4 defect + 2 optional. Vest under `Items`. No vest defect.

## D. Action matrix

| Action | Items |
|--------|-------|
| **NO DELETE** | All `Cabin/*`; graphic meshes/props; lights; `PlayerMR`; `TeleportFloor`; existing EPP meshes |
| **KEEP** | Bootstrap/HUD/briefing; Fases 1–4 logic |
| **CREATE done (Fase 1)** | `S1_RejectZone`, `S1_ContinueTrigger` |
| **CREATE optional (later)** | `EPP_ReflectiveVest_Defect` placeholder |

## E. Interaction contract (frozen)

1. EPP: grab / pinch via `XRGrabInteractable`.
2. Continue: dwell; `onContinue` on success.
3. Gate to S2: 5/5 mandatory good only.
4. Good grab → equip → body attach → `onEquipped`.
5. Defect: inspect (no score) → reject near zone; wear near body/`TryEquip` scores fail.
6. Defect score true only after reject-all without wear; never auto at 5/5.
7. Outline: **one** next good by `MandatoryTypes` order → then `S1_ContinueTrigger`; Brand Secondary.
8. Subtitles: fail keys use `isError` flag (es/en); HUD styles from flag, not EN substrings.
9. Anti-arcade: elapsed timer only; `timerFill` off.

## F. Gaps → phase

| ID | Gap | Phase | Status |
|----|-----|-------|--------|
| S1-FLOW-01 | Continue | 1 | **CLOSED** |
| S1-FLOW-02 | Reject zone | 1 | **CLOSED** |
| S1-FLOW-03 | Outline continue target + pedagogy | 1 / 4 | **CLOSED** |
| S1-INT-01/02/03 | UnityEvents / defect / vest | 2 | **CLOSED** |
| S1-RULE-01/02/03 | Scoring / body / sockets | 3 | **CLOSED** |
| S1-PED-01 | Outline one-at-a-time + subtitle E2E | 4 | **CLOSED** |

## G. Fase 4 exit

- [x] Outline ordered by MandatoryTypes; continue at 5/5
- [x] `LotoOutlineHint` Brand Secondary
- [x] Subtitle `isError` pipeline (bridge → orchestrator → HUD)
- [x] Fail keys S1 wired; S3 alerts / S2 false-positive also flagged
- [x] Smoke: Helmet → Glasses → Continue; DismissBriefing present
- [x] Cabin art intact

**Ready for Plan Fase 5:** full PC/Quest flow validation + merge gate.
