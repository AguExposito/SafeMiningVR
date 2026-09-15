using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ItemTrigger : MonoBehaviour
{
    public bool triggerCompletedEvent = false;
    [Header("Hold Settings")]
    [Tooltip("Tiempo necesario para completar la interacción.")]
    [SerializeField] private float holdDuration = 2f;

    [Tooltip("Si está activo, el evento solo podrá ejecutarse una vez.")]
    [SerializeField] private bool triggerOnce = false;


    [Header("Detection")]
    [Tooltip("Permite interactuar mediante Collider / Trigger.")]
    [SerializeField] private bool useCollider = true;

    [Tooltip("Permite interactuar mediante Raycast.")]
    [SerializeField] private bool useRaycast = true;

    [Tooltip("Tag que debe tener el Collider que entra en el Trigger.")]
    [SerializeField] private string targetTag = "Hand";


    [Header("Reticle")]
    [Tooltip("Image utilizada para mostrar el progreso de carga.")]
    [SerializeField] private Image reticleFill;

    [Tooltip("Objeto raíz de la retícula.")]
    [SerializeField] private GameObject reticleObject;


    [Header("Events")]
    [Tooltip("Se ejecuta cuando comienza la interacción.")]
    public UnityEvent OnEnter;

    [Tooltip("Se ejecuta cuando termina la interacción antes de completarse.")]
    public UnityEvent OnExit;

    [Tooltip("Se ejecuta cuando la carga llega al 100%.")]
    public UnityEvent OnHoldComplete;


    // ---------------------------------------------------------
    // Internal State
    // ---------------------------------------------------------

    private float currentTime;

    private bool isInside;
    private bool hasTriggered;

    // Estado de cada método de detección.
    private bool colliderInside;
    private bool raycastInside;


    // ---------------------------------------------------------
    // Unity
    // ---------------------------------------------------------

    private void Awake()
    {
        ResetReticle();
    }

    private void Update()
    {
        if (triggerCompletedEvent)
        {
            triggerCompletedEvent = false;

            OnHoldComplete?.Invoke();

            if (TryGetComponent(out XRGrabInteractable grabInteractable))
            {
                grabInteractable.selectEntered?.Invoke(
                    new UnityEngine.XR.Interaction.Toolkit.SelectEnterEventArgs()
                );
            }

            if (TryGetComponent(out XRSimpleInteractable simpleInteractable))
            {
                simpleInteractable.selectEntered?.Invoke(
                    new UnityEngine.XR.Interaction.Toolkit.SelectEnterEventArgs()
                );
            }
        }

        if (!isInside)
            return;

        if (triggerOnce && hasTriggered)
            return;

        // Evitamos divisiones por cero.
        if (holdDuration <= 0f)
        {
            CompleteHold();
            return;
        }

        currentTime += Time.deltaTime;

        float progress = Mathf.Clamp01(currentTime / holdDuration);

        UpdateReticle(progress);

        if (progress >= 1f)
        {
            CompleteHold();
        }       

    }


    // ---------------------------------------------------------
    // Collider Detection
    // ---------------------------------------------------------

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"OnTriggerEnter: {other.name} entered the trigger of {gameObject.name}");
        if (!useCollider)
            return;

        if (!other.CompareTag(targetTag))
            return;

        colliderInside = true;

        StartInteraction();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!useCollider)
            return;

        if (!other.CompareTag(targetTag))
            return;

        colliderInside = false;

        UpdateInteractionState();
        Debug.Log($"OnTriggerExit: {other.name} exited the trigger of {gameObject.name}");
    }


    // ---------------------------------------------------------
    // Raycast Detection
    // ---------------------------------------------------------

    /// <summary>
    /// Llamar desde el sistema de Raycast cuando el rayo
    /// comienza a apuntar a este objeto.
    /// </summary>
    public void PointerEnter()
    {
        if (!useRaycast)
            return;

        raycastInside = true;

        StartInteraction();
    }


    /// <summary>
    /// Llamar desde el sistema de Raycast cuando el rayo
    /// deja de apuntar a este objeto.
    /// </summary>
    public void PointerExit()
    {
        if (!useRaycast)
            return;

        raycastInside = false;

        UpdateInteractionState();
    }


    // ---------------------------------------------------------
    // Interaction State
    // ---------------------------------------------------------

    private void StartInteraction()
    {
        if (triggerOnce && hasTriggered)
            return;

        // Si ya estaba interactuando, no reiniciamos el tiempo.
        if (isInside)
            return;

        isInside = true;

        currentTime = 0f;

        ShowReticle();
        UpdateReticle(0f);

        OnEnter?.Invoke();
    }


    private void UpdateInteractionState()
    {
        // Si seguimos dentro por cualquiera de los dos sistemas,
        // no debemos cancelar la interacción.
        bool stillInside = colliderInside || raycastInside;

        if (stillInside)
            return;

        if (!isInside)
            return;

        isInside = false;

        currentTime = 0f;

        UpdateReticle(0f);
        HideReticle();

        OnExit?.Invoke();
    }


    // ---------------------------------------------------------
    // Hold
    // ---------------------------------------------------------

    private void CompleteHold()
    {
        if (hasTriggered)
            return;

        hasTriggered = true;

        UpdateReticle(1f);

        OnHoldComplete?.Invoke();

        if (!triggerOnce)
        {
            /*
             * Para interacciones repetibles no dejamos la
             * retícula permanentemente al 100%.
             *
             * El jugador deberá salir y volver a entrar
             * para comenzar otra interacción.
             */
            currentTime = 0f;
            hasTriggered = false;
        }
    }


    // ---------------------------------------------------------
    // Reticle
    // ---------------------------------------------------------

    private void ShowReticle()
    {
        if (reticleObject != null)
            reticleObject.SetActive(true);
    }


    private void HideReticle()
    {
        if (reticleObject != null)
            reticleObject.SetActive(false);
    }


    private void ResetReticle()
    {
        if (reticleFill != null)
            reticleFill.fillAmount = 0f;

        HideReticle();
    }


    private void UpdateReticle(float progress)
    {
        if (reticleFill != null)
            reticleFill.fillAmount = progress;
    }


    // ---------------------------------------------------------
    // Public Reset
    // ---------------------------------------------------------

    /// <summary>
    /// Permite reiniciar completamente el estado
    /// desde otro script o desde un UnityEvent.
    /// </summary>
    public void ResetTrigger()
    {
        currentTime = 0f;

        isInside = false;
        hasTriggered = false;

        colliderInside = false;
        raycastInside = false;

        ResetReticle();
    }
}