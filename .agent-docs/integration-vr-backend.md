# VR ↔ Backend Integration

## Principles

- Offline-first: all results saved locally before HTTP
- Deferred sync window: 24 hours (HU-220 CA8)
- Queue is **not** flushed while the HTTP adapter is unavailable (`OfflineBackendAdapter.IsAvailable = false`). With VR JWT login (Online), `SafeMiningBackendAdapter` posts results; otherwise entries remain in `PendingSyncQueue.json` until login or the 24h purge.
- Legacy IBManager `/crearSimReporte` preserved; VR API v1 runs in parallel when JWT session is active
- No remote session start; local `sessionId` (GUID) remains for offline queue / telemetry only
- Result POST path uses the fixed app simulation id `loto_simulation` (`LotoObjectiveIds.SimulationSceneId`), not the per-attempt UUID
- Shared catalog: `Assets/Resources/simulations.json` is the simulation database for VR and web. Company/profile access stays on the backend.

## Shared simulation catalog

**File:** `Assets/Resources/simulations.json` (versioned in this repo).

**Web:** `GET` the same file from GitHub raw, for example  
`https://raw.githubusercontent.com/<owner>/<repo>/<branch>/Assets/Resources/simulations.json`  
The repository (or that path) must be readable without a token.

**Headset:** `ControlCenter.simulationCatalogUrl` (HTTPS raw URL, empty = no download). On startup `SimulationCatalogLoader` picks the higher `version` between cache (`persistentDataPath/simulations.json`) and the bundled Resources file, then replaces the cache when the GET returns 200. Bump `version` when the catalog content changes. Gameplay does not wait on the network. A download that arrives after a LOTO attempt has started is stored and applied on the next `ResetRun`, so the active score does not change. A file without `loto_simulation` is rejected.

**Create or edit a simulation:** change this JSON and push. Do not register the same record separately in the web admin and in Unity. `implemented: false` means catalog-only (no scene). `sim_prueba` is that probe entry.

**LOTO scoring** reads `loto_simulation` phase weights/thresholds and objective `weightPercent` from the loaded catalog. Procedure formulas stay in code.

**Not in this file:** company ACL, JWT, result POST body.

## Endpoints (VR API v1 — async sequence)

### POST /api/v1/auth/login

**Request:** `{ "username", "password" }`

**Response:** `{ "token" }` (Bearer JWT, in-memory only on device)

Implemented in `LotoVrLoginController`.

### POST /api/v1/result/{Id_VR}

**Path:** `Id_VR` = **`loto_simulation`** (fixed SimScore / capacitacion id for this app). Not the local attempt UUID.

**Headers:** `Authorization: Bearer {token}`, `Content-Type: application/json`

**Body:** `VrResultSubmitBody` — `epp`, `limpieza`, `loto` (0–100 per phase), `duracion_epp`, `duracion_limpieza`, `duracion_loto`, `duracion_total` (seconds).

**Success:** HTTP 200 or 202.

Implemented in `SafeMiningBackendAdapter.CompleteSessionAsync`. Serialized snapshot stored on `SimulationResultPayload.vrResultJson` when enqueued.

## Endpoints (legacy / Fase 7 proposal)

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
| `SafeMiningBackendAdapter` | VR JWT — `POST /api/v1/result/{Id_VR}`; Start/Event no-op |

## Auth flow (future)

1. Employee presses "Iniciar VR" on EMP-02 web → JWT token issued
2. VR app login (VR-APP-01) validates token
3. Session start registered before scene load (RN09, F4-RF07)
