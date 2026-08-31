using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

[RequireComponent(typeof(TeleportationArea))]
public class LotoS1TeleportToNextArea : MonoBehaviour
{
    [SerializeField] private EppEquipmentManager equipmentManager;
    [SerializeField] private TeleportationArea teleportationArea;
    [SerializeField] private Renderer padRenderer;
    [SerializeField] private Color readyColor = new Color(0.05f, 0.92f, 0.95f, 1f);
    [SerializeField] private Color idleColor = new Color(0.15f, 0.18f, 0.2f, 0.35f);

    private bool isReady;
    private bool transitionTriggered;

    #region Lifecycle

    private void Awake()
    {
        if (teleportationArea == null)
        {
            teleportationArea = GetComponent<TeleportationArea>();
        }

        if (padRenderer == null)
        {
            padRenderer = GetComponent<Renderer>();
        }

        SetReady(false);
    }

    private void OnEnable()
    {
        if (teleportationArea != null)
        {
            teleportationArea.teleporting.AddListener(OnTeleporting);
        }
    }

    private void OnDisable()
    {
        if (teleportationArea != null)
        {
            teleportationArea.teleporting.RemoveListener(OnTeleporting);
        }
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

        if (teleportationArea != null)
        {
            teleportationArea.enabled = ready;
        }

        if (padRenderer != null)
        {
            padRenderer.enabled = true;
            ApplyPadColor(ready ? readyColor : idleColor, ready);
        }

        gameObject.SetActive(true);
    }

    #endregion

    #region Event Handlers

    private void OnTeleporting(TeleportingEventArgs args)
    {
        if (!isReady || transitionTriggered)
        {
            return;
        }

        transitionTriggered = true;
        if (equipmentManager != null)
        {
            equipmentManager.TryContinue();
        }
    }

    #endregion

    #region Helpers

    private void ApplyPadColor(Color color, bool emissive)
    {
        if (padRenderer == null)
        {
            return;
        }

        Material material = padRenderer.material;
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
