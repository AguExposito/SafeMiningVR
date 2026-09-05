#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public static class LotoNestedPrefabRebuild
{
    [MenuItem("SafeMining/LOTO/Rebuild Nested Scenario Prefabs")]
    public static void RebuildAll()
    {
        LotoVisualPrefabLibrary.EnsureLibrary();
        LotoS1LockerPrefabBuilder.Build();
        NestS3ConveyorStation();
        LotoS2InspectionYardPrefabBuilder.Build();
        LotoFase2PrefabSetupTool.EnsureS2YardWorldLabels();
        ApplyBuiltPrefabsToScenes();
        Debug.Log("[LOTO] Nested scenario prefabs rebuilt.");
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
        ReplaceNamed(root, "BeltVisual", LotoVisualPrefabLibrary.BeltVisual);
        ReplaceNamed(root, "PanelBody", LotoVisualPrefabLibrary.PanelBody);
        ReplaceNamed(root, "HaspBody", LotoVisualPrefabLibrary.Hasp);
        ReplaceNamed(root, "Hasp", LotoVisualPrefabLibrary.Hasp);

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
            XRSocketInteractor lockSocket = ResolveHaspLockSocket(hasp);
            SerializedObject sequenceSo = new SerializedObject(sequenceManager);
            sequenceSo.FindProperty("beltAnimator").objectReferenceValue = beltAnimator;
            sequenceSo.FindProperty("lockSnapAnchor").objectReferenceValue = hasp;
            sequenceSo.FindProperty("tagSnapAnchor").objectReferenceValue = tagAnchor;
            sequenceSo.FindProperty("lockSocket").objectReferenceValue = lockSocket;
            sequenceSo.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(sequenceManager);
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

    private static XRSocketInteractor ResolveHaspLockSocket(Transform hasp)
    {
        if (hasp == null)
        {
            return null;
        }

        XRSocketInteractor socket = hasp.GetComponent<XRSocketInteractor>();
        if (socket != null)
        {
            return socket;
        }

        Transform haspBody = hasp.Find("HaspBody");
        if (haspBody == null)
        {
            haspBody = FindDeep(hasp, "HaspBody");
        }

        if (haspBody == null)
        {
            return null;
        }

        return haspBody.GetComponent<XRSocketInteractor>();
    }

    private static void ApplyBuiltPrefabsToScenes()
    {
        ReplaceNamedPrefabInstance(
            "Assets/SafeMining/LOTOSimulation/Scenes/VR-LOTO-S2.unity",
            "PREF_S2_InspectionYard",
            LotoS2InspectionYardPrefabBuilder.PrefabPath,
            RebindS2Scene);
        ReplaceNamedPrefabInstance(
            "Assets/SafeMining/LOTOSimulation/Scenes/VR-LOTO-S1.unity",
            "PREF_S1_Locker",
            LotoS1LockerPrefabBuilder.PrefabPath,
            RebindS1Scene);
    }

    private static void ReplaceNamedPrefabInstance(
        string scenePath,
        string instanceName,
        string prefabPath,
        System.Action<GameObject> rebind)
    {
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        GameObject existing = FindSceneObject(instanceName);
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (existing == null || prefab == null)
        {
            return;
        }

        Transform parent = existing.transform.parent;
        Vector3 localPosition = existing.transform.localPosition;
        Quaternion localRotation = existing.transform.localRotation;
        Vector3 localScale = existing.transform.localScale;
        int sibling = existing.transform.GetSiblingIndex();
        Object.DestroyImmediate(existing);

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.name = instanceName;
        instance.transform.SetParent(parent, false);
        instance.transform.localPosition = localPosition;
        instance.transform.localRotation = localRotation;
        instance.transform.localScale = localScale;
        instance.transform.SetSiblingIndex(sibling);
        if (rebind != null)
        {
            rebind(instance);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void RebindS2Scene(GameObject yard)
    {
        GameObject root = FindSceneObject("LOTO_S2Root");
        if (root == null || yard == null)
        {
            return;
        }

        HazardInspectionManager inspectionManager = yard.GetComponent<HazardInspectionManager>();
        LotoConveyorInspectionMode inspectionMode = yard.GetComponentInChildren<LotoConveyorInspectionMode>(true);
        LotoSequenceManager sequenceManager = yard.GetComponentInChildren<LotoSequenceManager>(true);
        Transform conveyor = FindDeep(yard.transform, "PREF_S3_ConveyorStation");

        LotoScenario2Controller scenario2 = root.GetComponent<LotoScenario2Controller>();
        if (scenario2 != null)
        {
            SerializedObject scenario2So = new SerializedObject(scenario2);
            scenario2So.FindProperty("s2ZoneRoot").objectReferenceValue = yard;
            scenario2So.FindProperty("inspectionManager").objectReferenceValue = inspectionManager;
            scenario2So.FindProperty("conveyorInspectionMode").objectReferenceValue = inspectionMode;
            scenario2So.ApplyModifiedPropertiesWithoutUndo();
        }

        LotoScenario3Controller scenario3 = root.GetComponent<LotoScenario3Controller>();
        if (scenario3 != null)
        {
            SerializedObject scenario3So = new SerializedObject(scenario3);
            scenario3So.FindProperty("s3ZoneRoot").objectReferenceValue = conveyor != null ? conveyor.gameObject : null;
            scenario3So.FindProperty("sequenceManager").objectReferenceValue = sequenceManager;
            scenario3So.FindProperty("conveyorInspectionMode").objectReferenceValue = inspectionMode;
            scenario3So.ApplyModifiedPropertiesWithoutUndo();
        }

        if (inspectionManager != null && scenario2 != null)
        {
            SerializedObject inspectionSo = new SerializedObject(inspectionManager);
            inspectionSo.FindProperty("scenarioController").objectReferenceValue = scenario2;
            inspectionSo.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void RebindS1Scene(GameObject locker)
    {
        GameObject root = FindSceneObject("LOTO_S1Root");
        if (root == null || locker == null)
        {
            return;
        }

        LotoScenario1Controller scenario1 = root.GetComponent<LotoScenario1Controller>();
        if (scenario1 == null)
        {
            return;
        }

        SerializedObject scenario1So = new SerializedObject(scenario1);
        scenario1So.FindProperty("s1ZoneRoot").objectReferenceValue = locker;
        scenario1So.FindProperty("equipmentManager").objectReferenceValue = locker.GetComponent<EppEquipmentManager>();
        scenario1So.ApplyModifiedPropertiesWithoutUndo();
    }

    private static GameObject FindSceneObject(string objectName)
    {
        Transform[] transforms = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < transforms.Length; i++)
        {
            if (transforms[i] != null && transforms[i].name == objectName)
            {
                return transforms[i].gameObject;
            }
        }

        return null;
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
        if (parent.name == name || parent.name.StartsWith(name + " (Missing Prefab"))
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
