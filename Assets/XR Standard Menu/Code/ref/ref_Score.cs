using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
public class ref_Score : MonoBehaviour
{
    public Image detailScreenBackground, detailScreenBackground2;
    public TMP_Text detailScreenTitle, detailScreenDescription, time_detail;
    [Range(0, 100)] public int approbalThreshold = 70;
    public Color approbedColor = Color.green;
    public Color failColor = Color.red;
    public Transform sceneScoreContainer;
    public GameObject sceneScorePrefab;

    public Transform objectivesContainer;
    public GameObject objectivePrefab;

    public Transform detailScreen;
    public List<GameObject> stars = new List<GameObject>();
    private readonly List<GameObject> objectiveInstances = new List<GameObject>();

    #region Public API

    public void SetDetailScreen(SceneScore sceneScore)
    {
        if (detailScreen != null)
        {
            detailScreen.gameObject.SetActive(true);
        }
        ClearObjectiveInstances();
        if (sceneScore == null)
        {
            SetStars(0, 0);
            return;
        }
        int achievedCount = 0;
        int objectiveCount = sceneScore.objectives != null ? sceneScore.objectives.Count : 0;
        if (time_detail != null)
        {
            time_detail.text = sceneScore.time;
        }
        for (int i = 0; i < objectiveCount; i++)
        {
            ObjectiveScore objectiveScore = sceneScore.objectives[i];
            if (objectiveScore.isAchivied)
            {
                achievedCount++;
            }
            if (objectivePrefab == null || objectivesContainer == null)
            {
                continue;
            }
            GameObject objectiveObject = Instantiate(objectivePrefab, objectivesContainer);
            objectiveInstances.Add(objectiveObject);
            ref_Objective objectiveRef = objectiveObject.GetComponent<ref_Objective>();
            if (objectiveRef != null && objectiveRef.title != null)
            {
                objectiveRef.title.text = GetTranslatedText(objectiveScore.id);
            }
            if (objectiveRef != null && objectiveRef.icon != null)
            {
                objectiveRef.icon.enabled = true;
                objectiveRef.icon.color = objectiveScore.isAchivied ? approbedColor : failColor;
                if (SBD_Images.Instance != null)
                {
                    objectiveRef.icon.sprite = objectiveScore.isAchivied ? SBD_Images.Instance.UI_objective_achivied : SBD_Images.Instance.UI_objective_failed;
                }
            }
        }
        int calculatedApprobalRate = objectiveCount > 0 ? Mathf.Clamp(Mathf.RoundToInt(((float)achievedCount / objectiveCount) * 100f), 0, 100) : 0;
        int approbalRate = sceneScore.approbalRate > 0 ? sceneScore.approbalRate : calculatedApprobalRate;
        bool isApprobed = approbalRate >= Mathf.Clamp(approbalThreshold, 0, 100);
        SetApprobalVisuals(isApprobed);
        SetStars(achievedCount, objectiveCount);
    }

    #endregion

    #region Internal

    private void ClearObjectiveInstances()
    {
        for (int i = 0; i < objectiveInstances.Count; i++)
        {
            if (objectiveInstances[i] != null)
            {
                Destroy(objectiveInstances[i]);
            }
        }
        objectiveInstances.Clear();
    }

    private void SetStars(int achievedCount, int totalObjectives)
    {
        int starsToEnable = 0;
        if (totalObjectives > 0 && stars.Count > 0)
        {
            float ratio = (float)achievedCount / totalObjectives;
            starsToEnable = Mathf.RoundToInt(ratio * stars.Count);
        }
        for (int i = 0; i < stars.Count; i++)
        {
            if (stars[i] != null)
            {
                stars[i].SetActive(i < starsToEnable);
            }
        }
    }

    private void SetApprobalVisuals(bool isApprobed)
    {
        Color targetColor = isApprobed ? approbedColor : failColor;
        if (detailScreenBackground != null)
        {
            detailScreenBackground.color = targetColor;
        }
        if (detailScreenBackground2 != null)
        {
            detailScreenBackground2.color = targetColor;
        }
        if (detailScreenTitle != null)
        {
            detailScreenTitle.text = GetTranslatedText(isApprobed ? "SM_detailScreen_title_approbed" : "SM_detailScreen_title_fail");
        }
        if (detailScreenDescription != null)
        {
            detailScreenDescription.text = GetTranslatedText(isApprobed ? "SM_detailScreen_descript_approbed" : "SM_detailScreen_descript_fail");
        }
    }

    private string GetTranslatedText(string translationId)
    {
        if (ControlCenter.Instance == null || ControlCenter.Instance.languageManager == null || string.IsNullOrEmpty(translationId))
        {
            return translationId;
        }
        string translated = ControlCenter.Instance.languageManager.GetText(translationId);
        return string.IsNullOrEmpty(translated) ? translationId : translated;
    }

    #endregion
}
