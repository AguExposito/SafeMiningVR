---
name: safemining-compliance
description: >-
  Validate SafeMining LOTO simulation against functional/non-functional requirements, branding palette,
  naming conventions, C# code style, and Quest 3S performance budgets.
  Covers RN/RNF/HU traceability, brand color audit, asset naming, code pattern compliance,
  world-space UI standards, and XR performance checks.
  Use when auditing code or assets for compliance, verifying requirement coverage, checking brand adherence,
  or reviewing a pull request for SafeMining standards.
---

# SafeMining Compliance & Asset Audit

## Purpose

Systematically verify that LOTO simulation code, assets, and scenes conform to the SafeMining VR specification (functional and non-functional requirements), corporate branding, naming conventions, and Quest 3S performance budgets.

## When to use

- Before merging a feature branch or completing a development slice.
- After adding new UI elements, interactables, or scenes.
- When auditing an existing scene for branding or performance compliance.
- When reviewing code for adherence to project C# patterns.
- When verifying requirement traceability for a specific RN/RNF/HU.

## Preconditions

1. Unity MCP connected to `C:/AGU/Unity/Projects/SafeMiningVR`.
2. Access to `Doc_SMVR/SafeMining_VR_v1.1.md` as requirements source of truth.
3. Access to `.agent-docs/` for operational knowledge base.
4. Cursor Rules `.cursor/rules/*.mdc` loaded (especially `15-code-style`, `50-hud-ui-brand`, `60-quest-performance`).

## Procedure: Requirement traceability audit

### Step 1 — Identify applicable requirements

Review the feature or scene being audited. Map it to the LOTO scenario(s):

| Scenario | Key requirements |
|----------|-----------------|
| S1 — EPP Selection | RN01 (select 5 mandatory EPP), RN03 (reject ≥1 defective), RN05 (briefing) |
| S2 — Hazard Inspection | RN01 25% + HU-222 types; gate `IsS2Complete` (board counts) |
| S3 — LOTO Procedure | RN04 pedagogy STOP→breaker→lock→tag→START; board has no tag flag |
| Cross-cutting | RN09 (session register before load), RNF01 (Quest 3S), RNF03 (72 FPS), RNF04 (offline-first), RNF08 (JWT 2h expiry) |

### Step 2 — Verify objective IDs and weights

Confirm the code uses the canonical objective IDs from `LotoObjectiveIds`. Gates live on `LotoGameManager`. HUD/lobby checks go through `ScoreManager` via `Util_LotoSimulationBridge.SetObjective` (indices in `LotoSimulationConstants`). Do not audit leftover `EppEquipmentManager` / `HazardInspectionManager` / `LotoSequenceManager` as source of truth. Contract: `.agent-docs/loto-game-board.md`.

| Objective | ID constant | Weight |
|-----------|------------|--------|
| EPP selection | `EppSelection` | 20% |
| EPP defect | `EppDefect` | 10% |
| Hazard report | `HazardReport` | 25% |
| LOTO sequence | `LotoSequence` | 35% |
| Zero energy | `ZeroEnergy` | 10% |

Approval threshold: `DefaultApprovalThresholdPercent = 60`.

### Step 3 — Check scoring edge cases

- Full score (100): all 5 objectives achieved, 0 false positives.
- Threshold pass (60): EPP selection (20) + EPP defect (10) + hazard report with 0 FP (25) + at least 5 from loto/zero = 60.
- Hazard penalty: each false positive reduces hazard partial by 25% of weight.
- Abandoned sessions: `completionStatus = "Abandoned"`, score still computed.

## Procedure: Brand palette audit

### Step 1 — Verify color usage in code

Search for hardcoded hex values or Color constructors. All runtime colors must come from `Brand.*` accessors:

| Token | Hex | Usage |
|-------|-----|-------|
| Primary | `#1A365D` | HUD backgrounds, headers |
| Secondary | `#F97316` | CTAs, outline hints, dwell reticle fill |
| Tertiary | `#334155` | Borders, secondary text |
| Neutral | `#F9FAFB` | Text on dark panels |
| Success | `#22C55E` | Objective achieved indicator |
| Error | `#EF4444` | Failures, safety alerts |

### Step 2 — Verify in-scene materials

Use Unity MCP `Unity_FindProjectAssets` to locate materials in `SafeMining/` folders. Check that UI materials reference brand colors, not arbitrary values.

### Step 3 — Verify BrandConfig asset

`Assets/Resources/BrandConfig.asset` must contain the canonical palette. `BrandApplicator` components on UI roots apply colors at runtime.

## Procedure: C# code style audit

Review code against the project's enforced patterns:

### Naming

| Element | Convention | Example |
|---------|-----------|---------|
| Class (MonoBehaviour) | PascalCase, descriptive | `LotoLockInteractable` |
| ref_ class | `ref_` prefix + PascalCase | `ref_HazardSpot` |
| Private field | camelCase, `[SerializeField]` when inspector-exposed | `inspectionManager` |
| Constants | PascalCase | `DefaultApprovalThresholdPercent` |
| Static class | PascalCase | `LotoObjectiveIds` |
| Interface | `I` prefix | `ISimulationBackendAdapter` |

### Structure

