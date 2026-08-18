using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class LotoS2ContinueInteractable : MonoBehaviour
{
    [SerializeField] private HazardInspectionManager inspectionManager;
    [SerializeField] private XRSimpleInteractable interactable;

    #region Lifecycle

    private void Awake()
    {
        if (interactable == null)
        {
            interactable = GetComponent<XRSimpleInteractable>();
        }

        if (inspectionManager == null)
        {
            inspectionManager = FindFirstObjectByType<HazardInspectionManager>();
        }

        if (interactable != null)
        {
            interactable.selectEntered.AddListener(OnSelected);
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

    #region Configuration

    public void Configure(HazardInspectionManager manager)
    {
        inspectionManager = manager;
    }

    #endregion

    #region Interaction

    private void OnSelected(SelectEnterEventArgs args)
    {
        if (inspectionManager == null)
        {
            return;
        }

        inspectionManager.TryContinue();
    }

    #endregion
}
