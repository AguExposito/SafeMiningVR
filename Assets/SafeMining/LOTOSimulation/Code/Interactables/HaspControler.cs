using UnityEngine;

public class HaspControler : MonoBehaviour
{
    public LotoGameEventRelay lotoGameEventRelay;
    public ElectricalBoxDoor electricalBoxDoor;
    public Collider haspCollider;
    public GameObject correcHasp;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CheckToEnableCollider() {
        Debug.Log("CheckToEnableCollider: " + (lotoGameEventRelay.GetPowerCutState() || electricalBoxDoor.isPowerOff) + " " + !electricalBoxDoor.isDoorOpen);
        if ((lotoGameEventRelay.GetPowerCutState() || electricalBoxDoor.isPowerOff) && !electricalBoxDoor.isDoorOpen)
        {
            haspCollider.enabled = true;
        }
        else { 
            haspCollider.enabled = false;  
        }
    }

    public void HaspPlaced(GameObject hasp) {
        correcHasp.SetActive(true);
        hasp.SetActive(false);
        lotoGameEventRelay.NotifyHaspPlaced();
    }

    private void OnTriggerEnter(Collider other)
    {
        //if (other.tag == "Hasp") { 
        //    correcHasp.SetActive(true);
        //    other.GetComponent<GameObject>().SetActive(false);
        //    lotoGameEventRelay.NotifyHaspPlaced();
        //}
    }
}
