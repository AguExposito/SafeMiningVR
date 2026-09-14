using UnityEngine;

public class ElectricalBoxDoor : MonoBehaviour
{
    public Animator doorAnimatorController; // Reference to the Animator Controller for the door
    public Collider leverCollider;
    public Collider hingeCollider;
    public LotoGameEventRelay lotoGameEventRelay;
    public bool isDoorOpen;
    public bool isPowerOff;
    public HaspControler haspControler;


    public void SetLeverColliderActiveState(int state) {
        if (state == 0)
        {
            leverCollider.enabled = false;
        }
        else { 
            leverCollider.enabled = true;
        }
        hingeCollider.enabled = true; // Enable the hinge collider when the lever is disabled
    }

    public void TriggerDoorAnimation() {
        doorAnimatorController.SetTrigger("OpenClose");
        hingeCollider.enabled = false; // Disable the hinge collider during the animation
        isDoorOpen = !isDoorOpen; // Toggle the door state
    }

    public void NotifyPanelOpened() {
        lotoGameEventRelay.NotifyPanelOpened();
    }

    public void NotifyPanelClosed()
    {
        lotoGameEventRelay.NotifyPanelClosed();
    }

    public void CheckToEnableHaspCollider() { 
        haspControler.CheckToEnableCollider();
    }

    public void SetIsPowerOff(bool state) {
        isPowerOff = state;
    }
}
