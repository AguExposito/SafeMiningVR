# LOTO Simulation Objectives

Single SimScore scene: `loto_simulation`

| Objective ID | Weight | Scenario | Description |
|---|---|---|---|
| `loto_obj_epp_selection` | 20% | `VR-LOTO-S1` | Wear 5 mandatory PPE items in good condition |
| `loto_obj_epp_defect` | 10% | `VR-LOTO-S1` | Identify defective PPE (`S1DefectScore` 10 or 0) |
| `loto_obj_hazard_report` | 25% | `VR-LOTO-S2S3` (logical S2) | Board `IsS2Complete` (tools 3, sign 1, puddle 2, extinguisher 1). Spec still names 4 condition types. Decoys penalize the slice; they do not block S3. |
| `loto_obj_loto_sequence` | 35% | `VR-LOTO-S2S3` (in-scene S3) | Board sequence ready (`IsS3SequenceReady`) |
| `loto_obj_zero_energy` | 10% | `VR-LOTO-S2S3` (in-scene S3) | `CheckStartButton` with sequence ready |

## Phase scores (base 10) — current runtime model

`LotoPhaseScoreTracker` (component on the `ControlCenter` GameObject, persists with it) is the scoring source. `LotoGameManager` forwards every state change to it; its public methods can also be wired from UnityEvents (all idempotent, so double wiring does not double count). Inspector bools show live progress; toggling them in the Inspector recalculates scores (`OnValidate`).

| Phase | Formula | Rule |
|---|---|---|
| S1 | `10 * (1 - defectiveGrabbed / 4)` | 5 good EPP are the gate to continue, not points. 1 defective grab = 7.5, 4 = 0 |
| S2 | `10 * fixed / 7` | Frozen at the first S3 action (`CloseS2Inspection`) |
| S3 | `10 * steps / 6` | Frozen at the first START. Early START keeps only completed steps; full sequence = 10 |
| Global | `Σ(phase * weight) / Σ(weight)` | Weights base 1 (defaults 0.30 / 0.25 / 0.45) |

- Approval per phase and global: `score * 10 >= threshold` (base 100, default 60 each, editable on the tracker).
- Mistakes never block scoring; they only lower the ratio.
- On valid START, the tracker saves a `LotoAttemptRecord` via `ScoreManager.SaveLotoAttempt` into `ScorePersistantData.lotoAttempts` (index 0 = most recent).
- `LotoSessionScore.Build` fills the payload from the tracker (`loto_phase_s1..s3`, `scoreTotal = round(global * 10)`), so sync queue and legacy `SceneScore.points` stay 0–100.
- Session summary lists global + S1/S2/S3 and all attempts, most recent first.

The objective table above is the legacy per-objective model, still used as fallback when no tracker exists.

## Approval

- Default threshold: **60/100** (RN02, CONFIGURACION). Offline override lives in `LotoObjectiveIds.DefaultApprovalThresholdPercent`.
- Weighted score RN01: integer 0–100 from `LotoSessionScore` (`Σ` of per-objective `scorePartial`).
- Runtime flags/gates: `LotoGameManager`. HUD bools + lobby points: `ScoreManager` via `Util_LotoSimulationBridge.SetObjective` (indices in `LotoSimulationConstants`). Contract: [loto-game-board.md](loto-game-board.md).

## Constants

Defined in `LotoObjectiveIds.cs` under `Assets/SafeMining/LOTOSimulation/Code/Data/`.

## Localization keys

Same IDs used as `translationID` in `es.xml` / `en.xml` for score lobby and HUD.
