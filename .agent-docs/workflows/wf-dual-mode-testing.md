# WF1 — Dual-Mode Testing & Ergonomics

Validates that every LOTO scene works correctly in both PC Editor (no headset) and Meta Quest 3S (hand tracking + controllers), with smooth modality transitions and non-invasive gamefeel.

**Prerequisites:** Unity Editor open on `D:/Unity/Projects/SafeMiningVR`. For device testing: Quest 3S with USB debugging enabled.

---

## Phase A — PC Playmode Validation

Execute in Unity Editor without VR hardware connected.

### A1. Scene launch

- [ ] Open `StandardMainMenu.unity` → Press Play.
- [ ] **PASS:** Menu renders, carousel is navigable with mouse clicks.
- [ ] **FAIL:** Black screen, null ref errors, or UI unresponsive to mouse.
- [ ] MCP gate: `Unity_ReadConsole` (Errors) → **require 0 entries**.

### A2. Full flow walkthrough

- [ ] Navigate carousel → select LOTO simulation → S1 loads.
- [ ] **PASS:** `LotoScenarioSceneBootstrap.Start()` runs without errors; briefing panel appears.
- [ ] **FAIL:** Scene loads but no briefing, or `ControlCenter.Instance` is null without prior direct-scene launch.

### A3. LotoPcPlaytestDriver cheats

Select the bootstrap GameObject in Inspector. Verify each cheat:

| Cheat | Hotkey | Expected result | Pass/Fail |
|-------|--------|-----------------|-----------|
| Dismiss briefing | Numpad 1 | Briefing closes, HUD shows objectives | [ ] |
| Equip 5 mandatory EPP | Numpad 2 | 5/5 EPP equipped, objective rows update | [ ] |
| Continue S1→S2 | Numpad 3 | Scene transitions to VR-LOTO-S2 | [ ] |
| Reject one defect | Numpad 4 | Defective EPP rejected, defect objective progresses | [ ] |
| Report 4 hazards | Numpad 5 | 4/4 hazards reported, objective rows update | [ ] |
| Continue S2→S3 | Numpad 6 | Scene transitions to VR-LOTO-S3 | [ ] |
| Run LOTO happy path | Numpad 7 | STOP→breaker→lock→tag→START→summary | [ ] |

- [ ] **PASS:** All 7 cheats produce expected results without console errors.
- [ ] **FAIL:** Any cheat triggers an exception, soft-locks, or leaves an inconsistent state.

### A4. HUD interaction via mouse

- [ ] Click briefing "Begin" button with mouse → briefing dismisses.
- [ ] Click objective row buttons (if any) → no accidental triggers on panel background.
- [ ] Click exit/menu button → exit-confirm dialog appears.
- [ ] **PASS:** `GraphicRaycaster` handles all mouse clicks on buttons. Panel background does NOT intercept rays.
- [ ] **FAIL:** Buttons unresponsive, or panel background blocks world interaction.

### A5. XR Interaction Simulator

- [ ] With Play Mode running, press `=` to toggle XR Interaction Simulator.
- [ ] Move simulated hands/controllers → verify rays interact with world objects.
- [ ] **PASS:** Simulator instantiates, interactors work, no duplicate EventSystems.
- [ ] **FAIL:** Simulator missing, double input processing, or camera breaks.

### A6. Direct scene launch (S1/S2/S3)

- [ ] Open `VR-LOTO-S1.unity` directly (without menu flow) → Press Play.
- [ ] **PASS:** Scene loads, `ControlCenter.Instance is null` warning appears (expected), but visual layout and PC cheats work for visual inspection.
- [ ] **FAIL:** Hard crash or scene fails to initialize visuals.
- [ ] Repeat for S2 and S3.

**Phase A sign-off:** All checkboxes passed → PC Playmode validated.

---

## Phase B — Meta Quest 3S Device Validation

### B1. Device connectivity

- [ ] `adb devices` shows Quest 3S serial number.
- [ ] MCP check: `metavr_device` returns device info.
- [ ] **PASS:** Device connected and authorized.
- [ ] **FAIL:** Empty device list → enable USB debugging in Quest settings.

### B2. Install and launch

- [ ] Build APK: `Builds/SafeMiningVR-Quest3S-Dev.apk`.
- [ ] Install: `adb install -r Builds/SafeMiningVR-Quest3S-Dev.apk` (or `metavr install`).
- [ ] Launch `com.safeminingvr.sim` from Quest app library.
- [ ] **PASS:** App launches, menu renders in VR.
- [ ] **FAIL:** Install error, black screen on launch, or crash.

### B3. Hand tracking — default modality

