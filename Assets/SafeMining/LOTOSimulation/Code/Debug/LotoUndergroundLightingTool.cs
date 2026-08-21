#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
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
            ConfigureScene(ScenePaths[i]);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[LOTO] Underground lighting configured for S1-S3.");
    }

    public static void ConfigureScene(string scenePath)
    {
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.08f, 0.09f, 0.11f);
        RenderSettings.subtractiveShadowColor = new Color(0.12f, 0.12f, 0.14f);

        Light[] lights = Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] != null && lights[i].type == LightType.Directional)
            {
                lights[i].enabled = false;
                lights[i].gameObject.name = "Directional Light (Disabled Sun)";
            }
        }

        EnsureLight("LotoKeyIndustrial", new Vector3(0f, 3.2f, 0.5f), LightType.Spot, 4.2f, 18f, new Color(1f, 0.92f, 0.78f), true);
        EnsureLight("LotoFillA", new Vector3(-2.2f, 2.4f, -1.5f), LightType.Point, 1.1f, 10f, new Color(0.55f, 0.65f, 0.75f), false);
        EnsureLight("LotoFillB", new Vector3(2.4f, 2.2f, 1.8f), LightType.Point, 0.85f, 9f, new Color(0.5f, 0.58f, 0.68f), false);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
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
