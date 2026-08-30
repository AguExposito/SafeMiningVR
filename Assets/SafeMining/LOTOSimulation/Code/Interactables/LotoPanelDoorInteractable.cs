using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class LotoPanelDoorInteractable : MonoBehaviour
{
    [SerializeField] private XRSimpleInteractable interactable;
    [SerializeField] private Transform doorPivot;
    [SerializeField] private LotoBreakerInteractable breakerInteractable;
    [SerializeField] private float openAngle = 110f;

    private bool isOpen;
    private LotoDwellReticle dwellReticle;

    #region Lifecycle

    private void Awake()
    {
        if (interactable == null)
        {
            interactable = GetComponent<XRSimpleInteractable>();
        }

        BindDwellReticle();

        if (breakerInteractable != null)
        {
            breakerInteractable.SetInteractableEnabled(false);
        }
    }

    private void OnDestroy()
    {
        if (dwellReticle != null)
        {
            dwellReticle.Unbind();
        }
    }

    #endregion

    #region Interaction

    public void ActivateFromPc()
    {
        if (isOpen)
        {
            return;
        }

        isOpen = true;
        if (doorPivot != null)
        {
            doorPivot.localRotation = Quaternion.Euler(0f, openAngle, 0f);
        }

        if (breakerInteractable != null)
        {
            breakerInteractable.SetInteractableEnabled(true);
        }

        if (dwellReticle != null)
        {
            dwellReticle.SetDwellEnabled(false);
        }

        if (interactable != null)
        {
            interactable.enabled = false;
        }
    }

    private void BindDwellReticle()
    {
        if (interactable == null)
        {
            return;
        }

        dwellReticle = GetComponent<LotoDwellReticle>();
        if (dwellReticle == null)
        {
            dwellReticle = gameObject.AddComponent<LotoDwellReticle>();
        }

        dwellReticle.Bind(interactable, ActivateFromPc);
    }

    #endregion
}
