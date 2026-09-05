#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class LotoPhase1PanelSetupTool
{
    private const string HudPrefabPath = "Assets/SafeMining/LOTOSimulation/Prefabs/PREF_LotoPlayerHUD.prefab";
    private const string BriefingPrefabPath = "Assets/SafeMining/LOTOSimulation/Prefabs/PREF_LotoBriefingPanel.prefab";

    [MenuItem("SafeMining/LOTO/Apply Phase 1 Panel Setup")]
    public static void ApplyPhase1PanelSetup()
    {
        ApplyHudPanelButtons();
        ApplyBriefingPanelButtons();
        EnableScenarioBriefingInScenes();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[LOTO Phase1] Panel setup applied to HUD and Briefing prefabs.");
    }

    private static void EnableScenarioBriefingInScenes()
    {
        string[] scenePaths =
        {
            "Assets/SafeMining/LOTOSimulation/Scenes/VR-LOTO-S1.unity",
            "Assets/SafeMining/LOTOSimulation/Scenes/VR-LOTO-S2.unity"
        };

        for (int i = 0; i < scenePaths.Length; i++)
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePaths[i], UnityEditor.SceneManagement.OpenSceneMode.Single);
            LotoScenarioSceneBootstrap[] bootstraps = Object.FindObjectsByType<LotoScenarioSceneBootstrap>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int j = 0; j < bootstraps.Length; j++)
            {
                SerializedObject serialized = new SerializedObject(bootstraps[j]);
                serialized.FindProperty("startWithBriefing").boolValue = true;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        }
    }

    private static void ApplyHudPanelButtons()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(HudPrefabPath);
        ref_PlayerHUD hud = root.GetComponent<ref_PlayerHUD>();
        if (hud == null)
        {
            PrefabUtility.UnloadPrefabContents(root);
            return;
        }

        Transform canvasRoot = root.transform;
        Button reopenButton = EnsureButton(canvasRoot, "HudReopenButton", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(220f, 64f), new Color(0.976f, 0.451f, 0.086f, 1f));
        Button reopenExitButton = EnsureButton(canvasRoot, "HudReopenExitButton", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(140f, 24f), new Vector2(120f, 64f), new Color(0.102f, 0.212f, 0.365f, 0.92f));
        Button exitToMenuButton = EnsureButton(canvasRoot, "ExitToMenuButton", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-24f, 24f), new Vector2(140f, 56f), new Color(0.102f, 0.212f, 0.365f, 0.92f));

        Transform menuButton = canvasRoot.Find("MenuButton");
        if (menuButton != null)
        {
            RectTransform menuRect = menuButton.GetComponent<RectTransform>();
            if (menuRect != null)
            {
                menuRect.anchorMin = new Vector2(0f, 0f);
                menuRect.anchorMax = new Vector2(0f, 0f);
                menuRect.pivot = new Vector2(0f, 0f);
                menuRect.anchoredPosition = new Vector2(24f, 24f);
                menuRect.sizeDelta = new Vector2(160f, 56f);
            }
        }

        if (hud.timerText != null)
        {
            hud.timerText.gameObject.SetActive(false);
        }

        if (hud.timerFill != null)
        {
            hud.timerFill.gameObject.SetActive(false);
        }

        hud.hudReopenButton = reopenButton;
        hud.hudReopenExitButton = reopenExitButton;
        hud.exitToMenuButton = exitToMenuButton;
        reopenButton.gameObject.SetActive(false);

        PrefabUtility.SaveAsPrefabAsset(root, HudPrefabPath);
        PrefabUtility.UnloadPrefabContents(root);
    }

    private static void ApplyBriefingPanelButtons()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(BriefingPrefabPath);
        ref_BriefingPanel briefing = root.GetComponent<ref_BriefingPanel>();
        if (briefing == null)
        {
            PrefabUtility.UnloadPrefabContents(root);
            return;
        }

        Transform canvasRoot = root.transform;
        GameObject contentRoot = EnsureChild(canvasRoot, "BriefingContent");
        ReparentExistingBriefingChildren(canvasRoot, contentRoot.transform, briefing);

        Button closeButton = EnsureButton(contentRoot.transform, "CloseButton", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -24f), new Vector2(56f, 56f), new Color(0.2f, 0.25f, 0.32f, 0.85f));
        SetButtonLabel(closeButton, string.Empty, 22);

        Transform beginTransform = contentRoot.transform.Find("BeginButton");
        if (beginTransform != null)
        {
            RectTransform beginRect = beginTransform.GetComponent<RectTransform>();
            if (beginRect != null)
            {
                beginRect.anchorMin = new Vector2(0.5f, 0f);
                beginRect.anchorMax = new Vector2(0.5f, 0f);
                beginRect.pivot = new Vector2(0.5f, 0f);
                beginRect.anchoredPosition = new Vector2(0f, 32f);
                beginRect.sizeDelta = new Vector2(240f, 64f);
            }
        }

        Button reopenButton = EnsureButton(canvasRoot, "BriefingReopenButton", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(240f, 64f), new Color(0.976f, 0.451f, 0.086f, 1f));
        reopenButton.gameObject.SetActive(false);

        briefing.rootPanel = root;
        briefing.contentRoot = contentRoot;
        briefing.closeButton = closeButton;
        briefing.reopenButton = reopenButton;
        if (beginTransform != null)
        {
            briefing.beginButton = beginTransform.GetComponent<Button>();
        }

        PrefabUtility.SaveAsPrefabAsset(root, BriefingPrefabPath);
        PrefabUtility.UnloadPrefabContents(root);
    }

    private static void ReparentExistingBriefingChildren(Transform canvasRoot, Transform contentRoot, ref_BriefingPanel briefing)
    {
        string[] childNames = { "Background", "Title", "Body", "BeginButton" };
        for (int i = 0; i < childNames.Length; i++)
        {
            Transform child = canvasRoot.Find(childNames[i]);
            if (child != null && child.parent != contentRoot)
            {
                child.SetParent(contentRoot, false);
            }
        }

        Transform title = contentRoot.Find("Title");
        Transform body = contentRoot.Find("Body");
        Transform begin = contentRoot.Find("BeginButton");
        if (briefing.titleText == null && title != null)
        {
            briefing.titleText = title.GetComponent<TMP_Text>();
        }

        if (briefing.bodyText == null && body != null)
        {
            briefing.bodyText = body.GetComponent<TMP_Text>();
        }

        if (briefing.beginButton == null && begin != null)
        {
            briefing.beginButton = begin.GetComponent<Button>();
        }
    }

    private static GameObject EnsureChild(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
        {
            return existing.gameObject;
        }

        GameObject child = new GameObject(name, typeof(RectTransform));
        child.transform.SetParent(parent, false);
        RectTransform rect = child.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return child;
    }

    private static Button EnsureButton(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta, Color color)
    {
        Transform existing = parent.Find(name);
        GameObject buttonObject;
        if (existing != null)
        {
            buttonObject = existing.gameObject;
        }
        else
        {
            buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            SetButtonLabel(buttonObject.GetComponent<Button>() ?? buttonObject.AddComponent<Button>(), string.Empty, 22);
        }

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;

        Image image = buttonObject.GetComponent<Image>();
        if (image != null)
        {
            image.color = color;
        }

        return buttonObject.GetComponent<Button>();
    }

    private static void SetButtonLabel(Button button, string text, float fontSize)
    {
        if (button == null)
        {
            return;
        }

        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
        if (label == null)
        {
            GameObject labelObject = new GameObject("Label", typeof(RectTransform));
            labelObject.transform.SetParent(button.transform, false);
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            label = labelObject.AddComponent<TextMeshProUGUI>();
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
        }

        label.fontSize = fontSize;
        label.text = text;
    }
}
#endif
