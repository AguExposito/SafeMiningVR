# LOTO Simulation Objectives

Single SimScore scene: `loto_simulation`

| Objective ID | Weight | Scenario | Description |
|---|---|---|---|
| `loto_obj_epp_selection` | 20% | `VR-LOTO-S1` | Wear 5 mandatory PPE items in good condition |
| `loto_obj_epp_defect` | 10% | `VR-LOTO-S1` | Identify defective PPE (`S1DefectScore` 10 or 0) |
| `loto_obj_hazard_report` | 25% | `VR-LOTO-S2S3` (logical S2) | Board `IsS2Complete` (tools 3, sign 1, puddle 2, extinguisher 1). Spec still names 4 condition types. Decoys penalize the slice; they do not block S3. |
| `loto_obj_loto_sequence` | 35% | `VR-LOTO-S2S3` (in-scene S3) | Board sequence ready (`IsS3SequenceReady`) |
| `loto_obj_zero_energy` | 10% | `VR-LOTO-S2S3` (in-scene S3) | `CheckStartButton` with sequence ready |

## Approval

- Default threshold: **60/100** (RN02, CONFIGURACION). Offline override lives in `LotoObjectiveIds.DefaultApprovalThresholdPercent`.
- Weighted score RN01: integer 0–100 from `LotoSessionScore` (`Σ` of per-objective `scorePartial`).
- Runtime flags/gates: `LotoGameManager`. HUD bools + lobby points: `ScoreManager` via `Util_LotoSimulationBridge.SetObjective` (indices in `LotoSimulationConstants`). Contract: [loto-game-board.md](loto-game-board.md).

## Constants

Defined in `LotoObjectiveIds.cs` under `Assets/SafeMining/LOTOSimulation/Code/Data/`.

## Localization keys

Same IDs used as `translationID` in `es.xml` / `en.xml` for score lobby and HUD.
