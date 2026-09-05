using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HazardInspectionManager : MonoBehaviour
{
    public event Action<int, int> OnProgressChanged;
    public event Action<string> OnHintSubtitle;

    private static readonly HazardType[] RequiredHazards =
    {
        HazardType.SpilledMaterial,
        HazardType.AbandonedTools,
        HazardType.FallenSignage,
        HazardType.ObstructedExtinguisher
    };

    [SerializeField] private LotoScenario2Controller scenarioController;

    private readonly HashSet<HazardType> reportedHazards = new HashSet<HazardType>();
    private readonly List<ref_HazardSpot> pendingRealSpots = new List<ref_HazardSpot>();

    private bool objectiveScored;
    private bool reportingLocked;
    private int falsePositiveCount;
    private Coroutine hintCoroutine;

    public int ReportedCount => reportedHazards.Count;
    public int RequiredTotal => RequiredHazards.Length;
    public int FalsePositiveCount => falsePositiveCount;

    public bool IsReadyToAdvance => reportedHazards.Count >= RequiredHazards.Length;
    public bool IsReportingLocked => reportingLocked;

    #region Public API

    public void Configure(LotoScenario2Controller controller)
    {
        scenarioController = controller;
    }

    public void Begin()
    {
        reportedHazards.Clear();
        pendingRealSpots.Clear();
        objectiveScored = false;
        reportingLocked = false;
        falsePositiveCount = 0;

        if (hintCoroutine != null)
        {
            StopCoroutine(hintCoroutine);
            hintCoroutine = null;
        }

        ref_HazardSpot[] spots = GetComponentsInChildren<ref_HazardSpot>(true);
        for (int i = 0; i < spots.Length; i++)
        {
            RegisterSpawnedSpot(spots[i]);
            if (spots[i] != null && spots[i].interactable != null && !spots[i].IsReported)
            {
                spots[i].interactable.enabled = true;
            }
        }

        NotifyProgress();
        NotifyOrchestratorSubtitle("loto_s2_intro");
        hintCoroutine = StartCoroutine(HintTimeoutRoutine());
        RefreshOutlineHints();
    }

    public void LockReporting()
    {
        if (reportingLocked)
        {
            return;
        }

        reportingLocked = true;

        if (hintCoroutine != null)
        {
            StopCoroutine(hintCoroutine);
            hintCoroutine = null;
        }

        LotoOutlineHint hint = GetComponent<LotoOutlineHint>();
        if (hint != null)
        {
            hint.Clear();
        }

        ref_HazardSpot[] spots = GetComponentsInChildren<ref_HazardSpot>(true);
        for (int i = 0; i < spots.Length; i++)
        {
            ref_HazardSpot spot = spots[i];
            if (spot == null)
            {
                continue;
            }

            if (spot.interactable != null)
            {
                spot.interactable.enabled = false;
            }
        }
    }

    public void RegisterSpawnedSpot(ref_HazardSpot spot)
    {
        if (spot == null)
        {
            return;
        }

        if (HazardTypeRules.IsRequiredHazard(spot.hazardType))
        {
            pendingRealSpots.Add(spot);
        }
    }

    public bool TryReport(ref_HazardSpot spot)
    {
        if (reportingLocked || spot == null || spot.IsReported)
        {
            return false;
        }

        if (HazardTypeRules.IsDecoy(spot.hazardType))
        {
            falsePositiveCount++;
            spot.SetReported(true);
            if (ControlCenter.Instance != null && ControlCenter.Instance.simulationSessionManager != null)
            {
                ControlCenter.Instance.simulationSessionManager.RecordHazardFalsePositive();
            }

            EmitEvent("hazard_false_positive", false, spot.hazardType);
            NotifyOrchestratorSubtitle("loto_s2_false_positive");
            LotoTrainingFeedback.PlayFail();
            return false;
        }

        if (reportedHazards.Contains(spot.hazardType))
        {
            return false;
        }

        reportedHazards.Add(spot.hazardType);
        pendingRealSpots.Remove(spot);
        spot.SetReported(true);
        EmitEvent("hazard_report", true, spot.hazardType);
        NotifyProgress();
        NotifyOrchestratorSubtitle(GetReportSubtitleKey(spot.hazardType));
        LotoTrainingFeedback.PlaySuccess();

        if (reportedHazards.Count >= RequiredHazards.Length)
        {
            ScoreObjective();
            NotifyOrchestratorSubtitle(falsePositiveCount > 0
                ? "loto_s2_complete_with_penalty"
                : "loto_s2_step_ready");
        }

        return true;
    }

    public void TryContinue()
    {
        if (!IsReadyToAdvance)
        {
            NotifyOrchestratorSubtitle("loto_s2_progress_hint");
            LotoTrainingFeedback.PlayFail();
            return;
        }

        NotifyOrchestratorSubtitle("loto_s2_roller_anomaly");
        if (scenarioController != null)
        {
            scenarioController.OnInspectionCompleted();
        }
    }

    #endregion

    #region Scoring

    private void ScoreObjective()
    {
        if (objectiveScored)
        {
            return;
        }

        objectiveScored = true;
        SetObjective(true);

        if (hintCoroutine != null)
        {
            StopCoroutine(hintCoroutine);
            hintCoroutine = null;
        }
    }

    private static void SetObjective(bool achieved)
    {
        Util_LotoSimulationBridge.SetObjective(LotoSimulationConstants.ObjectiveHazardReportIndex, achieved);
    }

    #endregion

    #region Hints

    private IEnumerator HintTimeoutRoutine()
    {
        yield return new WaitForSeconds(LotoSimulationConstants.HazardHintTimeoutSeconds);

        if (IsReadyToAdvance)
        {
            yield break;
        }

        NotifyOrchestratorSubtitle("loto_s2_timeout_hint");
        PulseUnreportedSpots();
    }

    private void PulseUnreportedSpots()
    {
        for (int i = 0; i < pendingRealSpots.Count; i++)
        {
            ref_HazardSpot spot = pendingRealSpots[i];
            if (spot != null && !spot.IsReported)
            {
                spot.PulseHint();
            }
        }
    }

    #endregion

    #region Events

    private void EmitEvent(string optionId, bool success, HazardType type)
    {
        SimulationEventRecord record = new SimulationEventRecord
        {
            sessionId = Util_LotoSimulationBridge.GetSessionId(),
            eventType = "hazard_inspection",
            objectiveId = LotoObjectiveIds.HazardReport,
            optionId = $"{optionId}_{type}",
            timestampUtc = DateTime.UtcNow.ToString("o"),
            success = success
        };

        Util_LotoSimulationBridge.EnqueueEvent(record);
    }

    private void NotifyProgress()
    {
        OnProgressChanged?.Invoke(reportedHazards.Count, RequiredHazards.Length);
        RefreshOutlineHints();
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
            LotoS2ContinueInteractable continueDoor = GetComponentInChildren<LotoS2ContinueInteractable>(true);
            if (continueDoor != null)
            {
                hint.Show(continueDoor.gameObject);
                return;
            }

            hint.Clear();
            return;
        }

        System.Collections.Generic.List<GameObject> targets = new System.Collections.Generic.List<GameObject>(4);
        for (int i = 0; i < pendingRealSpots.Count; i++)
        {
            if (pendingRealSpots[i] != null && !pendingRealSpots[i].IsReported)
            {
                targets.Add(pendingRealSpots[i].gameObject);
            }
        }

        hint.Show(targets);
    }

    private static string GetReportSubtitleKey(HazardType type)
    {
        switch (type)
        {
            case HazardType.SpilledMaterial:
                return "loto_s2_report_spill";
            case HazardType.AbandonedTools:
                return "loto_s2_report_tools";
            case HazardType.FallenSignage:
                return "loto_s2_report_signage";
            case HazardType.ObstructedExtinguisher:
                return "loto_s2_report_extinguisher";
            default:
                return "loto_s2_report_generic";
        }
    }

    private void NotifyOrchestratorSubtitle(string key)
    {
        bool isError = key == "loto_s2_false_positive";
        Util_LotoSimulationBridge.NotifySubtitle(key, OnHintSubtitle, isError);
    }

    #endregion
}
