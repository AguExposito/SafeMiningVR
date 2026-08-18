public enum HazardSpotKind
{
    RealHazard,
    Decoy
}

public enum HazardType
{
    SpilledMaterial,
    AbandonedTools,
    FallenSignage,
    ObstructedExtinguisher,
    DecoyCleanFloor,
    DecoyStoredTools,
    DecoyVisibleSign
}

public static class HazardTypeRules
{
    public static bool IsRequiredHazard(HazardType type)
    {
        return type == HazardType.SpilledMaterial ||
               type == HazardType.AbandonedTools ||
               type == HazardType.FallenSignage ||
               type == HazardType.ObstructedExtinguisher;
    }

    public static bool IsDecoy(HazardType type)
    {
        return !IsRequiredHazard(type);
    }
}
