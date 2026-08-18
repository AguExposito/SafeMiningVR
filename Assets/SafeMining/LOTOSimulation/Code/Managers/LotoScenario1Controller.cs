using UnityEngine;

public class LotoScenario1Controller : MonoBehaviour
{
    [SerializeField] private GameObject s1ZoneRoot;
    [SerializeField] private GameObject s3ZoneRoot;
    [SerializeField] private EppEquipmentManager equipmentManager;
    [SerializeField] private LotoS1EnvironmentBuilder environmentBuilder;
    [SerializeField] private bool buildEnvironmentOnBegin = true;

    private bool hasBegun;

    #region Public API

    public void Configure(GameObject zoneRoot, GameObject lotoZoneRoot, EppEquipmentManager manager, LotoS1EnvironmentBuilder builder)
    {
        s1ZoneRoot = zoneRoot;
        s3ZoneRoot = lotoZoneRoot;
        equipmentManager = manager;
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

        if (s1ZoneRoot != null)
        {
            s1ZoneRoot.SetActive(true);
        }

        if (equipmentManager != null)
        {
            equipmentManager.Begin();
        }
    }

    public void OnEquipmentCompleted()
    {
        if (ControlCenter.Instance == null || ControlCenter.Instance.simulationOrchestratorManager == null)
        {
            return;
        }

        if (s1ZoneRoot != null)
        {
            s1ZoneRoot.SetActive(false);
        }

        ControlCenter.Instance.simulationOrchestratorManager.OnScenario1Completed();
    }

    #endregion

    #region Environment

    private void EnsureEnvironment()
    {
        if (environmentBuilder == null)
        {
            environmentBuilder = GetComponentInChildren<LotoS1EnvironmentBuilder>(true);
        }

        if (environmentBuilder == null && buildEnvironmentOnBegin)
        {
            GameObject builderObject = new GameObject("LotoS1EnvironmentBuilder");
            if (s1ZoneRoot != null)
            {
                builderObject.transform.SetParent(s1ZoneRoot.transform, false);
            }
            else
            {
                builderObject.transform.SetParent(transform, false);
            }

            environmentBuilder = builderObject.AddComponent<LotoS1EnvironmentBuilder>();
        }

        if (environmentBuilder != null && equipmentManager != null)
        {
            environmentBuilder.Build(equipmentManager);
        }
    }

    #endregion
}
