#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class LotoS2InspectionYardPrefabBuilder
{
    public const string PrefabPath = "Assets/SafeMining/LOTOSimulation/Prefabs/PREF_S2_InspectionYard.prefab";
    public const string ConveyorPrefabPath = "Assets/SafeMining/LOTOSimulation/Prefabs/PREF_S3_ConveyorStation.prefab";

    public static GameObject Build()
    {
        Debug.LogWarning("[LOTO] LotoS2InspectionYardPrefabBuilder is deprecated. Edit " + PrefabPath + " directly.");
        return AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
    }
}
#endif
