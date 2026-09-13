# WF2 — LOTO Serious Gaming & Step Compliance

Audits every step of the LOTO training simulation for industrial procedure fidelity, ensuring no arcade mechanics have been introduced and all feedback follows serious-gaming pedagogy.

**Source of truth:** `Doc_SMVR/SafeMining_VR_v1.1.md` (RN01, RN02, HU-220 through HU-223), `Propuesta Simulacion LOTO.txt`, `.cursor/rules/30-loto-domain.mdc`, `.agent-docs/loto-game-board.md`.

Gates and flags: `LotoGameManager`. HUD/lobby checks: `ScoreManager` via `Util_LotoSimulationBridge.SetObjective`. Do not treat leftover scenario managers as source of truth. World art frozen; HUD may change.

---

## Phase A — Serious-Gaming Philosophy Gate

Before auditing individual steps, verify the simulation's overarching design posture.

### A1. Anti-arcade checklist

- [ ] No countdown timers visible during any scenario.
- [ ] No score pop-ups, multipliers, combos, or streaks on any interaction.
- [ ] No leaderboard or ranking screen.
- [ ] No artificial urgency cues (flashing timer, "hurry" audio).
- [ ] Pacing is entirely trainee-controlled.
- [ ] **PASS:** Zero arcade mechanics found.
- [ ] **FAIL:** Any item above is present → remove immediately, reference rule `30-loto-domain.mdc` "NEVER #4".

### A2. Pedagogical error handling

- [ ] Every error (wrong EPP, wrong lock, wrong tag, false-positive hazard) produces a localized subtitle explaining WHY the choice is wrong.
- [ ] Error subtitle uses `LanguageManager` key (pattern `loto_*`), NOT hardcoded Spanish.
- [ ] After error explanation, trainee can retry without penalty beyond score impact.
- [ ] Error feedback uses subtitle + audio cue only — no camera shake, particle burst, or screen flash.
- [ ] **PASS:** All errors are learning moments with clear pedagogical explanation.
- [ ] **FAIL:** Any error produces only a generic "wrong" or uses non-localized text.

### A3. Branding compliance

| Token | Hex | Where expected | Pass/Fail |
|-------|-----|----------------|-----------|
| Primary `#1A365D` | HUD backgrounds, headers | [ ] |
| Secondary `#F97316` | CTAs, outline hints, dwell reticle | [ ] |
| Tertiary `#334155` | Borders, secondary text | [ ] |
| Neutral `#F9FAFB` | Text on dark panels | [ ] |
| Success `#22C55E` | Achieved objective indicator | [ ] |
| Error `#EF4444` | Failure alerts | [ ] |

- [ ] All colors applied via `Brand.*` runtime accessors (not hardcoded hex in scripts).
- [ ] MCP verify: `Unity_FindProjectAssets` type `BrandConfig` → asset exists at `Assets/Resources/BrandConfig.asset`.

---

## Phase B — S1 EPP Selection Compliance

**Requirement:** RN01 (20% weight), HU-220.

### B1. Mandatory EPP items (5/5)

| Item | Present in scene | Equippable | Pass/Fail |
|------|-----------------|------------|-----------|
| Casco de seguridad | [ ] | [ ] | [ ] |
| Lentes de seguridad | [ ] | [ ] | [ ] |
| Chaleco reflectante | [ ] | [ ] | [ ] |
| Guantes | [ ] | [ ] | [ ] |
| Botas de seguridad | [ ] | [ ] | [ ] |

- [ ] **PASS:** All 5 mandatory items present, equippable via grab (hand tracking + controller).
- [ ] **FAIL:** Any mandatory item missing or not equippable.

### B2. Defective items (10% weight)

- [ ] At least one defective variant exists per category that has defects (visible wear: dents, scratches, tears, worn soles).
- [ ] Equipping a defective item triggers localized feedback explaining the defect.
- [ ] Defective items can be discarded via reject zone/bin.
- [ ] Rejecting defective items progresses the `loto_obj_epp_defect` objective.
- [ ] **PASS:** Defect detection is visual (wear marks), feedback is pedagogical.
- [ ] **FAIL:** Defects are invisible, no feedback on equipping defective, or reject mechanism missing.

### B3. Optional items

- [ ] Respirador and protector auditivo are present (optional).
- [ ] Equipping or ignoring optional items does NOT block progress to S2.
- [ ] **PASS:** Optional items don't gate S2 transition.
- [ ] **FAIL:** Simulation requires >5 items or blocks on optional equipment.

### B4. S1 → S2 gate

