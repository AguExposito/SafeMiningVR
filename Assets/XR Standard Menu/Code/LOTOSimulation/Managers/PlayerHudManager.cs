using System;
using System.Text;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XRStandardMenu;

public class PlayerHudManager : MonoBehaviour
{
    [SerializeField] private Sprite objectivePendingSprite;
    [SerializeField] private Sprite objectiveAchievedSprite;
    [SerializeField] private Color primaryPanelColor = new Color(0.102f, 0.212f, 0.365f, 0.92f);
    [SerializeField] private Color secondaryAccentColor = new Color(0.976f, 0.451f, 0.086f, 1f);

    private ref_PlayerHUD playerHud;
    private ref_BriefingPanel briefingPanel;
    private SimulationOrchestratorManager orchestrator;
    private readonly List<ref_LotoObjectiveRow> objectiveRows = new List<ref_LotoObjectiveRow>();
    private float nextTimerRefreshTime;
    private float subtitleHideTime;
    private bool hudChromeVisible = true;
    private bool briefingPanelVisible = true;
    private LotoSimulationState lastAppliedState = LotoSimulationState.Initializing;
    private LotoPanelVisibilityController briefingVisibilityController;

    #region Initialization

    public void Initialize(ref_PlayerHUD hud, ref_BriefingPanel briefing, SimulationOrchestratorManager orchestratorManager)
    {
        playerHud = hud;
        briefingPanel = briefing;
        orchestrator = orchestratorManager;
        ApplyBrandColors();
        BindButtons();
        RefreshButtonLabels();
        BuildObjectiveRows();
        SubscribeOrchestratorEvents();
        ApplyState(orchestrator != null ? orchestrator.CurrentState : LotoSimulationState.Initializing);
        RefreshBriefingTexts();
        RefreshScenarioIndicator(orchestrator != null ? orchestrator.CurrentScenarioIndex : 1, LotoSimulationConstants.TotalScenarios);
        RefreshActiveObjectiveTitle();
        HideTimerElements();
    }

    private void OnDestroy()
    {
        UnsubscribeOrchestratorEvents();
    }

    #endregion

    #region Lifecycle

    private void Update()
    {
        if (orchestrator == null || playerHud == null)
        {
            return;
        }

        if (playerHud.subtitleText != null && subtitleHideTime > 0f && Time.unscaledTime >= subtitleHideTime)
        {
            playerHud.subtitleText.text = string.Empty;
            playerHud.subtitleText.color = Color.white;
            subtitleHideTime = 0f;
        }

        if (Time.unscaledTime < nextTimerRefreshTime)
        {
            return;
        }

        nextTimerRefreshTime = Time.unscaledTime + LotoSimulationConstants.HudTimerRefreshInterval;
    }

    private void RefreshButtonLabels()
    {
        EnsureButtonLabel(briefingPanel?.beginButton, "loto_briefing_begin");
        EnsureButtonLabel(briefingPanel?.closeButton, "loto_briefing_close");
        EnsureButtonLabel(briefingPanel?.reopenButton, "loto_briefing_reopen");
        EnsureButtonLabel(playerHud?.menuButton, "loto_hud_hide_panel");
        EnsureButtonLabel(playerHud?.exitToMenuButton, "loto_hud_menu");
        EnsureButtonLabel(playerHud?.hudReopenButton, "loto_hud_show_panel");
        EnsureButtonLabel(playerHud?.hudReopenExitButton, "loto_hud_menu");
        EnsureButtonLabel(playerHud?.exitConfirmYes, "loto_exit_confirm_yes");
        EnsureButtonLabel(playerHud?.exitConfirmNo, "loto_exit_confirm_no");
    }

