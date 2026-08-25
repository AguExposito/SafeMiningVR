using UnityEngine;
using XRStandardMenu;

public class LotoHubBootstrap : MonoBehaviour
{
    [SerializeField] private ref_PlayerHUD playerHud;
    [SerializeField] private ref_BriefingPanel briefingPanel;
    [SerializeField] private PlayerHudManager playerHudManagerPrefab;
    [SerializeField] private GameObject objectiveRowPrefab;
    [SerializeField] private Transform panelFollowTarget;
    [SerializeField] private GameObject s1ZoneRoot;
    [SerializeField] private GameObject s2ZoneRoot;
    [SerializeField] private GameObject s3ZoneRoot;
    [SerializeField] private LotoScenario1Controller scenario1Controller;
    [SerializeField] private LotoScenario2Controller scenario2Controller;
    [SerializeField] private LotoScenario3Controller scenario3Controller;

    private PlayerHudManager playerHudManagerInstance;

    #region Lifecycle

    private void Start()
    {
        InitializeHub();
    }

    #endregion

    #region Initialization

    private void InitializeHub()
    {
        ResolveReferences();
        ConfigurePanelFollowers();
        EnsureS1Zone();
        EnsureS2Zone();
        EnsureS3Zone();
        EnsurePlayerHudManager();
        WireOrchestrator();
    }

    private void ResolveReferences()
    {
        if (panelFollowTarget == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                panelFollowTarget = mainCamera.transform;
            }
        }

        if (playerHud == null)
        {
            playerHud = FindFirstObjectByType<ref_PlayerHUD>();
        }

        if (briefingPanel == null)
        {
            briefingPanel = FindFirstObjectByType<ref_BriefingPanel>();
        }

        if (playerHud == null)
        {
            playerHud = LotoHubUiBuilder.EnsurePlayerHud(panelFollowTarget, objectiveRowPrefab);
        }

        if (briefingPanel == null)
        {
            briefingPanel = LotoHubUiBuilder.EnsureBriefingPanel(panelFollowTarget);
        }

#if UNITY_EDITOR
        if (playerHud == null)
        {
            Debug.LogWarning("[LOTO] ref_PlayerHUD not found in hub scene.");
        }

