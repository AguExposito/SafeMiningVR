using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
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
        LotoPostProcessVolumeBootstrap.EnsureActive();
        BindTeleportationAreas();

        if (ControlCenter.Instance == null)
        {
            Debug.LogWarning("[LOTO] ControlCenter.Instance is null. Open this scene from the main menu.");
            RefreshAmbientDustPlayback();
            return;
        }

        SimulationOrchestratorManager orchestrator = ControlCenter.Instance.simulationOrchestratorManager;
        if (orchestrator == null)
        {
            Debug.LogWarning("[LOTO] simulationOrchestratorManager is null.");
            RefreshAmbientDustPlayback();
            return;
        }

        playerHudManager = GetComponent<PlayerHudManager>();
        if (playerHudManager == null)
        {
            Debug.LogWarning("[LOTO] PlayerHudManager is missing on LotoScenarioSceneBootstrap.");
            RefreshAmbientDustPlayback();
            return;
        }

        playerHudManager.Initialize(playerHud, briefingPanel, orchestrator);

        if (startWithBriefing)
        {
            if (scenarioIndex == LotoSimulationConstants.ScenarioEpp)
            {
                orchestrator.BeginSimulation();
            }
            else
            {
                orchestrator.BeginScenarioBriefing(scenarioIndex);
            }

            RefreshAmbientDustPlayback();
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

        RefreshAmbientDustPlayback();
    }

    private void RefreshAmbientDustPlayback()
    {
        LotoAmbientDustController[] dustControllers = Object.FindObjectsByType<LotoAmbientDustController>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);
        for (int i = 0; i < dustControllers.Length; i++)
        {
            if (dustControllers[i] != null)
            {
                dustControllers[i].RefreshPlayback();
            }
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

    private void BindTeleportationAreas()
    {
        TeleportationProvider provider = Object.FindFirstObjectByType<TeleportationProvider>();
        if (provider == null)
        {
            return;
        }

        TeleportationArea[] areas = Object.FindObjectsByType<TeleportationArea>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < areas.Length; i++)
        {
            TeleportationArea area = areas[i];
            if (area == null)
            {
                continue;
            }

            if (provider != null)
            {
                area.teleportationProvider = provider;
            }

            area.interactionLayers = new InteractionLayerMask
            {
                value = LotoSimulationConstants.XriTeleportInteractionLayerMask
            };
        }
    }

    #endregion
}
