using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(ref_EppItem))]
public class EppItemInteractable : MonoBehaviour
{
    [SerializeField] private EppEquipmentManager equipmentManager;
    [SerializeField] private float equipSnapDistance = 0.55f;
    [SerializeField] private float rejectSnapDistance = 0.6f;

    private ref_EppItem itemRef;

    #region Lifecycle

    private void Awake()
    {
        itemRef = GetComponent<ref_EppItem>();
        if (itemRef != null && itemRef.grabInteractable != null)
        {
            itemRef.grabInteractable.selectExited.AddListener(OnReleased);
        }
    }

    private void OnDestroy()
    {
        if (itemRef != null && itemRef.grabInteractable != null)
        {
            itemRef.grabInteractable.selectExited.RemoveListener(OnReleased);
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

    private void OnReleased(UnityEngine.XR.Interaction.Toolkit.SelectExitEventArgs args)
    {
        if (equipmentManager == null || itemRef == null || itemRef.IsLocked)
        {
            return;
        }

        if (TryRejectAtZone())
        {
            return;
        }

        TryEquipAtAnchor();
    }

    private void TryEquipAtAnchor()
    {
        Transform anchor = equipmentManager.GetAttachAnchor(itemRef.itemType);
        if (anchor == null)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, anchor.position);
        if (distance > equipSnapDistance)
        {
            return;
        }

        equipmentManager.TryEquip(itemRef, anchor);
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
