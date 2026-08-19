using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class LotoConveyorInspectionMode : MonoBehaviour
{
    [SerializeField] private BeltAnimator beltAnimator;

    public void Apply()
    {
        if (beltAnimator == null)
        {
            beltAnimator = GetComponentInChildren<BeltAnimator>(true);
        }

        if (beltAnimator != null)
        {
            beltAnimator.SetRunning(false);
        }

        XRGrabInteractable[] grabs = GetComponentsInChildren<XRGrabInteractable>(true);
        for (int i = 0; i < grabs.Length; i++)
        {
            if (grabs[i] != null)
            {
                grabs[i].enabled = false;
            }
        }

        XRSimpleInteractable[] simples = GetComponentsInChildren<XRSimpleInteractable>(true);
        for (int i = 0; i < simples.Length; i++)
        {
            if (simples[i] != null)
            {
                simples[i].enabled = false;
            }
        }

        XRSocketInteractor[] sockets = GetComponentsInChildren<XRSocketInteractor>(true);
        for (int i = 0; i < sockets.Length; i++)
        {
            if (sockets[i] != null)
            {
                sockets[i].socketActive = false;
                sockets[i].enabled = false;
            }
        }
    }
}
