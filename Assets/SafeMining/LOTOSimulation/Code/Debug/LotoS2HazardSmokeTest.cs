#if UNITY_EDITOR
using UnityEngine;

public class LotoS2HazardSmokeTest : MonoBehaviour
{
    [SerializeField] private HazardInspectionManager inspectionManager;

    [ContextMenu("LOTO/S2 Smoke Test - Happy Path")]
    public void RunHappyPathSmokeTest()
    {
        inspectionManager = ResolveManager();
        if (inspectionManager == null)
        {
            Debug.LogError("[LOTO-S2-SMOKE] HazardInspectionManager not found.");
            return;
        }

        inspectionManager.Begin();
        Report(HazardType.SpilledMaterial);
        Report(HazardType.AbandonedTools);
        Report(HazardType.FallenSignage);
        Report(HazardType.ObstructedExtinguisher);
        Debug.Log("[LOTO-S2-SMOKE] Happy path completed without exceptions.");
    }

    [ContextMenu("LOTO/S2 Smoke Test - False Positive")]
    public void RunFalsePositiveSmokeTest()
    {
        inspectionManager = ResolveManager();
        if (inspectionManager == null)
        {
            Debug.LogError("[LOTO-S2-SMOKE] HazardInspectionManager not found.");
            return;
        }

        inspectionManager.Begin();
        Report(HazardType.DecoyCleanFloor);
        Report(HazardType.SpilledMaterial);
        Report(HazardType.AbandonedTools);
        Report(HazardType.FallenSignage);
        Report(HazardType.ObstructedExtinguisher);
        Debug.Log("[LOTO-S2-SMOKE] Single false positive path invoked without exceptions.");
    }

    private void Report(HazardType type)
    {
        ref_HazardSpot spot = FindSpot(type);
        if (spot == null)
        {
            return;
        }

        inspectionManager.TryReport(spot);
    }

    private static ref_HazardSpot FindSpot(HazardType type)
    {
        ref_HazardSpot[] spots = FindObjectsByType<ref_HazardSpot>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < spots.Length; i++)
        {
            if (spots[i] != null && spots[i].hazardType == type)
            {
                return spots[i];
            }
        }

        return null;
    }

    private HazardInspectionManager ResolveManager()
    {
        if (inspectionManager != null)
        {
            return inspectionManager;
        }

        return FindFirstObjectByType<HazardInspectionManager>();
    }
}
#endif
