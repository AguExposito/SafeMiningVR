using System;
using UnityEngine;
using UnityEngine.UI;

public class LotoPanelVisibilityController : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button reopenChipButton;

    private bool panelVisible = true;

    public bool IsPanelVisible => panelVisible;

    public event Action<bool> OnVisibilityChanged;

    #region Public API

    public void Configure(GameObject root, Button close, Button reopen)
    {
        panelRoot = root;
        closeButton = close;
        reopenChipButton = reopen;
        BindButtons();
        ApplyVisibility(panelVisible, false);
    }

    public void SetPanelVisible(bool visible, bool notify = true)
    {
        if (panelVisible == visible && panelRoot != null && panelRoot.activeSelf == visible)
        {
            UpdateReopenChip();
            return;
        }

        panelVisible = visible;
        ApplyVisibility(visible, notify);
    }

    public void TogglePanel()
    {
        SetPanelVisible(!panelVisible);
    }

    public void ResetVisible()
    {
        panelVisible = true;
        ApplyVisibility(true, false);
    }

    #endregion

    #region Lifecycle

    private void Awake()
    {
        BindButtons();
        ApplyVisibility(panelVisible, false);
    }

    #endregion

    #region Internals

    private void BindButtons()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(HandleClosePressed);
            closeButton.onClick.AddListener(HandleClosePressed);
        }

        if (reopenChipButton != null)
        {
            reopenChipButton.onClick.RemoveListener(HandleReopenPressed);
            reopenChipButton.onClick.AddListener(HandleReopenPressed);
        }
    }

    private void HandleClosePressed()
    {
        SetPanelVisible(false);
    }

    private void HandleReopenPressed()
    {
        SetPanelVisible(true);
    }

    private void ApplyVisibility(bool visible, bool notify)
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(visible);
        }

        UpdateReopenChip();

        if (notify)
        {
            OnVisibilityChanged?.Invoke(visible);
        }
    }

    private void UpdateReopenChip()
    {
        if (reopenChipButton != null)
        {
            reopenChipButton.gameObject.SetActive(!panelVisible);
        }
    }

    #endregion
}
