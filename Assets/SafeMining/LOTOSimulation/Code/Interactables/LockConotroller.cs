using UnityEngine;

public class LockConotroller : MonoBehaviour
{

    public GameObject lockCorrect;
    public LotoGameEventRelay lotoGameEventRelay;

    public void LockPlaced(GameObject lockObj)
    {
        lockCorrect.SetActive(true);
        lockObj.SetActive(false);
        lotoGameEventRelay.NotifyLockPlaced();
    }

    private void OnTriggerEnter(Collider other)
    {
        //if(other.tag == "Lock")
        //{
        //    lockCorrect.SetActive(true);
        //    other.gameObject.SetActive(false);
        //    lotoGameEventRelay.NotifyLockPlaced();
        //}
    }
}
