#if UNITY_EDITOR
using UnityEngine;

public class LotoS3InteractableSmokeTest : MonoBehaviour
{
    [SerializeField] private LotoSequenceManager sequenceManager;

    [ContextMenu("LOTO/S3 Smoke Test - Happy Path")]
    public void RunHappyPathSmokeTest()
    {
        sequenceManager = ResolveManager();
        if (sequenceManager == null)
        {
            Debug.LogError("[LOTO-S3-SMOKE] LotoSequenceManager not found.");
            return;
        }

        sequenceManager.Begin();
        sequenceManager.TryStopConveyor();
        sequenceManager.TryToggleBreakerOff();
        Debug.Log("[LOTO-S3-SMOKE] Happy path partial complete up to lock step.");
    }

    [ContextMenu("LOTO/S3 Smoke Test - Full Sequence Logic")]
    public void RunFullSequenceLogicTest()
    {
        sequenceManager = ResolveManager();
        if (sequenceManager == null)
        {
            Debug.LogError("[LOTO-S3-SMOKE] LotoSequenceManager not found.");
            return;
        }

        sequenceManager.Begin();
        sequenceManager.TryStopConveyor();
        sequenceManager.TryToggleBreakerOff();

        GameObject lockObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lockObject.name = "SmokeTestLock";
        sequenceManager.TryApplyLock(LotoLockType.Personal, lockObject);

        GameObject tagObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tagObject.name = "SmokeTestTag";
        sequenceManager.TryApplyTag(LotoTagType.DangerDoNotOperate, tagObject);
        sequenceManager.TryStartVerification();

        Destroy(lockObject);
        Destroy(tagObject);
        Debug.Log("[LOTO-S3-SMOKE] Full sequence logic completed without exceptions.");
    }

    [ContextMenu("LOTO/S3 Smoke Test - All Reject Paths")]
    public void RunRejectPathsSmokeTest()
    {
        sequenceManager = ResolveManager();
        if (sequenceManager == null)
        {
            Debug.LogError("[LOTO-S3-SMOKE] LotoSequenceManager not found.");
            return;
        }

        sequenceManager.Begin();
        sequenceManager.TryToggleBreakerOff();
        sequenceManager.TryStartVerification();
        Debug.Log("[LOTO-S3-SMOKE] Reject paths invoked without exceptions.");
    }

    private LotoSequenceManager ResolveManager()
    {
        if (sequenceManager != null)
        {
            EnsureEnvironment(sequenceManager);
            return sequenceManager;
        }

        sequenceManager = FindFirstObjectByType<LotoSequenceManager>();
        EnsureEnvironment(sequenceManager);
        return sequenceManager;
    }

    private static void EnsureEnvironment(LotoSequenceManager manager)
    {
        if (manager == null)
        {
            return;
        }

        LotoS3EnvironmentBuilder builder = manager.GetComponent<LotoS3EnvironmentBuilder>();
        if (builder == null)
        {
            builder = manager.GetComponentInParent<LotoS3EnvironmentBuilder>();
        }

        if (builder != null)
        {
            builder.Build(manager);
        }
    }
}
#endif
