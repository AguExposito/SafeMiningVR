using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum LotoSimulationState
{
    Initializing,
    Briefing,
    Active,
    ExitConfirm,
    SessionSummary,
    Exiting
}

public class SimulationOrchestratorManager : MonoBehaviour
{
    public event Action<int, int> OnScenarioChanged;
    public event Action<int, bool> OnObjectiveVisualUpdate;
    public event Action<string> OnSubtitleChanged;
    public event Action<LotoSimulationState> OnStateChanged;

    public event Action<bool> OnSessionSummaryRequested;

    private LotoSimulationState currentState = LotoSimulationState.Initializing;
    private int currentScenarioIndex = 1;
    private bool sessionScoreInitialized;
    private float activeStartTime;
    private bool scenario1Completed;
    private bool scenario2Completed;

    public LotoSimulationState CurrentState => currentState;
    public int CurrentScenarioIndex => currentScenarioIndex;

    #region Initialization

    public void Initialize()
    {
        currentState = LotoSimulationState.Initializing;
        currentScenarioIndex = 1;
        sessionScoreInitialized = false;
        scenario1Completed = false;
        scenario2Completed = false;
    }

    #endregion

    #region Public API

    public void BeginSimulation()
    {
        if (ControlCenter.Instance == null)
        {
            Debug.LogWarning("[SimulationOrchestratorManager] ControlCenter.Instance is null. Open S1 from the main menu.");
            return;
        }

        InitializeSessionScore();
        if (ControlCenter.Instance.simulationSessionManager != null)
        {
            ControlCenter.Instance.simulationSessionManager.StartNewSession();
        }

        EnterBriefingState();
    }

    public void DismissBriefing()
    {
        if (currentState != LotoSimulationState.Briefing)
        {
            return;
        }

        activeStartTime = Time.time;
        SetState(LotoSimulationState.Active);
        EnterScenario(LotoSimulationConstants.ScenarioEpp);

        LotoScenario1Controller scenarioController = FindFirstObjectByType<LotoScenario1Controller>(FindObjectsInactive.Include);
        if (scenarioController != null)
        {
            scenarioController.Begin();
        }
    }

    public void EnterScenario1()
    {
        EnterScenario(LotoSimulationConstants.ScenarioEpp);

        LotoScenario1Controller scenarioController = FindFirstObjectByType<LotoScenario1Controller>(FindObjectsInactive.Include);
        if (scenarioController == null)
        {
            GameObject controllerObject = new GameObject("LotoScenario1Controller");
            scenarioController = controllerObject.AddComponent<LotoScenario1Controller>();
        }

        scenarioController.Begin();
    }

    public void EnterScenario2()
    {
        if (!CanAdvanceToScenario(LotoSimulationConstants.ScenarioInspection))
        {
            return;
        }

        EnterScenario(LotoSimulationConstants.ScenarioInspection);
        LoadScenarioSceneIfNeeded(LotoSimulationConstants.Scenario2BuildIndex);
    }

    public void OnScenario2Completed()
    {
        HazardInspectionManager manager = FindFirstObjectByType<HazardInspectionManager>(FindObjectsInactive.Include);
        if (manager != null && !manager.IsReadyToAdvance)
        {
            return;
        }

        scenario2Completed = true;
        EnterScenario3();
    }

    public void OnScenario1Completed()
    {
        EppEquipmentManager manager = FindFirstObjectByType<EppEquipmentManager>(FindObjectsInactive.Include);
        if (manager != null && !manager.IsReadyToAdvance)
        {
            return;
        }

        scenario1Completed = true;
        EnterScenario2();
    }

    public void EnterScenario(int scenarioIndex)
    {
        if (!CanAdvanceToScenario(scenarioIndex))
        {
            return;
        }

        currentScenarioIndex = scenarioIndex;
        OnScenarioChanged?.Invoke(currentScenarioIndex, LotoSimulationConstants.TotalScenarios);
        NotifySubtitleForScenario(scenarioIndex);
    }

    public void EnterScenario3()
    {
        if (!CanAdvanceToScenario(LotoSimulationConstants.ScenarioLoto))
        {
            return;
        }

        EnterScenario(LotoSimulationConstants.ScenarioLoto);
        LoadScenarioSceneIfNeeded(LotoSimulationConstants.Scenario3BuildIndex);
    }

