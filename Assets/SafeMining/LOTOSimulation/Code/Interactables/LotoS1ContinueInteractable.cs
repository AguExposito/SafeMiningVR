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
            ActivateFromPc();
        }

        public void ActivateFromPc()
        {
            if (equipmentManager == null)
            {
                return;
            }

            equipmentManager.TryContinue();
        }

    private void Update()
    {
        Renderer doorRenderer = GetComponent<Renderer>();
        if (doorRenderer == null || equipmentManager == null)
        {
            return;
        }

        Color ready = new Color(0.133f, 0.773f, 0.369f, 1f);
        Color blocked = new Color(0.45f, 0.47f, 0.5f, 1f);
        doorRenderer.material.color = equipmentManager.IsReadyToAdvance ? ready : blocked;
    }

    #endregion
}
