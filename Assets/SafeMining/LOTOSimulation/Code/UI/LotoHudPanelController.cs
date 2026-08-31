using UnityEngine;
using XRStandardMenu;

public class LotoHudPanelController : MonoBehaviour
{
    [SerializeField] private Util_PanelFollower panelFollower;
    [SerializeField] private Vector3 comfortOffset = new Vector3(0f, -0.06f, 0.72f);
    [SerializeField] private float comfortWorldScale = 0.00072f;
    [SerializeField] private Vector2 comfortSize = new Vector2(900f, 560f);

    #region Public API

    public void Configure(Util_PanelFollower follower)
    {
        panelFollower = follower;
        ApplyComfortLayout();
    }

    public void ApplyComfortLayout()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = comfortSize;
            transform.localScale = Vector3.one * comfortWorldScale;
        }

        if (panelFollower != null)
        {
            panelFollower.SetOffset(comfortOffset);
            panelFollower.SetFixedDistance(comfortOffset.z);
            panelFollower.SetFollowMode(FollowMode.Advanced);
        }
    }

    #endregion

    #region Lifecycle

    private void Awake()
    {
        if (panelFollower == null)
        {
            panelFollower = GetComponent<Util_PanelFollower>();
        }

        ApplyComfortLayout();
    }

    #endregion
}
