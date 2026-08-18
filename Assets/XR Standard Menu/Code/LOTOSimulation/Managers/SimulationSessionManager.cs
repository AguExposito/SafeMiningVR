using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SimulationSessionManager : MonoBehaviour
{
    private SimulationSessionContext currentSession;
    private string sessionContextFilePath;

    public SimulationSessionContext CurrentSession => currentSession;

    #region Initialization

    public void Initialize()
    {
        sessionContextFilePath = Path.Combine(Application.persistentDataPath, "SimulationSessionContext.json");
        LoadSessionContext();
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    #endregion

    #region Event Handlers

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 0)
        {
            return;
        }
        if (currentSession != null && currentSession.isActive)
        {
            return;
        }
        StartNewSession();
    }

    #endregion

    #region Public API

    public void StartNewSession()
    {
        string participantId = string.Empty;
        if (ControlCenter.Instance != null && ControlCenter.Instance.persistantDataManager != null)
        {
            participantId = ControlCenter.Instance.persistantDataManager.userPersistantData.participantID;
        }
        currentSession = new SimulationSessionContext
        {
            sessionId = Guid.NewGuid().ToString("N"),
            intentoId = string.Empty,
            programId = string.Empty,
            capacitacionId = LotoObjectiveIds.SimulationSceneId,
            participantId = participantId,
            startedAtUtc = DateTime.UtcNow.ToString("o"),
            simulationVersion = Application.version,
            isActive = true
        };
        SaveSessionContext();
        EnqueueSessionStart();
        Debug.Log($"[SimulationSessionManager] Session started id={currentSession.sessionId}");
    }

    public void CompleteCurrentSession(SimulationResultPayload result)
    {
        if (currentSession == null)
        {
            return;
        }
        currentSession.isActive = false;
        if (result != null && string.IsNullOrEmpty(result.sessionId))
        {
            result.sessionId = currentSession.sessionId;
        }
        SaveSessionContext();
        Debug.Log($"[SimulationSessionManager] Session completed id={currentSession.sessionId}");
    }

    #endregion

    #region Persistence

    private void LoadSessionContext()
    {
        if (!File.Exists(sessionContextFilePath))
        {
            return;
        }
        try
        {
            string json = File.ReadAllText(sessionContextFilePath);
            SimulationSessionContext loaded = JsonUtility.FromJson<SimulationSessionContext>(json);
            if (loaded != null)
            {
                currentSession = loaded;
            }
        }
        catch (Exception exception)
        {
            Debug.LogError($"[SimulationSessionManager] Load failed: {exception.Message}");
        }
    }

    private void SaveSessionContext()
    {
        if (currentSession == null)
        {
            return;
        }
        try
        {
            string json = JsonUtility.ToJson(currentSession, true);
            File.WriteAllText(sessionContextFilePath, json);
        }
        catch (Exception exception)
        {
            Debug.LogError($"[SimulationSessionManager] Save failed: {exception.Message}");
        }
    }

    #endregion

    #region Sync Queue

    private void EnqueueSessionStart()
    {
        if (ControlCenter.Instance == null || ControlCenter.Instance.syncQueueManager == null || currentSession == null)
        {
            return;
        }
        PendingSyncEntry entry = new PendingSyncEntry
        {
            entryType = PendingSyncEntry.EntryTypeStart,
            payloadJson = JsonUtility.ToJson(currentSession)
        };
        ControlCenter.Instance.syncQueueManager.Enqueue(entry);
    }

    #endregion
}
