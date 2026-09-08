using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

public class LotoS1TeleportToNextArea : MonoBehaviour
{
    [SerializeField] private EppEquipmentManager equipmentManager;
    [SerializeField] private Collider triggerCollider;
    [SerializeField] private Renderer[] padRenderers;
    [SerializeField] private Color readyColor = new Color(0.05f, 0.92f, 0.95f, 1f);
    [SerializeField] private Color idleColor = new Color(0.15f, 0.18f, 0.2f, 0.35f);
    [SerializeField] private UnityEvent onActivated;
    [SerializeField] private UnityEvent onTriggered;
    [SerializeField] private UnityEvent onBlocked;

    private bool isReady;
    private bool transitionTriggered;

    #region Lifecycle

    private void Awake()
    {
        DisableLocomotionTeleport();
        if (padRenderers == null || padRenderers.Length == 0)
        {
            padRenderers = GetComponentsInChildren<Renderer>(true);
        }

        EnsureTriggerCollider();
        SetReady(false);
    }

    #endregion

    #region Public API

    public void Configure(EppEquipmentManager manager)
    {
        equipmentManager = manager;
    }

    public void SetReady(bool ready)
    {
        isReady = ready;
        transitionTriggered = false;

        gameObject.SetActive(true);
        EnsureTriggerCollider();
        DisableLocomotionTeleport();

        if (triggerCollider != null)
        {
            triggerCollider.enabled = true;
            triggerCollider.isTrigger = true;
        }

        if (padRenderers != null)
        {
            Color color = ready ? readyColor : idleColor;
            for (int i = 0; i < padRenderers.Length; i++)
            {
                if (padRenderers[i] == null)
                {
                    continue;
                }

                padRenderers[i].enabled = true;
                ApplyPadColor(padRenderers[i], color, ready);
            }
        }

        if (ready)
        {
            onActivated?.Invoke();
        }
    }

    #endregion

    #region Event Handlers

    private void OnTriggerEnter(Collider other)
    {
        TryHandlePlayerOverlap(other, true);
    }

    private void OnTriggerStay(Collider other)
    {
        TryHandlePlayerOverlap(other, false);
    }

    private void TryHandlePlayerOverlap(Collider other, bool invokeBlocked)
    {
        if (!IsPlayerCollider(other))
        {
            return;
        }

        if (!isReady)
        {
            if (invokeBlocked)
            {
                onBlocked?.Invoke();
            }

            return;
        }

        if (transitionTriggered)
        {
            return;
        }

        transitionTriggered = true;
        onTriggered?.Invoke();
        if (equipmentManager != null)
        {
            equipmentManager.TryContinue();
        }
    }

    #endregion

    #region Helpers

    private void DisableLocomotionTeleport()
    {
        TeleportationArea teleportationArea = GetComponent<TeleportationArea>();
        if (teleportationArea != null)
        {
            teleportationArea.enabled = false;
        }
    }

    private void EnsureTriggerCollider()
    {
        if (triggerCollider == null)
        {
            triggerCollider = GetComponent<Collider>();
        }

        if (triggerCollider == null)
        {
            BoxCollider box = gameObject.AddComponent<BoxCollider>();
            box.center = new Vector3(0f, 0f, -0.5f);
            box.size = new Vector3(1.4f, 1.4f, 1f);
            triggerCollider = box;
        }

        triggerCollider.isTrigger = true;
        EnsureTriggerBody();
    }

    private void EnsureTriggerBody()
    {
        Rigidbody body = GetComponent<Rigidbody>();
        if (body == null)
        {
            body = gameObject.AddComponent<Rigidbody>();
        }

        body.isKinematic = true;
        body.useGravity = false;
    }

    private static bool IsPlayerCollider(Collider other)
    {
        if (other == null)
        {
            return false;
        }

        if (other is CharacterController)
        {
            return true;
        }

        if (other.GetComponentInParent<XROrigin>() != null)
        {
            return true;
        }

        return other.CompareTag("Player");
    }

    private static void ApplyPadColor(Renderer renderer, Color color, bool emissive)
    {
        if (renderer == null)
        {
            return;
        }

        Material material = renderer.material;
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }
        else if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }

        if (material.HasProperty("_EmissionColor"))
        {
            if (emissive)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 2.2f);
            }
            else
            {
                material.SetColor("_EmissionColor", Color.black);
            }
        }
    }

    #endregion
}
