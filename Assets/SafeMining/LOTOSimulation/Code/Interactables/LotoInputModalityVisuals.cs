using System;
using System.Collections;
using Unity.XR.CoreUtils.Bindings;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class LotoInputModalityVisuals : MonoBehaviour
{
    public static event Action<bool> OnHandsModeChanged;

    [SerializeField] private XRInputModalityManager modalityManager;
    [SerializeField] private GameObject leftHand;
    [SerializeField] private GameObject rightHand;
    [SerializeField] private GameObject leftController;
    [SerializeField] private GameObject rightController;
    [SerializeField] private float handsReturnDelay = 0.6f;

    private Coroutine pendingHandsReturn;
    private bool handsMode = true;
    private IEventBinding inputModeBinding;

    public static bool IsHandsMode { get; private set; } = true;

    #region Unity Messages

    private void Awake()
    {
        if (modalityManager == null)
        {
            modalityManager = GetComponent<XRInputModalityManager>();
        }
    }

    private void OnEnable()
    {
        if (modalityManager == null)
        {
            return;
        }

        modalityManager.trackedHandModeStarted.AddListener(OnHandModeStarted);
        modalityManager.motionControllerModeStarted.AddListener(OnControllerModeStarted);
        inputModeBinding = XRInputModalityManager.currentInputMode.SubscribeAndUpdate(OnInputModeChanged);
    }

    private void OnDisable()
    {
        if (modalityManager != null)
        {
            modalityManager.trackedHandModeStarted.RemoveListener(OnHandModeStarted);
            modalityManager.motionControllerModeStarted.RemoveListener(OnControllerModeStarted);
        }

        inputModeBinding?.Unbind();
        inputModeBinding = null;
        CancelPendingHands();
    }

    #endregion

    #region Event Handlers

    private void OnControllerModeStarted()
    {
        CancelPendingHands();
        ApplyHands(false);
    }

    private void OnHandModeStarted()
    {
        CancelPendingHands();
        pendingHandsReturn = StartCoroutine(ReturnToHandsAfterDelay());
    }

    private void OnInputModeChanged(XRInputModalityManager.InputMode mode)
    {
        switch (mode)
        {
            case XRInputModalityManager.InputMode.MotionController:
                CancelPendingHands();
                ApplyHands(false);
                break;
            case XRInputModalityManager.InputMode.TrackedHand:
                if (!handsMode)
                {
                    OnHandModeStarted();
                }
                else
                {
                    CancelPendingHands();
                    ApplyHands(true);
                }

                break;
            default:
                CancelPendingHands();
                ApplyNone();
                break;
        }
    }

    #endregion

    #region Modality Sync

    private IEnumerator ReturnToHandsAfterDelay()
    {
        yield return new WaitForSeconds(handsReturnDelay);
        pendingHandsReturn = null;
        if (XRInputModalityManager.currentInputMode.Value != XRInputModalityManager.InputMode.MotionController)
        {
            ApplyHands(true);
        }
    }

    private void ApplyHands(bool useHands)
    {
        handsMode = useHands;
        IsHandsMode = useHands;

        SetActive(leftHand, useHands);
        SetActive(rightHand, useHands);
        SetActive(leftController, !useHands);
        SetActive(rightController, !useHands);

        OnHandsModeChanged?.Invoke(useHands);
    }

    private void ApplyNone()
    {
        handsMode = true;
        IsHandsMode = true;
        SetActive(leftHand, false);
        SetActive(rightHand, false);
        SetActive(leftController, false);
        SetActive(rightController, false);
        OnHandsModeChanged?.Invoke(true);
    }

    private void CancelPendingHands()
    {
        if (pendingHandsReturn != null)
        {
            StopCoroutine(pendingHandsReturn);
            pendingHandsReturn = null;
        }
    }

    private static void SetActive(GameObject target, bool active)
    {
        if (target != null && target.activeSelf != active)
        {
            target.SetActive(active);
        }
    }

    #endregion
}
