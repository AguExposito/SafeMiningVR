# VR-LOTO-S1 — Scene Inventory & Contract

Living inventory for S1 alignment roadmap. Supersedes stale S1 PASS rows in `audit-alignment-baseline.md` that reflected prefab/code, not the post-Cabin art scene.

## A. Metadata

| Field | Value |
|-------|-------|
| Date | 2026-08-30 |
| Scene | `Assets/SafeMining/LOTOSimulation/Scenes/VR-LOTO-S1.unity` |
| Build index | 1 |
| Commit HEAD | `658305c94738d7a1d077a68c330312ef4a9d0b18` |
| Unity | 6000.3.10f1 |
| MCP | `GetProjectRoot` OK; console Errors idle (no Error entries); scene loaded |

## B. Root map

| Root | Role | Policy |
|------|------|--------|
| `Cabin` (+ `cabin2`, props, `CabinLights`, doors `container_door_*`) | Art — locker room | **NO DELETE** / do not relocate meshes |
| `LOTO_S1Root` | Simulation logic hub | KEEP; modify refs only |
| `PREF_S1_Locker` | `EppEquipmentManager` + Items + sockets | KEEP; logic OK; do not delete greybox nodes |
| `PREF_LotoPlayerHUD` / `PREF_LotoBriefingPanel` | World-space UI | KEEP |
| `PlayerMR` | XR rig (hands/controllers, locomotion) | KEEP |
| `TeleportFloor` | `TeleportationArea` | KEEP |
| `XR Interaction Manager` / `EventSystem` | XR + UI input | KEEP |
| `LotoKeyIndustrial` / `LotoFillA` / `LotoFillB` | Canonical Mixed lights | KEEP |

### Logic children under `LOTO_S1Root`

| GO | Components (MCP) | Notes |
|----|------------------|-------|
| `LOTO_S1Root` | `PlayerHudManager`, `LotoScenarioSceneBootstrap` (`scenarioIndex=1`, `startWithBriefing=true`), `LotoScenario1Controller` → locker | Wired |
| `PREF_S1_Locker` | `EppEquipmentManager` (`rejectZoneAnchor=null`; body anchors assigned; controller → S1Root) | Blocker: reject |
| `Items` | Active; 10 EPP children | Vest missing here |
| `LockerSlots` | **`activeSelf=false`** | 11 locker/defect/optional sockets inactive |
| `BodyAnchors` | **`activeSelf=false`** | 5 body sockets inactive |
| `EPP_ReflectiveVest_Good` | Direct child of `LOTO_S1Root` | Outside `Items` |

## C. EPP catalog (11)

| GO | Type | Condition | Parent | Components | Action |
|----|------|-----------|--------|------------|--------|
| `EPP_Helmet_Good` | Helmet | Good | `Items` | Grab + `ref_EppItem` + `EppItemInteractable` | KEEP / MODIFY wiring |
| `EPP_SafetyGlasses_Good` | SafetyGlasses | Good | `Items` | same | KEEP / MODIFY wiring |
| `EPP_ReflectiveVest_Good` | ReflectiveVest | Good | `LOTO_S1Root` | same | MODIFY reparent |
| `EPP_Gloves_Good` | Gloves | Good | `Items` | same | KEEP / MODIFY wiring |
| `EPP_SafetyBoots_Good` | SafetyBoots | Good | `Items` | same | KEEP / MODIFY wiring |
| `EPP_Helmet_Defect` | Helmet | Defective | `Items` | same | MODIFY (grab today no-op) |
| `EPP_SafetyGlasses_Defect` | SafetyGlasses | Defective | `Items` | same | MODIFY |
| `EPP_Gloves_Defect` | Gloves | Defective | `Items` | same | MODIFY |
| `EPP_SafetyBoots_Defect` | SafetyBoots | Defective | `Items` | same | MODIFY |
| `EPP_Respirator_Good` | Respirator | Good (optional) | `Items` | same | KEEP |
| `EPP_HearingProtection_Good` | HearingProtection | Good (optional) | `Items` | same | KEEP |

Missing: `EPP_ReflectiveVest_Defect` (optional create later).

Counts: `ref_EppItem` = 11 · `XRGrabInteractable` = 11 · `EppTypedSocket` = 16 (all under inactive parents).

## D. Action matrix (Fases 1–5 contract)

| Action | Items |
|--------|-------|
| **NO DELETE** | All `Cabin/*`; graphic meshes/props; lights; `PlayerMR`; `TeleportFloor`; existing EPP meshes |
| **KEEP as-is** | Bootstrap + HUD + briefing wiring; 5 good + 4 defect + 2 optional present; teleport; XR managers |
| **MODIFY (code/refs, Fases 2–3)** | UnityEvents on `EppItemInteractable`; allow defect grab; assign `rejectZoneAnchor`; defect scoring; `BindBodyAnchors`; reparent vest under `Items`; re-enable socket logic without showing greybox art |
| **CREATE placeholder (Fase 1)** | `S1_RejectZone`; `S1_ContinueTrigger` (invisible child near `container_door_*`); outline target |
| **CREATE optional (Fase 2+)** | `EPP_ReflectiveVest_Defect` placeholder |
| **MISSING blockers** | Continue → S2; Reject bin; `LotoOutlineHint`; `rejectZoneAnchor` |

### MCP absence check (2026-08-30)

| Component / name | Count |
|------------------|-------|
| `LotoS1ContinueInteractable` | 0 |
| `RejectBin` / reject zone GO | 0 |
| `LotoOutlineHint` | 0 |
| `EppEquipmentManager.rejectZoneAnchor` | null |

## E. Interaction contract (frozen)

1. EPP: grab / pinch via `XRGrabInteractable`.
2. Continue: dwell via `XRSimpleInteractable` + `LotoDwellReticle` (not grab).
3. Outcomes go through UnityEvents → `EppEquipmentManager` (no duplicate scene logic).
4. Gate to S2: 5/5 mandatory **good** only; defect objective does not block.
5. Good mandatory: equip on grab (VR-comfortable). Defect: grab to inspect; reject on release in reject zone / Reject socket; invalid wear attempt → pedagogical subtitle.
6. Optional PPE: feedback only; never gates S2.

## F. Gaps → phase

| ID | Gap | Phase |
|----|-----|-------|
| S1-FLOW-01 | No `LotoS1ContinueInteractable` | 1 |
| S1-FLOW-02 | No RejectZone / `rejectZoneAnchor` null | 1 |
| S1-FLOW-03 | No `LotoOutlineHint` | 1 / 4 |
| S1-INT-01 | `onEquipped` empty; missing UnityEvents | 2 |
| S1-INT-02 | Defect grab is no-op in `EppItemInteractable` | 2 |
| S1-INT-03 | Vest parented under `LOTO_S1Root`, not `Items` | 2 |
| S1-RULE-01 | Defect score auto-awarded at 5/5 | 3 |
| S1-RULE-02 | Body anchors inactive; equip hides item only | 3 |
| S1-RULE-03 | `LockerSlots` / `BodyAnchors` `activeSelf=false` | 3 |
| S1-PED-01 | Outline one-at-a-time + subtitle E2E | 4 |

## G. Exit criteria (Fase 0)

- [x] Inventory reflects live MCP (Continue / Reject / Outline absent)
- [x] NO-DELETE / CREATE / MODIFY matrices explicit
- [x] Gaps numbered for Fases 1–5
- [x] INDEX + baseline pointer

**Ready for Plan Fase 1:** placeholders `S1_RejectZone` + `S1_ContinueTrigger` + outline target wired to `TryContinue` / reject.
