using System;

[Serializable]
public class SimulationSessionContext
{
    public string sessionId;
    public string intentoId;
    public string programId;
    public string capacitacionId;
    public string participantId;
    public string loginUsername;
    public string startedAtUtc;
    public string simulationVersion;
    public bool isActive;

    public bool hasHelmet;
    public bool hasSafetyGlasses;
    public bool hasReflectiveVest;
    public bool hasGloves;
    public bool hasSafetyBoots;
    public bool hasRespirator;
    public bool hasHearingProtection;
    public int falsePositiveCount;
}
