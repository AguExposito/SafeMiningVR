# WF3 — HTTP Network, Auth & Telemetry Pipeline

Validates the offline-first backend sync pipeline: local persistence, sync queue behavior, API contract compliance, auth token handling, and network resilience.

**Source of truth:** `.agent-docs/integration-vr-backend.md`, `.cursor/rules/70-backend-sync.mdc`, `Doc_SMVR/SafeMining_VR_v1.1.md` (RN09, RNF04, RNF08, HU-220 CA8).

---

## Phase A — Local Persistence Validation

### A1. Score persistence after happy path

- [ ] Complete S1→S2→S3→summary (full happy path).
- [ ] Check `Application.persistentDataPath/ScorePersistantData.json` exists and is updated.
- [ ] Verify JSON contains the new score entry with `scoreTotal`, `approved`, and timestamp.
- [ ] **PASS:** Score persisted locally before any HTTP attempt.
- [ ] **FAIL:** File missing, corrupted, or empty after completion.

### A2. Session context persistence

- [ ] After starting LOTO simulation (post-briefing dismiss), check `SimulationSessionContext.json`.
- [ ] Verify JSON contains: `sessionId` (non-empty GUID), `isActive: true`, `startedAtUtc` (valid ISO-8601).
- [ ] After session completes, verify `isActive: false`.
- [ ] **PASS:** Context saved before gameplay begins, updated on completion.
- [ ] **FAIL:** No file, missing sessionId, or isActive not toggled.

### A3. Sync queue population

- [ ] After completing a session, check `PendingSyncQueue.json`.
- [ ] Verify it contains at least one `PendingSyncEntry` with:
  - `entryId`: non-empty GUID.
  - `entryType`: `"Complete"`.
  - `payloadJson`: valid JSON matching `SimulationResultPayload` schema.
  - `queuedAtUtc`: valid ISO-8601 timestamp.
  - `retryCount`: 0 (first entry).
- [ ] **PASS:** Queue entry correctly structured and persisted.
- [ ] **FAIL:** Queue empty, malformed entry, or missing fields.

### A4. Abandoned session persistence

- [ ] Start simulation, reach S2, then exit via menu confirm.
- [ ] Verify `ScorePersistantData.json` contains partial score with `completionStatus: "Abandoned"`.
- [ ] Verify `PendingSyncQueue.json` contains an entry for the abandoned session.
- [ ] **PASS:** Abandoned sessions persist with partial data.
- [ ] **FAIL:** No data saved on abandon, or completionStatus missing/wrong.

---

## Phase B — Adapter Pattern Validation

### B1. OfflineBackendAdapter (Fase 5 — current)

- [ ] Verify `OfflineBackendAdapter.IsAvailable` returns `false`.
- [ ] Call each adapter method:
  - `StartSessionAsync(context)` → returns `true`, logs `[OfflineBackendAdapter] StartSession queued locally`.
  - `RecordEventAsync(record)` → returns `true`, logs event type.
  - `CompleteSessionAsync(result)` → returns `true`, logs score.
- [ ] **PASS:** Adapter works without network, logs locally, IsAvailable is false.
- [ ] **FAIL:** IsAvailable returns true, methods throw, or no logging.

### B2. SafeMiningBackendAdapter (Fase 7 — stub)

- [ ] Verify `SafeMiningBackendAdapter.IsAvailable` returns `false` (BaseUrlPlaceholder is empty).
- [ ] Call each adapter method → returns `false`, logs stub messages.
- [ ] **PASS:** Stub correctly returns false and does not attempt HTTP.
- [ ] **FAIL:** Returns true, attempts HTTP, or throws.

### B3. IBManager legacy preservation

- [ ] Verify `IBManager.SendScoreToCloud` method exists and is callable.
- [ ] Verify `/crearSimReporte` endpoint path is still referenced in IBManager.
- [ ] Run a full session → verify both old (`ScoreManager` → `IBManager`) and new (`SyncQueueManager`) paths execute without conflict.
- [ ] **PASS:** Legacy pipeline untouched and functional alongside new sync.
- [ ] **FAIL:** IBManager modified, removed, or conflicts with new pipeline.

---

## Phase C — DTO & API Contract Compliance

### C1. SimulationResultPayload schema

Run `LotoSessionScore.Build()` with a mock `SceneScore` and verify the output JSON:

```json
{
  "sessionId": "string (non-empty)",
  "objectives": [
    {
      "objectiveId": "loto_obj_epp_selection",
      "achieved": true,
      "weightPercent": 20,
      "scorePartial": 20.0
    }
  ],
  "scoreTotal": 85,
  "approved": true,
  "durationSeconds": 420,
  "finishedAtUtc": "ISO-8601",
  "completionStatus": "Completed"
}
```

- [ ] All 5 objectives present in array.
- [ ] `scoreTotal` is integer 0–100.
- [ ] `approved` = `scoreTotal >= 60`.
- [ ] `durationSeconds` ≥ 0.
- [ ] `finishedAtUtc` is valid UTC ISO-8601.
- [ ] `completionStatus` is `"Completed"` or `"Abandoned"`.
- [ ] **PASS:** Payload matches `/api/vr/sessions/{id}/complete` request schema.
- [ ] **FAIL:** Missing fields, wrong types, or schema mismatch.

### C2. SimulationEventRecord schema

