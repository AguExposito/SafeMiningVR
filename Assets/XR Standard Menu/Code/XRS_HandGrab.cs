using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[AddComponentMenu("XR Standard Menu/XRS Hand Grab")]
public class XRS_HandGrab : MonoBehaviour
{
    #region Hand Settings

    [Header("Hand Settings")]
    [SerializeField] XRSHandedness handedness = XRSHandedness.Right;

    [Header("Thresholds")]
    [SerializeField] float closeThreshold = 0.65f;
    [SerializeField] float openThreshold = 0.45f;

    [Header("Events")]
    [SerializeField] UnityEvent onHandClosed;
    [SerializeField] UnityEvent onHandOpened;

    #endregion

    #region Grab Settings

    [Header("Grab Settings")]
    [SerializeField] NearFarInteractor nearFarInteractor;
    [SerializeField] Transform grabAttachTransform;
    [SerializeField] bool useLayerGating = true;
    [SerializeField] InteractionLayerMask xrsExclusiveGrabLayers;
    [SerializeField] bool closedHandUsesOnlyExclusiveLayers = true;
    [SerializeField] bool manualGrabOnlyExclusiveLayers = true;
    [SerializeField] float maxManualGrabDistance = 0.2f;

    #endregion

    #region Runtime State

    readonly List<IXRInteractable> validTargets = new();
    XRBaseInteractor xrBaseInteractor;
    Transform originalAttachTransform;
    bool attachTransformOverridden;
    InteractionLayerMask originalInteractorLayers;
    bool interactionLayersCached;
    bool originalFarCastingState;
    bool farCastingStateCached;
    bool pendingManualGrabRequest;
    int framesUntilManualGrab;
    IXRSelectInteractable gestureSelectedInteractable;
    bool previousClosedState;
    bool isHandClosed;
    float currentFistScore;

    #endregion

    #region Unity Messages

    void Awake()
    {
        InitializeReferences();
    }

    void OnEnable()
    {
        InitializeReferences();
        ApplyOpenInteractorLayers();
        ApplyFarCastingState();
    }

    void OnDisable()
    {
        TryReleaseGestureSelection();
        RestoreAttachTransformIfNeeded();
        RestoreInteractorLayersIfNeeded();
        RestoreFarCastingStateIfNeeded();
    }

    void Update()
    {
        if (!TryUpdateHandState())
            return;

        if (!previousClosedState && isHandClosed)
        {
            ApplyClosedInteractorLayers();
            pendingManualGrabRequest = true;
            framesUntilManualGrab = 1;
            onHandClosed?.Invoke();
        }
        else if (previousClosedState && !isHandClosed)
        {
            pendingManualGrabRequest = false;
            TryReleaseGestureSelection();
            ApplyOpenInteractorLayers();
            onHandOpened?.Invoke();
        }

        if (pendingManualGrabRequest && isHandClosed)
        {
            if (framesUntilManualGrab > 0)
                framesUntilManualGrab--;
            else
            {
                TryStartManualInteraction();
                pendingManualGrabRequest = false;
            }
        }

        previousClosedState = isHandClosed;
        ValidateGestureSelectionState();
    }

    #endregion

    #region Initialization

    void InitializeReferences()
    {
        if (nearFarInteractor == null)
            nearFarInteractor = GetComponent<NearFarInteractor>();

        if (nearFarInteractor == null)
            nearFarInteractor = GetComponentInChildren<NearFarInteractor>();

        xrBaseInteractor = nearFarInteractor as XRBaseInteractor;
        CacheOriginalInteractorLayersIfNeeded();

        if (grabAttachTransform == null)
            grabAttachTransform = transform;
    }

    #endregion

    #region Hand Tracking

    bool TryUpdateHandState()
    {
        if (!Util_HandGesture.TryGetHandSubsystem(out XRHandSubsystem handSubsystem))
            return false;

        Handedness xrHandedness = handedness == XRSHandedness.Left ? Handedness.Left : Handedness.Right;
        if (!Util_HandGesture.TryGetHand(handSubsystem, xrHandedness, out XRHand hand))
            return false;

        if (!Util_HandGesture.TryGetFistScore(hand, out float fistScore))
            return false;

        currentFistScore = fistScore;
        isHandClosed = Util_HandGesture.EvaluateClosedState(currentFistScore, isHandClosed, closeThreshold, openThreshold);
        return true;
    }

    #endregion

    #region Manual Grab

