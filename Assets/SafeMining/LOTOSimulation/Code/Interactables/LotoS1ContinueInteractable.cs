using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class LotoS1ContinueInteractable : MonoBehaviour
{
    [SerializeField] private EppEquipmentManager equipmentManager;
    [SerializeField] private XRSimpleInteractable interactable;

    #region Lifecycle

    private void Awake()
    {
        if (interactable == null)
        {
            interactable = GetComponent<XRSimpleInteractable>();
        }

        if (equipmentManager == null)
        {
            equipmentManager = FindFirstObjectByType<EppEquipmentManager>();
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

    public void Configure(EppEquipmentManager manager)
    {
        equipmentManager = manager;
    }

    #endregion

    #region Interaction

    private void OnSelected(SelectEnterEventArgs args)
    {
        if (equipmentManager == null)
        {
            return;
        }

        equipmentManager.TryContinue();
    }

    #endregion
}