- [ ] Continue door/transition is ONLY available after `LotoGameManager.IsS1Complete` (5 good EPP).
- [ ] Outline hint appears on continue door when 5/5 is achieved (HUD/outline may be bound to board events).
- [ ] `TryContinueS1` requires `ControlCenter` (Play from `StandardMainMenu`).
- [ ] **PASS:** Gate enforced on the board.
- [ ] **FAIL:** Transition available before 5/5, or gate still lives on `EppEquipmentManager`.

---

## Phase C — S2 Hazard Inspection Compliance

**Requirement:** RN01 (25% weight), HU-221.

### C1. Required hazard types (spec HU-222)

Four pedagogical types. Runtime complete is `LotoGameManager.IsS2Complete` (tools 3, sign 1, puddle 2, extinguisher 1) — not a leftover 4/4 manager gate.

| Hazard | Present | Reportable | Localized feedback | Pass/Fail |
|--------|---------|-----------|-------------------|-----------|
| Material derramado en zona de tránsito | [ ] | [ ] | [ ] | [ ] |
| Herramientas abandonadas | [ ] | [ ] | [ ] | [ ] |
| Señalización caída o deteriorada | [ ] | [ ] | [ ] | [ ] |
| Extintor parcialmente obstruido | [ ] | [ ] | [ ] | [ ] |

- [ ] **PASS:** Spec types are present and reportable; board counts can reach `IsS2Complete`.
- [ ] **FAIL:** Types missing, unreportable, or interactables do not notify the board.

### C2. Decoys

- [ ] At least one decoy present (clean floor, stored tools, upright sign, or equivalent).
- [ ] Reporting a decoy penalizes score (reduces hazard partial by 25% of weight per false positive).
- [ ] Reporting a decoy does NOT block transition to S3 if `IsS2Complete`.
- [ ] Decoy reporting triggers localized subtitle explaining why it's not a real hazard.
- [ ] Outline hints do NOT appear on decoys.
- [ ] **PASS:** Decoys test trainee judgment without blocking progression.
- [ ] **FAIL:** Decoys block S3, no penalty, or outlines highlight decoys.

### C3. S2 → S3 gate

- [ ] Continue is ONLY available after `LotoGameManager.IsS2Complete` (tools 3, sign 1, puddle 2, extinguisher 1).
- [ ] Interactables call `NotifyTools` / `NotifySign` / `NotifyPuddle` / `NotifyExtinguisher` (or relay).
- [ ] **PASS:** Gate enforced on the board.
- [ ] **FAIL:** Transition before board complete, or gate still lives on `HazardInspectionManager`.

---

## Phase D — S3 LOTO Sequence Compliance

**Requirement:** RN01 (35% + 10% weight), RN04, HU-222, HU-223.

### D1. Strict LOTO order

Execute each pedagogical step. Board flags: `NotifyStop`, `NotifyPanelOpened`, `NotifyPowerCut`, `NotifyPanelClosed`, `NotifyHaspPlaced`, `NotifyLockPlaced`, then `CheckStartButton`. Tag has no board flag.

| Step | Action | System response | Pass/Fail |
|------|--------|----------------|-----------|
| 1 | Press STOP button on control station | Conveyor stops | [ ] |
| 2 | Open panel, switch breaker to OFF | Breaker moves to OFF position | [ ] |
| 3 | Place RED lock (Personal Lockout) on hasp | Lock snaps into socket | [ ] |
| 4 | Attach PELIGRO NO OPERAR tag on lock shackle | Tag attaches to lock | [ ] |
| 5 | Press START to verify zero energy | Belt does NOT move → zero energy confirmed | [ ] |

- [ ] **PASS:** All 5 steps work in strict order. Each subsequent step is only available after the previous completes.
- [ ] **FAIL:** Steps can be skipped, or later steps activate before earlier ones complete.

### D2. Wrong lock rejection

| Lock color | Expected behavior | Pass/Fail |
|-----------|-------------------|-----------|
| Red (Personal Lockout) | Accepted — progresses LOTO | [ ] |
| Blue (Visitor) | Rejected — localized subtitle explains wrong lock type | [ ] |
| Yellow (Grupal) | Rejected — localized subtitle explains wrong lock type | [ ] |

- [ ] **PASS:** Only red personal lock advances the sequence.
- [ ] **FAIL:** Blue or yellow lock is accepted.

### D3. Wrong tag rejection

| Tag text | Expected behavior | Pass/Fail |
|---------|-------------------|-----------|
| PELIGRO NO OPERAR | Accepted — progresses LOTO | [ ] |
| INSPECCIONADO | Rejected — localized subtitle | [ ] |
| EQUIPO OPERATIVO | Rejected — localized subtitle | [ ] |
| MANTENIMIENTO TERMINADO | Rejected — localized subtitle | [ ] |

