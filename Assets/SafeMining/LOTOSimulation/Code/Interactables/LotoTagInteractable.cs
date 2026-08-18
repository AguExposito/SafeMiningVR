using UnityEngine;

[RequireComponent(typeof(ref_LotoTag))]
public class LotoTagInteractable : MonoBehaviour
{
    [SerializeField] private LotoSequenceManager sequenceManager;

    #region Interaction

    public void Configure(LotoSequenceManager manager)
    {
        sequenceManager = manager;
    }

    #endregion
}
