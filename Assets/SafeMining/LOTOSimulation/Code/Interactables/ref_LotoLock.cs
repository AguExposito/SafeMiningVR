using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ref_LotoLock : MonoBehaviour
{
    public XRGrabInteractable grabInteractable;
    public LotoLockType lockType = LotoLockType.Personal;
    public Renderer lockRenderer;
    public XRSocketInteractor tagSocket;
}
