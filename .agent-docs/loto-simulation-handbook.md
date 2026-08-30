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
| Branding | `BrandApplicator` on `PREF_LotoPlayerHUD`, `PREF_LotoBriefingPanel`, `PREF_LotoObjectiveRow`; colors via `BrandConfig` at runtime |
| World labels | S2 static keys `loto_s2_label_*`; S3 lock/tag keys `loto_lock_*` / `loto_tag_*` on interactable prefabs |
| Dwell reticle | `LotoDwellReticle` on trigger interactables (hazards, buttons, breaker, continue doors); ~1 s radial fill Secondary; grabs direct |

## Player flow

**Entry path:** always launch from `StandardMainMenu` (build 0) so `ControlCenter` persists via DontDestroyOnLoad. Direct launch of S1–S3 is for visual inspection only (CC null warning expected).

1. Main menu carousel → LOTO simulation.
2. **S1 Briefing** → Begin → equip 5 mandatory EPP (reject defective via reject zone) → continue door → S2. **PC happy path validated (Fase 5)** via **SafeMining/LOTO/Run Fase 5 S1 E2E Test**.
3. **S2** report 4 real hazards (decoys penalize) → continue → S3.
4. **S3** STOP → breaker OFF → personal lock → danger tag → START verify zero energy → session summary.
5. Abandon (menu/exit confirm) yields partial score + summary path.
6. Session summary **Continue** → `CompleteSimulation()` → returns to main menu with score persisted offline.

## Outline hints

- Color Secondary `#F97316` via `Brand.GetColor(Secondary)` when BrandConfig is loaded (fallback hardcoded), width ~4.
- One hint set at a time; cleared when the step completes.
- S1: next pending good mandatory EPP in **MandatoryTypes order** (Helmet → Glasses → Vest → Gloves → Boots) → continue door when ready.
- S2: unreported real hazards → continue door when ready (no decoys).
- S3: STOP → breaker → personal lock → danger tag → START.

## Input (hands vs controllers)

- **Today:** default **hand tracking** on `PlayerMR` (`Left/Right Hand` on, controllers off).
- **Target (dual rig):** `PREF_LotoPlayerPC` + `PREF_LotoPlayerVR` + `LotoPlayerRigSelector` (`Auto` / `ForcePc` / `ForceVr`). Contract: [player-rig-baseline.md](player-rig-baseline.md).
- `XRInputModalityManager` + [`LotoInputModalityVisuals`](Assets/SafeMining/LOTOSimulation/Code/Interactables/LotoInputModalityVisuals.cs): picking up Touch shows controller meshes and hides hands; releasing (~0.6 s hysteresis) returns to hands.
- Menu Controls help panels follow the live modality; Hands/Joysticks buttons still preview help art.
- OpenXR **Hand Interaction Profile** is enabled on Android and Standalone.

## Dwell-time triggers (non-grab)

- Triggers (hazard report, STOP/START, breaker, panel door, continue doors) use [`LotoDwellReticle`](Assets/SafeMining/LOTOSimulation/Code/Interactables/LotoDwellReticle.cs): hover ray → radial fill ~1 s (Secondary `#F97316`) → action.
- Moving ray away resets fill. Instant select is blocked until dwell completes.
- Grabs (EPP, locks, tags) remain direct — no dwell reticle.
- PC cheats call `ActivateFromPc()` on each trigger script, bypassing dwell.
- Prefab batch: menu **SafeMining/LOTO/Apply Fase 3 Dwell Reticle Setup**.

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
| Input modality | Today: `PlayerMR` + `LotoInputModalityVisuals`. Target: dual prefabs — [player-rig-baseline.md](player-rig-baseline.md) |
| HUD layers | `PlayerHudManager.ApplyState` (summary vs exit confirm mutually exclusive; active chrome hidden on overlays) |
| Underground lights | Menu **SafeMining/LOTO/Configure Underground Lighting S1-S3** or `LotoUndergroundLightingTool` |
| Sync payload | `SyncQueueManager` + `PendingSyncEntry` |
| CC ↔ simulation bridge | `Util_LotoSimulationBridge` (objectives, events, subtitles) |
| HUD brand colors | `BrandApplicator` on HUD/briefing/objective row prefabs |
| S2/S3 world labels | `LotoWorldLabel` + `loto_s2_label_*` / `loto_lock_*` / `loto_tag_*` |
| Prefab brand/loc batch | Menu **SafeMining/LOTO/Apply Fase 2 Brand and Loc Prefab Setup** |
| Prefab dwell batch | Menu **SafeMining/LOTO/Apply Fase 3 Dwell Reticle Setup** |
| Menu flow QA | **SafeMining/LOTO/Run Fase 4 Menu Flow Test** (Play Mode from StandardMainMenu) |
| S1 E2E QA | **SafeMining/LOTO/Run Fase 5 S1 E2E Test** (briefing→5/5→reject→S2); see [s1-scene-inventory.md](s1-scene-inventory.md) §H |
| Lighting cleanup | **SafeMining/LOTO/Apply Fase 5 Lighting Cleanup S1-S3** |
| Quest dev APK | **SafeMining/LOTO/Build Quest 3S Dev APK** → `Builds/SafeMiningVR-Quest3S-Dev.apk` |
| Device QA | See [qa-quest-fase6.md](../.agent-docs/qa-quest-fase6.md); deploy via `metavr_app install` |
| Trigger dwell | `LotoDwellReticle` on `XRSimpleInteractable` triggers only |

## Lighting (Quest budget)

Per S1–S3: Directional sun disabled; custom dark ambient; 1 Mixed Spot key + up to 2 Mixed Point fills; soft shadows only on key.
