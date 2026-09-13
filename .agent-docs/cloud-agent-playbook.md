# SafeMining VR — Cloud & Background Agent Playbook

Operational protocol for **Cursor Cloud Agents** (`/in-cloud`) and **background subagents** running long-duration, asynchronous work on the LOTO Quest 3S training simulation.

**Project root:** `D:/Unity/Projects/SafeMiningVR`

---

## When to use Cloud vs Local

| Mode | Use when | Isolation |
|------|----------|-----------|
| **Cloud** (`/in-cloud`) | WF2 full audit, S3 interactable refactor (10+ files), Quest perf trace analysis, CI babysit (`/babysit`) | Dedicated VM + branch |
| **Background subagent** | Parallel research (explore codebase) while you keep editing locally | Shared checkout — risk of file conflicts |
| **Foreground subagent** | Focused domain task ≤3 files, quick compliance slice | Shared checkout |

**Rule:** Any task estimated >30 minutes or touching >5 files → **Cloud Agent on a dedicated branch**.

---

## Branch strategy (mandatory)

Create an isolated branch **before** launching the cloud agent. Never run long jobs on `main`/`master`/`develop`.

| Prefix | Purpose | Example |
|--------|---------|---------|
| `feature/` | New capability | `feature/s3-dwell-reticle-unify` |
| `audit/` | Compliance / QA workflow execution | `audit/wf2-loto-compliance-2026-08` |
| `refactor/` | Large structural change | `refactor/s3-interactables-xri-3.3` |
| `perf/` | Performance investigation + fixes | `perf/s1-locker-72fps-quest3s` |
| `security/` | Security hardening / adapter review | `security/fase7-adapter-auth-headers` |

### Branch protocol

```bash
git fetch origin
git checkout develop
git pull origin develop
git checkout -b audit/wf2-loto-compliance-2026-08
git push -u origin audit/wf2-loto-compliance-2026-08
```

Cloud agent works on this branch only. Parent agent merges via PR after verification.

---

## Launch sequence

1. **Pre-flight (local or cloud)**
   - `Unity_ManageEditor` → `GetProjectRoot` = `D:/Unity/Projects/SafeMiningVR`
   - `python tools/ci/run_all.py` — baseline hygiene + LOTO matrix
   - `Unity_ReadConsole` (Errors) → 0 entries

2. **Create branch** (see above)

3. **Launch cloud agent**
   ```
   /in-cloud
   <paste self-contained brief from .agent-docs/templates/cloud-agent-briefs.md>
   ```

4. **Assign orchestrator** (optional first line in brief)
   ```
   /safemining-orchestrator route and delegate this cloud task.
   ```

5. **Monitor** — Cloud Agents Window in Cursor desktop app

6. **Merge gate** — PR must pass checklist in [Completion criteria](#completion-criteria-before-merge)

---

## Subagent routing (native)

| Task type | Native subagent | Workflow | Skill |
|-----------|-----------------|----------|-------|
| WF2 full compliance audit | `/loto-compliance` | WF2 | `safemining-compliance` |
| S3 interactable refactor | `/vr-usability-interaction` | WF1 | `meta-xr-interaction` |
| Quest perf analysis | `/unity-performance` | WF4 | — |
| Backend / sync / auth | `/network-session` | WF3 | `network-session-pipeline` |
| Multi-domain | `/safemining-orchestrator` | varies | `safemining-orchestrator` |

Legacy launchers (`agent-*` skills) remain valid but prefer native `/name` subagents.

---

## Self-contained brief structure

Every cloud/background prompt must include these sections (copy from templates):

1. **Objective** — one sentence outcome
2. **Branch** — exact branch name
3. **Scope** — files/scenes in/out of scope
4. **Sources** — `@` references (workflows, rules, INDEX line ranges)
5. **Subagent** — assigned specialist
6. **MCP gates** — mandatory calls
7. **Constraints** — serious-games, offline-first, no arcade mechanics
8. **Deliverables** — evidence + report format
9. **Done definition** — explicit pass/fail criteria

Templates: `.agent-docs/templates/cloud-agent-briefs.md`

---

## Completion criteria (before merge)

A cloud/background job is **complete** only when the PR or final summary includes:

### A. Classification & routing

- [ ] Domain(s) assigned and subagent(s) used
- [ ] Workflow ID executed (WF1–WF4) if applicable

### B. MCP evidence

- [ ] `GetProjectRoot` verified
- [ ] `Unity_ReadConsole` (Errors) → 0 after all changes
- [ ] `Unity_ValidateScript` on every new/modified `.cs`
- [ ] `meta_docs_search` logged if XR/Quest APIs touched

### C. Automated checks

- [ ] `python tools/ci/run_all.py` passes
- [ ] If backend paths changed: `python tools/ci/validate_security_surface.py` passes
- [ ] GitHub Actions `validate` job green on PR

### D. Domain deliverables

| Domain | Required evidence |
|--------|-------------------|
| WF2 / compliance | Sign-off matrix (Phases A–F) with PASS/FAIL per item |
| XR / interactables | Dual-modality checklist (hands + controllers + PC bypass) |
| Perf | Frame budget summary, lighting count, profiler snapshot or Perfetto ID |
| Network / sync | DTO schema diff, offline-first path unchanged, IBManager untouched |

### E. PR package

- [ ] PR title: `[domain] short description`
- [ ] PR body: Classification, gates passed, changes, verification, risks
- [ ] No `.unity`/`.prefab` YAML hand-edits (MCP for HUD / empty logic GOs only; world art frozen)
- [ ] Security review triggered if paths match `80-security-review.mdc` globs

### F. Reviews before merge

- [ ] `/review-bugbot` on branch changes (or Bugbot PR check)
- [ ] `/review-security` if Adapters, SyncQueue, DTOs, or IBManager changed

---

## Phases E & F — automatic (default)

The **agent** runs E/F after every substantive task without user prompt. Rule: `.cursor/rules/85-merge-gate.mdc`.

| Step | Who | Command / action |
|------|-----|------------------|
| E1 CI + PR draft | Agent auto | `python tools/ci/merge_gate.py` |
| E2 Unity gates | Agent auto | `Unity_ReadConsole`, `Unity_ValidateScript` |
| E3 Push / PR | User **only if** branch unpushed | `git push` + GitHub PR |
| F1 Bugbot | Agent auto | `/review-bugbot` when `.cs` changed |
| F2 Security | Agent auto | `/review-security` when backend paths changed |
| F3 Merge | User **only if** reviews pass | Approve PR on GitHub |

Agent ends with **"Tu intervención"** section only when steps require the user, with exact commands.

---

## Anti-patterns

- Running cloud agent on `main` without a feature branch
- Briefs without workflow/rule references (agent invents requirements)
- Merging without `Unity_ReadConsole` = 0 evidence
- Background parallel agents editing the same files (use cloud isolation or sequential execution)
- Skipping security review on Fase 7 HTTP adapter work

---

## Quick commands

```text
/in-cloud
<brief from templates>

/babysit
<PR URL or branch — iterate until CI green>

/review-security
Review branch changes against 80-security-review.mdc

/review-bugbot
Review branch changes for regressions in LOTO simulation code
```
