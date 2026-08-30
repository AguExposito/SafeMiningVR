using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class SyncQueueManager : MonoBehaviour
{
    public event Action OnSyncCompleted;
    public event Action<string> OnSyncFailed;

    private const double SyncRetentionHours = 24d;

    private ISimulationBackendAdapter activeAdapter;
    private PendingSyncQueueData queueData;
    private string queueFilePath;

    #region Initialization

    public void Initialize()
    {
        queueFilePath = Path.Combine(Application.persistentDataPath, "PendingSyncQueue.json");
        activeAdapter = ResolveActiveAdapter();
        LoadQueue();
        PurgeExpiredEntries();
    }

    #endregion

    #region Public API

    public void Enqueue(PendingSyncEntry entry)
    {
        if (entry == null)
        {
            return;
        }
        if (string.IsNullOrEmpty(entry.entryId))
        {
            entry.entryId = Guid.NewGuid().ToString("N");
        }
        if (string.IsNullOrEmpty(entry.queuedAtUtc))
        {
            entry.queuedAtUtc = DateTime.UtcNow.ToString("o");
        }
        if (queueData.entries == null)
        {
            queueData.entries = new List<PendingSyncEntry>();
        }
        queueData.entries.Add(entry);
        SaveQueue();
    }

    public void EnqueueCompleteFromSceneScore(SceneScore sceneScore, float elapsedSeconds, DateTime startDate, DateTime endDate)
    {
        EnqueueCompleteFromSceneScore(sceneScore, elapsedSeconds, startDate, endDate, LotoSessionScore.CompletionCompleted);
    }

    public SimulationResultPayload EnqueueCompleteFromSceneScore(
        SceneScore sceneScore,
        float elapsedSeconds,
        DateTime startDate,
        DateTime endDate,
        string completionStatus)
    {
        int falsePositiveCount = 0;
        if (ControlCenter.Instance != null && ControlCenter.Instance.simulationSessionManager != null)
        {
            falsePositiveCount = ControlCenter.Instance.simulationSessionManager.GetFalsePositiveCount();
        }

        SimulationResultPayload payload = LotoSessionScore.Build(
            sceneScore,
            falsePositiveCount,
            elapsedSeconds,
            endDate,
            completionStatus);
        payload.sessionId = ResolveSessionId();
        EnqueueComplete(payload);
        return payload;
    }

    public void EnqueueComplete(SimulationResultPayload payload)
    {
        if (payload == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(payload.sessionId))
        {
            payload.sessionId = ResolveSessionId();
        }

        PendingSyncEntry entry = new PendingSyncEntry
        {
            entryType = PendingSyncEntry.EntryTypeComplete,
            payloadJson = JsonUtility.ToJson(payload)
        };
        Enqueue(entry);
        if (ControlCenter.Instance != null && ControlCenter.Instance.simulationSessionManager != null)
        {
            ControlCenter.Instance.simulationSessionManager.CompleteCurrentSession(payload);
        }

        _ = TryFlushQueueAsync();
    }

    public async Task TryFlushQueueAsync()
    {
        if (activeAdapter == null || !activeAdapter.IsAvailable)
        {
            return;
        }
        PurgeExpiredEntries();
        if (queueData.entries == null || queueData.entries.Count == 0)
        {
            return;
        }
        List<PendingSyncEntry> pendingEntries = new List<PendingSyncEntry>(queueData.entries);
        List<PendingSyncEntry> completedEntries = new List<PendingSyncEntry>();
        for (int i = 0; i < pendingEntries.Count; i++)
        {
            PendingSyncEntry entry = pendingEntries[i];
            bool success = await ProcessEntryAsync(entry);
            if (success)
            {
                completedEntries.Add(entry);
            }
            else
            {
                entry.retryCount++;
                entry.lastError = "Sync attempt failed";
            }
        }
        for (int i = 0; i < completedEntries.Count; i++)
        {
            queueData.entries.Remove(completedEntries[i]);
        }
        SaveQueue();
        if (completedEntries.Count > 0)
        {
            OnSyncCompleted?.Invoke();
        }
        else if (pendingEntries.Count > 0)
        {
            OnSyncFailed?.Invoke("No entries synced");
        }
    }

    public void PurgeExpiredEntries()
    {
        if (queueData.entries == null || queueData.entries.Count == 0)
        {
            return;
        }
        DateTime cutoff = DateTime.UtcNow.AddHours(-SyncRetentionHours);
        queueData.entries.RemoveAll(entry =>
        {
            if (entry == null || string.IsNullOrEmpty(entry.queuedAtUtc))
            {
                return true;
            }
            if (!DateTime.TryParse(entry.queuedAtUtc, out DateTime queuedAt))
            {
                return false;
            }
            return queuedAt.ToUniversalTime() < cutoff;
        });
        SaveQueue();
    }

    public bool HasCompleteEntryWithStatus(string completionStatus)
    {
        if (queueData == null || queueData.entries == null || string.IsNullOrEmpty(completionStatus))
        {
            return false;
        }

        for (int i = 0; i < queueData.entries.Count; i++)
        {
            PendingSyncEntry entry = queueData.entries[i];
            if (entry == null || entry.entryType != PendingSyncEntry.EntryTypeComplete)
            {
                continue;
            }

            SimulationResultPayload payload = JsonUtility.FromJson<SimulationResultPayload>(entry.payloadJson);
            if (payload != null && payload.completionStatus == completionStatus)
            {
                return true;
            }
        }

        return false;
    }

    #endregion

    #region Queue Persistence

    private void LoadQueue()
    {
        queueData = new PendingSyncQueueData();
        if (!File.Exists(queueFilePath))
        {
            SaveQueue();
            return;
        }
        try
        {
            string json = File.ReadAllText(queueFilePath);
            PendingSyncQueueData loaded = JsonUtility.FromJson<PendingSyncQueueData>(json);
            if (loaded != null && loaded.entries != null)
            {
                queueData = loaded;
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"[SyncQueueManager] Load failed: {exception.Message}");
        }
    }

    private void SaveQueue()
    {
        try
        {
            if (queueData.entries == null)
            {
                queueData.entries = new List<PendingSyncEntry>();
            }
            string json = JsonUtility.ToJson(queueData, true);
            File.WriteAllText(queueFilePath, json);
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"[SyncQueueManager] Save failed: {exception.Message}");
        }
    }

    #endregion

    #region Adapter Processing

    private ISimulationBackendAdapter ResolveActiveAdapter()
    {
        if (ControlCenter.Instance != null && ControlCenter.Instance.ibManager != null && ControlCenter.Instance.ibManager.ShouldSendResults())
        {
            return new SafeMiningBackendAdapter();
        }
        return new OfflineBackendAdapter();
    }

    private async Task<bool> ProcessEntryAsync(PendingSyncEntry entry)
    {
        if (entry == null || string.IsNullOrEmpty(entry.payloadJson))
        {
            return false;
        }
        switch (entry.entryType)
        {
            case PendingSyncEntry.EntryTypeStart:
                SimulationSessionContext startContext = JsonUtility.FromJson<SimulationSessionContext>(entry.payloadJson);
                return startContext != null && await activeAdapter.StartSessionAsync(startContext);
            case PendingSyncEntry.EntryTypeEvent:
                SimulationEventRecord eventRecord = JsonUtility.FromJson<SimulationEventRecord>(entry.payloadJson);
                return eventRecord != null && await activeAdapter.RecordEventAsync(eventRecord);
            case PendingSyncEntry.EntryTypeComplete:
                SimulationResultPayload completePayload = JsonUtility.FromJson<SimulationResultPayload>(entry.payloadJson);
                return completePayload != null && await activeAdapter.CompleteSessionAsync(completePayload);
            default:
                return false;
        }
    }

    #endregion

    #region Payload Mapping

    private static string ResolveSessionId()
    {
        if (ControlCenter.Instance != null && ControlCenter.Instance.simulationSessionManager != null)
        {
            SimulationSessionContext session = ControlCenter.Instance.simulationSessionManager.CurrentSession;
            if (session != null && !string.IsNullOrEmpty(session.sessionId))
            {
                return session.sessionId;
            }
        }
        return Guid.NewGuid().ToString("N");
    }

    #endregion
}
