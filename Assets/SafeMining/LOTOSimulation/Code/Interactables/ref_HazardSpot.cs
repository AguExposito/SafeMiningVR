using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ref_HazardSpot : MonoBehaviour
{
    public HazardType hazardType;
    public HazardSpotKind spotKind;
    public Renderer spotRenderer;
    public XRSimpleInteractable interactable;

    [SerializeField] private GameObject[] objectsToHide;
    [SerializeField] private GameObject[] objectsToShow;
    [SerializeField] private Transform objectToUpright;
    [SerializeField] private Transform objectToMove;
    [SerializeField] private Transform moveTarget;

    private bool isReported;
    private Color originalColor;
    private Coroutine pulseRoutine;

    public bool IsReported => isReported;

    public void CacheOriginalColor()
    {
        ResolveRenderer();
        if (spotRenderer == null || spotRenderer.sharedMaterial == null)
        {
            return;
        }

        originalColor = spotRenderer.sharedMaterial.color;
    }

    public void SetReported(bool reported)
    {
        isReported = reported;
        if (interactable != null)
        {
            interactable.enabled = !reported;
        }

        if (reported && spotKind == HazardSpotKind.RealHazard)
        {
            Remediate();
        }
    }

    public void Remediate()
    {
        if (objectsToHide != null)
        {
            for (int i = 0; i < objectsToHide.Length; i++)
            {
                if (objectsToHide[i] != null)
                {
                    objectsToHide[i].SetActive(false);
                }
            }
        }

        if (objectsToShow != null)
        {
            for (int i = 0; i < objectsToShow.Length; i++)
            {
                if (objectsToShow[i] != null)
                {
                    objectsToShow[i].SetActive(true);
                }
            }
        }

        if (objectToUpright != null)
        {
            objectToUpright.localRotation = Quaternion.identity;
        }

        if (objectToMove != null && moveTarget != null)
        {
            objectToMove.SetPositionAndRotation(moveTarget.position, moveTarget.rotation);
        }
    }

    public void PulseHint()
    {
        ResolveRenderer();
        if (isReported || spotRenderer == null || spotRenderer.sharedMaterial == null)
        {
            return;
        }

        if (pulseRoutine != null)
        {
            StopCoroutine(pulseRoutine);
        }

        pulseRoutine = StartCoroutine(PulseRoutine());
    }

    public void RestoreHintColor()
    {
        ResolveRenderer();
        if (isReported || spotRenderer == null || spotRenderer.sharedMaterial == null)
        {
            return;
        }

        spotRenderer.material.color = originalColor;
    }

    private void ResolveRenderer()
    {
        if (spotRenderer != null)
        {
            return;
        }

        spotRenderer = GetComponentInChildren<Renderer>(true);
    }

    private IEnumerator PulseRoutine()
    {
        spotRenderer.material.color = new Color(0.98f, 0.82f, 0.35f);
        yield return new WaitForSeconds(0.85f);
        RestoreHintColor();
        pulseRoutine = null;
    }
}
