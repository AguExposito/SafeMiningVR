using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(ref_EppItem))]
public class EppItemInteractable : MonoBehaviour
{
    [SerializeField] private EppEquipmentManager equipmentManager;
    [SerializeField] private float rejectSnapDistance = 0.6f;
    [SerializeField] private UnityEvent onGrabbed;
    [SerializeField] private UnityEvent onReleased;
    [SerializeField] private UnityEvent onEquipped;
    [SerializeField] private UnityEvent onRejected;
    [SerializeField] private Color hoverOutlineColor = new Color(0.05f, 0.85f, 0.95f, 1f);
    [SerializeField] private float hoverOutlineWidth = 3.5f;

    private ref_EppItem itemRef;
    private Outline hoverOutline;

    #region Lifecycle

    private void Awake()
    {
        itemRef = GetComponent<ref_EppItem>();
        EnsureGrabInteractable();
        EnsureGrabCollider();
        if (itemRef != null && itemRef.grabInteractable != null)
        {
            itemRef.grabInteractable.selectEntered.AddListener(OnGrabbed);
            itemRef.grabInteractable.selectExited.AddListener(OnReleased);
            itemRef.grabInteractable.hoverEntered.AddListener(OnHoverEntered);
            itemRef.grabInteractable.hoverExited.AddListener(OnHoverExited);
        }
    }

    private void OnDestroy()
    {
        if (itemRef != null && itemRef.grabInteractable != null)
        {
            itemRef.grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            itemRef.grabInteractable.selectExited.RemoveListener(OnReleased);
            itemRef.grabInteractable.hoverEntered.RemoveListener(OnHoverEntered);
            itemRef.grabInteractable.hoverExited.RemoveListener(OnHoverExited);
        }

        SetHoverOutline(false);
    }

    #endregion

    #region Configuration

    public void Configure(EppEquipmentManager manager)
    {
        equipmentManager = manager;
        EnsureGrabInteractable();
        EnsureGrabCollider();
    }

    public void RaiseEquipped()
    {
        onEquipped?.Invoke();
    }

    public void RaiseRejected()
    {
        onRejected?.Invoke();
    }

    #endregion

    #region Interaction

    private void OnGrabbed(UnityEngine.XR.Interaction.Toolkit.SelectEnterEventArgs args)
    {
        if (equipmentManager == null || itemRef == null || itemRef.IsLocked)
        {
            return;
        }

        if (args != null && args.interactorObject is XRSocketInteractor)
        {
            return;
        }

        SetHoverOutline(false);
        onGrabbed?.Invoke();

        if (itemRef.condition == EppItemCondition.Defective)
        {
            equipmentManager.NotifyDefectiveInspected(itemRef);
            return;
        }

        equipmentManager.TryEquip(itemRef, null);
    }

    private void OnReleased(UnityEngine.XR.Interaction.Toolkit.SelectExitEventArgs args)
    {
        if (equipmentManager == null || itemRef == null || itemRef.IsLocked)
        {
            return;
        }

        onReleased?.Invoke();

        if (TryRejectAtZone())
        {
            return;
        }

        if (itemRef.condition == EppItemCondition.Defective)
        {
            equipmentManager.TryWearDefectiveNearBody(itemRef);
        }
    }

    private void OnHoverEntered(UnityEngine.XR.Interaction.Toolkit.HoverEnterEventArgs args)
    {
        if (itemRef == null || itemRef.IsLocked)
        {
            return;
        }

        if (args != null && args.interactorObject is XRSocketInteractor)
        {
            return;
        }

        SetHoverOutline(true);
    }

    private void OnHoverExited(UnityEngine.XR.Interaction.Toolkit.HoverExitEventArgs args)
    {
        SetHoverOutline(false);
    }

    private bool TryRejectAtZone()
    {
        Transform rejectZone = equipmentManager.rejectZoneAnchor;
        if (rejectZone == null || itemRef.condition != EppItemCondition.Defective)
        {
            return false;
        }

        float distance = Vector3.Distance(transform.position, rejectZone.position);
        if (distance > rejectSnapDistance)
        {
            return false;
        }

        return equipmentManager.TryReject(itemRef);
    }

    #endregion

    #region Grab Readiness

    private void EnsureGrabInteractable()
    {
        if (itemRef == null)
        {
            itemRef = GetComponent<ref_EppItem>();
        }

        if (itemRef == null)
        {
            return;
        }

        if (itemRef.grabInteractable == null)
        {
            itemRef.grabInteractable = GetComponent<XRGrabInteractable>();
        }

        if (itemRef.grabInteractable == null)
        {
            itemRef.grabInteractable = gameObject.AddComponent<XRGrabInteractable>();
        }

        itemRef.grabInteractable.enabled = !itemRef.IsLocked;
    }

    private void EnsureGrabCollider()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        if (box == null)
        {
            box = gameObject.AddComponent<BoxCollider>();
            box.center = Vector3.zero;
            box.size = new Vector3(0.28f, 0.12f, 0.28f);
        }

        Vector3 minSize = new Vector3(0.22f, 0.1f, 0.22f);
        if (itemRef != null && itemRef.itemType == EppItemType.SafetyGlasses)
        {
            minSize = new Vector3(0.34f, 0.16f, 0.3f);
        }

        box.size = Vector3.Max(box.size, minSize);
        box.isTrigger = false;
        box.enabled = true;
    }

    #endregion

    #region Hover Outline

    private void SetHoverOutline(bool enabled)
    {
        if (!enabled)
        {
            if (hoverOutline != null)
            {
                hoverOutline.enabled = false;
            }

            return;
        }

            if (hoverOutline == null)
            {
                hoverOutline = GetComponent<Outline>();
                if (hoverOutline == null)
                {
                    hoverOutline = gameObject.AddComponent<Outline>();
                }
            }

        hoverOutline.OutlineMode = Outline.Mode.OutlineVisible;
        hoverOutline.OutlineColor = Brand.HasConfig
            ? Brand.GetColor(BrandColorRole.Secondary)
            : hoverOutlineColor;
        hoverOutline.OutlineWidth = hoverOutlineWidth;
        hoverOutline.enabled = true;
    }

    #endregion
}
