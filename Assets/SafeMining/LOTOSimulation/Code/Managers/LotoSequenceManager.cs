using System;
using UnityEngine;

public class LotoSequenceManager : MonoBehaviour
{
    public event Action<LotoSequenceStep> OnStepChanged;
    public event Action<string> OnHintSubtitle;

    [SerializeField] private BeltAnimator beltAnimator;
    [SerializeField] private Transform lockSnapAnchor;
    [SerializeField] private Transform tagSnapAnchor;

    private LotoSequenceStep currentStep = LotoSequenceStep.BeltRunning;
    private bool sequenceObjectiveScored;
    private bool zeroEnergyObjectiveScored;
    private GameObject placedLock;
    private GameObject placedTag;

    public LotoSequenceStep CurrentStep => currentStep;
    public Transform LockSnapAnchor => lockSnapAnchor;
    public Transform TagSnapAnchor => tagSnapAnchor;
    public bool HasPlacedLock => placedLock != null;
    public bool HasPlacedTag => placedTag != null;

    #region Public API

    public void Configure(BeltAnimator animator, Transform lockAnchor, Transform tagAnchor)
    {
        beltAnimator = animator;
        lockSnapAnchor = lockAnchor;
        tagSnapAnchor = tagAnchor;
    }

    public void Begin()
    {
        currentStep = LotoSequenceStep.BeltRunning;
        sequenceObjectiveScored = false;
        zeroEnergyObjectiveScored = false;
        ClearPlacedProps();
        SetBeltRunning(true);
        NotifyStep(LotoSequenceStep.BeltRunning, "loto_s3_intro");
    }

    public void TryStopConveyor()
    {
        if (currentStep != LotoSequenceStep.BeltRunning)
        {
            Reject("loto_s3_reject_order", "stop_early");
            return;
        }

        currentStep = LotoSequenceStep.BeltStopped;
        SetBeltRunning(false);
        EmitEvent("stop_conveyor", true);
        NotifyStep(LotoSequenceStep.BeltStopped, "loto_s3_step_isolate");
    }

    public void TryToggleBreakerOff()
    {
        if (currentStep == LotoSequenceStep.BeltRunning)
        {
            Reject("loto_s3_reject_order", "breaker_before_stop");
            return;
        }

        if (currentStep != LotoSequenceStep.BeltStopped)
        {
            Reject("loto_s3_reject_order", "breaker_invalid_state");
            return;
        }

        currentStep = LotoSequenceStep.EnergyIsolated;
        EmitEvent("breaker_off", true);
        NotifyStep(LotoSequenceStep.EnergyIsolated, "loto_s3_step_lock");
    }

    public bool TryApplyLock(LotoLockType lockType, GameObject lockObject)
    {
        if (currentStep != LotoSequenceStep.EnergyIsolated)
        {
            Reject("loto_s3_reject_order", "lock_wrong_step");
            return false;
        }

        if (lockType != LotoLockType.Personal)
        {
            Reject("loto_s3_reject_wrong_lock", lockType == LotoLockType.Visitor ? "lock_visitor" : "lock_group");
            return false;
        }

        if (lockSnapAnchor == null || lockObject == null)
        {
            return false;
        }

        SnapToAnchor(lockObject, lockSnapAnchor);
        placedLock = lockObject;
        DisableGrab(lockObject);
        currentStep = LotoSequenceStep.LockApplied;
        EmitEvent("lock_personal", true);
        NotifyStep(LotoSequenceStep.LockApplied, "loto_s3_step_tag");
        return true;
    }

    public bool TryApplyTag(LotoTagType tagType, GameObject tagObject)
    {
        if (currentStep != LotoSequenceStep.LockApplied)
        {
            Reject("loto_s3_reject_order", "tag_wrong_step");
            return false;
        }

        if (tagType != LotoTagType.DangerDoNotOperate)
        {
            Reject("loto_s3_reject_wrong_tag", "tag_wrong_type");
            return false;
        }

        Transform anchor = tagSnapAnchor != null ? tagSnapAnchor : lockSnapAnchor;
        if (anchor == null || tagObject == null)
        {
            return false;
        }

        SnapToAnchor(tagObject, anchor);
        placedTag = tagObject;
        DisableGrab(tagObject);
        currentStep = LotoSequenceStep.TagApplied;
        ScoreSequenceObjective();
        EmitEvent("tag_danger", true);
        NotifyStep(LotoSequenceStep.TagApplied, "loto_s3_step_verify");
        return true;
    }

    public void TryStartVerification()
    {
        if (currentStep == LotoSequenceStep.BeltRunning)
        {
            Reject("loto_s3_reject_order", "start_before_stop");
            return;
        }

        if (currentStep != LotoSequenceStep.TagApplied)
        {
            EmitEvent("start_incomplete", false);
            OnHintSubtitle?.Invoke(Translate("loto_s3_alert_incomplete"));
            if (beltAnimator != null)
            {
                beltAnimator.PlayFailedStartAttempt();
            }

            NotifyOrchestratorSubtitle("loto_s3_alert_incomplete");
            ScoreZeroEnergyObjective(false);
            return;
        }

        SetBeltRunning(false);
        currentStep = LotoSequenceStep.ZeroEnergyVerified;
        ScoreZeroEnergyObjective(true);
        EmitEvent("zero_energy_verified", true);
        OnHintSubtitle?.Invoke(Translate("loto_s3_success_zero_energy"));
        NotifyOrchestratorSubtitle("loto_s3_success_zero_energy");
        OnStepChanged?.Invoke(currentStep);

        LotoScenario3Controller scenario = FindFirstObjectByType<LotoScenario3Controller>();
        if (scenario != null)
        {
            scenario.OnSequenceCompleted(true);
        }
    }

    #endregion

    #region Scoring

    private void ScoreSequenceObjective()
    {
        if (sequenceObjectiveScored)
        {
            return;
        }

        sequenceObjectiveScored = true;
        SetObjective(LotoSimulationConstants.ObjectiveLotoSequenceIndex, true);
    }

    private void ScoreZeroEnergyObjective(bool achieved)
    {
        if (zeroEnergyObjectiveScored && achieved)
        {
            return;
        }

        zeroEnergyObjectiveScored = achieved;
        SetObjective(LotoSimulationConstants.ObjectiveZeroEnergyIndex, achieved);
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

    #region Events

    private void EmitEvent(string optionId, bool success)
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
            eventType = "loto_step",
            objectiveId = LotoObjectiveIds.LotoSequence,
            optionId = optionId,
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

    private void Reject(string subtitleKey, string optionId)
    {
        EmitEvent(optionId, false);
        NotifyOrchestratorSubtitle(subtitleKey);
    }

    #endregion

    #region Helpers

    private void NotifyStep(LotoSequenceStep step, string subtitleKey)
    {
        OnStepChanged?.Invoke(step);
        NotifyOrchestratorSubtitle(subtitleKey);
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

    private void SetBeltRunning(bool running)
    {
        if (beltAnimator != null)
        {
            beltAnimator.SetRunning(running);
        }
    }

    private static void SnapToAnchor(GameObject target, Transform anchor)
    {
        target.transform.SetPositionAndRotation(anchor.position, anchor.rotation);
        target.transform.SetParent(anchor, true);
    }

    private static void DisableGrab(GameObject target)
    {
        Collider[] colliders = target.GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }

        Rigidbody body = target.GetComponent<Rigidbody>();
        if (body != null)
        {
            body.isKinematic = true;
        }
    }

    private void ClearPlacedProps()
    {
        placedLock = null;
        placedTag = null;
    }

    #endregion
}
