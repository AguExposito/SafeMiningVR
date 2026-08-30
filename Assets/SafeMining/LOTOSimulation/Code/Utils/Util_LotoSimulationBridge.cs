using System;
using XRStandardMenu;

public static class Util_LotoSimulationBridge
{
    #region Scoring

    public static void SetObjective(int objectiveIndex, bool achieved)
    {
        if (ControlCenter.Instance == null || ControlCenter.Instance.GetManager<ScoreManager>() == null)
        {
            return;
        }

        ScoreManager scoreManager = ControlCenter.Instance.GetManager<ScoreManager>();
        if (scoreManager == null)
        {
            return;
        }

        scoreManager.AchieveObjective(
            LotoSimulationConstants.LotoSimScoreIndex,
            objectiveIndex,
            achieved);

        if (ControlCenter.Instance.GetManager<SimulationOrchestratorManager>() != null)
        {
            ControlCenter.Instance.GetManager<SimulationOrchestratorManager>().NotifyObjectiveVisualUpdate(objectiveIndex, achieved);
        }
    }

    #endregion

    #region Sync

    public static void EnqueueEvent(SimulationEventRecord record)
    {
        if (ControlCenter.Instance == null || ControlCenter.Instance.GetManager<SyncQueueManager>() == null || record == null)
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
        ControlCenter.Instance.GetManager<SyncQueueManager>().Enqueue(entry);
    }

    public static string GetSessionId()
    {
        if (ControlCenter.Instance == null ||
            ControlCenter.Instance.GetManager<SimulationSessionManager>() == null ||
            ControlCenter.Instance.GetManager<SimulationSessionManager>().CurrentSession == null)
        {
            return string.Empty;
        }

        return ControlCenter.Instance.GetManager<SimulationSessionManager>().CurrentSession.sessionId;
    }

    #endregion

    #region Localization

    public static string Translate(string key)
    {
        if (ControlCenter.Instance == null || ControlCenter.Instance.GetManager<LanguageManager>() == null)
        {
            return key;
        }

        string translated = ControlCenter.Instance.GetManager<LanguageManager>().GetText(key);
        return string.IsNullOrEmpty(translated) ? key : translated;
    }

    public static void NotifySubtitle(string locKey, Action<string> localHint = null)
    {
        string translated = Translate(locKey);
        localHint?.Invoke(translated);
        if (ControlCenter.Instance != null && ControlCenter.Instance.GetManager<SimulationOrchestratorManager>() != null)
        {
            ControlCenter.Instance.GetManager<SimulationOrchestratorManager>().NotifySubtitle(translated);
        }
    }

    #endregion
}
