#if UNITY_EDITOR
using UnityEngine;

public class LotoS1EppSmokeTest : MonoBehaviour
{
    [SerializeField] private EppEquipmentManager equipmentManager;

    [ContextMenu("LOTO/S1 Smoke Test - Happy Path")]
    public void RunHappyPathSmokeTest()
    {
        equipmentManager = ResolveManager();
        if (equipmentManager == null)
        {
            Debug.LogError("[LOTO-S1-SMOKE] EppEquipmentManager not found.");
            return;
        }

        equipmentManager.Begin();
        EquipGood(EppItemType.Helmet);
        EquipGood(EppItemType.SafetyGlasses);
        EquipGood(EppItemType.ReflectiveVest);
        EquipGood(EppItemType.Gloves);
        EquipGood(EppItemType.SafetyBoots);
        Debug.Log("[LOTO-S1-SMOKE] Happy path completed without exceptions.");
    }

    [ContextMenu("LOTO/S1 Smoke Test - Reject Paths")]
    public void RunRejectPathsSmokeTest()
    {
        equipmentManager = ResolveManager();
        if (equipmentManager == null)
        {
            Debug.LogError("[LOTO-S1-SMOKE] EppEquipmentManager not found.");
            return;
        }

        equipmentManager.Begin();
        TryEquipDefective(EppItemType.Helmet);
        equipmentManager.TryContinue();
        EquipGood(EppItemType.Helmet);
        EquipGood(EppItemType.SafetyGlasses);
        EquipGood(EppItemType.ReflectiveVest);
        EquipGood(EppItemType.Gloves);
        EquipGood(EppItemType.SafetyBoots);
        equipmentManager.TryContinue();
        Debug.Log("[LOTO-S1-SMOKE] Reject paths invoked without exceptions.");
    }

    private void EquipGood(EppItemType type)
    {
        ref_EppItem item = FindItem(type, EppItemCondition.Good);
        if (item == null)
        {
            return;
        }

        equipmentManager.TryEquip(item, equipmentManager.GetAttachAnchor(type));
    }

    private void TryEquipDefective(EppItemType type)
    {
        ref_EppItem item = FindItem(type, EppItemCondition.Defective);
        if (item == null)
        {
            return;
        }

        equipmentManager.TryEquip(item, equipmentManager.GetAttachAnchor(type));
    }

    private static ref_EppItem FindItem(EppItemType type, EppItemCondition condition)
    {
        ref_EppItem[] items = FindObjectsByType<ref_EppItem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null && items[i].itemType == type && items[i].condition == condition)
            {
                return items[i];
            }
        }

        return null;
    }

    private EppEquipmentManager ResolveManager()
    {
        if (equipmentManager != null)
        {
            return equipmentManager;
        }

        return FindFirstObjectByType<EppEquipmentManager>();
    }
}
#endif
