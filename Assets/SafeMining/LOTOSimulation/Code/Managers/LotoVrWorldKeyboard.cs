using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class LotoVrWorldKeyboard : MonoBehaviour
{
    private static readonly string[] Rows =
    {
        "1234567890",
        "qwertyuiop",
        "asdfghjkl",
        "zxcvbnm"
    };

    private RectTransform root;
    private TMP_InputField targetField;
    private bool shiftOn;
    private bool built;
    private Sprite keySprite;
    private TMP_FontAsset labelFont;
    private Color keyColor = new Color(0.976f, 0.447f, 0.086f, 1f);
    private Color panelColor = new Color(0.08f, 0.1f, 0.14f, 0.96f);
    private Color activeFieldColor = new Color(1f, 0.85f, 0.55f, 1f);
    private Color idleFieldColor = Color.white;
    private Action onDone;
    private Image targetFieldGraphic;

    #region Public API

    public bool IsVisible => root != null && root.gameObject.activeSelf;

    public void EnsureBuilt(RectTransform parent, Sprite sprite, TMP_FontAsset font, Action doneCallback)
    {
        keySprite = sprite;
        labelFont = font;
        onDone = doneCallback;
        if (built && root != null && root.GetComponentInChildren<LotoVrKeyButton>(true) != null)
        {
            bool hasHoverAudio = root.GetComponentInChildren<UIInteractable>(true) != null;
            LayoutInsidePanel();
            if (!hasHoverAudio && root.pivot.y < 0.5f && root.anchoredPosition.y >= 0f)
            {
                return;
            }

            DestroyImmediate(root.gameObject);
            root = null;
            built = false;
        }

        if (root != null)
        {
            DestroyImmediate(root.gameObject);
            root = null;
        }

        Transform leftover = parent.Find("LotoVrWorldKeyboard");
        while (leftover != null)
        {
            DestroyImmediate(leftover.gameObject);
            leftover = parent.Find("LotoVrWorldKeyboard");
        }

        Build(parent);
        built = true;
        Hide();
    }

    public void Show(TMP_InputField field)
    {
        if (!built || root == null || field == null)
        {
            return;
        }

        ClearActiveFieldHighlight();
        targetField = field;
        targetFieldGraphic = field.targetGraphic as Image;
        PrepareField(field);
        HighlightActiveField();
        LayoutInsidePanel();
        root.gameObject.SetActive(true);
        root.SetAsLastSibling();
        RefreshShiftLabels();
        RefreshFieldVisual();
    }

    public void Hide()
    {
        ClearActiveFieldHighlight();
        if (root != null)
        {
            root.gameObject.SetActive(false);
        }

        targetField = null;
        targetFieldGraphic = null;
        shiftOn = false;
        RefreshShiftLabels();
    }

    #endregion

    #region Build

    private void Build(RectTransform parent)
    {
        GameObject rootGo = new GameObject("LotoVrWorldKeyboard", typeof(RectTransform), typeof(Image));
        root = rootGo.GetComponent<RectTransform>();
        root.SetParent(parent, false);
        LayoutInsidePanel();

        Image panel = rootGo.GetComponent<Image>();
        panel.color = panelColor;
        panel.raycastTarget = true;

        float keyW = 72f;
        float keyH = 58f;
        float gap = 6f;
        float startY = -22f;

        for (int r = 0; r < Rows.Length; r++)
        {
            string row = Rows[r];
            float rowWidth = row.Length * keyW + (row.Length - 1) * gap;
            float startX = -rowWidth * 0.5f + keyW * 0.5f;
            float y = startY - r * (keyH + gap);
            for (int c = 0; c < row.Length; c++)
            {
                char captured = row[c];
                float x = startX + c * (keyW + gap);
                CreateKey(root, captured.ToString(), captured.ToString(), new Vector2(x, y), new Vector2(keyW, keyH), () => AppendChar(captured));
            }
        }

        float utilY = startY - Rows.Length * (keyH + gap);
        CreateKey(root, "Shift", "SH", new Vector2(-300f, utilY), new Vector2(100f, keyH), ToggleShift);
        CreateKey(root, "Space", "SPC", new Vector2(-40f, utilY), new Vector2(280f, keyH), () => AppendText(" "));
        CreateKey(root, "Back", "DEL", new Vector2(220f, utilY), new Vector2(100f, keyH), Backspace);
        CreateKey(root, "Dot", ".", new Vector2(330f, utilY), new Vector2(70f, keyH), () => AppendText("."));

        float util2Y = utilY - (keyH + gap);
        CreateKey(root, "Clear", "CLR", new Vector2(-300f, util2Y), new Vector2(100f, keyH), ClearField);
        CreateKey(root, "Underscore", "_", new Vector2(-180f, util2Y), new Vector2(70f, keyH), () => AppendText("_"));
        CreateKey(root, "Dash", "-", new Vector2(-100f, util2Y), new Vector2(70f, keyH), () => AppendText("-"));
        CreateKey(root, "At", "@", new Vector2(-20f, util2Y), new Vector2(70f, keyH), () => AppendText("@"));
        CreateKey(root, "Done", "OK", new Vector2(180f, util2Y), new Vector2(280f, keyH), Done);
    }

    private void LayoutInsidePanel()
    {
        if (root == null)
        {
            return;
        }

        root.anchorMin = new Vector2(0.5f, 0f);
        root.anchorMax = new Vector2(0.5f, 0f);
        root.pivot = new Vector2(0.5f, 0f);
        root.anchoredPosition = new Vector2(0f, 16f);
        root.sizeDelta = new Vector2(800f, 340f);
    }

    private void CreateKey(RectTransform parent, string name, string label, Vector2 anchoredPos, Vector2 size, Action onPress)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LotoVrKeyButton));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        Image image = go.GetComponent<Image>();
        image.sprite = keySprite;
        image.type = keySprite != null ? Image.Type.Sliced : Image.Type.Simple;
        image.color = keyColor;
        image.raycastTarget = true;

        Button button = go.GetComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.92f, 0.85f, 1f);
        colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;
        button.onClick.RemoveAllListeners();

        LotoVrKeyButton keyButton = go.GetComponent<LotoVrKeyButton>();
        keyButton.Configure(onPress);

        GameObject labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform labelRt = labelGo.GetComponent<RectTransform>();
        labelRt.SetParent(rt, false);
        labelRt.anchorMin = Vector2.zero;
        labelRt.anchorMax = Vector2.one;
        labelRt.offsetMin = Vector2.zero;
        labelRt.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = labelGo.GetComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = label.Length > 1 ? 22f : 28f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        if (labelFont != null)
        {
            tmp.font = labelFont;
        }
    }

    #endregion

    #region Input

    private static void PrepareField(TMP_InputField field)
    {
        if (field == null)
        {
            return;
        }

        field.shouldHideMobileInput = true;
        field.shouldHideSoftKeyboard = true;
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == field.gameObject)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        field.DeactivateInputField();
    }

    private void AppendChar(char ch)
    {
        if (char.IsLetter(ch))
        {
            ch = shiftOn ? char.ToUpperInvariant(ch) : char.ToLowerInvariant(ch);
        }

        AppendText(ch.ToString());
        if (shiftOn && char.IsLetter(ch))
        {
            shiftOn = false;
            RefreshShiftLabels();
        }
    }

    private void AppendText(string value)
    {
        if (targetField == null || string.IsNullOrEmpty(value))
        {
            return;
        }

        if (!targetField.gameObject.activeInHierarchy)
        {
            return;
        }

        PrepareField(targetField);
        string current = targetField.text ?? string.Empty;
        int limit = targetField.characterLimit;
        if (limit > 0 && current.Length >= limit)
        {
            return;
        }

        string next = current + value;
        if (limit > 0 && next.Length > limit)
        {
            next = next.Substring(0, limit);
        }

        ApplyFieldText(next);
    }

    private void Backspace()
    {
        if (targetField == null || !targetField.gameObject.activeInHierarchy)
        {
            return;
        }

        PrepareField(targetField);
        string current = targetField.text ?? string.Empty;
        if (current.Length == 0)
        {
            return;
        }

        ApplyFieldText(current.Substring(0, current.Length - 1));
    }

    private void ClearField()
    {
        if (targetField == null || !targetField.gameObject.activeInHierarchy)
        {
            return;
        }

        PrepareField(targetField);
        ApplyFieldText(string.Empty);
    }

    private void ApplyFieldText(string next)
    {
        bool wasInteractable = targetField.interactable;
        targetField.interactable = true;
        targetField.SetTextWithoutNotify(string.Empty);
        targetField.SetTextWithoutNotify(next ?? string.Empty);
        targetField.text = next ?? string.Empty;
        targetField.caretPosition = targetField.text.Length;
        targetField.selectionAnchorPosition = targetField.caretPosition;
        targetField.selectionFocusPosition = targetField.caretPosition;
        targetField.ForceLabelUpdate();
        if (targetField.textComponent != null)
        {
            if (targetField.contentType == TMP_InputField.ContentType.Password)
            {
                targetField.ForceLabelUpdate();
            }
            else
            {
                targetField.textComponent.text = targetField.text;
            }

            targetField.textComponent.SetAllDirty();
            targetField.textComponent.ForceMeshUpdate(true);
        }

        targetField.interactable = wasInteractable;
        Canvas.ForceUpdateCanvases();
        RefreshFieldVisual();
    }

    private void RefreshFieldVisual()
    {
        if (targetField == null || targetField.textComponent == null)
        {
            return;
        }

        targetField.ForceLabelUpdate();
        targetField.textComponent.SetAllDirty();
    }

    private void HighlightActiveField()
    {
        if (targetFieldGraphic != null)
        {
            idleFieldColor = targetFieldGraphic.color;
            targetFieldGraphic.color = activeFieldColor;
        }
    }

    private void ClearActiveFieldHighlight()
    {
        if (targetFieldGraphic != null)
        {
            targetFieldGraphic.color = idleFieldColor;
        }
    }

    private void ToggleShift()
    {
        shiftOn = !shiftOn;
        RefreshShiftLabels();
    }

    private void Done()
    {
        TMP_InputField finished = targetField;
        Hide();
        onDone?.Invoke();
        if (finished != null)
        {
            PrepareField(finished);
        }
    }

    private void RefreshShiftLabels()
    {
        if (root == null)
        {
            return;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            string name = child.name;
            if (name.Length != 1 || !char.IsLetter(name[0]))
            {
                continue;
            }

            TextMeshProUGUI label = child.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label == null)
            {
                continue;
            }

            label.text = shiftOn ? name.ToUpperInvariant() : name.ToLowerInvariant();
        }
    }

    #endregion
}

[DisallowMultipleComponent]
public class LotoVrKeyButton : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, ISubmitHandler
{
    private Action pressed;
    private float lastPressTime;
    private int pressedPointerId = int.MinValue;

    #region Public API

    public void Configure(Action onPressed)
    {
        pressed = onPressed;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Fire(eventData != null ? eventData.pointerId : 0);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData != null)
        {
            pressedPointerId = eventData.pointerId;
        }

        Fire(eventData != null ? eventData.pointerId : 0);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData != null && eventData.pointerId == pressedPointerId)
        {
            pressedPointerId = int.MinValue;
        }
    }

    public void OnSubmit(BaseEventData eventData)
    {
        Fire(0);
    }

    #endregion

    #region Helpers

    private void Fire(int pointerId)
    {
        if (pressed == null)
        {
            return;
        }

        if (Time.unscaledTime - lastPressTime < 0.15f)
        {
            return;
        }

        lastPressTime = Time.unscaledTime;
        pressed.Invoke();
    }

    #endregion
}
