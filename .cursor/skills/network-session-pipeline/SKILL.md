---
name: network-session-pipeline
description: >-
  Build, test, and debug the offline-first backend sync pipeline for SafeMining LOTO sessions.
  Covers ISimulationBackendAdapter implementation, DTO construction, API contract validation,
  SyncQueueManager flow, auth token handling, and SimulationResultPayload scoring.
  Use when implementing backend adapters, testing HTTP payloads, adding sync queue entries,
  modifying scoring logic,   or preparing for Fase 7 endpoint integration.
---

# Network & Session Pipeline

## Purpose

Guide the agent through creating, testing, and extending the offline-first backend sync layer that records LOTO training sessions, events, and results for later synchronization with the SafeMining enterprise backend.

## When to use

- Implementing a new `ISimulationBackendAdapter` (e.g., for a real REST endpoint).
- Modifying or extending `SimulationResultPayload` scoring logic.
- Testing DTO serialization to match API contracts.
- Adding new telemetry events to the session pipeline.
- Debugging sync queue persistence or deferred flush behavior.
- Preparing the auth flow for Fase 7 integration.

## Preconditions

1. Unity MCP connected — `Unity_ManageEditor` → `GetProjectRoot` confirms project path.
2. `ControlCenter.Instance` available with `simulationSessionManager` and `scoreManager` refs.
3. `IBManager.ConnectionMode` is `Offline` for current phase (Fase 5).
4. Familiarity with `.agent-docs/integration-vr-backend.md` as source of truth for API contracts.

## Architecture overview

```
ScoreManager.EndScene
    │
    ├─► LotoSessionScore.Build() → SimulationResultPayload
    │
    ├─► SyncQueueManager.EnqueueCompleteFromSceneScore()
    │       └─► PendingSyncEntry → PendingSyncQueue.json
    │
    └─► ISimulationBackendAdapter.CompleteSessionAsync()
            ├─ OfflineBackendAdapter (Fase 5) → local log only
            └─ SafeMiningBackendAdapter (Fase 7) → POST /api/vr/sessions/{id}/complete
```

## API contracts (Fase 7 stubs)

### POST /api/vr/sessions/start

```json
// Request
{ "token": "jwt-from-emp02", "capacitacionId": "string", "programId": "string" }

// Response
{ "intentoId": "string", "sessionId": "string", "expiresAt": "ISO-8601" }
```

Maps to: `SimulationSessionContext`

### POST /api/vr/sessions/{id}/events

```json
// Request — SimulationEventRecord
{
  "sessionId": "string",
  "eventType": "string",
  "objectiveId": "string",
  "optionId": "string",
  "timestampUtc": "ISO-8601",
  "success": true,
  "metadata": "string"
}
```

### POST /api/vr/sessions/{id}/complete

```json
// Request — SimulationResultPayload
{
  "sessionId": "string",
  "objectives": [
    { "objectiveId": "string", "achieved": true, "weightPercent": 20, "scorePartial": 20.0 }
  ],
  "scoreTotal": 85,
  "approved": true,
  "durationSeconds": 420,
  "finishedAtUtc": "ISO-8601",
  "completionStatus": "Completed"
}
```

### POST /api/vr/sessions/sync

```json
// Request
{ "entries": [ { "entryId": "guid", "entryType": "Complete", "payloadJson": "...", "queuedAtUtc": "ISO-8601", "retryCount": 0 } ] }

// Response
{ "synced": ["guid1"], "failed": [{ "entryId": "guid2", "error": "reason" }] }
```

## Procedure: Implement a new backend adapter

1. Create a class implementing `ISimulationBackendAdapter`:

```csharp
public class YourAdapter : ISimulationBackendAdapter
{
    public bool IsAvailable => /* connectivity check */;

    public async Task<bool> StartSessionAsync(SimulationSessionContext context)
    {
        string json = JsonUtility.ToJson(new StartSessionRequest
        {
            token = context.participantId,
            capacitacionId = context.capacitacionId,
            programId = context.programId
        });
        // POST to /api/vr/sessions/start
        // Parse response, set context.intentoId and context.sessionId
        return true;
    }

    public async Task<bool> RecordEventAsync(SimulationEventRecord record)
    {
        string json = JsonUtility.ToJson(record);
        // POST to /api/vr/sessions/{record.sessionId}/events
        return true;
    }

    public async Task<bool> CompleteSessionAsync(SimulationResultPayload result)
    {
        string json = JsonUtility.ToJson(result);
        // POST to /api/vr/sessions/{result.sessionId}/complete
        return true;
    }
}
```

