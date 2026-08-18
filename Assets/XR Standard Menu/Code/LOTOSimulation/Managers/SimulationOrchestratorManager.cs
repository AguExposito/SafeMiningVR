using System;
using UnityEngine;

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
            Debug.LogError("[SimulationOrchestratorManager] ControlCenter.Instance is null.");
            return;
        }

        InitializeSessionScore();
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
        EnterScenario1();
    }

    public void EnterScenario1()
    {
        EnterScenario(LotoSimulationConstants.ScenarioEpp);

        LotoScenario1Controller scenarioController = FindFirstObjectByType<LotoScenario1Controller>();
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

        LotoScenario2Controller scenarioController = FindFirstObjectByType<LotoScenario2Controller>();
        if (scenarioController == null)
        {
            GameObject controllerObject = new GameObject("LotoScenario2Controller");
            scenarioController = controllerObject.AddComponent<LotoScenario2Controller>();
        }

        scenarioController.Begin();
    }

    public void OnScenario2Completed()
    {
        HazardInspectionManager manager = FindFirstObjectByType<HazardInspectionManager>();
        if (manager != null && !manager.IsReadyToAdvance)
        {
            return;
        }

        scenario2Completed = true;
        EnterScenario3();
    }

    public void OnScenario1Completed()
    {
        EppEquipmentManager manager = FindFirstObjectByType<EppEquipmentManager>();
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
        EnterScenario(LotoSimulationConstants.ScenarioLoto);

        LotoScenario3Controller scenarioController = FindFirstObjectByType<LotoScenario3Controller>();
        if (scenarioController == null)
        {
            GameObject controllerObject = new GameObject("LotoScenario3Controller");
            scenarioController = controllerObject.AddComponent<LotoScenario3Controller>();
        }

        scenarioController.Begin();
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

    public void ShowSessionSummary(bool success)
    {
        SetState(LotoSimulationState.SessionSummary);
        OnSessionSummaryRequested?.Invoke(success);
        OnSubtitleChanged?.Invoke(GetTranslation("loto_s3_summary_title"));
    }

    public async void CompleteSimulation()
    {
        if (currentState != LotoSimulationState.SessionSummary)
        {
            return;
        }

        SetState(LotoSimulationState.Exiting);
        FinalizeSessionScore();

        if (ControlCenter.Instance != null && ControlCenter.Instance.loadManager != null)
        {
            await ControlCenter.Instance.loadManager.LoadScene_VisualProgress(LotoSimulationConstants.MainMenuBuildIndex);
        }
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
        FinalizeSessionScore();

        if (ControlCenter.Instance != null && ControlCenter.Instance.loadManager != null)
        {
            await ControlCenter.Instance.loadManager.LoadScene_VisualProgress(LotoSimulationConstants.MainMenuBuildIndex);
        }
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

    private void FinalizeSessionScore()
    {
        if (!sessionScoreInitialized || ControlCenter.Instance == null || ControlCenter.Instance.scoreManager == null)
        {
            return;
        }

        ControlCenter.Instance.scoreManager.EndScene(LotoSimulationConstants.LotoSimScoreIndex);
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
