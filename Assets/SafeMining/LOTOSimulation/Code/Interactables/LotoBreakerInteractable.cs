using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(ref_LotoBreaker))]
public class LotoBreakerInteractable : MonoBehaviour
{
    [SerializeField] private LotoSequenceManager sequenceManager;
    [SerializeField] private Color onColor = new Color(0.2f, 0.8f, 0.2f);
    [SerializeField] private Color offColor = new Color(0.8f, 0.2f, 0.2f);
    [SerializeField] private Color alertFlashColor = new Color(1f, 0.55f, 0.1f);
    [SerializeField] private float alertFlashSeconds = 0.45f;

    private ref_LotoBreaker breakerRef;
    private LotoDwellReticle dwellReticle;
    private Coroutine alertFlashRoutine;

    #region Lifecycle

    private void Awake()
    {
        breakerRef = GetComponent<ref_LotoBreaker>();
        BindDwellReticle();
        ApplyVisual();
    }

    private void OnDestroy()
    {
        if (dwellReticle != null)
        {
            dwellReticle.Unbind();
        }

        if (alertFlashRoutine != null)
        {
            StopCoroutine(alertFlashRoutine);
            alertFlashRoutine = null;
        }
    }

    #endregion

    #region Interaction

    public void Configure(LotoSequenceManager manager)
    {
        sequenceManager = manager;
    }

    public void SetInteractableEnabled(bool enabled)
    {
        if (breakerRef != null && breakerRef.interactable != null)
        {
            breakerRef.interactable.enabled = enabled;
        }

        if (dwellReticle != null)
        {
            dwellReticle.SetDwellEnabled(enabled);
        }
    }

    public void ActivateFromPc()
    {
        if (sequenceManager == null || breakerRef == null || breakerRef.isOff)
        {
            return;
        }

        bool missingGloves = !sequenceManager.HasSessionGloves();
        if (!sequenceManager.TryToggleBreakerOff())
        {
            return;
        }

        breakerRef.isOff = true;
        ApplyVisual();
        if (missingGloves)
        {
            PlayAlertFlash();
        }

        SetInteractableEnabled(false);
    }

    private void BindDwellReticle()
    {
        if (breakerRef == null || breakerRef.interactable == null)
        {
            return;
        }

        dwellReticle = GetComponent<LotoDwellReticle>();
        if (dwellReticle == null)
        {
            dwellReticle = gameObject.AddComponent<LotoDwellReticle>();
        }

        dwellReticle.Bind(breakerRef.interactable, ActivateFromPc);
    }

    private void PlayAlertFlash()
    {
        if (breakerRef == null || breakerRef.breakerRenderer == null)
        {
            return;
        }

        if (alertFlashRoutine != null)
        {
            StopCoroutine(alertFlashRoutine);
        }

        alertFlashRoutine = StartCoroutine(AlertFlashRoutine());
    }

    private IEnumerator AlertFlashRoutine()
    {
        if (breakerRef.breakerRenderer != null)
        {
            breakerRef.breakerRenderer.material.color = alertFlashColor;
        }

        yield return new WaitForSeconds(alertFlashSeconds);
        ApplyVisual();
        alertFlashRoutine = null;
    }

    private void ApplyVisual()
    {
        if (breakerRef != null && breakerRef.breakerRenderer != null)
        {
            breakerRef.breakerRenderer.material.color = breakerRef.isOff ? offColor : onColor;
        }
    }

    #endregion
}
