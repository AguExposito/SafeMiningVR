# WF4 — Build & Performance Verification

Validates Quest 3S build configuration, rendering performance, script efficiency, and asset optimization for sustained 72 FPS on Meta Quest 3S.

**Source of truth:** `.cursor/rules/60-quest-performance.mdc`, `.agent-docs/qa-quest-fase6.md`, `Doc_SMVR/SafeMining_VR_v1.1.md` (RNF01, RNF03).

---

## Phase A — Build Settings Verification

### A1. Platform and compilation

| Setting | Required value | Actual | Pass/Fail |
|---------|---------------|--------|-----------|
| Active platform | Android | [ ] | [ ] |
| Scripting backend | IL2CPP | [ ] | [ ] |
| Architecture | ARM64 | [ ] | [ ] |
| Min API level | 32 | [ ] | [ ] |
| Target API level | 34 | [ ] | [ ] |
| Application ID | `com.safeminingvr.sim` | [ ] | [ ] |

- [ ] MCP verify: `Unity_ManageEditor` → confirm project settings match.
- [ ] **PASS:** All build settings correct.
- [ ] **FAIL:** Any setting differs → fix before proceeding.

### A2. Graphics and rendering

| Setting | Required value | Actual | Pass/Fail |
|---------|---------------|--------|-----------|
| Graphics API | Vulkan | [ ] | [ ] |
| OpenXR render mode | Single Pass Instanced | [ ] | [ ] |
| Multiview render regions | None | [ ] | [ ] |
| URP version | 17.x | [ ] | [ ] |

- [ ] **PASS:** Rendering configured for optimal Quest 3S stereo rendering.
- [ ] **FAIL:** OpenGL ES3 selected, or multi-pass rendering active.

### A3. Build scenes in correct order

| Build index | Scene | Present | Pass/Fail |
|------------|-------|---------|-----------|
| 0 | `StandardMainMenu` | [ ] | [ ] |
| 1 | `VR-LOTO-S1` | [ ] | [ ] |
| 2 | `VR-LOTO-S2` | [ ] | [ ] |

- [ ] `VR-LOTO-S3` is NOT in the build list (`VR-LOTO-S3.unity` may remain on disk).
- [ ] `VR-LOTO-Placeholder` is NOT in build list.
- [ ] **PASS:** Correct 3 scenes in correct order.
- [ ] **FAIL:** Wrong order, missing scenes, leftover S3 in build, or placeholder included.

### A4. APK build

- [ ] File → Build → `Builds/SafeMiningVR-Quest3S-Dev.apk`.
- [ ] Build completes without errors.
- [ ] APK size < 500 MB (reasonable for training sim).
- [ ] **PASS:** Clean build, reasonable size.
- [ ] **FAIL:** Build errors, bloated APK, or linker issues.

---

## Phase B — Scene Lighting Budget

Run for each build LOTO scene (S1, S2). S3 lighting lives on the nested `PREF_S3_ConveyorStation` inside S2.

### B1. Light count audit

For each scene, use `Unity_ManageScene` or `Unity_FindProjectAssets` type `Light`:

| Scene | Key light (Mixed Spot) | Fill lights (Mixed Point) | Directional sun | Pass/Fail |
|-------|----------------------|--------------------------|----------------|-----------|
| VR-LOTO-S1 | ≤1 [ ] | ≤2 [ ] | Disabled [ ] | [ ] |
| VR-LOTO-S2 | ≤1 [ ] | ≤2 [ ] | Disabled [ ] | [ ] |

- [ ] Soft shadows only on key light.
- [ ] Custom dark ambient for underground/industrial feel.
- [ ] **PASS:** All scenes within budget (1 key + 2 fill, no directional).
- [ ] **FAIL:** Excess lights, directional sun enabled, or hard shadows on fills.

### B2. Baked vs realtime

- [ ] Environment lighting is baked where possible.
- [ ] Only the key/fill lights listed above are realtime/mixed.
- [ ] No full-scene realtime GI.
- [ ] **PASS:** Minimal realtime lighting cost.
- [ ] **FAIL:** Excessive realtime lights or GI enabled.

---

## Phase C — Script Performance Audit

### C1. Empty Update/FixedUpdate check

Search all SafeMining scripts for empty Update loops:

```
Grep pattern="void\s+(Update|FixedUpdate)\s*\(\s*\)\s*\{?\s*\}?" glob="*.cs" path="Assets/SafeMining"
```

- [ ] **PASS:** Zero empty Update/FixedUpdate methods.
- [ ] **FAIL:** Empty methods found → remove them.

### C2. FindObjectOfType in Update check

```
Grep pattern="FindObjectOfType|FindObjectsByType" glob="*.cs" path="Assets/SafeMining"
```

For each match, verify it is NOT inside `Update()`, `FixedUpdate()`, or `LateUpdate()`:

- [ ] **PASS:** All FindObjectOfType calls are in `Awake()`, `Start()`, `Initialize()`, or one-time methods.
- [ ] **FAIL:** FindObjectOfType in update loop → refactor to cache in Start.

### C3. ControlCenter.Instance caching

```
Grep pattern="ControlCenter\.Instance" glob="*.cs" path="Assets/SafeMining"
```

Verify each reference:
- [ ] Cached in a local variable in `Start`/`Initialize` (not called per-frame).
- [ ] **PASS:** No per-frame ControlCenter.Instance access.
- [ ] **FAIL:** Uncached access in Update path → cache in Start.

