using System.Collections.Generic;
using UnityEngine;

public class LotoGloveHandVisuals : MonoBehaviour
{
    private static LotoGloveHandVisuals instance;

    [SerializeField] private Material gloveMaterial;
    [SerializeField] private Color gloveBaseColor = new Color(0.08f, 0.08f, 0.09f, 1f);
    [SerializeField] private Color gloveAccentColor = new Color(0.92f, 0.78f, 0.12f, 1f);

    private readonly List<Renderer> handRenderers = new List<Renderer>();
    private readonly List<Material[]> originalMaterials = new List<Material[]>();
    private bool glovesApplied;

    #region Public API

    public static void SetGlovesEquipped(bool equipped)
    {
        EnsureInstance();
        if (instance != null)
        {
            instance.ApplyGloves(equipped);
        }
    }

    #endregion

    #region Lifecycle

    private void Awake()
    {
        instance = this;
        CacheHandRenderers();
        EnsureGloveMaterial();
    }

    private void OnDestroy()
    {
        if (glovesApplied)
        {
            RestoreOriginalMaterials();
        }

        if (instance == this)
        {
            instance = null;
        }
    }

    #endregion

    #region Helpers

    private static void EnsureInstance()
    {
        if (instance != null)
        {
            return;
        }

        GameObject player = GameObject.Find("PlayerMR");
        if (player == null)
        {
            return;
        }

        instance = player.GetComponent<LotoGloveHandVisuals>();
        if (instance == null)
        {
            instance = player.AddComponent<LotoGloveHandVisuals>();
        }

        instance.CacheHandRenderers();
        instance.EnsureGloveMaterial();
    }

    private void CacheHandRenderers()
    {
        handRenderers.Clear();
        originalMaterials.Clear();

        CollectUnderName("Left Hand");
        CollectUnderName("Right Hand");
        CollectUnderName("Hand Visualizer");
        CollectUnderName("LeftHandQuestVisual");
        CollectUnderName("RightHandQuestVisual");
    }

    private void CollectUnderName(string objectName)
    {
        Transform root = FindDeepChild(transform.root != null ? transform.root : transform, objectName);
        if (root == null && transform != null)
        {
            GameObject found = GameObject.Find(objectName);
            root = found != null ? found.transform : null;
        }

        if (root == null)
        {
            return;
        }

        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null || handRenderers.Contains(renderer))
            {
                continue;
            }

            handRenderers.Add(renderer);
            originalMaterials.Add(renderer.sharedMaterials);
        }
    }

    private static Transform FindDeepChild(Transform parent, string name)
    {
        if (parent == null)
        {
            return null;
        }

        if (parent.name == name)
        {
            return parent;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform found = FindDeepChild(parent.GetChild(i), name);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private void EnsureGloveMaterial()
    {
        if (gloveMaterial != null)
        {
            return;
        }

        Material loaded = Resources.Load<Material>("MAT_S1_GloveHands");
        if (loaded != null)
        {
            gloveMaterial = loaded;
            return;
        }

        Shader lit = Shader.Find("Universal Render Pipeline/Lit");
        if (lit == null)
        {
            lit = Shader.Find("Standard");
        }

        gloveMaterial = new Material(lit);
        gloveMaterial.name = "MAT_S1_GloveHands_Runtime";
        if (gloveMaterial.HasProperty("_BaseColor"))
        {
            gloveMaterial.SetColor("_BaseColor", gloveBaseColor);
        }
        else if (gloveMaterial.HasProperty("_Color"))
        {
            gloveMaterial.SetColor("_Color", gloveBaseColor);
        }

        if (gloveMaterial.HasProperty("_EmissionColor"))
        {
            gloveMaterial.EnableKeyword("_EMISSION");
            gloveMaterial.SetColor("_EmissionColor", gloveAccentColor * 0.45f);
        }

        Texture2D albedo = Resources.Load<Texture2D>("MechGloves_Low_MechGloves_AlbedoTransparen");
        if (albedo == null)
        {
            albedo = LoadTextureFromModels();
        }

        if (albedo != null && gloveMaterial.HasProperty("_BaseMap"))
        {
            gloveMaterial.SetTexture("_BaseMap", albedo);
        }
        else if (albedo != null && gloveMaterial.HasProperty("_MainTex"))
        {
            gloveMaterial.SetTexture("_MainTex", albedo);
        }
    }

    private static Texture2D LoadTextureFromModels()
    {
#if UNITY_EDITOR
        return UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(
            "Assets/SafeMining/Models/EPP/Textures/MechGloves_Low_MechGloves_AlbedoTransparen.png");
#else
        return null;
#endif
    }

    private void ApplyGloves(bool equipped)
    {
        if (handRenderers.Count == 0)
        {
            CacheHandRenderers();
        }

        EnsureGloveMaterial();
        if (equipped)
        {
            for (int i = 0; i < handRenderers.Count; i++)
            {
                Renderer renderer = handRenderers[i];
                if (renderer == null || gloveMaterial == null)
                {
                    continue;
                }

                Material[] mats = renderer.sharedMaterials;
                Material[] gloves = new Material[mats.Length > 0 ? mats.Length : 1];
                for (int m = 0; m < gloves.Length; m++)
                {
                    gloves[m] = gloveMaterial;
                }

                renderer.sharedMaterials = gloves;
            }

            glovesApplied = true;
            return;
        }

        RestoreOriginalMaterials();
        glovesApplied = false;
    }

    private void RestoreOriginalMaterials()
    {
        for (int i = 0; i < handRenderers.Count; i++)
        {
            if (handRenderers[i] != null && i < originalMaterials.Count && originalMaterials[i] != null)
            {
                handRenderers[i].sharedMaterials = originalMaterials[i];
            }
        }
    }

    #endregion
}
