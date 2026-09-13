using UnityEngine;

public class LotoScenario1Controller : MonoBehaviour
{
    [SerializeField] private GameObject s1ZoneRoot;
    [SerializeField] private EppEquipmentManager equipmentManager;

    private bool hasBegun;

    #region Public API

    public void Configure(GameObject zoneRoot, GameObject lotoZoneRoot, EppEquipmentManager manager)
    {
        s1ZoneRoot = zoneRoot;
        equipmentManager = manager;
    }

    public void Begin()
    {
        if (hasBegun)
        {
            return;
        }

        hasBegun = true;

        if (s1ZoneRoot != null)
        {
            s1ZoneRoot.SetActive(true);
        }

        if (equipmentManager != null)
        {
            equipmentManager.Configure(this);
            equipmentManager.Begin();
        }
    }

    #endregion
}
