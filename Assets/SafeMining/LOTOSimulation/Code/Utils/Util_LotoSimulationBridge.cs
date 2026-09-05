using System;

public static class Util_LotoSimulationBridge
{
    #region Scoring

    public static void SetObjective(int objectiveIndex, bool achieved)
    {
        if (ControlCenter.Instance == null || ControlCenter.Instance.scoreManager == null)
        {
            return;
        }

        ControlCenter.Instance.scoreManager.AchiveObjective(
            LotoSimulationConstants.LotoSimScoreIndex,
            objectiveIndex,
            achieved);

        if (ControlCenter.Instance.simulationOrchestratorManager != null)
        {
            ControlCenter.Instance.simulationOrchestratorManager.NotifyObjectiveVisualUpdate(objectiveIndex, achieved);
        }
    }

    #endregion

    #region Sync

    public static void EnqueueEvent(SimulationEventRecord record)
    {
        if (ControlCenter.Instance == null || ControlCenter.Instance.syncQueueManager == null || record == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(record.sessionId))
        {
            record.sessionId = GetSessionId();
        }

        PendingSyncEntry entry = new PendingSyncEntry
        {
            entryType = PendingSyncEntry.EntryTypeEvent,
            payloadJson = UnityEngine.JsonUtility.ToJson(record)
        };
        ControlCenter.Instance.syncQueueManager.Enqueue(entry);
    }

    public static string GetSessionId()
    {
        if (ControlCenter.Instance == null ||
            ControlCenter.Instance.simulationSessionManager == null ||
            ControlCenter.Instance.simulationSessionManager.CurrentSession == null)
        {
            return string.Empty;
        }

        return ControlCenter.Instance.simulationSessionManager.CurrentSession.sessionId;
    }

    #endregion

    #region Localization

    public static string Translate(string key)
    {
        if (ControlCenter.Instance == null || ControlCenter.Instance.languageManager == null)
        {
            return key;
        }

        string translated = ControlCenter.Instance.languageManager.GetText(key);
        return string.IsNullOrEmpty(translated) ? key : translated;
    }

    public static void NotifySubtitle(string locKey, Action<string> localHint = null, bool isError = false)
    {
        string translated = Translate(locKey);
        localHint?.Invoke(translated);
        if (ControlCenter.Instance != null && ControlCenter.Instance.simulationOrchestratorManager != null)
        {
            ControlCenter.Instance.simulationOrchestratorManager.NotifySubtitle(translated, isError);
        }
    }

    public static void NotifySessionSummary()
    {
        ControlCenter center = ControlCenter.ResolveInstance();
        if (center == null || center.simulationOrchestratorManager == null)
        {
            return;
        }

        center.simulationOrchestratorManager.ShowSessionSummary();
    }

    #endregion
}
