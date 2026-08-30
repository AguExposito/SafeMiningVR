#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class LotoMenuBrandSetupTool
{
    private const string MenuScenePath = "Assets/XR Standard Menu/Level/Scenes/StandardMainMenu.unity";

    [MenuItem("SafeMining/LOTO/Apply Menu Brand Setup")]
    public static void ApplyMenuBrandSetup()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("LOTO Menu Brand", "Stop Play Mode before applying menu brand setup.", "OK");
            return;
        }

        EditorSceneManager.OpenScene(MenuScenePath);
        int applied = 0;
        Image[] images = Object.FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < images.Length; i++)
        {
            Image image = images[i];
            if (image == null)
            {
                continue;
            }

            if (image.gameObject.name == "Logo")
            {
                applied += ApplyBrand(image.gameObject, BrandGraphicRole.Logo, BrandColorRole.None);
                continue;
            }

            if (image.gameObject.name.Contains("Play") || image.gameObject.name.Contains("Start"))
            {
                applied += ApplyBrand(image.gameObject, BrandGraphicRole.None, BrandColorRole.Primary);
            }
        }

        Button[] buttons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            if (button == null)
            {
                continue;
            }

            Image image = button.GetComponent<Image>();
            if (image == null)
            {
                continue;
            }

            if (button.gameObject.name.Contains("Next") || button.gameObject.name.Contains("Prev"))
            {
                applied += ApplyBrand(image.gameObject, BrandGraphicRole.None, BrandColorRole.Secondary);
            }
        }

        TMP_Text[] texts = Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < texts.Length; i++)
        {
            TMP_Text text = texts[i];
            if (text == null)
            {
                continue;
            }

            TranslatableText translatable = text.GetComponent<TranslatableText>();
            if (translatable != null && translatable.translationID == LotoObjectiveIds.SimulationSceneId)
            {
                applied += ApplyBrand(text.gameObject, BrandGraphicRole.None, BrandColorRole.Secondary);
            }
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        Debug.Log("[LOTO Menu Brand] BrandApplicator applied count=" + applied);
    }

    private static int ApplyBrand(GameObject target, BrandGraphicRole graphicRole, BrandColorRole colorRole)
    {
        BrandApplicator applicator = target.GetComponent<BrandApplicator>();
        if (applicator == null)
        {
            applicator = target.AddComponent<BrandApplicator>();
        }

        SerializedObject serialized = new SerializedObject(applicator);
        serialized.FindProperty("graphicRole").enumValueIndex = (int)graphicRole;
        serialized.FindProperty("colorRole").enumValueIndex = (int)colorRole;
        serialized.FindProperty("textRole").enumValueIndex = (int)BrandTextRole.None;
        serialized.FindProperty("applyColorToGraphic").boolValue = true;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        applicator.Apply();
        return 1;
    }
}
#endif
