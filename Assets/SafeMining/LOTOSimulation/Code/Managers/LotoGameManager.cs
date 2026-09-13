using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
public class LotoGameManager : MonoBehaviour
{
    public const int ToolsRequired = 3;
    public const int SignRequired = 1;
    public const int PuddleRequired = 2;
    public const int ExtinguisherRequired = 1;
    public const int DefectScoreMax = 10;

    public const string Scenario1SceneName = "VR-LOTO-S1";

    public static LotoGameManager Instance { get; private set; }

    public event Action S1Completed;
    public event Action S2Completed;
    public event Action S3Completed;
    public event Action S3StartTooEarly;

    [SerializeField] private UnityEvent onS1Complete;
    [SerializeField] private UnityEvent onS2Complete;
    [SerializeField] private UnityEvent onS3Complete;
    [SerializeField] private UnityEvent onS3StartTooEarly;

    [Header("S1 EPP Good")]
    [SerializeField] private bool helmetGood;
    [SerializeField] private bool glassesGood;
    [SerializeField] private bool vestGood;
    [SerializeField] private bool glovesGood;
    [SerializeField] private bool bootsGood;

    [Header("S1 EPP Defective")]
    [SerializeField] private bool helmetBad;
    [SerializeField] private bool glassesBad;
    [SerializeField] private bool vestBad;
    [SerializeField] private bool glovesBad;
    [SerializeField] private bool bootsBad;
    [SerializeField] private int s1DefectScore = DefectScoreMax;

    [Header("S2 Hazards")]
    [SerializeField] private int toolsReported;
    [SerializeField] private int signReported;
    [SerializeField] private int puddlesReported;
    [SerializeField] private int extinguisherReported;

    [Header("S3 Sequence")]
    [SerializeField] private bool stopPressed;
    [SerializeField] private bool panelOpened;
    [SerializeField] private bool powerCut;
    [SerializeField] private bool panelClosed;
    [SerializeField] private bool haspPlaced;
    [SerializeField] private bool lockPlaced;
    [SerializeField] private bool startedBeforeSequence;
    [SerializeField] private bool zeroEnergyVerified;

    [SerializeField] private bool s1CompleteRaised;
    [SerializeField] private bool s2CompleteRaised;
    [SerializeField] private bool s3CompleteRaised;
    [SerializeField] private bool s3StartTooEarlyRaised;

    public bool HelmetGood => helmetGood;
    public bool GlassesGood => glassesGood;
    public bool VestGood => vestGood;
    public bool GlovesGood => glovesGood;
    public bool BootsGood => bootsGood;

    public bool HelmetBad => helmetBad;
    public bool GlassesBad => glassesBad;
    public bool VestBad => vestBad;
    public bool GlovesBad => glovesBad;
    public bool BootsBad => bootsBad;

    public int S1DefectScore => s1DefectScore;
    public bool HasAnyDefectiveGrab => helmetBad || glassesBad || vestBad || glovesBad || bootsBad;

    public int ToolsReported => toolsReported;
    public int SignReported => signReported;
    public int PuddlesReported => puddlesReported;
    public int ExtinguisherReported => extinguisherReported;

    public bool StopPressed => stopPressed;
    public bool PanelOpened => panelOpened;
    public bool PowerCut => powerCut;
    public bool PanelClosed => panelClosed;
    public bool HaspPlaced => haspPlaced;
    public bool LockPlaced => lockPlaced;
    public bool StartedBeforeSequence => startedBeforeSequence;
    public bool ZeroEnergyVerified => zeroEnergyVerified;

    public bool IsS1Complete => helmetGood && glassesGood && vestGood && glovesGood && bootsGood;

    public bool IsS2Complete =>
        toolsReported >= ToolsRequired &&
        signReported >= SignRequired &&
        puddlesReported >= PuddleRequired &&
        extinguisherReported >= ExtinguisherRequired;

    public bool IsS3SequenceReady =>
        stopPressed && panelOpened && powerCut && panelClosed && haspPlaced && lockPlaced;

    #region Lifecycle

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        if (Instance != this)
        {
            return;
        }

        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        if (Instance != this)
        {
            return;
        }

        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    #endregion

    #region Public API

    public void NotifyHelmetGood()
    {
        if (helmetGood)
        {
            return;
        }

        helmetGood = true;
        TryRaiseS1Complete();
    }

    public void NotifyGlassesGood()
    {
        if (glassesGood)
        {
            return;
        }

        glassesGood = true;
        TryRaiseS1Complete();
    }

    public void NotifyVestGood()
    {
        if (vestGood)
        {
            return;
        }

        vestGood = true;
        TryRaiseS1Complete();
    }

    public void NotifyGlovesGood()
    {
        if (glovesGood)
        {
            return;
        }

        glovesGood = true;
        TryRaiseS1Complete();
    }

    public void NotifyBootsGood()
    {
        if (bootsGood)
        {
            return;
        }

        bootsGood = true;
        TryRaiseS1Complete();
    }

    public void NotifyHelmetBad()
    {
        RecordDefectiveGrab(ref helmetBad);
    }

    public void NotifyGlassesBad()
    {
        RecordDefectiveGrab(ref glassesBad);
    }

    public void NotifyVestBad()
    {
        RecordDefectiveGrab(ref vestBad);
    }

