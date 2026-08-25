#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public static class LotoS1LockerPrefabBuilder
{
    private static readonly Color Orange = new Color(0.976f, 0.451f, 0.086f);
    private static readonly Color Cabinet = new Color(0.42f, 0.45f, 0.48f);
    private static readonly Color Shelf = new Color(0.52f, 0.54f, 0.56f);
    private static readonly Color HelmetYellow = new Color(0.90f, 0.72f, 0.12f);
    private static readonly Color Frame = new Color(0.12f, 0.16f, 0.22f);
    private static readonly Color Lens = new Color(0.55f, 0.65f, 0.72f);
    private static readonly Color Glove = new Color(0.42f, 0.45f, 0.50f);
    private static readonly Color Boot = new Color(0.22f, 0.18f, 0.16f);
    private static readonly Color DefectMark = new Color(0.22f, 0.22f, 0.24f);
    private static readonly Color Respirator = new Color(0.32f, 0.38f, 0.44f);
    private static readonly Color Hearing = new Color(0.20f, 0.25f, 0.32f);
    private static readonly Color Bin = new Color(0.22f, 0.24f, 0.28f);
    private static readonly Color Floor = new Color(0.28f, 0.30f, 0.33f);

    public const string PrefabPath = "Assets/SafeMining/LOTOSimulation/Prefabs/PREF_S1_Locker.prefab";

    public static GameObject Build()
    {
        LotoVisualPrefabLibrary.EnsureLibrary();
        Shader lit = Shader.Find("Universal Render Pipeline/Lit");
        if (lit == null)
        {
            lit = Shader.Find("Standard");
        }

        GameObject root = new GameObject("PREF_S1_Locker");
        EppEquipmentManager manager = root.AddComponent<EppEquipmentManager>();

        GameObject floor = CreateCube("FloorPlate", root.transform, new Vector3(2.2f, 0.04f, 1.1f), Floor, lit);
        floor.transform.localPosition = new Vector3(0f, 0.02f, 0f);

        GameObject back = CreateCube("CabinetBack", root.transform, new Vector3(1.8f, 1.7f, 0.08f), Cabinet, lit);
        back.transform.localPosition = new Vector3(0f, 1.05f, 0.38f);

        GameObject leftWall = CreateCube("CabinetLeft", root.transform, new Vector3(0.08f, 1.7f, 0.55f), Cabinet, lit);
        leftWall.transform.localPosition = new Vector3(-0.86f, 1.05f, 0.14f);
        GameObject rightWall = CreateCube("CabinetRight", root.transform, new Vector3(0.08f, 1.7f, 0.55f), Cabinet, lit);
        rightWall.transform.localPosition = new Vector3(0.86f, 1.05f, 0.14f);

        CreateShelf(root.transform, "ShelfTop", new Vector3(0f, 1.45f, 0.12f), lit);
        CreateShelf(root.transform, "ShelfMid", new Vector3(0f, 0.95f, 0.12f), lit);
        CreateShelf(root.transform, "ShelfLow", new Vector3(0f, 0.48f, 0.12f), lit);

        Transform slots = CreateEmpty("LockerSlots", root.transform, Vector3.zero);
        Transform itemsRoot = CreateEmpty("Items", root.transform, Vector3.zero);
        Transform bodyRoot = CreateEmpty("BodyAnchors", root.transform, Vector3.zero);

        SlotSpec[] goodSlots =
        {
            new SlotSpec(EppItemType.Helmet, "loto_epp_helmet", new Vector3(-0.64f, 1.58f, 0.05f), new Vector3(0.28f, 0.22f, 0.28f), HelmetYellow, false),
            new SlotSpec(EppItemType.SafetyGlasses, "loto_epp_glasses", new Vector3(-0.32f, 1.58f, 0.05f), new Vector3(0.22f, 0.08f, 0.12f), Frame, false),
            new SlotSpec(EppItemType.ReflectiveVest, "loto_epp_vest", new Vector3(0f, 1.58f, 0.05f), new Vector3(0.32f, 0.38f, 0.10f), Orange, false),
            new SlotSpec(EppItemType.Gloves, "loto_epp_gloves", new Vector3(0.32f, 1.58f, 0.05f), new Vector3(0.14f, 0.08f, 0.18f), Glove, false),
            new SlotSpec(EppItemType.SafetyBoots, "loto_epp_boots", new Vector3(0.64f, 1.58f, 0.05f), new Vector3(0.18f, 0.14f, 0.28f), Boot, false)
        };

        SlotSpec[] defectSlots =
        {
            new SlotSpec(EppItemType.Helmet, "loto_epp_helmet", new Vector3(-0.48f, 1.08f, 0.05f), new Vector3(0.28f, 0.22f, 0.28f), HelmetYellow, true),
            new SlotSpec(EppItemType.SafetyGlasses, "loto_epp_glasses", new Vector3(-0.16f, 1.08f, 0.05f), new Vector3(0.22f, 0.08f, 0.12f), Frame, true),
            new SlotSpec(EppItemType.Gloves, "loto_epp_gloves", new Vector3(0.16f, 1.08f, 0.05f), new Vector3(0.14f, 0.08f, 0.18f), Glove, true),
            new SlotSpec(EppItemType.SafetyBoots, "loto_epp_boots", new Vector3(0.48f, 1.08f, 0.05f), new Vector3(0.18f, 0.08f, 0.28f), Boot, true)
        };

        SlotSpec[] optionalSlots =
        {
            new SlotSpec(EppItemType.Respirator, "loto_epp_optional", new Vector3(-0.32f, 0.62f, 0.05f), new Vector3(0.18f, 0.16f, 0.16f), Respirator, false),
            new SlotSpec(EppItemType.HearingProtection, "loto_epp_optional", new Vector3(0.32f, 0.62f, 0.05f), new Vector3(0.16f, 0.16f, 0.12f), Hearing, false)
        };

        for (int i = 0; i < goodSlots.Length; i++)
        {
            CreateLockerSlot(slots, goodSlots[i], manager, lit);
            CreateEppItem(itemsRoot, goodSlots[i], EppItemCondition.Good, manager, lit);
        }

        for (int i = 0; i < defectSlots.Length; i++)
        {
            CreateLockerSlot(slots, defectSlots[i], manager, lit);
            CreateEppItem(itemsRoot, defectSlots[i], EppItemCondition.Defective, manager, lit);
        }

        for (int i = 0; i < optionalSlots.Length; i++)
        {
            CreateLockerSlot(slots, optionalSlots[i], manager, lit);
            CreateEppItem(itemsRoot, optionalSlots[i], EppItemCondition.Good, manager, lit);
        }

        Transform helmetAnchor = CreateBodyAnchor(bodyRoot, "Helmet", manager, EppItemType.Helmet);
        Transform glassesAnchor = CreateBodyAnchor(bodyRoot, "SafetyGlasses", manager, EppItemType.SafetyGlasses);
        Transform vestAnchor = CreateBodyAnchor(bodyRoot, "ReflectiveVest", manager, EppItemType.ReflectiveVest);
        Transform glovesAnchor = CreateBodyAnchor(bodyRoot, "Gloves", manager, EppItemType.Gloves);
        Transform bootsAnchor = CreateBodyAnchor(bodyRoot, "SafetyBoots", manager, EppItemType.SafetyBoots);

        GameObject rejectBin = CreateCube("RejectBin", root.transform, new Vector3(0.45f, 0.38f, 0.40f), Bin, lit);
        rejectBin.transform.localPosition = new Vector3(1.15f, 0.22f, -0.15f);
        AddSocket(rejectBin, 0.28f);
        EppTypedSocket rejectTyped = rejectBin.AddComponent<EppTypedSocket>();
        rejectTyped.Configure(manager, EppSocketRole.Reject, EppItemType.Helmet);
        manager.rejectZoneAnchor = rejectBin.transform;

        GameObject exitDoor = CreateCube("ExitDoor", root.transform, new Vector3(0.7f, 1.35f, 0.08f), Orange, lit);
        exitDoor.transform.localPosition = new Vector3(-1.25f, 0.72f, 0.05f);
        XRSimpleInteractable exitInteractable = exitDoor.AddComponent<XRSimpleInteractable>();
        exitInteractable.interactionLayers = new InteractionLayerMask { value = -1 };
        LotoS1ContinueInteractable continueLogic = exitDoor.AddComponent<LotoS1ContinueInteractable>();
        continueLogic.Configure(manager);

        SerializedObject so = new SerializedObject(manager);
        so.FindProperty("helmetAnchor").objectReferenceValue = helmetAnchor;
        so.FindProperty("glassesAnchor").objectReferenceValue = glassesAnchor;
        so.FindProperty("vestAnchor").objectReferenceValue = vestAnchor;
        so.FindProperty("glovesAnchor").objectReferenceValue = glovesAnchor;
        so.FindProperty("bootsAnchor").objectReferenceValue = bootsAnchor;
        so.ApplyModifiedPropertiesWithoutUndo();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        UnityEngine.Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        return prefab;
    }

    private struct SlotSpec
    {
        public EppItemType Type;
        public string LocKey;
        public Vector3 Position;
        public Vector3 Scale;
        public Color Color;
        public bool Defective;

        public SlotSpec(EppItemType type, string locKey, Vector3 position, Vector3 scale, Color color, bool defective)
        {
            Type = type;
            LocKey = locKey;
            Position = position;
            Scale = scale;
            Color = color;
            Defective = defective;
        }
    }

    private static Transform CreateEmpty(string name, Transform parent, Vector3 localPos)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        return go.transform;
    }

    private static GameObject CreateCube(string name, Transform parent, Vector3 scale, Color color, Shader lit)
    {
        return LotoVisualPrefabLibrary.Instantiate(ResolvePlaceholderPath(name), parent, name, Vector3.zero, scale);
    }

    private static string ResolvePlaceholderPath(string name)
    {
        if (name.StartsWith("Shelf"))
        {
            return LotoVisualPrefabLibrary.Shelf;
        }

        if (name.Contains("Cabinet"))
        {
            return LotoVisualPrefabLibrary.CabinetPanel;
        }

        if (name.Contains("Floor"))
        {
            return LotoVisualPrefabLibrary.FloorPlate;
        }

        if (name.Contains("Reject") || name.Contains("Bin"))
        {
            return LotoVisualPrefabLibrary.RejectBin;
        }

        if (name.Contains("Door") || name.Contains("Exit"))
        {
            return LotoVisualPrefabLibrary.ExitDoor;
        }

        return LotoVisualPrefabLibrary.Cube;
    }

    private static void CreateShelf(Transform parent, string name, Vector3 localPos, Shader lit)
    {
        GameObject shelf = CreateCube(name, parent, new Vector3(1.7f, 0.04f, 0.5f), Shelf, lit);
        shelf.transform.localPosition = localPos;
    }

    private static void ApplyColor(GameObject target, Color color, Shader lit)
    {
        MeshRenderer renderer = target.GetComponent<MeshRenderer>();
        if (renderer == null || lit == null)
        {
            return;
        }

        Material material = new Material(lit);
        material.color = color;
        renderer.sharedMaterial = material;
    }

    private static void CreateLockerSlot(Transform parent, SlotSpec spec, EppEquipmentManager manager, Shader lit)
    {
        GameObject slot = new GameObject((spec.Defective ? "SlotDefect_" : "Slot_") + spec.Type);
        slot.transform.SetParent(parent, false);
        slot.transform.localPosition = spec.Position;
        AddSocket(slot, 0.14f);
        EppTypedSocket typed = slot.AddComponent<EppTypedSocket>();
        typed.Configure(manager, EppSocketRole.Locker, spec.Type);
    }

    private static Transform CreateBodyAnchor(Transform parent, string name, EppEquipmentManager manager, EppItemType type)
    {
        GameObject anchor = new GameObject(name);
        anchor.transform.SetParent(parent, false);
        AddSocket(anchor, 0.18f);
        EppTypedSocket typed = anchor.AddComponent<EppTypedSocket>();
        typed.Configure(manager, EppSocketRole.Body, type);
        return anchor.transform;
    }

    private static XRSocketInteractor AddSocket(GameObject host, float radius)
    {
        SphereCollider collider = host.GetComponent<SphereCollider>();
        if (collider == null)
        {
            collider = host.AddComponent<SphereCollider>();
        }

        collider.isTrigger = true;
        collider.radius = radius;
        XRSocketInteractor socket = host.AddComponent<XRSocketInteractor>();
        socket.interactionLayers = new InteractionLayerMask { value = -1 };
        socket.socketActive = true;
        socket.showInteractableHoverMeshes = true;
        return socket;
    }

    private static void CreateEppItem(Transform parent, SlotSpec spec, EppItemCondition condition, EppEquipmentManager manager, Shader lit)
    {
        string name = "EPP_" + spec.Type + (condition == EppItemCondition.Defective ? "_Defect" : "_Good");
        GameObject item = LotoVisualPrefabLibrary.InstantiateUnit(
            LotoVisualPrefabLibrary.EppPath(spec.Type, condition),
            parent,
            name,
            spec.Position + new Vector3(0f, 0.08f, -0.08f));

        EppItemInteractable logic = item.GetComponent<EppItemInteractable>();
        if (logic != null)
        {
            logic.Configure(manager);
        }

        ref_EppItem itemRef = item.GetComponent<ref_EppItem>();
        if (condition == EppItemCondition.Defective && itemRef != null)
        {
            manager.RegisterSpawnedItem(itemRef);
        }
    }

    private static void AddDefectCue(Transform parent, EppItemType type, Shader lit)
    {
        if (type == EppItemType.Helmet)
        {
            GameObject dent = CreateCube("Dent", parent, new Vector3(0.35f, 0.22f, 0.35f), DefectMark, lit);
            dent.transform.localPosition = new Vector3(0.18f, 0.42f, 0.1f);
            UnityEngine.Object.DestroyImmediate(dent.GetComponent<Collider>());
            return;
        }

        if (type == EppItemType.SafetyGlasses)
        {
            GameObject scratchA = CreateCube("ScratchA", parent, new Vector3(0.7f, 0.08f, 0.08f), DefectMark, lit);
            scratchA.transform.localPosition = new Vector3(0f, 0.12f, 0.55f);
            scratchA.transform.localRotation = Quaternion.Euler(0f, 0f, 25f);
            UnityEngine.Object.DestroyImmediate(scratchA.GetComponent<Collider>());
            GameObject scratchB = CreateCube("ScratchB", parent, new Vector3(0.55f, 0.07f, 0.07f), DefectMark, lit);
            scratchB.transform.localPosition = new Vector3(0.05f, -0.1f, 0.55f);
            scratchB.transform.localRotation = Quaternion.Euler(0f, 0f, -18f);
            UnityEngine.Object.DestroyImmediate(scratchB.GetComponent<Collider>());
            return;
        }

        if (type == EppItemType.Gloves)
        {
            GameObject flap = CreateCube("TearFlap", parent, new Vector3(0.35f, 0.12f, 0.55f), DefectMark, lit);
            flap.transform.localPosition = new Vector3(0.45f, 0.05f, 0.1f);
            flap.transform.localRotation = Quaternion.Euler(0f, 0f, 28f);
            UnityEngine.Object.DestroyImmediate(flap.GetComponent<Collider>());
            return;
        }

        if (type == EppItemType.SafetyBoots)
        {
            GameObject worn = CreateCube("WornSole", parent, new Vector3(0.95f, 0.18f, 0.95f), DefectMark, lit);
            worn.transform.localPosition = new Vector3(0f, -0.55f, 0f);
            UnityEngine.Object.DestroyImmediate(worn.GetComponent<Collider>());
        }
    }

}
#endif
