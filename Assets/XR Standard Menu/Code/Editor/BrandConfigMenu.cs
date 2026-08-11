using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class BrandConfigMenu
{
    private const string ResourcesFolder = "Assets/Resources";
    private const string AssetPath = "Assets/Resources/BrandConfig.asset";
    private const string DefaultLogoPath = "Assets/XR Standard Menu/Level/UI/Logos/BrandLogo.png";

    [MenuItem("XR Standard Menu/Branding/Create Or Select Brand Config")]
    public static void CreateOrSelectBrandConfig()
    {
        BrandConfig config = EnsureAsset();
        Selection.activeObject = config;
        EditorGUIUtility.PingObject(config);
    }

    [MenuItem("XR Standard Menu/Branding/Sync Player Settings From Brand Config")]
    public static void SyncPlayerSettingsFromBrandConfig()
    {
        BrandConfig config = EnsureAsset();
        PlayerSettings.companyName = config.CompanyName;
        PlayerSettings.productName = config.ProductName;
        Debug.Log($"[BrandConfig] Player Settings synced. company={config.CompanyName}, product={config.ProductName}");
    }

    [MenuItem("XR Standard Menu/Branding/Apply Branding In Open Scenes")]
    public static void ApplyBrandingInOpenScenes()
    {
        BrandApplicator[] applicators = Object.FindObjectsByType<BrandApplicator>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < applicators.Length; i++)
        {
            applicators[i].Apply();
            EditorUtility.SetDirty(applicators[i]);
        }

        Image[] images = Object.FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int logosUpdated = 0;
        Sprite logo = Brand.Logo;
        if (logo != null)
        {
            for (int i = 0; i < images.Length; i++)
            {
                if (images[i] != null && images[i].gameObject.name == "Logo")
                {
                    images[i].sprite = logo;
                    images[i].preserveAspect = true;
                    EditorUtility.SetDirty(images[i]);
                    logosUpdated++;
                }
            }
        }

        Debug.Log($"[BrandConfig] Applied branding. applicators={applicators.Length}, named Logos={logosUpdated}");
    }

    public static BrandConfig EnsureAsset()
    {
        BrandConfig config = AssetDatabase.LoadAssetAtPath<BrandConfig>(AssetPath);
        if (config != null)
        {
            return config;
        }

        if (!AssetDatabase.IsValidFolder(ResourcesFolder))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        config = ScriptableObject.CreateInstance<BrandConfig>();
        AssetDatabase.CreateAsset(config, AssetPath);

        SerializedObject so = new SerializedObject(config);
        so.FindProperty("companyName").stringValue = string.IsNullOrEmpty(PlayerSettings.companyName)
            ? "DefaultCompany"
            : PlayerSettings.companyName;
        so.FindProperty("productName").stringValue = string.IsNullOrEmpty(PlayerSettings.productName)
            ? "XR Menu"
            : PlayerSettings.productName;

        Sprite logo = AssetDatabase.LoadAssetAtPath<Sprite>(DefaultLogoPath);
        if (logo != null)
        {
            so.FindProperty("logo").objectReferenceValue = logo;
        }

        so.FindProperty("primaryColor").colorValue = new Color(0.12f, 0.45f, 0.58f, 1f);
        so.FindProperty("secondaryColor").colorValue = new Color(0.18f, 0.20f, 0.22f, 1f);
        so.FindProperty("tertiaryColor").colorValue = new Color(0.91f, 0.94f, 0.96f, 1f);
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Brand.ClearCache();
        Debug.Log($"[BrandConfig] Created asset at {AssetPath}");
        return config;
    }
}

[CustomEditor(typeof(BrandConfig))]
public class BrandConfigInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GUILayout.Space(10f);
        EditorGUILayout.HelpBox(
            "Cambia nombres, logo y colores aquí. Luego usa el menú XR Standard Menu/Branding para sincronizar Player Settings o aplicar en escenas abiertas.",
            MessageType.Info);

        if (GUILayout.Button("Sync Player Settings", GUILayout.Height(28f)))
        {
            BrandConfigMenu.SyncPlayerSettingsFromBrandConfig();
        }
        if (GUILayout.Button("Apply Branding In Open Scenes", GUILayout.Height(28f)))
        {
            BrandConfigMenu.ApplyBrandingInOpenScenes();
        }
    }
}
