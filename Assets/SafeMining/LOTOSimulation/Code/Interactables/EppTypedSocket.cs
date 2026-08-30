using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRSocketInteractor))]
public class EppTypedSocket : MonoBehaviour
{
    [SerializeField] private EppEquipmentManager equipmentManager;
    [SerializeField] private XRSocketInteractor socket;
    [SerializeField] private EppSocketRole role = EppSocketRole.Locker;
    [SerializeField] private EppItemType expectedType;

    #region Lifecycle

    private void Awake()
    {
        if (socket == null)
        {
            socket = GetComponent<XRSocketInteractor>();
        }
    }

    private void OnEnable()
    {
        if (socket != null)
        {
            socket.selectEntered.AddListener(OnSocketed);
        }
    }

    private void OnDisable()
    {
        if (socket != null)
        {
            socket.selectEntered.RemoveListener(OnSocketed);
        }
    }

    #endregion

    #region Configuration

    public EppSocketRole Role => role;

    public EppItemType ExpectedType => expectedType;

    public void Configure(EppEquipmentManager manager, EppSocketRole socketRole, EppItemType type)
    {
        equipmentManager = manager;
        role = socketRole;
        expectedType = type;
        if (socket == null)
        {
            socket = GetComponent<XRSocketInteractor>();
        }
    }

    public void BindManager(EppEquipmentManager manager)
    {
        equipmentManager = manager;
        if (socket == null)
        {
            socket = GetComponent<XRSocketInteractor>();
        }
    }

    #endregion

    #region Interaction

    private void OnSocketed(SelectEnterEventArgs args)
    {
        if (equipmentManager == null || args.interactableObject == null)
        {
            return;
        }

        ref_EppItem item = args.interactableObject.transform.GetComponent<ref_EppItem>();
        if (item == null)
        {
            Eject(args.interactableObject);
            return;
        }

        if (role == EppSocketRole.Reject)
        {
            if (!equipmentManager.TryReject(item))
            {
                Eject(args.interactableObject);
            }

            return;
        }

        if (item.itemType != expectedType)
        {
            Eject(args.interactableObject);
            return;
        }

        if (role == EppSocketRole.Body)
        {
            bool equipped = equipmentManager.TryEquip(item, transform);
            if (!equipped)
            {
                Eject(args.interactableObject);
            }
        }
    }

    private void Eject(IXRSelectInteractable interactable)
    {
        if (socket == null || interactable == null || socket.interactionManager == null)
        {
            return;
        }

        socket.interactionManager.SelectExit(socket, interactable);
    }

    #endregion
}
