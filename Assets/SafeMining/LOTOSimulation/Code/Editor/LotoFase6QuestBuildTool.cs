#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class LotoFase6QuestBuildTool
{
    private const string ApkPath = "Builds/SafeMiningVR-Quest3S-Dev.apk";

    private static readonly string[] ScenePaths =
    {
        "Assets/XR Standard Menu/Level/Scenes/StandardMainMenu.unity",
        "Assets/SafeMining/LOTOSimulation/Scenes/VR-LOTO-S1.unity",
        "Assets/SafeMining/LOTOSimulation/Scenes/VR-LOTO-S2.unity"
    };

    #region Public API

    [MenuItem("SafeMining/LOTO/Build Quest 3S Dev APK")]
    public static void BuildQuestDevApkMenu()
    {
        BuildQuestDevApk();
    }

    public static void BuildQuestDevApk()
    {
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        }

        EditorUserBuildSettings.buildAppBundle = false;
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;

        string outputPath = Path.Combine(Application.dataPath, "..", ApkPath);
        string outputDirectory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = ScenePaths,
            locationPathName = outputPath,
            target = BuildTarget.Android,
            options = BuildOptions.Development | BuildOptions.CompressWithLz4
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;
        if (summary.result != BuildResult.Succeeded)
        {
            Debug.LogError("[LOTO Fase6] Quest APK build failed: " + summary.result);
            return;
        }

        long sizeBytes = new FileInfo(outputPath).Length;
        Debug.Log("[LOTO Fase6] Quest APK built: " + ApkPath + " sizeBytes=" + sizeBytes);
    }

    #endregion
}
#endif
