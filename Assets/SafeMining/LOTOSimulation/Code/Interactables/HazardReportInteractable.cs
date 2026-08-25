using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(ref_HazardSpot))]
public class HazardReportInteractable : MonoBehaviour
{
    [SerializeField] private HazardInspectionManager inspectionManager;
    [SerializeField] private XRSimpleInteractable interactable;

    private ref_HazardSpot spotRef;

    #region Lifecycle

    private void Awake()
    {
        spotRef = GetComponent<ref_HazardSpot>();
        if (interactable == null)
        {
            interactable = GetComponent<XRSimpleInteractable>();
        }

        if (spotRef != null)
        {
            spotRef.CacheOriginalColor();
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
            ActivateFromPc();
        }

        public void ActivateFromPc()
        {
            if (inspectionManager == null || spotRef == null || spotRef.IsReported)
            {
                return;
            }

            inspectionManager.TryReport(spotRef);
        }

    #endregion
}
