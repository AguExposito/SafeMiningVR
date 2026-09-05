using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class LotoS2ContinueInteractable : MonoBehaviour
{
    [SerializeField] private HazardInspectionManager inspectionManager;
    [SerializeField] private XRSimpleInteractable interactable;

    private LotoDwellReticle dwellReticle;
    private Renderer doorRenderer;
    private bool lastReadyState;
    private bool hasPaintedReadyState;

    #region Lifecycle

    private void Awake()
    {
        if (interactable == null)
        {
            interactable = GetComponent<XRSimpleInteractable>();
        }

        doorRenderer = GetComponent<Renderer>();
        if (doorRenderer == null)
        {
            doorRenderer = GetComponentInChildren<Renderer>(true);
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
        hasPaintedReadyState = false;
    }

    #endregion

    #region Interaction

    public void ActivateFromPc()
    {
        if (inspectionManager == null)
        {
            return;
        }

        if (!inspectionManager.IsReadyToAdvance)
        {
            inspectionManager.TryContinue();
            return;
        }

        inspectionManager.TryContinue();
        ConsumeContinueInteraction();
    }

    private void ConsumeContinueInteraction()
    {
        if (dwellReticle != null)
        {
            dwellReticle.SetDwellEnabled(false);
        }

        if (interactable != null)
        {
            interactable.enabled = false;
        }
    }

    private void Update()
    {
        if (doorRenderer == null || inspectionManager == null)
        {
            return;
        }

        bool ready = inspectionManager.IsReadyToAdvance;
        if (hasPaintedReadyState && ready == lastReadyState)
        {
            return;
        }

        lastReadyState = ready;
        hasPaintedReadyState = true;
        Color readyColor = Brand.Secondary;
        Color blockedColor = Brand.Tertiary;
        blockedColor.a = 0.85f;
        doorRenderer.material.color = ready ? readyColor : blockedColor;
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
