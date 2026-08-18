using System;

[Serializable]
public class SimulationObjectiveResult
{
    public string objectiveId;
    public bool achieved;
    public int weightPercent;
    public float scorePartial;
}

[Serializable]
public class SimulationResultPayload
{
    public string sessionId;
    public SimulationObjectiveResult[] objectives;
    public int scoreTotal;
    public bool approved;
    public int durationSeconds;
    public string finishedAtUtc;
    public string completionStatus;
}
