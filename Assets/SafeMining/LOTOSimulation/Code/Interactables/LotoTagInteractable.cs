using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(ref_LotoTag))]
public class LotoTagInteractable : MonoBehaviour
{
    [SerializeField] private LotoSequenceManager sequenceManager;
    [SerializeField] private float snapDistance = 0.45f;

    private ref_LotoTag tagRef;

    #region Lifecycle

    private void Awake()
    {
        tagRef = GetComponent<ref_LotoTag>();
        if (sequenceManager == null)
        {
            sequenceManager = FindFirstObjectByType<LotoSequenceManager>();
        }

        if (tagRef != null && tagRef.grabInteractable != null)
        {
            tagRef.grabInteractable.selectExited.AddListener(OnReleased);
        }
    }

    private void OnDestroy()
    {
        if (tagRef != null && tagRef.grabInteractable != null)
        {
            tagRef.grabInteractable.selectExited.RemoveListener(OnReleased);
        }
    }

    #endregion

    #region Interaction

    public void Configure(LotoSequenceManager manager)
    {
        sequenceManager = manager;
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        if (sequenceManager == null || tagRef == null || sequenceManager.HasPlacedTag || !sequenceManager.HasPlacedLock)
        {
            return;
        }

        Transform anchor = sequenceManager.TagSnapAnchor ?? sequenceManager.LockSnapAnchor;
        if (anchor == null)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, anchor.position);
        if (distance > snapDistance)
        {
            return;
        }

        sequenceManager.TryApplyTag(tagRef.tagType, gameObject);
    }

    #endregion
}
