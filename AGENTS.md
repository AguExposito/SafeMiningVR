# AGENTS.md — SafeMining VR

SafeMining VR is an industrial safety training simulation (Serious Games) for the LOTO (Lockout/Tagout) procedure, built in Unity 6 with XRI 3.3.1 targeting Meta Quest 3S and PC Editor Playmode. This is workplace training — not an arcade game. Interactions replicate real-world safety procedures; feedback is pedagogical; pacing is trainee-controlled.

## Source-of-truth hierarchy

Resolve any design, domain, or requirement question using this strict cascade. Stop at the first level that answers:

1. **Formal spec** — `Doc_SMVR/SafeMining_VR_v1.1.md` (~5800 lines — **never load fully**; use `.agent-docs/INDEX.md` to find line ranges)
2. **Gameplay brief** — `Propuesta Simulacion LOTO.txt`
3. **Operational docs** — `.agent-docs/` (handbooks, objectives, integration notes, QA workflows)
4. **Brand reference** — `Doc_SMVR/paleta de colores.jpg` + `Assets/Resources/BrandConfig.asset`
5. **Cursor rules** — `.cursor/rules/*.mdc` (coding directives, performance budgets, MCP protocols)
6. **Cursor skills** — `.cursor/skills/*/SKILL.md` (procedures + subagent templates)
7. **Existing codebase** — `Assets/SafeMining/` and `Assets/XR Standard Menu/`
8. **Meta VR MCP** — `meta_docs_search` for Quest SDK verification
9. **Ask the user** — last resort when all sources are exhausted

## MCP gates (mandatory)

- **Session start** — `Unity_ManageEditor → GetProjectRoot` must return `D:/Unity/Projects/SafeMiningVR`. Abort otherwise.
- **After every code/scene change** — `Unity_ReadConsole` (Errors) → require 0 entries.
- **Before any XR/Quest change** — `meta_docs_search` to verify the API is current and not deprecated.
- **Never hand-edit** `.unity`, `.prefab`, or `.asset` YAML — use Unity MCP tools.

## Task routing

| Task domain | Rule | Skill | Native subagent | Legacy skill launcher |
|-------------|------|-------|-----------------|----------------------|
| XR interaction, hand tracking, dwell-time, outline hints | `40-vr-interaction` | `meta-xr-interaction` | `/vr-usability-interaction` | `agent-vr-usability` |
| PC playtesting, dual-mode rig, camera/input | `40-vr-interaction` | `dual-mode-playtest` | `/vr-usability-interaction` | `agent-vr-usability` |
| Backend sync, DTOs, auth, scoring payload | `70-backend-sync` | `network-session-pipeline` | `/network-session` | `agent-network-security` |
| LOTO compliance, pedagogy, branding, localization | `30-loto-domain` + `50-hud-ui-brand` | `safemining-compliance` | `/loto-compliance` | `agent-serious-games` |
| Performance, lighting, architecture, build settings | `60-quest-performance` + `10-unity-architecture` | — | `/unity-performance` | `agent-unity-performance` |

Native subagents live in `.cursor/agents/` and appear in **Settings → Customize → Subagents**. Invoke explicitly with `/name` syntax.

| Role | Command | When to use |
|------|---------|-------------|
| Orchestrator | `/safemining-orchestrator` | Any task, multi-domain work, unsure which specialist |
| VR Interaction | `/vr-usability-interaction` | XRI, hand tracking, dwell, dual-mode |
| Network & Session | `/network-session` | Adapters, sync queue, JWT, payloads |
| LOTO Compliance | `/loto-compliance` | RN/RNF/HU audit, branding, scoring gates |
| Unity Performance | `/unity-performance` | FPS, lighting, architecture, builds |
| Security Review | `/security-review` | Auth, adapters, sync queue, score payloads |
| Bugbot | `/bugbot` | Regressions, logic bugs, pre-merge PR review |

## Cloud & background agents

Long-running async work (WF2 audits, S3 refactors, perf analysis): see `.agent-docs/cloud-agent-playbook.md`.

- Launch: `/in-cloud` + brief from `.agent-docs/templates/cloud-agent-briefs.md`
- Branch protocol: `feature/`, `audit/`, `refactor/`, `perf/`, `security/`
- PR babysit: `/babysit`
- Merge gate: `/review-bugbot` + `/review-security` (if backend paths changed)

## Security review

Rule: `.cursor/rules/80-security-review.mdc` — vectors V1–V6 for Adapters, DTOs, SyncQueue, IBManager.

- Invoke: `/review-security` or skill `review-security`
- Static scan: `python tools/ci/validate_security_surface.py`
- Full local CI with security: `python tools/ci/run_all.py --security`

## QA workflows

See `.agent-docs/workflows/README.md` for the full index. Execute in order: **WF4** (build/perf) → **WF1** (dual-mode) → **WF2** (LOTO compliance) → **WF3** (network).

## Key constraints

- C# code: no comments, `#region` grouping, naming conventions per `15-code-style.mdc`.
- Cross-platform: `Application.persistentDataPath` for saves, `Resources.Load` for assets — per `20-cross-platform.mdc`.
- Scoring: weights in `LotoObjectiveIds` must sum to 100%, approval threshold is 60%.
- Offline-first: gameplay never blocks on network; `IBManager` legacy path preserved.

## Local automation

- **Cursor Hooks:** `.cursor/hooks.json` reinforces MCP gates (`sessionStart`, `afterFileEdit`, `beforeMCPExecution`).
- **Local CI:** `python tools/ci/run_all.py` — workspace hygiene, LOTO matrix, C# sanity.
- **Merge gate (E/F):** `python tools/ci/merge_gate.py` — agent runs automatically at end of substantive tasks; see `.cursor/rules/85-merge-gate.mdc`.
- **GitHub Actions:** `.github/workflows/safemining-ci.yml` — runs on PR/push; enable Unity batchmode compile with repo variable `CI_UNITY_ENABLED=true` plus `UNITY_LICENSE` (and activation secrets).

## End-of-task protocol (Phases E & F)

The agent **always** runs merge gate + reviews after substantive work without being asked.

1. `python tools/ci/merge_gate.py`
2. MCP console/script validation if C# changed
3. `/review-bugbot` if C# changed
4. `/review-security` if backend paths changed
5. Chat section **"Tu intervención"** only when push, PR, Unity Editor, or merge needs the user
