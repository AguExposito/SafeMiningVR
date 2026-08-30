using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public static class LotoFase3PrefabSetupTool
{
    private static readonly string[] PrefabPaths =
    {
        "Assets/SafeMining/LOTOSimulation/Prefabs/PREF_S1_Locker.prefab",
        "Assets/SafeMining/LOTOSimulation/Prefabs/PREF_S2_InspectionYard.prefab",
        "Assets/SafeMining/LOTOSimulation/Prefabs/PREF_S3_ConveyorStation.prefab"
    };

    [MenuItem("SafeMining/LOTO/Apply Fase 3 Dwell Reticle Setup")]
    public static void ApplyFase3Setup()
    {
        int addedCount = 0;
        int skippedCount = 0;

        for (int i = 0; i < PrefabPaths.Length; i++)
        {
            string prefabPath = PrefabPaths[i];
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            int prefabAdded = 0;
            int prefabSkipped = 0;
            ProcessPrefabRoot(root, ref prefabAdded, ref prefabSkipped);
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            PrefabUtility.UnloadPrefabContents(root);
            addedCount += prefabAdded;
            skippedCount += prefabSkipped;
            Debug.Log("[LOTO Fase3] " + prefabPath + " added=" + prefabAdded + " skipped=" + prefabSkipped);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[LOTO Fase3] Dwell reticle setup complete. added=" + addedCount + " skipped=" + skippedCount);
    }

    private static void ProcessPrefabRoot(GameObject root, ref int addedCount, ref int skippedCount)
    {
        XRSimpleInteractable[] interactables = root.GetComponentsInChildren<XRSimpleInteractable>(true);
        for (int i = 0; i < interactables.Length; i++)
        {
            XRSimpleInteractable interactable = interactables[i];
            if (interactable == null)
            {
                continue;
            }

            GameObject targetObject = interactable.gameObject;
            if (!IsDwellTriggerTarget(targetObject))
            {
                skippedCount++;
                continue;
            }

            if (targetObject.GetComponent<LotoDwellReticle>() != null)
            {
                skippedCount++;
                continue;
            }

            targetObject.AddComponent<LotoDwellReticle>();
            addedCount++;
        }
    }

    private static bool IsDwellTriggerTarget(GameObject targetObject)
    {
        if (targetObject.GetComponent<XRGrabInteractable>() != null)
        {
            return false;
        }

        if (targetObject.GetComponent<EppItemInteractable>() != null)
        {
            return false;
        }

        if (targetObject.GetComponent<LotoLockInteractable>() != null)
        {
            return false;
        }

        if (targetObject.GetComponent<LotoTagInteractable>() != null)
        {
            return false;
        }

        return targetObject.GetComponent<HazardReportInteractable>() != null
            || targetObject.GetComponent<LotoControlButtonInteractable>() != null
            || targetObject.GetComponent<LotoBreakerInteractable>() != null
            || targetObject.GetComponent<LotoPanelDoorInteractable>() != null
            || targetObject.GetComponent<LotoS1ContinueInteractable>() != null
            || targetObject.GetComponent<LotoS2ContinueInteractable>() != null;
    }
}
