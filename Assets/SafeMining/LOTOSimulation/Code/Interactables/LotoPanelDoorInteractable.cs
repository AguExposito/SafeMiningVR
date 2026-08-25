using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class LotoPanelDoorInteractable : MonoBehaviour
{
    [SerializeField] private XRSimpleInteractable interactable;
    [SerializeField] private Transform doorPivot;
    [SerializeField] private LotoBreakerInteractable breakerInteractable;
    [SerializeField] private float openAngle = 110f;

    private bool isOpen;

    #region Lifecycle

    private void Awake()
    {
        if (interactable == null)
        {
            interactable = GetComponent<XRSimpleInteractable>();
        }

        if (interactable != null)
        {
            interactable.selectEntered.AddListener(OnSelected);
        }

        if (breakerInteractable != null)
        {
            breakerInteractable.SetInteractableEnabled(false);
        }
    }

    private void OnDestroy()
    {
        if (interactable != null)
        {
            interactable.selectEntered.RemoveListener(OnSelected);
        }
    }

    #endregion

    #region Interaction

        private void OnSelected(SelectEnterEventArgs args)
        {
            ActivateFromPc();
        }

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
        }

    #endregion
}
