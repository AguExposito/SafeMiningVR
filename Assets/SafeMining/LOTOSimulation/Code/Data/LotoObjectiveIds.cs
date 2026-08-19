public static class LotoObjectiveIds
{
    public const string EppSelection = "loto_obj_epp_selection";
    public const string EppDefect = "loto_obj_epp_defect";
    public const string HazardReport = "loto_obj_hazard_report";
    public const string LotoSequence = "loto_obj_loto_sequence";
    public const string ZeroEnergy = "loto_obj_zero_energy";

    public const string SimulationSceneId = "loto_simulation";

    public const int DefaultApprovalThresholdPercent = 60;

    public static readonly string[] AllObjectiveIds =
    {
        EppSelection,
        EppDefect,
        HazardReport,
        LotoSequence,
        ZeroEnergy
    };

    public static int GetWeightPercent(string objectiveId)
    {
        switch (objectiveId)
        {
            case EppSelection: return 20;
            case EppDefect: return 10;
            case HazardReport: return 25;
            case LotoSequence: return 35;
            case ZeroEnergy: return 10;
            default: return 0;
        }
    }
}
