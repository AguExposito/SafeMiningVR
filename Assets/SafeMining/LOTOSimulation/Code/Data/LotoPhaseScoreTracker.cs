using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LotoPhaseScoreTracker : MonoBehaviour
{
    public const float MaxPhaseScore = 10f;
    public const int DefectiveEppTotal = 4;
    public const int ToolsTotal = 3;
    public const int SignTotal = 1;
    public const int PuddleTotal = 2;
    public const int ExtinguisherTotal = 1;
    public const int ObstacleTotal = ToolsTotal + SignTotal + PuddleTotal + ExtinguisherTotal;
    public const int SequenceStepTotal = 6;

    public static LotoPhaseScoreTracker Instance { get; private set; }

    [Header("Weights (base 1)")]
    [SerializeField] private float weightS1 = 0.30f;
    [SerializeField] private float weightS2 = 0.25f;
    [SerializeField] private float weightS3 = 0.45f;

    [Header("Approval Thresholds (base 100)")]
    [SerializeField] private int thresholdS1 = 60;
    [SerializeField] private int thresholdS2 = 60;
    [SerializeField] private int thresholdS3 = 60;
    [SerializeField] private int thresholdGlobal = 60;

    [Header("S1 Good EPP")]
    [SerializeField] private bool helmetGood;
    [SerializeField] private bool glassesGood;
    [SerializeField] private bool vestGood;
    [SerializeField] private bool glovesGood;
    [SerializeField] private bool bootsGood;

    [Header("S1 Defective EPP Grabbed")]
    [SerializeField] private bool helmetBad;
    [SerializeField] private bool glassesBad;
    [SerializeField] private bool vestBad;
    [SerializeField] private bool glovesBad;
    [SerializeField] private bool bootsBad;

    [Header("S2 Obstacles Fixed")]
    [SerializeField] private int toolsFixed;
    [SerializeField] private int signFixed;
    [SerializeField] private int puddlesFixed;
    [SerializeField] private int extinguisherFixed;
    [SerializeField] private bool s2Frozen;

    [Header("S3 Sequence")]
    [SerializeField] private bool stopPressed;
    [SerializeField] private bool panelOpened;
    [SerializeField] private bool powerCut;
    [SerializeField] private bool panelClosed;
    [SerializeField] private bool haspPlaced;
    [SerializeField] private bool lockPlaced;
    [SerializeField] private bool startedTooEarly;
    [SerializeField] private bool zeroEnergyVerified;
    [SerializeField] private bool s3Frozen;
    [SerializeField] private int s3ScoredSteps;

    [Header("Scores (base 10)")]
    [SerializeField] private float scoreS1 = MaxPhaseScore;
    [SerializeField] private float scoreS2;
    [SerializeField] private float scoreS3;
    [SerializeField] private float scoreGlobal;

    [Header("Approval")]
    [SerializeField] private bool approvedS1;
    [SerializeField] private bool approvedS2;
    [SerializeField] private bool approvedS3;
    [SerializeField] private bool approvedGlobal;
    [SerializeField] private bool runFinalized;

    public float ScoreS1 => scoreS1;
    public float ScoreS2 => scoreS2;
    public float ScoreS3 => scoreS3;
    public float ScoreGlobal => scoreGlobal;
    public bool ApprovedS1 => approvedS1;
    public bool ApprovedS2 => approvedS2;
    public bool ApprovedS3 => approvedS3;
    public bool ApprovedGlobal => approvedGlobal;
    public bool StartedTooEarly => startedTooEarly;
    public bool ZeroEnergyVerified => zeroEnergyVerified;
    public float WeightS1 => weightS1;
    public float WeightS2 => weightS2;
    public float WeightS3 => weightS3;
    public int ThresholdS1 => thresholdS1;
    public int ThresholdS2 => thresholdS2;
    public int ThresholdS3 => thresholdS3;
    public int ThresholdGlobal => thresholdGlobal;
    public int DefectiveGrabbedCount => CountTrue(helmetBad, glassesBad, vestBad, glovesBad, bootsBad);
    public int ObstaclesFixedCount => toolsFixed + signFixed + puddlesFixed + extinguisherFixed;
    public int SequenceStepCount => CountTrue(stopPressed, panelOpened, powerCut, panelClosed, haspPlaced, lockPlaced);
    public bool IsSequenceComplete => SequenceStepCount >= SequenceStepTotal;
    public bool RunFinalized => runFinalized;

    #region Lifecycle

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        Recalculate();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
        TrySubscribeBoard();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        UnsubscribeBoard();
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TrySubscribeBoard();
    }

    private void TrySubscribeBoard()
    {
        LotoGameManager board = LotoGameManager.Instance;
        if (board == null)
        {
            return;
        }

        board.S1Completed -= HandleBoardChanged;
        board.S2Completed -= HandleBoardChanged;
        board.S3Completed -= HandleBoardChanged;
        board.S3StartTooEarly -= HandleBoardChanged;
        board.S1Completed += HandleBoardChanged;
        board.S2Completed += HandleBoardChanged;
        board.S3Completed += HandleBoardChanged;
        board.S3StartTooEarly += HandleBoardChanged;
        SyncFromBoard(board);
    }

    private void UnsubscribeBoard()
    {
        LotoGameManager board = LotoGameManager.Instance;
        if (board == null)
        {
            return;
        }

        board.S1Completed -= HandleBoardChanged;
        board.S2Completed -= HandleBoardChanged;
        board.S3Completed -= HandleBoardChanged;
        board.S3StartTooEarly -= HandleBoardChanged;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnValidate()
    {
        toolsFixed = Mathf.Clamp(toolsFixed, 0, ToolsTotal);
        signFixed = Mathf.Clamp(signFixed, 0, SignTotal);
        puddlesFixed = Mathf.Clamp(puddlesFixed, 0, PuddleTotal);
        extinguisherFixed = Mathf.Clamp(extinguisherFixed, 0, ExtinguisherTotal);
        Recalculate();
    }

    #endregion

    #region S1 EPP

    public void NotifyHelmetGood() => SetFlag(ref helmetGood);
    public void NotifyGlassesGood() => SetFlag(ref glassesGood);
    public void NotifyVestGood() => SetFlag(ref vestGood);
    public void NotifyGlovesGood() => SetFlag(ref glovesGood);
    public void NotifyBootsGood() => SetFlag(ref bootsGood);

    public void NotifyHelmetBad() => SetFlag(ref helmetBad);
    public void NotifyGlassesBad() => SetFlag(ref glassesBad);
    public void NotifyVestBad() => SetFlag(ref vestBad);
    public void NotifyGlovesBad() => SetFlag(ref glovesBad);
    public void NotifyBootsBad() => SetFlag(ref bootsBad);

    public void NotifyGoodEpp()
    {
        if (!helmetGood)
        {
            NotifyHelmetGood();
            return;
        }

        if (!glassesGood)
        {
            NotifyGlassesGood();
            return;
        }

        if (!vestGood)
        {
            NotifyVestGood();
            return;
        }

        if (!glovesGood)
        {
            NotifyGlovesGood();
            return;
        }

        NotifyBootsGood();
    }

    public void NotifyBadEpp()
    {
        if (!helmetBad)
        {
            NotifyHelmetBad();
            return;
        }

        if (!glassesBad)
        {
            NotifyGlassesBad();
            return;
        }

        if (!vestBad)
        {
            NotifyVestBad();
            return;
        }

        if (!glovesBad)
        {
            NotifyGlovesBad();
            return;
        }

        NotifyBootsBad();
    }

    #endregion

    #region S2 Obstacles

    public void NotifyToolFixed() => IncrementObstacle(ref toolsFixed, ToolsTotal);
    public void NotifySignFixed() => IncrementObstacle(ref signFixed, SignTotal);
    public void NotifyPuddleFixed() => IncrementObstacle(ref puddlesFixed, PuddleTotal);
    public void NotifyExtinguisherFixed() => IncrementObstacle(ref extinguisherFixed, ExtinguisherTotal);

    public void NotifyObstacleFixed()
    {
        if (toolsFixed < ToolsTotal)
        {
            NotifyToolFixed();
            return;
        }

        if (signFixed < SignTotal)
        {
            NotifySignFixed();
            return;
        }

        if (puddlesFixed < PuddleTotal)
        {
            NotifyPuddleFixed();
            return;
        }

        NotifyExtinguisherFixed();
    }

    public void FreezeS2()
    {
        if (s2Frozen)
        {
            return;
        }

        s2Frozen = true;
        Recalculate();
    }

    #endregion

    #region S3 Sequence

    public void NotifyStop() => SetSequenceStep(ref stopPressed, true);
    public void NotifyPanelOpened() => SetSequenceStep(ref panelOpened, stopPressed);
    public void NotifyPowerCut() => SetSequenceStep(ref powerCut, panelOpened);
    public void NotifyPanelClosed() => SetSequenceStep(ref panelClosed, powerCut);
    public void NotifyHaspPlaced() => SetSequenceStep(ref haspPlaced, panelClosed);
    public void NotifyLockPlaced() => SetSequenceStep(ref lockPlaced, haspPlaced);

    public void NotifyStartPressed()
    {
        FreezeS2();
        if (runFinalized)
        {
            return;
        }

        if (!IsSequenceComplete)
        {
            if (!s3Frozen)
            {
                startedTooEarly = true;
                FreezeS3();
            }

            return;
        }

        if (!s3Frozen)
        {
            zeroEnergyVerified = true;
            FreezeS3();
        }

        FinalizeRun();
    }

    #endregion

    #region Run Lifecycle

    public void ResetRun()
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
        toolsFixed = 0;
        signFixed = 0;
        puddlesFixed = 0;
        extinguisherFixed = 0;
        s2Frozen = false;
        stopPressed = false;
        panelOpened = false;
        powerCut = false;
        panelClosed = false;
        haspPlaced = false;
        lockPlaced = false;
        startedTooEarly = false;
        zeroEnergyVerified = false;
        s3Frozen = false;
        s3ScoredSteps = 0;
        runFinalized = false;
        Recalculate();
    }

    public void FinalizeRun()
    {
        if (runFinalized)
        {
            return;
        }

        runFinalized = true;
        s2Frozen = true;
        FreezeS3();

        if (ControlCenter.Instance != null && ControlCenter.Instance.scoreManager != null)
        {
            ControlCenter.Instance.scoreManager.SaveLotoAttempt(BuildAttemptRecord());
        }
    }

    public LotoAttemptRecord BuildAttemptRecord()
    {
        DateTime now = DateTime.Now;
        return new LotoAttemptRecord
        {
            date = now.ToString("dd/MM/yy"),
            time = now.ToString("HH:mm"),
            scoreS1 = scoreS1,
            scoreS2 = scoreS2,
            scoreS3 = scoreS3,
            scoreGlobal = scoreGlobal,
            approvedS1 = approvedS1,
            approvedS2 = approvedS2,
            approvedS3 = approvedS3,
            approved = approvedGlobal
        };
    }

    [ContextMenu("Debug/Recalculate")]
    public void Recalculate()
    {
        scoreS1 = MaxPhaseScore * (1f - Mathf.Min(DefectiveGrabbedCount, DefectiveEppTotal) / (float)DefectiveEppTotal);
        scoreS2 = MaxPhaseScore * ObstaclesFixedCount / ObstacleTotal;
        scoreS3 = MaxPhaseScore * (s3Frozen ? s3ScoredSteps : SequenceStepCount) / SequenceStepTotal;

        float totalWeight = weightS1 + weightS2 + weightS3;
        scoreGlobal = totalWeight > 0f
            ? (scoreS1 * weightS1 + scoreS2 * weightS2 + scoreS3 * weightS3) / totalWeight
            : 0f;

        approvedS1 = IsApproved(scoreS1, thresholdS1);
        approvedS2 = IsApproved(scoreS2, thresholdS2);
        approvedS3 = IsApproved(scoreS3, thresholdS3);
        approvedGlobal = IsApproved(scoreGlobal, thresholdGlobal);
    }

    #endregion

    #region Helpers

    private void HandleBoardChanged()
    {
        LotoGameManager board = LotoGameManager.Instance;
        if (board != null)
        {
            SyncFromBoard(board);
        }
    }

    private void SyncFromBoard(LotoGameManager board)
    {
        helmetGood = board.HelmetGood;
        glassesGood = board.GlassesGood;
        vestGood = board.VestGood;
        glovesGood = board.GlovesGood;
        bootsGood = board.BootsGood;
        helmetBad = board.HelmetBad;
        glassesBad = board.GlassesBad;
        vestBad = board.VestBad;
        glovesBad = board.GlovesBad;
        bootsBad = board.BootsBad;
        toolsFixed = Mathf.Min(board.ToolsReported, ToolsTotal);
        signFixed = Mathf.Min(board.SignReported, SignTotal);
        puddlesFixed = Mathf.Min(board.PuddlesReported, PuddleTotal);
        extinguisherFixed = Mathf.Min(board.ExtinguisherReported, ExtinguisherTotal);
        stopPressed = board.StopPressed;
        panelOpened = board.PanelOpened;
        powerCut = board.PowerCut;
        panelClosed = board.PanelClosed;
        haspPlaced = board.HaspPlaced;
        lockPlaced = board.LockPlaced;
        startedTooEarly = board.StartedBeforeSequence;
        zeroEnergyVerified = board.ZeroEnergyVerified;
        if (board.S3ScoredStepCount > 0 || board.StartedBeforeSequence || board.ZeroEnergyVerified)
        {
            s3Frozen = true;
            s3ScoredSteps = board.S3ScoredStepCount > 0 ? board.S3ScoredStepCount : SequenceStepCount;
        }

        Recalculate();
    }

    private void SetFlag(ref bool flag)
    {
        if (flag)
        {
            return;
        }

        flag = true;
        Recalculate();
    }

    private void IncrementObstacle(ref int count, int total)
    {
        if (s2Frozen || count >= total)
        {
            return;
        }

        count++;
        Recalculate();
    }

    private void SetSequenceStep(ref bool step, bool prerequisite)
    {
        FreezeS2();
        if (step || !prerequisite)
        {
            return;
        }

        step = true;
        Recalculate();
    }

    private void FreezeS3()
    {
        if (!s3Frozen)
        {
            s3Frozen = true;
            s3ScoredSteps = SequenceStepCount;
        }

        Recalculate();
    }

    private static bool IsApproved(float phaseScore, int thresholdPercent)
    {
        return phaseScore * 10f >= thresholdPercent - 0.001f;
    }

    private static int CountTrue(params bool[] flags)
    {
        int count = 0;
        for (int i = 0; i < flags.Length; i++)
        {
            if (flags[i])
            {
                count++;
            }
        }

        return count;
    }

    #endregion
}