    void TryStartManualInteraction()
    {
        if (nearFarInteractor == null || xrBaseInteractor == null)
            return;

        if (gestureSelectedInteractable != null || nearFarInteractor.hasSelection || xrBaseInteractor.interactionManager == null)
            return;

        OverrideAttachTransformIfNeeded();

        validTargets.Clear();
        xrBaseInteractor.interactionManager.GetValidTargets(nearFarInteractor, validTargets);

        for (int i = 0; i < validTargets.Count; i++)
        {
            if (validTargets[i] is not IXRSelectInteractable selectInteractable)
                continue;

            if (manualGrabOnlyExclusiveLayers && !HasLayerOverlap(selectInteractable.interactionLayers, xrsExclusiveGrabLayers))
                continue;

            if (maxManualGrabDistance > 0f &&
                selectInteractable.GetDistanceSqrToInteractor(nearFarInteractor) > (maxManualGrabDistance * maxManualGrabDistance))
                continue;

            if (!nearFarInteractor.CanSelect(selectInteractable))
                continue;

            xrBaseInteractor.interactionManager.SelectEnter(nearFarInteractor, selectInteractable);
            if (nearFarInteractor.IsSelecting(selectInteractable))
            {
                gestureSelectedInteractable = selectInteractable;
                return;
            }

            RestoreAttachTransformIfNeeded();
            return;
        }

        RestoreAttachTransformIfNeeded();
    }

    void TryReleaseGestureSelection()
    {
        if (xrBaseInteractor == null || xrBaseInteractor.interactionManager == null || gestureSelectedInteractable == null)
            return;

        if (nearFarInteractor.IsSelecting(gestureSelectedInteractable))
            xrBaseInteractor.interactionManager.SelectExit(nearFarInteractor, gestureSelectedInteractable);

        gestureSelectedInteractable = null;
        RestoreAttachTransformIfNeeded();
    }

    void ValidateGestureSelectionState()
    {
        if (gestureSelectedInteractable == null || nearFarInteractor == null)
            return;

        if (nearFarInteractor.IsSelecting(gestureSelectedInteractable))
            return;

        gestureSelectedInteractable = null;
        RestoreAttachTransformIfNeeded();
    }

    void OverrideAttachTransformIfNeeded()
    {
        if (xrBaseInteractor == null || grabAttachTransform == null || attachTransformOverridden)
            return;

        originalAttachTransform = xrBaseInteractor.attachTransform;
        xrBaseInteractor.attachTransform = grabAttachTransform;
        attachTransformOverridden = true;
    }

    void RestoreAttachTransformIfNeeded()
    {
        if (xrBaseInteractor == null || !attachTransformOverridden)
            return;

        xrBaseInteractor.attachTransform = originalAttachTransform;
        attachTransformOverridden = false;
    }

    void CacheOriginalInteractorLayersIfNeeded()
    {
        if (nearFarInteractor == null || interactionLayersCached)
            return;

        originalInteractorLayers = nearFarInteractor.interactionLayers;
        interactionLayersCached = true;
    }

    void ApplyOpenInteractorLayers()
    {
        if (!useLayerGating || nearFarInteractor == null)
            return;

        CacheOriginalInteractorLayersIfNeeded();
        nearFarInteractor.interactionLayers = ((int)originalInteractorLayers & ~(int)xrsExclusiveGrabLayers);
    }

    void ApplyClosedInteractorLayers()
    {
        if (!useLayerGating || nearFarInteractor == null)
            return;

        CacheOriginalInteractorLayersIfNeeded();
        if (closedHandUsesOnlyExclusiveLayers)
            nearFarInteractor.interactionLayers = xrsExclusiveGrabLayers;
        else
            nearFarInteractor.interactionLayers = ((int)originalInteractorLayers | (int)xrsExclusiveGrabLayers);
    }

    void RestoreInteractorLayersIfNeeded()
    {
        if (!interactionLayersCached || nearFarInteractor == null)
            return;

        nearFarInteractor.interactionLayers = originalInteractorLayers;
    }

    void ApplyFarCastingState()
    {
        if (nearFarInteractor == null)
            return;

        if (!farCastingStateCached)
        {
            originalFarCastingState = nearFarInteractor.enableFarCasting;
            farCastingStateCached = true;
        }
    }

    void RestoreFarCastingStateIfNeeded()
    {
        if (!farCastingStateCached || nearFarInteractor == null)
            return;

        nearFarInteractor.enableFarCasting = originalFarCastingState;
    }

    bool HasLayerOverlap(InteractionLayerMask first, InteractionLayerMask second)
    {
        return ((int)first & (int)second) != 0;
    }

    #endregion

    #region Public API

    public bool IsHandClosed => isHandClosed;
    public float CurrentFistScore => currentFistScore;

    public void SetHandedness(XRSHandedness targetHandedness)
    {
        handedness = targetHandedness;
    }

    #endregion
}

public enum XRSHandedness
{
    Left = 0,
    Right = 1
}
