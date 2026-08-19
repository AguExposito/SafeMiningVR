using System;
using System.Collections.Generic;
using UnityEngine;

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

    private readonly HashSet<EppItemType> equippedMandatory = new HashSet<EppItemType>();
    private readonly HashSet<int> rejectedDefectiveIds = new HashSet<int>();
    private readonly HashSet<int> pendingDefectiveIds = new HashSet<int>();

    private bool selectionObjectiveScored;
    private bool defectObjectiveScored;
    private bool attemptedDefectiveWear;

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

        NotifyProgress();
        NotifyOrchestratorSubtitle("loto_s1_step_select");
    }

    public void BindBodyAnchors()
    {
        Transform cameraTransform = Camera.main != null ? Camera.main.transform : null;
        if (cameraTransform == null)
        {
            return;
        }

        Transform origin = cameraTransform.root;
        ReparentAnchor(helmetAnchor, cameraTransform, new Vector3(0f, 0.12f, 0.04f));
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

        if (!EppTypeRules.IsMandatory(item.itemType))
        {
            EmitEvent("epp_optional_grab", true, item.itemType);
            NotifyOrchestratorSubtitle("loto_s1_optional_item");
            return false;
        }

        if (equippedMandatory.Contains(item.itemType))
        {
            return false;
        }

        if (item.condition == EppItemCondition.Defective)
        {
            attemptedDefectiveWear = true;
            EmitEvent("epp_equip_defect", false, item.itemType);
            NotifyOrchestratorSubtitle("loto_s1_reject_defective");
            ScoreDefectObjective(false);
            return false;
        }

        if (attachAnchor != null)
        {
            item.transform.SetPositionAndRotation(attachAnchor.position, attachAnchor.rotation);
            item.transform.SetParent(attachAnchor, true);
        }

        item.SetEquipped(true);
        equippedMandatory.Add(item.itemType);
        UpdateSessionEppFlags(item.itemType, true);
        EmitEvent("epp_equip_good", true, item.itemType);
        NotifyProgress();

        if (equippedMandatory.Count >= MandatoryTypes.Length)
        {
            ScoreSelectionObjective();
            if (!attemptedDefectiveWear)
            {
                ScoreDefectObjective(true);
            }

            NotifyOrchestratorSubtitle("loto_s1_step_ready");
        }

        return true;
    }

    public bool TryReject(ref_EppItem item)
    {
        if (item == null || item.condition != EppItemCondition.Defective)
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
        EmitEvent("epp_reject_defect", true, item.itemType);

        if (pendingDefectiveIds.Count == 0 && !attemptedDefectiveWear)
        {
            ScoreDefectObjective(true);
            NotifyOrchestratorSubtitle("loto_s1_success_defect");
            if (IsReadyToAdvance)
            {
                NotifyOrchestratorSubtitle("loto_s1_step_ready");
            }
        }

        return true;
    }

    public void TryContinue()
    {
        if (!IsReadyToAdvance)
        {
            NotifyOrchestratorSubtitle("loto_s1_block_incomplete");
            return;
        }

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
        if (ControlCenter.Instance == null || ControlCenter.Instance.scoreManager == null)
        {
            return;
        }

        ControlCenter.Instance.scoreManager.AchiveObjective(
            LotoSimulationConstants.LotoSimScoreIndex,
            objectiveIndex,
            achieved);

        if (ControlCenter.Instance.simulationOrchestratorManager != null)
        {
            ControlCenter.Instance.simulationOrchestratorManager.NotifyObjectiveVisualUpdate(objectiveIndex, achieved);
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
        }
    }

    #endregion

    #region Events

    private void EmitEvent(string optionId, bool success, EppItemType type)
    {
        if (ControlCenter.Instance == null || ControlCenter.Instance.syncQueueManager == null)
        {
            return;
        }

        string sessionId = string.Empty;
        if (ControlCenter.Instance.simulationSessionManager != null &&
            ControlCenter.Instance.simulationSessionManager.CurrentSession != null)
        {
            sessionId = ControlCenter.Instance.simulationSessionManager.CurrentSession.sessionId;
        }

        SimulationEventRecord record = new SimulationEventRecord
        {
            sessionId = sessionId,
            eventType = "epp_step",
            objectiveId = success && optionId.Contains("reject")
                ? LotoObjectiveIds.EppDefect
                : LotoObjectiveIds.EppSelection,
            optionId = $"{optionId}_{type}",
            timestampUtc = DateTime.UtcNow.ToString("o"),
            success = success
        };

        PendingSyncEntry entry = new PendingSyncEntry
        {
            entryType = PendingSyncEntry.EntryTypeEvent,
            payloadJson = JsonUtility.ToJson(record)
        };
        ControlCenter.Instance.syncQueueManager.Enqueue(entry);
    }

    private void NotifyProgress()
    {
        OnProgressChanged?.Invoke(equippedMandatory.Count, MandatoryTypes.Length);
    }

    private void NotifyOrchestratorSubtitle(string key)
    {
        string translated = Translate(key);
        OnHintSubtitle?.Invoke(translated);
        if (ControlCenter.Instance != null && ControlCenter.Instance.simulationOrchestratorManager != null)
        {
            ControlCenter.Instance.simulationOrchestratorManager.NotifySubtitle(translated);
        }
    }

    private static string Translate(string key)
    {
        if (ControlCenter.Instance == null || ControlCenter.Instance.languageManager == null)
        {
            return key;
        }

        string translated = ControlCenter.Instance.languageManager.GetText(key);
        return string.IsNullOrEmpty(translated) ? key : translated;
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