- [ ] Put down Touch controllers. Hands should be tracked.
- [ ] **PASS:** Hand meshes visible, pinch gesture triggers rays, no controller visuals.
- [ ] **FAIL:** Controllers visible when not held, hand meshes missing, or tracking lost.

### B4. Controller auto-switch

- [ ] Pick up one Touch controller.
- [ ] **PASS:** Within ~0.3s, controller mesh appears and hand mesh hides for that hand.
- [ ] Put controller down (release grip).
- [ ] **PASS:** After ~0.6s hysteresis, hand mesh returns and controller mesh hides.
- [ ] **FAIL:** Visuals stuck, no transition, or transition takes >1.5s.
- [ ] MCP verify: `LotoInputModalityVisuals` event flow — `XRInputModalityManager.trackedHandModeStarted` / `motionControllerModeStarted`.

### B5. Dwell-time reticle verification

For each trigger interactable (hazard spots in S2, buttons in S3):

- [ ] Point ray at interactable → radial fill ring appears at contact point.
- [ ] **PASS:** Ring color is Secondary `#F97316`, scale ~0.03m, fill duration between 0.8–1.2s.
- [ ] Move ray away before completion → ring resets smoothly.
- [ ] Hold until completion → interaction fires (hazard reported / button pressed).
- [ ] **FAIL:** No visual feedback, instant activation, fill too fast (<0.5s) or too slow (>2s), wrong color.

### B6. Grab interaction verification

For each grabbable (EPP items in S1, locks/tags in S3):

- [ ] Grab with hand tracking: pinch or fist gesture near object.
- [ ] Grab with Touch controller: grip button.
- [ ] **PASS:** Object attaches to hand/controller, follows movement, releases on open/grip release.
- [ ] **FAIL:** Object snaps to wrong position, doesn't release, or only works with one modality.
- [ ] Confirm: NO dwell reticle appears on grab interactables.

### B7. Outline hint validation

| Scenario | Expected hints | Pass/Fail |
|----------|---------------|-----------|
| S1 start | Pending mandatory EPP items outlined (`#F97316`) | [ ] |
| S1 all equipped | Continue door outlined | [ ] |
| S2 start | Unreported real hazards outlined (NOT decoys) | [ ] |
| S2 all reported | Continue door outlined | [ ] |
| S3 start | STOP button outlined | [ ] |
| S3 after STOP | Breaker outlined | [ ] |
| S3 after breaker | Lock socket outlined | [ ] |
| S3 after lock | Tag socket outlined | [ ] |
| S3 after tag | START button outlined | [ ] |

- [ ] **PASS:** One hint set active at a time, correct color, clears on step completion.
- [ ] **FAIL:** Multiple hint sets visible, hints on wrong objects, or hints don't clear.

### B8. HUD legibility & comfort

- [ ] HUD follows head at ~1.75m distance via `Util_PanelFollower`.
- [ ] Objective text readable at Quest resolution (~1832x1920/eye).
- [ ] Body text ≥24pt TMP equivalent. Touch targets ≥48x48px.
- [ ] Contrast ratio ≥4.5:1 on text panels (WCAG AA).
- [ ] **PASS:** All HUD elements legible, comfortable distance, no motion sickness from follow.
- [ ] **FAIL:** Text blurry, panels too close/far, or aggressive follow causing discomfort.

### B9. Haptics & feedback sobriety

- [ ] Lock snap → short haptic pulse (100–200ms).
- [ ] Error feedback (wrong lock, wrong tag) → subtitle + audio, NO camera shake.
- [ ] Hover over interactable → NO haptic spam.
- [ ] **PASS:** Haptics are brief and meaningful, feedback is pedagogical not dramatic.
- [ ] **FAIL:** Camera shake, continuous vibration, particle explosions, or score pop-ups.

### B10. Teleport locomotion

- [ ] Point at floor `TeleportationArea` → teleport reticle appears.
- [ ] Trigger teleport → player moves, tunneling vignette activates.
- [ ] **PASS:** Smooth teleport, vignette reduces motion sickness, no clipping.
- [ ] **FAIL:** Missing vignette, teleport to invalid areas, or smooth locomotion active.

### B11. Device console check

- [ ] `adb logcat -s Unity | grep -i error` → 0 lines.
- [ ] Or MCP: `get_device_logcat` filtered for errors.
- [ ] **PASS:** Zero Unity errors in logcat.
- [ ] **FAIL:** Any `Debug.LogError` or unhandled exception.

**Phase B sign-off:** All checkboxes passed → Quest 3S device validated.

---

## Sign-off matrix

| Phase | Tester | Date | Result |
|-------|--------|------|--------|
| A — PC Playmode | | | |
| B — Quest 3S Device | | | |

**Workflow complete when both phases show PASS.**
