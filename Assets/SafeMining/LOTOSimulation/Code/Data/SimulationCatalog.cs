using System;

[Serializable]
public class SimulationCatalogFile
{
    public int version;
    public SimulationCatalogEntry[] simulations;
}

[Serializable]
public class SimulationCatalogEntry
{
    public string id;
    public string name;
    public string platform;
    public bool implemented;
    public int approvalThreshold;
    public SimulationCatalogPhase[] phases;
    public SimulationCatalogObjective[] objectives;
}

[Serializable]
public class SimulationCatalogPhase
{
    public string id;
    public string apiKey;
    public float weight;
    public int threshold;
}

[Serializable]
public class SimulationCatalogObjective
{
    public string id;
    public int weightPercent;
    public string phaseId;
}
