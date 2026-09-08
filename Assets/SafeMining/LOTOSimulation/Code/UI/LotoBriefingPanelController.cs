using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class LotoBriefingPanelController : MonoBehaviour
{
    [SerializeField] private ref_BriefingPanel briefingPanel;

    private Canvas canvas;
    private GraphicRaycaster graphicRaycaster;
    private TrackedDeviceGraphicRaycaster trackedRaycaster;

    #region Lifecycle

    private void Awake()
    {
        CacheComponents();
        ConfigureInteraction();
        ApplyContent();
    }

    private void OnEnable()
    {
        ConfigureInteraction();
        ApplyContent();
    }

    private void Start()
    {
        ConfigureInteraction();
        ApplyContent();
        StartCoroutine(BindAfterHudInitialize());
    }

    #endregion

    #region Initialization

    private void CacheComponents()
    {
        if (briefingPanel == null)
        {
            briefingPanel = GetComponent<ref_BriefingPanel>();
        }

        canvas = GetComponent<Canvas>();
        graphicRaycaster = GetComponent<GraphicRaycaster>();
        trackedRaycaster = GetComponent<TrackedDeviceGraphicRaycaster>();
    }

    private IEnumerator BindAfterHudInitialize()
    {
        yield return null;
        ConfigureInteraction();
        ApplyContent();
        BindButtons();
    }

    #endregion

    #region Interaction

    private void ConfigureInteraction()
    {
        int uiLayer = LayerMask.NameToLayer("UI");
        if (uiLayer >= 0)
        {
            SetLayerRecursively(transform, uiLayer);
        }

        if (canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.overrideSorting = true;
            canvas.worldCamera = ResolveEventCamera();
        }

        if (graphicRaycaster != null)
        {
            graphicRaycaster.ignoreReversedGraphics = false;
            graphicRaycaster.enabled = true;
        }

        if (trackedRaycaster != null)
        {
            trackedRaycaster.ignoreReversedGraphics = false;
            trackedRaycaster.enabled = true;
        }

        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.ignoreParentGroups = true;

        ConfigureButton(briefingPanel != null ? briefingPanel.beginButton : null);
        ConfigureButton(briefingPanel != null ? briefingPanel.closeButton : null);
        ConfigureButton(briefingPanel != null ? briefingPanel.reopenButton : null);
    }

    private Camera ResolveEventCamera()
    {
        Camera[] cameras = Camera.allCameras;
        Camera namedMain = null;
        Camera withUiLayer = null;
        int uiLayerMask = 1 << 5;

        for (int i = 0; i < cameras.Length; i++)
        {
            Camera candidate = cameras[i];
            if (candidate == null || !candidate.isActiveAndEnabled || IsExcludedCamera(candidate))
            {
                continue;
            }

            bool includesUi = (candidate.cullingMask & uiLayerMask) != 0;
            if (candidate.name == "Main Camera" && includesUi)
            {
                return candidate;
            }

            if (namedMain == null && candidate.name == "Main Camera")
            {
                namedMain = candidate;
            }

            if (withUiLayer == null && includesUi)
            {
                withUiLayer = candidate;
            }
        }

        if (namedMain != null)
        {
            return namedMain;
        }

        if (withUiLayer != null)
        {
            return withUiLayer;
        }

        return Camera.main;
    }

    private static bool IsExcludedCamera(Camera candidate)
    {
        if (candidate == null)
        {
            return true;
        }

        string cameraName = candidate.name;
        return cameraName.IndexOf("PPEx", StringComparison.OrdinalIgnoreCase) >= 0
            || cameraName.IndexOf("Exclude", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static void SetLayerRecursively(Transform root, int layer)
    {
        root.gameObject.layer = layer;
        for (int i = 0; i < root.childCount; i++)
        {
            SetLayerRecursively(root.GetChild(i), layer);
        }
    }

    private static void ConfigureButton(Button button)
    {
        if (button == null)
        {
            return;
        }

        button.interactable = true;
        button.enabled = true;
        if (button.targetGraphic != null)
        {
            button.targetGraphic.raycastTarget = true;
        }
    }

    #endregion

    #region Content

    private void ApplyContent()
    {
        if (briefingPanel == null)
        {
            return;
        }

        int scenarioIndex = ResolveScenarioIndex();
        ApplyText(briefingPanel.titleText, ResolveTitleKey(scenarioIndex), "Title");
        ApplyText(briefingPanel.bodyText, ResolveBodyKey(scenarioIndex), "Body");
        ApplyButtonLabel(briefingPanel.beginButton, "loto_briefing_begin");
        ApplyButtonLabel(briefingPanel.closeButton, "loto_briefing_close");
        ApplyButtonLabel(briefingPanel.reopenButton, "loto_briefing_reopen");
    }

    private static int ResolveScenarioIndex()
    {
        if (ControlCenter.Instance != null && ControlCenter.Instance.simulationOrchestratorManager != null)
        {
            return ControlCenter.Instance.simulationOrchestratorManager.CurrentScenarioIndex;
        }

        return LotoSimulationConstants.ScenarioEpp;
    }

    private static string ResolveTitleKey(int scenarioIndex)
    {
        if (scenarioIndex == LotoSimulationConstants.ScenarioInspection)
        {
            return "loto_s2_briefing_title";
        }

        if (scenarioIndex >= LotoSimulationConstants.ScenarioLoto)
        {
            return "loto_s3_briefing_title";
        }

        return "loto_briefing_title";
    }

    private static string ResolveBodyKey(int scenarioIndex)
    {
        if (scenarioIndex == LotoSimulationConstants.ScenarioInspection)
        {
            return "loto_s2_briefing_body";
        }

        if (scenarioIndex >= LotoSimulationConstants.ScenarioLoto)
        {
            return "loto_s3_briefing_body";
        }

        return "loto_s1_briefing_body";
    }

    private static void ApplyText(TMP_Text field, string localizationKey, string placeholder)
    {
        if (field == null)
        {
            return;
        }

        string translated = Util_LotoSimulationBridge.Translate(localizationKey);
        if (!string.IsNullOrEmpty(translated) && translated != localizationKey)
        {
            field.text = translated;
            return;
        }

        if (string.IsNullOrEmpty(field.text) || field.text == placeholder)
        {
            field.text = translated;
        }
    }

    private static void ApplyButtonLabel(Button button, string localizationKey)
    {
        if (button == null)
        {
            return;
        }

        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
        ApplyText(label, localizationKey, string.Empty);
    }

    #endregion

    #region Event Handlers

    private void BindButtons()
    {
        if (briefingPanel == null)
        {
            return;
        }

        if (briefingPanel.beginButton != null)
        {
            briefingPanel.beginButton.onClick.RemoveListener(HandleBeginPressed);
            briefingPanel.beginButton.onClick.AddListener(HandleBeginPressed);
        }

        if (briefingPanel.closeButton != null)
        {
            briefingPanel.closeButton.onClick.RemoveListener(HandleClosePressed);
            briefingPanel.closeButton.onClick.AddListener(HandleClosePressed);
        }

        if (briefingPanel.reopenButton != null)
        {
            briefingPanel.reopenButton.onClick.RemoveListener(HandleReopenPressed);
            briefingPanel.reopenButton.onClick.AddListener(HandleReopenPressed);
        }
    }

    private void HandleBeginPressed()
    {
        if (ControlCenter.Instance != null && ControlCenter.Instance.simulationOrchestratorManager != null)
        {
            ControlCenter.Instance.simulationOrchestratorManager.DismissBriefing();
            return;
        }

        HideContent();
    }

    private void HandleClosePressed()
    {
        HideContent();
    }

    private void HandleReopenPressed()
    {
        ShowContent();
    }

    private void HideContent()
    {
        if (briefingPanel == null)
        {
            return;
        }

        if (briefingPanel.contentRoot != null)
        {
            briefingPanel.contentRoot.SetActive(false);
        }

        if (briefingPanel.reopenButton != null)
        {
            briefingPanel.reopenButton.gameObject.SetActive(true);
        }
    }

    private void ShowContent()
    {
        if (briefingPanel == null)
        {
            return;
        }

        if (briefingPanel.contentRoot != null)
        {
            briefingPanel.contentRoot.SetActive(true);
        }

        if (briefingPanel.reopenButton != null)
        {
            briefingPanel.reopenButton.gameObject.SetActive(false);
        }

        ConfigureInteraction();
        ApplyContent();
    }

    #endregion
}
