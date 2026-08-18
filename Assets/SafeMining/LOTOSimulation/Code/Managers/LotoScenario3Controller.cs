using UnityEngine;

public class LotoScenario3Controller : MonoBehaviour
{
    [SerializeField] private GameObject s3ZoneRoot;
    [SerializeField] private LotoSequenceManager sequenceManager;
    [SerializeField] private LotoS3EnvironmentBuilder environmentBuilder;
    [SerializeField] private bool buildEnvironmentOnBegin = true;

    private bool hasBegun;

    #region Public API

    public void Configure(GameObject zoneRoot, LotoSequenceManager manager, LotoS3EnvironmentBuilder builder)
    {
        s3ZoneRoot = zoneRoot;
        sequenceManager = manager;
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
            s3ZoneRoot.SetActive(true);
        }

        if (sequenceManager != null)
        {
            sequenceManager.Begin();
        }
    }

    public void OnSequenceCompleted(bool success)
    {
        if (ControlCenter.Instance == null || ControlCenter.Instance.simulationOrchestratorManager == null)
        {
            return;
        }

        ControlCenter.Instance.simulationOrchestratorManager.ShowSessionSummary(success);
    }

    #endregion

    #region Environment

    private void EnsureEnvironment()
    {
        if (environmentBuilder == null)
        {
            environmentBuilder = GetComponentInChildren<LotoS3EnvironmentBuilder>(true);
        }

        if (environmentBuilder == null && buildEnvironmentOnBegin)
        {
            GameObject builderObject = new GameObject("LotoS3EnvironmentBuilder");
            if (s3ZoneRoot != null)
            {
                builderObject.transform.SetParent(s3ZoneRoot.transform, false);
            }
            else
            {
                builderObject.transform.SetParent(transform, false);
            }

            environmentBuilder = builderObject.AddComponent<LotoS3EnvironmentBuilder>();
        }

        if (environmentBuilder != null && sequenceManager != null)
        {
            environmentBuilder.Build(sequenceManager);
            if (s3ZoneRoot == null)
            {
                s3ZoneRoot = environmentBuilder.gameObject;
            }
        }
    }

    #endregion
}
