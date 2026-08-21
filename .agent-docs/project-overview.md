# SafeMining VR — Project Overview

## Stack

- **Unity:** 6000.3.10f1
- **XR:** OpenXR + Meta OpenXR, XR Interaction Toolkit 3.3.1
- **Target device:** Meta Quest 3S (Android ARM64)
- **Render pipeline:** URP 17.3

## Current state (visual polish + PC playtest post Fase 6)

- **Roadmap:** Fases 0–6 complete. Visual polish: URP Lit mats S1/S3, HUD non-overlap + TMP channels, underground Mixed lighting, QuickOutline→URP + pedagogical `LotoOutlineHint`, XR Interaction Simulator + `LotoPcPlaytestDriver`. JWT/online remains Fase 7.
- **Docs:** [loto-simulation-handbook.md](loto-simulation-handbook.md), [loto-asset-list.md](loto-asset-list.md).
- **Shell:** XR Standard Menu framework (`Assets/XR Standard Menu/`)
- **Entry scene:** `StandardMainMenu.unity` (build index 0). LOTO carousel title uses loc `loto_simulation` («Bloqueo/Etiquetado de Energía»).
- **Simulation scenes:** `VR-LOTO-S1` (1, briefing → HUD 1/3 + locker EPP), `VR-LOTO-S2` (2, HUD 2/3 + inspection yard), `VR-LOTO-S3` (3, HUD 3/3 + LOTO station → session summary).
- **Legacy hub:** `VR-LOTO-Placeholder.unity` remains on disk, **out of Build Settings**
- **HUD:** World-space `PREF_LotoPlayerHUD` and `PREF_LotoBriefingPanel` (scale 0.001, `TrackedDeviceGraphicRaycaster`, follow ~1.75 m). Session summary / exit confirm are mutually exclusive; active objective chrome hides while those panels show.
- **Bootstrap:** `LotoScenarioSceneBootstrap` with serialized HUD refs + runtime PC cheats driver. `LotoHubBootstrap` stays on Placeholder only.
- **PC playtest:** XRI Interaction Simulator auto-instantiate (Editor); Inspector bools / ContextMenus on `LotoPcPlaytestDriver`.
- **Verification:** Unity MCP connected to `D:/Unity/Projects/SafeMiningVR`. Rule: `Assets/.cursor/rules/safemining-mcp-console.mdc`

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
