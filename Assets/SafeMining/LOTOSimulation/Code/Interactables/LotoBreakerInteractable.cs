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
        if (sequenceManager == null)
        {
            sequenceManager = FindFirstObjectByType<LotoSequenceManager>();
        }

        if (breakerRef != null && breakerRef.interactable != null)
        {
            breakerRef.interactable.selectEntered.AddListener(OnSelected);
        }
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

    private void OnSelected(SelectEnterEventArgs args)
    {
        if (sequenceManager == null || breakerRef == null || breakerRef.isOff)
        {
            return;
        }

        breakerRef.isOff = true;
        ApplyVisual();
        sequenceManager.TryToggleBreakerOff();
    }

    private void ApplyVisual()
    {
        if (breakerRef.breakerRenderer != null)
        {
            breakerRef.breakerRenderer.material.color = breakerRef.isOff ? offColor : onColor;
        }
    }

    #endregion
}
