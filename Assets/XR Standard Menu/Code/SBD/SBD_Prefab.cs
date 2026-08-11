using UnityEngine;

public class SBD_Prefab : MonoBehaviour, I_SBD
{
    public static SBD_Prefab Instance;
    public GameObject UI_Waiter_normal;

    public void Initialize()
    {
        if (Instance == null)
        {
            Instance = this;
            return;
        }
        Destroy(gameObject);
    }
}
