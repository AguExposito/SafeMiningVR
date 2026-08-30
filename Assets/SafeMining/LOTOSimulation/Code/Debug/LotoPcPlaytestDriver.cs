using UnityEngine;

public class LotoPcPlaytestDriver : MonoBehaviour
{
    [Header("PC Playtest")]
    [SerializeField] private bool enablePcCheats = true;
    [Tooltip("WASD mover, RMB mirar. Cheats Numpad: 1 briefing, 2 5/5 PPE, 3 continue S1, 4 reject, 5 hazards, 6 continue S2, 7 LOTO path")]
    [SerializeField] private bool enableHotkeys = true;

    [Header("Triggers (toggle in Play Mode)")]
    [SerializeField] private bool dismissBriefing;
    [SerializeField] private bool equipMandatoryFive;
    [SerializeField] private bool rejectOneDefect;
    [SerializeField] private bool continueS1ToS2;
    [SerializeField] private bool reportFourHazards;
    [SerializeField] private bool continueS2ToS3;
    [SerializeField] private bool runLotoHappyPath;
    [SerializeField] private bool forceNoGloves;
    [SerializeField] private bool forceIncompleteStart;
    [SerializeField] private bool confirmExit;
    [SerializeField] private bool completeSummary;

    private void Update()
    {
        if (!enablePcCheats || !Application.isPlaying)
        {
            return;
        }

        if (Consume(ref dismissBriefing))
        {
            DismissBriefing();
        }

        if (Consume(ref equipMandatoryFive))
        {
            EquipMandatoryFive();
        }

        if (Consume(ref rejectOneDefect))
        {
            RejectOneDefect();
        }

        if (Consume(ref continueS1ToS2))
        {
            ContinueS1();
        }

        if (Consume(ref reportFourHazards))
        {
            ReportFourHazards();
        }

        if (Consume(ref continueS2ToS3))
        {
            ContinueS2();
        }

        if (Consume(ref runLotoHappyPath))
        {
            RunLotoHappyPath();
        }

        if (Consume(ref forceNoGloves))
        {
            ForceNoGloves();
        }

        if (Consume(ref forceIncompleteStart))
        {
            ForceIncompleteStart();
        }

        if (Consume(ref confirmExit))
        {
            ConfirmExit();
        }

        if (Consume(ref completeSummary))
        {
            CompleteSummary();
        }

        if (enableHotkeys)
        {
            if (Input.GetKeyDown(KeyCode.Keypad1))
            {
                DismissBriefing();
            }

            if (Input.GetKeyDown(KeyCode.Keypad2))
            {
                EquipMandatoryFive();
            }

            if (Input.GetKeyDown(KeyCode.Keypad3))
            {
                ContinueS1();
            }

            if (Input.GetKeyDown(KeyCode.Keypad4))
            {
                RejectOneDefect();
            }

            if (Input.GetKeyDown(KeyCode.Keypad5))
            {
                ReportFourHazards();
            }

            if (Input.GetKeyDown(KeyCode.Keypad6))
            {
                ContinueS2();
            }

            if (Input.GetKeyDown(KeyCode.Keypad7))
            {
                RunLotoHappyPath();
            }
        }
    }

    private static bool Consume(ref bool flag)
    {
        if (!flag)
        {
            return false;
        }

        flag = false;
        return true;
    }

    [ContextMenu("LOTO/PC/Dismiss Briefing")]
    public void DismissBriefing()
    {
        SimulationOrchestratorManager orchestrator = GetOrchestrator();
        if (orchestrator != null)
        {
            orchestrator.DismissBriefing();
        }
    }

    [ContextMenu("LOTO/PC/Equip 5/5")]
    public void EquipMandatoryFive()
    {
        EppEquipmentManager manager = FindFirstObjectByType<EppEquipmentManager>();
        if (manager == null)
        {
            Debug.LogWarning("[LOTO-PC] EppEquipmentManager not found.");
            return;
        }

        EquipGood(manager, EppItemType.Helmet);
        EquipGood(manager, EppItemType.SafetyGlasses);
        EquipGood(manager, EppItemType.ReflectiveVest);
        EquipGood(manager, EppItemType.Gloves);
        EquipGood(manager, EppItemType.SafetyBoots);
    }

