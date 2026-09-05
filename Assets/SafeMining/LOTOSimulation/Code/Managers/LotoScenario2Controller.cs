using UnityEngine;

public class LotoScenario2Controller : MonoBehaviour
{
    [SerializeField] private GameObject s2ZoneRoot;
    [SerializeField] private HazardInspectionManager inspectionManager;
    [SerializeField] private LotoConveyorInspectionMode conveyorInspectionMode;

    private bool hasBegun;

    #region Public API

    public void Configure(
        GameObject zoneRoot,
        GameObject eppZoneRoot,
        GameObject lotoZoneRoot,
        HazardInspectionManager manager)
    {
        s2ZoneRoot = zoneRoot;
        inspectionManager = manager;
    }

    public void Begin()
    {
        if (hasBegun)
        {
            return;
        }

        hasBegun = true;

        if (s2ZoneRoot != null)
        {
            s2ZoneRoot.SetActive(true);
        }

        if (conveyorInspectionMode == null && s2ZoneRoot != null)
        {
            conveyorInspectionMode = s2ZoneRoot.GetComponentInChildren<LotoConveyorInspectionMode>(true);
        }

        if (conveyorInspectionMode != null)
        {
            conveyorInspectionMode.Apply();
        }

        if (inspectionManager == null)
        {
            Debug.LogWarning("[LOTO] HazardInspectionManager is not assigned on LotoScenario2Controller.");
            return;
        }

        inspectionManager.Configure(this);
        inspectionManager.Begin();
    }

    public void OnInspectionCompleted()
    {
        if (inspectionManager != null)
        {
            inspectionManager.LockReporting();
        }

        if (ControlCenter.Instance == null || ControlCenter.Instance.simulationOrchestratorManager == null)
        {
            return;
        }

        ControlCenter.Instance.simulationOrchestratorManager.OnScenario2Completed();
    }

    #endregion
}
