# Folder Structure — SafeMining

```
Assets/SafeMining/
├── LOTOSimulation/
│   ├── Scenes/
│   │   └── VR-LOTO-Placeholder.unity
│   ├── Prefabs/
│   ├── Art/
│   │   ├── Materials/
│   │   ├── Models/
│   │   └── UI/
│   └── Code/
│       ├── SafeMining.LOTOSimulation.asmdef
│       ├── Managers/
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
