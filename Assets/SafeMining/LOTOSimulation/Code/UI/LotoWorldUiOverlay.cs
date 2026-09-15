using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using TMPro;

public class LotoWorldUiOverlay : MonoBehaviour
{
    #region Overlay

    [SerializeField] private int sortingOrder = 500;


    private void Awake()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.overrideSorting = true;
            canvas.sortingOrder = sortingOrder;
        }

        ApplyZTestAlways();
    }

    private void ApplyZTestAlways()
    {
        Graphic[] graphics = GetComponentsInChildren<Graphic>(true);
        for (int i = 0; i < graphics.Length; i++)
        {
            Graphic graphic = graphics[i];
            if (graphic == null)
            {
                continue;
            }

            Material source = graphic.material;
            if (source == null)
            {
                continue;
            }

            Material overlay = new Material(source);
            overlay.SetInt("_ZTest", (int)CompareFunction.Always);
            graphic.material = overlay;
        }

        TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            TMP_Text text = texts[i];
            if (text == null)
            {
                continue;
            }

            Material overlay = new Material(text.fontMaterial);
            overlay.SetFloat("_ZTestMode", 8f);
            overlay.SetInt("_ZTest", (int)CompareFunction.Always);
            text.fontMaterial = overlay;
        }
    }

    #endregion
}

