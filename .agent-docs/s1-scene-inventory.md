# VR-LOTO-S1 — Scene Inventory & Contract

Living inventory for S1 alignment roadmap. Supersedes stale S1 PASS rows in `audit-alignment-baseline.md` that reflected prefab/code, not the post-Cabin art scene.

## A. Metadata

| Field | Value |
|-------|-------|
| Date | 2026-08-30 (Fase 2 applied) |
| Scene | `Assets/SafeMining/LOTOSimulation/Scenes/VR-LOTO-S1.unity` |
| Build index | 1 |
| Prior inventory commit | `e3e1b77` (Fase 1) |
| Unity | 6000.3.10f1 |
| MCP | Vest under Items; API smoke defect wear-fail + reject + equip PASS |

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
| `LOTO_S1Root` | `PlayerHudManager`, `LotoScenarioSceneBootstrap`, `LotoScenario1Controller` | Wired |
| `PREF_S1_Locker` | `EppEquipmentManager` (`rejectZoneAnchor` → `S1_RejectZone`) | Begin wires child interactables |
| `S1_RejectZone` | BoxCollider, `XRSocketInteractor`, `EppTypedSocket` (Reject); MeshRenderer off | Placeholder under locker |
| `S1_ContinueTrigger` | BoxCollider, `XRSimpleInteractable`, `LotoDwellReticle`, `LotoS1ContinueInteractable` + `onContinue`; MeshRenderer off | At Cabin door bounds |
| `Items` | Active; **11** EPP children (vest included) | Fase 2 reparent done |
| `LockerSlots` / `BodyAnchors` | `activeSelf=false` | Fase 3 |
| `EPP_ReflectiveVest_Good` | Under `PREF_S1_Locker/Items` | INT-03 closed |

## C. EPP catalog (11)

5 good mandatory + 4 defect + 2 optional. Vest good under `Items`. No vest defect (optional deferred).

## D. Action matrix

| Action | Items |
|--------|-------|
| **NO DELETE** | All `Cabin/*`; graphic meshes/props; lights; `PlayerMR`; `TeleportFloor`; existing EPP meshes |
| **KEEP** | Bootstrap/HUD/briefing; Fase 1 placeholders |
| **MODIFY (Fase 3)** | Scoring defect; body bind; socket activation |
| **CREATE done (Fase 1)** | `S1_RejectZone`, `S1_ContinueTrigger` |
| **CREATE optional (later)** | `EPP_ReflectiveVest_Defect` placeholder |

## E. Interaction contract (frozen)

1. EPP: grab / pinch via `XRGrabInteractable`.
2. Continue: dwell via `XRSimpleInteractable` + `LotoDwellReticle` (not grab); `onContinue` on success.
3. Gate to S2: 5/5 mandatory **good** only.
4. Good grab → `onGrabbed` → `TryEquip` → `onEquipped`.
5. Defect grab → `onGrabbed` → `TryEquip` fail (subtitle) + remain holdable; release near reject / Reject socket → `TryReject` → `onRejected`.

## F. Gaps → phase

| ID | Gap | Phase | Status |
|----|-----|-------|--------|
| S1-FLOW-01 | No `LotoS1ContinueInteractable` | 1 | **CLOSED** — `S1_ContinueTrigger` |
| S1-FLOW-02 | No RejectZone / `rejectZoneAnchor` null | 1 | **CLOSED** — `S1_RejectZone` wired |
| S1-FLOW-03 | No outline continue target | 1 / 4 | **CLOSED (target)** — manager finds continue child; pedagogy polish → Fase 4 |
| S1-INT-01 | `onEquipped` empty; missing UnityEvents | 2 | **CLOSED** — `onGrabbed` / `onReleased` / `onEquipped` / `onRejected` |
| S1-INT-02 | Defect grab no-op semantics | 2 | **CLOSED** — wear-fail + reject path |
| S1-INT-03 | Vest outside `Items` | 2 | **CLOSED** — under `Items` |
| S1-RULE-01 | Defect score auto at 5/5 | 3 | open |
| S1-RULE-02 | Body anchors inactive / equip hide-only | 3 | open |
| S1-RULE-03 | `LockerSlots` / `BodyAnchors` inactive | 3 | open |
| S1-PED-01 | Outline one-at-a-time + subtitle E2E | 4 | open |

## G. Fase 2 exit

- [x] UnityEvents on `EppItemInteractable` + `onContinue` on continue
- [x] Defect grab wear-fail + reject; item stays holdable until rejected
- [x] Vest reparented under `Items` (11 children)
- [x] `Begin` wires interactables / continue / reject socket
- [x] API smoke: vest_under_items, defect_wear_fail, reject_ok, equip_good PASS
- [x] Cabin art intact

**Ready for Plan Fase 3:** scoring defect + body/sockets.
