using UnityEngine;
using UnityEngine.Events;

public class LotoGameEventRelay : MonoBehaviour
{
    [SerializeField] private UnityEvent onS1Complete;
    [SerializeField] private UnityEvent onS2Complete;
    [SerializeField] private UnityEvent onS3Complete;
    [SerializeField] private UnityEvent onS3StartTooEarly;

    private bool isSubscribed;

    #region Lifecycle

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void Start()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        TryUnsubscribe();
    }

    #endregion

    #region Public API

    public void NotifyHelmetGood()
    {
        Forward(manager => manager.NotifyHelmetGood());
    }

    public void NotifyGlassesGood()
    {
        Forward(manager => manager.NotifyGlassesGood());
    }

    public void NotifyVestGood()
    {
        Forward(manager => manager.NotifyVestGood());
    }

    public void NotifyGlovesGood()
    {
        Forward(manager => manager.NotifyGlovesGood());
    }

    public void NotifyBootsGood()
    {
        Forward(manager => manager.NotifyBootsGood());
    }

    public void NotifyHelmetBad()
    {
        Forward(manager => manager.NotifyHelmetBad());
    }

    public void NotifyGlassesBad()
    {
        Forward(manager => manager.NotifyGlassesBad());
    }

    public void NotifyVestBad()
    {
        Forward(manager => manager.NotifyVestBad());
    }

    public void NotifyGlovesBad()
    {
        Forward(manager => manager.NotifyGlovesBad());
    }

    public void NotifyBootsBad()
    {
        Forward(manager => manager.NotifyBootsBad());
    }

    public void NotifyTools()
    {
        Forward(manager => manager.NotifyTools());
    }

    public void NotifySign()
    {
        Forward(manager => manager.NotifySign());
    }

    public void NotifyPuddle()
    {
        Forward(manager => manager.NotifyPuddle());
    }

    public void NotifyExtinguisher()
    {
        Forward(manager => manager.NotifyExtinguisher());
    }

    public void NotifyStop()
    {
        Forward(manager => manager.NotifyStop());
    }

    public void NotifyPanelOpened()
    {
        Forward(manager => manager.NotifyPanelOpened());
    }

    public void NotifyPowerCut()
    {
        Forward(manager => manager.NotifyPowerCut());
    }

    public void NotifyPanelClosed()
    {
        Forward(manager => manager.NotifyPanelClosed());
    }

    public void NotifyHaspPlaced()
    {
        Forward(manager => manager.NotifyHaspPlaced());
    }

    public void NotifyLockPlaced()
    {
        Forward(manager => manager.NotifyLockPlaced());
    }

    public void CheckStartButton()
    {
        Forward(manager => manager.CheckStartButton());
    }

    public void TryContinueS1()
    {
        Forward(manager => manager.TryContinueS1());
    }

    public void ResetBoard()
    {
        Forward(manager => manager.ResetBoard());
    }

    #endregion

    #region Event Handlers

    private void HandleS1Completed()
    {
        onS1Complete?.Invoke();
    }

    private void HandleS2Completed()
    {
        onS2Complete?.Invoke();
    }

    private void HandleS3Completed()
    {
        onS3Complete?.Invoke();
    }

    private void HandleS3StartTooEarly()
    {
        onS3StartTooEarly?.Invoke();
    }

    #endregion

    #region Helpers

    private void TrySubscribe()
    {
        if (isSubscribed)
        {
            return;
        }

        LotoGameManager manager = LotoGameManager.Instance;
        if (manager == null)
        {
            return;
        }

        manager.S1Completed += HandleS1Completed;
        manager.S2Completed += HandleS2Completed;
        manager.S3Completed += HandleS3Completed;
        manager.S3StartTooEarly += HandleS3StartTooEarly;
        isSubscribed = true;
    }

    private void TryUnsubscribe()
    {
        if (!isSubscribed)
        {
            return;
        }

        LotoGameManager manager = LotoGameManager.Instance;
        if (manager != null)
        {
            manager.S1Completed -= HandleS1Completed;
            manager.S2Completed -= HandleS2Completed;
            manager.S3Completed -= HandleS3Completed;
            manager.S3StartTooEarly -= HandleS3StartTooEarly;
        }

        isSubscribed = false;
    }

    private static void Forward(System.Action<LotoGameManager> action)
    {
        LotoGameManager manager = LotoGameManager.Instance;
        if (manager == null)
        {
            return;
        }

        action(manager);
    }

    public bool GetPowerCutState() {
        LotoGameManager manager = LotoGameManager.Instance;
        if (manager == null)
        {
            return false;
        }

        return manager.GetPowerCutState();

    }
    #endregion
}
