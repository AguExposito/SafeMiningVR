using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
[RequireComponent(typeof(TMP_InputField))]
public class LotoVrLoginFieldActivator : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, ISubmitHandler
{
    private TMP_InputField field;
    private bool secure;
    private System.Action<TMP_InputField, bool> activated;
    private float lastActivateTime;

    #region Public API

    public void Configure(bool isSecure, System.Action<TMP_InputField, bool> onActivated)
    {
        field = GetComponent<TMP_InputField>();
        secure = isSecure;
        activated = onActivated;
        if (field != null)
        {
            field.shouldHideMobileInput = true;
            field.shouldHideSoftKeyboard = true;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ActivateFromPointer();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        ActivateFromPointer();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        ActivateFromPointer();
    }

    #endregion

    #region Helpers

    private void ActivateFromPointer()
    {
        if (field == null || !field.interactable)
        {
            return;
        }

        if (Time.unscaledTime - lastActivateTime < 0.15f)
        {
            return;
        }

        lastActivateTime = Time.unscaledTime;
        field.shouldHideMobileInput = true;
        field.shouldHideSoftKeyboard = true;
        field.DeactivateInputField();
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == field.gameObject)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        activated?.Invoke(field, secure);
    }

    #endregion
}
