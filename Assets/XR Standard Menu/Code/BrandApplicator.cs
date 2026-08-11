using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Applies centralized branding (logo / color / name text) to this UI element.
/// Attach to Images, TMP texts, or any Graphic that should follow BrandConfig.
/// </summary>
[DisallowMultipleComponent]
[ExecuteAlways]
public class BrandApplicator : MonoBehaviour
{
    [SerializeField] private BrandGraphicRole graphicRole = BrandGraphicRole.None;
    [SerializeField] private BrandColorRole colorRole = BrandColorRole.None;
    [SerializeField] private BrandTextRole textRole = BrandTextRole.None;
    [Tooltip("If true, multiplies the brand color onto Image/RawImage/TMP instead of replacing sprite only.")]
    [SerializeField] private bool applyColorToGraphic = true;

    private void OnEnable()
    {
        Apply();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            Apply();
        }
    }
#endif

    public void Apply()
    {
        if (graphicRole == BrandGraphicRole.Logo && Brand.Logo != null)
        {
            Image image = GetComponent<Image>();
            if (image != null)
            {
                image.sprite = Brand.Logo;
                image.preserveAspect = true;
            }

            RawImage rawImage = GetComponent<RawImage>();
            if (rawImage != null && Brand.Logo.texture != null)
            {
                rawImage.texture = Brand.Logo.texture;
            }
        }

        if (colorRole != BrandColorRole.None && applyColorToGraphic)
        {
            Color color = Brand.GetColor(colorRole);
            Graphic graphic = GetComponent<Graphic>();
            if (graphic != null)
            {
                graphic.color = color;
            }

            TMP_Text tmp = GetComponent<TMP_Text>();
            if (tmp != null)
            {
                tmp.color = color;
            }
        }

        if (textRole != BrandTextRole.None)
        {
            string value = Brand.GetText(textRole);
            TMP_Text tmp = GetComponent<TMP_Text>();
            if (tmp != null)
            {
                tmp.text = value;
            }

            Text text = GetComponent<Text>();
            if (text != null)
            {
                text.text = value;
            }
        }
    }
}
