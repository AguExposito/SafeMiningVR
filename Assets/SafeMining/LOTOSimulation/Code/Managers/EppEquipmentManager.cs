using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class EppEquipmentManager : MonoBehaviour
{
    public event Action<int, int> OnProgressChanged;
    public event Action<string> OnHintSubtitle;

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

    private bool selectionObjectiveScored;
    private bool defectObjectiveScored;
    private bool attemptedDefectiveWear;
    private GameObject helmetVisor;
    private static Material helmetVisorMaterial;

    public Transform rejectZoneAnchor;

    public bool IsReadyToAdvance => equippedMandatory.Count >= MandatoryTypes.Length;

    public int EquippedMandatoryCount => equippedMandatory.Count;
    public int MandatoryTotal => MandatoryTypes.Length;
    public bool IsDefectObjectiveScored => defectObjectiveScored;
    public bool IsSelectionObjectiveScored => selectionObjectiveScored;

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
        selectionObjectiveScored = false;
        defectObjectiveScored = false;
        attemptedDefectiveWear = false;

        ref_EppItem[] items = GetComponentsInChildren<ref_EppItem>(true);
        for (int i = 0; i < items.Length; i++)
        {
            RegisterSpawnedItem(items[i]);
        }

        WireChildInteractables();
        ActivateLogicRoots();
        FreezeWorldItems();
        BindBodyAnchors();
        ResolveTeleportPad();
        LotoSafetyGlassesVisionFilter.SetGlassesEquipped(false);
        LotoGloveHandVisuals.SetGlovesEquipped(false);
        SetHelmetVisorVisible(false);

        NotifyProgress();
        NotifyOrchestratorSubtitle("loto_s1_step_select");
        RefreshOutlineHints();
        RefreshTeleportPad();
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
        EnsureHelmetVisor();
    }

    public void RegisterSpawnedItem(ref_EppItem item)
    {
        if (item == null || item.condition != EppItemCondition.Defective)
        {
            return;
        }

        pendingDefectiveIds.Add(item.GetInstanceID());
    }

    public void NotifyDefectiveInspected(ref_EppItem item)
    {
        if (item == null || item.condition != EppItemCondition.Defective)
        {
            return;
        }

        EmitEvent("epp_inspect_defect", true, item.itemType);
        NotifyOrchestratorSubtitle("loto_s1_reject_defective", true);
        LotoTrainingFeedback.PlayFail();
    }

    public bool TryWearDefectiveNearBody(ref_EppItem item)
    {
        if (item == null || item.condition != EppItemCondition.Defective || item.IsLocked)
        {
            return false;
        }

        Transform cameraTransform = Camera.main != null ? Camera.main.transform : null;
        if (cameraTransform == null)
        {
            return false;
        }

        float distance = Vector3.Distance(item.transform.position, cameraTransform.position);
        if (distance > 0.55f)
        {
            return false;
        }

        return TryEquip(item, null);
    }

    public bool TryEquip(ref_EppItem item, Transform attachAnchor)
    {
        if (item == null)
        {
            return false;
        }

        if (item.condition == EppItemCondition.Defective)
        {
            attemptedDefectiveWear = true;
            EmitEvent("epp_equip_defect", false, item.itemType);
            NotifyOrchestratorSubtitle("loto_s1_reject_defective", true);
            LotoTrainingFeedback.PlayFail();
            ScoreDefectObjective(false);
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
            EmitEvent("epp_optional_equip", true, item.itemType);
            NotifyOrchestratorSubtitle("loto_s1_optional_equipped");
        }

        UpdateSessionEppFlags(item.itemType, true);
        if (isMandatory)
        {
            EmitEvent("epp_equip_good", true, item.itemType);
            Debug.Log($"[LOTO] Equipado: {GetDisplayName(item.itemType)} {equippedMandatory.Count}/{MandatoryTypes.Length}");
            NotifyProgress();
        }

        EppItemInteractable interactable = item.GetComponent<EppItemInteractable>();
        if (interactable != null)
        {
            interactable.RaiseEquipped();
        }

        ApplyEquipPresentation(item, attachAnchor);

        if (isMandatory && equippedMandatory.Count >= MandatoryTypes.Length)
        {
            ScoreSelectionObjective();
            NotifyOrchestratorSubtitle("loto_s1_step_ready");
            RefreshTeleportPad();
            onAllMandatoryEquipped?.Invoke();
        }

        LotoTrainingFeedback.PlaySuccess();
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
            NotifyOrchestratorSubtitle("loto_s1_block_incomplete", true);
            LotoTrainingFeedback.PlayFail();
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
        EmitEvent("epp_reject_defect", true, item.itemType);

        EppItemInteractable interactable = item.GetComponent<EppItemInteractable>();
        if (interactable != null)
        {
            interactable.RaiseRejected();
        }

        if (pendingDefectiveIds.Count == 0 && !attemptedDefectiveWear)
        {
            ScoreDefectObjective(true);
            NotifyOrchestratorSubtitle("loto_s1_success_defect");
            if (IsReadyToAdvance)
            {
                NotifyOrchestratorSubtitle("loto_s1_step_ready");
            }
        }

        RefreshOutlineHints();
        LotoTrainingFeedback.PlaySuccess();
        return true;
    }

    public void TryContinue()
    {
        if (!IsReadyToAdvance)
        {
            NotifyOrchestratorSubtitle("loto_s1_block_incomplete", true);
            LotoTrainingFeedback.PlayFail();
            return;
        }

        onContinueAccepted?.Invoke();
        if (scenarioController != null)
        {
            scenarioController.OnEquipmentCompleted();
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

    #region Scoring

    private void ScoreSelectionObjective()
    {
        if (selectionObjectiveScored)
        {
            return;
        }

        selectionObjectiveScored = true;
        SetObjective(LotoSimulationConstants.ObjectiveEppSelectionIndex, true);
        NotifyOrchestratorSubtitle("loto_s1_success_equipped");
    }

    private void ScoreDefectObjective(bool achieved)
    {
        if (defectObjectiveScored && achieved)
        {
            return;
        }

        defectObjectiveScored = true;
        SetObjective(LotoSimulationConstants.ObjectiveEppDefectIndex, achieved);
    }

    private static void SetObjective(int objectiveIndex, bool achieved)
    {
        Util_LotoSimulationBridge.SetObjective(objectiveIndex, achieved);
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

    #region Events

    private void EmitEvent(string optionId, bool success, EppItemType type)
    {
        SimulationEventRecord record = new SimulationEventRecord
        {
            sessionId = Util_LotoSimulationBridge.GetSessionId(),
            eventType = "epp_step",
            objectiveId = success && optionId.Contains("reject")
                ? LotoObjectiveIds.EppDefect
                : LotoObjectiveIds.EppSelection,
            optionId = $"{optionId}_{type}",
            timestampUtc = DateTime.UtcNow.ToString("o"),
            success = success
        };

        Util_LotoSimulationBridge.EnqueueEvent(record);
    }

    private void NotifyProgress()
    {
        OnProgressChanged?.Invoke(equippedMandatory.Count, MandatoryTypes.Length);
        RefreshOutlineHints();
        RefreshTeleportPad();
    }

    private void RefreshOutlineHints()
    {
        LotoOutlineHint hint = GetComponent<LotoOutlineHint>();
        if (hint == null)
        {
            hint = gameObject.AddComponent<LotoOutlineHint>();
        }

        if (IsReadyToAdvance)
        {
            if (teleportToNextArea != null)
            {
                hint.Show(teleportToNextArea.gameObject);
                return;
            }

            LotoS1ContinueInteractable continueDoor = GetComponentInChildren<LotoS1ContinueInteractable>(true);
            if (continueDoor != null)
            {
                hint.Show(continueDoor.gameObject);
                return;
            }

            hint.Clear();
            return;
        }

        ref_EppItem[] items = GetComponentsInChildren<ref_EppItem>(true);
        for (int typeIndex = 0; typeIndex < MandatoryTypes.Length; typeIndex++)
        {
            EppItemType requiredType = MandatoryTypes[typeIndex];
            if (equippedMandatory.Contains(requiredType))
            {
                continue;
            }

            for (int i = 0; i < items.Length; i++)
            {
                ref_EppItem item = items[i];
                if (item == null || item.IsLocked)
                {
                    continue;
                }

                if (item.condition == EppItemCondition.Good && item.itemType == requiredType)
                {
                    hint.Show(item.gameObject);
                    return;
                }
            }
        }

        hint.Clear();
    }

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

        LotoS1ContinueInteractable continueInteractable = GetComponentInChildren<LotoS1ContinueInteractable>(true);
        if (continueInteractable != null)
        {
            continueInteractable.Configure(this);
        }

        ResolveTeleportPad();
        if (teleportToNextArea != null)
        {
            teleportToNextArea.Configure(this);
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

    private void ResolveTeleportPad()
    {
        if (teleportToNextArea == null)
        {
            teleportToNextArea = FindFirstObjectByType<LotoS1TeleportToNextArea>(FindObjectsInactive.Include);
        }
    }

    private void RefreshTeleportPad()
    {
        ResolveTeleportPad();
        if (teleportToNextArea != null)
        {
            teleportToNextArea.SetReady(IsReadyToAdvance);
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

        if (item.itemType == EppItemType.Helmet)
        {
            SetHelmetVisorVisible(true);
            return;
        }

        if (item.itemType == EppItemType.SafetyGlasses)
        {
            LotoSafetyGlassesVisionFilter.SetGlassesEquipped(true);
        }
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

    private void NotifyOrchestratorSubtitle(string key, bool isError = false)
    {
        Util_LotoSimulationBridge.NotifySubtitle(key, OnHintSubtitle, isError);
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

    private void EnsureHelmetVisor()
    {
        if (helmetAnchor == null)
        {
            return;
        }

        if (helmetVisor == null)
        {
            Transform existing = helmetAnchor.Find("HelmetVisor");
            if (existing != null)
            {
                helmetVisor = existing.gameObject;
            }
        }

        if (helmetVisor == null)
        {
            helmetVisor = GameObject.CreatePrimitive(PrimitiveType.Quad);
            helmetVisor.name = "HelmetVisor";
            helmetVisor.SetActive(false);
            Collider visorCollider = helmetVisor.GetComponent<Collider>();
            if (visorCollider != null)
            {
                Destroy(visorCollider);
            }
        }

        helmetVisor.transform.SetParent(helmetAnchor, false);
        helmetVisor.transform.localPosition = new Vector3(0f, -0.04f, 0.12f);
        helmetVisor.transform.localRotation = Quaternion.identity;
        helmetVisor.transform.localScale = new Vector3(0.18f, 0.035f, 1f);

        MeshRenderer visorRenderer = helmetVisor.GetComponent<MeshRenderer>();
        if (visorRenderer != null)
        {
            visorRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            visorRenderer.receiveShadows = false;
            visorRenderer.sharedMaterial = GetHelmetVisorMaterial();
        }
    }

    private void SetHelmetVisorVisible(bool visible)
    {
        EnsureHelmetVisor();
        if (helmetVisor != null)
        {
            helmetVisor.SetActive(visible);
        }
    }

    private static Material GetHelmetVisorMaterial()
    {
        if (helmetVisorMaterial != null)
        {
            return helmetVisorMaterial;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }

        Material material = new Material(shader);
        helmetVisorMaterial = material;
        Color visorColor = new Color(0.12f, 0.1f, 0.08f, 0.22f);
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", visorColor);
        }
        else if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", visorColor);
        }

        material.SetFloat("_Surface", 1f);
        material.SetOverrideTag("RenderType", "Transparent");
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 3000;
        return material;
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