- `#region` grouping: Lifecycle → Configuration → Interaction (or domain-specific).
- No comments explaining what code does. Comments only for non-obvious constraints.
- `[RequireComponent]` on MonoBehaviours that need sibling refs.
- `Configure(ManagerType manager)` is presentation DI only. Evaluable actions notify `LotoGameManager` / `LotoGameEventRelay`.
- `ActivateFromPc()` public method on interactables for PC playtest bypass.

### Prohibited

- `FindObjectOfType` in `Update`.
- Empty `Update()` / `FixedUpdate()` methods.
- `Debug.Log` in production paths (only in adapters/stubs and `[Conditional("UNITY_EDITOR")]` guarded).
- `.md` files outside `.agent-docs/` (documentation lives only in `.agent-docs/` and `Doc_SMVR/`).

## Procedure: Asset naming audit

### Naming conventions

| Asset type | Pattern | Example |
|-----------|---------|---------|
| Scene | `VR-LOTO-S{n}` or `StandardMainMenu` | `VR-LOTO-S1.unity` |
| Prefab | `PREF_{PascalName}` | `PREF_LotoObjectiveRow` |
| Material | `MAT_{PascalName}` | `MAT_ConveyorBelt` |
| Texture | Source name accepted; atlas prefix if bundled | `Binary_35.png` |
| Script | PascalCase matching class name | `LotoLockInteractable.cs` |
| ScriptableObject data | PascalCase | `BrandConfig.asset` |
| Translation | `{lang}.xml` in `Resources/` | `es.xml`, `en.xml` |

### Folder structure

```
Assets/SafeMining/
├── LOTOSimulation/
│   ├── Code/ (Adapters, Data, Debug, Interactables, Managers, UI)
│   ├── Prefabs/
│   ├── Scenes/
│   └── Sprites/
├── Models/ (Scenery1, Scenery2, Scenery3)
├── Shared/UI/
└── ...
Assets/XR Standard Menu/
├── Code/ (framework-level: ControlCenter, Managers, refs, SBD, Utils)
└── ...
```

## Procedure: Quest 3S performance audit

### Step 1 — Scene light budget

Check each LOTO scene for:
- Max 1 realtime/mixed Spot light (key).
- Max 2 realtime/mixed Point lights (fill).
- No realtime directional sun in underground scenes.
- Baked lighting for environment.

Use `Unity_FindProjectAssets` type `Light` or `Unity_ManageScene` to inspect scene hierarchy.

### Step 2 — Script performance

Search for violations:
- `FindObjectOfType` in `Update` → must resolve in `Start`/`Awake`.
- Empty `Update()` bodies → remove.
- Uncached `ControlCenter.Instance` access in hot loops.
- `GetComponent` calls in `Update` → cache in `Awake`.

### Step 3 — Rendering settings

Verify via Project Settings or `Unity_GetProjectData`:
- Graphics API: Vulkan.
- Render mode: Single Pass Instanced.
- URP asset: no MSAA above 2x, no HDR on Quest.
- Static batching flag on environment meshes.

### Step 4 — Asset optimization

- Imported meshes (Sketchfab) must have LOD groups or be decimated.
- No raw high-poly meshes (>50k tris) without LOD.
- Texture sizes ≤ 2048 for environment, ≤ 1024 for UI sprites.

## Procedure: Localization audit

1. All user-facing strings must use `TranslatableText` + `translationID`.
2. LOTO keys use `loto_*` prefix.
3. Both `es.xml` and `en.xml` must contain matching keys.
4. No hardcoded Spanish strings in C# scripts.

Search with: `Grep pattern="\"[A-ZÁÉÍÓÚÑ][a-záéíóúñ]+" glob="*.cs" path="Assets/SafeMining"` to find potential hardcoded Spanish text.

## MCP integration

- `Unity_FindProjectAssets` — scan for assets by type or name pattern.
- `Unity_ReadConsole` (Errors) — zero-error gate after any change.
- `Unity_ManageScene` — inspect scene hierarchy for light counts and UI canvas settings.
- `Unity_ManageGameObject` — verify component configuration on specific objects.
- `Unity_ValidateScript` — check script compilation after code changes.

## Checklist

### Requirements
- [ ] All applicable RN/RNF/HU IDs traced to implementation.
- [ ] Objective IDs and weights match `LotoObjectiveIds`.
- [ ] Approval threshold is 60% (not 75%).
- [ ] Scoring edge cases validated (full, threshold, penalty, abandoned).

### Branding
- [ ] No hardcoded color values outside `Brand.*` accessors.
- [ ] `BrandConfig.asset` contains canonical palette.
- [ ] UI elements use brand tokens consistently.

### Code style
- [ ] `#region` grouping present and consistent.
- [ ] No explanatory comments (only constraint/intent comments).
- [ ] `Configure()` + `ActivateFromPc()` pattern on interactables.
- [ ] No `FindObjectOfType` in `Update`.

### Assets
- [ ] Naming follows conventions (PREF_, MAT_, VR-LOTO-S*).
- [ ] Folder structure matches canonical layout.
- [ ] No orphaned or duplicate assets in SafeMining folders.

### Performance
- [ ] Light budget per scene (1 key + 2 fill max).
- [ ] No empty Update loops.
- [ ] Static batching on environment.
- [ ] Mesh LOD on imported models.

### Localization
- [ ] All strings via TranslatableText + translationID.
- [ ] loto_* prefix on LOTO-specific keys.
- [ ] es.xml and en.xml in sync.
