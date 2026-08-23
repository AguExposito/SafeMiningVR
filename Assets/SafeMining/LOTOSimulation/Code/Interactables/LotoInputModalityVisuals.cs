using System;
using System.Collections;
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

    public static bool IsHandsMode { get; private set; } = true;

    private void Awake()
    {
        if (modalityManager == null)
        {
            modalityManager = GetComponent<XRInputModalityManager>();
        }

        ApplyHands(true);
    }

    private void OnEnable()
    {
        if (modalityManager == null)
        {
            return;
        }

        modalityManager.trackedHandModeStarted.AddListener(OnHandModeStarted);
        modalityManager.motionControllerModeStarted.AddListener(OnControllerModeStarted);
    }

    private void OnDisable()
    {
        if (modalityManager != null)
        {
            modalityManager.trackedHandModeStarted.RemoveListener(OnHandModeStarted);
            modalityManager.motionControllerModeStarted.RemoveListener(OnControllerModeStarted);
        }

        CancelPendingHands();
    }

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

    private IEnumerator ReturnToHandsAfterDelay()
    {
        yield return new WaitForSeconds(handsReturnDelay);
        pendingHandsReturn = null;
        ApplyHands(true);
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
}
