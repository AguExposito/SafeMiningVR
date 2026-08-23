using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public static class LotoTrainingFeedback
{
    public static void PlaySuccess()
    {
        PlayClip(true);
        SendHaptics(0.25f, 0.08f);
    }

    public static void PlayFail()
    {
        PlayClip(false);
        SendHaptics(0.55f, 0.12f);
    }

    private static void PlayClip(bool success)
    {
        if (SBD_Audio.Instance == null || SBD_Audio.Instance.UI_btn_click == null)
        {
            return;
        }

        AudioSource.PlayClipAtPoint(
            SBD_Audio.Instance.UI_btn_click,
            ResolvePlayPosition(),
            success ? 0.7f : 0.9f);
    }

    private static Vector3 ResolvePlayPosition()
    {
        Camera cam = Camera.main;
        return cam != null ? cam.transform.position : Vector3.zero;
    }

    private static void SendHaptics(float amplitude, float duration)
    {
        XRBaseController[] controllers = Object.FindObjectsByType<XRBaseController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < controllers.Length; i++)
        {
            if (controllers[i] != null)
            {
                controllers[i].SendHapticImpulse(amplitude, duration);
            }
        }
    }
}
