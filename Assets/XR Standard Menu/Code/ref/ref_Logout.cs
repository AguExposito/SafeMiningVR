using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ref_Logout : MonoBehaviour
{
    public Button yes;
    public TMP_Text text_userdata;
    public GameObject logout_menu_panel;

    private void Awake()
    {
        if (yes != null)
        {
            yes.onClick.AddListener(OnYesClicked);
        }
    }

    private void OnDestroy()
    {
        if (yes != null)
        {
            yes.onClick.RemoveListener(OnYesClicked);
        }
    }

    private void OnYesClicked()
    {
        if (ControlCenter.Instance != null && ControlCenter.Instance.ibManager != null)
        {
            ControlCenter.Instance.ibManager.LogoutFromMenu();
        }
        if (logout_menu_panel != null)
        {
            logout_menu_panel.SetActive(false);
        }
        gameObject.SetActive(false);
    }
}
