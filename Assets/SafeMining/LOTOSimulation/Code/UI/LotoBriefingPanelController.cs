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
    private string authoredTitle;
    private string authoredBody;
    private string authoredBeginLabel;
    private string authoredCloseLabel;
    private string authoredReopenLabel;

    #region Lifecycle

    private void Awake()
    {
        CacheComponents();
        CacheAuthoredTexts();
        DisableAuthoredTextOverrides();
        ConfigureInteraction();
    }

    private void OnEnable()
    {
        ConfigureInteraction();
    }

    private void Start()
    {
        ConfigureInteraction();
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
        RestoreAuthoredTexts();
        ConfigureInteraction();
        BindButtons();
    }

    private void CacheAuthoredTexts()
    {
        authoredTitle = ReadText(briefingPanel != null ? briefingPanel.titleText : null);
        authoredBody = ReadText(briefingPanel != null ? briefingPanel.bodyText : null);
        authoredBeginLabel = ReadButtonLabel(briefingPanel != null ? briefingPanel.beginButton : null);
        authoredCloseLabel = ReadButtonLabel(briefingPanel != null ? briefingPanel.closeButton : null);
        authoredReopenLabel = ReadButtonLabel(briefingPanel != null ? briefingPanel.reopenButton : null);
    }

    private void RestoreAuthoredTexts()
    {
        WriteText(briefingPanel != null ? briefingPanel.titleText : null, authoredTitle);
        WriteText(briefingPanel != null ? briefingPanel.bodyText : null, authoredBody);
        WriteButtonLabel(briefingPanel != null ? briefingPanel.beginButton : null, authoredBeginLabel);
        WriteButtonLabel(briefingPanel != null ? briefingPanel.closeButton : null, authoredCloseLabel);
        WriteButtonLabel(briefingPanel != null ? briefingPanel.reopenButton : null, authoredReopenLabel);
    }

    private static string ReadText(TMP_Text field)
    {
        return field != null ? field.text : null;
    }

    private static void WriteText(TMP_Text field, string value)
    {
        if (field != null && !string.IsNullOrEmpty(value))
        {
            field.text = value;
        }
    }

    private static string ReadButtonLabel(Button button)
    {
        if (button == null)
        {
            return null;
        }

        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
        return ReadText(label);
    }

    private static void WriteButtonLabel(Button button, string value)
    {
        if (button == null)
        {
            return;
        }

        WriteText(button.GetComponentInChildren<TMP_Text>(true), value);
    }

    private void DisableAuthoredTextOverrides()
    {
        TranslatableText[] translatableTexts = GetComponentsInChildren<TranslatableText>(true);
        for (int i = 0; i < translatableTexts.Length; i++)
        {
            if (translatableTexts[i] != null)
            {
                translatableTexts[i].enabled = false;
            }
        }
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
        HideContent();
        if (ControlCenter.Instance != null && ControlCenter.Instance.simulationOrchestratorManager != null)
        {
            ControlCenter.Instance.simulationOrchestratorManager.DismissBriefing();
        }
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
            gameObject.SetActive(false);
            return;
        }

        if (briefingPanel.contentRoot != null)
        {
            briefingPanel.contentRoot.SetActive(false);
        }
        else if (briefingPanel.rootPanel != null)
        {
            briefingPanel.rootPanel.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
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
    }

    #endregion
}
