# LOTO Simulation Handbook

Post Fase 6 visual polish + PC playtest. JWT/online (Fase 7) is out of scope.

## What exists today

| Area | Location / notes |
|------|------------------|
| Scenes | `VR-LOTO-S1` → `S2` → `S3` (Build Settings). Menu entry: `StandardMainMenu`. |
| Prefabs | `PREF_S1_Locker`, `PREF_S2_InspectionYard`, `PREF_S3_ConveyorStation`, `PREF_LotoPlayerHUD`, `PREF_LotoBriefingPanel` |
| Materials | `Art/Materials/MAT_S1_*`, `MAT_S2_*`, `MAT_S3_*` (URP Lit) |
| Managers | `EppEquipmentManager`, `HazardInspectionManager`, `LotoSequenceManager`, `SimulationOrchestratorManager`, `PlayerHudManager` |
| Bootstrap | `LotoScenarioSceneBootstrap` on each S scene (+ runtime `LotoPcPlaytestDriver`) |
| Outline | QuickOutline shaders ported to URP; `LotoOutlineHint` driven by managers per pedagogical step |
| Scoring | RN01/RN02 via `ScoreManager` + session context; pass threshold 60 |
| Sync | Offline queue 24h via `SyncQueueManager` (Fase 7 JWT later) |
| Malo | Malo 1 incomplete START jolt; Malo 2 breaker without gloves alert |

## Player flow

1. Main menu carousel → LOTO simulation.
2. **S1 Briefing** → Begin → equip 5 mandatory EPP (reject defective via reject zone) → continue door → S2.
3. **S2** report 4 real hazards (decoys penalize) → continue → S3.
4. **S3** STOP → breaker OFF → personal lock → danger tag → START verify zero energy → session summary.
5. Abandon (menu/exit confirm) yields partial score + summary path.

## Outline hints

- Color Secondary `#F97316`, width ~4.
- One hint set at a time; cleared when the step completes.
- S1: pending good mandatory EPP → continue door when ready.
- S2: unreported real hazards → continue door when ready (no decoys).
- S3: STOP → breaker → personal lock → danger tag → START.

## Input (hands vs controllers)

- Default: **hand tracking** on `PlayerMR` (`Left/Right Hand` on, controllers off).
- `XRInputModalityManager` + [`LotoInputModalityVisuals`](Assets/SafeMining/LOTOSimulation/Code/Interactables/LotoInputModalityVisuals.cs): picking up Touch shows controller meshes and hides hands; releasing (~0.6 s hysteresis) returns to hands.
- Menu Controls help panels follow the live modality; Hands/Joysticks buttons still preview help art.
- OpenXR **Hand Interaction Profile** is enabled on Android and Standalone.

## PC playtest (no headset)

1. Play **StandardMainMenu** or S1–S3 with Game view focused. Mouse UI is enabled on the menu and LOTO EventSystems.
2. **XR Interaction Simulator** auto-instantiates in Editor (treat as joysticks). Hands remain default until simulator/controller input arrives.
3. HUD/briefing use `TrackedDeviceGraphicRaycaster` + `GraphicRaycaster`; only buttons block rays.
4. Select bootstrap → `LotoPcPlaytestDriver` Inspector bools / Context Menu **LOTO/PC/**… for scenario cheats (S1–S3 only).

## Where to maintain

| Concern | Touch |
|---------|--------|
| Scenario rules / scoring | Managers under `Code/Managers/` |
| Prefab layout / art | `Prefabs/`, `Art/Materials/` |
| Loc strings | Language assets used by `LanguageManager` (`loto_*` keys) |
| Input modality | `PlayerMR` + `LotoInputModalityVisuals` |
| HUD layers | `PlayerHudManager.ApplyState` (summary vs exit confirm mutually exclusive; active chrome hidden on overlays) |
| Underground lights | Menu **SafeMining/LOTO/Configure Underground Lighting S1-S3** or `LotoUndergroundLightingTool` |
| Sync payload | `SyncQueueManager` + `PendingSyncEntry` |
| EnvironmentBuilders | Editor/debug only — authored prefabs are source of truth |

## Lighting (Quest budget)

Per S1–S3: Directional sun disabled; custom dark ambient; 1 Mixed Spot key + up to 2 Mixed Point fills; soft shadows only on key.
