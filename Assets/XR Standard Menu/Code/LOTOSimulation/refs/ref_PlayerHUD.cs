using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ref_PlayerHUD : MonoBehaviour
{
    public GameObject rootPanel;
    public TMP_Text activeObjectiveTitle;
    public Transform objectivesContainer;
    public GameObject objectiveRowPrefab;
    public TMP_Text timerText;
    public Image timerFill;
    public TMP_Text scenarioIndicatorText;
    public TMP_Text subtitleText;
    public GameObject menuButtonRoot;
    public GameObject exitConfirmPanel;
    public TMP_Text exitConfirmTitleText;
    public TMP_Text exitConfirmBodyText;
    public Button exitConfirmYes;
    public Button exitConfirmNo;
    public Button menuButton;
    public GameObject sessionSummaryPanel;
    public TMP_Text sessionSummaryTitleText;
    public TMP_Text sessionSummaryBodyText;
    public Button sessionSummaryContinueButton;
}
