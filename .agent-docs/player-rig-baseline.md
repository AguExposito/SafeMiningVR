# Player Rig Baseline — Fase 0

Canonical contract for updating the SafeMining player (PC + VR). Distinct from the project-wide audit in [audit-alignment-baseline.md](audit-alignment-baseline.md).

## Metadata

| Field | Value |
|-------|-------|
| Date | 2026-08-30 |
| Commit | `def1ee3c549ea261141d827248b989d872fa1674` |
| Unity | 6000.3.10f1 |
| Project root | `D:/Unity/Projects/SafeMiningVR` |
| MCP scene | `VR-LOTO-S1` |
| Evidence | `Unity_ManageGameObject` on `PlayerMR` / `TunnelingVignette` |

## Architecture target (frozen)

**Dual prefabs + device selector** — not a single unified rig.

| Prefab | Base | Role |
|--------|------|------|
| `PREF_LotoPlayerVR` | XRI sample `XR Origin Hands (XR Rig)` + `*QuestVisual` | Hands-first Quest; no artificial gravity; SnapTurn; vignette wired; no PC scripts |
| `PREF_LotoPlayerPC` | Clean evolution of `PlayerMR` | WASD/click, CharacterController, Editor cheats, Simulator-friendly |

| Concern | Decision |
|---------|----------|
| Selector | `LotoPlayerRigSelector` — `Auto` / `ForcePc` / `ForceVr` |
| Auto rule | Editor/Standalone → PC; Android Quest → VR |
| Lifetime | One XR Origin per scene (not DontDestroyOnLoad) |
| Hub DDOL | `ControlCenter` only |
| Stack | XRI 3.3.1 + Meta OpenXR 2.4.0 — no `OVRCameraRig` unless Meta docs require it |
| Teleport bind | `LotoScenarioSceneBootstrap` rewires floors to the active rig provider |

## Current state (`PlayerMR`)

Source prefab: `Assets/XR Standard Menu/PlayerMR.prefab`. Instances: `StandardMainMenu`, `VR-LOTO-S1/S2/S3`, `Testing`.

| Area | Status | Notes |
|------|--------|-------|
| Hands-first + `XRInputModalityManager` + `LotoInputModalityVisuals` | PASS | Hands on; controllers off by default |
| `TeleportationProvider`; continuous Move off | PASS | Bootstrap binds `TeleportationArea` |
| PC: `LotoPcLocomotion` + `LotoPcClickInteractor` | PASS | Editor / non-device path |
| `TunnelingVignetteController` | GAP | Component present; all `locomotionProvider` refs **null** (MCP 2026-08-30) |
| `GravityProvider` + PC gravity | GAP | Conflicts with standing Quest (no artificial gravity) |
| `ContinuousTurnProvider` + `SnapTurnProvider` | GAP | Continuous turn active — comfort risk |
| `ARPlaneManager` | GAP | MR leftover / cost |
| `Climb` active | GAP | Unused leftover |
| Legacy `VR_Player.prefab` | DELETE candidate | No scene/prefab/C# references |

## Gaps vs `40-vr-interaction.mdc`

| Requirement | Status |
|-------------|--------|
| Hands-first default | PASS |
| Modality manager + `LotoInputModalityVisuals` | PASS |
| Teleportation / floor areas | PASS |
| TunnelingVignette on locomotion providers | FAIL — refs null |
| Prefer teleport over smooth move | PARTIAL — Move off; ContinuousTurn still on |
| No `OVRCameraRig` unless needed | PASS |
| PC playtest path | PASS |

## Create / modify / delete (orientation — Fases 1–4)

### Create

- `PREF_LotoPlayerVR` from `Assets/Samples/XR Interaction Toolkit/3.3.1/Hands Interaction Demo/Prefabs/XR Origin Hands (XR Rig).prefab`
- `PREF_LotoPlayerPC` (or cleaned rename of `PlayerMR`)
- `LotoPlayerRigSelector` (+ bootstrap spawn/bind)
- Handbook / rule updates after scene migration (Fase 4)

### Modify

- `LotoScenarioSceneBootstrap` — activate/bind selected rig
- `LotoInputModalityVisuals` — refs on both prefabs
- Build scenes 0–3 + `Testing` via Unity MCP
- Docs already pointed here; rules/skills after migration

### Delete / disable

- `ARPlaneManager` on training rig
- Gravity on VR path; ContinuousTurn by default
- Climb/Jump leftovers if unused
- `Assets/XR Standard Menu/VR_Player.prefab` after confirm unused
- Orphan `PlayerMR` instances after dual migration

## Acceptance criteria (player only)

Done when Fases 1–5 close. Does **not** require full S1–S3 happy path.

| ID | Mode | Criterion |
|----|------|-----------|
| ACC-PC-01 | Editor ForcePc / Auto Editor | WASD + look; click interactables; `LotoPcPlaytestDriver` cheats |
| ACC-PC-02 | Editor | Single active XR Origin; HUD follow `Camera.main` |
| ACC-VR-01 | Quest / ForceVr | Quest hand visuals default; controllers via modality |
| ACC-VR-02 | Quest | No artificial gravity (origin does not fall) |
| ACC-VR-03 | Quest | SnapTurn on; ContinuousTurn off; vignette providers assigned |
| ACC-VR-04 | Quest standing | Floor / room-scale; physical play area |
| ACC-SEL-01 | Both | Auto → PC in Editor, VR on Android |
| ACC-SEL-02 | Editor | `ForcePc` / `ForceVr` switch rig without project reload |
| ACC-COM-01 | Both | Zero rig setup `LogError`; no `OVRCameraRig` |

Sign-off checklist also in [workflows/wf-dual-mode-testing.md](workflows/wf-dual-mode-testing.md) (Player rig acceptance section).

## Out of scope

Scoring, scenario gates S1–S3, LOTO procedure fidelity, backend sync, JWT / Fase 7.

## Pointers

| Asset | Path |
|-------|------|
| Sample VR base | `Assets/Samples/XR Interaction Toolkit/3.3.1/Hands Interaction Demo/Prefabs/XR Origin Hands (XR Rig).prefab` |
| Quest hand visuals | `…/Hands Interaction Demo/Prefabs/LeftHandQuestVisual.prefab`, `RightHandQuestVisual.prefab` |
| Current unified rig | `Assets/XR Standard Menu/PlayerMR.prefab` |
| Legacy unused | `Assets/XR Standard Menu/VR_Player.prefab` |
| Rule | `.cursor/rules/40-vr-interaction.mdc` |

## Fase 1 status (2026-08-30)

- Created: `Assets/SafeMining/LOTOSimulation/Prefabs/PREF_LotoPlayerVR.prefab` (from XRI Hands sample).
- Vignette GAP **closed on VR prefab** (`TunnelingVignette` → TeleportationProvider + SnapTurnProvider). `PlayerMR` vignette refs remain GAP until Fase 2/4.
- Sandbox: `Testing.unity` has `PREF_LotoPlayerVR` instance; legacy `PlayerMR` disabled in that scene only.
- `XROrigin.RequestedTrackingOriginMode` = **Floor** (standing / room-scale).
- ACC-VR-01…04 / ACC-COM-01 prefab checks: PASS (Editor MCP). Build scenes 0–3 unchanged.