    public void NotifyGlovesBad()
    {
        RecordDefectiveGrab(ref glovesBad);
    }

    public void NotifyBootsBad()
    {
        RecordDefectiveGrab(ref bootsBad);
    }

    public void NotifyEppGrabbed(EppItemType type, EppItemCondition condition)
    {
        if (condition == EppItemCondition.Defective)
        {
            NotifyEppBad(type);
            return;
        }

        NotifyEppGood(type);
    }

    public void TryContinueS1()
    {
        if (!IsS1Complete)
        {
            return;
        }

        if (ControlCenter.Instance == null || ControlCenter.Instance.simulationOrchestratorManager == null)
        {
            return;
        }

        ControlCenter.Instance.simulationOrchestratorManager.OnScenario1Completed();
    }

    public void NotifyTools()
    {
        IncrementHazard(ref toolsReported, ToolsRequired);
    }

    public void NotifySign()
    {
        IncrementHazard(ref signReported, SignRequired);
    }

    public void NotifyPuddle()
    {
        IncrementHazard(ref puddlesReported, PuddleRequired);
    }

    public void NotifyExtinguisher()
    {
        IncrementHazard(ref extinguisherReported, ExtinguisherRequired);
    }

    public void NotifyStop()
    {
        if (stopPressed)
        {
            return;
        }

        stopPressed = true;
    }

    public void NotifyPanelOpened()
    {
        if (panelOpened)
        {
            return;
        }

        panelOpened = true;
    }

    public void NotifyPowerCut()
    {
        if (powerCut)
        {
            return;
        }

        powerCut = true;
    }

    public void NotifyPanelClosed()
    {
        if (panelClosed)
        {
            return;
        }

        panelClosed = true;
    }

    public void NotifyHaspPlaced()
    {
        if (haspPlaced)
        {
            return;
        }

        haspPlaced = true;
    }

    public void NotifyLockPlaced()
    {
        if (lockPlaced)
        {
            return;
        }

        lockPlaced = true;
    }

    public void CheckStartButton()
    {
        if (s3CompleteRaised)
        {
            return;
        }

        if (!IsS3SequenceReady)
        {
            startedBeforeSequence = true;
            if (s3StartTooEarlyRaised)
            {
                return;
            }

            s3StartTooEarlyRaised = true;
            onS3StartTooEarly?.Invoke();
            S3StartTooEarly?.Invoke();
            return;
        }

        zeroEnergyVerified = true;
        s3CompleteRaised = true;
        onS3Complete?.Invoke();
        S3Completed?.Invoke();
    }

    public void ResetBoard()
    {
        helmetGood = false;
        glassesGood = false;
        vestGood = false;
        glovesGood = false;
        bootsGood = false;

        helmetBad = false;
        glassesBad = false;
        vestBad = false;
        glovesBad = false;
        bootsBad = false;
        s1DefectScore = DefectScoreMax;

        toolsReported = 0;
        signReported = 0;
        puddlesReported = 0;
        extinguisherReported = 0;

        stopPressed = false;
        panelOpened = false;
        powerCut = false;
        panelClosed = false;
        haspPlaced = false;
        lockPlaced = false;
        startedBeforeSequence = false;
        zeroEnergyVerified = false;

        s1CompleteRaised = false;
        s2CompleteRaised = false;
        s3CompleteRaised = false;
        s3StartTooEarlyRaised = false;
    }

    #endregion

    #region Event Handlers

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == Scenario1SceneName)
        {
            ResetBoard();
        }
    }

    #endregion

    #region Helpers

    private void NotifyEppGood(EppItemType type)
    {
        switch (type)
        {
            case EppItemType.Helmet:
                NotifyHelmetGood();
                break;
            case EppItemType.SafetyGlasses:
                NotifyGlassesGood();
                break;
            case EppItemType.ReflectiveVest:
                NotifyVestGood();
                break;
            case EppItemType.Gloves:
                NotifyGlovesGood();
                break;
            case EppItemType.SafetyBoots:
                NotifyBootsGood();
                break;
        }
    }

    private void NotifyEppBad(EppItemType type)
    {
        switch (type)
        {
            case EppItemType.Helmet:
                NotifyHelmetBad();
                break;
            case EppItemType.SafetyGlasses:
                NotifyGlassesBad();
                break;
            case EppItemType.ReflectiveVest:
                NotifyVestBad();
                break;
            case EppItemType.Gloves:
                NotifyGlovesBad();
                break;
            case EppItemType.SafetyBoots:
                NotifyBootsBad();
                break;
        }
    }

    private void RecordDefectiveGrab(ref bool flag)
    {
        if (flag)
        {
            return;
        }

        flag = true;
        s1DefectScore = 0;
    }

    private void IncrementHazard(ref int count, int required)
    {
        if (count >= required)
        {
            return;
        }

        count++;
        TryRaiseS2Complete();
    }

    private void TryRaiseS1Complete()
    {
        if (s1CompleteRaised || !IsS1Complete)
        {
            return;
        }

        s1CompleteRaised = true;
        onS1Complete?.Invoke();
        S1Completed?.Invoke();
    }

    private void TryRaiseS2Complete()
    {
        if (s2CompleteRaised || !IsS2Complete)
        {
            return;
        }

        s2CompleteRaised = true;
        onS2Complete?.Invoke();
        S2Completed?.Invoke();
    }

    #endregion
}
