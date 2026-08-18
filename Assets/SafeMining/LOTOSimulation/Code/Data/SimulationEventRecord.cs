using System;

[Serializable]
public class SimulationEventRecord
{
    public string sessionId;
    public string eventType;
    public string objectiveId;
    public string optionId;
    public string timestampUtc;
    public bool success;
    public string metadata;
}
