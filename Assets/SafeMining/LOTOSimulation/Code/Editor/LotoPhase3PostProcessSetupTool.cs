#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public static class LotoPhase3PostProcessSetupTool
{
    private const string VolumeProfilePath = "Assets/Resources/VOL_LotoUnderground.asset";
    private const string VolumePrefabPath = "Assets/SafeMining/LOTOSimulation/Prefabs/PREF_LotoPostProcessVolume.prefab";
    private const string AndroidRendererPath = "Assets/URPDefaultResources/Android Preset.asset";
    private const string ForwardRendererPath = "Assets/URPDefaultResources/Default_Forward_Renderer.asset";

    private static readonly string[] ScenePaths =
    {
        "Assets/SafeMining/LOTOSimulation/Scenes/VR-LOTO-S1.unity",
        "Assets/SafeMining/LOTOSimulation/Scenes/VR-LOTO-S2.unity"
    };

    private static readonly string[] SceneRootNames =
    {
        "LOTO_S1Root",
        "LOTO_S2Root"
    };

    [MenuItem("SafeMining/LOTO/Apply Phase 3 Post Process Setup")]
    public static void ApplyPhase3PostProcessSetup()
    {
        VolumeProfile profile = EnsureVolumeProfile();
        if (profile == null)
        {
            Debug.LogError("[LOTO Phase3] Volume profile creation failed.");
            return;
        }

        EnsurePostProcessPrefab(profile);
        WireAndroidRendererPostProcess();
        AddVolumePrefabToScenes();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[LOTO Phase3] Post-process volume, Android renderer, and scene instances configured.");
    }

    private static VolumeProfile EnsureVolumeProfile()
    {
        string directory = Path.GetDirectoryName(VolumeProfilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        const string templatePath = "Assets/DefaultVolumeProfile.asset";
        if (AssetDatabase.LoadAssetAtPath<VolumeProfile>(VolumeProfilePath) != null)
        {
            AssetDatabase.DeleteAsset(VolumeProfilePath);
        }

        if (!AssetDatabase.CopyAsset(templatePath, VolumeProfilePath))
        {
            Debug.LogError("[LOTO Phase3] Failed to copy DefaultVolumeProfile template.");
            return null;
        }

        VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(VolumeProfilePath);
        if (profile == null)
        {
            return null;
        }

        profile.name = "VOL_LotoUnderground";
        for (int i = 0; i < profile.components.Count; i++)
        {
            VolumeComponent component = profile.components[i];
            if (component == null)
            {
                continue;
            }

            bool keepActive = component is Bloom || component is ColorAdjustments;
            component.active = keepActive;
        }

        if (profile.TryGet(out Bloom bloom))
        {
            bloom.active = true;
            bloom.threshold.Override(0.8f);
            bloom.intensity.Override(0.55f);
            bloom.scatter.Override(0.65f);
            bloom.highQualityFiltering.Override(false);
            bloom.maxIterations.Override(4);
        }

        if (profile.TryGet(out ColorAdjustments colorAdjustments))
        {
            colorAdjustments.active = true;
            colorAdjustments.postExposure.Override(0.08f);
            colorAdjustments.contrast.Override(4f);
            colorAdjustments.saturation.Override(2f);
        }

        EditorUtility.SetDirty(profile);
        AssetDatabase.SaveAssets();
        return profile;
    }

    private static void EnsurePostProcessPrefab(VolumeProfile profile)
    {
        string directory = Path.GetDirectoryName(VolumePrefabPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        GameObject root = AssetDatabase.LoadAssetAtPath<GameObject>(VolumePrefabPath);
        if (root == null)
        {
            root = new GameObject("PREF_LotoPostProcessVolume");
            PrefabUtility.SaveAsPrefabAsset(root, VolumePrefabPath);
            Object.DestroyImmediate(root);
            root = AssetDatabase.LoadAssetAtPath<GameObject>(VolumePrefabPath);
        }

        GameObject instance = PrefabUtility.LoadPrefabContents(VolumePrefabPath);
        Volume volume = instance.GetComponent<Volume>();
        if (volume == null)
        {
            volume = instance.AddComponent<Volume>();
        }

        volume.isGlobal = true;
        volume.priority = 15f;
        volume.weight = 1f;
        volume.sharedProfile = profile;
        PrefabUtility.SaveAsPrefabAsset(instance, VolumePrefabPath);
        PrefabUtility.UnloadPrefabContents(instance);
    }

    private static void WireAndroidRendererPostProcess()
    {
        UniversalRendererData androidRenderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(AndroidRendererPath);
        UniversalRendererData forwardRenderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(ForwardRendererPath);
        if (androidRenderer == null || forwardRenderer == null || forwardRenderer.postProcessData == null)
        {
            Debug.LogWarning("[LOTO Phase3] Could not wire Android renderer postProcessData.");
            return;
        }

        androidRenderer.postProcessData = forwardRenderer.postProcessData;
        EditorUtility.SetDirty(androidRenderer);
    }

    private static void AddVolumePrefabToScenes()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(VolumePrefabPath);
        if (prefab == null)
        {
            return;
        }

        for (int i = 0; i < ScenePaths.Length; i++)
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePaths[i], OpenSceneMode.Single);
            Transform root = FindSceneRoot(SceneRootNames[i]);
            if (root == null)
            {
                Debug.LogWarning("[LOTO Phase3] Missing root " + SceneRootNames[i] + " in " + scene.name);
                continue;
            }

            Transform existing = root.Find("PREF_LotoPostProcessVolume");
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab, root) as GameObject;
            if (instance != null)
            {
                instance.name = "PREF_LotoPostProcessVolume";
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
    }

    private static Transform FindSceneRoot(string rootName)
    {
        GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            if (roots[i] != null && roots[i].name == rootName)
            {
                return roots[i].transform;
            }
        }

        return null;
    }
}
#endif
