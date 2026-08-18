public enum EppItemType
{
    Helmet,
    SafetyGlasses,
    ReflectiveVest,
    Gloves,
    SafetyBoots,
    Respirator,
    HearingProtection
}

public enum EppItemCondition
{
    Good,
    Defective
}

public static class EppTypeRules
{
    public static bool IsMandatory(EppItemType type)
    {
        return type == EppItemType.Helmet ||
               type == EppItemType.SafetyGlasses ||
               type == EppItemType.ReflectiveVest ||
               type == EppItemType.Gloves ||
               type == EppItemType.SafetyBoots;
    }
}
