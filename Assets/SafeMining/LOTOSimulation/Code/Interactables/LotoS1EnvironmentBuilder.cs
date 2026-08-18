using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class LotoS1EnvironmentBuilder : MonoBehaviour
{
    private static readonly Color LockerColor = new Color(0.55f, 0.52f, 0.48f);
    private static readonly Color GoodColor = new Color(0.2f, 0.65f, 0.35f);
    private static readonly Color DefectColor = new Color(0.75f, 0.25f, 0.2f);
    private static readonly Color OptionalColor = new Color(0.45f, 0.55f, 0.75f);
    private static readonly Color ContinueColor = new Color(0.976f, 0.451f, 0.086f, 1f);

    #region Public API

    public void Build(EppEquipmentManager manager)
    {
        if (manager == null)
        {
            return;
        }

        ClearChildren();

        Transform roomRoot = CreateEmpty("LockerRoom", new Vector3(-3f, 0f, 3f));
        CreateCube("Floor", roomRoot, new Vector3(4f, 0.05f, 3f), LockerColor).transform.localPosition = new Vector3(0f, 0f, 0f);
        CreateCube("BackWall", roomRoot, new Vector3(4f, 2.5f, 0.1f), LockerColor).transform.localPosition = new Vector3(0f, 1.25f, 1.45f);

        Transform attachRoot = CreateEmpty("AttachPoints", roomRoot.position + new Vector3(-1.2f, 1f, 0.5f));
        attachRoot.SetParent(roomRoot, true);
        CreateAttachPoint(attachRoot, EppItemType.Helmet, new Vector3(0f, 0.5f, 0f));
        CreateAttachPoint(attachRoot, EppItemType.SafetyGlasses, new Vector3(0f, 0.35f, 0f));
        CreateAttachPoint(attachRoot, EppItemType.ReflectiveVest, new Vector3(0f, 0.1f, 0f));
        CreateAttachPoint(attachRoot, EppItemType.Gloves, new Vector3(-0.35f, -0.15f, 0f));
        CreateAttachPoint(attachRoot, EppItemType.SafetyBoots, new Vector3(0.35f, -0.35f, 0f));

        Transform rack = CreateEmpty("SpawnRack", roomRoot.position + new Vector3(1f, 0.6f, 0.2f));
        rack.SetParent(roomRoot, true);

        CreateEppItem("EPP_Helmet_Good", rack.position + new Vector3(-0.6f, 0.4f, 0f), EppItemType.Helmet, EppItemCondition.Good, GoodColor, manager);
        CreateEppItem("EPP_Glasses_Good", rack.position + new Vector3(-0.3f, 0.4f, 0f), EppItemType.SafetyGlasses, EppItemCondition.Good, GoodColor, manager);
        CreateEppItem("EPP_Vest_Good", rack.position + new Vector3(0f, 0.4f, 0f), EppItemType.ReflectiveVest, EppItemCondition.Good, GoodColor, manager);
        CreateEppItem("EPP_Gloves_Good", rack.position + new Vector3(0.3f, 0.4f, 0f), EppItemType.Gloves, EppItemCondition.Good, GoodColor, manager);
        CreateEppItem("EPP_Boots_Good", rack.position + new Vector3(0.6f, 0.4f, 0f), EppItemType.SafetyBoots, EppItemCondition.Good, GoodColor, manager);

        CreateEppItem("EPP_Helmet_Defect", rack.position + new Vector3(-0.45f, 0f, 0.35f), EppItemType.Helmet, EppItemCondition.Defective, DefectColor, manager);
        CreateEppItem("EPP_Gloves_Defect", rack.position + new Vector3(0.15f, 0f, 0.35f), EppItemType.Gloves, EppItemCondition.Defective, DefectColor, manager);

        CreateEppItem("EPP_Respirator", rack.position + new Vector3(0.45f, 0f, 0.35f), EppItemType.Respirator, EppItemCondition.Good, OptionalColor, manager);
        CreateEppItem("EPP_Hearing", rack.position + new Vector3(0.75f, 0f, 0.35f), EppItemType.HearingProtection, EppItemCondition.Good, OptionalColor, manager);

        Transform rejectZone = CreateEmpty("RejectZone", roomRoot.position + new Vector3(1.5f, 0.4f, -0.8f));
        rejectZone.SetParent(roomRoot, true);
        GameObject rejectVisual = CreateCube("RejectBin", rejectZone, new Vector3(0.5f, 0.4f, 0.4f), DefectColor);
        BoxCollider rejectCollider = rejectVisual.AddComponent<BoxCollider>();
        rejectCollider.isTrigger = true;
        manager.rejectZoneAnchor = rejectZone;

        GameObject continueDoor = CreateCube("ContinueDoor", roomRoot, new Vector3(0.6f, 1.2f, 0.15f), ContinueColor);
        continueDoor.transform.localPosition = new Vector3(0f, 0.6f, -1.35f);
        XRSimpleInteractable continueInteractable = continueDoor.AddComponent<XRSimpleInteractable>();
        ConfigureInteractionLayers(continueInteractable);
        LotoS1ContinueInteractable continueLogic = continueDoor.AddComponent<LotoS1ContinueInteractable>();
        continueLogic.Configure(manager);
    }

    #endregion

    #region Builders

    private static void CreateAttachPoint(Transform parent, EppItemType type, Vector3 localPos)
    {
        GameObject point = new GameObject(type.ToString());
        point.transform.SetParent(parent, false);
        point.transform.localPosition = localPos;
    }

    private void CreateEppItem(string name, Vector3 worldPos, EppItemType type, EppItemCondition condition, Color color, EppEquipmentManager manager)
    {
        GameObject itemObject = CreateCube(name, transform, GetScaleForType(type), color);
        itemObject.transform.position = worldPos;
        AddGrabPhysics(itemObject);

        ref_EppItem itemRef = itemObject.AddComponent<ref_EppItem>();
        itemRef.itemType = type;
        itemRef.condition = condition;
        itemRef.itemRenderer = itemObject.GetComponent<Renderer>();
        itemRef.grabInteractable = itemObject.AddComponent<XRGrabInteractable>();
        ConfigureGrabInteractable(itemRef.grabInteractable);

        EppItemInteractable logic = itemObject.AddComponent<EppItemInteractable>();
        logic.Configure(manager);

        if (condition == EppItemCondition.Defective)
        {
            manager.RegisterSpawnedItem(itemRef);
        }
    }

    private static Vector3 GetScaleForType(EppItemType type)
    {
        switch (type)
        {
            case EppItemType.Helmet:
                return new Vector3(0.28f, 0.22f, 0.28f);
            case EppItemType.SafetyGlasses:
                return new Vector3(0.22f, 0.08f, 0.12f);
            case EppItemType.ReflectiveVest:
                return new Vector3(0.35f, 0.4f, 0.12f);
            case EppItemType.Gloves:
                return new Vector3(0.14f, 0.08f, 0.18f);
            case EppItemType.SafetyBoots:
                return new Vector3(0.18f, 0.14f, 0.28f);
            default:
                return new Vector3(0.16f, 0.16f, 0.16f);
        }
    }

    #endregion

    #region Helpers

    private Transform CreateEmpty(string name, Vector3 worldPosition)
    {
        GameObject empty = new GameObject(name);
        empty.transform.SetParent(transform, false);
        empty.transform.position = worldPosition;
        return empty.transform;
    }

    private static GameObject CreateCube(string name, Transform parent, Vector3 scale, Color color)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent, false);
        cube.transform.localScale = scale;
        Renderer renderer = cube.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }

        return cube;
    }

    private static void AddGrabPhysics(GameObject target)
    {
        Rigidbody body = target.AddComponent<Rigidbody>();
        body.useGravity = true;
        body.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    private static void ConfigureInteractionLayers(XRSimpleInteractable interactable)
    {
        if (interactable == null)
        {
            return;
        }

        interactable.interactionLayers = new InteractionLayerMask { value = -1 };
    }

    private static void ConfigureGrabInteractable(XRGrabInteractable grab)
    {
        if (grab == null)
        {
            return;
        }

        grab.interactionLayers = new InteractionLayerMask { value = LotoSimulationConstants.XriGrabInteractionLayerMask };
        grab.movementType = XRBaseInteractable.MovementType.Kinematic;
        grab.throwOnDetach = false;
    }

    private void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }

    #endregion
}
