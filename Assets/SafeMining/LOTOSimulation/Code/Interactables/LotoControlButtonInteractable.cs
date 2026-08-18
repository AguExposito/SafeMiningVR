using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(ref_LotoControlButton))]
public class LotoControlButtonInteractable : MonoBehaviour
{
    [SerializeField] private LotoSequenceManager sequenceManager;

    private ref_LotoControlButton buttonRef;

    #region Lifecycle

    private void Awake()
    {
        buttonRef = GetComponent<ref_LotoControlButton>();
        if (buttonRef != null && buttonRef.interactable != null)
        {
            buttonRef.interactable.selectEntered.AddListener(OnSelected);
        }
    }

    private void OnDestroy()
    {
        if (buttonRef != null && buttonRef.interactable != null)
        {
            buttonRef.interactable.selectEntered.RemoveListener(OnSelected);
        }
    }

    #endregion

    #region Interaction

    public void Configure(LotoSequenceManager manager)
    {
        sequenceManager = manager;
    }

    private void OnSelected(SelectEnterEventArgs args)
    {
        if (sequenceManager == null || buttonRef == null)
        {
            return;
        }

        if (buttonRef.isStartButton)
        {
            sequenceManager.TryStartVerification();
            return;
        }

        sequenceManager.TryStopConveyor();
    }

    #endregion
}
