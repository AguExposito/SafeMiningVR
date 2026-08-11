using UnityEngine;

public class SBD_Audio : MonoBehaviour, I_SBD
{
    public static SBD_Audio Instance;
    public AudioClip UI_btn_click, UI_btn_hover;
    public AudioClip Ambiental_menu_music0;

    public void Initialize()
    {
        if (Instance == null)
        {
            Instance = this;
            return;
        }
        Destroy(gameObject);
    }
}
