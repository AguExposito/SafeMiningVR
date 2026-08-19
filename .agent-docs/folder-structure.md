# Folder Structure — SafeMining

```
Assets/SafeMining/
├── LOTOSimulation/
│   ├── Scenes/
│   │   ├── VR-LOTO-S1.unity          — build index 1 (briefing + HUD 1/3 + PREF_S1_Locker)
│   │   ├── VR-LOTO-S2.unity          — build index 2 (HUD 2/3)
│   │   ├── VR-LOTO-S3.unity          — build index 3 (HUD 3/3)
│   │   └── VR-LOTO-Placeholder.unity — on disk, not in build
│   ├── Prefabs/
│   │   ├── PREF_LotoPlayerHUD.prefab
│   │   ├── PREF_LotoBriefingPanel.prefab
│   │   ├── PREF_LotoObjectiveRow.prefab
│   │   ├── PREF_S1_Locker.prefab
│   │   └── PREF_S3_ConveyorStation.prefab
│   ├── Art/
│   │   ├── Materials/
│   │   ├── Models/
│   │   └── UI/
│   └── Code/
│       ├── SafeMining.LOTOSimulation.asmdef
│       ├── Managers/
│       │   ├── LotoScenarioSceneBootstrap.cs
│       │   ├── LotoScenario1Controller.cs
│       │   ├── LotoScenario3Controller.cs
│       │   ├── EppEquipmentManager.cs
│       │   ├── LotoSequenceManager.cs
│       │   ├── SyncQueueManager.cs
│       │   └── SimulationSessionManager.cs
│       ├── Data/
│       │   ├── LotoObjectiveIds.cs
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
│       └── LotoPlaceholderController.cs
└── Shared/
    └── UI/
        ├── BrandLogo.png
        ├── Timer_frame.png
        ├── Timer_filled.png
        ├── Score_misionStatus_frame.png
        └── Score_misionStatus_filled.png
```

## Related (existing)

```
Assets/XR Standard Menu/     — Menu shell, ControlCenter, ScoreManager
Assets/Resources/            — es.xml, en.xml, SimScore.asset, BrandConfig
Assets/.cursor/rules/        — Cursor rules including safemining-*
.agent-docs/                 — This knowledge base
```
