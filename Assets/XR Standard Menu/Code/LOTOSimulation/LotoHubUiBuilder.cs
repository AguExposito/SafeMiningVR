using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XRStandardMenu;

public static class LotoHubUiBuilder
{
    private const float WorldCanvasScale = 0.001f;

    public static ref_PlayerHUD EnsurePlayerHud(Transform followTarget, GameObject objectiveRowPrefab)
    {
        ref_PlayerHUD existing = Object.FindFirstObjectByType<ref_PlayerHUD>();
        if (existing != null)
        {
            return existing;
        }

        GameObject root = CreateWorldCanvasRoot("PREF_LotoPlayerHUD", new Vector3(0f, 1.5f, 1.8f), followTarget, active: false);
        ref_PlayerHUD hud = root.AddComponent<ref_PlayerHUD>();
        hud.rootPanel = root;
        hud.activeObjectiveTitle = CreateText(root.transform, "ActiveObjectiveTitle", new Vector2(0f, 1f), new Vector2(0.55f, 1f), new Vector2(20f, -20f), new Vector2(-40f, 48f), 28);
        hud.objectivesContainer = CreateStretchPanel(root.transform, "ObjectivesContainer", new Vector2(0f, 0.35f), new Vector2(0.55f, 0.95f), new Vector2(20f, 20f), new Vector2(-20f, -80f));
        hud.timerText = CreateText(root.transform, "TimerText", new Vector2(0.65f, 1f), new Vector2(1f, 1f), new Vector2(-20f, -20f), new Vector2(-40f, 48f), 28);
        hud.timerText.text = "00:00";
        hud.timerFill = CreateFilledImage(root.transform, "TimerFill", new Vector2(0.65f, 0.88f), new Vector2(1f, 0.92f), new Vector2(20f, 0f), new Vector2(-20f, 0f));
        hud.scenarioIndicatorText = CreateText(root.transform, "ScenarioIndicator", new Vector2(0.65f, 0.8f), new Vector2(1f, 0.86f), new Vector2(20f, 0f), new Vector2(-20f, 0f), 24);
        hud.subtitleText = CreateText(root.transform, "SubtitleText", new Vector2(0f, 0f), new Vector2(1f, 0.25f), new Vector2(20f, 20f), new Vector2(-20f, 20f), 24);
        hud.subtitleText.alignment = TextAlignmentOptions.Bottom;
        hud.menuButtonRoot = CreateButton(root.transform, "MenuButton", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-80f, -80f), new Vector2(120f, 48f), out hud.menuButton);
        hud.exitConfirmPanel = CreatePanel(root.transform, "ExitConfirmPanel", new Vector2(0.2f, 0.35f), new Vector2(0.8f, 0.65f), Vector2.zero, Vector2.zero, active: false);
        hud.exitConfirmTitleText = CreateText(hud.exitConfirmPanel.transform, "ExitConfirmTitle", new Vector2(0f, 0.65f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero, 30);
        hud.exitConfirmTitleText.alignment = TextAlignmentOptions.Center;
        hud.exitConfirmBodyText = CreateText(hud.exitConfirmPanel.transform, "ExitConfirmBody", new Vector2(0f, 0.35f), new Vector2(1f, 0.65f), new Vector2(16f, 0f), new Vector2(-16f, 0f), 22);
        hud.exitConfirmBodyText.alignment = TextAlignmentOptions.Top;
        CreateButton(hud.exitConfirmPanel.transform, "ExitConfirmYes", new Vector2(0.1f, 0.05f), new Vector2(0.45f, 0.25f), Vector2.zero, Vector2.zero, out hud.exitConfirmYes);
        CreateButton(hud.exitConfirmPanel.transform, "ExitConfirmNo", new Vector2(0.55f, 0.05f), new Vector2(0.9f, 0.25f), Vector2.zero, Vector2.zero, out hud.exitConfirmNo);
        hud.objectiveRowPrefab = objectiveRowPrefab;
        return hud;
    }

    public static ref_BriefingPanel EnsureBriefingPanel(Transform followTarget)
    {
        ref_BriefingPanel existing = Object.FindFirstObjectByType<ref_BriefingPanel>();
        if (existing != null)
        {
            return existing;
        }

        GameObject root = CreateWorldCanvasRoot("PREF_LotoBriefingPanel", new Vector3(0f, 1.5f, 2f), followTarget, active: true);
        ref_BriefingPanel panel = root.AddComponent<ref_BriefingPanel>();
        panel.rootPanel = root;
        panel.titleText = CreateText(root.transform, "Title", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -40f), new Vector2(-40f, 60f), 34);
        panel.bodyText = CreateText(root.transform, "Body", new Vector2(0f, 0.2f), new Vector2(1f, 0.85f), new Vector2(30f, 20f), new Vector2(-30f, -20f), 24);
        panel.bodyText.alignment = TextAlignmentOptions.Top;
        CreateButton(root.transform, "BeginButton", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(220f, 56f), out panel.beginButton);
        return panel;
    }

    private static GameObject CreateWorldCanvasRoot(string name, Vector3 localPosition, Transform followTarget, bool active)
    {
        GameObject root = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        root.layer = LayerMask.NameToLayer("UI");
        root.SetActive(active);

        RectTransform rect = root.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(1200f, 800f);
        rect.localScale = Vector3.one * WorldCanvasScale;
        rect.localPosition = localPosition;

        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10f;

        if (followTarget != null)
        {
            Util_PanelFollower follower = root.AddComponent<Util_PanelFollower>();
            follower.SetTarget(followTarget);
        }

        return root;
    }

    private static Transform CreateStretchPanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform));
        panel.transform.SetParent(parent, false);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
        return rect;
    }

    private static GameObject CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, bool active)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(parent, false);
        panel.SetActive(active);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
        panel.GetComponent<Image>().color = new Color(0.102f, 0.212f, 0.365f, 0.95f);
        return panel;
    }

    private static TMP_Text CreateText(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetOrPosition, Vector2 sizeOrOffsetMax, float fontSize)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        if (anchorMin == anchorMax)
        {
            rect.anchoredPosition = offsetOrPosition;
            rect.sizeDelta = sizeOrOffsetMax;
        }
        else
        {
            rect.offsetMin = offsetOrPosition;
            rect.offsetMax = sizeOrOffsetMax;
        }

        TMP_Text text = textObject.GetComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        return text;
    }

    private static Image CreateFilledImage(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(parent, false);
        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
        Image image = imageObject.GetComponent<Image>();
        image.type = Image.Type.Filled;
        image.fillMethod = Image.FillMethod.Horizontal;
        image.color = new Color(0.133f, 0.773f, 0.369f, 1f);
        return image;
    }

    private static GameObject CreateButton(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta, out Button button)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.976f, 0.451f, 0.086f, 1f);
        button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        return buttonObject;
    }
}
