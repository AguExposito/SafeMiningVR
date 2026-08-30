# SafeMining VR — Project Overview

## Stack

- **Unity:** 6000.3.10f1
- **XR:** OpenXR + Meta OpenXR, XR Interaction Toolkit 3.3.1
- **Target device:** Meta Quest 3S (Android ARM64)
- **Render pipeline:** URP 17.3

## Current state (hands-default input + VR/PC UI)

- **Roadmap:** Fases 0–6 + visual polish. Input today: hand tracking default on `PlayerMR`, auto visual switch to Touch (`LotoInputModalityVisuals`). Target: dual PC/VR prefabs + selector — [player-rig-baseline.md](player-rig-baseline.md). JWT/online remains Fase 7.
- **Docs:** [loto-simulation-handbook.md](loto-simulation-handbook.md), [loto-asset-list.md](loto-asset-list.md), [player-rig-baseline.md](player-rig-baseline.md).
- **Shell:** XR Standard Menu framework (`Assets/XR Standard Menu/`)
- **Entry scene:** `StandardMainMenu.unity` (build index 0). World-space menu is XR-clickable; **mouse enabled** in Play Mode.
- **Simulation scenes:** `VR-LOTO-S1` (1, briefing → HUD 1/3 + locker EPP), `VR-LOTO-S2` (2, HUD 2/3 + inspection yard), `VR-LOTO-S3` (3, HUD 3/3 + LOTO station → session summary).
- **HUD:** World-space `PREF_LotoPlayerHUD` and `PREF_LotoBriefingPanel` (scale 0.001, `TrackedDeviceGraphicRaycaster` + `GraphicRaycaster`, follow ~1.75 m). Raycasts only on buttons. Session summary / exit confirm mutually exclusive.
- **Bootstrap:** `LotoScenarioSceneBootstrap` on each S scene with serialized HUD refs + runtime PC cheats driver. Shared CC bridge: `Util_LotoSimulationBridge`.
- **PC playtest:** XRI Interaction Simulator auto-instantiate (Editor); Inspector bools / ContextMenus on `LotoPcPlaytestDriver`.
- **Verification:** Unity MCP connected to `D:/Unity/Projects/SafeMiningVR`. Config: `.cursor/mcp.json`. Rules: `.cursor/rules/02-mcp-bindings.mdc`

## Architecture

- **ControlCenter:** Singleton hub, DontDestroyOnLoad
- **Managers:** PersistantDataManager, ScoreManager, IBManager, LanguageManager, SyncQueueManager, SimulationSessionManager, SimulationOrchestratorManager
- **New code:** `Assets/SafeMining/LOTOSimulation/`

## Branding

- **Company:** SafeMining VR
- **Colors:** Primary `#1A365D`, Secondary `#F97316`, Tertiary `#334155`
- **Config:** `Assets/Resources/BrandConfig.asset`

## Documentation

- Functional spec: `Propuesta Simulacion LOTO.txt`
- Project report: `Doc_SMVR/SafeMining_VR_v1.1.md`
- EA models: `Doc_SMVR/DiagraSafeMining_V4.eap` (reference only)
