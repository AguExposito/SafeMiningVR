---
name: meta-xr-interaction
description: >-
  Setup, validate, and extend XR interactions for the SafeMining LOTO simulation using XRI 3.3.1 and Meta OpenXR on Quest 3S.
  Covers hand/controller auto-switch, dwell-time reticle creation, outline hint wiring, grab/socket/poke interactables,
  and XRS_HandGrab integration. Use when adding new interactables, modifying input modality, creating dwell-time triggers,
  or debugging hand tracking issues.
---

# Meta XR Interaction & Hand/Controller Sync

## Purpose

Guide the agent through creating, wiring, and validating XR interactables for the LOTO simulation, ensuring they work with both hand tracking (default) and Touch controllers (auto-switch), follow the dwell-time reticle pattern for triggers, and reuse existing XRI / Meta SDK components.

## When to use

- Adding a new interactable object to any LOTO scene (S1–S3).
- Implementing or modifying dwell-time trigger behavior on buttons, hazard spots, or panel controls.
- Debugging hand tracking grab failures or controller fallback issues.
- Wiring `LotoOutlineHint` to a new pedagogical step.
- Setting up a new `XRSocketInteractor` for lock/tag placement.

## Preconditions

1. Unity MCP connected — verify with `Unity_ManageEditor` → `GetProjectRoot` = `D:/Unity/Projects/SafeMiningVR`.
2. XR packages installed: `com.unity.xr.interaction.toolkit` 3.3.1, `com.unity.xr.hands` 1.7.3, `com.unity.xr.meta-openxr` 2.4.0.
3. `PlayerMR` prefab present in scene with `XRInputModalityManager` and `LotoInputModalityVisuals`.

## Procedure: Add a new grab interactable

1. **Choose the right base.** Physical objects the trainee picks up (EPP, locks, tags) use `XRGrabInteractable`:

```csharp
[RequireComponent(typeof(ref_YourObject))]
public class YourInteractable : MonoBehaviour
{
    [SerializeField] private YourManager manager;
    private ref_YourObject objectRef;

    private void Awake()
    {
        objectRef = GetComponent<ref_YourObject>();
    }

    public void Configure(YourManager mgr)
    {
        manager = mgr;
    }
}
```

2. **Set interaction layers.** If it should respond to `XRS_HandGrab` fist gesture, assign the `XRS Exclusive Grab` layer on the interactable. Standard ray/poke stays on `Default`.

3. **Add Rigidbody** (isKinematic = true for snappable items, false for throwable). Attach collider matching the visual mesh.

4. **Wire to manager.** The scenario controller's `Begin()` method calls `interactable.Configure(this.manager)`. Never self-wire in `Awake`.

5. **Verify dual modality.** Enter Play Mode → XR Interaction Simulator. Grab with simulated hand (H key + fist). Then grab with simulated controller. Both must work.

## Procedure: Add a dwell-time trigger interactable

For non-grab interactions (hazard reporting, buttons, breaker switches):

1. **Use `XRSimpleInteractable`** on the GameObject. It fires `hoverEntered` / `hoverExited` for dwell tracking.

2. **Create or reuse a dwell-time component.** The component should:
   - Listen to `hoverEntered` → start filling radial UI.
   - Listen to `hoverExited` → cancel and reset fill.
   - On fill complete (0.8–1.2 s) → invoke the action and play confirmation audio.
   - Visual: world-space `Canvas` child with radial `Image` (fill method = Radial 360), color = Secondary `#F97316`.
   - Scale: ~0.03 m at pointer contact point.

3. **Wire interaction event** to the manager via the same `Configure()` + `ActivateFromPc()` pattern used by `HazardReportInteractable` and `LotoControlButtonInteractable`.

4. **PC fallback.** Ensure `ActivateFromPc()` is public so `LotoPcPlaytestDriver` can invoke it directly, bypassing dwell.

## Procedure: Wire outline hints to a new step

1. Locate the scenario manager (e.g., `EppEquipmentManager`, `HazardInspectionManager`, `LotoSequenceManager`).

2. Find the method that fires after each step completion. Add or modify the `RefreshOutlineHints()` call.

3. `LotoOutlineHint.Show(targets)` highlights the next actionable objects. `LotoOutlineHint.ClearAll()` removes all outlines.

4. Color is always Secondary `#F97316`, width ~4. One active set at a time.

## Procedure: Validate input modality auto-switch

1. Confirm `LotoInputModalityVisuals` is on `PlayerMR` with serialized refs to `leftHand`, `rightHand`, `leftController`, `rightController`.

2. `XRInputModalityManager` events `trackedHandModeStarted` / `motionControllerModeStarted` drive the switch.

3. New hand/controller visuals must subscribe to `LotoInputModalityVisuals.OnHandsModeChanged` to sync visibility.

4. Hysteresis delay (0.6 s) prevents flicker when controllers are briefly detected.

## MCP integration

- **Before creating interactables:** call `meta_docs_search` with the interaction type (e.g., "XRGrabInteractable hand tracking") to verify no API changes in current Meta OpenXR version.
- **After wiring:** call `Unity_ReadConsole` (Errors) — require 0 entries.
- **Scene authoring:** use `Unity_ManageGameObject` to add components; never hand-edit prefab YAML.

## Key project files

| File | Role |
|------|------|
| `Code/Interactables/LotoInputModalityVisuals.cs` | Hand/controller auto-switch |
| `Code/Interactables/LotoOutlineHint.cs` | Pedagogical outline hints |
| `Code/Interactables/HazardReportInteractable.cs` | S2 dwell-trigger reference pattern |
| `Code/Interactables/LotoControlButtonInteractable.cs` | S3 button trigger reference pattern |
| `Code/Interactables/EppItemInteractable.cs` | S1 grab reference pattern |
| `Code/Interactables/LotoLockInteractable.cs` | S3 socket/grab reference pattern |
| `XR Standard Menu/Code/XRS_HandGrab.cs` | Fist-based grab for hand tracking |

## Checklist

- [ ] Interactable works with hand tracking (pinch or fist grab).
- [ ] Interactable works with Touch controllers (grip/trigger).
- [ ] Dwell-time triggers have radial fill visual + configurable duration.
- [ ] Grabs do NOT use dwell — direct physical interaction only.
- [ ] Outline hint shows on the correct pedagogical step and clears on completion.
- [ ] `ActivateFromPc()` method exists for PC playtest bypass.
- [ ] `Unity_ReadConsole` shows 0 errors after integration.
