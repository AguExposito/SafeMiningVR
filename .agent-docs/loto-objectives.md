# LOTO Simulation Objectives

Single SimScore scene: `loto_simulation`

| Objective ID | Weight | Scenario | Description |
|---|---|---|---|
| `loto_obj_epp_selection` | 20% | VR-LOTO-S1 | Wear 5 mandatory PPE items in good condition |
| `loto_obj_epp_defect` | 10% | VR-LOTO-S1 | Identify defective PPE (do not wear; optional discard bin) |
| `loto_obj_hazard_report` | 25% | VR-LOTO-S2 | Report 4 unsafe conditions (scored at 4/4). Each decoy reduces this slice by 25/4 (RN01). Decoys do not block S3. |
| `loto_obj_loto_sequence` | 35% | VR-LOTO-S3 | Complete LOTO sequence in order |
| `loto_obj_zero_energy` | 10% | VR-LOTO-S3 | Zero energy verification (START with no movement) |

## Approval

- Default threshold: **60/100** (RN02, CONFIGURACION). Offline override lives in `LotoObjectiveIds.DefaultApprovalThresholdPercent`.
- Weighted score RN01: integer 0–100 from `LotoSessionScore` (`Σ` of per-objective `scorePartial`).
- `ScoreManager` still stores bool checks for the HUD; lobby `points` / `approbalRate` persist the weighted total.

## Constants

Defined in `LotoObjectiveIds.cs` under `Assets/SafeMining/LOTOSimulation/Code/Data/`.

## Localization keys

Same IDs used as `translationID` in `es.xml` / `en.xml` for score lobby and HUD.