    public void NotifyAuthoredSceneLoaded(int scenarioIndex)
    {
        InitializeSessionScore();
        currentScenarioIndex = scenarioIndex;

        if (currentState != LotoSimulationState.Active &&
            currentState != LotoSimulationState.ExitConfirm &&
            currentState != LotoSimulationState.SessionSummary)
        {
            if (activeStartTime <= 0f)
            {
                activeStartTime = Time.time;
            }

            SetState(LotoSimulationState.Active);
        }

        OnScenarioChanged?.Invoke(currentScenarioIndex, LotoSimulationConstants.TotalScenarios);
        NotifySubtitleForScenario(scenarioIndex);
    }

    public bool CanAdvanceToScenario(int scenarioIndex)
    {
        if (scenarioIndex == LotoSimulationConstants.ScenarioEpp)
        {
            return currentState == LotoSimulationState.Active || currentState == LotoSimulationState.Briefing;
        }

        if (scenarioIndex == LotoSimulationConstants.ScenarioInspection)
        {
            return scenario1Completed && currentState == LotoSimulationState.Active;
        }

        if (scenarioIndex == LotoSimulationConstants.ScenarioLoto)
        {
            return scenario2Completed && currentState == LotoSimulationState.Active;
        }

        return false;
    }

    public void NotifySubtitle(string subtitle)
    {
        if (string.IsNullOrEmpty(subtitle))
        {
            return;
        }

        OnSubtitleChanged?.Invoke(subtitle);
    }

    public void ShowSessionSummary()
    {
        SetState(LotoSimulationState.SessionSummary);
        SceneScore activeScore = null;
        if (ControlCenter.Instance != null && ControlCenter.Instance.scoreManager != null)
        {
            activeScore = ControlCenter.Instance.scoreManager.GetActiveSceneScore(LotoSimulationConstants.LotoSimScoreIndex);
        }

        int falsePositiveCount = 0;
        if (ControlCenter.Instance != null && ControlCenter.Instance.simulationSessionManager != null)
        {
            falsePositiveCount = ControlCenter.Instance.simulationSessionManager.GetFalsePositiveCount();
        }

        SimulationResultPayload payload = LotoSessionScore.Build(
            activeScore,
            falsePositiveCount,
            GetActiveElapsedSeconds(),
            DateTime.UtcNow,
            LotoSessionScore.CompletionCompleted);
        OnSessionSummaryRequested?.Invoke(payload.approved);
        OnSubtitleChanged?.Invoke(GetTranslation("loto_s3_summary_title"));
    }

    public async void CompleteSimulation()
    {
        if (currentState != LotoSimulationState.SessionSummary)
        {
            return;
        }

        SetState(LotoSimulationState.Exiting);
        FinalizeSessionScore(LotoSessionScore.CompletionCompleted);
        await LoadMainMenu();
    }

    public void RequestExitToMenu()
    {
        if (currentState == LotoSimulationState.Exiting)
        {
            return;
        }

        SetState(LotoSimulationState.ExitConfirm);
    }

    public void CancelExit()
    {
        if (currentState != LotoSimulationState.ExitConfirm)
        {
            return;
        }

        SetState(LotoSimulationState.Active);
    }

    public async void ConfirmExit()
    {
        if (currentState != LotoSimulationState.ExitConfirm)
        {
            return;
        }

        SetState(LotoSimulationState.Exiting);
        FinalizeSessionScore(LotoSessionScore.CompletionAbandoned);
        await LoadMainMenu();
    }

    public float GetActiveElapsedSeconds()
    {
        if (currentState != LotoSimulationState.Active &&
            currentState != LotoSimulationState.ExitConfirm &&
            currentState != LotoSimulationState.SessionSummary)
        {
            if (ControlCenter.Instance != null &&
                ControlCenter.Instance.scoreManager != null &&
                ControlCenter.Instance.scoreManager.TryGetActiveSceneElapsed(LotoSimulationConstants.LotoSimScoreIndex, out float elapsed))
            {
                return elapsed;
            }
            return 0f;
        }

        if (ControlCenter.Instance != null &&
            ControlCenter.Instance.scoreManager != null &&
            ControlCenter.Instance.scoreManager.TryGetActiveSceneElapsed(LotoSimulationConstants.LotoSimScoreIndex, out float scoreElapsed))
        {
            return scoreElapsed;
        }

        return Mathf.Max(0f, Time.time - activeStartTime);
    }

