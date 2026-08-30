# LOTO Asset List

## Mínimos MVP (required for playable loop)

| Category | Items |
|----------|--------|
| EPP | Helmet, glasses, vest, gloves, boots (good) ×1 each |
| EPP defect | Helmet/glasses/gloves/boots defective variants + reject bin |
| EPP optional | Respirator, hearing protection |
| S1 set | Locker / cabinet + floor teleport |
| S2 hazards | Spill, abandoned tools, fallen signage, obstructed extinguisher |
| S2 decoys | Clean floor, stored tools, upright sign (or equivalent) |
| S3 belt | Conveyor + START / STOP controls |
| S3 panel | Door, breaker, hasp / lock socket |
| Locks | Personal (required), visitor, group (wrong-path) |
| Tags | Danger Do Not Operate (required) + distractors |
| UI | World-space HUD + briefing panels, objective rows |
| Navigation | Floor `TeleportationArea` |

## Worldbuilding (post-MVP polish)

| Category | Ideas |
|----------|--------|
| Architecture | Gallery walls, posts, arches, grated floors |
| Industrial | Pipes, cable trays, valves, mesh fencing |
| Signage | Danger / LOTO / PPE posters (readable at ~1–2 m) |
| Lighting props | Cage lamps, fluorescent fixtures matching Mixed lights |
| Hero prop | Anomalous roller (S2 narrative beat) |
| Atmosphere | Plant noise emitters, optional status screens (static) |
| Perf | LODs, atlases, static batching, baked lightmaps after layout freeze |

Keep green≠good / red≠bad as the only cue — prefer wear marks, labels, and silhouette.

## Editor tooling notes (audit-fix)

- **Placeholders retired:** Do not use `PREF_PH_*` or **Rebuild Visual Prefab Library** for scenario geometry. Source of truth: `PREF_S1_Locker`, `PREF_S2_InspectionYard`, `PREF_S3_ConveyorStation`.
- **Interactable library:** Menu **Rebuild Interactable Prefab Library** regenerates locks, tags, and control prefabs under `Prefabs/Interactables/` only.
- **Scenery2:** Legacy folder removed; no LOTO runtime references.
