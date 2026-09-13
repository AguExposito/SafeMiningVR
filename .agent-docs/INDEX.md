# SafeMining VR — Document Index

Lightweight lookup table for surgical reads. Find the ID, read only the lines indicated.

## How to use this index

1. Find the requirement/topic ID in the tables below.
2. Read **only** the line range from the source file: `Read(path, offset=<start>, limit=<end-start>)`.
3. Cite the ID and source in your response before generating code.

Do NOT load full documents. Use this index for targeted extraction.

---

## Business Rules (RN)

Source: `Doc_SMVR/SafeMining_VR_v1.1.md`

| ID | Summary | Lines |
|----|---------|-------|
| RN01 | Scoring formula: weighted sum of objectives, 0–100 | 3628–3629 |
| RN02 | Approval threshold: score ≥ configurable min (default 60) | 3630–3631 |
| RN03 | Program approval: all mandatory sims + exams approved | 3632–3633 |
| RN04 | Attempt limit: exhausted = program failed; manual override possible | 3634–3635 |
| RN05 | Assignment uniqueness: no duplicate active assignments | 3636–3637 |
| RN06 | Simulation validity in active programs | 3638–3639 |
| RN07 | Company data isolation (multi-tenant) | 3640–3641 |
| RN08 | Certificate validity: one per employee-program pair | 3642–3643 |
| RN09 | VR session must register before execution begins | 3644–3645 |
| RN10 | Inactive employees: no sessions, history preserved | 3646–3647 |

## Non-Functional Requirements (RNF)

Source: `Doc_SMVR/SafeMining_VR_v1.1.md`

| ID | Summary | Lines |
|----|---------|-------|
| RNF01 | API response ≤2s for auth/sim queries (200 concurrent) | 3556 |
| RNF02 | Web UI loads <3s on ≥10 Mbps | 3558 |
| RNF03 | PDF reports ≤10s for 500 records | 3560 |
| RNF04 | Horizontal scaling: 50 companies, 10k employees | 3564 |
| RNF05 | Multi-tenant via empresa_id field | 3566 |
| RNF06 | HTTPS/TLS 1.2+ mandatory | 3570 |
| RNF07 | BCrypt cost ≥12 for passwords | 3572 |
| RNF07.1 | Password policy: 12+ chars, upper/lower/digit/special | 3574–3582 |
| RNF08 | JWT expiry: 8h web, 2h VR (configurable via I3) | 3584 |
| RNF09 | Brute-force protection: lock after N failures (default 5/15min) | 3586 |
| RNF10 | Audit log for critical events | 3588 |
| RNF10.1 | Audit records immutable, lifetime retention | 3590 |
| RNF10.2 | Audit transactional with operation | 3592 |
| RNF10.3 | Audit filterable by entity/event/user/company/date | 3594 |
| RNF11 | Company data access restricted by empresa_id in service layer | 3596 |
| RNF12 | Responsive web: ≥1280×720, modern browsers | 3600 |
| RNF13 | Realtime form validation, Spanish error messages | 3602 |
| RNF14 | Destructive ops require explicit confirmation | 3604 |
| RNF15 | 99% monthly uptime (max 7.2h downtime) | 3608 |
| RNF17 | VR client: Quest 2/3 compatible, Unity 2022+ | 3614 |
| RNF18 | REST API: JSON, OpenAPI 3.0 | 3616 |
| RNF19 | PostgreSQL, ACID transactions | 3620 |
| RNF21 | VR results + exam attempts: logical delete only | 3624 |

## VR LOTO User Stories (HU-220 to HU-223)

Source: `Doc_SMVR/SafeMining_VR_v1.1.md`

| ID | Title | Scenario | Lines |
|----|-------|----------|-------|
| HU-220 | Start/end VR LOTO simulation from assigned program | All (S1–S3) | 4757–4776 |
| HU-221 | Select and equip mandatory PPE in locker room | S1 | 4780–4798 |
| HU-222 | Identify and report unsafe conditions in pre-op inspection | S2 | 4802–4820 |
| HU-223 | Execute LOTO sequence and zero-energy verification | S3 | 4824–4845 |

