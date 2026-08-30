using UnityEditor;
using UnityEngine;

public static class LotoFase4MenuFlowTestTool
{
    [MenuItem("SafeMining/LOTO/Run Fase 4 Menu Flow Test")]
    public static void RunMenuFlowTest()
    {
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog(
                "LOTO Fase 4",
                "Enter Play Mode on StandardMainMenu first, then run this command again.",
                "OK");
            return;
        }

        LotoFase4MenuFlowRunner runner = EnsureRunner();
        runner.RunFullMenuFlow();
    }

    [MenuItem("SafeMining/LOTO/Run Fase 4 Negative Gate Test")]
    public static void RunNegativeGateTest()
    {
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog(
                "LOTO Fase 4",
                "Enter Play Mode on StandardMainMenu first, then run this command again.",
                "OK");
            return;
        }

        LotoFase4MenuFlowRunner runner = EnsureRunner();
        runner.RunNegativeGateChecks();
    }

    [MenuItem("SafeMining/LOTO/Run Fase 4 Malo 1 Test")]
    public static void RunMalo1Test()
    {
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog("LOTO Fase 4", "Enter Play Mode on S3 first.", "OK");
            return;
        }

        EnsureRunner().RunMalo1IncompleteStart();
    }

    [MenuItem("SafeMining/LOTO/Run Fase 4 Malo 2 Test")]
    public static void RunMalo2Test()
    {
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog("LOTO Fase 4", "Enter Play Mode on S3 first.", "OK");
            return;
        }

        EnsureRunner().RunMalo2NoGloves();
    }

    [MenuItem("SafeMining/LOTO/Prepare Fase 4 Menu Flow Test")]
    public static void PrepareMenuFlowTest()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("LOTO Fase 4", "Stop Play Mode before preparing the test scene.", "OK");
            return;
        }

        UnityEditor.SceneManagement.EditorSceneManager.OpenScene(
            "Assets/XR Standard Menu/Level/Scenes/StandardMainMenu.unity");
        Debug.Log("[LOTO Fase4] StandardMainMenu loaded. Press Play, then run Menu Flow Test.");
    }

    private static LotoFase4MenuFlowRunner EnsureRunner()
    {
        LotoFase4MenuFlowRunner existing = Object.FindFirstObjectByType<LotoFase4MenuFlowRunner>();
        if (existing != null)
        {
            return existing;
        }

        GameObject host = new GameObject("LotoFase4MenuFlowRunner");
        Object.DontDestroyOnLoad(host);
        return host.AddComponent<LotoFase4MenuFlowRunner>();
    }
}
