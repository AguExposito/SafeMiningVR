#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class LotoFullSetupTool
{
    [MenuItem("SafeMining/LOTO/Apply Full LOTO Setup (Fases 2-5)")]
    public static void ApplyFullSetup()
    {
        LotoFase2PrefabSetupTool.ApplyFase2Setup();
        LotoFase3PrefabSetupTool.ApplyFase3Setup();
        LotoFase5LightingCleanupTool.ApplyCanonicalLighting();
        Debug.Log("[LOTO] Full setup complete (Fases 2-5). Run Prepare Fase 4 Menu Flow Test for menu QA.");
    }
}
#endif