    public void NotifyObjectiveVisualUpdate(int objectiveIndex, bool achieved)
    {
        OnObjectiveVisualUpdate?.Invoke(objectiveIndex, achieved);
    }

    private async void LoadScenarioSceneIfNeeded(int buildIndex)
    {
        if (SceneManager.GetActiveScene().buildIndex == buildIndex)
        {
            return;
        }

        bool canShowWaiter = ControlCenter.Instance != null &&
            ControlCenter.Instance.loadManager != null &&
            SBD_Prefab.Instance != null &&
            SBD_Prefab.Instance.UI_Waiter_normal != null;

        if (canShowWaiter)
        {
            await ControlCenter.Instance.loadManager.LoadScene_VisualProgress(buildIndex);
            return;
        }

        SceneManager.LoadScene(buildIndex);
    }

    private async System.Threading.Tasks.Task LoadMainMenu()
    {
        bool canShowWaiter = ControlCenter.Instance != null &&
            ControlCenter.Instance.loadManager != null &&
            SBD_Prefab.Instance != null &&
            SBD_Prefab.Instance.UI_Waiter_normal != null;

        if (canShowWaiter)
        {
            await ControlCenter.Instance.loadManager.LoadScene_VisualProgress(LotoSimulationConstants.MainMenuBuildIndex);
            return;
        }

        SceneManager.LoadScene(LotoSimulationConstants.MainMenuBuildIndex);
    }

    private void NotifySubtitleForScenario(int scenarioIndex)
    {
        switch (scenarioIndex)
        {
            case LotoSimulationConstants.ScenarioEpp:
                OnSubtitleChanged?.Invoke(GetTranslation("loto_s1_intro"));
                break;
            case LotoSimulationConstants.ScenarioInspection:
                OnSubtitleChanged?.Invoke(GetTranslation("loto_s2_intro"));
                break;
            case LotoSimulationConstants.ScenarioLoto:
                OnSubtitleChanged?.Invoke(GetTranslation("loto_s3_intro"));
                break;
            default:
                OnSubtitleChanged?.Invoke(GetTranslation("loto_subtitle_welcome"));
                break;
        }
    }

    #endregion

    #region State Machine

    private void EnterBriefingState()
    {
        SetState(LotoSimulationState.Briefing);
        OnScenarioChanged?.Invoke(currentScenarioIndex, LotoSimulationConstants.TotalScenarios);
        OnSubtitleChanged?.Invoke(GetTranslation("loto_briefing_body"));
    }

    private void SetState(LotoSimulationState newState)
    {
        currentState = newState;
        OnStateChanged?.Invoke(currentState);
    }

    #endregion

    #region Score Integration

    private void InitializeSessionScore()
    {
        if (sessionScoreInitialized || ControlCenter.Instance == null || ControlCenter.Instance.scoreManager == null)
        {
            return;
        }

        ControlCenter.Instance.scoreManager.InitializeScene(LotoSimulationConstants.LotoSimScoreIndex);
        sessionScoreInitialized = true;
    }

    private void FinalizeSessionScore(string completionStatus)
    {
        if (!sessionScoreInitialized || ControlCenter.Instance == null || ControlCenter.Instance.scoreManager == null)
        {
            return;
        }

        ControlCenter.Instance.scoreManager.EndScene(
            LotoSimulationConstants.LotoSimScoreIndex,
            completionStatus);
        sessionScoreInitialized = false;
    }

    #endregion

    #region Localization

    private static string GetTranslation(string key)
    {
        if (ControlCenter.Instance == null || ControlCenter.Instance.languageManager == null || string.IsNullOrEmpty(key))
        {
            return key;
        }

        string translated = ControlCenter.Instance.languageManager.GetText(key);
        return string.IsNullOrEmpty(translated) ? key : translated;
    }

    #endregion
}
