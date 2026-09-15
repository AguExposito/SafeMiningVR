using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.XR.Interaction.Toolkit.UI;
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
    [SerializeField] private GameObject inspectionBriefing;
    [SerializeField] private GameObject lotoBriefing;
    [SerializeField] private float briefingFollowDistance = 0.9f;

    private PlayerHudManager playerHudManager;
    private bool s3BriefingRequested;

    #region Lifecycle

    private void Start()
    {
        InitializeScene();
        InitializeStandaloneBriefings();
        SubscribeS3Briefing();
    }

    private void OnDestroy()
    {
        LotoGameManager board = LotoGameManager.Instance;
        if (board != null)
        {
            board.S2Completed -= HandleS2CompletedForBriefing;
        }
    }

    private void Update()
    {
        if (s3BriefingRequested || scenarioIndex < LotoSimulationConstants.ScenarioInspection)
        {
            return;
        }

        LotoGameManager board = LotoGameManager.Instance;
        if (board != null && board.IsS2Complete)
        {
            RequestS3Briefing();
        }
    }

    #endregion

    #region Initialization

    private void InitializeScene()
    {
        ResolveHudRefs();
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

        playerHudManager.Initialize(playerHud, null, orchestrator);

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

    private void SubscribeS3Briefing()
    {
        LotoGameManager board = LotoGameManager.Instance;
        if (board == null)
        {
            return;
        }

        board.S2Completed -= HandleS2CompletedForBriefing;
        board.S2Completed += HandleS2CompletedForBriefing;
        if (board.IsS2Complete)
        {
            RequestS3Briefing();
        }
    }

    private void HandleS2CompletedForBriefing()
    {
        RequestS3Briefing();
    }

    private void RequestS3Briefing()
    {
        if (s3BriefingRequested)
        {
            return;
        }

        s3BriefingRequested = true;
        SetPanelActive(inspectionBriefing, false);
        SetPanelActive(lotoBriefing, true);
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

    private void ResolveHudRefs()
    {
        if (playerHud == null)
        {
            playerHud = FindFirstObjectByType<ref_PlayerHUD>(FindObjectsInactive.Include);
        }

        if (briefingPanel == null)
        {
            briefingPanel = FindFirstObjectByType<ref_BriefingPanel>(FindObjectsInactive.Include);
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

    #region Standalone Briefings

    private void InitializeStandaloneBriefings()
    {
        if (inspectionBriefing == null)
        {
            inspectionBriefing = FindNamedChild("Briefing_S2");
        }

        if (lotoBriefing == null)
        {
            lotoBriefing = FindNamedChild("Briefing_S3");
        }

        BindDismissButtons(inspectionBriefing, HideInspectionBriefing);
        BindDismissButtons(lotoBriefing, HideLotoBriefing);
        ConfigureStandalonePanel(inspectionBriefing);
        ConfigureStandalonePanel(lotoBriefing);
        SetPanelActive(inspectionBriefing, true);
        SetPanelActive(lotoBriefing, false);
    }

    private GameObject FindNamedChild(string childName)
    {
        Transform child = transform.Find(childName);
        return child != null ? child.gameObject : null;
    }

    private void HideInspectionBriefing()
    {
        SetPanelActive(inspectionBriefing, false);
    }

    private void HideLotoBriefing()
    {
        SetPanelActive(lotoBriefing, false);
    }

    private static void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
    }

    private void ConfigureStandalonePanel(GameObject panel)
    {
        if (panel == null)
        {
            return;
        }

        Canvas canvas = panel.GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
        }

        GraphicRaycaster graphicRaycaster = panel.GetComponent<GraphicRaycaster>();
        if (graphicRaycaster != null)
        {
            graphicRaycaster.enabled = true;
        }

        TrackedDeviceGraphicRaycaster trackedRaycaster = panel.GetComponent<TrackedDeviceGraphicRaycaster>();
        if (trackedRaycaster != null)
        {
            trackedRaycaster.enabled = true;
        }

        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = panel.AddComponent<CanvasGroup>();
        }

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.ignoreParentGroups = true;

        Util_PanelFollower follower = panel.GetComponent<Util_PanelFollower>();
        if (follower != null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                follower.SetTarget(mainCamera.transform);
            }

            follower.SetFixedDistance(briefingFollowDistance);
            follower.SetOffset(new Vector3(0f, -0.05f, briefingFollowDistance));
        }
    }

    private static void BindDismissButtons(GameObject panel, UnityAction hide)
    {
        if (panel == null || hide == null)
        {
            return;
        }

        Button[] buttons = panel.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            if (button == null)
            {
                continue;
            }

            string buttonName = button.gameObject.name;
            if (buttonName != "BeginButton" && buttonName != "CloseButton")
            {
                continue;
            }

            button.onClick.AddListener(hide);
        }
    }

    #endregion
}