2. Register the adapter in `SimulationOrchestratorManager` or `SimulationSessionManager` based on `IBManager.ConnectionMode`.

3. Preserve the `IBManager` legacy path — new adapter runs in **parallel**, not as replacement.

## Procedure: Validate scoring payload

1. Review `LotoObjectiveIds` weights:

| Objective | ID | Weight |
|-----------|----|--------|
| EPP selection | `loto_obj_epp_selection` | 20% |
| EPP defect rejection | `loto_obj_epp_defect` | 10% |
| Hazard reporting | `loto_obj_hazard_report` | 25% |
| LOTO sequence | `loto_obj_loto_sequence` | 35% |
| Zero energy verification | `loto_obj_zero_energy` | 10% |

Total weights = 100%. Approval threshold = 60%.

2. `LotoSessionScore.Build()` computes `scoreTotal` as sum of weighted partials. Hazard report applies false-positive penalty: `weight * max(0, 1 - falsePositives/4)`.

3. Test payload generation by constructing a `SceneScore` mock and calling `LotoSessionScore.Build()`. Verify `scoreTotal` and `approved` match expected values.

4. Serialize with `JsonUtility.ToJson(payload)` and confirm it matches the API contract schema.

## Procedure: Test sync queue flow

1. `SyncQueueManager` enqueues a `PendingSyncEntry` with `entryType = "Complete"` and `payloadJson` containing the serialized `SimulationResultPayload`.

2. Queue persists to `Application.persistentDataPath/PendingSyncQueue.json`.

3. Flush rules:
   - Queue is NOT flushed while `ISimulationBackendAdapter.IsAvailable` returns `false`.
   - Entries older than 24 hours are purged on flush attempt (HU-220 CA8).
   - Retry count increments on each failed attempt; `lastError` stores failure reason.

4. To test flush: temporarily set a mock adapter's `IsAvailable = true` and trigger `SyncQueueManager.TryFlush()`.

## Procedure: Auth flow preparation (Fase 7)

1. Employee clicks "Iniciar VR" on EMP-02 web portal → JWT issued (2h expiry per RNF08).
2. VR app login screen (VR-APP-01) shows username field only — no password entry in headset.
3. On token validation, call `StartSessionAsync` BEFORE loading the simulation scene (RN09, F4-RF07).
4. Store `intentoId` and `sessionId` in `SimulationSessionContext` for the duration of the session.

## Constraints

- **Gameplay never blocks on network.** All operations are fire-and-forget with local persistence.
- **No PII** beyond `participantId` / `sessionId` in payloads.
- **JsonUtility** with wrapper classes for list serialization (Unity limitation).
- **IBManager legacy** path (`SendScoreToCloud`, `/crearSimReporte`) runs in parallel — do not remove.
- Local score history is never deleted on sync failure.

## Key project files

| File | Role |
|------|------|
| `Code/Adapters/ISimulationBackendAdapter.cs` | Adapter interface |
| `Code/Adapters/OfflineBackendAdapter.cs` | Fase 5 local-only adapter |
| `Code/Adapters/SafeMiningBackendAdapter.cs` | Fase 7 REST stub |
| `Code/Data/SimulationSessionContext.cs` | Session context DTO |
| `Code/Data/SimulationEventRecord.cs` | Event record DTO |
| `Code/Data/SimulationObjectiveResult.cs` | Objective + payload DTOs |
| `Code/Data/PendingSyncEntry.cs` | Sync queue entry |
| `Code/Data/PendingSyncQueueData.cs` | Queue wrapper for serialization |
| `Code/Data/LotoSessionScore.cs` | Scoring computation |
| `Code/Data/LotoObjectiveIds.cs` | Objective IDs and weights |
| `Code/Managers/SyncQueueManager.cs` | Queue persistence and flush |
| `.agent-docs/integration-vr-backend.md` | API contract source of truth |

## Checklist

- [ ] Adapter implements all 3 methods of `ISimulationBackendAdapter`.
- [ ] `IsAvailable` returns `false` until real connectivity exists.
- [ ] Payload JSON matches API contract schemas.
- [ ] Scoring weights sum to 100% and threshold is 60%.
- [ ] Hazard false-positive penalty applies correctly.
- [ ] Sync queue entries include valid `entryId` (GUID) and `queuedAtUtc`.
- [ ] 24h purge on flush is tested.
- [ ] `IBManager` legacy path is preserved and unmodified.
- [ ] No gameplay blocking on network operations.
