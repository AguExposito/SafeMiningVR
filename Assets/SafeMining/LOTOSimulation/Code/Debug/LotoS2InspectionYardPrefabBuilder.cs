#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

public static class LotoS2InspectionYardPrefabBuilder
{
    public const string PrefabPath = "Assets/SafeMining/LOTOSimulation/Prefabs/PREF_S2_InspectionYard.prefab";
    public const string ConveyorPrefabPath = "Assets/SafeMining/LOTOSimulation/Prefabs/PREF_S3_ConveyorStation.prefab";

    private static readonly Color Floor = new Color(0.30f, 0.31f, 0.33f);
    private static readonly Color Spill = new Color(0.18f, 0.16f, 0.12f);
    private static readonly Color Tool = new Color(0.45f, 0.42f, 0.38f);
    private static readonly Color Sign = new Color(0.82f, 0.68f, 0.18f);
    private static readonly Color Extinguisher = new Color(0.72f, 0.18f, 0.16f);
    private static readonly Color Box = new Color(0.48f, 0.36f, 0.22f);
    private static readonly Color Cone = new Color(0.90f, 0.45f, 0.10f);
    private static readonly Color Safe = new Color(0.40f, 0.42f, 0.44f);
    private static readonly Color Exit = new Color(0.976f, 0.451f, 0.086f);
    private static readonly Color Roller = new Color(0.12f, 0.12f, 0.14f);
    private static readonly Color Rack = new Color(0.38f, 0.39f, 0.41f);

    public static GameObject Build()
    {
        LotoVisualPrefabLibrary.EnsureLibrary();
        Shader lit = Shader.Find("Universal Render Pipeline/Lit");
        if (lit == null)
        {
            lit = Shader.Find("Standard");
        }

        GameObject root = new GameObject("PREF_S2_InspectionYard");
        HazardInspectionManager manager = root.AddComponent<HazardInspectionManager>();

        GameObject floor = CreateCube("YardFloor", root.transform, new Vector3(8f, 0.04f, 7f), Floor, lit);
        floor.transform.localPosition = new Vector3(0f, 0.02f, 1.2f);
        ConfigureTeleport(floor);

        GameObject conveyorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ConveyorPrefabPath);
        GameObject conveyor = null;
        if (conveyorPrefab != null)
        {
            conveyor = (GameObject)PrefabUtility.InstantiatePrefab(conveyorPrefab);
            conveyor.name = "PREF_S3_ConveyorStation";
            conveyor.transform.SetParent(root.transform, false);
            conveyor.transform.localPosition = new Vector3(0f, 0f, 2.4f);
            conveyor.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
            LotoConveyorInspectionMode inspectionMode = conveyor.AddComponent<LotoConveyorInspectionMode>();
            SerializedObject modeSo = new SerializedObject(inspectionMode);
            modeSo.FindProperty("beltAnimator").objectReferenceValue = conveyor.GetComponentInChildren<BeltAnimator>(true);
            modeSo.ApplyModifiedPropertiesWithoutUndo();
            AddAnomalousRoller(conveyor.transform, lit);
        }

        CreateSpill(root.transform, manager, lit);
        CreateTools(root.transform, manager, lit);
        CreateFallenSign(root.transform, manager, lit);
        CreateObstructedExtinguisher(root.transform, manager, lit);
        CreateDecoyCleanFloor(root.transform, manager, lit);
        CreateDecoyStoredTools(root.transform, manager, lit);
        CreateDecoyUprightSign(root.transform, manager, lit);