## VR-Relevant Functional Modules

Source: `Doc_SMVR/SafeMining_VR_v1.1.md`

| Module | Name | Lines |
|--------|------|-------|
| T1 | Authentication & Access Control | 3291–3309 |
| F2 | VR Simulation Management | 3384–3398 |
| F4 | Assignment, Execution & Evaluation | 3418–3454 |
| I3 | Configuration & Parameters | 3500–3540 |

## LOTO Simulation Objectives

Source: `.agent-docs/loto-objectives.md` (full file — safe to load entirely). Runtime gates: `.agent-docs/loto-game-board.md`.

| Objective ID | Weight | Scenario |
|--------------|--------|----------|
| `loto_obj_epp_selection` | 20% | S1 |
| `loto_obj_epp_defect` | 10% | S1 |
| `loto_obj_hazard_report` | 25% | S2 |
| `loto_obj_loto_sequence` | 35% | S3 |
| `loto_obj_zero_energy` | 10% | S3 |

Approval threshold: 60/100 (`LotoObjectiveIds.DefaultApprovalThresholdPercent`).

## API Contracts (Fase 7)

Source: `.agent-docs/integration-vr-backend.md` (full file, 82 lines — safe to load entirely)

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/api/vr/sessions/start` | POST | Register session before gameplay (RN09) |
| `/api/vr/sessions/{id}/events` | POST | Record evaluable interactions |
| `/api/vr/sessions/{id}/complete` | POST | Send SimulationResultPayload |
| `/api/vr/sessions/sync` | POST | Flush deferred sync queue |

## LOTO Procedure (Quick Reference)

Source: `Propuesta Simulacion LOTO.txt` (full file, 100 lines — safe to load entirely)

| Scenario | Gate | Key elements |
|----------|------|--------------|
| S1 — EPP | `LotoGameManager.IsS1Complete` (5/5 good) | Helmet, glasses, vest, gloves, boots; defective grab zeros `S1DefectScore` |
| S2 — Hazards | `IsS2Complete` | Board counts: tools 3, sign 1, puddle 2, extinguisher 1 |
| S3 — LOTO | `IsS3SequenceReady` then `CheckStartButton` | STOP → panel → power cut → close → hasp → lock → START |

## Operational Docs (.agent-docs/)

All files are small enough to load entirely when needed.

| File | Lines | Content |
|------|-------|---------|
| `project-overview.md` | ~40 | Stack, state, architecture, branding |
| `loto-game-board.md` | ~120 | KISS: LotoGameManager, relay, ScoreManager, spec vs board |
| `loto-simulation-handbook.md` | ~90 | Player flow, outline hints, input, lighting |
| `loto-objectives.md` | ~30 | Objective IDs, weights, threshold |
| `s1-scene-inventory.md` | ~120 | VR-LOTO-S1 inventory; art frozen; board GOs |
| `s2-scene-inventory.md` | ~170 | VR-LOTO-S2S3 inventory; art frozen; board/relay |
| `loto-asset-list.md` | ~30 | MVP and polish asset requirements |
| `integration-vr-backend.md` | ~82 | API contracts, DTOs, auth flow |
| `folder-structure.md` | ~65 | Canonical folder layout |
| `qa-quest-fase6.md` | ~45 | Build settings, comfort, fail paths, device checklist |
| `cloud-agent-playbook.md` | ~120 | Cloud/background agent branch protocol, merge gates |
| `templates/cloud-agent-briefs.md` | ~200 | Copy-paste `/in-cloud` brief templates (WF2, S3, perf, Fase 7) |

## Brand Palette

Source: `Doc_SMVR/paleta de colores.jpg` (image) + `.cursor/rules/50-hud-ui-brand.mdc`

| Token | Hex | Usage |
|-------|-----|-------|
| Primary | `#1A365D` | HUD backgrounds, headers |
| Secondary | `#F97316` | CTAs, outline hints, dwell reticle |
| Tertiary | `#334155` | Borders, secondary text |
| Neutral | `#F9FAFB` | Text on dark panels |
| Success | `#22C55E` | Objective achieved |
| Error | `#EF4444` | Failures, safety alerts |
