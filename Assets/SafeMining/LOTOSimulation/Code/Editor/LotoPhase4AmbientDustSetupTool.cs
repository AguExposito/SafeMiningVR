#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LotoPhase4AmbientDustSetupTool
{
    private const string MaterialPath = "Assets/SafeMining/LOTOSimulation/Art/Materials/MAT_LotoAmbientDust.mat";
    private const string PrefabPath = "Assets/SafeMining/LOTOSimulation/Prefabs/PREF_LotoAmbientDust.prefab";

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

    [MenuItem("SafeMining/LOTO/Apply Phase 4 Ambient Dust Setup")]
    public static void ApplyPhase4AmbientDustSetup()
    {
        Material dustMaterial = EnsureDustMaterial();
        EnsureDustPrefab(dustMaterial);
        AddDustPrefabToScenes();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[LOTO Phase4] Ambient dust prefab and scene instances configured.");
    }

    private static Material EnsureDustMaterial()
    {
        string directory = Path.GetDirectoryName(MaterialPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Particles/Standard Unlit");
            }

            material = new Material(shader);
            AssetDatabase.CreateAsset(material, MaterialPath);
        }

        material.name = "MAT_LotoAmbientDust";
        material.SetColor("_BaseColor", new Color(0.72f, 0.68f, 0.6f, 0.16f));
        material.SetFloat("_Surface", 1f);
        material.SetFloat("_Blend", 0f);
        material.renderQueue = 3000;
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void EnsureDustPrefab(Material dustMaterial)
    {
        string directory = Path.GetDirectoryName(PrefabPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        GameObject root = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (root == null)
        {
            GameObject temp = new GameObject("PREF_LotoAmbientDust");
            PrefabUtility.SaveAsPrefabAsset(temp, PrefabPath);
            Object.DestroyImmediate(temp);
        }

        GameObject instance = PrefabUtility.LoadPrefabContents(PrefabPath);
        ParticleSystem particleSystem = instance.GetComponent<ParticleSystem>();
        if (particleSystem == null)
        {
            particleSystem = instance.AddComponent<ParticleSystem>();
        }

        LotoAmbientDustController controller = instance.GetComponent<LotoAmbientDustController>();
        if (controller == null)
        {
            controller = instance.AddComponent<LotoAmbientDustController>();
        }

        ConfigureParticleSystem(particleSystem, dustMaterial);
        SerializedObject controllerSerialized = new SerializedObject(controller);
        controllerSerialized.FindProperty("dustParticles").objectReferenceValue = particleSystem;
        controllerSerialized.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SaveAsPrefabAsset(instance, PrefabPath);
        PrefabUtility.UnloadPrefabContents(instance);
    }

    private static void ConfigureParticleSystem(ParticleSystem particleSystem, Material dustMaterial)
    {
        ParticleSystem.MainModule main = particleSystem.main;
        main.loop = true;
        main.prewarm = true;
        main.playOnAwake = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(8f, 14f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.012f, 0.035f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.006f, 0.016f);
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.72f, 0.68f, 0.6f, 0.16f));
        main.maxParticles = 48;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.gravityModifier = -0.015f;

        ParticleSystem.EmissionModule emission = particleSystem.emission;
        emission.enabled = true;
        emission.rateOverTime = 6f;

        ParticleSystem.ShapeModule shape = particleSystem.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(3.2f, 0.5f, 3.2f);

        ParticleSystem.VelocityOverLifetimeModule velocity = particleSystem.velocityOverLifetime;
        velocity.enabled = true;
        velocity.y = new ParticleSystem.MinMaxCurve(0.008f, 0.025f);

        ParticleSystem.NoiseModule noise = particleSystem.noise;
        noise.enabled = true;
        noise.strength = 0.06f;
        noise.frequency = 0.3f;
        noise.scrollSpeed = 0.08f;

        ParticleSystem.CollisionModule collision = particleSystem.collision;
        collision.enabled = false;

        ParticleSystem.LightsModule lights = particleSystem.lights;
        lights.enabled = false;

        ParticleSystemRenderer renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = dustMaterial;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
    }

    private static void AddDustPrefabToScenes()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null)
        {
            return;
        }

        for (int i = 0; i < ScenePaths.Length; i++)
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePaths[i], OpenSceneMode.Single);
            Transform keyLight = FindKeyLightTransform();
            Transform sceneRoot = FindSceneRoot(SceneRootNames[i]);
            Transform parent = keyLight != null ? keyLight : sceneRoot;
            if (parent == null)
            {
                Debug.LogWarning("[LOTO Phase4] Missing placement parent in " + scene.name);
                continue;
            }

            Transform existing = parent.Find("PREF_LotoAmbientDust");
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject;
            if (instance == null)
            {
                continue;
            }

            instance.name = "PREF_LotoAmbientDust";
            if (keyLight != null)
            {
                instance.transform.localPosition = new Vector3(0f, -0.35f, 0f);
                instance.transform.localRotation = Quaternion.identity;
            }
            else
            {
                instance.transform.localPosition = Vector3.zero;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
    }

    private static Transform FindKeyLightTransform()
    {
        Light[] lights = Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < lights.Length; i++)
        {
            Light light = lights[i];
            if (light != null && light.gameObject.name == "LotoKeyIndustrial")
            {
                return light.transform;
            }
        }

        for (int i = 0; i < lights.Length; i++)
        {
            Light light = lights[i];
            if (light != null && light.gameObject.name.StartsWith("LotoKeyIndustrial"))
            {
                return light.transform;
            }
        }

        return null;
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