        GameObject exitDoor = CreateCube("ExitDoor", root.transform, new Vector3(0.7f, 1.35f, 0.08f), Exit, lit);
        exitDoor.transform.localPosition = new Vector3(0f, 0.72f, -1.85f);
        XRSimpleInteractable exitInteractable = exitDoor.AddComponent<XRSimpleInteractable>();
        exitInteractable.interactionLayers = new InteractionLayerMask { value = -1 };
        LotoS2ContinueInteractable continueLogic = exitDoor.AddComponent<LotoS2ContinueInteractable>();
        continueLogic.Configure(manager);
        CreateWorldLabel(exitDoor.transform, "Label", new Vector3(0f, 0.78f, -0.06f), "loto_s2_exit");

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        UnityEngine.Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        return prefab;
    }

    public static void RepairMissingMaterials()
    {
        Shader lit = Shader.Find("Universal Render Pipeline/Lit");
        if (lit == null)
        {
            lit = Shader.Find("Standard");
        }

        GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);
        MeshRenderer[] renderers = root.GetComponentsInChildren<MeshRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            MeshRenderer renderer = renderers[i];
            if (renderer == null || renderer.sharedMaterial != null)
            {
                continue;
            }

            string parentName = renderer.transform.parent != null
                ? renderer.transform.parent.gameObject.name
                : renderer.gameObject.name;
            string key = parentName + "_" + renderer.gameObject.name;
            renderer.sharedMaterial = GetOrCreateMaterial(key, InferColor(parentName, renderer.gameObject.name), lit);
        }

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        PrefabUtility.UnloadPrefabContents(root);
        AssetDatabase.SaveAssets();
    }

    private static void AddAnomalousRoller(Transform conveyorRoot, Shader lit)
    {
        Transform belt = FindDeep(conveyorRoot, "BeltVisual");
        Transform parent = belt != null ? belt.parent : conveyorRoot;
        GameObject roller = LotoVisualPrefabLibrary.Instantiate(
            LotoVisualPrefabLibrary.Roller,
            parent,
            "AnomalousRoller",
            belt != null ? belt.localPosition + new Vector3(0.55f, -0.12f, 0.28f) : new Vector3(0.55f, 0.35f, 0.3f),
            new Vector3(0.18f, 0.22f, 0.18f));
        roller.transform.localRotation = Quaternion.Euler(0f, 0f, 18f);
    }

    private static void CreateSpill(Transform parent, HazardInspectionManager manager, Shader lit)
    {
        GameObject spot = CreateSpotRoot("Hazard_Spill", parent, new Vector3(-1.9f, 0.06f, 0.35f), new Vector3(1.1f, 0.05f, 0.7f), Spill, lit, HazardType.SpilledMaterial, HazardSpotKind.RealHazard, manager);
        GameObject puddle = spot.transform.Find("Visual").gameObject;

        GameObject coneA = CreateCone("ConeA", spot.transform, new Vector3(-0.35f, 0.12f, 0.15f), lit);
        coneA.SetActive(false);
        GameObject coneB = CreateCone("ConeB", spot.transform, new Vector3(0.35f, 0.12f, -0.1f), lit);
        coneB.SetActive(false);

        AssignRemediation(spot.GetComponent<ref_HazardSpot>(), new[] { puddle }, new[] { coneA, coneB }, null, null, null);
    }

    private static void CreateTools(Transform parent, HazardInspectionManager manager, Shader lit)
    {
        GameObject spot = CreateSpotRoot("Hazard_Tools", parent, new Vector3(1.55f, 0.08f, 0.55f), new Vector3(0.55f, 0.08f, 0.35f), Tool, lit, HazardType.AbandonedTools, HazardSpotKind.RealHazard, manager);
        GameObject wrench = CreateCube("Wrench", spot.transform, new Vector3(0.55f, 0.7f, 0.18f), Tool, lit);
        wrench.transform.localPosition = new Vector3(0.1f, 0.9f, 0f);
        wrench.transform.localRotation = Quaternion.Euler(0f, 0f, 25f);
        UnityEngine.Object.DestroyImmediate(wrench.GetComponent<Collider>());

        GameObject rack = CreateCube("SafeRack", parent, new Vector3(0.5f, 0.7f, 0.18f), Rack, lit);
        rack.transform.localPosition = new Vector3(2.6f, 0.4f, -0.6f);
        Transform rackAnchor = new GameObject("ToolsSafeAnchor").transform;
        rackAnchor.SetParent(rack.transform, false);
        rackAnchor.localPosition = new Vector3(0f, 0.55f, 0f);

        AssignRemediation(spot.GetComponent<ref_HazardSpot>(), new[] { spot.transform.Find("Visual").gameObject }, null, null, wrench.transform, rackAnchor);
    }

    private static void CreateFallenSign(Transform parent, HazardInspectionManager manager, Shader lit)
    {
        GameObject spot = CreateSpotRoot("Hazard_Signage", parent, new Vector3(-0.35f, 0.08f, 1.55f), new Vector3(0.7f, 0.06f, 0.45f), Sign, lit, HazardType.FallenSignage, HazardSpotKind.RealHazard, manager);
        GameObject board = CreateCube("FallenBoard", spot.transform, new Vector3(0.9f, 0.55f, 0.08f), Sign, lit);
        board.transform.localPosition = new Vector3(0f, 0.4f, 0f);
        board.transform.localRotation = Quaternion.Euler(82f, 12f, 0f);
        UnityEngine.Object.DestroyImmediate(board.GetComponent<Collider>());
        CreateWorldLabel(board.transform, "Label", new Vector3(0f, 0f, -0.6f), "loto_s2_report_signage");
        AssignRemediation(spot.GetComponent<ref_HazardSpot>(), null, null, board.transform, null, null);
    }

    private static void CreateObstructedExtinguisher(Transform parent, HazardInspectionManager manager, Shader lit)
    {
        GameObject spot = CreateSpotRoot("Hazard_Extinguisher", parent, new Vector3(2.35f, 0.55f, 1.45f), new Vector3(0.22f, 0.7f, 0.22f), Extinguisher, lit, HazardType.ObstructedExtinguisher, HazardSpotKind.RealHazard, manager);
        GameObject boxA = CreateCube("BlockBoxA", spot.transform, new Vector3(1.1f, 0.7f, 0.9f), Box, lit);
        boxA.transform.localPosition = new Vector3(0f, -0.15f, -0.85f);
        GameObject boxB = CreateCube("BlockBoxB", spot.transform, new Vector3(0.9f, 0.55f, 0.7f), Box, lit);
        boxB.transform.localPosition = new Vector3(0.35f, -0.25f, -1.35f);
        AssignRemediation(spot.GetComponent<ref_HazardSpot>(), new[] { boxA, boxB }, null, null, null, null);
    }

    private static void CreateDecoyCleanFloor(Transform parent, HazardInspectionManager manager, Shader lit)
    {
        CreateSpotRoot("Decoy_CleanFloor", parent, new Vector3(-2.35f, 0.04f, 1.15f), new Vector3(0.7f, 0.03f, 0.7f), Safe, lit, HazardType.DecoyCleanFloor, HazardSpotKind.Decoy, manager);
    }

    private static void CreateDecoyStoredTools(Transform parent, HazardInspectionManager manager, Shader lit)
    {
        GameObject spot = CreateSpotRoot("Decoy_StoredTools", parent, new Vector3(2.15f, 0.55f, -0.95f), new Vector3(0.45f, 0.35f, 0.22f), Safe, lit, HazardType.DecoyStoredTools, HazardSpotKind.Decoy, manager);
        GameObject shelf = CreateCube("Shelf", spot.transform, new Vector3(1.2f, 0.15f, 0.6f), Rack, lit);
        shelf.transform.localPosition = new Vector3(0f, -0.7f, 0f);
        UnityEngine.Object.DestroyImmediate(shelf.GetComponent<Collider>());
    }

    private static void CreateDecoyUprightSign(Transform parent, HazardInspectionManager manager, Shader lit)
    {
        GameObject spot = CreateSpotRoot("Decoy_VisibleSign", parent, new Vector3(-2.15f, 1.15f, 2.15f), new Vector3(0.55f, 0.4f, 0.06f), Safe, lit, HazardType.DecoyVisibleSign, HazardSpotKind.Decoy, manager);
        CreateWorldLabel(spot.transform, "Label", new Vector3(0f, 0f, -0.7f), "loto_s2_false_positive");
    }

    private static GameObject CreateSpotRoot(
        string name,
        Transform parent,
        Vector3 localPos,
        Vector3 visualScale,
        Color color,
        Shader lit,
        HazardType type,
        HazardSpotKind kind,
        HazardInspectionManager manager)
    {
        GameObject spot = new GameObject(name);
        spot.transform.SetParent(parent, false);
        spot.transform.localPosition = localPos;

        BoxCollider collider = spot.AddComponent<BoxCollider>();
        collider.isTrigger = false;
        collider.size = visualScale + new Vector3(0.05f, 0.12f, 0.05f);
        collider.center = new Vector3(0f, visualScale.y * 0.5f, 0f);

        GameObject visual = CreateCube("Visual", spot.transform, visualScale, color, lit);
        visual.transform.localPosition = new Vector3(0f, visualScale.y * 0.5f, 0f);
        UnityEngine.Object.DestroyImmediate(visual.GetComponent<Collider>());

        XRSimpleInteractable interactable = spot.AddComponent<XRSimpleInteractable>();
        interactable.interactionLayers = new InteractionLayerMask { value = -1 };

        ref_HazardSpot spotRef = spot.AddComponent<ref_HazardSpot>();
        spotRef.hazardType = type;
        spotRef.spotKind = kind;
        spotRef.spotRenderer = visual.GetComponent<Renderer>();
        spotRef.interactable = interactable;
        spotRef.CacheOriginalColor();

        HazardReportInteractable logic = spot.AddComponent<HazardReportInteractable>();
        logic.Configure(manager);
        return spot;
    }

    private static void AssignRemediation(
        ref_HazardSpot spot,
        GameObject[] hide,
        GameObject[] show,
        Transform upright,
        Transform move,
        Transform moveTarget)
    {
        SerializedObject so = new SerializedObject(spot);
        AssignObjectArray(so, "objectsToHide", hide);
        AssignObjectArray(so, "objectsToShow", show);
        so.FindProperty("objectToUpright").objectReferenceValue = upright;
        so.FindProperty("objectToMove").objectReferenceValue = move;
        so.FindProperty("moveTarget").objectReferenceValue = moveTarget;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void AssignObjectArray(SerializedObject so, string propertyName, GameObject[] values)
    {
        SerializedProperty property = so.FindProperty(propertyName);
        if (values == null)
        {
            property.arraySize = 0;
            return;
        }

        property.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
        {
            property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
    }

    private static GameObject CreateCone(string name, Transform parent, Vector3 localPos, Shader lit)
    {
        GameObject cone = LotoVisualPrefabLibrary.Instantiate(
            LotoVisualPrefabLibrary.Cone,
            parent,
            name,
            localPos,
            new Vector3(0.18f, 0.16f, 0.18f));
        Collider collider = cone.GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        return cone;
    }

    private static GameObject CreateCube(string name, Transform parent, Vector3 scale, Color color, Shader lit)
    {
        return LotoVisualPrefabLibrary.Instantiate(ResolvePlaceholderPath(name), parent, name, Vector3.zero, scale);
    }

    private static string ResolvePlaceholderPath(string name)
    {
        if (name.Contains("Floor") || name.Contains("Yard"))
        {
            return LotoVisualPrefabLibrary.YardFloor;
        }

        if (name.Contains("Cone"))
        {
            return LotoVisualPrefabLibrary.Cone;
        }

        if (name.Contains("Wrench") || name.Contains("Tool"))
        {
            return LotoVisualPrefabLibrary.Wrench;
        }

        if (name.Contains("Board") || name.Contains("Sign"))
        {
            return LotoVisualPrefabLibrary.FallenBoard;
        }

        if (name.Contains("Box") || name.Contains("Block"))
        {
            return LotoVisualPrefabLibrary.Crate;
        }

        if (name.Contains("Extinguish"))
        {
            return LotoVisualPrefabLibrary.Extinguisher;
        }

        if (name.Contains("Rack") || name.Contains("Shelf"))
        {
            return LotoVisualPrefabLibrary.Rack;
        }

        if (name.Contains("Door") || name.Contains("Exit"))
        {
            return LotoVisualPrefabLibrary.ExitDoor;
        }

        if (name.Contains("Spill") || name == "Visual")
        {
            return LotoVisualPrefabLibrary.Spill;
        }

        return LotoVisualPrefabLibrary.Cube;
    }

    private static void ApplyColor(GameObject target, Color color, Shader lit)
    {
        MeshRenderer renderer = target.GetComponent<MeshRenderer>();
        if (renderer == null || lit == null)
        {
            return;
        }

        string materialKey = target.transform.parent != null
            ? target.transform.parent.gameObject.name + "_" + target.name
            : target.name;
        renderer.sharedMaterial = GetOrCreateMaterial(materialKey, color, lit);
    }

    private static Color InferColor(string parentName, string objectName)
    {
        if (objectName == "YardFloor" || parentName == "YardFloor")
        {
            return Floor;
        }

        if (objectName == "AnomalousRoller")
        {
            return Roller;
        }

        if (objectName == "ExitDoor")
        {
            return Exit;
        }

        if (objectName == "ConeA" || objectName == "ConeB")
        {
            return Cone;
        }

        if (objectName == "BlockBoxA" || objectName == "BlockBoxB")
        {
            return Box;
        }

        if (objectName == "Wrench")
        {
            return Tool;
        }

        if (objectName == "FallenBoard")
        {
            return Sign;
        }

        if (objectName == "SafeRack" || objectName == "Shelf")
        {
            return Rack;
        }

        if (parentName.Contains("Spill"))
        {
            return Spill;
        }

        if (parentName.Contains("Tools") && !parentName.Contains("Stored"))
        {
            return Tool;
        }

        if (parentName.Contains("Signage") || parentName.Contains("Sign"))
        {
            return Sign;
        }

        if (parentName.Contains("Extinguisher"))
        {
            return Extinguisher;
        }

        return Safe;
    }

    private static Material GetOrCreateMaterial(string objectName, Color color, Shader lit)
    {
        string folder = "Assets/SafeMining/LOTOSimulation/Art/Materials";
        if (!AssetDatabase.IsValidFolder("Assets/SafeMining/LOTOSimulation/Art"))
        {
            AssetDatabase.CreateFolder("Assets/SafeMining/LOTOSimulation", "Art");
        }

        if (!AssetDatabase.IsValidFolder(folder))
        {
            AssetDatabase.CreateFolder("Assets/SafeMining/LOTOSimulation/Art", "Materials");
        }

        string materialName = "MAT_S2_" + objectName;
        string path = folder + "/" + materialName + ".mat";
        Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing == null)
        {
            existing = new Material(lit);
            existing.color = color;
            AssetDatabase.CreateAsset(existing, path);
            return existing;
        }

        existing.shader = lit;
        existing.color = color;
        EditorUtility.SetDirty(existing);
        return existing;
    }

    private static void ConfigureTeleport(GameObject floor)
    {
        TeleportationArea teleportArea = floor.GetComponent<TeleportationArea>();
        if (teleportArea == null)
        {
            teleportArea = floor.AddComponent<TeleportationArea>();
        }

        teleportArea.interactionLayers = new InteractionLayerMask
        {
            value = LotoSimulationConstants.XriTeleportInteractionLayerMask
        };
        teleportArea.matchOrientation = MatchOrientation.None;
    }

    private static void CreateWorldLabel(Transform parent, string name, Vector3 localPos, string locKey)
    {
        GameObject labelObject = new GameObject(name);
        labelObject.transform.SetParent(parent, false);
        labelObject.transform.localPosition = localPos;
        labelObject.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        TextMeshPro tmp = labelObject.AddComponent<TextMeshPro>();
        tmp.text = locKey;
        tmp.fontSize = 1.4f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.rectTransform.sizeDelta = new Vector2(0.8f, 0.2f);
        LotoWorldLabel worldLabel = labelObject.AddComponent<LotoWorldLabel>();
        worldLabel.Configure(tmp, locKey);
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