- [ ] **PASS:** Only PELIGRO NO OPERAR tag advances the sequence.
- [ ] **FAIL:** Wrong tag is accepted.

### D4. Failure path — Malo 1 (Incomplete LOTO + START)

- [ ] Press START without completing all LOTO steps.
- [ ] **PASS:** Belt jolts briefly + safety alert subtitle `loto_s3_alert_incomplete`. Recovery is possible: trainee can complete remaining steps and press START again.
- [ ] **FAIL:** No jolt, no alert, or simulation soft-locks.

### D5. Failure path — Malo 2 (Breaker without gloves)

- [ ] If S1 gloves were not equipped, operate breaker in S3.
- [ ] **PASS:** Electrocution alert `loto_s3_alert_no_gloves` + event `breaker_no_gloves`. Breaker still advances (no soft-lock).
- [ ] **FAIL:** No alert, or breaker blocks permanently.

---

## Phase E — Scoring Validation

### E0. Board vs ScoreManager

- [ ] Evaluable actions notify `LotoGameManager` (or `LotoGameEventRelay`).
- [ ] HUD/lobby checks go through `Util_LotoSimulationBridge.SetObjective` → `ScoreManager` (Play from menu so CC exists).
- [ ] **FAIL:** New scoring or gates added to `EppEquipmentManager` / `HazardInspectionManager` / `LotoSequenceManager`.

### E1. Objective weights

| Objective | Expected weight | Actual in `LotoObjectiveIds` | Pass/Fail |
|-----------|----------------|------------------------------|-----------|
| `loto_obj_epp_selection` | 20% | [ ] | [ ] |
| `loto_obj_epp_defect` | 10% | [ ] | [ ] |
| `loto_obj_hazard_report` | 25% | [ ] | [ ] |
| `loto_obj_loto_sequence` | 35% | [ ] | [ ] |
| `loto_obj_zero_energy` | 10% | [ ] | [ ] |
| **Total** | **100%** | [ ] | [ ] |

### E2. Approval threshold

- [ ] `LotoObjectiveIds.DefaultApprovalThresholdPercent` = **60** (not 75).
- [ ] `LotoSessionScore.Build()` → `approved` is `true` when `scoreTotal >= 60`.
- [ ] **PASS:** Threshold matches RN02 specification.
- [ ] **FAIL:** Threshold is wrong value.

### E3. False-positive penalty

- [ ] Happy path (0 false positives): hazard score = full 25%.
- [ ] 1 false positive: hazard score = 25 * (1 - 1/4) = 18.75 → rounded.
- [ ] 4 false positives: hazard score = 0.
- [ ] **PASS:** Penalty formula matches `LotoSessionScore.ComputePartial`.
- [ ] **FAIL:** Penalty not applied or formula wrong.

### E4. Session summary

- [ ] After S3 completion, session summary displays: total score, per-objective breakdown, approved/not approved.
- [ ] Abandoned sessions show `completionStatus = "Abandoned"` with partial score.
- [ ] **PASS:** Summary matches computed payload.
- [ ] **FAIL:** Summary missing data or showing wrong values.

---

## Phase F — UI & Localization Compliance

### F1. Localization binding

- [ ] Search codebase: NO hardcoded Spanish strings in `Assets/SafeMining/**/*.cs`.
- [ ] All user-facing text uses `TranslatableText` + `translationID`.
- [ ] LOTO keys follow `loto_*` prefix pattern.
- [ ] Both `es.xml` and `en.xml` contain matching keys.
- [ ] **PASS:** Full localization coverage.
- [ ] **FAIL:** Hardcoded strings or missing translation keys.

### F2. World-space canvas standards

- [ ] All simulation canvases are World Space (not Screen Space Overlay).
- [ ] Canvas scale ~0.001.
- [ ] HUD distance ~1.5–2m from player.
- [ ] No non-diegetic UI chrome (health bars, minimap, etc.).
- [ ] **PASS:** UI follows VR world-space standards.
- [ ] **FAIL:** Screen Space Overlay canvas or immersion-breaking chrome.

---

## Sign-off matrix

| Phase | Auditor | Date | Result |
|-------|---------|------|--------|
| A — Philosophy gate | | | |
| B — S1 EPP | | | |
| C — S2 Hazards | | | |
| D — S3 LOTO | | | |
| E — Scoring | | | |
| F — UI & Localization | | | |

**Workflow complete when all 6 phases show PASS.**
