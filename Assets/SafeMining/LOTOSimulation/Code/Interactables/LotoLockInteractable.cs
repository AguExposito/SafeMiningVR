using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(ref_LotoLock))]
public class LotoLockInteractable : MonoBehaviour
{
    [SerializeField] private LotoSequenceManager sequenceManager;
    [SerializeField] private float snapDistance = 0.45f;

    private ref_LotoLock lockRef;

    #region Lifecycle

    private void Awake()
    {
        lockRef = GetComponent<ref_LotoLock>();
        if (sequenceManager == null)
        {
            sequenceManager = FindFirstObjectByType<LotoSequenceManager>();
        }

        if (lockRef != null && lockRef.grabInteractable != null)
        {
            lockRef.grabInteractable.selectExited.AddListener(OnReleased);
        }
    }

    private void OnDestroy()
    {
        if (lockRef != null && lockRef.grabInteractable != null)
        {
            lockRef.grabInteractable.selectExited.RemoveListener(OnReleased);
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
        if (sequenceManager == null || lockRef == null || sequenceManager.HasPlacedLock)
        {
            return;
        }

        Transform anchor = sequenceManager.LockSnapAnchor;
        if (anchor == null)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, anchor.position);
        if (distance > snapDistance)
        {
            return;
        }

        sequenceManager.TryApplyLock(lockRef.lockType, gameObject);
    }

    #endregion
}
