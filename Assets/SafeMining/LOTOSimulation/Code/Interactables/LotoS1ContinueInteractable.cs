using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class LotoS1ContinueInteractable : MonoBehaviour
{
    [SerializeField] private EppEquipmentManager equipmentManager;
    [SerializeField] private XRSimpleInteractable interactable;

    private LotoDwellReticle dwellReticle;

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

    public void Configure(EppEquipmentManager manager)
    {
        equipmentManager = manager;
    }

    #endregion

    #region Interaction

    public void ActivateFromPc()
    {
        if (equipmentManager == null)
        {
            return;
        }

        if (!equipmentManager.IsReadyToAdvance)
        {
            equipmentManager.TryContinue();
            return;
        }

        equipmentManager.TryContinue();
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
        Renderer doorRenderer = GetComponent<Renderer>();
        if (doorRenderer == null || equipmentManager == null)
        {
            return;
        }

        Color ready = new Color(0.133f, 0.773f, 0.369f, 1f);
        Color blocked = new Color(0.45f, 0.47f, 0.5f, 1f);
        doorRenderer.material.color = equipmentManager.IsReadyToAdvance ? ready : blocked;
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
