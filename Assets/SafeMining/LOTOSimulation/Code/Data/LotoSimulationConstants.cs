using System;

public static class LotoSimulationConstants
{
    public const int MainMenuBuildIndex = 0;
    public const int Scenario1BuildIndex = 1;
    public const int DefaultLotoEntryBuildIndex = Scenario1BuildIndex;

    [Obsolete("Use DefaultLotoEntryBuildIndex. Loto hub scene was removed.")]
    public const int LotoHubBuildIndex = DefaultLotoEntryBuildIndex;
    public const int Scenario2BuildIndex = 2;
    public const int Scenario3BuildIndex = 3;
    public const int LotoSimScoreIndex = 0;
    public const int TotalScenarios = 3;
    public const float HudTimerRefreshInterval = 0.25f;

    public const int ScenarioEpp = 1;
    public const int ScenarioInspection = 2;
    public const int ScenarioLoto = 3;

    public const int ObjectiveEppSelectionIndex = 0;
    public const int ObjectiveEppDefectIndex = 1;
    public const int ObjectiveHazardReportIndex = 2;
    public const int ObjectiveLotoSequenceIndex = 3;
    public const int ObjectiveZeroEnergyIndex = 4;

    public const int XriGrabInteractionLayerMask = 1 << 1;
    public const int XriTeleportInteractionLayerMask = unchecked((int)(1u << 31));

    public const float HazardHintTimeoutSeconds = 90f;

    public const float DwellSecondsDefault = 1f;
    public const float DwellSecondsHazard = 1f;
    public const float DwellSecondsLoto = 1.1f;
}
