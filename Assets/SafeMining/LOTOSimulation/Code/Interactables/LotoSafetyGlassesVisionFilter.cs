using UnityEngine;
using UnityEngine.UI;

public class LotoSafetyGlassesVisionFilter : MonoBehaviour
{
    private static LotoSafetyGlassesVisionFilter instance;

    [SerializeField] private Color tintColor = new Color(0.35f, 0.55f, 0.75f, 0.22f);
    [SerializeField] private float canvasDistance = 0.18f;

    private Canvas filterCanvas;
    private Image tintImage;
    private bool isActive;

    #region Public API

    public static void EnsureOnCamera(Camera camera)
    {
        if (camera == null)
        {
            return;
        }

        if (instance != null)
        {
            return;
        }

        instance = camera.GetComponentInChildren<LotoSafetyGlassesVisionFilter>(true);
        if (instance != null)
        {
            return;
        }

        GameObject host = new GameObject("LotoSafetyGlassesVisionFilter");
        host.transform.SetParent(camera.transform, false);
        instance = host.AddComponent<LotoSafetyGlassesVisionFilter>();
        instance.BuildVisual();
        instance.SetActive(false);
    }

    public static void SetGlassesEquipped(bool equipped)
    {
        Camera camera = Camera.main;
        EnsureOnCamera(camera);
        if (instance != null)
        {
            instance.SetActive(equipped);
        }
    }

    public void SetActive(bool active)
    {
        isActive = active;
        if (filterCanvas != null)
        {
            filterCanvas.gameObject.SetActive(active);
        }
    }

    #endregion

    #region Initialization

    private void Awake()
    {
        instance = this;
        if (filterCanvas == null)
        {
            BuildVisual();
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    private void BuildVisual()
    {
        if (filterCanvas != null)
        {
            return;
        }

        GameObject canvasObject = new GameObject("GlassesTintCanvas");
        canvasObject.transform.SetParent(transform, false);
        canvasObject.transform.localPosition = new Vector3(0f, 0f, canvasDistance);
        canvasObject.transform.localRotation = Quaternion.identity;
        canvasObject.transform.localScale = Vector3.one * 0.001f;

        filterCanvas = canvasObject.AddComponent<Canvas>();
        filterCanvas.renderMode = RenderMode.WorldSpace;
        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(1200f, 800f);

        GameObject imageObject = new GameObject("Tint");
        imageObject.transform.SetParent(canvasObject.transform, false);
        RectTransform imageRect = imageObject.AddComponent<RectTransform>();
        imageRect.anchorMin = Vector2.zero;
        imageRect.anchorMax = Vector2.one;
        imageRect.offsetMin = Vector2.zero;
        imageRect.offsetMax = Vector2.zero;
        tintImage = imageObject.AddComponent<Image>();
        tintImage.raycastTarget = false;
        tintImage.color = tintColor;
        tintImage.sprite = CreateWhiteSprite();
    }

    private static Sprite CreateWhiteSprite()
    {
        Texture2D texture = Texture2D.whiteTexture;
        return Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            100f);
    }

    #endregion
}
