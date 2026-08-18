using TMPro;
using UnityEngine;

public class LotoWorldLabel : MonoBehaviour
{
    [SerializeField] private TMP_Text label;
    [SerializeField] private string locKey;

    #region Lifecycle

    private void OnEnable()
    {
        Refresh();
    }

    #endregion

    #region Public API

    public void Configure(TMP_Text text, string key)
    {
        label = text;
        locKey = key;
        Refresh();
    }

    public void Refresh()
    {
        if (label == null || string.IsNullOrEmpty(locKey))
        {
            return;
        }

        if (ControlCenter.Instance == null || ControlCenter.Instance.languageManager == null)
        {
            label.text = locKey;
            return;
        }

        string translated = ControlCenter.Instance.languageManager.GetText(locKey);
        label.text = string.IsNullOrEmpty(translated) ? locKey : translated;
    }

    #endregion
}