    [ContextMenu("LOTO/PC/Reject Defect")]
    public void RejectOneDefect()
    {
        EppEquipmentManager manager = FindFirstObjectByType<EppEquipmentManager>();
        if (manager == null)
        {
            return;
        }

        ref_EppItem[] items = FindObjectsByType<ref_EppItem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null
                && items[i].condition == EppItemCondition.Defective
                && !items[i].IsLocked)
            {
                manager.TryReject(items[i]);
                return;
            }
        }
    }

    [ContextMenu("LOTO/PC/Reject All Defects")]
    public void RejectAllDefects()
    {
        EppEquipmentManager manager = FindFirstObjectByType<EppEquipmentManager>();
        if (manager == null)
        {
            return;
        }

        ref_EppItem[] items = FindObjectsByType<ref_EppItem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null
                && items[i].condition == EppItemCondition.Defective
                && !items[i].IsLocked)
            {
                manager.TryReject(items[i]);
            }
        }
    }

    [ContextMenu("LOTO/PC/Continue S1→S2")]
    public void ContinueS1()
    {
        EppEquipmentManager manager = FindFirstObjectByType<EppEquipmentManager>();
        manager?.TryContinue();
    }

    [ContextMenu("LOTO/PC/Report 4 Hazards")]
    public void ReportFourHazards()
    {
        HazardInspectionManager manager = FindFirstObjectByType<HazardInspectionManager>();
        if (manager == null)
        {
            Debug.LogWarning("[LOTO-PC] HazardInspectionManager not found.");
            return;
        }

        ref_HazardSpot[] spots = FindObjectsByType<ref_HazardSpot>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < spots.Length; i++)
        {
            if (spots[i] != null && HazardTypeRules.IsRequiredHazard(spots[i].hazardType))
            {
                manager.TryReport(spots[i]);
            }
        }
    }

    [ContextMenu("LOTO/PC/Continue S2→S3")]
    public void ContinueS2()
    {
        HazardInspectionManager manager = FindFirstObjectByType<HazardInspectionManager>();
        manager?.TryContinue();
    }

    [ContextMenu("LOTO/PC/STOP→OFF→Lock→Tag→START")]
    public void RunLotoHappyPath()
    {
        LotoSequenceManager manager = FindFirstObjectByType<LotoSequenceManager>();
        if (manager == null)
        {
            Debug.LogWarning("[LOTO-PC] LotoSequenceManager not found.");
            return;
        }

        manager.TryStopConveyor();
        manager.TryToggleBreakerOff();

        ref_LotoLock personal = FindLock(LotoLockType.Personal);
        if (personal != null)
        {
            manager.TryApplyLock(personal.lockType, personal.gameObject);
        }

        ref_LotoTag danger = FindTag(LotoTagType.DangerDoNotOperate);
        if (danger != null)
        {
            manager.TryApplyTag(danger.tagType, danger.gameObject);
        }

        manager.TryStartVerification();
    }

    [ContextMenu("LOTO/PC/Force No Gloves")]
    public void ForceNoGloves()
    {
        if (ControlCenter.Instance != null &&
            ControlCenter.Instance.simulationSessionManager != null &&
            ControlCenter.Instance.simulationSessionManager.CurrentSession != null)
        {
            ControlCenter.Instance.simulationSessionManager.CurrentSession.hasGloves = false;
        }

        LotoSequenceManager manager = FindFirstObjectByType<LotoSequenceManager>();
        manager?.TryToggleBreakerOff();
    }

    [ContextMenu("LOTO/PC/Force Incomplete START")]
    public void ForceIncompleteStart()
    {
        LotoSequenceManager manager = FindFirstObjectByType<LotoSequenceManager>();
        if (manager == null)
        {
            return;
        }

        manager.TryStopConveyor();
        manager.TryStartVerification();
    }

    [ContextMenu("LOTO/PC/Confirm Exit")]
    public void ConfirmExit()
    {
        GetOrchestrator()?.ConfirmExit();
    }

    [ContextMenu("LOTO/PC/Complete Summary")]
    public void CompleteSummary()
    {
        SimulationOrchestratorManager orchestrator = GetOrchestrator();
        if (orchestrator == null)
        {
            return;
        }

        PlayerHudManager hud = FindFirstObjectByType<PlayerHudManager>();
        if (hud != null)
        {
            ref_PlayerHUD playerHud = FindFirstObjectByType<ref_PlayerHUD>();
            if (playerHud != null && playerHud.sessionSummaryContinueButton != null)
            {
                playerHud.sessionSummaryContinueButton.onClick.Invoke();
                return;
            }
        }

        orchestrator.CompleteSimulation();
    }

    private static void EquipGood(EppEquipmentManager manager, EppItemType type)
    {
        ref_EppItem item = FindItem(type, EppItemCondition.Good);
        if (item == null)
        {
            return;
        }

        manager.TryEquip(item, null);
    }

    private static ref_EppItem FindItem(EppItemType type, EppItemCondition condition)
    {
        ref_EppItem[] items = FindObjectsByType<ref_EppItem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null && items[i].itemType == type && items[i].condition == condition)
            {
                return items[i];
            }
        }

        return null;
    }

    private static ref_LotoLock FindLock(LotoLockType lockType)
    {
        ref_LotoLock[] locks = FindObjectsByType<ref_LotoLock>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < locks.Length; i++)
        {
            if (locks[i] != null && locks[i].lockType == lockType)
            {
                return locks[i];
            }
        }

        return null;
    }

    private static ref_LotoTag FindTag(LotoTagType tagType)
    {
        ref_LotoTag[] tags = FindObjectsByType<ref_LotoTag>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < tags.Length; i++)
        {
            if (tags[i] != null && tags[i].tagType == tagType)
            {
                return tags[i];
            }
        }

        return null;
    }

    private static SimulationOrchestratorManager GetOrchestrator()
    {
        if (ControlCenter.Instance != null && ControlCenter.Instance.simulationOrchestratorManager != null)
        {
            return ControlCenter.Instance.simulationOrchestratorManager;
        }

        return FindFirstObjectByType<SimulationOrchestratorManager>();
    }
}
