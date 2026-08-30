#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class LotoS1LockerPrefabBuilder
{
    public const string PrefabPath = "Assets/SafeMining/LOTOSimulation/Prefabs/PREF_S1_Locker.prefab";

    public static GameObject Build()
    {
        Debug.LogWarning("[LOTO] LotoS1LockerPrefabBuilder is deprecated. Edit " + PrefabPath + " directly.");
        return AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
    }
}
#endif
