# SafeMining VR — QA Workflows

Executable checklists for validating the LOTO training simulation across all quality dimensions.

## Workflow index

| ID | Workflow | Scope | Primary rules |
|----|----------|-------|---------------|
| WF1 | [Dual-Mode Testing & Ergonomics](wf-dual-mode-testing.md) | PC Playmode + Quest 3S device, input modality, dwell-time, HUD comfort | `40-vr-interaction`, `50-hud-ui-brand` |
| WF2 | [LOTO Serious Gaming & Step Compliance](wf-loto-step-compliance.md) | S1–S3 on `LotoGameManager` + `ScoreManager`, pedagogy, branding, localization | `30-loto-domain`, `50-hud-ui-brand` |
| WF3 | [HTTP Network, Auth & Telemetry Pipeline](wf-network-auth-telemetry.md) | Offline-first sync, DTOs, API contracts, auth readiness, network resilience | `70-backend-sync` |
| WF4 | [Build & Performance Verification](wf-build-performance.md) | Build settings, lighting budget, script perf, asset optimization, 72 FPS target | `60-quest-performance`, `10-unity-architecture` |

## Execution order

1. **WF4** (Build & Performance) — ensures the project compiles and runs at target framerate.
2. **WF1** (Dual-Mode Testing) — confirms input and ergonomics on both platforms.
3. **WF2** (LOTO Compliance) — audits functional correctness and pedagogical fidelity.
4. **WF3** (Network Pipeline) — validates data persistence and sync readiness.

## Subagent mapping

| Workflow | Primary subagent | Backup subagent |
|----------|-----------------|----------------|
| WF1 | `/vr-usability-interaction` | `/unity-performance` (camera/rig issues) |
| WF2 | `/loto-compliance` | `/vr-usability-interaction` (interaction UX) |
| WF3 | `/network-session` | `/security-review` (auth/adapter changes) |
| WF4 | `/unity-performance` | — |

## Cloud agent mapping

| Workflow | Cloud brief template | Branch prefix |
|----------|---------------------|---------------|
| WF2 full audit | Template A in `.agent-docs/templates/cloud-agent-briefs.md` | `audit/` |
| S3 interactable refactor | Template B | `refactor/` |
| Quest perf audit | Template C | `perf/` |
| Fase 7 HTTP adapter | Template D | `security/` |

Playbook: `.agent-docs/cloud-agent-playbook.md`

## Convention

- `[ ]` = unchecked, awaiting verification.
- `[x]` = passed.
- **PASS** / **FAIL** criteria are based on project documentation, not subjective judgment.
- MCP calls referenced inline are the minimum verification steps; additional inspection is encouraged.
