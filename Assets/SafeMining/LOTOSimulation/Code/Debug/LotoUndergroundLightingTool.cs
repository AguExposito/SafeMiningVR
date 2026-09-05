#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public static class LotoUndergroundLightingTool
{
    private const float KeyIntensityMax = 4.2f;
    private const float AssistKeyIntensityMax = 3.4f;
    private const float ShadowNearPlane = 0.8f;

    private static readonly string[] ScenePaths =
    {
        "Assets/SafeMining/LOTOSimulation/Scenes/VR-LOTO-S1.unity",
        "Assets/SafeMining/LOTOSimulation/Scenes/VR-LOTO-S2.unity"
    };

    private static readonly string[] CanonicalLightNames =
    {
        "LotoKeyIndustrial",
        "LotoFillA",
        "LotoFillB"
    };

    [MenuItem("SafeMining/LOTO/Configure Underground Lighting S1-S2")]
    public static void ConfigureAll()
    {
        for (int i = 0; i < ScenePaths.Length; i++)
        {
            ConfigureScene(ScenePaths[i], i, DefaultKeyPosition(i), DefaultFillAPosition(i), DefaultFillBPosition(i));
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[LOTO] Underground lighting configured for S1-S2.");
    }

    [MenuItem("SafeMining/LOTO/Fix Duplicate Lights S1-S2")]
    public static void FixDuplicateLightsAll()
    {
        for (int i = 0; i < ScenePaths.Length; i++)
        {
            FixDuplicateLights(ScenePaths[i]);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[LOTO] Duplicate light fix applied for S1-S2.");
    }

    [MenuItem("SafeMining/LOTO/Rebake S1 Lightmaps")]
    public static void RebakeS1Lightmaps()
    {
        string scenePath = ScenePaths[0];
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        FixDuplicateLights(scenePath);
        ApplyAmbientSettings();
        Lightmapping.Bake();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("[LOTO] S1 lightmaps rebaked.");
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
        Scene activeScene = OpenScene(scenePath);
        ApplyAmbientSettings();
        StripAllSceneLights();

        EnsureLight("LotoKeyIndustrial", keyPosition, LightType.Spot, KeyIntensityMax, 18f, new Color(1f, 0.92f, 0.78f), true);
        EnsureLight("LotoFillA", fillAPosition, LightType.Point, 1.1f, 10f, new Color(0.55f, 0.65f, 0.75f), false);
        EnsureLight("LotoFillB", fillBPosition, LightType.Point, 0.85f, 9f, new Color(0.5f, 0.58f, 0.68f), false);

        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveScene(activeScene);
    }

    public static int FixDuplicateLights(string scenePath)
    {
        Scene activeScene = OpenScene(scenePath);
        ApplyAmbientSettings();
        int removed = StripDuplicateFillLights();
        ConfigureCeilingKeyLights();
        ApplyAntiFlickerToAllLotoLights();
        int lightCount = CountSceneLights();
        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveScene(activeScene);
        Debug.Log("[LOTO] " + activeScene.name + " duplicate fix removed=" + removed + " lights=" + lightCount);
        return removed;
    }

    public static int StripAllSceneLights()
    {
        Light[] lights = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
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

    private static Scene OpenScene(string scenePath)
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.path != scenePath)
        {
            activeScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        }

        return activeScene;
    }

    private static void ApplyAmbientSettings()
    {
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.08f, 0.09f, 0.11f);
        RenderSettings.subtractiveShadowColor = new Color(0.12f, 0.12f, 0.14f);
        RenderSettings.fog = false;
    }

    private static int StripDuplicateFillLights()
    {
        Light[] lights = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int removed = 0;
        for (int i = 0; i < lights.Length; i++)
        {
            Light light = lights[i];
            if (light == null)
            {
                continue;
            }

            string name = light.gameObject.name;
            if (!IsLotoFillLightName(name))
            {
                continue;
            }

            if (IsCanonicalLightName(name))
            {
                continue;
            }

            removed++;
            DestroyLightObject(light.gameObject);
        }

        return removed;
    }

    private static void ConfigureCeilingKeyLights()
    {
        List<Light> keyLights = CollectKeyLights();
        if (keyLights.Count == 0)
        {
            return;
        }

        Light canonical = FindCanonicalKeyLight(keyLights);
        if (canonical == null)
        {
            return;
        }

        for (int i = 0; i < keyLights.Count; i++)
        {
            Light light = keyLights[i];
            if (light == null)
            {
                continue;
            }

            bool isCanonical = light == canonical;
            ApplyKeyLightSettings(light, isCanonical);
        }
    }

    private static List<Light> CollectKeyLights()
    {
        List<Light> keyLights = new List<Light>();
        Light[] lights = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < lights.Length; i++)
        {
            Light light = lights[i];
            if (light != null && IsLotoKeyLightName(light.gameObject.name))
            {
                keyLights.Add(light);
            }
        }

        return keyLights;
    }

    private static Light FindCanonicalKeyLight(List<Light> keyLights)
    {
        for (int i = 0; i < keyLights.Count; i++)
        {
            if (keyLights[i] != null && keyLights[i].gameObject.name == "LotoKeyIndustrial")
            {
                return keyLights[i];
            }
        }

        keyLights.Sort(CompareCeilingPriority);
        return keyLights.Count > 0 ? keyLights[0] : null;
    }

    private static int CompareCeilingPriority(Light a, Light b)
    {
        if (a == null && b == null)
        {
            return 0;
        }

        if (a == null)
        {
            return 1;
        }

        if (b == null)
        {
            return -1;
        }

        int heightCompare = b.transform.position.y.CompareTo(a.transform.position.y);
        if (heightCompare != 0)
        {
            return heightCompare;
        }

        return string.Compare(a.gameObject.name, b.gameObject.name, StringComparison.Ordinal);
    }

    private static void ApplyAntiFlickerToAllLotoLights()
    {
        Light[] lights = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < lights.Length; i++)
        {
            Light light = lights[i];
            if (light == null || !IsLotoManagedLightName(light.gameObject.name))
            {
                continue;
            }

            if (IsLotoKeyLightName(light.gameObject.name))
            {
                continue;
            }

            ApplyFillLightSettings(light);
        }
    }

    private static void ApplyKeyLightSettings(Light light, bool castShadows)
    {
        light.type = LightType.Spot;
        light.lightmapBakeType = LightmapBakeType.Mixed;
        light.renderMode = LightRenderMode.Auto;
        light.spotAngle = Mathf.Max(light.spotAngle, 80f);
        light.innerSpotAngle = Mathf.Clamp(light.innerSpotAngle, 45f, 60f);
        light.range = Mathf.Max(light.range, 16f);
        light.shadows = castShadows ? LightShadows.Hard : LightShadows.None;
        light.shadowNearPlane = ShadowNearPlane;
        light.shadowBias = 0.05f;
        light.shadowNormalBias = 0.3f;
        light.intensity = castShadows
            ? Mathf.Min(light.intensity, KeyIntensityMax)
            : Mathf.Min(light.intensity, AssistKeyIntensityMax);

        UniversalAdditionalLightData urpData = GetOrAddUrpLightData(light.gameObject);
        urpData.softShadowQuality = SoftShadowQuality.Low;
    }

    private static void ApplyFillLightSettings(Light light)
    {
        light.lightmapBakeType = LightmapBakeType.Mixed;
        light.renderMode = LightRenderMode.Auto;
        light.shadows = LightShadows.None;
    }

    private static bool IsLotoManagedLightName(string name)
    {
        return IsLotoKeyLightName(name) || IsLotoFillLightName(name);
    }

    private static bool IsLotoKeyLightName(string name)
    {
        return name.StartsWith("LotoKeyIndustrial", StringComparison.Ordinal);
    }

    private static bool IsLotoFillLightName(string name)
    {
        return name.StartsWith("LotoFill", StringComparison.Ordinal);
    }

    private static bool IsCanonicalLightName(string name)
    {
        for (int i = 0; i < CanonicalLightNames.Length; i++)
        {
            if (CanonicalLightNames[i] == name)
            {
                return true;
            }
        }

        return false;
    }

    private static int CountSceneLights()
    {
        return UnityEngine.Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
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

    private static void DestroyLightObject(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        UniversalAdditionalLightData urpData = target.GetComponent<UniversalAdditionalLightData>();
        if (urpData != null)
        {
            UnityEngine.Object.DestroyImmediate(urpData);
        }

        UnityEngine.Object.DestroyImmediate(target);
    }

    private static void EnsureLight(string name, Vector3 position, LightType type, float intensity, float range, Color color, bool key)
    {
        GameObject existing = GameObject.Find(name);
        bool created = existing == null;
        if (created)
        {
            existing = new GameObject(name);
            existing.transform.position = position;
            if (type == LightType.Spot)
            {
                existing.transform.rotation = Quaternion.Euler(75f, 0f, 0f);
            }
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
        light.renderMode = LightRenderMode.Auto;
        light.lightmapBakeType = LightmapBakeType.Mixed;
        if (type == LightType.Spot)
        {
            light.spotAngle = 95f;
            light.innerSpotAngle = 55f;
        }

        if (key)
        {
            ApplyKeyLightSettings(light, true);
        }
        else
        {
            ApplyFillLightSettings(light);
        }
    }

    private static UniversalAdditionalLightData GetOrAddUrpLightData(GameObject target)
    {
        UniversalAdditionalLightData urpData = target.GetComponent<UniversalAdditionalLightData>();
        if (urpData == null)
        {
            urpData = target.AddComponent<UniversalAdditionalLightData>();
        }

        return urpData;
    }
}
#endif
