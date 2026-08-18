# SafeMining VR — Project Overview

## Stack

- **Unity:** 6000.3.10f1
- **XR:** OpenXR + Meta OpenXR, XR Interaction Toolkit 3.3.1
- **Target device:** Meta Quest 3S (Android ARM64)
- **Render pipeline:** URP 17.3

## Current state (Fase 4)

- **Shell:** XR Standard Menu framework (`Assets/XR Standard Menu/`)
- **Entry scene:** `StandardMainMenu.unity` (build index 0) — IBManager **Offline + Singular**; Select carga `VR-LOTO-Placeholder` (build index 1)
- **Simulation hub:** `VR-LOTO-Placeholder.unity` (build index 1) — briefing + HUD + **S1 EPP** + **S2 hazard inspection** + S3 LOTO
- **Orchestration:** `SimulationOrchestratorManager` + `PlayerHudManager` + S1/S2/S3 scenario controllers + managers
- **Gameplay:** S1 EPP → S2 report 4 hazards (decoys penalize) → S3 LOTO; full session flow S1→S2→S3 enabled

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
