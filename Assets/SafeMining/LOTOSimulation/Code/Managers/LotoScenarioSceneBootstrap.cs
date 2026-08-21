using UnityEngine;
using XRStandardMenu;

public class LotoScenarioSceneBootstrap : MonoBehaviour
{
    [SerializeField] private ref_PlayerHUD playerHud;
    [SerializeField] private ref_BriefingPanel briefingPanel;
    [SerializeField] private Transform panelFollowTarget;
    [SerializeField] private int scenarioIndex = 1;
    [SerializeField] private bool startWithBriefing = true;
    [SerializeField] private LotoScenario2Controller scenario2Controller;
    [SerializeField] private LotoScenario3Controller scenario3Controller;

    private PlayerHudManager playerHudManager;

    #region Lifecycle

    private void Start()
    {
        InitializeScene();
    }

    #endregion

    #region Initialization

    private void InitializeScene()
    {
        ResolveFollowTarget();
        ConfigurePanelFollowers();
        EnsurePcPlaytestDriver();

        if (ControlCenter.Instance == null)
        {
            Debug.LogWarning("[LOTO] ControlCenter.Instance is null. Open this scene from the main menu.");
            return;
        }

        SimulationOrchestratorManager orchestrator = ControlCenter.Instance.simulationOrchestratorManager;
        if (orchestrator == null)
        {
            Debug.LogWarning("[LOTO] simulationOrchestratorManager is null.");
            return;
        }

        playerHudManager = GetComponent<PlayerHudManager>();
        if (playerHudManager == null)
        {
            Debug.LogWarning("[LOTO] PlayerHudManager is missing on LotoScenarioSceneBootstrap.");
            return;
        }

        playerHudManager.Initialize(playerHud, briefingPanel, orchestrator);

        if (startWithBriefing)
        {
            orchestrator.BeginSimulation();
            return;
        }

        orchestrator.NotifyAuthoredSceneLoaded(scenarioIndex);

        if (scenario2Controller != null)
        {
            scenario2Controller.Begin();
        }

        if (scenario3Controller != null)
        {
            scenario3Controller.Begin();
        }
    }

    private void ResolveFollowTarget()
    {
        if (panelFollowTarget != null)
        {
            return;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            panelFollowTarget = mainCamera.transform;
        }
    }

    private void ConfigurePanelFollowers()
    {
        if (panelFollowTarget == null)
        {
            return;
        }

        Util_PanelFollower[] followers = GetComponentsInChildren<Util_PanelFollower>(true);
        for (int i = 0; i < followers.Length; i++)
        {
            if (followers[i] != null)
            {
                followers[i].SetTarget(panelFollowTarget);
            }
        }
    }

    private void EnsurePcPlaytestDriver()
    {
        if (GetComponent<LotoPcPlaytestDriver>() == null)
        {
            gameObject.AddComponent<LotoPcPlaytestDriver>();
        }
    }

    #endregion
}
