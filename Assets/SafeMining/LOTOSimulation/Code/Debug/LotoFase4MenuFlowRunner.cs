#if UNITY_EDITOR
using System.Collections;
using NJ.Finder;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LotoFase4MenuFlowRunner : MonoBehaviour
{
    [SerializeField] private float sceneLoadTimeoutSeconds = 20f;
    [SerializeField] private float stateWaitTimeoutSeconds = 10f;
    [SerializeField] private float subtitleWaitSeconds = 2f;

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

    public void RunMalo1IncompleteStart()
    {
        StopAllCoroutines();
        StartCoroutine(Malo1Routine());
    }

    public void RunMalo2NoGloves()
    {
        StopAllCoroutines();
        StartCoroutine(Malo2Routine());
    }

    #endregion

    #region Flow Routines

    private IEnumerator FullMenuFlowRoutine()
    {
        yield return null;

        if (ControlCenter.Instance == null)
        {
            LogStep("A1 CC present", false, "ControlCenter.Instance is null");
            yield break;
        }

        LogStep("A1 CC present", true, string.Empty);

        bool onMenu = SceneManager.GetActiveScene().buildIndex == LotoSimulationConstants.MainMenuBuildIndex;
        LogStep("A1 menu scene", onMenu, onMenu ? string.Empty : "expected build 0");

        if (!onMenu)
        {
            yield break;
        }

        UIContentCarousel carousel = ResolveCarousel();
        if (carousel == null)
        {
            LogStep("A2 carousel", false, "UIContentCarousel not found");
            yield break;
        }

        int lotoIndex = ResolveLotoCarouselIndex(carousel);
        carousel.currentIndex = lotoIndex;
        LogStep("A2 carousel LOTO index", true, lotoIndex.ToString());

        if (ControlCenter.Instance.ibManager != null)
        {
            ControlCenter.Instance.ibManager.HandleSceneViewerSelect(carousel);
        }
        else
        {
            ControlCenter.Instance.scoreManager.LoadSceneByCarouselIndex();
        }

        yield return WaitForScene(LotoSimulationConstants.Scenario1BuildIndex);
        if (!AssertScene(LotoSimulationConstants.Scenario1BuildIndex, "A2 carousel→S1"))
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

        bool syncQueued = HasCompletedSyncEntry();
        LogStep("A3.6b sync queue Completed", syncQueued, string.Empty);

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

        yield return LaunchS1FromMenu();

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

    private IEnumerator Malo1Routine()
    {
        yield return LaunchS3FromMenu();
        playtestDriver = ResolvePlaytestDriver();
        if (playtestDriver == null)
        {
            yield break;
        }

        playtestDriver.DismissBriefingIfPresent();
        playtestDriver.ForceIncompleteStart();
        yield return new WaitForSeconds(subtitleWaitSeconds);
        LogStep("Malo1 loto_s3_alert_incomplete", SubtitleMatchesLocKey("loto_s3_alert_incomplete"), string.Empty);
    }

    private IEnumerator Malo2Routine()
    {
        yield return LaunchS3FromMenu();
        playtestDriver = ResolvePlaytestDriver();
        if (playtestDriver == null)
        {
            yield break;
        }

        playtestDriver.DismissBriefingIfPresent();
        playtestDriver.ForceNoGloves();
        yield return new WaitForSeconds(subtitleWaitSeconds);
        LogStep("Malo2 loto_s3_alert_no_gloves", SubtitleMatchesLocKey("loto_s3_alert_no_gloves"), string.Empty);
    }

    #endregion

    #region Helpers

    private IEnumerator LaunchS1FromMenu()
    {
        if (SceneManager.GetActiveScene().buildIndex != LotoSimulationConstants.MainMenuBuildIndex)
        {
            ControlCenter.Instance.scoreManager.LoadSceneByBuildIndex(LotoSimulationConstants.Scenario1BuildIndex);
            yield return WaitForScene(LotoSimulationConstants.Scenario1BuildIndex);
            yield break;
        }

        UIContentCarousel carousel = ResolveCarousel();
        if (carousel != null && ControlCenter.Instance.ibManager != null)
        {
            carousel.currentIndex = ResolveLotoCarouselIndex(carousel);
            ControlCenter.Instance.ibManager.HandleSceneViewerSelect(carousel);
        }
        else
        {
            ControlCenter.Instance.scoreManager.LoadSceneByBuildIndex(LotoSimulationConstants.Scenario1BuildIndex);
        }

        yield return WaitForScene(LotoSimulationConstants.Scenario1BuildIndex);
    }

    private IEnumerator LaunchS3FromMenu()
    {
        if (ControlCenter.Instance == null)
        {
            yield break;
        }

        ControlCenter.Instance.scoreManager.LoadSceneByBuildIndex(LotoSimulationConstants.Scenario3BuildIndex);
        yield return WaitForScene(LotoSimulationConstants.Scenario3BuildIndex);
    }

    private static UIContentCarousel ResolveCarousel()
    {
        ref_SceneViewer sceneViewer = ComponentFinder.FindComponentInScene<ref_SceneViewer>();
        if (sceneViewer != null && sceneViewer.uIContentCarousel != null)
        {
            return sceneViewer.uIContentCarousel;
        }

        return FindFirstObjectByType<UIContentCarousel>();
    }

    private static int ResolveLotoCarouselIndex(UIContentCarousel carousel)
    {
        if (carousel == null || carousel.gridLayoutGroup == null)
        {
            return 0;
        }

        Transform root = carousel.gridLayoutGroup.transform;
        for (int i = 0; i < root.childCount; i++)
        {
            TranslatableText[] texts = root.GetChild(i).GetComponentsInChildren<TranslatableText>(true);
            for (int t = 0; t < texts.Length; t++)
            {
                if (texts[t] != null && texts[t].translationID == LotoObjectiveIds.SimulationSceneId)
                {
                    return i;
                }
            }
        }

        return 0;
    }

    private static bool HasCompletedSyncEntry()
    {
        if (ControlCenter.Instance == null || ControlCenter.Instance.syncQueueManager == null)
        {
            return false;
        }

        return ControlCenter.Instance.syncQueueManager.HasCompleteEntryWithStatus(LotoSessionScore.CompletionCompleted);
    }

    private static bool SubtitleMatchesLocKey(string locKey)
    {
        ref_PlayerHUD hud = FindFirstObjectByType<ref_PlayerHUD>();
        if (hud == null || hud.subtitleText == null)
        {
            return false;
        }

        string expected = Util_LotoSimulationBridge.Translate(locKey);
        string actual = hud.subtitleText.text ?? string.Empty;
        return actual.Contains(expected) || actual.Contains(locKey);
    }

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
        if (ControlCenter.Instance != null)
        {
            SimulationOrchestratorManager orchestrator = ControlCenter.Instance.GetManager<SimulationOrchestratorManager>();
            if (orchestrator != null)
            {
                return orchestrator;
            }
        }

        return FindFirstObjectByType<SimulationOrchestratorManager>();
    }

    private static void LogStep(string stepId, bool pass, string detail)
    {
        if (pass)
        {
            Debug.Log("[LOTO Fase4][PASS] " + stepId + (string.IsNullOrEmpty(detail) ? string.Empty : " (" + detail + ")"));
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
#endif
