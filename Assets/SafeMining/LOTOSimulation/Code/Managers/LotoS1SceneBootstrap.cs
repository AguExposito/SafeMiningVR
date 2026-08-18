using UnityEngine;
using XRStandardMenu;

public class LotoS1SceneBootstrap : MonoBehaviour
{
    [SerializeField] private ref_PlayerHUD playerHud;
    [SerializeField] private ref_BriefingPanel briefingPanel;
    [SerializeField] private Transform panelFollowTarget;

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

        if (ControlCenter.Instance == null)
        {
            Debug.LogWarning("[LOTO] ControlCenter.Instance is null. Open S1 from the main menu.");
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
            Debug.LogWarning("[LOTO] PlayerHudManager is missing on LotoS1SceneBootstrap.");
            return;
        }

        playerHudManager.Initialize(playerHud, briefingPanel, orchestrator);
        orchestrator.BeginSimulation();
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

    #endregion
}
