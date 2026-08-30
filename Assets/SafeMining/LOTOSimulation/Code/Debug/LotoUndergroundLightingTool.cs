#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public static class LotoUndergroundLightingTool
{
    private static readonly string[] ScenePaths =
    {
        "Assets/SafeMining/LOTOSimulation/Scenes/VR-LOTO-S1.unity",
        "Assets/SafeMining/LOTOSimulation/Scenes/VR-LOTO-S2.unity",
        "Assets/SafeMining/LOTOSimulation/Scenes/VR-LOTO-S3.unity"
    };

    [MenuItem("SafeMining/LOTO/Configure Underground Lighting S1-S3")]
    public static void ConfigureAll()
    {
        for (int i = 0; i < ScenePaths.Length; i++)
        {
            ConfigureScene(ScenePaths[i], i, DefaultKeyPosition(i), DefaultFillAPosition(i), DefaultFillBPosition(i));
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[LOTO] Underground lighting configured for S1-S3.");
    }

    public static void ConfigureScene(string scenePath)
    {
        int sceneIndex = 0;
        for (int i = 0; i < ScenePaths.Length; i++)
        {
            if (ScenePaths[i] == scenePath)
            {
                sceneIndex = i;
                break;
            }
        }

        ConfigureScene(scenePath, sceneIndex, DefaultKeyPosition(sceneIndex), DefaultFillAPosition(sceneIndex), DefaultFillBPosition(sceneIndex));
    }

    public static void ConfigureScene(string scenePath, int sceneIndex, Vector3 keyPosition, Vector3 fillAPosition, Vector3 fillBPosition)
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.path != scenePath)
        {
            activeScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        }

        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.08f, 0.09f, 0.11f);
        RenderSettings.subtractiveShadowColor = new Color(0.12f, 0.12f, 0.14f);

        StripAllSceneLights();

        EnsureLight("LotoKeyIndustrial", keyPosition, LightType.Spot, 4.2f, 18f, new Color(1f, 0.92f, 0.78f), true);
        EnsureLight("LotoFillA", fillAPosition, LightType.Point, 1.1f, 10f, new Color(0.55f, 0.65f, 0.75f), false);
        EnsureLight("LotoFillB", fillBPosition, LightType.Point, 0.85f, 9f, new Color(0.5f, 0.58f, 0.68f), false);

        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveScene(activeScene);
    }

    private static Vector3 DefaultKeyPosition(int sceneIndex)
    {
        if (sceneIndex == 1)
        {
            return new Vector3(0f, 4f, -2f);
        }

        if (sceneIndex == 2)
        {
            return new Vector3(0f, 3.5f, 1.2f);
        }

        return new Vector3(0f, 3.2f, 0.5f);
    }

    private static Vector3 DefaultFillAPosition(int sceneIndex)
    {
        if (sceneIndex == 1)
        {
            return new Vector3(-3f, 2.8f, 0f);
        }

        if (sceneIndex == 2)
        {
            return new Vector3(-2.5f, 2.6f, -0.5f);
        }

        return new Vector3(-2.2f, 2.4f, -1.5f);
    }

    private static Vector3 DefaultFillBPosition(int sceneIndex)
    {
        if (sceneIndex == 1)
        {
            return new Vector3(3f, 2.5f, -1f);
        }

        if (sceneIndex == 2)
        {
            return new Vector3(2.2f, 2.4f, 2f);
        }

        return new Vector3(2.4f, 2.2f, 1.8f);
    }

    public static int StripAllSceneLights()
    {
        Light[] lights = Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int removed = 0;
        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] == null)
            {
                continue;
            }

            removed++;
            DestroyLightObject(lights[i].gameObject);
        }

        return removed;
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

    private static void EnsureLight(string name, Vector3 position, LightType type, float intensity, float range, Color color, bool key)
    {
        GameObject existing = GameObject.Find(name);
        if (existing == null)
        {
            existing = new GameObject(name);
        }

        existing.transform.position = position;
        if (type == LightType.Spot)
        {
            existing.transform.rotation = Quaternion.Euler(75f, 0f, 0f);
        }

        Light light = existing.GetComponent<Light>();
        if (light == null)
        {
            light = existing.AddComponent<Light>();
        }

        light.type = type;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        light.shadows = key ? LightShadows.Soft : LightShadows.None;
        light.renderMode = LightRenderMode.Auto;
        light.lightmapBakeType = LightmapBakeType.Mixed;
        if (type == LightType.Spot)
        {
            light.spotAngle = 95f;
            light.innerSpotAngle = 55f;
        }
    }
}
#endif
