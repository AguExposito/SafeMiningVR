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
        if (scene.buildIndex != LotoSimulationConstants.Scenario1BuildIndex)
        {
            return;
        }

        if (currentSession != null && currentSession.isActive && !SessionOwnerMatchesLogin())
        {
            currentSession.isActive = false;
            SaveSessionContext();
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
        if (currentSession != null && currentSession.isActive)
        {
            return;
        }

        string participantId = string.Empty;
        string loginUsername = string.Empty;
        string programId = string.Empty;
        string capacitacionId = LotoObjectiveIds.SimulationSceneId;
        if (LotoVrLoginController.Instance != null && LotoVrLoginController.Instance.IsLoggedIn)
        {
            LotoVrLoginController.VrLoginSession vrSession = LotoVrLoginController.Instance.Session;
            loginUsername = vrSession != null ? vrSession.username ?? string.Empty : string.Empty;
        }
        else if (LotoVrLoginController.Instance == null
            && ControlCenter.Instance != null
            && ControlCenter.Instance.persistantDataManager != null)
        {
            participantId = ControlCenter.Instance.persistantDataManager.userPersistantData.participantID;
        }

        currentSession = new SimulationSessionContext
        {
            sessionId = Guid.NewGuid().ToString("D"),
            intentoId = string.Empty,
            programId = programId,
            capacitacionId = capacitacionId,
            participantId = participantId,
            loginUsername = loginUsername,
            startedAtUtc = DateTime.UtcNow.ToString("o"),
            simulationVersion = Application.version,
            isActive = true,
            falsePositiveCount = 0
        };
        SaveSessionContext();
        EnqueueSessionStart();
        Debug.Log($"[SimulationSessionManager] Session started id={currentSession.sessionId}");
    }

    public void RecordHazardFalsePositive()
    {
        if (currentSession == null || !currentSession.isActive)
        {
            return;
        }

        currentSession.falsePositiveCount++;
        SaveSessionContext();
    }

    public int GetFalsePositiveCount()
    {
        if (currentSession == null)
        {
            return 0;
        }

        return currentSession.falsePositiveCount;
    }

    public void InvalidateActiveSession()
    {
        if (currentSession == null || !currentSession.isActive)
        {
            return;
        }

        currentSession.isActive = false;
        SaveSessionContext();
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

    private bool SessionOwnerMatchesLogin()
    {
        if (LotoVrLoginController.Instance == null || currentSession == null)
        {
            return true;
        }

        if (LotoVrLoginController.Instance.Mode == LotoVrLoginController.ConnectionMode.Offline)
        {
            return true;
        }

        if (!LotoVrLoginController.Instance.IsLoggedIn || LotoVrLoginController.Instance.Session == null)
        {
            return false;
        }

        string currentName = LotoVrLoginController.Instance.Session.username ?? string.Empty;
        string bound = currentSession.loginUsername ?? string.Empty;
        return !string.IsNullOrEmpty(currentName) && currentName == bound;
    }

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
            Debug.LogWarning($"[SimulationSessionManager] Load failed: {exception.Message}");
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
            Debug.LogWarning($"[SimulationSessionManager] Save failed: {exception.Message}");
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

        LotoVrLoginController login = LotoVrLoginController.Instance;
        if (login != null
            && login.Mode == LotoVrLoginController.ConnectionMode.Online
            && login.IsLoggedIn)
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
