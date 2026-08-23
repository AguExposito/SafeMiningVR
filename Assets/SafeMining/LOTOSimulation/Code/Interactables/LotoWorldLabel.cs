using TMPro;
using UnityEngine;

public class LotoWorldLabel : MonoBehaviour
{
    [SerializeField] private TMP_Text label;
    [SerializeField] private string locKey;

    private Camera cachedCamera;

    #region Lifecycle

    private void OnEnable()
    {
        Refresh();
    }

    private void LateUpdate()
    {
        Camera cam = ResolveCamera();
        if (cam == null)
        {
            return;
        }

        Vector3 toCamera = transform.position - cam.transform.position;
        toCamera.y = 0f;
        if (toCamera.sqrMagnitude < 0.0001f)
        {
            return;
        }

        transform.rotation = Quaternion.LookRotation(toCamera.normalized, Vector3.up);
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

    private Camera ResolveCamera()
    {
        if (cachedCamera != null)
        {
            return cachedCamera;
        }

        cachedCamera = Camera.main;
        return cachedCamera;
    }
}
