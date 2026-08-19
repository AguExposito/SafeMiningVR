# SafeMining VR — Project Overview

## Stack

- **Unity:** 6000.3.10f1
- **XR:** OpenXR + Meta OpenXR, XR Interaction Toolkit 3.3.1
- **Target device:** Meta Quest 3S (Android ARM64)
- **Render pipeline:** URP 17.3

## Current state (Fase 5 complete — E2E + resumen RN01/RN02)

- **Roadmap:** Fase 0–5 complete. Fase 5: local `IntentoCapacitacionCapacitado` across S1→S2→S3, in-sim summary (score 0–100, Aprobado/No aprobado at threshold **60**, five-objective RN01 breakdown), offline queue retained 24h, abandon records partial score. Quest QA is Fase 6; JWT/online is Fase 7.
- **Shell:** XR Standard Menu framework (`Assets/XR Standard Menu/`)
- **Entry scene:** `StandardMainMenu.unity` (build index 0). LOTO carousel title uses loc `loto_simulation` («Bloqueo/Etiquetado de Energía»).
- **Simulation scenes:** `VR-LOTO-S1` (1, briefing → HUD 1/3 + locker EPP), `VR-LOTO-S2` (2, HUD 2/3 + inspection yard), `VR-LOTO-S3` (3, HUD 3/3 + LOTO station → session summary).
- **Legacy hub:** `VR-LOTO-Placeholder.unity` remains on disk, **out of Build Settings**
- **HUD:** World-space `PREF_LotoPlayerHUD` and `PREF_LotoBriefingPanel` (scale 0.001, `TrackedDeviceGraphicRaycaster`, no CanvasScaler). Session summary panel is authored on the HUD prefab.
- **Bootstrap:** `LotoScenarioSceneBootstrap` with serialized HUD refs; S1 serializes `LotoScenario1Controller`; S2 serializes `LotoScenario2Controller`; S3 serializes `LotoScenario3Controller`. `LotoHubBootstrap` stays on Placeholder only.
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
