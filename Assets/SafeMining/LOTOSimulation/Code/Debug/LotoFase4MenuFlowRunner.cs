using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LotoFase4MenuFlowRunner : MonoBehaviour
{
    [SerializeField] private float sceneLoadTimeoutSeconds = 20f;
    [SerializeField] private float stateWaitTimeoutSeconds = 10f;

    private LotoPcPlaytestDriver playtestDriver;

    #region Lifecycle

    private void Awake()
    {
        playtestDriver = FindFirstObjectByType<LotoPcPlaytestDriver>();
    }

    #endregion

    #region Public API

    public void RunFullMenuFlow()
    {
        StopAllCoroutines();
        StartCoroutine(FullMenuFlowRoutine());
    }

    public void RunNegativeGateChecks()
    {
        StopAllCoroutines();
        StartCoroutine(NegativeGateRoutine());
    }

    public void RunS1E2eFlow()
    {
        StopAllCoroutines();
        StartCoroutine(S1E2eRoutine());
    }

    #endregion

    #region Flow Routines

    private IEnumerator S1E2eRoutine()
    {
        yield return null;

        if (ControlCenter.Instance == null)
        {
            LogStep("F5 CC present", false, "ControlCenter.Instance is null");
            yield break;
        }

        LogStep("F5 CC present", true, string.Empty);

        ControlCenter.Instance.scoreManager.LoadSceneByBuildIndex(LotoSimulationConstants.Scenario1BuildIndex);
        yield return WaitForScene(LotoSimulationConstants.Scenario1BuildIndex);
        if (!AssertScene(LotoSimulationConstants.Scenario1BuildIndex, "F5 load S1"))
        {
            yield break;
        }

        playtestDriver = ResolvePlaytestDriver();
        if (playtestDriver == null)
        {
            LogStep("F5 playtest driver", false, "LotoPcPlaytestDriver missing on S1");
            yield break;
        }

        playtestDriver.DismissBriefing();
        yield return WaitForOrchestratorState(LotoSimulationState.Active);
        LogStep("F5.1 dismiss briefing", OrchestratorStateIs(LotoSimulationState.Active), string.Empty);

        playtestDriver.EquipMandatoryFive();
        EppEquipmentManager eppManager = FindFirstObjectByType<EppEquipmentManager>();
        LogStep("F5.2 equip 5/5", eppManager != null && eppManager.IsReadyToAdvance, string.Empty);

        playtestDriver.RejectAllDefects();
        eppManager = FindFirstObjectByType<EppEquipmentManager>();
        LogStep("F5.3 reject defect", eppManager != null && eppManager.IsDefectObjectiveScored, string.Empty);

        playtestDriver.ContinueS1();
        yield return WaitForScene(LotoSimulationConstants.Scenario2BuildIndex);
        if (!AssertScene(LotoSimulationConstants.Scenario2BuildIndex, "F5.4 continue S1→S2"))
        {
            yield break;
        }

        Debug.Log("[LOTO Fase5] S1 E2E happy path finished.");
    }

    private IEnumerator FullMenuFlowRoutine()
    {
        yield return null;

        if (ControlCenter.Instance == null)
        {
            LogStep("A2 CC present", false, "ControlCenter.Instance is null");
            yield break;
        }

        LogStep("A2 CC present", true, string.Empty);

        ControlCenter.Instance.scoreManager.LoadSceneByBuildIndex(LotoSimulationConstants.Scenario1BuildIndex);
        yield return WaitForScene(LotoSimulationConstants.Scenario1BuildIndex);
        if (!AssertScene(LotoSimulationConstants.Scenario1BuildIndex, "A2 load S1"))
        {
            yield break;
        }

        playtestDriver = ResolvePlaytestDriver();
        if (playtestDriver == null)
        {
            LogStep("A3 playtest driver", false, "LotoPcPlaytestDriver missing on S1");
            yield break;
        }

        playtestDriver.DismissBriefing();
        yield return WaitForOrchestratorState(LotoSimulationState.Active);
        LogStep("A3.1 dismiss briefing", OrchestratorStateIs(LotoSimulationState.Active), string.Empty);

        playtestDriver.EquipMandatoryFive();
        EppEquipmentManager eppManager = FindFirstObjectByType<EppEquipmentManager>();
        LogStep("A3.2 equip 5/5", eppManager != null && eppManager.IsReadyToAdvance, string.Empty);

        playtestDriver.RejectAllDefects();
        eppManager = FindFirstObjectByType<EppEquipmentManager>();
        LogStep("A3.2b reject defect", eppManager != null && eppManager.IsDefectObjectiveScored, string.Empty);

        playtestDriver.ContinueS1();
        yield return WaitForScene(LotoSimulationConstants.Scenario2BuildIndex);
        if (!AssertScene(LotoSimulationConstants.Scenario2BuildIndex, "A3.3 continue S1→S2"))
        {
            yield break;
        }

        playtestDriver = ResolvePlaytestDriver();
        playtestDriver.ReportFourHazards();
        HazardInspectionManager hazardManager = FindFirstObjectByType<HazardInspectionManager>();
        LogStep("A3.4 report 4 hazards", hazardManager != null && hazardManager.IsReadyToAdvance, string.Empty);

        playtestDriver.ContinueS2();
        yield return WaitForScene(LotoSimulationConstants.Scenario3BuildIndex);
        if (!AssertScene(LotoSimulationConstants.Scenario3BuildIndex, "A3.5 continue S2→S3"))
        {
            yield break;
        }

        playtestDriver = ResolvePlaytestDriver();
        playtestDriver.RunLotoHappyPath();
        yield return WaitForOrchestratorState(LotoSimulationState.SessionSummary);
        LogStep("A3.6 LOTO happy path summary", OrchestratorStateIs(LotoSimulationState.SessionSummary), string.Empty);

        playtestDriver.CompleteSummary();
        yield return WaitForScene(LotoSimulationConstants.MainMenuBuildIndex);
        LogStep("A3.7 return to menu", SceneManager.GetActiveScene().buildIndex == LotoSimulationConstants.MainMenuBuildIndex, string.Empty);

        Debug.Log("[LOTO Fase4] Full menu flow test finished.");
    }

    private IEnumerator NegativeGateRoutine()
    {
        yield return null;

        if (ControlCenter.Instance == null)
        {
            LogStep("Gate CC present", false, "ControlCenter.Instance is null");
            yield break;
        }

        ControlCenter.Instance.scoreManager.LoadSceneByBuildIndex(LotoSimulationConstants.Scenario1BuildIndex);
        yield return WaitForScene(LotoSimulationConstants.Scenario1BuildIndex);

        playtestDriver = ResolvePlaytestDriver();
        if (playtestDriver == null)
        {
            yield break;
        }

        playtestDriver.DismissBriefing();
        yield return WaitForOrchestratorState(LotoSimulationState.Active);

        int sceneBefore = SceneManager.GetActiveScene().buildIndex;
        playtestDriver.ContinueS1();
        yield return new WaitForSeconds(1f);
        bool s1GateHeld = SceneManager.GetActiveScene().buildIndex == sceneBefore;
        LogStep("Gate S1 block without 5/5", s1GateHeld, string.Empty);

        playtestDriver.EquipMandatoryFive();
        playtestDriver.ContinueS1();
        yield return WaitForScene(LotoSimulationConstants.Scenario2BuildIndex);

        playtestDriver = ResolvePlaytestDriver();
        sceneBefore = SceneManager.GetActiveScene().buildIndex;
        playtestDriver.ContinueS2();
        yield return new WaitForSeconds(1f);
        bool s2GateHeld = SceneManager.GetActiveScene().buildIndex == sceneBefore;
        LogStep("Gate S2 block without 4/4", s2GateHeld, string.Empty);

        Debug.Log("[LOTO Fase4] Negative gate test finished.");
    }

    #endregion

    #region Helpers

    private LotoPcPlaytestDriver ResolvePlaytestDriver()
    {
        if (playtestDriver != null)
        {
            return playtestDriver;
        }

        playtestDriver = FindFirstObjectByType<LotoPcPlaytestDriver>();
        return playtestDriver;
    }

    private IEnumerator WaitForScene(int buildIndex)
    {
        float elapsed = 0f;
        while (SceneManager.GetActiveScene().buildIndex != buildIndex && elapsed < sceneLoadTimeoutSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private IEnumerator WaitForOrchestratorState(LotoSimulationState targetState)
    {
        float elapsed = 0f;
        SimulationOrchestratorManager orchestrator = GetOrchestrator();
        while ((orchestrator == null || orchestrator.CurrentState != targetState) && elapsed < stateWaitTimeoutSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            orchestrator = GetOrchestrator();
            yield return null;
        }
    }

    private bool AssertScene(int expectedBuildIndex, string stepId)
    {
        bool pass = SceneManager.GetActiveScene().buildIndex == expectedBuildIndex;
        LogStep(stepId, pass, pass ? string.Empty : "expected build " + expectedBuildIndex);
        return pass;
    }

    private static bool OrchestratorStateIs(LotoSimulationState state)
    {
        SimulationOrchestratorManager orchestrator = GetOrchestrator();
        return orchestrator != null && orchestrator.CurrentState == state;
    }

    private static SimulationOrchestratorManager GetOrchestrator()
    {
        if (ControlCenter.Instance != null && ControlCenter.Instance.simulationOrchestratorManager != null)
        {
            return ControlCenter.Instance.simulationOrchestratorManager;
        }

        return FindFirstObjectByType<SimulationOrchestratorManager>();
    }

    private static void LogStep(string stepId, bool pass, string detail)
    {
        if (pass)
        {
            Debug.Log("[LOTO Fase4][PASS] " + stepId);
            return;
        }

        if (string.IsNullOrEmpty(detail))
        {
            Debug.LogWarning("[LOTO Fase4][FAIL] " + stepId);
            return;
        }

        Debug.LogWarning("[LOTO Fase4][FAIL] " + stepId + " — " + detail);
    }

    #endregion
}
