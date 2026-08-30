#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public static class LotoNestedPrefabRebuild
{
    private const string DeprecatedMessage =
        "Scenario prefab rebuild from placeholders is retired.\n\n" +
        "Edit PREF_S1_Locker, PREF_S2_InspectionYard, and PREF_S3_ConveyorStation directly.\n\n" +
        "This menu now only re-nests S3 interactable prefabs (locks, tags, buttons, breaker).";

    [MenuItem("SafeMining/LOTO/Rebuild Nested Scenario Prefabs", false, 2000)]
    public static void RebuildAll()
    {
        if (!EditorUtility.DisplayDialog("Deprecated rebuild", DeprecatedMessage, "Continue S3 interactables only", "Cancel"))
        {
            return;
        }

        LotoVisualPrefabLibrary.EnsureLibrary();
        NestS3ConveyorStation();
        Debug.Log("[LOTO] S3 interactable nest complete. S1/S2 scenario rebuild skipped (use final prefabs).");
    }

    public static void NestS3ConveyorStation()
    {
        string path = "Assets/SafeMining/LOTOSimulation/Prefabs/PREF_S3_ConveyorStation.prefab";
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        SplitEmbeddedMeshes(root);
        ReplaceNamed(root, "Lock_Red", LotoVisualPrefabLibrary.LockPersonal);
        ReplaceNamed(root, "Lock_Yellow", LotoVisualPrefabLibrary.LockGroup);
        ReplaceNamed(root, "Lock_Blue", LotoVisualPrefabLibrary.LockVisitor);
        ReplaceNamed(root, "Tag_Danger", LotoVisualPrefabLibrary.TagDanger);
        ReplaceNamed(root, "Tag_Inspected", LotoVisualPrefabLibrary.TagDecoy);
        ReplaceNamed(root, "Tag_Operational", LotoVisualPrefabLibrary.TagOperational);
        ReplaceNamed(root, "Tag_Maintenance", LotoVisualPrefabLibrary.TagMaintenance);
        ReplaceNamed(root, "StopButton", LotoVisualPrefabLibrary.ButtonStop);
        ReplaceNamed(root, "StartButton", LotoVisualPrefabLibrary.ButtonStart);
        ReplaceNamed(root, "Breaker", LotoVisualPrefabLibrary.Breaker);

        LotoSequenceManager sequenceManager = root.GetComponent<LotoSequenceManager>();
        if (sequenceManager != null)
        {
            BeltAnimator beltAnimator = root.GetComponentInChildren<BeltAnimator>(true);
            Transform hasp = FindDeep(root.transform, "Hasp");
            if (hasp == null)
            {
                hasp = FindDeep(root.transform, "HaspBody");
            }

            Transform tagAnchor = FindDeep(root.transform, "TagSocket");
            XRSocketInteractor lockSocket = root.GetComponentInChildren<XRSocketInteractor>(true);
            sequenceManager.Configure(beltAnimator, hasp, tagAnchor, lockSocket);
        }

        PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);
        AssetDatabase.SaveAssets();
    }

    private static void ReplaceNamed(GameObject root, string objectName, string prefabPath)
    {
        Transform current = FindDeep(root.transform, objectName);
        if (current == null)
        {
            return;
        }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            return;
        }

        Transform parent = current.parent;
        Vector3 localPosition = current.localPosition;
        Quaternion localRotation = current.localRotation;
        Vector3 localScale = current.localScale;
        int sibling = current.GetSiblingIndex();
        Object.DestroyImmediate(current.gameObject);

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.name = objectName;
        instance.transform.SetParent(parent, false);
        instance.transform.localPosition = localPosition;
        instance.transform.localRotation = localRotation;
        instance.transform.localScale = localScale;
        instance.transform.SetSiblingIndex(sibling);
    }

    private static void SplitEmbeddedMeshes(GameObject root)
    {
        MeshFilter[] filters = root.GetComponentsInChildren<MeshFilter>(true);
        for (int i = 0; i < filters.Length; i++)
        {
            MeshFilter filter = filters[i];
            if (filter == null || filter.gameObject.name == "Visual")
            {
                continue;
            }

            if (filter.transform.Find("Visual") != null)
            {
                continue;
            }

            if (PrefabUtility.IsPartOfPrefabInstance(filter) && PrefabUtility.IsAnyPrefabInstanceRoot(filter.gameObject) == false)
            {
                continue;
            }

            GameObject host = filter.gameObject;
            MeshRenderer renderer = host.GetComponent<MeshRenderer>();
            if (renderer == null || host.GetComponent<TMPro.TMP_Text>() != null)
            {
                continue;
            }

            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(host.transform, false);
            MeshFilter visualFilter = visual.AddComponent<MeshFilter>();
            visualFilter.sharedMesh = filter.sharedMesh;
            MeshRenderer visualRenderer = visual.AddComponent<MeshRenderer>();
            visualRenderer.sharedMaterials = renderer.sharedMaterials;
            Object.DestroyImmediate(filter);
            Object.DestroyImmediate(renderer);
        }
    }

    private static Transform FindDeep(Transform parent, string name)
    {
        if (parent.name == name)
        {
            return parent;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform found = FindDeep(parent.GetChild(i), name);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }
}
#endif
