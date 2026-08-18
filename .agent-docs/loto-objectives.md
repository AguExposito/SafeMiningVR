# LOTO Simulation Objectives

Single SimScore scene: `loto_simulation`

| Objective ID | Weight | Scenario | Description |
|---|---|---|---|
| `loto_obj_epp_selection` | 20% | VR-LOTO-S1 | Correct selection of 5 mandatory PPE items |
| `loto_obj_epp_defect` | 10% | VR-LOTO-S1 | Reject defective PPE |
| `loto_obj_hazard_report` | 25% | VR-LOTO-S2 | Report 4 unsafe conditions |
| `loto_obj_loto_sequence` | 35% | VR-LOTO-S3 | Complete LOTO sequence in order |
| `loto_obj_zero_energy` | 10% | VR-LOTO-S3 | Zero energy verification (START with no movement) |

## Approval

- Default threshold: **75%** (CONFIGURACION / PROGRAMA-CAPACITACION)
- Weighted score: RN01 — sum of (objective_score × weight)

## Constants

Defined in `LotoObjectiveIds.cs` under `Assets/SafeMining/LOTOSimulation/Code/Data/`.

## Localization keys

Same IDs used as `translationID` in `es.xml` / `en.xml` for score lobby and HUD.
