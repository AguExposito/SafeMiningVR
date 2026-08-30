using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(ref_HazardSpot))]
public class HazardReportInteractable : MonoBehaviour
{
    [SerializeField] private HazardInspectionManager inspectionManager;
    [SerializeField] private XRSimpleInteractable interactable;

    private ref_HazardSpot spotRef;
    private LotoDwellReticle dwellReticle;

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

    public void Configure(HazardInspectionManager manager)
    {
        inspectionManager = manager;
    }

    #endregion

    #region Interaction

    public void ActivateFromPc()
    {
        if (inspectionManager == null || spotRef == null || spotRef.IsReported)
        {
            return;
        }

        inspectionManager.TryReport(spotRef);
        RefreshDwellState();
    }

    private void Update()
    {
        RefreshDwellState();
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
        RefreshDwellState();
    }

    private void RefreshDwellState()
    {
        if (dwellReticle == null)
        {
            return;
        }

        bool canDwell = spotRef != null && !spotRef.IsReported;
        dwellReticle.SetDwellEnabled(canDwell);
    }

    #endregion
}
