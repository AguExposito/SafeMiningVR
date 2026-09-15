using System;
using System.Collections.Generic;
using UnityEngine;

public static class LotoSessionScore
{
    public const string CompletionCompleted = "Completed";
    public const string CompletionAbandoned = "Abandoned";
    public const int RequiredHazardCount = 4;

    public static int ApprovalThresholdPercent => LotoObjectiveIds.DefaultApprovalThresholdPercent;

    public static SimulationResultPayload Build(
        SceneScore sceneScore,
        int falsePositiveCount,
        float elapsedSeconds,
        DateTime endDate,
        string completionStatus)
    {
        SimulationResultPayload payload = new SimulationResultPayload
        {
            durationSeconds = Mathf.Max(0, Mathf.RoundToInt(elapsedSeconds)),
            finishedAtUtc = endDate.ToUniversalTime().ToString("o"),
            completionStatus = string.IsNullOrEmpty(completionStatus) ? CompletionCompleted : completionStatus
        };

        if (sceneScore == null || sceneScore.objectives == null)
        {
            payload.objectives = Array.Empty<SimulationObjectiveResult>();
            payload.scoreTotal = 0;
            payload.approved = false;
            return payload;
        }

        List<SimulationObjectiveResult> objectiveResults = new List<SimulationObjectiveResult>();
        float weightedScore = 0f;
        for (int i = 0; i < sceneScore.objectives.Count; i++)
        {
            ObjectiveScore objective = sceneScore.objectives[i];
            if (objective == null)
            {
                continue;
            }

            int weight = LotoObjectiveIds.GetWeightPercent(objective.id);
            float partial = ComputePartial(objective.id, objective.isAchivied, weight, falsePositiveCount);
            weightedScore += partial;
            objectiveResults.Add(new SimulationObjectiveResult
            {
                objectiveId = objective.id,
                achieved = objective.isAchivied,
                weightPercent = weight,
                scorePartial = partial
            });
        }

        payload.objectives = objectiveResults.ToArray();
        payload.scoreTotal = Mathf.Clamp(Mathf.RoundToInt(weightedScore), 0, 100);
        payload.approved = payload.scoreTotal >= ApprovalThresholdPercent;
        return payload;
    }

    private static float ComputePartial(string objectiveId, bool achieved, int weight, int falsePositiveCount)
    {
        LotoGameManager board = LotoGameManager.Instance;

        if (objectiveId == LotoObjectiveIds.HazardReport)
        {
            float ratio = 0f;
            if (board != null)
            {
                ratio = board.S2ScoredReportCount / (float)LotoGameManager.S2RequiredTotal;
            }
            else if (achieved)
            {
                ratio = 1f;
            }

            float penaltyRatio = Mathf.Max(0f, 1f - (falsePositiveCount / (float)RequiredHazardCount));
            return Mathf.Round(weight * Mathf.Clamp01(ratio) * penaltyRatio);
        }

        if (objectiveId == LotoObjectiveIds.LotoSequence)
        {
            if (board != null)
            {
                return Mathf.Round(weight * (board.S3ScoredStepCount / (float)LotoGameManager.S3RequiredSteps));
            }

            return achieved ? weight : 0f;
        }

        if (!achieved)
        {
            return 0f;
        }

        return weight;
    }
}
