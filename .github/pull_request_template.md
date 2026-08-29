## Summary

<!-- What changed and why (1–3 sentences) -->

## Domain & routing

- [ ] Domain: `xr` | `network` | `compliance` | `perf` | `arch` | `security`
- [ ] Subagent(s) used: `/...`
- [ ] Workflow: WF1 | WF2 | WF3 | WF4 | N/A

## MCP verification

- [ ] `Unity_ReadConsole` (Errors) → 0 entries
- [ ] `Unity_ValidateScript` on modified `.cs` files
- [ ] `meta_docs_search` if XR/Quest APIs touched

## Automated checks

- [ ] `python tools/ci/run_all.py` passes locally
- [ ] GitHub Actions `validate` job green

## Security (required if Adapters, DTOs, SyncQueue, IBManager, or score logic changed)

- [ ] `python tools/ci/validate_security_surface.py` passes
- [ ] `/review-security` completed — 0 `[CRITICAL]`, 0 open `[HIGH]`
- [ ] Offline-first preserved — gameplay does not block on network
- [ ] IBManager legacy path untouched

## Bugbot (required for cloud agent PRs and gameplay changes)

- [ ] `/review-bugbot` completed — 0 `[CRITICAL]`

## Test plan

- [ ] PC Editor Play Mode
- [ ] Quest 3S device (if XR/interaction changed)
- [ ] Scenario slice tested: S1 | S2 | S3 | full flow

## Risks & follow-ups

<!-- Known limitations or deferred items -->
