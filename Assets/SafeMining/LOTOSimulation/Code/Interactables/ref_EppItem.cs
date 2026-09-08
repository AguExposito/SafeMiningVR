using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ref_EppItem : MonoBehaviour
{
    public EppItemType itemType = EppItemType.Helmet;
    public EppItemCondition condition = EppItemCondition.Good;
    public Renderer itemRenderer;
    public XRGrabInteractable grabInteractable;

    private bool isLocked;

    public bool IsLocked => isLocked;

    #region Public API

    public void SetEquipped(bool equipped)
    {
        isLocked = equipped;
        if (grabInteractable != null)
        {
            grabInteractable.enabled = !equipped;
        }

        Rigidbody body = GetComponent<Rigidbody>();
        if (body != null)
        {
            body.useGravity = false;
            body.isKinematic = true;
        }
    }

    public void SetRejected(bool rejected)
    {
        isLocked = rejected;
        if (grabInteractable != null)
        {
            grabInteractable.enabled = !rejected;
        }

        if (rejected)
        {
            gameObject.SetActive(false);
        }
    }

    #endregion
}
