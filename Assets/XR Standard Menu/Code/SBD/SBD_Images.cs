using UnityEngine;

public class SBD_Images : MonoBehaviour, I_SBD
{
    public static SBD_Images Instance;
    public void Initialize()
    {
        if (Instance == null)
        {
            Instance = this;
            return;
        }
        Destroy(gameObject);
    }

    public Sprite UI_spanish_flag, UI_english_flag;
    public Sprite UI_objective_achivied, UI_objective_failed;
}
