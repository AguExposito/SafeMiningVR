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

    private void Update()
    {
        Renderer doorRenderer = GetComponent<Renderer>();
        if (doorRenderer == null || inspectionManager == null)
        {
            return;
        }

        Color ready = new Color(0.133f, 0.773f, 0.369f, 1f);
        Color blocked = new Color(0.45f, 0.47f, 0.5f, 1f);
        doorRenderer.material.color = inspectionManager.IsReadyToAdvance ? ready : blocked;
    }

    #endregion
}
