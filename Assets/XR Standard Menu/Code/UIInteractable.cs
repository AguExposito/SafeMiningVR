using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIInteractable : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    private Button button;
    private TMP_Dropdown tMP_Dropdown;
    private Toggle toggle;
    private bool interactionsEnabled = true;

    void Awake()
    {
        if (GetComponent<Button>() != null) button = GetComponent<Button>();
        if (GetComponent<TMP_Dropdown>() != null) tMP_Dropdown = GetComponent<TMP_Dropdown>();
        if (GetComponent<Toggle>() != null) toggle = GetComponent<Toggle>();
    }
    public void DisableInteractions()
    {
        interactionsEnabled = false;
        if (button != null) button.interactable = false;
        if (tMP_Dropdown != null) tMP_Dropdown.interactable = false;
        if (toggle != null) toggle.interactable = false;
    }
    public void EnableInteractions()
    {
        interactionsEnabled = true;
        if (button != null) button.interactable = true;
        if (tMP_Dropdown != null) tMP_Dropdown.interactable = true;
        if (toggle != null) toggle.interactable = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button == null) return;
        if (!interactionsEnabled || !button.interactable) return;
        if (SBD_Audio.Instance != null && SBD_Audio.Instance.UI_btn_hover != null)
        {
            ControlCenter.Instance.soundManager.PlayOneShotClip(SBD_Audio.Instance.UI_btn_hover);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (button == null) return;
        if (!interactionsEnabled || !button.interactable) return;
        if (SBD_Audio.Instance != null && SBD_Audio.Instance.UI_btn_click != null)
        {
            ControlCenter.Instance.soundManager.PlayOneShotClip(SBD_Audio.Instance.UI_btn_click);
        }
    }
}
