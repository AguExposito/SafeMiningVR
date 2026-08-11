using UnityEditor;
using UnityEngine;
using UnityEditor.SettingsManagement;

[CustomEditor(typeof(IBManager))]
public class IBManagerEditor : Editor
{
    private SerializedProperty connectionModeProp;
    private SerializedProperty simulatorTypeProp;
    private SerializedProperty urlTypeProp;
    private SerializedProperty sessionStartModeProp;
    private SerializedProperty sendResultsProp;
    private SerializedProperty verboseLogsProp;
    private SerializedProperty customUrlProp;
    private SerializedProperty simulatorIdProp;
    private SerializedProperty packagePassArgProp;
    private SerializedProperty packagePartOpenSceneViewerAfterLoginProp;
    private SerializedProperty packagePartSceneStartIndexProp;
    private SerializedProperty packagePartTrustHubSessionArgsProp;

    private SerializedProperty autoUpdateManifestProp;
    private SerializedProperty hubPackageNameProp;
    private SerializedProperty hubDependentPackagesProp;
    private SerializedProperty hubSimulatorsProp;

    private GUIStyle sectionStyle;
    private GUIStyle titleStyle;

    private void OnEnable()
    {
        connectionModeProp = serializedObject.FindProperty("connectionMode");
        simulatorTypeProp = serializedObject.FindProperty("simulatorType");
        urlTypeProp = serializedObject.FindProperty("urlType");
        sessionStartModeProp = serializedObject.FindProperty("sessionStartMode");
        sendResultsProp = serializedObject.FindProperty("sendResultsToCloud");
        verboseLogsProp = serializedObject.FindProperty("verboseLoginLogs");
        customUrlProp = serializedObject.FindProperty("customUrl");
        simulatorIdProp = serializedObject.FindProperty("simulatorID");
        packagePassArgProp = serializedObject.FindProperty("packagePassArg");
        packagePartOpenSceneViewerAfterLoginProp = serializedObject.FindProperty("packagePartOpenSceneViewerAfterLogin");
        packagePartSceneStartIndexProp = serializedObject.FindProperty("packagePartSceneStartIndex");
        packagePartTrustHubSessionArgsProp = serializedObject.FindProperty("packagePartTrustHubSessionArgs");

        autoUpdateManifestProp = serializedObject.FindProperty("autoUpdateAndroidManifest");
        hubPackageNameProp = serializedObject.FindProperty("hubPackageName");
        hubDependentPackagesProp = serializedObject.FindProperty("hubDependentPackages");
        hubSimulatorsProp = serializedObject.FindProperty("hubSimulators");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EnsureStyles();

        DrawSection("Conexion y Simulador", () =>
        {
            EditorGUILayout.PropertyField(connectionModeProp);
            EditorGUILayout.PropertyField(simulatorTypeProp);
            EditorGUILayout.PropertyField(urlTypeProp);
            if ((IBManager.UrlType)urlTypeProp.enumValueIndex == IBManager.UrlType.Custom)
            {
                EditorGUILayout.PropertyField(customUrlProp, new GUIContent("Custom URL"));
            }
            EditorGUILayout.PropertyField(simulatorIdProp, new GUIContent("Simulator ID"));
            if ((IBManager.SimulatorType)simulatorTypeProp.enumValueIndex == IBManager.SimulatorType.PackagePart)
            {
                EditorGUILayout.PropertyField(packagePassArgProp, new GUIContent("Package Pass Arg"));
                EditorGUILayout.PropertyField(packagePartTrustHubSessionArgsProp, new GUIContent("Trust Hub Session Args"));
                EditorGUILayout.PropertyField(packagePartSceneStartIndexProp, new GUIContent("PackagePart Scene Start Index"));
                EditorGUILayout.PropertyField(packagePartOpenSceneViewerAfterLoginProp, new GUIContent("Open SceneViewer After Login"));
            }
        });

        DrawSection("Sesion y Reportes", () =>
        {
            EditorGUILayout.PropertyField(sessionStartModeProp);
            EditorGUILayout.PropertyField(sendResultsProp, new GUIContent("Send Results To Cloud"));
            EditorGUILayout.PropertyField(verboseLogsProp, new GUIContent("Verbose Login Logs"));

            IBManager.SessionStartMode mode = (IBManager.SessionStartMode)sessionStartModeProp.enumValueIndex;
            string modeInfo = mode switch
            {
                IBManager.SessionStartMode.AlwaysLogin => "AlwaysLogin: limpia credenciales al iniciar y obliga login.",
                IBManager.SessionStartMode.LoadActivationCodeOnly => "LoadActivationCodeOnly: conserva activacion, solicita PIN.",
                _ => "AutoStartSession: intenta autologin si ya hay activacion + PIN persistidos."
            };
            EditorGUILayout.HelpBox(modeInfo, MessageType.None);
        });

        DrawSection("Android Manifest Preset", () =>
        {
            EditorGUILayout.PropertyField(autoUpdateManifestProp, new GUIContent("Auto Update Manifest"));
            IBManager manager = (IBManager)target;
            string androidPackageName = GetAndroidPackageName();
            EditorGUILayout.HelpBox(
                $"Android Package actual: {androidPackageName}\n" +
                $"SimulatorType: {manager.ManifestSimulatorType}",
                MessageType.Info
            );

            if (manager.ManifestSimulatorType == IBManager.SimulatorType.PackagePart)
            {
                EditorGUILayout.PropertyField(hubPackageNameProp, new GUIContent("Target Hub Package"));
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.TextField("Current App Package (Player)", androidPackageName);
                }
            }
            else if (manager.ManifestSimulatorType == IBManager.SimulatorType.Hub)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.TextField("Hub Package (Player)", androidPackageName);
                }
                EditorGUILayout.PropertyField(hubDependentPackagesProp, new GUIContent("Hub Dependent Packages"), true);
                EditorGUILayout.Space(4f);
                EditorGUILayout.PropertyField(hubSimulatorsProp, new GUIContent("Hub Simulator Launch Data"), true);
            }