### C4. GetComponent in hot paths

```
Grep pattern="GetComponent" glob="*.cs" path="Assets/SafeMining"
```

- [ ] All GetComponent calls are in `Awake()`, `Start()`, or `Initialize()`.
- [ ] None in `Update()`, `FixedUpdate()`, or `LateUpdate()`.
- [ ] **PASS:** Components cached at init.
- [ ] **FAIL:** GetComponent in hot path → cache.

### C5. Object pooling

- [ ] Frequently spawned objects (feedback visuals, highlight rings) use pooling, not `Instantiate`/`Destroy` in loops.
- [ ] **PASS:** Pool pattern used or no high-frequency spawn paths exist.
- [ ] **FAIL:** Instantiate/Destroy in Update-driven loops.

---

## Phase D — Asset Optimization

### D1. Mesh complexity

For imported meshes (Sketchfab, FBX) in SafeMining folders:

- [ ] No raw meshes with >50k triangles without LOD groups.
- [ ] LOD groups configured on hero props (conveyor, electrical panel, locker).
- [ ] Static batching flag enabled on non-moving environment geometry.
- [ ] **PASS:** Meshes optimized with LOD and batching.
- [ ] **FAIL:** High-poly meshes without LOD, or static batching disabled.

### D2. Texture sizes

| Category | Max size | Check |
|----------|---------|-------|
| Environment textures | 2048x2048 | [ ] |
| UI sprites | 1024x1024 | [ ] |
| Normal/detail maps | 1024x1024 | [ ] |

- [ ] **PASS:** All textures within budget.
- [ ] **FAIL:** Oversized textures → resize or compress.

### D3. Transparent overdraw

- [ ] HUD panels use opaque materials with alpha clip (not transparent blend).
- [ ] No full-screen transparent overlays.
- [ ] **PASS:** Minimal transparent overdraw.
- [ ] **FAIL:** Multiple transparent layers stacking.

### D4. Occlusion culling

- [ ] Occlusion culling enabled for industrial scenes (S1–S3).
- [ ] Occluder/occludee marking on large environment objects.
- [ ] **PASS:** Culling active and configured.
- [ ] **FAIL:** Culling disabled or not configured.

---

## Phase E — Runtime Performance Validation

### E1. FPS target on device

- [ ] Install APK on Quest 3S.
- [ ] Run full happy path (menu → S1 → S2 → S3 → summary → menu).
- [ ] Monitor with OVR Metrics Tool or `metavr` capture.
- [ ] **PASS:** Sustained ≥72 FPS throughout. No drops below 68 FPS for >500ms.
- [ ] **FAIL:** FPS drops below 68 sustained, or any frame >20ms.

### E2. Per-scene FPS check

| Scene | Min FPS | Avg FPS | Max frame time | Pass/Fail |
|-------|---------|---------|---------------|-----------|
| Menu | ≥72 | | | [ ] |
| S1 | ≥72 | | | [ ] |
| S2 | ≥72 | | | [ ] |
| S3 | ≥72 | | | [ ] |
| Summary | ≥72 | | | [ ] |

### E3. GC allocation check

- [ ] No GC allocations in `Update()` paths (verified via Unity Profiler or `Unity_Profiler_GetFrameGcAllocations`).
- [ ] **PASS:** Zero per-frame GC allocations in SafeMining scripts.
- [ ] **FAIL:** GC allocs in update loop → fix with caching, StringBuilder, or preallocated arrays.

### E4. Scene load times

| Transition | Target | Actual | Pass/Fail |
|-----------|--------|--------|-----------|
| Menu → S1 | <5s | | [ ] |
| S1 → S2 | <3s | | [ ] |
| S2 → S3 | <3s | | [ ] |
| S3 → Summary | <1s | | [ ] |

- [ ] **PASS:** All transitions within target.
- [ ] **FAIL:** Load time exceeds target → investigate asset loading.

### E5. Device logcat

- [ ] `adb logcat -s Unity` during full session → 0 Error-level entries.
- [ ] Or MCP: `get_device_logcat` filtered for errors.
- [ ] **PASS:** Clean logcat.
- [ ] **FAIL:** Any Debug.LogError or unhandled exception → investigate.

---

## Phase F — Console Hygiene (Editor)

### F1. Error count

- [ ] Play each scene in Editor → `Unity_ReadConsole` (Errors) → 0 entries.
- [ ] **PASS:** Zero errors for all 3 build scenes.
- [ ] **FAIL:** Any error → fix before proceeding.

### F2. Warning audit

- [ ] `Unity_ReadConsole` (Warnings) for SafeMining scripts.
- [ ] Warnings introduced by our changes: fix all.
- [ ] Pre-existing warnings: document but do not block.
- [ ] **PASS:** Zero new warnings.
- [ ] **FAIL:** New warnings introduced → fix.

---

## Sign-off matrix

| Phase | Tester | Date | Result |
|-------|--------|------|--------|
| A — Build settings | | | |
| B — Lighting budget | | | |
| C — Script performance | | | |
| D — Asset optimization | | | |
| E — Runtime performance | | | |
| F — Console hygiene | | | |

**Workflow complete when all phases show PASS. Phase E marked BLOCKED if no Quest 3S available — document and schedule device test.**