    private void EnsureButtonLabel(Button button, string translationKey)
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
            RectTransform labelTransform = labelObject.GetComponent<RectTransform>();
            labelTransform.anchorMin = Vector2.zero;
            labelTransform.anchorMax = Vector2.one;
            labelTransform.offsetMin = Vector2.zero;
            labelTransform.offsetMax = Vector2.zero;
            label = labelObject.AddComponent<TextMeshProUGUI>();
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = 22;
            label.color = Color.white;
        }

        label.text = GetTranslation(translationKey);
    }

    #endregion

    #region Orchestrator Events

    private void SubscribeOrchestratorEvents()
    {
        if (orchestrator == null)
        {
            return;
        }

        orchestrator.OnStateChanged += HandleStateChanged;
        orchestrator.OnScenarioChanged += HandleScenarioChanged;
        orchestrator.OnObjectiveVisualUpdate += HandleObjectiveVisualUpdate;
        orchestrator.OnSubtitleChanged += HandleSubtitleChanged;
        orchestrator.OnSessionSummaryRequested += HandleSessionSummaryRequested;
    }

    private void UnsubscribeOrchestratorEvents()
    {
        if (orchestrator == null)
        {
            return;
        }

        orchestrator.OnStateChanged -= HandleStateChanged;
        orchestrator.OnScenarioChanged -= HandleScenarioChanged;
        orchestrator.OnObjectiveVisualUpdate -= HandleObjectiveVisualUpdate;
        orchestrator.OnSubtitleChanged -= HandleSubtitleChanged;
        orchestrator.OnSessionSummaryRequested -= HandleSessionSummaryRequested;
    }

    private void HandleStateChanged(LotoSimulationState state)
    {
        ApplyState(state);
    }

    private void HandleScenarioChanged(int currentScenario, int totalScenarios)
    {
        RefreshScenarioIndicator(currentScenario, totalScenarios);
        RefreshActiveObjectiveTitle();
    }

    private void HandleSessionSummaryRequested(bool success)
    {
        EnsureSessionSummaryPanel();
        RefreshSessionSummary();
    }

    private void HandleObjectiveVisualUpdate(int objectiveIndex, bool achieved)
    {
        RefreshObjectiveRow(objectiveIndex, achieved);
    }

    private void HandleSubtitleChanged(string subtitle, bool isError)
    {
        if (playerHud == null || playerHud.subtitleText == null)
        {
            return;
        }

        playerHud.subtitleText.text = subtitle;
        playerHud.subtitleText.color = isError
            ? new Color(1f, 0.72f, 0.45f, 1f)
            : Color.white;
        subtitleHideTime = Time.unscaledTime + 3f;
    }

    #endregion

    #region UI State

    private void ApplyState(LotoSimulationState state)
    {
        bool showBriefingCanvas = state == LotoSimulationState.Briefing;
        if (briefingPanel != null && briefingPanel.rootPanel != null)
        {
            briefingPanel.rootPanel.SetActive(showBriefingCanvas);
        }

        if (showBriefingCanvas)
        {
            if (lastAppliedState != LotoSimulationState.Briefing)
            {
                briefingPanelVisible = true;
            }

            ApplyBriefingPanelVisibility();
            RefreshBriefingTexts();
        }
        else if (briefingPanel != null && briefingPanel.reopenButton != null)
        {
            briefingPanel.reopenButton.gameObject.SetActive(false);
        }

        if (playerHud != null && playerHud.rootPanel != null)
        {
            bool showHud = state == LotoSimulationState.Active ||
                           state == LotoSimulationState.ExitConfirm ||
                           state == LotoSimulationState.SessionSummary;
            playerHud.rootPanel.SetActive(showHud);
        }

        bool showExitConfirm = state == LotoSimulationState.ExitConfirm;
        bool showSessionSummary = state == LotoSimulationState.SessionSummary;
        bool showActiveChrome = hudChromeVisible && state == LotoSimulationState.Active;

        if (playerHud != null && playerHud.exitConfirmPanel != null)
        {
            playerHud.exitConfirmPanel.SetActive(showExitConfirm);
        }

        if (playerHud != null && playerHud.sessionSummaryPanel != null)
        {
            playerHud.sessionSummaryPanel.SetActive(showSessionSummary);
        }

        SetActiveChromeVisible(showActiveChrome);
        ApplyHudReopenVisibility(state);

        if (state == LotoSimulationState.Active)
        {
            RefreshActiveObjectiveTitle();
            RefreshAllObjectiveRows();
            HideTimerElements();
        }

        if (state == LotoSimulationState.ExitConfirm)
        {
            RefreshExitConfirmTexts();
        }

        if (state == LotoSimulationState.SessionSummary)
        {
            RefreshSessionSummary();
        }

        lastAppliedState = state;
    }

    private void ApplyBriefingPanelVisibility()
    {
        if (briefingPanel == null)
        {
            return;
        }

        GameObject contentRoot = briefingPanel.contentRoot != null
            ? briefingPanel.contentRoot
            : briefingPanel.rootPanel;
        if (contentRoot != null)
        {
            contentRoot.SetActive(briefingPanelVisible);
        }

        if (briefingPanel.reopenButton != null)
        {
            briefingPanel.reopenButton.gameObject.SetActive(!briefingPanelVisible);
        }
    }

    private void ApplyHudReopenVisibility(LotoSimulationState state)
    {
        bool showReopen = !hudChromeVisible && state == LotoSimulationState.Active;
        if (playerHud != null && playerHud.hudReopenButton != null)
        {
            playerHud.hudReopenButton.gameObject.SetActive(showReopen);
        }

        if (playerHud != null && playerHud.hudReopenExitButton != null)
        {
            playerHud.hudReopenExitButton.gameObject.SetActive(showReopen);
        }
    }

    private void SetActiveChromeVisible(bool visible)
    {
        if (playerHud == null)
        {
            return;
        }

        if (playerHud.activeObjectiveTitle != null)
        {
            playerHud.activeObjectiveTitle.gameObject.SetActive(visible);
        }

        if (playerHud.objectivesContainer != null)
        {
            playerHud.objectivesContainer.gameObject.SetActive(visible);
        }

        if (playerHud.timerText != null)
        {
            playerHud.timerText.gameObject.SetActive(visible);
        }

        if (playerHud.timerFill != null)
        {
            playerHud.timerFill.gameObject.SetActive(false);
        }

        if (playerHud.scenarioIndicatorText != null)
        {
            playerHud.scenarioIndicatorText.gameObject.SetActive(visible);
        }

        if (playerHud.subtitleText != null)
        {
            playerHud.subtitleText.gameObject.SetActive(true);
        }

        if (playerHud.exitToMenuButton != null)
        {
            playerHud.exitToMenuButton.gameObject.SetActive(visible);
        }

        if (playerHud.menuButtonRoot != null)
        {
            playerHud.menuButtonRoot.SetActive(visible);
        }
        else if (playerHud.menuButton != null)
        {
            playerHud.menuButton.gameObject.SetActive(visible);
        }
    }

    private void LateUpdate()
    {
        if (playerHud == null || playerHud.subtitleText == null)
        {
            return;
        }

        if (subtitleHideTime > 0f && Time.unscaledTime >= subtitleHideTime)
        {
            playerHud.subtitleText.text = string.Empty;
            playerHud.subtitleText.color = Color.white;
            subtitleHideTime = 0f;
        }
    }

    private void EnsureSessionSummaryPanel()
    {
        if (playerHud == null || playerHud.rootPanel == null)
        {
            return;
        }

        if (playerHud.sessionSummaryPanel != null)
        {
            return;
        }

        GameObject panel = new GameObject("SessionSummaryPanel", typeof(RectTransform));
        panel.transform.SetParent(playerHud.rootPanel.transform, false);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.15f, 0.2f);
        rect.anchorMax = new Vector2(0.85f, 0.75f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image background = panel.AddComponent<Image>();
        background.color = new Color(0.102f, 0.212f, 0.365f, 0.95f);

        playerHud.sessionSummaryPanel = panel;
        playerHud.sessionSummaryTitleText = CreateSummaryText(panel.transform, "Title", new Vector2(0.5f, 0.78f), 28);
        playerHud.sessionSummaryBodyText = CreateSummaryText(panel.transform, "Body", new Vector2(0.5f, 0.5f), 22);
        playerHud.sessionSummaryBodyText.alignment = TextAlignmentOptions.Top;

        GameObject buttonObject = new GameObject("ContinueButton", typeof(RectTransform));
        buttonObject.transform.SetParent(panel.transform, false);
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.3f, 0.08f);
        buttonRect.anchorMax = new Vector2(0.7f, 0.2f);
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;
        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.color = new Color(0.976f, 0.451f, 0.086f, 1f);
        playerHud.sessionSummaryContinueButton = buttonObject.AddComponent<Button>();
        playerHud.sessionSummaryContinueButton.onClick.AddListener(() => orchestrator?.CompleteSimulation());
        CreateSummaryText(buttonObject.transform, "Label", new Vector2(0.5f, 0.5f), 20).text = GetTranslation("backToMenu");
    }

    private static TMP_Text CreateSummaryText(Transform parent, string name, Vector2 anchor, float fontSize)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.sizeDelta = new Vector2(500f, 120f);
        rect.anchoredPosition = Vector2.zero;
        TMP_Text text = textObject.AddComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = fontSize;
        text.color = Color.white;
        return text;
    }

    private void RefreshSessionSummary()
    {
        if (playerHud == null)
        {
            return;
        }

        SceneScore activeScore = GetActiveSceneScore();
        int falsePositiveCount = 0;
        if (ControlCenter.Instance != null && ControlCenter.Instance.simulationSessionManager != null)
        {
            falsePositiveCount = ControlCenter.Instance.simulationSessionManager.GetFalsePositiveCount();
        }

        SimulationResultPayload payload = LotoSessionScore.Build(
            activeScore,
            falsePositiveCount,
            orchestrator != null ? orchestrator.GetActiveElapsedSeconds() : 0f,
            DateTime.UtcNow,
            LotoSessionScore.CompletionCompleted);

        if (playerHud.sessionSummaryTitleText != null)
        {
            playerHud.sessionSummaryTitleText.text = GetTranslation("loto_s3_summary_title");
        }

        if (playerHud.sessionSummaryBodyText != null)
        {
            playerHud.sessionSummaryBodyText.text = FormatSessionSummaryBody(payload);
        }

        EnsureButtonLabel(playerHud.sessionSummaryContinueButton, "backToMenu");
    }

    private static string FormatSessionSummaryBody(SimulationResultPayload payload)
    {
        StringBuilder builder = new StringBuilder();
        int scoreTotal = payload != null ? payload.scoreTotal : 0;
        bool approved = payload != null && payload.approved;
        builder.AppendLine(string.Format(GetTranslation("loto_summary_score"), scoreTotal));
        builder.AppendLine(approved ? GetTranslation("loto_summary_approved") : GetTranslation("loto_summary_not_approved"));
        if (payload == null || payload.objectives == null)
        {
            return builder.ToString();
        }

        for (int i = 0; i < payload.objectives.Length; i++)
        {
            SimulationObjectiveResult objective = payload.objectives[i];
            if (objective == null)
            {
                continue;
            }

            builder.AppendLine(string.Format(
                GetTranslation("loto_summary_objective_line"),
                GetTranslation(objective.objectiveId),
                Mathf.RoundToInt(objective.scorePartial),
                objective.weightPercent));
        }

        return builder.ToString();
    }

    private void BindButtons()
    {
        if (briefingPanel != null && briefingPanel.beginButton != null)
        {
            briefingPanel.beginButton.onClick.RemoveAllListeners();
            briefingPanel.beginButton.onClick.AddListener(() => orchestrator?.DismissBriefing());
        }

        if (briefingPanel != null && briefingPanel.closeButton != null)
        {
            briefingPanel.closeButton.onClick.RemoveAllListeners();
            briefingPanel.closeButton.onClick.AddListener(OnBriefingClosePressed);
        }

        if (briefingPanel != null && briefingPanel.reopenButton != null)
        {
            briefingPanel.reopenButton.onClick.RemoveAllListeners();
            briefingPanel.reopenButton.onClick.AddListener(OnBriefingReopenPressed);
        }

        EnsureHudPanelController();

        if (playerHud != null && playerHud.menuButton != null)
        {
            playerHud.menuButton.onClick.RemoveAllListeners();
            playerHud.menuButton.onClick.AddListener(OnMenuButtonPressed);
        }

        if (playerHud != null && playerHud.exitToMenuButton != null)
        {
            playerHud.exitToMenuButton.onClick.RemoveAllListeners();
            playerHud.exitToMenuButton.onClick.AddListener(OnExitToMenuPressed);
        }

        if (playerHud != null && playerHud.hudReopenButton != null)
        {
            playerHud.hudReopenButton.onClick.RemoveAllListeners();
            playerHud.hudReopenButton.onClick.AddListener(OnHudReopenPressed);
        }

        if (playerHud != null && playerHud.hudReopenExitButton != null)
        {
            playerHud.hudReopenExitButton.onClick.RemoveAllListeners();
            playerHud.hudReopenExitButton.onClick.AddListener(OnExitToMenuPressed);
        }

        if (playerHud != null && playerHud.exitConfirmYes != null)
        {
            playerHud.exitConfirmYes.onClick.RemoveAllListeners();
            playerHud.exitConfirmYes.onClick.AddListener(() => orchestrator?.ConfirmExit());
        }

        if (playerHud != null && playerHud.exitConfirmNo != null)
        {
            playerHud.exitConfirmNo.onClick.RemoveAllListeners();
            playerHud.exitConfirmNo.onClick.AddListener(() => orchestrator?.CancelExit());
        }

        if (playerHud != null && playerHud.sessionSummaryContinueButton != null)
        {
            playerHud.sessionSummaryContinueButton.onClick.RemoveAllListeners();
            playerHud.sessionSummaryContinueButton.onClick.AddListener(() => orchestrator?.CompleteSimulation());
        }
    }

    private void OnBriefingClosePressed()
    {
        briefingPanelVisible = false;
        ApplyBriefingPanelVisibility();
    }

    private void OnBriefingReopenPressed()
    {
        briefingPanelVisible = true;
        ApplyBriefingPanelVisibility();
    }

    private void OnExitToMenuPressed()
    {
        orchestrator?.RequestExitToMenu();
    }

    private void OnHudReopenPressed()
    {
        hudChromeVisible = true;
        ApplyChromeVisibilityForCurrentState();
    }

    private void OnMenuButtonPressed()
    {
        ToggleHudChrome();
    }

    public void ToggleHudChrome()
    {
        hudChromeVisible = !hudChromeVisible;
        ApplyChromeVisibilityForCurrentState();
    }

    private void ApplyChromeVisibilityForCurrentState()
    {
        LotoSimulationState state = orchestrator != null ? orchestrator.CurrentState : LotoSimulationState.Initializing;
        bool showActiveChrome = hudChromeVisible
            && state != LotoSimulationState.Briefing
            && state != LotoSimulationState.ExitConfirm
            && state != LotoSimulationState.SessionSummary;
        SetActiveChromeVisible(showActiveChrome);
        ApplyHudReopenVisibility(state);
    }

    private void EnsureHudPanelController()
    {
        if (playerHud == null)
        {
            return;
        }

        LotoHudPanelController controller = playerHud.GetComponent<LotoHudPanelController>();
        if (controller == null)
        {
            controller = playerHud.gameObject.AddComponent<LotoHudPanelController>();
        }

        Util_PanelFollower follower = playerHud.GetComponent<Util_PanelFollower>();
        controller.Configure(follower);
        EnsureHudPanelButtonFallbacks();
        EnsureBriefingPanelButtonFallbacks();
        EnsureBriefingVisibilityController();

        if (briefingPanel != null)
        {
            Util_PanelFollower briefingFollower = briefingPanel.GetComponent<Util_PanelFollower>();
            if (briefingFollower != null)
            {
                briefingFollower.SetOffset(new Vector3(0f, -0.05f, 0.78f));
                briefingFollower.SetFixedDistance(0.78f);
            }

            RectTransform briefingRect = briefingPanel.GetComponent<RectTransform>();
            if (briefingRect != null)
            {
                briefingRect.sizeDelta = new Vector2(820f, 520f);
                briefingPanel.transform.localScale = Vector3.one * 0.00075f;
            }
        }
    }

    private void EnsureBriefingVisibilityController()
    {
        if (briefingPanel == null || briefingVisibilityController != null)
        {
            return;
        }

        briefingVisibilityController = briefingPanel.GetComponent<LotoPanelVisibilityController>();
        if (briefingVisibilityController == null)
        {
            briefingVisibilityController = briefingPanel.gameObject.AddComponent<LotoPanelVisibilityController>();
        }

        GameObject contentRoot = briefingPanel.contentRoot != null
            ? briefingPanel.contentRoot
            : briefingPanel.rootPanel;
        briefingVisibilityController.Configure(contentRoot, briefingPanel.closeButton, briefingPanel.reopenButton);
    }

    private void EnsureHudPanelButtonFallbacks()
    {
        if (playerHud == null)
        {
            return;
        }

        if (playerHud.hudReopenButton == null)
        {
            Transform existing = playerHud.transform.Find("HudReopenButton");
            if (existing != null)
            {
                playerHud.hudReopenButton = existing.GetComponent<Button>();
            }
        }

        if (playerHud.hudReopenExitButton == null)
        {
            Transform existing = playerHud.transform.Find("HudReopenExitButton");
            if (existing != null)
            {
                playerHud.hudReopenExitButton = existing.GetComponent<Button>();
            }
        }

        if (playerHud.exitToMenuButton == null)
        {
            Transform existing = playerHud.transform.Find("ExitToMenuButton");
            if (existing != null)
            {
                playerHud.exitToMenuButton = existing.GetComponent<Button>();
            }
        }
    }

    private void EnsureBriefingPanelButtonFallbacks()
    {
        if (briefingPanel == null)
        {
            return;
        }

        if (briefingPanel.contentRoot == null)
        {
            Transform content = briefingPanel.transform.Find("BriefingContent");
            if (content != null)
            {
                briefingPanel.contentRoot = content.gameObject;
            }
        }

        if (briefingPanel.closeButton == null)
        {
            Transform close = briefingPanel.transform.Find("BriefingContent/CloseButton");
            if (close == null)
            {
                close = briefingPanel.transform.Find("CloseButton");
            }

            if (close != null)
            {
                briefingPanel.closeButton = close.GetComponent<Button>();
            }
        }

        if (briefingPanel.reopenButton == null)
        {
            Transform reopen = briefingPanel.transform.Find("BriefingReopenButton");
            if (reopen != null)
            {
                briefingPanel.reopenButton = reopen.GetComponent<Button>();
            }
        }
    }

    #endregion

    #region Objective Rows

    private void BuildObjectiveRows()
    {
        objectiveRows.Clear();
        if (playerHud == null || playerHud.objectivesContainer == null || playerHud.objectiveRowPrefab == null)
        {
            return;
        }

        for (int i = playerHud.objectivesContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(playerHud.objectivesContainer.GetChild(i).gameObject);
        }

        for (int i = 0; i < LotoObjectiveIds.AllObjectiveIds.Length; i++)
        {
            GameObject rowObject = Instantiate(playerHud.objectiveRowPrefab, playerHud.objectivesContainer);
            ref_LotoObjectiveRow row = rowObject.GetComponent<ref_LotoObjectiveRow>();
            if (row == null)
            {
                continue;
            }

            if (row.title != null)
            {
                row.title.text = GetTranslation(LotoObjectiveIds.AllObjectiveIds[i]);
            }

            objectiveRows.Add(row);
            RefreshObjectiveRow(i, false);
        }
    }

    private void RefreshAllObjectiveRows()
    {
        SceneScore activeScore = GetActiveSceneScore();
        for (int i = 0; i < objectiveRows.Count; i++)
        {
            bool achieved = activeScore != null &&
                            activeScore.objectives != null &&
                            i < activeScore.objectives.Count &&
                            activeScore.objectives[i].isAchivied;
            RefreshObjectiveRow(i, achieved);
        }
    }

    private void RefreshObjectiveRow(int objectiveIndex, bool achieved)
    {
        if (objectiveIndex < 0 || objectiveIndex >= objectiveRows.Count)
        {
            return;
        }

        ref_LotoObjectiveRow row = objectiveRows[objectiveIndex];
        if (row == null || row.statusIcon == null)
        {
            return;
        }

        row.statusIcon.sprite = achieved ? GetAchievedSprite() : GetPendingSprite();
        row.statusIcon.color = achieved ? new Color(0.133f, 0.773f, 0.369f, 1f) : new Color(0.75f, 0.78f, 0.82f, 1f);
        row.gameObject.SetActive(IsObjectiveVisibleForCurrentScenario(objectiveIndex));
    }

    private bool IsObjectiveVisibleForCurrentScenario(int objectiveIndex)
    {
        if (orchestrator == null)
        {
            return true;
        }

        int scenario = orchestrator.CurrentScenarioIndex;
        if (scenario == LotoSimulationConstants.ScenarioEpp)
        {
            return objectiveIndex <= 1;
        }

        if (scenario == LotoSimulationConstants.ScenarioInspection)
        {
            return objectiveIndex == 2;
        }

        return objectiveIndex >= 3;
    }

    #endregion

    #region Display Refresh

    private void RefreshBriefingTexts()
    {
        if (briefingPanel == null || orchestrator == null)
        {
            return;
        }

        int scenarioIndex = orchestrator.CurrentScenarioIndex;
        if (briefingPanel.titleText != null)
        {
            briefingPanel.titleText.text = GetScenarioBriefingTitle(scenarioIndex);
        }

        if (briefingPanel.bodyText != null)
        {
            briefingPanel.bodyText.text = GetScenarioBriefingBody(scenarioIndex);
        }
    }

    private static string GetScenarioBriefingTitle(int scenarioIndex)
    {
        if (scenarioIndex == LotoSimulationConstants.ScenarioInspection)
        {
            return GetTranslation("loto_s2_briefing_title");
        }

        if (scenarioIndex >= LotoSimulationConstants.ScenarioLoto)
        {
            return GetTranslation("loto_s3_briefing_title");
        }

        return GetTranslation("loto_briefing_title");
    }

    private static string GetScenarioBriefingBody(int scenarioIndex)
    {
        if (scenarioIndex == LotoSimulationConstants.ScenarioInspection)
        {
            return GetTranslation("loto_s2_briefing_body");
        }

        if (scenarioIndex >= LotoSimulationConstants.ScenarioLoto)
        {
            return GetTranslation("loto_s3_briefing_body");
        }

        return GetTranslation("loto_s1_briefing_body");
    }

    private void HideTimerElements()
    {
        if (playerHud == null)
        {
            return;
        }

        if (playerHud.timerText != null)
        {
            playerHud.timerText.gameObject.SetActive(false);
        }

        if (playerHud.timerFill != null)
        {
            playerHud.timerFill.gameObject.SetActive(false);
        }
    }

    private void RefreshExitConfirmTexts()
    {
        if (playerHud == null || playerHud.exitConfirmPanel == null)
        {
            return;
        }

        if (playerHud.exitConfirmTitleText != null)
        {
            playerHud.exitConfirmTitleText.text = GetTranslation("loto_exit_confirm_title");
        }

        if (playerHud.exitConfirmBodyText != null)
        {
            playerHud.exitConfirmBodyText.text = GetTranslation("loto_exit_confirm_body");
        }
    }

    private void RefreshActiveObjectiveTitle()
    {
        if (playerHud == null || playerHud.activeObjectiveTitle == null || orchestrator == null)
        {
            return;
        }

        string label = GetTranslation("loto_hud_active_objective");
        string objectiveId = ResolveActiveObjectiveId();
        string objective = GetTranslation(objectiveId);
        playerHud.activeObjectiveTitle.text = $"{label}: {objective}";
    }

    private string ResolveActiveObjectiveId()
    {
        if (orchestrator == null)
        {
            return LotoObjectiveIds.EppSelection;
        }

        if (orchestrator.CurrentScenarioIndex == LotoSimulationConstants.ScenarioEpp)
        {
            SceneScore activeScore = GetActiveSceneScore();
            if (activeScore != null &&
                activeScore.objectives != null &&
                activeScore.objectives.Count > LotoSimulationConstants.ObjectiveEppSelectionIndex &&
                activeScore.objectives[LotoSimulationConstants.ObjectiveEppSelectionIndex].isAchivied)
            {
                return LotoObjectiveIds.EppDefect;
            }

            return LotoObjectiveIds.EppSelection;
        }

        if (orchestrator.CurrentScenarioIndex == LotoSimulationConstants.ScenarioInspection)
        {
            return LotoObjectiveIds.HazardReport;
        }

        if (orchestrator.CurrentScenarioIndex >= LotoSimulationConstants.ScenarioLoto)
        {
            SceneScore activeScore = GetActiveSceneScore();
            if (activeScore != null &&
                activeScore.objectives != null &&
                activeScore.objectives.Count > LotoSimulationConstants.ObjectiveLotoSequenceIndex &&
                activeScore.objectives[LotoSimulationConstants.ObjectiveLotoSequenceIndex].isAchivied)
            {
                return LotoObjectiveIds.ZeroEnergy;
            }

            return LotoObjectiveIds.LotoSequence;
        }

        return LotoObjectiveIds.EppSelection;
    }

    private void RefreshScenarioIndicator(int currentScenario, int totalScenarios)
    {
        if (playerHud == null || playerHud.scenarioIndicatorText == null)
        {
            return;
        }

        string template = GetTranslation("loto_hud_scenario_label");
        playerHud.scenarioIndicatorText.text = string.Format(template, currentScenario, totalScenarios);
    }

    private void RefreshTimerDisplay()
    {
        if (playerHud == null || orchestrator == null)
        {
            return;
        }

        float elapsed = orchestrator.GetActiveElapsedSeconds();
        if (playerHud.timerText != null)
        {
            playerHud.timerText.text = FormatElapsedTime(elapsed);
        }

        if (playerHud.timerFill != null)
        {
            playerHud.timerFill.gameObject.SetActive(false);
        }
    }

    private void ApplyBrandColors()
    {
        if (playerHud == null)
        {
            return;
        }

        Color primary = Brand.Primary;
        Color secondary = Brand.Secondary;
        if (primary.a <= 0f)
        {
            primary = primaryPanelColor;
        }

        if (secondary.a <= 0f)
        {
            secondary = secondaryAccentColor;
        }

        Image[] images = playerHud.GetComponentsInChildren<Image>(true);
        for (int i = 0; i < images.Length; i++)
        {
            if (images[i] == null || images[i] == playerHud.timerFill)
            {
                continue;
            }

            if (images[i].gameObject.name.Contains("Header") || images[i].gameObject.name.Contains("Panel"))
            {
                images[i].color = primary;
            }
        }

        if (playerHud.menuButton != null)
        {
            Image menuImage = playerHud.menuButton.GetComponent<Image>();
            if (menuImage != null)
            {
                menuImage.color = secondary;
            }
        }

        if (playerHud.exitToMenuButton != null)
        {
            Image exitImage = playerHud.exitToMenuButton.GetComponent<Image>();
            if (exitImage != null)
            {
                exitImage.color = primary;
            }
        }

        if (playerHud.hudReopenButton != null)
        {
            Image reopenImage = playerHud.hudReopenButton.GetComponent<Image>();
            if (reopenImage != null)
            {
                reopenImage.color = secondary;
            }
        }

        if (playerHud.hudReopenExitButton != null)
        {
            Image reopenExitImage = playerHud.hudReopenExitButton.GetComponent<Image>();
            if (reopenExitImage != null)
            {
                reopenExitImage.color = primary;
            }
        }
    }

    #endregion

    #region Helpers

    private SceneScore GetActiveSceneScore()
    {
        if (ControlCenter.Instance == null || ControlCenter.Instance.scoreManager == null)
        {
            return null;
        }

        return ControlCenter.Instance.scoreManager.GetActiveSceneScore(LotoSimulationConstants.LotoSimScoreIndex);
    }

    private Sprite GetPendingSprite()
    {
        if (objectivePendingSprite != null)
        {
            return objectivePendingSprite;
        }

        if (SBD_Images.Instance != null)
        {
            return SBD_Images.Instance.UI_objective_achivied;
        }

        return null;
    }

    private Sprite GetAchievedSprite()
    {
        if (objectiveAchievedSprite != null)
        {
            return objectiveAchievedSprite;
        }

        if (SBD_Images.Instance != null)
        {
            return SBD_Images.Instance.UI_objective_achivied;
        }

        return null;
    }

    private static string GetTranslation(string key)
    {
        if (ControlCenter.Instance == null || ControlCenter.Instance.languageManager == null || string.IsNullOrEmpty(key))
        {
            return key;
        }

        string translated = ControlCenter.Instance.languageManager.GetText(key);
        return string.IsNullOrEmpty(translated) ? key : translated;
    }

    private static string FormatElapsedTime(float elapsedSeconds)
    {
        int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(elapsedSeconds));
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        return $"{minutes:00}:{seconds:00}";
    }

    #endregion
}
