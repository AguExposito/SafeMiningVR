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
    public string loginUsername;
    public SimulationObjectiveResult[] objectives;
    public int scoreTotal;
    public bool approved;
    public int durationSeconds;
    public string finishedAtUtc;
    public string completionStatus;
    public string vrResultJson;
    public string catalogSimulationId;
}
