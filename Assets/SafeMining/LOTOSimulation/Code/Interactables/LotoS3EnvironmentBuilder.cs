using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class LotoS3EnvironmentBuilder : MonoBehaviour
{
    private static readonly Color ConveyorColor = new Color(0.45f, 0.45f, 0.48f);
    private static readonly Color PanelColor = new Color(0.25f, 0.28f, 0.32f);
    private static readonly Color LockRed = new Color(0.85f, 0.15f, 0.15f);
    private static readonly Color LockBlue = new Color(0.15f, 0.35f, 0.85f);
    private static readonly Color LockYellow = new Color(0.9f, 0.8f, 0.15f);
    private static readonly Color TagWhite = new Color(0.95f, 0.95f, 0.9f);
    private static readonly Color TagDecoy = new Color(0.7f, 0.7f, 0.75f);

    #region Public API

    public void Build(LotoSequenceManager sequenceManager)
    {
        if (sequenceManager == null)
        {
            return;
        }

        ClearChildren();

        Transform conveyorRoot = CreateEmpty("Conveyor", new Vector3(4f, 0.5f, 2f));
        Transform beltVisual = CreateCube("BeltVisual", conveyorRoot, new Vector3(3f, 0.15f, 0.8f), ConveyorColor).transform;
        beltVisual.localPosition = Vector3.zero;
        BeltAnimator beltAnimator = conveyorRoot.gameObject.AddComponent<BeltAnimator>();
        beltAnimator.Configure(beltVisual);

        Transform controlStation = CreateEmpty("ControlStation", new Vector3(2f, 1f, 0.5f));
        CreateControlButton("StopButton", controlStation, new Vector3(-0.35f, 0f, 0f), false, sequenceManager, new Color(0.85f, 0.2f, 0.2f));
        CreateControlButton("StartButton", controlStation, new Vector3(0.35f, 0f, 0f), true, sequenceManager, new Color(0.2f, 0.75f, 0.25f));

        Transform panelRoot = CreateEmpty("ElectricalPanel", new Vector3(6f, 1.2f, 2f));
        CreateCube("PanelBody", panelRoot, new Vector3(0.8f, 1.2f, 0.4f), PanelColor);
        ref_LotoBreaker breakerRef = CreateBreaker(panelRoot, new Vector3(0f, 0.1f, -0.25f), sequenceManager);

        Transform haspAnchor = CreateEmpty("HaspAnchor", panelRoot.position + new Vector3(0f, -0.2f, -0.35f));
        haspAnchor.SetParent(panelRoot, true);
        CreateCube("Hasp", haspAnchor, new Vector3(0.15f, 0.08f, 0.08f), Color.gray);

        Transform tagAnchor = CreateEmpty("TagAnchor", haspAnchor.position + new Vector3(0f, 0.12f, 0f));
        tagAnchor.SetParent(haspAnchor, true);

        Transform lockSpawn = CreateEmpty("LockSpawn", new Vector3(5.2f, 0.6f, 0.5f));
        CreateLock("Lock_Red", lockSpawn.position + new Vector3(0f, 0f, 0f), LotoLockType.Personal, LockRed, sequenceManager);
        CreateLock("Lock_Blue", lockSpawn.position + new Vector3(0.35f, 0f, 0f), LotoLockType.Visitor, LockBlue, sequenceManager);
        CreateLock("Lock_Yellow", lockSpawn.position + new Vector3(0.7f, 0f, 0f), LotoLockType.Group, LockYellow, sequenceManager);

        Transform tagSpawn = CreateEmpty("TagSpawn", new Vector3(5.2f, 0.6f, 1.2f));
        CreateTag("Tag_Danger", tagSpawn.position + new Vector3(0f, 0f, 0f), LotoTagType.DangerDoNotOperate, TagWhite, sequenceManager);
        CreateTag("Tag_Decoy", tagSpawn.position + new Vector3(0.35f, 0f, 0f), LotoTagType.Inspected, TagDecoy, sequenceManager);

        sequenceManager.Configure(beltAnimator, haspAnchor, tagAnchor);
    }

    #endregion

    #region Builders

    private void CreateControlButton(string name, Transform parent, Vector3 localPos, bool isStart, LotoSequenceManager manager, Color color)
    {
        GameObject buttonObject = CreateCube(name, parent, new Vector3(0.45f, 0.2f, 0.15f), color);
        buttonObject.transform.localPosition = localPos;

        ref_LotoControlButton buttonRef = buttonObject.AddComponent<ref_LotoControlButton>();
        buttonRef.isStartButton = isStart;
        buttonRef.buttonRenderer = buttonObject.GetComponent<Renderer>();
        buttonRef.interactable = buttonObject.AddComponent<XRSimpleInteractable>();
        ConfigureInteractionLayers(buttonRef.interactable);

        LotoControlButtonInteractable logic = buttonObject.AddComponent<LotoControlButtonInteractable>();
        logic.Configure(manager);
    }

    private ref_LotoBreaker CreateBreaker(Transform parent, Vector3 localPos, LotoSequenceManager manager)
    {
        GameObject breakerObject = CreateCube("Breaker", parent, new Vector3(0.25f, 0.35f, 0.08f), new Color(0.2f, 0.8f, 0.2f));
        breakerObject.transform.localPosition = localPos;

        ref_LotoBreaker breakerRef = breakerObject.AddComponent<ref_LotoBreaker>();
        breakerRef.breakerRenderer = breakerObject.GetComponent<Renderer>();
        breakerRef.interactable = breakerObject.AddComponent<XRSimpleInteractable>();
        ConfigureInteractionLayers(breakerRef.interactable);

        LotoBreakerInteractable logic = breakerObject.AddComponent<LotoBreakerInteractable>();
        logic.Configure(manager);
        return breakerRef;
    }

    private void CreateLock(string name, Vector3 worldPos, LotoLockType type, Color color, LotoSequenceManager manager)
    {
        GameObject lockObject = CreateCube(name, transform, new Vector3(0.12f, 0.18f, 0.06f), color);
        lockObject.transform.position = worldPos;
        AddGrabPhysics(lockObject);

        ref_LotoLock lockRef = lockObject.AddComponent<ref_LotoLock>();
        lockRef.lockType = type;
        lockRef.lockRenderer = lockObject.GetComponent<Renderer>();
        lockRef.grabInteractable = lockObject.AddComponent<XRGrabInteractable>();
        ConfigureGrabInteractable(lockRef.grabInteractable);

        LotoLockInteractable logic = lockObject.AddComponent<LotoLockInteractable>();
        logic.Configure(manager);
    }

    private void CreateTag(string name, Vector3 worldPos, LotoTagType type, Color color, LotoSequenceManager manager)
    {
        GameObject tagObject = CreateCube(name, transform, new Vector3(0.18f, 0.12f, 0.02f), color);
        tagObject.transform.position = worldPos;
        AddGrabPhysics(tagObject);

        ref_LotoTag tagRef = tagObject.AddComponent<ref_LotoTag>();
        tagRef.tagType = type;
        tagRef.tagRenderer = tagObject.GetComponent<Renderer>();
        tagRef.grabInteractable = tagObject.AddComponent<XRGrabInteractable>();
        ConfigureGrabInteractable(tagRef.grabInteractable);

        LotoTagInteractable logic = tagObject.AddComponent<LotoTagInteractable>();
        logic.Configure(manager);
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
