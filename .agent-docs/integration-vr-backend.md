# VR ↔ Backend Integration

## Principles

- Offline-first: all results saved locally before HTTP
- Deferred sync window: 24 hours (HU-220 CA8)
- Queue is **not** flushed while the HTTP adapter is unavailable (`OfflineBackendAdapter.IsAvailable = false`; `SafeMiningBackendAdapter` stays a stub until Fase 7). Entries remain in `PendingSyncQueue.json` until a real adapter is available or the 24h purge.
- Legacy IBManager `/crearSimReporte` preserved; new API runs in parallel

## Endpoints (proposed — Fase 7)

### POST /api/vr/sessions/start

**Request:**
```json
{
  "token": "jwt-from-emp02",
  "capacitacionId": "string",
  "programId": "string"
}
```

**Response:**
```json
{
  "intentoId": "string",
  "sessionId": "string",
  "expiresAt": "2026-08-17T22:00:00Z"
}
```

**EA entity:** `IntentoCapacitacionCapacitado`

### POST /api/vr/sessions/{id}/events

**Request:** `SimulationEventRecord`

**Response:** `{ "ok": true }`

**EA entity:** `EleccionOpcionCapacitacion`

### POST /api/vr/sessions/{id}/complete

**Request:** `SimulationResultPayload`

**Response:**
```json
{
  "approved": true,
  "scoreTotal": 85
}
```

**EA entity:** `resultadoIntentoCapacitacion`

### POST /api/vr/sessions/sync

**Request:** `{ "entries": [ PendingSyncEntry ] }`

**Response:** `{ "synced": [], "failed": [] }`

## Local persistence

| File | Manager |
|---|---|
| `PendingSyncQueue.json` | SyncQueueManager |
| `SimulationSessionContext.json` | SimulationSessionManager |
| `ScorePersistantData.json` | PersistantDataManager / ScoreManager |

## Unity adapters

| Class | Phase |
|---|---|
| `OfflineBackendAdapter` | Fase 5 — local persist only (`IsAvailable` false so the queue is not drained) |
| `SafeMiningBackendAdapter` | Fase 7 — stub |

## Auth flow (future)

1. Employee presses "Iniciar VR" on EMP-02 web → JWT token issued
2. VR app login (VR-APP-01) validates token
3. Session start registered before scene load (RN09, F4-RF07)
