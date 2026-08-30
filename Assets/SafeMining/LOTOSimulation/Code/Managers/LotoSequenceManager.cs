using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class LotoSequenceManager : MonoBehaviour
{
    public event Action<LotoSequenceStep> OnStepChanged;
    public event Action<string> OnHintSubtitle;
    public event Action<bool> OnSequenceCompleted;

    [SerializeField] private BeltAnimator beltAnimator;
    [SerializeField] private Transform lockSnapAnchor;
    [SerializeField] private Transform tagSnapAnchor;
    [SerializeField] private XRSocketInteractor lockSocket;

    private LotoSequenceStep currentStep = LotoSequenceStep.BeltRunning;
    private bool sequenceObjectiveScored;
    private bool zeroEnergyObjectiveScored;
    private GameObject placedLock;
    private GameObject placedTag;
    private XRSocketInteractor activeTagSocket;

    public LotoSequenceStep CurrentStep => currentStep;
    public Transform LockSnapAnchor => lockSnapAnchor;
    public Transform TagSnapAnchor => tagSnapAnchor;
    public bool HasPlacedLock => placedLock != null;
    public bool HasPlacedTag => placedTag != null;

    #region Lifecycle

    private void OnEnable()
    {
        if (lockSocket != null)
        {
            lockSocket.selectEntered.AddListener(OnLockSocketed);
        }
    }

    private void OnDisable()
    {
        if (lockSocket != null)
        {
            lockSocket.selectEntered.RemoveListener(OnLockSocketed);
        }

        UnbindTagSocket();
    }

    #endregion

    #region Public API

    public void Configure(BeltAnimator animator, Transform lockAnchor, Transform tagAnchor)
    {
        Configure(animator, lockAnchor, tagAnchor, lockSocket);
    }

    public void Configure(BeltAnimator animator, Transform lockAnchor, Transform tagAnchor, XRSocketInteractor socket)
    {
        beltAnimator = animator;
        lockSnapAnchor = lockAnchor;
        tagSnapAnchor = tagAnchor;
        lockSocket = socket;
    }

    public void Begin()
    {
        currentStep = LotoSequenceStep.BeltRunning;
        sequenceObjectiveScored = false;
        zeroEnergyObjectiveScored = false;
        ClearPlacedProps();
        UnbindTagSocket();
        SetLockSocketActive(false);
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

    public bool TryToggleBreakerOff()
    {
        if (currentStep == LotoSequenceStep.BeltRunning)
        {
            Reject("loto_s3_reject_order", "breaker_before_stop");
            return false;
        }

        if (currentStep != LotoSequenceStep.BeltStopped)
        {
            Reject("loto_s3_reject_order", "breaker_invalid_state");
            return false;
        }

        WarnMissingGlovesIfNeeded();

        currentStep = LotoSequenceStep.EnergyIsolated;
        SetLockSocketActive(true);
        EmitEvent("breaker_off", true);
        NotifyStep(LotoSequenceStep.EnergyIsolated, "loto_s3_step_lock");
        return true;
    }

    public bool HasSessionGloves()
    {
        if (ControlCenter.Instance == null ||
            ControlCenter.Instance.simulationSessionManager == null ||
            ControlCenter.Instance.simulationSessionManager.CurrentSession == null)
        {
            return true;
        }

        return ControlCenter.Instance.simulationSessionManager.CurrentSession.hasGloves;
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

        if (lockObject == null)
        {
            return false;
        }

        if (lockSnapAnchor != null && (lockSocket == null || !lockSocket.hasSelection))
        {
            SnapToAnchor(lockObject, lockSnapAnchor);
        }

        placedLock = lockObject;
        DisableGrab(lockObject);
        currentStep = LotoSequenceStep.LockApplied;
        EmitEvent("lock_personal", true);
        NotifyStep(LotoSequenceStep.LockApplied, "loto_s3_step_tag");

        ref_LotoLock lockRef = lockObject.GetComponent<ref_LotoLock>();
        if (lockRef != null && lockRef.tagSocket != null)
        {
            tagSnapAnchor = lockRef.tagSocket.transform;
            BindTagSocket(lockRef.tagSocket);
        }

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

        if (activeTagSocket == null || !activeTagSocket.hasSelection)
        {
            SnapToAnchor(tagObject, anchor);
        }

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
            Util_LotoSimulationBridge.NotifySubtitle("loto_s3_alert_incomplete", OnHintSubtitle);
            if (beltAnimator != null)
            {
                beltAnimator.PlayFailedStartAttempt();
            }

            LotoTrainingFeedback.PlayFail();
            ScoreZeroEnergyObjective(false);
            return;
        }

        SetBeltRunning(false);
        currentStep = LotoSequenceStep.ZeroEnergyVerified;
        ScoreZeroEnergyObjective(true);
        EmitEvent("zero_energy_verified", true);
        Util_LotoSimulationBridge.NotifySubtitle("loto_s3_success_zero_energy", OnHintSubtitle);
        LotoTrainingFeedback.PlaySuccess();
        OnStepChanged?.Invoke(currentStep);
        OnSequenceCompleted?.Invoke(true);
        RefreshOutlineHints(LotoSequenceStep.ZeroEnergyVerified);
    }

    #endregion

    #region Sockets

    private void OnLockSocketed(SelectEnterEventArgs args)
    {
        if (args.interactableObject == null)
        {
            return;
        }

        Transform target = args.interactableObject.transform;
        ref_LotoLock lockRef = target.GetComponent<ref_LotoLock>();
        if (lockRef == null || !TryApplyLock(lockRef.lockType, target.gameObject))
        {
            Eject(lockSocket, args.interactableObject);
        }
    }

    private void OnTagSocketed(SelectEnterEventArgs args)
    {
        if (args.interactableObject == null)
        {
            return;
        }

        Transform target = args.interactableObject.transform;
        ref_LotoTag tagRef = target.GetComponent<ref_LotoTag>();
        if (tagRef == null || !TryApplyTag(tagRef.tagType, target.gameObject))
        {
            Eject(activeTagSocket, args.interactableObject);
        }
    }

    private void SetLockSocketActive(bool active)
    {
        if (lockSocket != null)
        {
            lockSocket.socketActive = active;
        }
    }

    private void BindTagSocket(XRSocketInteractor socket)
    {
        UnbindTagSocket();
        activeTagSocket = socket;
        if (activeTagSocket == null)
        {
            return;
        }

        activeTagSocket.selectEntered.AddListener(OnTagSocketed);
        activeTagSocket.socketActive = true;
    }

    private void UnbindTagSocket()
    {
        if (activeTagSocket == null)
        {
            return;
        }

        activeTagSocket.selectEntered.RemoveListener(OnTagSocketed);
        activeTagSocket.socketActive = false;
        activeTagSocket = null;
    }

    private static void Eject(XRSocketInteractor socket, IXRSelectInteractable interactable)
    {
        if (socket == null || interactable == null || socket.interactionManager == null)
        {
            return;
        }

        socket.interactionManager.SelectExit(socket, interactable);
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
        Util_LotoSimulationBridge.SetObjective(objectiveIndex, achieved);
    }

    #endregion

    #region Events

    private void EmitEvent(string optionId, bool success)
    {
        SimulationEventRecord record = new SimulationEventRecord
        {
            sessionId = Util_LotoSimulationBridge.GetSessionId(),
            eventType = "loto_step",
            objectiveId = LotoObjectiveIds.LotoSequence,
            optionId = optionId,
            timestampUtc = DateTime.UtcNow.ToString("o"),
            success = success
        };

        Util_LotoSimulationBridge.EnqueueEvent(record);
    }

    private void Reject(string subtitleKey, string optionId)
    {
        EmitEvent(optionId, false);
        Util_LotoSimulationBridge.NotifySubtitle(subtitleKey, OnHintSubtitle);
        LotoTrainingFeedback.PlayFail();
    }

    private void WarnMissingGlovesIfNeeded()
    {
        if (HasSessionGloves())
        {
            return;
        }

        EmitEvent("breaker_no_gloves", false);
        Util_LotoSimulationBridge.NotifySubtitle("loto_s3_alert_no_gloves", OnHintSubtitle);
        LotoTrainingFeedback.PlayFail();
    }

    #endregion

    #region Helpers

    private void NotifyStep(LotoSequenceStep step, string subtitleKey)
    {
        OnStepChanged?.Invoke(step);
        Util_LotoSimulationBridge.NotifySubtitle(subtitleKey, OnHintSubtitle);
        RefreshOutlineHints(step);
        LotoTrainingFeedback.PlaySuccess();
    }

    private void RefreshOutlineHints(LotoSequenceStep step)
    {
        LotoOutlineHint hint = GetComponent<LotoOutlineHint>();
        if (hint == null)
        {
            hint = gameObject.AddComponent<LotoOutlineHint>();
        }

        System.Collections.Generic.List<GameObject> targets = new System.Collections.Generic.List<GameObject>(8);
        switch (step)
        {
            case LotoSequenceStep.BeltRunning:
                CollectControlButtons(targets, false);
                break;
            case LotoSequenceStep.BeltStopped:
                CollectBreakers(targets);
                break;
            case LotoSequenceStep.EnergyIsolated:
                CollectLocks(targets, LotoLockType.Personal);
                break;
            case LotoSequenceStep.LockApplied:
                CollectTags(targets, LotoTagType.DangerDoNotOperate);
                break;
            case LotoSequenceStep.TagApplied:
                CollectControlButtons(targets, true);
                break;
            default:
                hint.Clear();
                return;
        }

        hint.Show(targets);
    }

    private void CollectControlButtons(System.Collections.Generic.List<GameObject> targets, bool startButtons)
    {
        ref_LotoControlButton[] buttons = GetComponentsInChildren<ref_LotoControlButton>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null && buttons[i].isStartButton == startButtons)
            {
                targets.Add(buttons[i].gameObject);
            }
        }
    }

    private void CollectBreakers(System.Collections.Generic.List<GameObject> targets)
    {
        ref_LotoBreaker[] breakers = GetComponentsInChildren<ref_LotoBreaker>(true);
        for (int i = 0; i < breakers.Length; i++)
        {
            if (breakers[i] != null)
            {
                targets.Add(breakers[i].gameObject);
            }
        }
    }

    private void CollectLocks(System.Collections.Generic.List<GameObject> targets, LotoLockType lockType)
    {
        ref_LotoLock[] locks = GetComponentsInChildren<ref_LotoLock>(true);
        for (int i = 0; i < locks.Length; i++)
        {
            if (locks[i] != null && locks[i].lockType == lockType)
            {
                targets.Add(locks[i].gameObject);
            }
        }
    }

    private void CollectTags(System.Collections.Generic.List<GameObject> targets, LotoTagType tagType)
    {
        ref_LotoTag[] tags = GetComponentsInChildren<ref_LotoTag>(true);
        for (int i = 0; i < tags.Length; i++)
        {
            if (tags[i] != null && tags[i].tagType == tagType)
            {
                targets.Add(tags[i].gameObject);
            }
        }
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
        XRGrabInteractable grab = target.GetComponent<XRGrabInteractable>();
        if (grab != null)
        {
            grab.enabled = false;
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
