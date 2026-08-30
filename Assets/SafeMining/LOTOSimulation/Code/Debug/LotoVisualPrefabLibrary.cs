#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public static class LotoVisualPrefabLibrary
{
    public const string InteractablesFolder = "Assets/SafeMining/LOTOSimulation/Prefabs/Interactables";
    public const string PlaceholdersFolder = "Assets/SafeMining/LOTOSimulation/Prefabs/Placeholders";
    public const string MaterialsFolder = "Assets/SafeMining/LOTOSimulation/Art/Materials";

    public const string FloorPlate = PlaceholdersFolder + "/PREF_PH_FloorPlate.prefab";
    public const string CabinetPanel = PlaceholdersFolder + "/PREF_PH_CabinetPanel.prefab";
    public const string Shelf = PlaceholdersFolder + "/PREF_PH_Shelf.prefab";
    public const string RejectBin = PlaceholdersFolder + "/PREF_PH_RejectBin.prefab";
    public const string ExitDoor = PlaceholdersFolder + "/PREF_PH_ExitDoor.prefab";
    public const string YardFloor = PlaceholdersFolder + "/PREF_PH_YardFloor.prefab";
    public const string Spill = PlaceholdersFolder + "/PREF_PH_Spill.prefab";
    public const string Wrench = PlaceholdersFolder + "/PREF_PH_Wrench.prefab";
    public const string FallenBoard = PlaceholdersFolder + "/PREF_PH_FallenBoard.prefab";
    public const string Crate = PlaceholdersFolder + "/PREF_PH_Crate.prefab";
    public const string Extinguisher = PlaceholdersFolder + "/PREF_PH_Extinguisher.prefab";
    public const string Cone = PlaceholdersFolder + "/PREF_PH_Cone.prefab";
    public const string Rack = PlaceholdersFolder + "/PREF_PH_Rack.prefab";
    public const string BeltVisual = PlaceholdersFolder + "/PREF_PH_BeltVisual.prefab";
    public const string PanelBody = PlaceholdersFolder + "/PREF_PH_PanelBody.prefab";
    public const string Hasp = PlaceholdersFolder + "/PREF_PH_Hasp.prefab";
    public const string Roller = PlaceholdersFolder + "/PREF_PH_Roller.prefab";
    public const string Pedestal = PlaceholdersFolder + "/PREF_PH_Pedestal.prefab";
    public const string Cube = PlaceholdersFolder + "/PREF_PH_Cube.prefab";

    public const string HelmetGood = InteractablesFolder + "/PREF_EPP_Helmet_Good.prefab";
    public const string HelmetDefect = InteractablesFolder + "/PREF_EPP_Helmet_Defect.prefab";
    public const string GlassesGood = InteractablesFolder + "/PREF_EPP_Glasses_Good.prefab";
    public const string GlassesDefect = InteractablesFolder + "/PREF_EPP_Glasses_Defect.prefab";
    public const string VestGood = InteractablesFolder + "/PREF_EPP_Vest_Good.prefab";
    public const string GlovesGood = InteractablesFolder + "/PREF_EPP_Gloves_Good.prefab";
    public const string GlovesDefect = InteractablesFolder + "/PREF_EPP_Gloves_Defect.prefab";
    public const string BootsGood = InteractablesFolder + "/PREF_EPP_Boots_Good.prefab";
    public const string BootsDefect = InteractablesFolder + "/PREF_EPP_Boots_Defect.prefab";
    public const string RespiratorGood = InteractablesFolder + "/PREF_EPP_Respirator_Good.prefab";
    public const string HearingGood = InteractablesFolder + "/PREF_EPP_Hearing_Good.prefab";

    public const string LockPersonal = InteractablesFolder + "/PREF_LotoLock_Personal.prefab";
    public const string LockVisitor = InteractablesFolder + "/PREF_LotoLock_Visitor.prefab";
    public const string LockGroup = InteractablesFolder + "/PREF_LotoLock_Group.prefab";
    public const string TagDanger = InteractablesFolder + "/PREF_LotoTag_Danger.prefab";
    public const string TagDecoy = InteractablesFolder + "/PREF_LotoTag_Inspected.prefab";
    public const string TagOperational = InteractablesFolder + "/PREF_LotoTag_Operational.prefab";
    public const string TagMaintenance = InteractablesFolder + "/PREF_LotoTag_Maintenance.prefab";
    public const string ButtonStop = InteractablesFolder + "/PREF_Button_Stop.prefab";
    public const string ButtonStart = InteractablesFolder + "/PREF_Button_Start.prefab";
    public const string Breaker = InteractablesFolder + "/PREF_Breaker.prefab";

    [MenuItem("SafeMining/LOTO/Rebuild Visual Prefab Library")]
    public static void RebuildLibraryMenu()
    {
        EnsureLibrary();
        Debug.Log("[LOTO] Visual prefab library rebuilt.");
    }

    public static void EnsureLibrary()
    {
        EnsureFolder("Assets/SafeMining/LOTOSimulation/Prefabs", "Interactables");
        EnsureFolder("Assets/SafeMining/LOTOSimulation", "Art");
        EnsureFolder("Assets/SafeMining/LOTOSimulation/Art", "Materials");

        Shader lit = FindLit();
        SaveEpp(HelmetGood, EppItemType.Helmet, EppItemCondition.Good, new Vector3(0.28f, 0.22f, 0.28f), new Color(0.90f, 0.72f, 0.12f), lit, false);
        SaveEpp(HelmetDefect, EppItemType.Helmet, EppItemCondition.Defective, new Vector3(0.28f, 0.22f, 0.28f), new Color(0.90f, 0.72f, 0.12f), lit, true);
        SaveEpp(GlassesGood, EppItemType.SafetyGlasses, EppItemCondition.Good, new Vector3(0.22f, 0.08f, 0.12f), new Color(0.12f, 0.16f, 0.22f), lit, false);
        SaveEpp(GlassesDefect, EppItemType.SafetyGlasses, EppItemCondition.Defective, new Vector3(0.22f, 0.08f, 0.12f), new Color(0.12f, 0.16f, 0.22f), lit, true);
        SaveEpp(VestGood, EppItemType.ReflectiveVest, EppItemCondition.Good, new Vector3(0.32f, 0.38f, 0.10f), new Color(0.976f, 0.451f, 0.086f), lit, false);
        SaveEpp(GlovesGood, EppItemType.Gloves, EppItemCondition.Good, new Vector3(0.14f, 0.08f, 0.18f), new Color(0.42f, 0.45f, 0.50f), lit, false);
        SaveEpp(GlovesDefect, EppItemType.Gloves, EppItemCondition.Defective, new Vector3(0.14f, 0.08f, 0.18f), new Color(0.42f, 0.45f, 0.50f), lit, true);
        SaveEpp(BootsGood, EppItemType.SafetyBoots, EppItemCondition.Good, new Vector3(0.18f, 0.14f, 0.28f), new Color(0.22f, 0.18f, 0.16f), lit, false);
        SaveEpp(BootsDefect, EppItemType.SafetyBoots, EppItemCondition.Defective, new Vector3(0.18f, 0.14f, 0.28f), new Color(0.22f, 0.18f, 0.16f), lit, true);
        SaveEpp(RespiratorGood, EppItemType.Respirator, EppItemCondition.Good, new Vector3(0.18f, 0.16f, 0.16f), new Color(0.32f, 0.38f, 0.44f), lit, false);
        SaveEpp(HearingGood, EppItemType.HearingProtection, EppItemCondition.Good, new Vector3(0.16f, 0.16f, 0.12f), new Color(0.20f, 0.25f, 0.32f), lit, false);

        SaveLock(LockPersonal, LotoLockType.Personal, new Color(0.85f, 0.15f, 0.15f), lit);
        SaveLock(LockVisitor, LotoLockType.Visitor, new Color(0.15f, 0.35f, 0.85f), lit);
        SaveLock(LockGroup, LotoLockType.Group, new Color(0.90f, 0.80f, 0.15f), lit);
        SaveTag(TagDanger, LotoTagType.DangerDoNotOperate, new Color(0.95f, 0.95f, 0.90f), lit);
        SaveTag(TagDecoy, LotoTagType.Inspected, new Color(0.70f, 0.70f, 0.75f), lit);
        SaveTag(TagOperational, LotoTagType.EquipmentOperational, new Color(0.55f, 0.72f, 0.42f), lit);
        SaveTag(TagMaintenance, LotoTagType.MaintenanceComplete, new Color(0.45f, 0.55f, 0.70f), lit);
        SaveButton(ButtonStop, false, new Color(0.85f, 0.20f, 0.20f), lit);
        SaveButton(ButtonStart, true, new Color(0.20f, 0.75f, 0.25f), lit);
        SaveBreaker(Breaker, lit);
        AssetDatabase.SaveAssets();
    }

    public static GameObject Instantiate(string prefabPath, Transform parent, string name, Vector3 localPosition, Vector3 localScale)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            EnsureLibrary();
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }

        GameObject instance = prefab != null
            ? (GameObject)PrefabUtility.InstantiatePrefab(prefab)
            : CreateVisualRoot(name, PrimitiveType.Cube, Color.magenta, FindLit());
        instance.name = name;
        instance.transform.SetParent(parent, false);
        instance.transform.localPosition = localPosition;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = localScale;
        return instance;
    }

    public static GameObject InstantiateUnit(string prefabPath, Transform parent, string name, Vector3 localPosition)
    {
        return Instantiate(prefabPath, parent, name, localPosition, Vector3.one);
    }

    public static string EppPath(EppItemType type, EppItemCondition condition)
    {
        bool defect = condition == EppItemCondition.Defective;
        if (type == EppItemType.Helmet)
        {
            return defect ? HelmetDefect : HelmetGood;
        }

        if (type == EppItemType.SafetyGlasses)
        {
            return defect ? GlassesDefect : GlassesGood;
        }

        if (type == EppItemType.ReflectiveVest)
        {
            return VestGood;
        }

        if (type == EppItemType.Gloves)
        {
            return defect ? GlovesDefect : GlovesGood;
        }

        if (type == EppItemType.SafetyBoots)
        {
            return defect ? BootsDefect : BootsGood;
        }

        if (type == EppItemType.Respirator)
        {
            return RespiratorGood;
        }

        return HearingGood;
    }

    public static Renderer FindVisualRenderer(GameObject root)
    {
        Transform visual = root.transform.Find("Visual");
        if (visual != null)
        {
            return visual.GetComponent<Renderer>();
        }

        return root.GetComponentInChildren<Renderer>(true);
    }

    private static void SavePlaceholder(string path, string name, PrimitiveType primitive, Color color, Shader lit)
    {
        GameObject root = CreateVisualRoot(name, primitive, color, lit);
        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
    }

    private static GameObject CreateVisualRoot(string name, PrimitiveType primitive, Color color, Shader lit)
    {
        GameObject root = new GameObject(name);
        GameObject visual = GameObject.CreatePrimitive(primitive);
        visual.name = "Visual";
        visual.transform.SetParent(root.transform, false);
        Collider visualCollider = visual.GetComponent<Collider>();
        if (visualCollider != null)
        {
            Object.DestroyImmediate(visualCollider);
        }

        MeshRenderer renderer = visual.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = GetOrCreateMaterial(name, color, lit);
        }

        BoxCollider box = root.AddComponent<BoxCollider>();
        box.size = Vector3.one;
        return root;
    }

    private static void SaveEpp(
        string path,
        EppItemType type,
        EppItemCondition condition,
        Vector3 visualScale,
        Color color,
        Shader lit,
        bool defective)
    {
        GameObject root = new GameObject(System.IO.Path.GetFileNameWithoutExtension(path));
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = "Visual";
        visual.transform.SetParent(root.transform, false);
        visual.transform.localScale = visualScale;
        Object.DestroyImmediate(visual.GetComponent<Collider>());
        visual.GetComponent<MeshRenderer>().sharedMaterial = GetOrCreateMaterial(root.name, color, lit);

        if (type == EppItemType.SafetyGlasses)
        {
            GameObject lens = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lens.name = "Lens";
            lens.transform.SetParent(visual.transform, false);
            lens.transform.localPosition = new Vector3(0f, 0f, 0.4f);
            lens.transform.localScale = new Vector3(0.9f, 0.55f, 0.2f);
            Object.DestroyImmediate(lens.GetComponent<Collider>());
            lens.GetComponent<MeshRenderer>().sharedMaterial = GetOrCreateMaterial("EPP_Lens", new Color(0.55f, 0.65f, 0.72f), lit);
        }

        if (defective)
        {
            AddDefectCue(visual.transform, type, lit);
        }

        BoxCollider box = root.AddComponent<BoxCollider>();
        box.size = visualScale;
        Rigidbody body = root.AddComponent<Rigidbody>();
        body.useGravity = true;
        body.collisionDetectionMode = CollisionDetectionMode.Continuous;
        XRGrabInteractable grab = root.AddComponent<XRGrabInteractable>();
        grab.interactionLayers = new InteractionLayerMask { value = -1 };
        grab.movementType = XRBaseInteractable.MovementType.Kinematic;
        grab.throwOnDetach = false;
        ref_EppItem itemRef = root.AddComponent<ref_EppItem>();
        itemRef.itemType = type;
        itemRef.condition = condition;
        itemRef.itemRenderer = visual.GetComponent<Renderer>();
        itemRef.grabInteractable = grab;
        root.AddComponent<EppItemInteractable>();
        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
    }

    private static void AddDefectCue(Transform visual, EppItemType type, Shader lit)
    {
        GameObject cue = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cue.name = "Defect";
        cue.transform.SetParent(visual, false);
        Object.DestroyImmediate(cue.GetComponent<Collider>());
        cue.GetComponent<MeshRenderer>().sharedMaterial = GetOrCreateMaterial("EPP_Defect", new Color(0.22f, 0.22f, 0.24f), lit);
        if (type == EppItemType.Helmet)
        {
            cue.transform.localPosition = new Vector3(0.18f, 0.42f, 0.1f);
            cue.transform.localScale = new Vector3(0.35f, 0.22f, 0.35f);
            return;
        }

        if (type == EppItemType.SafetyGlasses)
        {
            cue.transform.localPosition = new Vector3(0f, 0.12f, 0.55f);
            cue.transform.localScale = new Vector3(0.7f, 0.08f, 0.08f);
            return;
        }

        if (type == EppItemType.Gloves)
        {
            cue.transform.localPosition = new Vector3(0.45f, 0.05f, 0.1f);
            cue.transform.localScale = new Vector3(0.35f, 0.12f, 0.55f);
            return;
        }

        cue.transform.localPosition = new Vector3(0f, -0.55f, 0f);
        cue.transform.localScale = new Vector3(0.95f, 0.18f, 0.95f);
    }

    private static void SaveLock(string path, LotoLockType type, Color color, Shader lit)
    {
        GameObject root = CreateGrabRoot(System.IO.Path.GetFileNameWithoutExtension(path), new Vector3(0.12f, 0.18f, 0.06f), color, lit);
        XRGrabInteractable grab = root.GetComponent<XRGrabInteractable>();
        grab.interactionLayers = new InteractionLayerMask { value = LotoSimulationConstants.XriGrabInteractionLayerMask };
        SphereCollider socketCollider = root.AddComponent<SphereCollider>();
        socketCollider.isTrigger = true;
        socketCollider.radius = 0.08f;
        socketCollider.center = new Vector3(0f, 0.12f, 0f);
        XRSocketInteractor tagSocket = root.AddComponent<XRSocketInteractor>();
        tagSocket.interactionLayers = new InteractionLayerMask { value = -1 };
        tagSocket.socketActive = false;
        ref_LotoLock lockRef = root.AddComponent<ref_LotoLock>();
        lockRef.lockType = type;
        lockRef.lockRenderer = FindVisualRenderer(root);
        lockRef.grabInteractable = grab;
        lockRef.tagSocket = tagSocket;
        root.AddComponent<LotoLockInteractable>();
        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
    }

    private static void SaveTag(string path, LotoTagType type, Color color, Shader lit)
    {
        GameObject root = CreateGrabRoot(System.IO.Path.GetFileNameWithoutExtension(path), new Vector3(0.18f, 0.12f, 0.02f), color, lit);
        XRGrabInteractable grab = root.GetComponent<XRGrabInteractable>();
        grab.interactionLayers = new InteractionLayerMask { value = LotoSimulationConstants.XriGrabInteractionLayerMask };
        ref_LotoTag tagRef = root.AddComponent<ref_LotoTag>();
        tagRef.tagType = type;
        tagRef.tagRenderer = FindVisualRenderer(root);
        tagRef.grabInteractable = grab;
        root.AddComponent<LotoTagInteractable>();
        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
    }

    private static void SaveButton(string path, bool isStart, Color color, Shader lit)
    {
        GameObject root = CreateVisualRoot(System.IO.Path.GetFileNameWithoutExtension(path), PrimitiveType.Cube, color, lit);
        root.transform.Find("Visual").localScale = new Vector3(0.45f, 0.2f, 0.15f);
        root.GetComponent<BoxCollider>().size = new Vector3(0.45f, 0.2f, 0.15f);
        XRSimpleInteractable interactable = root.AddComponent<XRSimpleInteractable>();
        interactable.interactionLayers = new InteractionLayerMask { value = -1 };
        ref_LotoControlButton buttonRef = root.AddComponent<ref_LotoControlButton>();
        buttonRef.isStartButton = isStart;
        buttonRef.buttonRenderer = FindVisualRenderer(root);
        buttonRef.interactable = interactable;
        root.AddComponent<LotoControlButtonInteractable>();
        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
    }

    private static void SaveBreaker(string path, Shader lit)
    {
        GameObject root = CreateVisualRoot("PREF_Breaker", PrimitiveType.Cube, new Color(0.2f, 0.8f, 0.2f), lit);
        root.transform.Find("Visual").localScale = new Vector3(0.25f, 0.35f, 0.08f);
        root.GetComponent<BoxCollider>().size = new Vector3(0.25f, 0.35f, 0.08f);
        XRSimpleInteractable interactable = root.AddComponent<XRSimpleInteractable>();
        interactable.interactionLayers = new InteractionLayerMask { value = -1 };
        ref_LotoBreaker breakerRef = root.AddComponent<ref_LotoBreaker>();
        breakerRef.breakerRenderer = FindVisualRenderer(root);
        breakerRef.interactable = interactable;
        root.AddComponent<LotoBreakerInteractable>();
        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
    }

    private static GameObject CreateGrabRoot(string name, Vector3 visualScale, Color color, Shader lit)
    {
        GameObject root = CreateVisualRoot(name, PrimitiveType.Cube, color, lit);
        root.transform.Find("Visual").localScale = visualScale;
        root.GetComponent<BoxCollider>().size = visualScale;
        Rigidbody body = root.AddComponent<Rigidbody>();
        body.useGravity = true;
        body.collisionDetectionMode = CollisionDetectionMode.Continuous;
        XRGrabInteractable grab = root.AddComponent<XRGrabInteractable>();
        grab.interactionLayers = new InteractionLayerMask { value = -1 };
        grab.movementType = XRBaseInteractable.MovementType.Kinematic;
        grab.throwOnDetach = false;
        return root;
    }

    private static Material GetOrCreateMaterial(string key, Color color, Shader lit)
    {
        string path = MaterialsFolder + "/MAT_" + key + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(lit);
            material.color = color;
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        material.shader = lit;
        material.color = color;
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Shader FindLit()
    {
        Shader lit = Shader.Find("Universal Render Pipeline/Lit");
        return lit != null ? lit : Shader.Find("Standard");
    }

    private static void EnsureFolder(string parent, string child)
    {
        string path = parent + "/" + child;
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, child);
        }
    }
}
#endif
