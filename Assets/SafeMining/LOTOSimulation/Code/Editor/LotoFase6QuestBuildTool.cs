#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class LotoFase6QuestBuildTool
{
    private const string ApkPath = "Builds/SafeMiningVR-Quest3S-Dev.apk";
    private const string ExpectedBundleId = "com.safeminingvr.sim";

    private static readonly string[] ScenePaths =
    {
        "Assets/XR Standard Menu/Level/Scenes/StandardMainMenu.unity",
        "Assets/SafeMining/LOTOSimulation/Scenes/VR-LOTO-S1.unity",
        "Assets/SafeMining/LOTOSimulation/Scenes/VR-LOTO-S2.unity",
        "Assets/SafeMining/LOTOSimulation/Scenes/VR-LOTO-S3.unity"
    };

    #region Public API

    [MenuItem("SafeMining/LOTO/Build Quest 3S Dev APK")]
    public static void BuildQuestDevApkMenu()
    {
        BuildQuestDevApk();
    }

    public static void BuildQuestDevApk()
    {
        if (!ValidateQuestBuildSettings())
        {
            Debug.LogError("[LOTO Fase6] Pre-build validation FAIL — aborting APK build.");
            return;
        }

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

    public static bool ValidateQuestBuildSettings()
    {
        bool pass = true;

        if (PlayerSettings.applicationIdentifier != ExpectedBundleId)
        {
            Debug.LogError("[LOTO Fase6] bundleId expected " + ExpectedBundleId + " got " + PlayerSettings.applicationIdentifier);
            pass = false;
        }

        if (PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) != ScriptingImplementation.IL2CPP)
        {
            Debug.LogError("[LOTO Fase6] Android scripting backend must be IL2CPP.");
            pass = false;
        }

        if (PlayerSettings.Android.targetArchitectures != AndroidArchitecture.ARM64)
        {
            Debug.LogError("[LOTO Fase6] Android architecture must be ARM64.");
            pass = false;
        }

        EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;
        if (buildScenes == null || buildScenes.Length != ScenePaths.Length)
        {
            Debug.LogError("[LOTO Fase6] EditorBuildSettings scene count must be " + ScenePaths.Length);
            pass = false;
        }
        else
        {
            for (int i = 0; i < ScenePaths.Length; i++)
            {
                if (!buildScenes[i].enabled || buildScenes[i].path != ScenePaths[i])
                {
                    Debug.LogError("[LOTO Fase6] Build scene mismatch index " + i + " expected " + ScenePaths[i]);
                    pass = false;
                }
            }
        }

        if (pass)
        {
            Debug.Log("[LOTO Fase6] Pre-build validation PASS.");
        }

        return pass;
    }

    #endregion
}
#endif
