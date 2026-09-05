---
name: dual-mode-playtest
description: >-
  Automate and validate PC Editor playtesting and Quest 3S device builds for the SafeMining LOTO simulation.
  Covers XR Interaction Simulator setup, LotoPcPlaytestDriver cheats, scene bootstrap validation,
  dual-raycaster HUD config, and APK build/deploy workflow. Use when testing scenes without a headset,
  setting up PC playtest shortcuts, preparing Quest builds,   or debugging camera/input rig issues.
---

# Dual-Mode Rig & PC Playtesting

## Purpose

Ensure every LOTO scene (S1–S3) is testable both in PC Editor (no headset) and on Meta Quest 3S hardware, without breaking camera rigs, input systems, or HUD raycasting when switching between modes.

## When to use

- Running a quick smoke test of a scenario in Editor without VR hardware.
- Verifying that a newly added interactable responds to both XR Interaction Simulator and mouse.
- Preparing and deploying a Quest 3S APK build.
- Debugging a broken camera rig, missing EventSystem, or HUD that doesn't respond to clicks.

## Preconditions

1. Unity MCP connected to `D:/Unity/Projects/SafeMiningVR`.
2. Build Settings contain: `StandardMainMenu` (0), `VR-LOTO-S1` (1), `VR-LOTO-S2` (2). LOTO S3 runs in-scene inside S2.
3. `LotoScenarioSceneBootstrap` present on each build S scene's bootstrap GameObject.

## Procedure: PC playtest smoke test

### Step 1 — Launch scene

Open `StandardMainMenu.unity` (for full flow) or any `VR-LOTO-S*` scene directly. Press Play.

### Step 2 — Verify auto-setup

`LotoScenarioSceneBootstrap.Start()` auto-runs:
- Resolves `panelFollowTarget` from `Camera.main`.
- Configures all `Util_PanelFollower` instances.
- Adds `LotoPcPlaytestDriver` if absent.
- Binds `TeleportationArea` instances to the scene's `TeleportationProvider`.

If `ControlCenter.Instance` is null, the scene was opened directly without menu flow — expected warning, not a blocker for visual testing.

### Step 3 — Use PC cheats

Select the bootstrap GameObject → Inspector → `LotoPcPlaytestDriver`:

| Inspector bool | Hotkey | Effect |
|----------------|--------|--------|
| `dismissBriefing` | Numpad 1 | Skip briefing panel |
| `equipMandatoryFive` | Numpad 2 | Auto-equip 5 mandatory EPP |
| `rejectOneDefect` | Numpad 4 | Reject one defective EPP |
| `continueS1ToS2` | Numpad 3 | Transition to S2 |
| `reportFourHazards` | Numpad 5 | Report all 4 real hazards |
| `continueS2ToS3` | Numpad 6 | Transition to S3 |
| `runLotoHappyPath` | Numpad 7 | Execute full LOTO sequence |
| `forceNoGloves` | — | Trigger Malo 2 path |
| `forceIncompleteStart` | — | Trigger Malo 1 path |
| `confirmExit` | — | Trigger exit confirm dialog |
| `completeSummary` | — | Complete session summary |

Context Menu alternative: right-click component → `LOTO/PC/…` items.

### Step 4 — Verify HUD interaction

- Click buttons on briefing panel and HUD with mouse → `GraphicRaycaster` handles this.
- If VR pointer is simulated (XR Interaction Simulator active, toggle with `=` key), buttons also respond to `TrackedDeviceGraphicRaycaster`.
- Panel backgrounds must NOT block rays — verify `Raycast Target` is off on non-button images.

### Step 5 — Validate console

Call `Unity_ReadConsole` (Errors). Require 0 entries. Warnings about missing `ControlCenter` when opening S scenes directly are acceptable.

## Procedure: Quest 3S build and deploy

### Step 1 — Verify build settings

| Setting | Required value |
|---------|---------------|
| Platform | Android |
| Scripting backend | IL2CPP |
| Architecture | ARM64 |
| Min API | 32 |
| Target API | 34 |
| Graphics API | Vulkan |
| OpenXR render mode | Single Pass Instanced |
| Package name | `com.safeminingvr.sim` |

### Step 2 — Build APK

File → Build and Run, or Build to `Builds/SafeMiningVR-Quest3S-Dev.apk`.

### Step 3 — Deploy and test

```bash
adb install -r Builds/SafeMiningVR-Quest3S-Dev.apk
```

Or via Meta Quest Developer Hub / `metavr install`.

### Step 4 — Device smoke test checklist

- [ ] Launch app → menu renders, carousel works.
- [ ] Select LOTO → S1 loads with briefing.
- [ ] Complete S1→S2→S3→summary→menu (happy path).
- [ ] Verify hand tracking is default mode (no controllers in hand).
- [ ] Pick up Touch controller → visuals switch to controllers.
- [ ] Put down controller → visuals return to hands (~0.6 s delay).
- [ ] HUD follows head at ~1.75 m, text is legible.
- [ ] Capture logcat: `adb logcat -s Unity` — 0 Error lines.
- [ ] Check FPS: OVR Metrics Tool or `metavr` capture → ≥72 FPS on happy path.

## Procedure: Debug broken camera/input rig

1. **No HUD visible:** check `Util_PanelFollower.SetTarget()` — target must be `Camera.main.transform`. Verify `panelFollowTarget` on bootstrap.

2. **Mouse doesn't click buttons:** ensure `GraphicRaycaster` component on the Canvas AND an `EventSystem` in the scene. `LotoScenarioSceneBootstrap` does NOT create EventSystem — it should exist on the `PlayerMR` or scene root.

3. **XR Interaction Simulator not appearing:** it auto-instantiates in Editor if `com.unity.xr.interaction.toolkit` is installed. If missing, check Project Settings → XR Plug-in Management → ensure `XR Interaction Toolkit` sample "XR Device Simulator" is imported.

4. **Controller visuals stuck:** check `LotoInputModalityVisuals` serialized refs. All four fields (`leftHand`, `rightHand`, `leftController`, `rightController`) must be assigned on `PlayerMR`.

## MCP integration

- `Unity_ManageEditor` → `GetProjectRoot` for gate check.
- `Unity_ReadConsole` (Errors) after every playtest session.
- `Unity_GetConsoleLogs` to inspect warnings if behavior is unexpected.
- `metavr_device` / `metavr_run` for device deployment when Quest is connected.

## Key project files

| File | Role |
|------|------|
| `Code/Managers/LotoScenarioSceneBootstrap.cs` | Scene auto-setup and PC driver injection |
| `Code/Debug/LotoPcPlaytestDriver.cs` | Inspector bools, hotkeys, scenario cheats |
| `Code/Interactables/LotoInputModalityVisuals.cs` | Hand/controller visual auto-switch |
| `Code/Debug/LotoPcLocomotion.cs` | WASD locomotion for Editor testing |
| `Code/Debug/LotoPcClickInteractor.cs` | Mouse click → interactable activation |
