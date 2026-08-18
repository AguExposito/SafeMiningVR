using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ref_HazardSpot : MonoBehaviour
{
    public HazardType hazardType;
    public HazardSpotKind spotKind;
    public Renderer spotRenderer;
    public XRSimpleInteractable interactable;

    private bool isReported;
    private Color originalColor;

    public bool IsReported => isReported;

    public void SetReported(bool reported)
    {
        isReported = reported;
        if (spotRenderer != null)
        {
            spotRenderer.material.color = reported
                ? new Color(0.2f, 0.75f, 0.35f)
                : originalColor;
        }

        if (interactable != null)
        {
            interactable.enabled = !reported;
        }
    }

    public void CacheOriginalColor()
    {
        if (spotRenderer != null)
        {
            originalColor = spotRenderer.material.color;
        }
    }

    public void PulseHint()
    {
        if (isReported || spotRenderer == null)
        {
            return;
        }

        spotRenderer.material.color = new Color(0.98f, 0.75f, 0.1f);
    }

    public void RestoreHintColor()
    {
        if (isReported || spotRenderer == null)
        {
            return;
        }

        spotRenderer.material.color = originalColor;
    }
}
