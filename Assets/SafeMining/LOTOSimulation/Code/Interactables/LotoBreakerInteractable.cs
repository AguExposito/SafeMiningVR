using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(ref_LotoBreaker))]
public class LotoBreakerInteractable : MonoBehaviour
{
    [SerializeField] private LotoSequenceManager sequenceManager;
    [SerializeField] private Color onColor = new Color(0.2f, 0.8f, 0.2f);
    [SerializeField] private Color offColor = new Color(0.8f, 0.2f, 0.2f);

    private ref_LotoBreaker breakerRef;

    #region Lifecycle

    private void Awake()
    {
        breakerRef = GetComponent<ref_LotoBreaker>();
        if (breakerRef != null && breakerRef.interactable != null)
        {
            breakerRef.interactable.selectEntered.AddListener(OnSelected);
        }

        ApplyVisual();
    }

    private void OnDestroy()
    {
        if (breakerRef != null && breakerRef.interactable != null)
        {
            breakerRef.interactable.selectEntered.RemoveListener(OnSelected);
        }
    }

    #endregion

    #region Interaction

    public void Configure(LotoSequenceManager manager)
    {
        sequenceManager = manager;
    }

    public void SetInteractableEnabled(bool enabled)
    {
        if (breakerRef != null && breakerRef.interactable != null)
        {
            breakerRef.interactable.enabled = enabled;
        }
    }

    private void OnSelected(SelectEnterEventArgs args)
    {
        if (sequenceManager == null || breakerRef == null || breakerRef.isOff)
        {
            return;
        }

        if (!sequenceManager.TryToggleBreakerOff())
        {
            return;
        }

        breakerRef.isOff = true;
        ApplyVisual();
    }

    private void ApplyVisual()
    {
        if (breakerRef != null && breakerRef.breakerRenderer != null)
        {
            breakerRef.breakerRenderer.material.color = breakerRef.isOff ? offColor : onColor;
        }
    }

    #endregion
}
