# SafeMining VR — Project Overview

## Stack

- **Unity:** 6000.3.10f1
- **XR:** OpenXR + Meta OpenXR, XR Interaction Toolkit 3.3.1
- **Target device:** Meta Quest 3S (Android ARM64)
- **Render pipeline:** URP 17.3

## Current state (Fase 0 — roadmap v2)

- **Roadmap:** Fase 0 complete (Unity MCP + Meta VR MCP + clean console gate). Gameplay S1–S3 remains a greybox prototype pending Fase 1+
- **Shell:** XR Standard Menu framework (`Assets/XR Standard Menu/`)
- **Entry scene:** `StandardMainMenu.unity` (build index 0)
- **Simulation hub:** `VR-LOTO-Placeholder.unity` (build index 1) — briefing + HUD + S1 EPP + S2 hazard inspection + S3 LOTO (runtime greybox; not the authored-scene target)
- **Orchestration:** `SimulationOrchestratorManager` + `PlayerHudManager` + S1/S2/S3 scenario controllers + managers
- **Verification:** Unity MCP connected to `D:/Unity/Projects/SafeMiningVR` (`GetProjectRoot`, `ReadConsole` Errors = 0). Meta VR MCP `ready` (OpenXR Quest docs search). Rule: `Assets/.cursor/rules/safemining-mcp-console.mdc`

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
