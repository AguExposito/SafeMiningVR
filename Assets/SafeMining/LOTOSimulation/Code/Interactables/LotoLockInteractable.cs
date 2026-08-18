using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(ref_LotoLock))]
public class LotoLockInteractable : MonoBehaviour
{
    [SerializeField] private LotoSequenceManager sequenceManager;

    private ref_LotoLock lockRef;

    #region Lifecycle

    private void Awake()
    {
        lockRef = GetComponent<ref_LotoLock>();
        if (lockRef != null && lockRef.tagSocket != null)
        {
            lockRef.tagSocket.socketActive = false;
        }
    }

    #endregion

    #region Interaction

    public void Configure(LotoSequenceManager manager)
    {
        sequenceManager = manager;
    }

    #endregion
}
