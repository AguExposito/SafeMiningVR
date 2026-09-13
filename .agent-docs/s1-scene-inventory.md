# VR-LOTO-S1 — Scene Inventory & Contract

Living inventory for S1 alignment. Gameplay SoT: [loto-game-board.md](loto-game-board.md). **World art frozen. HUD may change.**

> Historical PASS rows in `audit-alignment-baseline.md` are not the board contract.

## A. Metadata

| Field | Value |
|-------|-------|
| Date | 2026-08-30 (Fase 5 post-align) |
| Scene | `Assets/SafeMining/LOTOSimulation/Scenes/VR-LOTO-S1.unity` |
| Build index | 1 |
| Prior inventory commit | `1f8393e` (Fase 4) |
| Unity | 6000.3.10f1 |
| MCP | PC E2E S1→S2 PASS; Quest launch smoke PASS; console 0 Errors |

## B. Root map

| Root | Role | Policy |
|------|------|--------|
| `Cabin` (+ `cabin2`, props, `CabinLights`, doors `container_door_*`) | Art — locker room | **NO DELETE** / do not relocate meshes (**frozen**) |
| `LotoGameManager` | DDOL board (empty GO) | KEEP; do not duplicate |
| `LotoGameEventRelay` | Scene UnityEvent API (empty GO) | KEEP; do not duplicate |
| `LOTO_S1Root` | Bootstrap / HUD host if present | KEEP logic refs; do not rebuild cabin art. Gates are on `LotoGameManager` |
| `PREF_S1_Locker` | `EppEquipmentManager` presentation + Items + sockets | KEEP art; gates are on the board |
| `PREF_LotoPlayerHUD` / `PREF_LotoBriefingPanel` | World-space UI | KEEP |
| `PlayerMR` | XR rig (hands/controllers, locomotion) | KEEP |
| `TeleportFloor` | `TeleportationArea` | KEEP |
| `XR Interaction Manager` / `EventSystem` | XR + UI input | KEEP |
| `LotoKeyIndustrial` / `LotoFillA` / `LotoFillB` | Canonical Mixed lights | KEEP |

### Logic children under `LOTO_S1Root` / locker

| GO | Components (MCP) | Notes |
|----|------------------|-------|
| `LOTO_S1Root` | `PlayerHudManager`, `LotoScenarioSceneBootstrap`, `LotoScenario1Controller` | Briefing→Begin wired |
| `PREF_S1_Locker` | `EppEquipmentManager` presentation + `LotoOutlineHint` | Outlines should follow the board; art frozen |
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
| **KEEP** | Bootstrap/HUD/briefing; Fases 1–5 logic + E2E runners |
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
| S1-E2E-01 | PC menu→S1→S2 + negative gate | 5 | **CLOSED** |
| S1-E2E-02 | Quest launch smoke | 5 | **CLOSED** |

## G. Fases 0–4 exit (summary)

All FLOW / INT / RULE / PED gaps CLOSED. Cabin art intact.

## H. Fase 5 E2E results (post-align, historical)

Rows below are a 2026-08 snapshot. Runtime gates now live on `LotoGameManager` ([loto-game-board.md](loto-game-board.md)). “4/4” in this table is historical naming.

| Step | Mode | Result |
|------|------|--------|
| Build order 0–3 | Editor | PASS |
| F5.1 dismiss briefing → Active | PC Play | PASS |
| F5.2 equip 5/5 | PC Play (Numpad/cheat) | PASS |
| F5.3 reject all defects → defect scored | PC Play | PASS |
| F5.4 continue S1→S2 | PC Play | PASS |
| Gate S1 block without 5/5 | PC Play | PASS |
| Gate S2 block without 4/4 | PC Play | PASS |
| Console Errors after dwell sprite fix | PC Play | PASS (0) |
| Quest `340YC20G8W04C3` launch `com.safeminingvr.sim` | Device | PASS (no Unity:E) |

Menu: **SafeMining/LOTO/Run Fase 5 S1 E2E Test** (Play from StandardMainMenu).

### Residuals (non-blocking)

| Residual | Notes |
|----------|-------|
| Quest in-headset hands/controllers happy path | Manual WF1 Phase B; APK may need rebuild post-Fase 5 for latest C# |
| JWT / backend online | Fase 7 |
| Optional `EPP_ReflectiveVest_Defect` | Art placeholder only |
| Cabin art polish | Out of S1 align scope |

**Roadmap S1 Fases 0–5: CLOSED (post-align).**

## I. UX pass (post Fase 5) — 2026-08-31

| Feature | Behavior |
|---------|----------|
| HUD/Briefing size | Comfort scale (~0.00072) + centered follow offset |
| Hide/Show HUD | Menu = Ocultar panel; HudReopenButton = Mostrar; Salir shortcut top-right |
| Helmet | Body attach (unchanged) |
| Glasses / Vest / Gloves / Boots | Disappear on grab (no body mesh); glasses tint filter; gloves hand materials |
| Glasses grab | Larger collider + EnsureGrabInteractable |
| Hover outline | Cyan/brand Outline on all EPP when hand/controller hovers |
| Optional Respirator / Hearing | Equipable (consume visual); not required for gate |
| `S1_TeleportToNextArea` | Cyan emissive pad; activates at 5/5; teleport triggers continue → S2 |

## J. Lighting anti-flicker (2026-08-31)

| Item | S1 state |
|------|----------|
| Ceiling spots | 3 retained (`LotoKeyIndustrial` + `(1)` + `(2)`) at user positions |
| Shadow caster | **Only** canonical `LotoKeyIndustrial` (Hard shadows, intensity ≤4.2) |
| Assist spots | `(1)` / `(2)` shadowless, intensity ≤3.4 |
| Fills | `LotoFillA` + `LotoFillB`; duplicate `LotoFillA (1)` removed |
| URP Quest | `Performance URP Config` shadow distance **8 m** (was 2.5 m) |
| Lightmaps | Rebaked via **SafeMining/LOTO/Rebake S1 Lightmaps** |
| Menu | **SafeMining/LOTO/Fix Duplicate Lights S1-S2** (preserves transforms) |

## K. Post-process bloom (2026-08-31)

| Item | Value |
|------|-------|
| Profile | `Assets/Resources/VOL_LotoUnderground.asset` |
| Prefab | `PREF_LotoPostProcessVolume` (global Volume in S1–S3 roots) |
| Bloom | threshold 0.95, intensity 0.2, scatter 0.55, maxIterations 3 |
| HDR Quest | Enabled on `Performance URP Config` for emissive bloom |
| Camera | `LotoPostProcessVolumeBootstrap` enables `renderPostProcessing` on Main Camera |
| Quest renderer | `Android Preset` wired to URP `postProcessData` |
| Menu | **SafeMining/LOTO/Apply Phase 3 Post Process Setup** |

## L. Ambient dust (2026-08-31)

| Item | Value |
|------|-------|
| Prefab | `PREF_LotoAmbientDust` parented under `LotoKeyIndustrial` |
| Particles | 48 max, alpha ~0.16, slow drift + light noise, no collision/lights |
| Controller | `LotoAmbientDustController` plays only in `Active` simulation state |
| Menu | **SafeMining/LOTO/Apply Phase 4 Ambient Dust Setup** |