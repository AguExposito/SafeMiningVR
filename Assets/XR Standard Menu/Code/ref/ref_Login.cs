using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ref_Login : MonoBehaviour
{
    public GameObject activationCheck, pinCheck;
    public TMP_Text activation_text, pin_text;
    public GameObject pin_panel;
    public Button activationBtn, pinBtn, closePinPanelBtn;
    public TMP_Text login_status;
    public TMP_Text welcome_text;
    public GameObject welcome_panel;
    #region Pin Panel
    public TMP_Text displayPin;
    public Button Btn_0, Btn_1, Btn_2, Btn_3, Btn_4, Btn_5, Btn_6, Btn_7, Btn_8, Btn_9, Btn_clear, Btn_enter;

    #endregion
}
