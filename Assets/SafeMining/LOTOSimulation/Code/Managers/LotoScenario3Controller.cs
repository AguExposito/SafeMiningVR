using UnityEngine;

public class LotoScenario3Controller : MonoBehaviour
{
    [SerializeField] private GameObject s3ZoneRoot;
    [SerializeField] private LotoSequenceManager sequenceManager;
    [SerializeField] private LotoConveyorInspectionMode conveyorInspectionMode;

    private bool hasBegun;

    #region Lifecycle

    private void OnDestroy()
    {
        if (sequenceManager != null)
        {
            sequenceManager.OnSequenceCompleted -= OnSequenceCompleted;
        }
    }

    #endregion

    #region Public API

    public void Configure(GameObject zoneRoot, LotoSequenceManager manager)
    {
        s3ZoneRoot = zoneRoot;
        sequenceManager = manager;
    }

    public void Begin()
    {
        if (s3ZoneRoot != null)
        {
            s3ZoneRoot.SetActive(true);
        }

        Transform searchRoot = s3ZoneRoot != null ? s3ZoneRoot.transform : transform;

        if (conveyorInspectionMode == null)
        {
            conveyorInspectionMode = searchRoot.GetComponentInChildren<LotoConveyorInspectionMode>(true);
        }

        if (sequenceManager == null)
        {
            sequenceManager = searchRoot.GetComponentInChildren<LotoSequenceManager>(true);
        }

        if (sequenceManager == null)
        {
            sequenceManager = FindFirstObjectByType<LotoSequenceManager>(FindObjectsInactive.Include);
        }

        if (sequenceManager == null)
        {
            Debug.LogWarning("[LOTO] LotoSequenceManager is not assigned on LotoScenario3Controller.");
            return;
        }

        sequenceManager.OnSequenceCompleted -= OnSequenceCompleted;
        sequenceManager.OnSequenceCompleted += OnSequenceCompleted;

        if (hasBegun)
        {
            return;
        }

        hasBegun = true;

        if (conveyorInspectionMode != null)
        {
            conveyorInspectionMode.Revert();
        }

        sequenceManager.Begin();
    }

    public void OnSequenceCompleted(bool success)
    {
        if (!success)
        {
            return;
        }

        Util_LotoSimulationBridge.NotifySessionSummary();
    }

    #endregion
}
