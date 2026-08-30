# Fase 6 — QA Quest 3S

Target: Meta Quest 3S, package `com.safeminingvr.sim`, interactive apps ≥ **72 FPS** ([Meta Unity Perf](https://developers.meta.com/horizon/documentation/unity/unity-perf/)).

## Device session (2026-08-29)

| Field | Value |
|-------|-------|
| Device | Quest 3S (`340YC20G8W04C3`, Android 14, SDK 34) |
| Transport | USB (`metavr_device` state=device) |
| APK installed | `Builds/SafeMiningVR-Quest3S-Dev.apk` (315 MB) via `metavr_app install` |
| Launch | PASS — `com.safeminingvr.sim` started |
| Logcat Unity E | PASS — 0 Error lines (package + Unity tag filters) |
| Perfetto | BLOCKED — `capture_perfetto_trace` requires adb path (`HZDB_ADB_PATH` / MQDH) |
| APK freshness | PARTIAL — on-disk APK dated 2026-08-20; rebuild via **SafeMining/LOTO/Build Quest 3S Dev APK** before full sign-off post-Fase 5 |

## Build settings (verified)

| Setting | Value | Status |
|---------|-------|--------|
| Application ID | `com.safeminingvr.sim` | PASS |
| Scripting backend | IL2CPP | PASS |
| Architectures | ARM64 (`AndroidTargetArchitectures: 2`) | PASS |
| Min / Target SDK | 32 / 34 | PASS |
| Graphics API | Vulkan | PASS |
| OpenXR render mode | Single Pass Instanced (`m_renderMode: 1`) | PASS |
| Multiview render regions | None (`optimizeMultiviewRenderRegions: 0`) | PASS |
| Dev APK path | `Builds/SafeMiningVR-Quest3S-Dev.apk` | PASS (exists) |
| Build tool | `LotoFase6QuestBuildTool.cs` menu **Build Quest 3S Dev APK** | CLOSED |

## WF1 Phase B — Quest device (manual in-headset)

| Step | Automated | Manual sign-off |
|------|-----------|-----------------|
| B1 Device | PASS (metavr list) | |
| B2 Install/Launch | PASS | |
| B3 Hand tracking default | | Pending headset session |
| B4 Controller switch | | Pending headset session |
| B5 Dwell reticle | | Pending headset session |
| B6 Grab EPP/lock/tag | | Pending headset session |
| B7 Outline hints | | Pending headset session |
| B8 HUD legibility | | Pending headset session |
| B9 Haptics sobriety | | Pending headset session |
| B10 Teleport | | Pending headset session |
| B11 Logcat Errors | PASS (automated) | |

## WF4 Phase E — device performance

| Check | Result | Notes |
|-------|--------|-------|
| E1/E2 FPS ≥72 | BLOCKED | Perfetto adb path; use OVR Metrics after APK rebuild |
| E3 GC Editor proxy | PASS | Fase 5 WF4-C |
| E4 Load times | Pending | Manual stopwatch during happy path |
| E5 Logcat E=0 | PASS | 2026-08-29 session |

## Comfort

- HUD / briefing `Util_PanelFollower` distance ~**1.75 m** (`PREF_LotoPlayerHUD`, `PREF_LotoBriefingPanel`).
- PlayerMR `TunnelingVignette` locomotion providers already enabled (XRI comfort).
- Alerts use subtitle + breaker flash only — no camera shake.

## Fail paths (HU-223)

| Caso | Esperado | Editor | Device |
|------|----------|--------|--------|
| Pass feliz | Resumen ≥60 Aprobado; cola `Completed` | PASS (Fase 4 flow) | Pending manual |
| Malo 1 START incompleto | Jolt + `loto_s3_alert_incomplete`; recovery | PASS | Pending manual |
| Malo 2 sin guantes | `loto_s3_alert_no_gloves`; breaker advances | PASS | Pending manual |
| Abandono S2 | `Abandoned` parcial; menú | PASS (code) | Optional |
| Offline queue | `PendingSyncQueue.json` retains entries | PASS | Verify on device after happy path |

## Device checklist (repeat after fresh APK)

1. **SafeMining/LOTO/Build Quest 3S Dev APK** (Unity Editor open).
2. `metavr_app install` → `Builds/SafeMiningVR-Quest3S-Dev.apk`.
3. Launch `com.safeminingvr.sim`; run menu → S1 → S2 → S3 → summary → menu.
4. WF1-B B3–B10 sign-off in headset.
5. `capture_perfetto_trace` (after adb path configured) or OVR Metrics ≥72 FPS on S3.
6. Update baseline **Fase 6 applied** manual rows to PASS.
