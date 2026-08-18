using UnityEngine;

public class LotoScenario2Controller : MonoBehaviour
{
    [SerializeField] private GameObject s2ZoneRoot;
    [SerializeField] private GameObject s3ZoneRoot;
    [SerializeField] private HazardInspectionManager inspectionManager;
    [SerializeField] private LotoS2EnvironmentBuilder environmentBuilder;
    [SerializeField] private bool buildEnvironmentOnBegin = true;

    private bool hasBegun;

    #region Public API

    public void Configure(GameObject zoneRoot, GameObject lotoZoneRoot, HazardInspectionManager manager, LotoS2EnvironmentBuilder builder)
    {
        s2ZoneRoot = zoneRoot;
        s3ZoneRoot = lotoZoneRoot;
        inspectionManager = manager;
        environmentBuilder = builder;
    }

    public void Begin()
    {
        if (hasBegun)
        {
            return;
        }

        hasBegun = true;
        EnsureEnvironment();

        if (s3ZoneRoot != null)
        {
            s3ZoneRoot.SetActive(false);
        }

        if (s2ZoneRoot != null)
        {
            s2ZoneRoot.SetActive(true);
        }

        if (inspectionManager != null)
        {
            inspectionManager.Begin();
        }
    }

    public void OnInspectionCompleted()
    {
        if (ControlCenter.Instance == null || ControlCenter.Instance.simulationOrchestratorManager == null)
        {
            return;
        }

        if (s2ZoneRoot != null)
        {
            s2ZoneRoot.SetActive(false);
        }

        ControlCenter.Instance.simulationOrchestratorManager.OnScenario2Completed();
    }

    #endregion

    #region Environment

    private void EnsureEnvironment()
    {
        if (environmentBuilder == null)
        {
            environmentBuilder = GetComponentInChildren<LotoS2EnvironmentBuilder>(true);
        }

        if (environmentBuilder == null && buildEnvironmentOnBegin)
        {
            GameObject builderObject = new GameObject("LotoS2EnvironmentBuilder");
            if (s2ZoneRoot != null)
            {
                builderObject.transform.SetParent(s2ZoneRoot.transform, false);
            }
            else
            {
                builderObject.transform.SetParent(transform, false);
            }

            environmentBuilder = builderObject.AddComponent<LotoS2EnvironmentBuilder>();
        }

        if (environmentBuilder != null && inspectionManager != null)
        {
            environmentBuilder.Build(inspectionManager);
        }
    }

    #endregion
}
