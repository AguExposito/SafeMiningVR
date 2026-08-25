using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

public class LotoS2EnvironmentBuilder : MonoBehaviour
{
    private static readonly Color ConveyorColor = new Color(0.45f, 0.45f, 0.48f);
    private static readonly Color FloorColor = new Color(0.35f, 0.34f, 0.32f);
    private static readonly Color HazardColor = new Color(0.85f, 0.35f, 0.15f);
    private static readonly Color DecoyColor = new Color(0.35f, 0.55f, 0.35f);
    private static readonly Color ContinueColor = new Color(0.976f, 0.451f, 0.086f, 1f);

    #region Public API

    public void Build(HazardInspectionManager manager)
    {
        if (manager == null)
        {
            return;
        }

        ClearChildren();

        Transform areaRoot = CreateEmpty("InspectionArea", new Vector3(0f, 0f, -4.5f));
        GameObject floor = CreateCube("Floor", areaRoot, new Vector3(7f, 0.05f, 5f), FloorColor);
        floor.transform.localPosition = Vector3.zero;
        ConfigureTeleportArea(floor);

        Transform conveyorRoot = CreateEmpty("StoppedConveyor", areaRoot.position + new Vector3(0f, 0.5f, 1.2f));
        conveyorRoot.SetParent(areaRoot, true);
        CreateCube("BeltVisual", conveyorRoot, new Vector3(3.5f, 0.15f, 0.8f), ConveyorColor).transform.localPosition = Vector3.zero;

        CreateHazardSpot("Hazard_Spill", areaRoot.position + new Vector3(-1.8f, 0.08f, -1f),
            new Vector3(0.6f, 0.06f, 0.4f), HazardColor, HazardType.SpilledMaterial, HazardSpotKind.RealHazard, manager);
        CreateHazardSpot("Hazard_Tools", areaRoot.position + new Vector3(1.4f, 0.12f, -0.5f),
            new Vector3(0.35f, 0.12f, 0.25f), HazardColor, HazardType.AbandonedTools, HazardSpotKind.RealHazard, manager);
        CreateHazardSpot("Hazard_Signage", areaRoot.position + new Vector3(-0.4f, 0.05f, 1.6f),
            new Vector3(0.5f, 0.08f, 0.3f), HazardColor, HazardType.FallenSignage, HazardSpotKind.RealHazard, manager);
        CreateHazardSpot("Hazard_Extinguisher", areaRoot.position + new Vector3(2.2f, 0.35f, 0.9f),
            new Vector3(0.18f, 0.45f, 0.18f), HazardColor, HazardType.ObstructedExtinguisher, HazardSpotKind.RealHazard, manager);

        CreateHazardSpot("Decoy_CleanFloor", areaRoot.position + new Vector3(-2.2f, 0.04f, 0.5f),
            new Vector3(0.5f, 0.04f, 0.5f), DecoyColor, HazardType.DecoyCleanFloor, HazardSpotKind.Decoy, manager);
        CreateHazardSpot("Decoy_StoredTools", areaRoot.position + new Vector3(0.9f, 0.5f, -1.5f),
            new Vector3(0.4f, 0.35f, 0.25f), DecoyColor, HazardType.DecoyStoredTools, HazardSpotKind.Decoy, manager);
        CreateHazardSpot("Decoy_VisibleSign", areaRoot.position + new Vector3(-1.2f, 1.2f, 1.5f),
            new Vector3(0.6f, 0.4f, 0.06f), DecoyColor, HazardType.DecoyVisibleSign, HazardSpotKind.Decoy, manager);

        GameObject continueDoor = CreateCube("ContinueDoor", areaRoot, new Vector3(0.6f, 1.2f, 0.15f), ContinueColor);
        continueDoor.transform.localPosition = new Vector3(0f, 0.6f, -2.3f);
        XRSimpleInteractable continueInteractable = continueDoor.AddComponent<XRSimpleInteractable>();
        ConfigureReportInteractionLayers(continueInteractable);
        LotoS2ContinueInteractable continueLogic = continueDoor.AddComponent<LotoS2ContinueInteractable>();
        continueLogic.Configure(manager);
    }

    #endregion

    #region Builders

    private void CreateHazardSpot(string name, Vector3 worldPos, Vector3 scale, Color color,
        HazardType type, HazardSpotKind kind, HazardInspectionManager manager)
    {
        GameObject spotObject = CreateCube(name, transform, scale, color);
        spotObject.transform.position = worldPos;
        EnsureSolidCollider(spotObject);

        ref_HazardSpot spotRef = spotObject.AddComponent<ref_HazardSpot>();
        spotRef.hazardType = type;
        spotRef.spotKind = kind;
        spotRef.spotRenderer = spotObject.GetComponent<Renderer>();
        spotRef.interactable = spotObject.AddComponent<XRSimpleInteractable>();
        ConfigureReportInteractionLayers(spotRef.interactable);

        HazardReportInteractable logic = spotObject.AddComponent<HazardReportInteractable>();
        logic.Configure(manager);
        spotRef.CacheOriginalColor();

        if (kind == HazardSpotKind.RealHazard)
        {
            manager.RegisterSpawnedSpot(spotRef);
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

    private static void EnsureSolidCollider(GameObject target)
    {
        Collider collider = target.GetComponent<Collider>();
        if (collider == null)
        {
            collider = target.AddComponent<BoxCollider>();
        }

        collider.isTrigger = false;
        collider.enabled = true;
    }

    private static void ConfigureTeleportArea(GameObject floor)
    {
        EnsureSolidCollider(floor);
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

    private static void ConfigureReportInteractionLayers(XRSimpleInteractable interactable)
    {
        if (interactable == null)
        {
            return;
        }

        interactable.interactionLayers = new InteractionLayerMask { value = -1 };
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
