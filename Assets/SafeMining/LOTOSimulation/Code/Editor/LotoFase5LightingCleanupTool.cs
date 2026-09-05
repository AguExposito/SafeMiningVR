#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public static class LotoFase5LightingCleanupTool
{
    private static readonly string[] ScenePaths =
    {
        "Assets/SafeMining/LOTOSimulation/Scenes/VR-LOTO-S1.unity",
        "Assets/SafeMining/LOTOSimulation/Scenes/VR-LOTO-S2.unity"
    };

    private static readonly string[] PrefabPaths =
    {
        "Assets/SafeMining/LOTOSimulation/Prefabs/PREF_S1_Locker.prefab",
        "Assets/SafeMining/LOTOSimulation/Prefabs/PREF_S2_InspectionYard.prefab",
        "Assets/SafeMining/LOTOSimulation/Prefabs/PREF_S3_ConveyorStation.prefab"
    };

    #region Public API

    [MenuItem("SafeMining/LOTO/Apply Fase 5 Lighting Cleanup S1-S2")]
    public static void ApplyFase5Cleanup()
    {
        int prefabRemoved = 0;
        for (int i = 0; i < PrefabPaths.Length; i++)
        {
            prefabRemoved += StripPrefabLights(PrefabPaths[i]);
        }

        for (int i = 0; i < ScenePaths.Length; i++)
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePaths[i], OpenSceneMode.Single);
            int before = CountSceneLights();
            LotoUndergroundLightingTool.FixDuplicateLights(ScenePaths[i]);
            int after = CountSceneLights();
            Debug.Log("[LOTO Fase5] " + scene.name + " lights before=" + before + " after=" + after);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[LOTO Fase5] Lighting cleanup complete. prefabLightsRemoved=" + prefabRemoved);
    }

    public static int StripPrefabLights(string prefabPath)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
        Light[] lights = root.GetComponentsInChildren<Light>(true);
        GameObject[] targets = new GameObject[lights.Length];
        for (int i = 0; i < lights.Length; i++)
        {
            targets[i] = lights[i] != null ? lights[i].gameObject : null;
        }

        int removed = 0;
        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] == null)
            {
                continue;
            }

            removed++;
            DestroyLightObject(targets[i]);
        }

        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        PrefabUtility.UnloadPrefabContents(root);
        Debug.Log("[LOTO Fase5] " + prefabPath + " prefabLightsRemoved=" + removed);
        return removed;
    }

    #endregion

    #region Scene Stripping

    public static int StripSceneLights()
    {
        Light[] lights = Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        GameObject[] targets = new GameObject[lights.Length];
        for (int i = 0; i < lights.Length; i++)
        {
            targets[i] = lights[i] != null ? lights[i].gameObject : null;
        }

        int removed = 0;
        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] == null)
            {
                continue;
            }

            removed++;
            DestroyLightObject(targets[i]);
        }

        return removed;
    }

    private static int CountSceneLights()
    {
        return Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
    }

    #endregion

    #region Helpers

    private static void RemoveLightFromObject(Light light)
    {
        if (light == null)
        {
            return;
        }

        DestroyLightObject(light.gameObject);
    }

    private static void DestroyLightObject(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        UniversalAdditionalLightData urpData = target.GetComponent<UniversalAdditionalLightData>();
        if (urpData != null)
        {
            Object.DestroyImmediate(urpData);
        }

        Object.DestroyImmediate(target);
    }

    #endregion
}
#endif
