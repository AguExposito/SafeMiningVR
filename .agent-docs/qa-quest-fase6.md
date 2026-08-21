# Fase 6 — QA Quest 3S

Target: Meta Quest 3S, package `com.safeminingvr.sim`, interactive apps ≥ **72 FPS** ([Meta Unity Perf](https://developers.meta.com/horizon/documentation/unity/unity-perf/)).

## Build settings (verified)

| Setting | Value |
|---------|-------|
| Application ID | `com.safeminingvr.sim` |
| Scripting backend | IL2CPP |
| Architectures | ARM64 |
| Min / Target SDK | 32 / 34 |
| Graphics API | Vulkan |
| OpenXR render mode | Single Pass Instanced |
| Multiview render regions | None (avoids MetaQuestFeature validation errors) |
| Dev APK (local, not in git) | `Builds/SafeMiningVR-Quest3S-Dev.apk` |

## Comfort

- HUD / briefing `Util_PanelFollower` distance ~**1.75 m** (`PREF_LotoPlayerHUD`, `PREF_LotoBriefingPanel`).
- PlayerMR `TunnelingVignette` locomotion providers already enabled (XRI comfort).
- Alerts use subtitle + breaker flash only — no camera shake.

## Fail paths (HU-223)

| Caso | Esperado | Editor MCP |
|------|----------|------------|
| Pass feliz | Resumen ≥60 Aprobado; cola `Completed` | Covered prior Fase 5 + recovery after Malo 1 |
| Malo 1 START incompleto | Correa jolt + `loto_s3_alert_incomplete`; no resumen; recovery with lock+tag+START → summary | Pass — stayed `Active` then recovered to `SessionSummary`; queue `start_incomplete` |
| Malo 2 sin guantes | Alert `loto_s3_alert_no_gloves`; event `breaker_no_gloves`; breaker still advances (no soft-lock) | Pass — queue event `success:false` |
| Abandono S2 | `Abandoned` parcial; menú | Covered Fase 5 |
| Offline queue | `PendingSyncQueue.json` retains entries | Pass |
| Device logcat | 0 LogError; 72 FPS smoke | **Blocked** — no Quest connected (`adb devices` empty) at Fase 6 close |

## Device checklist (when Quest 3S is available)

1. USB debugging on; `adb devices` shows headset.
2. `adb install -r Builds/SafeMiningVR-Quest3S-Dev.apk` (or metavr install).
3. Launch `com.safeminingvr.sim`; run menu → S1 → S2 → S3 → summary → menu.
4. Capture logcat / OVR Metrics; confirm ≥72 FPS on happy path and 0 Errors.
5. Update this table Device row to Pass with date.
