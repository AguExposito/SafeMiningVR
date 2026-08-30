using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class LotoDwellReticle : MonoBehaviour
{
    [SerializeField] private float dwellSeconds = 1f;
    [SerializeField] private float reticleDiameterMeters = 0.03f;
    [SerializeField] private float cooldownSeconds = 0.3f;
    [SerializeField] private Color fallbackFillColor = new Color(0.976f, 0.451f, 0.086f, 1f);
    [SerializeField] private Color ringBackgroundColor = new Color(0.12f, 0.12f, 0.12f, 0.55f);

    private XRSimpleInteractable boundInteractable;
    private Action onComplete;
    private bool dwellEnabled = true;
    private bool isHovering;
    private float hoverFill;
    private float cooldownRemaining;
    private IXRHoverInteractor activeHoverInteractor;
    private Transform reticleRoot;
    private Image fillImage;
    private Image ringImage;
    private Coroutine dwellRoutine;

    #region Lifecycle

    private void Update()
    {
        if (cooldownRemaining > 0f)
        {
            cooldownRemaining -= Time.deltaTime;
        }
    }

    private void OnDestroy()
    {
        Unbind();
    }

    #endregion

    #region Public API

    public void ConfigureDwellSeconds(float seconds)
    {
        dwellSeconds = Mathf.Max(seconds, 0.01f);
    }

    public void Bind(XRSimpleInteractable target, Action completeAction)
    {
        Unbind();
        boundInteractable = target;
        onComplete = completeAction;

        if (boundInteractable == null)
        {
            return;
        }

        boundInteractable.hoverEntered.AddListener(OnHoverEntered);
        boundInteractable.hoverExited.AddListener(OnHoverExited);
        boundInteractable.selectEntered.AddListener(OnSelectEntered);
        EnsureVisual();
        SetReticleVisible(false);
    }

    public void SetDwellEnabled(bool enabled)
    {
        dwellEnabled = enabled;
        if (!dwellEnabled)
        {
            ResetDwell();
        }
    }

    public void Unbind()
    {
        StopDwellRoutine();

        if (boundInteractable != null)
        {
            boundInteractable.hoverEntered.RemoveListener(OnHoverEntered);
            boundInteractable.hoverExited.RemoveListener(OnHoverExited);
            boundInteractable.selectEntered.RemoveListener(OnSelectEntered);
        }

        boundInteractable = null;
        onComplete = null;
        ResetDwell();
    }

    #endregion

    #region Visual

    private void EnsureVisual()
    {
        if (reticleRoot != null)
        {
            return;
        }

        GameObject rootObject = new GameObject("DwellReticleRoot");
        rootObject.transform.SetParent(transform, false);

        Canvas canvas = rootObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 500;

        CanvasScaler scaler = rootObject.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 100f;

        float canvasScale = reticleDiameterMeters / 30f;
        rootObject.transform.localScale = new Vector3(canvasScale, canvasScale, canvasScale);

        RectTransform canvasRect = rootObject.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(30f, 30f);

        GameObject ringObject = new GameObject("Ring");
        ringObject.transform.SetParent(rootObject.transform, false);
        RectTransform ringRect = ringObject.AddComponent<RectTransform>();
        ringRect.anchorMin = Vector2.zero;
        ringRect.anchorMax = Vector2.one;
        ringRect.offsetMin = Vector2.zero;
        ringRect.offsetMax = Vector2.zero;
        ringImage = ringObject.AddComponent<Image>();
        ringImage.sprite = ResolveUiSprite();
        ringImage.type = Image.Type.Simple;
        ringImage.color = ringBackgroundColor;

        GameObject fillObject = new GameObject("Fill");
        fillObject.transform.SetParent(rootObject.transform, false);
        RectTransform fillRect = fillObject.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = new Vector2(3f, 3f);
        fillRect.offsetMax = new Vector2(-3f, -3f);
        fillImage = fillObject.AddComponent<Image>();
        fillImage.sprite = ResolveUiSprite();
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Radial360;
        fillImage.fillOrigin = (int)Image.Origin360.Top;
        fillImage.fillClockwise = true;
        fillImage.fillAmount = 0f;
        fillImage.color = ResolveFillColor();

        reticleRoot = rootObject.transform;
    }

    private static Sprite cachedUiSprite;

    private static Sprite ResolveUiSprite()
    {
        if (cachedUiSprite != null)
        {
            return cachedUiSprite;
        }

        Sprite sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
        if (sprite != null)
        {
            cachedUiSprite = sprite;
            return cachedUiSprite;
        }

        Texture2D texture = Texture2D.whiteTexture;
        cachedUiSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            100f);
        return cachedUiSprite;
    }

    private Color ResolveFillColor()
    {
        if (Brand.HasConfig)
        {
            return Brand.GetColor(BrandColorRole.Secondary);
        }

        return fallbackFillColor;
    }

    private void SetReticleVisible(bool visible)
    {
        if (reticleRoot != null)
        {
            reticleRoot.gameObject.SetActive(visible);
        }
    }

    private void ApplyFillAmount(float amount)
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = Mathf.Clamp01(amount);
            fillImage.color = ResolveFillColor();
        }
    }

    private void UpdateReticleFacing()
    {
        if (reticleRoot == null)
        {
            return;
        }

        Camera viewCamera = ResolveViewCamera();
        if (viewCamera == null)
        {
            return;
        }

        Vector3 forward = reticleRoot.position - viewCamera.transform.position;
        if (forward.sqrMagnitude > 0.0001f)
        {
            reticleRoot.rotation = Quaternion.LookRotation(forward.normalized, Vector3.up);
        }
    }

    private void UpdateReticleAnchor()
    {
        if (reticleRoot == null || boundInteractable == null)
        {
            return;
        }

        if (activeHoverInteractor != null)
        {
            Transform attachTransform = activeHoverInteractor.GetAttachTransform(boundInteractable);
            if (attachTransform != null)
            {
                reticleRoot.position = attachTransform.position;
                return;
            }
        }

        Collider collider = boundInteractable.GetComponent<Collider>();
        if (collider != null)
        {
            reticleRoot.position = collider.bounds.center;
            return;
        }

        reticleRoot.position = boundInteractable.transform.position;
    }

    private static Camera ResolveViewCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            return mainCamera;
        }

        Camera[] cameras = FindObjectsByType<Camera>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i] != null && cameras[i].enabled)
            {
                return cameras[i];
            }
        }

        return null;
    }

    #endregion

    #region Dwell Logic

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        if (!dwellEnabled || cooldownRemaining > 0f)
        {
            return;
        }

        isHovering = true;
        activeHoverInteractor = args.interactorObject;
        hoverFill = 0f;
        ApplyFillAmount(0f);
        EnsureVisual();
        UpdateReticleAnchor();
        UpdateReticleFacing();
        SetReticleVisible(true);
        StartDwellRoutine();
    }

    private void OnHoverExited(HoverExitEventArgs args)
    {
        if (args.interactorObject == activeHoverInteractor)
        {
            ResetDwell();
        }
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (hoverFill < 1f && args.manager != null)
        {
            args.manager.SelectExit(args.interactorObject, args.interactableObject);
        }
    }

    private void StartDwellRoutine()
    {
        StopDwellRoutine();
        dwellRoutine = StartCoroutine(DwellFillRoutine());
    }

    private void StopDwellRoutine()
    {
        if (dwellRoutine != null)
        {
            StopCoroutine(dwellRoutine);
            dwellRoutine = null;
        }
    }

    private IEnumerator DwellFillRoutine()
    {
        while (isHovering && dwellEnabled && boundInteractable != null)
        {
            UpdateReticleFacing();
            UpdateReticleAnchor();

            if (cooldownRemaining <= 0f)
            {
                hoverFill += Time.deltaTime / Mathf.Max(dwellSeconds, 0.01f);
                ApplyFillAmount(hoverFill);

                if (hoverFill >= 1f)
                {
                    CompleteDwell();
                    yield break;
                }
            }

            yield return null;
        }
    }

    private void CompleteDwell()
    {
        StopDwellRoutine();
        hoverFill = 1f;
        ApplyFillAmount(1f);
        cooldownRemaining = cooldownSeconds;
        isHovering = false;
        activeHoverInteractor = null;
        SetReticleVisible(false);

        Action callback = onComplete;
        callback?.Invoke();
    }

    private void ResetDwell()
    {
        StopDwellRoutine();
        isHovering = false;
        activeHoverInteractor = null;
        hoverFill = 0f;
        ApplyFillAmount(0f);
        SetReticleVisible(false);
    }

    #endregion
}
