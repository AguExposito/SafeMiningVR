using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
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
            body.isKinematic = equipped;
        }
    }

    public void SetRejected(bool rejected)
    {
        isLocked = rejected;
        if (grabInteractable != null)
        {
            grabInteractable.enabled = !rejected;
        }

        if (itemRenderer != null)
        {
            itemRenderer.material.color = new Color(0.35f, 0.35f, 0.35f, 0.6f);
        }

        gameObject.SetActive(!rejected);
    }

    #endregion
}
