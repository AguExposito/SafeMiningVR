#if UNITY_EDITOR
using UnityEngine;

public class LotoS1EppSmokeTest : MonoBehaviour
{
    [ContextMenu("LOTO/S1 Smoke Test - Happy Path")]
    public void RunHappyPathSmokeTest()
    {
        NotifyGood(EppItemType.Helmet);
        NotifyGood(EppItemType.SafetyGlasses);
        NotifyGood(EppItemType.ReflectiveVest);
        NotifyGood(EppItemType.Gloves);
        NotifyGood(EppItemType.SafetyBoots);

        LotoGameManager board = LotoGameManager.Instance;
        if (board == null || !board.IsS1Complete)
        {
            Debug.LogError("[LOTO-S1-SMOKE] Board did not complete S1.");
            return;
        }

        board.TryContinueS1();
        Debug.Log("[LOTO-S1-SMOKE] Happy path completed without exceptions.");
    }

    [ContextMenu("LOTO/S1 Smoke Test - Reject Paths")]
    public void RunRejectPathsSmokeTest()
    {
        LotoGameManager board = LotoGameManager.Instance;
        if (board == null)
        {
            Debug.LogError("[LOTO-S1-SMOKE] LotoGameManager not found.");
            return;
        }

        board.NotifyEppGrabbed(EppItemType.Helmet, EppItemCondition.Defective);
        board.TryContinueS1();
        NotifyGood(EppItemType.Helmet);
        NotifyGood(EppItemType.SafetyGlasses);
        NotifyGood(EppItemType.ReflectiveVest);
        NotifyGood(EppItemType.Gloves);
        NotifyGood(EppItemType.SafetyBoots);
        board.TryContinueS1();
        Debug.Log("[LOTO-S1-SMOKE] Reject paths invoked without exceptions.");
    }

    private static void NotifyGood(EppItemType type)
    {
        LotoGameManager board = LotoGameManager.Instance;
        if (board == null)
        {
            return;
        }

        board.NotifyEppGrabbed(type, EppItemCondition.Good);
    }
}
#endif
