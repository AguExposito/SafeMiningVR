using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(ref_EppItem))]
public class EppItemInteractable : MonoBehaviour
{
    [SerializeField] private EppEquipmentManager equipmentManager;
    [SerializeField] private float rejectSnapDistance = 0.6f;
    [SerializeField] private UnityEvent onEquipped;

    private ref_EppItem itemRef;

    #region Lifecycle

    private void Awake()
    {
        itemRef = GetComponent<ref_EppItem>();
        if (itemRef != null && itemRef.grabInteractable != null)
        {
            itemRef.grabInteractable.selectEntered.AddListener(OnGrabbed);
            itemRef.grabInteractable.selectExited.AddListener(OnReleased);
        }
    }

    private void OnDestroy()
    {
        if (itemRef != null && itemRef.grabInteractable != null)
        {
            itemRef.grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            itemRef.grabInteractable.selectExited.RemoveListener(OnReleased);
        }
    }

    #endregion

    #region Configuration

    public void Configure(EppEquipmentManager manager)
    {
        equipmentManager = manager;
    }

    public void RaiseEquipped()
    {
        onEquipped?.Invoke();
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

        if (itemRef.condition == EppItemCondition.Defective)
        {
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

        TryRejectAtZone();
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
}