        if (briefingPanel == null)
        {
            Debug.LogWarning("[LOTO] ref_BriefingPanel not found in hub scene.");
        }
#endif
    }

    private void EnsureS1Zone()
    {
        if (s1ZoneRoot == null)
        {
            s1ZoneRoot = GameObject.Find("S1_ZoneRoot");
        }

        if (s1ZoneRoot == null)
        {
            s1ZoneRoot = new GameObject("S1_ZoneRoot");
            s1ZoneRoot.transform.SetParent(transform, false);
        }

        s1ZoneRoot.SetActive(false);

        if (scenario1Controller == null)
        {
            scenario1Controller = s1ZoneRoot.GetComponent<LotoScenario1Controller>();
        }

        if (scenario1Controller == null)
        {
            scenario1Controller = s1ZoneRoot.AddComponent<LotoScenario1Controller>();
        }

        EppEquipmentManager equipmentManager = s1ZoneRoot.GetComponentInChildren<EppEquipmentManager>(true);
        if (equipmentManager == null)
        {
            equipmentManager = s1ZoneRoot.AddComponent<EppEquipmentManager>();
        }

        LotoS1EnvironmentBuilder builder = s1ZoneRoot.GetComponentInChildren<LotoS1EnvironmentBuilder>(true);
        if (builder == null)
        {
            builder = s1ZoneRoot.AddComponent<LotoS1EnvironmentBuilder>();
        }

        if (s3ZoneRoot == null)
        {
            s3ZoneRoot = GameObject.Find("S3_ZoneRoot");
        }

        scenario1Controller.Configure(s1ZoneRoot, s3ZoneRoot, equipmentManager, builder);
#if UNITY_EDITOR
        if (s1ZoneRoot.GetComponent<LotoS1EppSmokeTest>() == null)
        {
            s1ZoneRoot.AddComponent<LotoS1EppSmokeTest>();
        }
#endif
    }

    private void EnsureS2Zone()
    {
        if (s2ZoneRoot == null)
        {
            s2ZoneRoot = GameObject.Find("S2_ZoneRoot");
        }

        if (s2ZoneRoot == null)
        {
            s2ZoneRoot = new GameObject("S2_ZoneRoot");
            s2ZoneRoot.transform.SetParent(transform, false);
        }

        s2ZoneRoot.SetActive(false);

        if (scenario2Controller == null)
        {
            scenario2Controller = s2ZoneRoot.GetComponent<LotoScenario2Controller>();
        }

        if (scenario2Controller == null)
        {
            scenario2Controller = s2ZoneRoot.AddComponent<LotoScenario2Controller>();
        }

        HazardInspectionManager inspectionManager = s2ZoneRoot.GetComponentInChildren<HazardInspectionManager>(true);
        if (inspectionManager == null)
        {
            inspectionManager = s2ZoneRoot.AddComponent<HazardInspectionManager>();
        }

        LotoS2EnvironmentBuilder builder = s2ZoneRoot.GetComponentInChildren<LotoS2EnvironmentBuilder>(true);
        if (builder == null)
        {
            builder = s2ZoneRoot.AddComponent<LotoS2EnvironmentBuilder>();
        }

        if (s3ZoneRoot == null)
        {
            s3ZoneRoot = GameObject.Find("S3_ZoneRoot");
        }

        if (s1ZoneRoot == null)
        {
            s1ZoneRoot = GameObject.Find("S1_ZoneRoot");
        }

        scenario2Controller.Configure(s2ZoneRoot, s1ZoneRoot, s3ZoneRoot, inspectionManager, builder);
#if UNITY_EDITOR
        if (s2ZoneRoot.GetComponent<LotoS2HazardSmokeTest>() == null)
        {
            s2ZoneRoot.AddComponent<LotoS2HazardSmokeTest>();
        }
#endif
    }

    private void EnsureS3Zone()
    {
        if (s3ZoneRoot == null)
        {
            s3ZoneRoot = GameObject.Find("S3_ZoneRoot");
        }

        if (s3ZoneRoot == null)
        {
            s3ZoneRoot = new GameObject("S3_ZoneRoot");
            s3ZoneRoot.transform.SetParent(transform, false);
        }

        s3ZoneRoot.SetActive(false);

        if (scenario3Controller == null)
        {
            scenario3Controller = s3ZoneRoot.GetComponent<LotoScenario3Controller>();
        }

        if (scenario3Controller == null)
        {
            scenario3Controller = s3ZoneRoot.AddComponent<LotoScenario3Controller>();
        }

        LotoSequenceManager sequenceManager = s3ZoneRoot.GetComponentInChildren<LotoSequenceManager>(true);
        if (sequenceManager == null)
        {
            sequenceManager = s3ZoneRoot.AddComponent<LotoSequenceManager>();
        }

        LotoS3EnvironmentBuilder builder = s3ZoneRoot.GetComponentInChildren<LotoS3EnvironmentBuilder>(true);
        if (builder == null)
        {
            builder = s3ZoneRoot.AddComponent<LotoS3EnvironmentBuilder>();
        }

        scenario3Controller.Configure(s3ZoneRoot, sequenceManager, builder);
#if UNITY_EDITOR
        if (s3ZoneRoot.GetComponent<LotoS3InteractableSmokeTest>() == null)
        {
            s3ZoneRoot.AddComponent<LotoS3InteractableSmokeTest>();
        }
#endif
    }

    private void ConfigurePanelFollowers()
    {
        if (panelFollowTarget == null)
        {
            return;
        }

        Util_PanelFollower[] followers = FindObjectsByType<Util_PanelFollower>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < followers.Length; i++)
        {
            if (followers[i] == null)
            {
                continue;
            }

            followers[i].SetTarget(panelFollowTarget);
        }
    }

    private void EnsurePlayerHudManager()
    {
        if (playerHudManagerInstance != null)
        {
            return;
        }

        if (playerHudManagerPrefab != null)
        {
            playerHudManagerInstance = Instantiate(playerHudManagerPrefab, transform);
        }
        else
        {
            playerHudManagerInstance = gameObject.AddComponent<PlayerHudManager>();
        }
    }

    private void WireOrchestrator()
    {
        if (ControlCenter.Instance == null)
        {
            Debug.LogError("[LOTO] ControlCenter.Instance is null in hub scene.");
            return;
        }

        SimulationOrchestratorManager orchestrator = ControlCenter.Instance.simulationOrchestratorManager;
        if (orchestrator == null)
        {
            Debug.LogError("[LOTO] simulationOrchestratorManager is null.");
            return;
        }

        playerHudManagerInstance.Initialize(playerHud, briefingPanel, orchestrator);
        orchestrator.BeginSimulation();
    }

    #endregion
}