- [ ] Verify event records include: `sessionId`, `eventType`, `objectiveId`, `optionId`, `timestampUtc`, `success`, `metadata`.
- [ ] All fields are strings except `success` (bool).
- [ ] `timestampUtc` is ISO-8601.
- [ ] **PASS:** Event record matches `/api/vr/sessions/{id}/events` request schema.
- [ ] **FAIL:** Missing fields or wrong types.

### C3. PendingSyncEntry schema

- [ ] `entryType` is one of: `"Start"`, `"Event"`, `"Complete"`.
- [ ] `payloadJson` deserializes to the correct DTO for its entryType.
- [ ] `queuedAtUtc` is valid ISO-8601.
- [ ] `retryCount` starts at 0.
- [ ] **PASS:** Queue entries are self-contained and re-serializable.
- [ ] **FAIL:** Corrupt payloadJson, wrong entryType, or non-deserializable.

### C4. EA entity mapping

| Unity DTO | EA Entity | Verified |
|-----------|-----------|----------|
| `SimulationSessionContext` | `IntentoCapacitacionCapacitado` | [ ] |
| `SimulationEventRecord` | `EleccionOpcionCapacitacion` | [ ] |
| `SimulationObjectiveResult` | `OBJETIVOSCapacitacion` | [ ] |
| `SimulationResultPayload` | `resultadoIntentoCapacitacion` | [ ] |

- [ ] **PASS:** All mappings documented and field correspondence reviewed.
- [ ] **FAIL:** Unmapped DTO or field name mismatch.

---

## Phase D — Sync Queue Behavior

### D1. Queue NOT flushed when adapter unavailable

- [ ] Run session with `OfflineBackendAdapter` (IsAvailable = false).
- [ ] Verify queue entries accumulate but are NOT sent.
- [ ] **PASS:** Queue grows without drain attempts.
- [ ] **FAIL:** Flush attempted despite IsAvailable = false.

### D2. 24-hour purge rule

- [ ] Manually create a queue entry with `queuedAtUtc` older than 24 hours.
- [ ] Trigger flush attempt.
- [ ] **PASS:** Stale entry is purged from queue (HU-220 CA8).
- [ ] **FAIL:** Stale entry remains or causes errors.

### D3. Retry count increment

- [ ] Simulate a failed sync (adapter returns false).
- [ ] Verify `retryCount` increments and `lastError` is populated.
- [ ] **PASS:** Retry tracking works correctly.
- [ ] **FAIL:** retryCount stays at 0 or lastError empty.

### D4. No data loss on sync failure

- [ ] Simulate sync failure partway through queue flush.
- [ ] Verify `ScorePersistantData.json` is intact.
- [ ] Verify failed queue entries remain in `PendingSyncQueue.json`.
- [ ] **PASS:** Local data never deleted on failure.
- [ ] **FAIL:** Data lost or queue corrupted.

---

## Phase E — Auth Flow Readiness (Fase 7 Prep)

### E1. Auth contract review

- [ ] JWT token issued by EMP-02 web portal contains `capacitacionId` and `programId`.
- [ ] Token expiry: 2 hours (RNF08).
- [ ] VR-APP-01 login screen: username field only, NO password field in headset.
- [ ] **PASS:** Auth contract documented and ready for implementation.
- [ ] **FAIL:** Missing fields or unclear contract.

### E2. Session pre-registration (RN09)

- [ ] Verify code path: `StartSessionAsync` is called BEFORE loading simulation scene.
- [ ] In offline mode: `OfflineBackendAdapter.StartSessionAsync` returns true, session context is populated.
- [ ] **PASS:** Session registered before gameplay begins.
- [ ] **FAIL:** StartSessionAsync called after scene load or not at all.

### E3. No PII in payloads

- [ ] Grep all DTO classes for sensitive fields.
- [ ] Only `participantId` and `sessionId` are present — no name, email, or device identifiers.
- [ ] **PASS:** PII policy respected.
- [ ] **FAIL:** Unauthorized personal data in payloads.

---

## Phase F — Network Resilience (Fase 7 scenarios)

These checks apply when `SafeMiningBackendAdapter` becomes active.

### F1. Gameplay does not block on network

- [ ] Disconnect network during active simulation.
- [ ] **PASS:** Gameplay continues uninterrupted, data queued locally.
- [ ] **FAIL:** Freeze, hang, or error dialog interrupts gameplay.

### F2. Timeout handling

- [ ] Simulate server timeout (no response within 10s).
- [ ] **PASS:** Adapter returns false, entry stays in queue, retryCount increments.
- [ ] **FAIL:** Hang, unhandled exception, or data loss.

### F3. Connection restoration

- [ ] Reconnect network after offline gameplay.
- [ ] Trigger sync.
- [ ] **PASS:** Queued entries are sent, synced entries removed from queue.
- [ ] **FAIL:** Queue not flushed, or duplicate sends.

### F4. Partial sync

- [ ] Queue contains 5 entries. Server syncs 3, fails 2.
- [ ] **PASS:** `synced` entries removed, `failed` entries remain with updated retryCount.
- [ ] **FAIL:** All entries removed or all retained.

---

## Sign-off matrix

| Phase | Tester | Date | Result |
|-------|--------|------|--------|
| A — Local persistence | | | |
| B — Adapter pattern | | | |
| C — DTO/API contracts | | | |
| D — Sync queue behavior | | | |
| E — Auth readiness | | | |
| F — Network resilience | | | |

**Workflow complete when all phases show PASS. Phases E/F marked N/A if Fase 7 not yet started.**
