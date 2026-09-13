using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class LotoS1ContinueInteractable : MonoBehaviour
{
    [SerializeField] private XRSimpleInteractable interactable;
    [SerializeField] private UnityEvent onContinue;

    private LotoDwellReticle dwellReticle;

    #region Lifecycle

    private void Awake()
    {
        if (interactable == null)
        {
            interactable = GetComponent<XRSimpleInteractable>();
        }

        BindDwellReticle();
    }

    private void OnDestroy()
    {
        if (dwellReticle != null)
        {
            dwellReticle.Unbind();
        }
    }

    #endregion

    #region Interaction

    public void ActivateFromPc()
    {
        LotoGameManager board = LotoGameManager.Instance;
        if (board == null || !board.IsS1Complete)
        {
            return;
        }

        board.TryContinueS1();
        onContinue?.Invoke();
        ConsumeContinueInteraction();
    }

    private void ConsumeContinueInteraction()
    {
        if (dwellReticle != null)
        {
            dwellReticle.SetDwellEnabled(false);
        }

        if (interactable != null)
        {
            interactable.enabled = false;
        }
    }

    private void Update()
    {
        Renderer doorRenderer = GetComponent<Renderer>();
        if (doorRenderer == null)
        {
            return;
        }

        LotoGameManager board = LotoGameManager.Instance;
        bool ready = board != null && board.IsS1Complete;
        Color readyColor = new Color(0.133f, 0.773f, 0.369f, 1f);
        Color blocked = new Color(0.45f, 0.47f, 0.5f, 1f);
        doorRenderer.material.color = ready ? readyColor : blocked;
    }

    private void BindDwellReticle()
    {
        if (interactable == null)
        {
            return;
        }

        dwellReticle = GetComponent<LotoDwellReticle>();
        if (dwellReticle == null)
        {
            dwellReticle = gameObject.AddComponent<LotoDwellReticle>();
        }

        dwellReticle.Bind(interactable, ActivateFromPc);
    }

    #endregion
}
