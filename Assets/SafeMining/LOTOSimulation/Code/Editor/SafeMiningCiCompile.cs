using UnityEditor;

public static class SafeMiningCiCompile
{
    public static void ValidateProjectCompiles()
    {
        EditorApplication.update += WaitForCompilation;
    }

    private static void WaitForCompilation()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            return;
        }

        EditorApplication.update -= WaitForCompilation;
        int exitCode = EditorUtility.scriptCompilationFailed ? 1 : 0;
        EditorApplication.Exit(exitCode);
    }
}
