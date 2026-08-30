using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(ref_LotoControlButton))]
public class LotoControlButtonInteractable : MonoBehaviour
{
    [SerializeField] private LotoSequenceManager sequenceManager;

    private ref_LotoControlButton buttonRef;
    private LotoDwellReticle dwellReticle;

    #region Lifecycle

    private void Awake()
    {
        buttonRef = GetComponent<ref_LotoControlButton>();
        BindDwellReticle();
    }

    private void OnDestroy()
    {
        if (dwellReticle != null)
        {
            dwellReticle.Unbind();
        }
    }

    #endregion

    #region Configuration

    public void Configure(LotoSequenceManager manager)
    {
        sequenceManager = manager;
    }

    #endregion

    #region Interaction

    public void ActivateFromPc()
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

    private void BindDwellReticle()
    {
        if (buttonRef == null || buttonRef.interactable == null)
        {
            return;
        }

        dwellReticle = GetComponent<LotoDwellReticle>();
        if (dwellReticle == null)
        {
            dwellReticle = gameObject.AddComponent<LotoDwellReticle>();
        }

        dwellReticle.Bind(buttonRef.interactable, ActivateFromPc);
    }

    #endregion
}
