# Folder Structure — SafeMining

```
Assets/SafeMining/
├── LOTOSimulation/
│   ├── Scenes/
│   │   ├── VR-LOTO-S1.unity          — build index 1 (briefing + HUD 1/3 + PREF_S1_Locker)
│   │   ├── VR-LOTO-S2.unity          — build index 2 (HUD 2/3 + PREF_S2_InspectionYard)
│   │   └── VR-LOTO-S3.unity          — build index 3 (HUD 3/3 + PREF_S3_ConveyorStation)
│   ├── Prefabs/
│   │   ├── PREF_LotoPlayerHUD.prefab
│   │   ├── PREF_LotoBriefingPanel.prefab
│   │   ├── PREF_LotoObjectiveRow.prefab
│   │   ├── PREF_S1_Locker.prefab
│   │   ├── PREF_S2_InspectionYard.prefab
│   │   └── PREF_S3_ConveyorStation.prefab
│   ├── Art/
│   │   ├── Materials/
│   │   ├── Models/
│   │   └── UI/
│   └── Code/
│       ├── Managers/
│       │   ├── LotoScenarioSceneBootstrap.cs
│       │   ├── LotoScenario1Controller.cs
│       │   ├── LotoScenario2Controller.cs
│       │   ├── LotoScenario3Controller.cs
│       │   ├── EppEquipmentManager.cs
│       │   ├── HazardInspectionManager.cs
│       │   └── LotoSequenceManager.cs
│       ├── Utils/
│       │   └── Util_LotoSimulationBridge.cs
│       ├── Data/
│       │   ├── LotoObjectiveIds.cs
│       │   ├── LotoSessionScore.cs
│       │   ├── SimulationSessionContext.cs
│       │   ├── SimulationEventRecord.cs
│       │   ├── SimulationObjectiveResult.cs
│       │   ├── SimulationResultPayload.cs
│       │   ├── PendingSyncEntry.cs
│       │   └── PendingSyncQueueData.cs
│       ├── Adapters/
│       │   ├── ISimulationBackendAdapter.cs
│       │   ├── OfflineBackendAdapter.cs
│       │   └── SafeMiningBackendAdapter.cs
│       ├── Interactables/
│       │   ├── LotoDwellReticle.cs
│       │   ├── LotoInputModalityVisuals.cs
│       │   ├── LotoOutlineHint.cs
│       │   └── *Interactable.cs (trigger + grab)
│       ├── Debug/
│       │   ├── LotoPcPlaytestDriver.cs
│       │   ├── LotoFase4MenuFlowRunner.cs   (#if UNITY_EDITOR)
│       │   ├── LotoVisualPrefabLibrary.cs   (interactables only; no PREF_PH_)
│       │   ├── LotoUndergroundLightingTool.cs (ConfigureScene API; no public duplicate menu)
│       │   ├── LotoS*SmokeTest.cs
│       │   └── *PrefabBuilder.cs (deprecated — dialog only)
│       ├── Editor/
│       │   ├── LotoFase2PrefabSetupTool.cs
│       │   ├── LotoFase3PrefabSetupTool.cs
│       │   ├── LotoFase4MenuFlowTestTool.cs
│       │   ├── LotoFase5LightingCleanupTool.cs
│       │   ├── LotoFase6QuestBuildTool.cs
│       │   ├── LotoFullSetupTool.cs
│       │   └── LotoMenuBrandSetupTool.cs
└── Shared/
    └── UI/
        ├── BrandLogo.png
        ├── Timer_frame.png
        ├── Timer_filled.png
        ├── Score_misionStatus_frame.png
        └── Score_misionStatus_filled.png
```

## XR Standard Menu (framework + LOTO orchestration)

```
Assets/XR Standard Menu/
├── Code/
│   ├── ControlCenter.cs
│   ├── Managers/
│   │   ├── PersistantDataManager.cs
│   │   ├── ScoreManager.cs
│   │   ├── LanguageManager.cs
│   │   └── SoundManager.cs
│   └── LOTOSimulation/
│       ├── Managers/
│       │   ├── SimulationOrchestratorManager.cs
│       │   ├── SimulationSessionManager.cs
│       │   ├── SyncQueueManager.cs
│       │   └── PlayerHudManager.cs
│       └── refs/
│           └── ref_PlayerHUD.cs
```

## Related

```
Assets/Resources/            — es.xml, en.xml, SimScore.asset, BrandConfig
.cursor/rules/               — Cursor rules (project gate, MCP, LOTO domain, XR, UI, perf, backend)
.cursor/skills/              — Cursor skills (procedures + subagent launch templates)
.cursor/mcp.json             — Unity MCP relay config (project-scoped)
.agent-docs/                 — This knowledge base
```

**Fase 1 (2026-08):** Removed legacy hub/builders/placeholder. Single bootstrap: `LotoScenarioSceneBootstrap`.

**Fase 2 (2026-08):** `BrandApplicator` on HUD prefabs; S2 static label keys; `LotoWorldLabel` on lock/tag prefabs; canvas scale 0.001.

**Fase 3 (2026-08):** `LotoDwellReticle` on trigger interactables; dwell 1 s radial fill; PC bypass via `ActivateFromPc()`.

**Fase 4 (2026-08):** WF1-FLOW validated menu→S3→menu; `LotoFase4MenuFlowRunner`; `ControlCenter` GO rename in menu scene.

**Fase 5 (2026-08):** Lighting budget 3 lights/scene via `LotoFase5LightingCleanupTool`; retired `PREF_PH_*` placeholders.

**Audit-fix (2026-08-30):** Removed `Models/Scenery2/`; unified lighting menus; `LotoFullSetupTool` + `LotoMenuBrandSetupTool`; deprecated scenario rebuild builders; CI `validate_loto_editor_hygiene.py` (no `PREF_PH_` in `.cs`).

**Fase 6 (2026-08):** Quest 3S device install/launch via metavr; `LotoFase6QuestBuildTool` + pre-build validation; WF1-B/WF4-E PARTIAL pending in-headset + fresh APK + Perfetto adb.
