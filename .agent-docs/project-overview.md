# SafeMining VR — Project Overview

## Stack

- **Unity:** 6000.3.10f1
- **XR:** OpenXR + Meta OpenXR, XR Interaction Toolkit 3.3.1
- **Target device:** Meta Quest 3S (Android ARM64)
- **Render pipeline:** URP 17.3

## Current state (Fase 1 — HUD + briefing in S1)

- **Roadmap:** Fase 0 complete. Fase 1 complete: authored HUD/briefing in `VR-LOTO-S1`. S2/S3 authored scenes and EPP gameplay are later phases.
- **Shell:** XR Standard Menu framework (`Assets/XR Standard Menu/`)
- **Entry scene:** `StandardMainMenu.unity` (build index 0)
- **Simulation scene:** `VR-LOTO-S1.unity` (build index 1) — briefing → HUD 1/3 → Menú back to menu. Vestuario EPP is not spawned yet.
- **Legacy hub:** `VR-LOTO-Placeholder.unity` remains on disk, **out of Build Settings**
- **HUD:** World-space `PREF_LotoPlayerHUD` and `PREF_LotoBriefingPanel` (scale 0.001, `TrackedDeviceGraphicRaycaster`, no CanvasScaler)
- **Bootstrap:** `LotoS1SceneBootstrap` with serialized HUD/briefing refs. `LotoHubBootstrap` stays on Placeholder only.
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