            DrawPresetValidation(manager);

            EditorGUILayout.Space(4f);
            using (new EditorGUI.DisabledScope(Application.isPlaying))
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Apply Manifest Now"))
                {
                    AndroidManifestPresetUpdater.ApplyFrom(manager);
                }
                if (GUILayout.Button("Open AndroidManifest"))
                {
                    Object manifestAsset = AssetDatabase.LoadAssetAtPath<Object>("Assets/Plugins/Android/AndroidManifest.xml");
                    if (manifestAsset != null)
                    {
                        Selection.activeObject = manifestAsset;
                        EditorGUIUtility.PingObject(manifestAsset);
                    }
                }
                if (GUILayout.Button("Open Player Settings"))
                {
                    SettingsService.OpenProjectSettings("Project/Player");
                }
                EditorGUILayout.EndHorizontal();
            }
        });

        serializedObject.ApplyModifiedProperties();
    }

    private static void DrawPresetValidation(IBManager manager)
    {
        if (manager.ManifestSimulatorType == IBManager.SimulatorType.PackagePart && string.IsNullOrWhiteSpace(manager.HubPackageName))
        {
            EditorGUILayout.HelpBox(
                "PackagePart requiere hubPackageName para crear <queries> del HUB.",
                MessageType.Warning
            );
        }

        if (manager.ManifestSimulatorType == IBManager.SimulatorType.Hub &&
            (manager.HubDependentPackages == null || manager.HubDependentPackages.Count == 0))
        {
            EditorGUILayout.HelpBox(
                "Hub no tiene paquetes dependientes configurados. El preset resultara sin <queries>.",
                MessageType.Warning
            );
        }
    }

    private static string GetAndroidPackageName()
    {
#if UNITY_2021_2_OR_NEWER
        return PlayerSettings.GetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android);
#else
        return PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);
#endif
    }

    private void EnsureStyles()
    {
        if (sectionStyle == null)
        {
            sectionStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(10, 10, 8, 8)
            };
        }
        if (titleStyle == null)
        {
            titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12
            };
        }
    }

    private void DrawSection(string title, System.Action drawBody)
    {
        EditorGUILayout.BeginVertical(sectionStyle);
        EditorGUILayout.LabelField(title, titleStyle);
        EditorGUILayout.Space(2f);
        drawBody?.Invoke();
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(6f);
    }
}
