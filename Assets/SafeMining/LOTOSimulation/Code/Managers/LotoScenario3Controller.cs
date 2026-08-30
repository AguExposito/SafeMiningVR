using UnityEngine;

public class LotoScenario3Controller : MonoBehaviour
{
    [SerializeField] private GameObject s3ZoneRoot;
    [SerializeField] private LotoSequenceManager sequenceManager;

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
        if (hasBegun)
        {
            return;
        }

        hasBegun = true;

        if (s3ZoneRoot != null)
        {
            s3ZoneRoot.SetActive(true);
        }

        if (sequenceManager == null)
        {
            Debug.LogWarning("[LOTO] LotoSequenceManager is not assigned on LotoScenario3Controller.");
            return;
        }

        sequenceManager.OnSequenceCompleted += OnSequenceCompleted;
        sequenceManager.Begin();
    }

    public void OnSequenceCompleted(bool success)
    {
        if (ControlCenter.Instance == null || ControlCenter.Instance.GetManager<SimulationOrchestratorManager>() == null)
        {
            return;
        }

        ControlCenter.Instance.GetManager<SimulationOrchestratorManager>().ShowSessionSummary();
    }

    #endregion
}
