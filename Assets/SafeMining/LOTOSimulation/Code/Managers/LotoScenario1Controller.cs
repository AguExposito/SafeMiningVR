using UnityEngine;

public class LotoScenario1Controller : MonoBehaviour
{
    [SerializeField] private GameObject s1ZoneRoot;
    [SerializeField] private EppEquipmentManager equipmentManager;

    private bool hasBegun;

    #region Public API

    public void Configure(GameObject zoneRoot, GameObject lotoZoneRoot, EppEquipmentManager manager, LotoS1EnvironmentBuilder builder)
    {
        s1ZoneRoot = zoneRoot;
        equipmentManager = manager;
    }

    public void Begin()
    {
        if (hasBegun)
        {
            return;
        }

        hasBegun = true;

        if (s1ZoneRoot != null)
        {
            s1ZoneRoot.SetActive(true);
        }

        if (equipmentManager == null)
        {
            Debug.LogWarning("[LOTO] EppEquipmentManager is not assigned on LotoScenario1Controller.");
            return;
        }

        equipmentManager.Configure(this);
        equipmentManager.Begin();
    }

    public void OnEquipmentCompleted()
    {
        if (ControlCenter.Instance == null || ControlCenter.Instance.simulationOrchestratorManager == null)
        {
            return;
        }

        ControlCenter.Instance.simulationOrchestratorManager.OnScenario1Completed();
    }

    #endregion
}
