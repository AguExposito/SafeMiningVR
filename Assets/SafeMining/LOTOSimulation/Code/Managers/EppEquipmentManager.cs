using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class EppEquipmentManager : MonoBehaviour
{
    private static readonly EppItemType[] MandatoryTypes =
    {
        EppItemType.Helmet,
        EppItemType.SafetyGlasses,
        EppItemType.ReflectiveVest,
        EppItemType.Gloves,
        EppItemType.SafetyBoots
    };

    [SerializeField] private LotoScenario1Controller scenarioController;
    [SerializeField] private Transform helmetAnchor;
    [SerializeField] private Transform glassesAnchor;
    [SerializeField] private Transform vestAnchor;
    [SerializeField] private Transform glovesAnchor;
    [SerializeField] private Transform bootsAnchor;
    [SerializeField] private LotoS1TeleportToNextArea teleportToNextArea;
    [SerializeField] private UnityEvent onAllMandatoryEquipped;
    [SerializeField] private UnityEvent onContinueAccepted;

    private readonly HashSet<EppItemType> equippedMandatory = new HashSet<EppItemType>();
    private readonly HashSet<EppItemType> equippedOptional = new HashSet<EppItemType>();
    private readonly HashSet<int> rejectedDefectiveIds = new HashSet<int>();
    private readonly HashSet<int> pendingDefectiveIds = new HashSet<int>();

    public Transform rejectZoneAnchor;

    public bool IsReadyToAdvance => equippedMandatory.Count >= MandatoryTypes.Length;

    public int EquippedMandatoryCount => equippedMandatory.Count;
    public int MandatoryTotal => MandatoryTypes.Length;

    #region Public API

    public void Configure(LotoScenario1Controller controller)
    {
        scenarioController = controller;
    }

    public void Begin()
    {
        equippedMandatory.Clear();
        equippedOptional.Clear();
        rejectedDefectiveIds.Clear();
        pendingDefectiveIds.Clear();

        ref_EppItem[] items = GetComponentsInChildren<ref_EppItem>(true);
        for (int i = 0; i < items.Length; i++)
        {
            RegisterSpawnedItem(items[i]);
        }

        WireChildInteractables();
        ActivateLogicRoots();
        FreezeWorldItems();
        BindBodyAnchors();
        LotoGloveHandVisuals.SetGlovesEquipped(false);
    }

    public void BindBodyAnchors()
    {
        Transform cameraTransform = Camera.main != null ? Camera.main.transform : null;
        if (cameraTransform == null)
        {
            return;
        }

        Transform origin = cameraTransform.root;
        ReparentAnchor(helmetAnchor, cameraTransform, new Vector3(0f, 0f, 0f));
        ReparentAnchor(glassesAnchor, cameraTransform, new Vector3(0f, 0f, 0.12f));
        ReparentAnchor(vestAnchor, cameraTransform, new Vector3(0f, -0.38f, 0.16f));
        ReparentAnchor(glovesAnchor, origin, new Vector3(-0.22f, 0.95f, 0.28f));
        ReparentAnchor(bootsAnchor, origin, new Vector3(0.12f, 0.08f, 0.22f));
    }

    public void RegisterSpawnedItem(ref_EppItem item)
    {
        if (item == null || item.condition != EppItemCondition.Defective)
        {
            return;
        }

        pendingDefectiveIds.Add(item.GetInstanceID());
    }

    public bool TryEquip(ref_EppItem item, Transform attachAnchor)
    {
        if (item == null)
        {
            return false;
        }

        if (item.condition == EppItemCondition.Defective)
        {
            return false;
        }

        bool isMandatory = EppTypeRules.IsMandatory(item.itemType);
        if (isMandatory)
        {
            if (equippedMandatory.Contains(item.itemType))
            {
                return false;
            }
        }
        else
        {
            if (equippedOptional.Contains(item.itemType))
            {
                return false;
            }
        }

        item.SetEquipped(true);
        if (isMandatory)
        {
            equippedMandatory.Add(item.itemType);
        }
        else
        {
            equippedOptional.Add(item.itemType);
        }

        EppItemInteractable interactable = item.GetComponent<EppItemInteractable>();
        if (interactable != null)
        {
            interactable.RaiseEquipped();
        }

        ApplyEquipPresentation(item, attachAnchor);
        UpdateSessionEppFlags(item.itemType, true);
        return true;
    }

    public bool TryReject(ref_EppItem item)
    {
        if (item == null)
        {
            return false;
        }

        if (item.condition != EppItemCondition.Defective)
        {
            return false;
        }

        int itemId = item.GetInstanceID();
        if (rejectedDefectiveIds.Contains(itemId))
        {
            return false;
        }

        rejectedDefectiveIds.Add(itemId);
        pendingDefectiveIds.Remove(itemId);
        item.SetRejected(true);

        EppItemInteractable interactable = item.GetComponent<EppItemInteractable>();
        if (interactable != null)
        {
            interactable.RaiseRejected();
        }

        return true;
    }

    public void TryContinue()
    {
        LotoGameManager board = LotoGameManager.Instance;
        if (board != null)
        {
            board.TryContinueS1();
        }
    }

    public Transform GetAttachAnchor(EppItemType type)
    {
        switch (type)
        {
            case EppItemType.Helmet:
                return helmetAnchor;
            case EppItemType.SafetyGlasses:
                return glassesAnchor;
            case EppItemType.ReflectiveVest:
                return vestAnchor;
            case EppItemType.Gloves:
                return glovesAnchor;
            case EppItemType.SafetyBoots:
                return bootsAnchor;
            default:
                return null;
        }
    }

    #endregion

    #region Session

    private static void UpdateSessionEppFlags(EppItemType type, bool equipped)
    {
        if (ControlCenter.Instance == null || ControlCenter.Instance.simulationSessionManager == null)
        {
            return;
        }

        SimulationSessionContext session = ControlCenter.Instance.simulationSessionManager.CurrentSession;
        if (session == null)
        {
            return;
        }

        switch (type)
        {
            case EppItemType.Helmet:
                session.hasHelmet = equipped;
                break;
            case EppItemType.SafetyGlasses:
                session.hasSafetyGlasses = equipped;
                break;
            case EppItemType.ReflectiveVest:
                session.hasReflectiveVest = equipped;
                break;
            case EppItemType.Gloves:
                session.hasGloves = equipped;
                break;
            case EppItemType.SafetyBoots:
                session.hasSafetyBoots = equipped;
                break;
            case EppItemType.Respirator:
                session.hasRespirator = equipped;
                break;
            case EppItemType.HearingProtection:
                session.hasHearingProtection = equipped;
                break;
        }
    }

    #endregion

    #region Presentation

    private void WireChildInteractables()
    {
        EppItemInteractable[] itemInteractables = GetComponentsInChildren<EppItemInteractable>(true);
        for (int i = 0; i < itemInteractables.Length; i++)
        {
            if (itemInteractables[i] != null)
            {
                itemInteractables[i].Configure(this);
            }
        }

        if (rejectZoneAnchor != null)
        {
            EppTypedSocket rejectSocket = rejectZoneAnchor.GetComponent<EppTypedSocket>();
            if (rejectSocket != null)
            {
                rejectSocket.Configure(this, EppSocketRole.Reject, EppItemType.Helmet);
            }
        }
    }

    private void ApplyEquipPresentation(ref_EppItem item, Transform attachAnchor)
    {
        if (item == null)
        {
            return;
        }

        HideItemRenderers(item);
        item.gameObject.SetActive(false);
    }

    private void ActivateLogicRoots()
    {
        ActivateLogicRoot("BodyAnchors");
        ActivateLogicRoot("LockerSlots");
    }

    private void FreezeWorldItems()
    {
        ref_EppItem[] items = GetComponentsInChildren<ref_EppItem>(true);
        for (int i = 0; i < items.Length; i++)
        {
            ref_EppItem item = items[i];
            if (item == null || item.IsLocked)
            {
                continue;
            }

            EppItemInteractable interactable = item.GetComponent<EppItemInteractable>();
            if (interactable != null)
            {
                interactable.ApplyRestingPhysics();
            }
        }
    }

    private void ActivateLogicRoot(string rootName)
    {
        Transform root = transform.Find(rootName);
        if (root == null)
        {
            return;
        }

        root.gameObject.SetActive(true);
        DisableRenderersRecursive(root);

        EppTypedSocket[] sockets = root.GetComponentsInChildren<EppTypedSocket>(true);
        for (int i = 0; i < sockets.Length; i++)
        {
            EppTypedSocket typed = sockets[i];
            if (typed == null)
            {
                continue;
            }

            typed.BindManager(this);

            XRSocketInteractor xrSocket = typed.GetComponent<XRSocketInteractor>();
            if (xrSocket != null)
            {
                xrSocket.socketActive = false;
            }
        }
    }

    private static void DisableRenderersRecursive(Transform root)
    {
        if (root == null)
        {
            return;
        }

        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].enabled = false;
            }
        }
    }

    public static string GetDisplayName(EppItemType type)
    {
        switch (type)
        {
            case EppItemType.Helmet:
                return "Casco";
            case EppItemType.SafetyGlasses:
                return "Gafas";
            case EppItemType.ReflectiveVest:
                return "Chaleco";
            case EppItemType.Gloves:
                return "Guantes";
            case EppItemType.SafetyBoots:
                return "Botas";
            case EppItemType.Respirator:
                return "Respirador";
            case EppItemType.HearingProtection:
                return "Proteccion auditiva";
            default:
                return type.ToString();
        }
    }

    private static void HideItemRenderers(ref_EppItem item)
    {
        if (item == null)
        {
            return;
        }

        Renderer[] renderers = item.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].enabled = false;
            }
        }
    }


    private static void ReparentAnchor(Transform anchor, Transform parent, Vector3 localPosition)
    {
        if (anchor == null || parent == null)
        {
            return;
        }

        anchor.SetParent(parent, false);
        anchor.localPosition = localPosition;
        anchor.localRotation = Quaternion.identity;
        anchor.localScale = Vector3.one;
    }

    #endregion
}
