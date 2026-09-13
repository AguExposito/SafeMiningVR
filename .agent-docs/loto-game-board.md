# LOTO Game Board — KISS contract

Single operational contract for gameplay state. Spec (RN/HU) still defines *what* to train; this file defines *where state lives*.

## KISS

1. **One board.** `LotoGameManager` owns S1/S2/S3 flags, gates, and C# events. Parameterless `Notify*` methods for UnityEvents.
2. **One relay per scene.** `LotoGameEventRelay` forwards Inspector UnityEvents to the board and re-fires local UnityEvents when the board completes a scenario.
3. **One score sink.** `ScoreManager` (via `ControlCenter` + `Util_LotoSimulationBridge.SetObjective`) owns HUD checks and lobby points. The board does not replace ScoreManager.
4. **Interactables notify; they do not gate.** Grab/dwell/PC cheats call the board or relay. Do not restore gates inside `EppEquipmentManager`, `HazardInspectionManager`, or `LotoSequenceManager`.
5. **No visual scene work.** Cabin, warehouse, conveyor, locker meshes, lights, and prefab transforms are frozen. Do not add decorative GOs, move art, or rebuild scenario prefabs “to fix logic”.
6. **HUD may change.** `PREF_LotoPlayerHUD`, `PREF_LotoBriefingPanel`, `PREF_LotoObjectiveRow`, `PlayerHudManager`, `TranslatableText` bindings, and relay UnityEvents on the HUD are in scope.
7. **Empty logic GOs only.** Board and relay are Transform + script at origin. They already exist in the build scenes — inspect, do not duplicate.
8. **Play from menu for scores and S1→S2.** `StandardMainMenu` creates `ControlCenter`. Direct Play on `VR-LOTO-S1` can still drive the board; `TryContinueS1` and `SetObjective` no-op without CC.

## Placement

| Scene | GO | Role |
|-------|----|------|
| `VR-LOTO-S1` (build 1) | `LotoGameManager` | DDOL singleton. Resets when `VR-LOTO-S1` loads again. |
| `VR-LOTO-S1` | `LotoGameEventRelay` | Scene-local UnityEvent API. |
| `VR-LOTO-S2S3` (build 2) | `LotoGameEventRelay` | Same API after the board persists from S1. No second board. |

Logical S2 then in-scene S3 share `VR-LOTO-S2S3`. `VR-LOTO-S3.unity` is not in Build Settings.

## Layers

```
Interactable / PC cheat / UnityEvent
        │
        ▼
LotoGameEventRelay  ──forward──►  LotoGameManager (state + gates)
        ▲                                │
        └── local UnityEvents ◄──────────┘ S1Completed / S2Completed / S3Completed / S3StartTooEarly
                                             │
                                             ▼
                          Util_LotoSimulationBridge.SetObjective
                                             │
                                             ▼
                          ScoreManager + PlayerHudManager (HUD rows)
```

## Board API (implemented)

### S1 — EPP

| Method | Effect |
|--------|--------|
| `NotifyHelmetGood` … `NotifyBootsGood` or `NotifyEppGrabbed(type, Good)` | Sets the matching good flag. |
| `NotifyHelmetBad` … `NotifyBootsBad` or `NotifyEppGrabbed(type, Defective)` | Sets bad flag; `S1DefectScore` → 0. Does not block 5/5. |
| `IsS1Complete` | All 5 good flags. Gate for pad/door. |
| `S1DefectScore` | 10 until any defective grab, else 0. |
| `TryContinueS1` | If complete and CC present → `SimulationOrchestratorManager.OnScenario1Completed()`. |

Completing 5 good still raises `S1Completed` even if defectives were grabbed.

### S2 — hazards (board counts)

| Method | Required count |
|--------|----------------|
| `NotifyTools` | 3 |
| `NotifySign` | 1 |
| `NotifyPuddle` | 2 |
| `NotifyExtinguisher` | 1 |

Spec HU-222 still names four unsafe-condition *types*. Operational gates use these **counts**, not `HazardInspectionManager` and not “4/4 spots”.

### S3 — sequence

`NotifyStop` → `NotifyPanelOpened` → `NotifyPowerCut` → `NotifyPanelClosed` → `NotifyHaspPlaced` → `NotifyLockPlaced`.

`CheckStartButton`: if sequence not ready → `S3StartTooEarly` (Malo 1); if ready → `zeroEnergyVerified` + `S3Completed`.

Lock color / tag text remain **interactable validation** (spec still requires red lock + PELIGRO NO OPERAR). The board has **no** `NotifyTag` flag. `IsS3SequenceReady` is Stop + panel opened + power cut + panel closed + hasp + lock.

## ScoreManager

| Piece | Role |
|-------|------|
| `ScoreManager` | Framework manager on CC. Bool checks per SimScore objective for HUD + lobby. |
| `LotoSimulationConstants.Objective*Index` | 0 EPP selection, 1 EPP defect, 2 hazard, 3 LOTO sequence, 4 zero energy. |
| `Util_LotoSimulationBridge.SetObjective` | Only scoring entry for LOTO. Also notifies HUD visual update. |
| `LotoSessionScore` / `LotoObjectiveIds` | Weighted RN01 total and 60% threshold. |

Call `SetObjective` from board-complete handlers or a thin listener on `LotoGameManager` events — not from leftover `SetObjective` inside old scenario managers. Do not duplicate weights in interactables.

Direct S1 Play: CC is null → bridge returns; board flags still update.

## Presentation leftovers

These scripts may still hide meshes, visors, or body anchors. They are **not** the source of truth:

- `EppEquipmentManager` — locker presentation; `TryContinue()` forwards to `TryContinueS1()`.
- `HazardInspectionManager` / `LotoSequenceManager` — do not re-add as gates if missing from `VR-LOTO-S2S3`.
- Scenario controllers / `LotoScenarioSceneBootstrap` — HUD follow, PC driver injection, not board state.

## Spec vs board

| Topic | Spec / pedagogy | Runtime gate |
|-------|-----------------|--------------|
| S2 | Four unsafe-condition types (HU-222) | `IsS2Complete`: tools 3, sign 1, puddle 2, extinguisher 1 |
| S3 | STOP → breaker → red lock → PELIGRO NO OPERAR tag → START | `IsS3SequenceReady` then `CheckStartButton` (no tag flag) |
| Scene name | Logical `VR-LOTO-S2` | Disk/build `VR-LOTO-S2S3` |

Checklists and agents must assert the **runtime** column. Do not fail a gate because leftover managers or a 4/4 `IsReadyToAdvance` are missing.

## ALWAYS

1. Notify the board (or relay) on every evaluable interaction.
2. Gate continue/teleport/S3 START on `LotoGameManager` properties.
3. Push HUD/score through `ScoreManager` via the bridge when CC exists.
4. Change HUD prefabs/bindings when pedagogy needs it.

## NEVER

1. Move, scale, or rebuild cabin/warehouse/conveyor/locker art to “fix” gameplay.
2. Add visual GOs or duplicate `LotoGameManager` / `LotoGameEventRelay`.
3. Call `ScoreManager.AchiveObjective` or `Util_LotoSimulationBridge.SetObjective` from leftover EPP/hazard/LOTO manager logic.
4. Treat `EppEquipmentManager.IsReadyToAdvance` (or equivalent) as the S1/S2/S3 gate.
