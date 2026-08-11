using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEditor;
using UnityEngine;

public static class AndroidManifestPresetUpdater
{
    private const string ManifestRelativePath = "Assets/Plugins/Android/AndroidManifest.xml";
    private const string AndroidNamespaceUri = "http://schemas.android.com/apk/res/android";

    public static void RequestApply(IBManager manager)
    {
        if (manager == null || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }
        EditorApplication.delayCall += () => ApplyFrom(manager);
    }

    [MenuItem("Tools/XR Standard Menu/Apply Android Manifest Preset")]
    public static void ApplyFromSceneIbManager()
    {
        IBManager manager = UnityEngine.Object.FindAnyObjectByType<IBManager>();
        if (manager == null)
        {
            Debug.LogWarning("[AndroidManifestPresetUpdater] No se encontro IBManager en la escena para aplicar el preset.");
            return;
        }
        ApplyFrom(manager);
    }

    public static void ApplyFrom(IBManager manager)
    {
        if (manager == null || !manager.AutoUpdateAndroidManifest)
        {
            return;
        }

        string manifestPath = Path.Combine(Directory.GetCurrentDirectory(), ManifestRelativePath);
        if (!File.Exists(manifestPath))
        {
            Debug.LogWarning($"[AndroidManifestPresetUpdater] No existe el manifest en: {ManifestRelativePath}");
            return;
        }

        XmlDocument document = new XmlDocument
        {
            PreserveWhitespace = true
        };
        try
        {
            document.Load(manifestPath);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[AndroidManifestPresetUpdater] No se pudo leer AndroidManifest.xml. {ex.Message}");
            return;
        }

        XmlElement manifest = document.DocumentElement;
        if (manifest == null || manifest.Name != "manifest")
        {
            Debug.LogError("[AndroidManifestPresetUpdater] El archivo no contiene un nodo <manifest> valido.");
            return;
        }

        string safetySnapshotBefore = BuildSafetySnapshot(document);
        string currentPackageName = GetCurrentAndroidPackageName();
        List<string> queryPackages = BuildQueryPackageList(manager, currentPackageName);

        // Backup de seguridad antes de modificar.
        string backupPath = manifestPath + ".bak";
        try
        {
            File.Copy(manifestPath, backupPath, true);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[AndroidManifestPresetUpdater] No se pudo crear backup del manifest. {ex.Message}");
            return;
        }

        RemoveQueriesNodes(manifest);
        if (queryPackages.Count > 0)
        {
            XmlElement queries = document.CreateElement("queries");
            for (int i = 0; i < queryPackages.Count; i++)
            {
                XmlElement packageNode = document.CreateElement("package");
                packageNode.SetAttribute("name", AndroidNamespaceUri, queryPackages[i]);
                queries.AppendChild(packageNode);
            }
            XmlNode applicationNode = FindDirectChild(manifest, "application");
            if (applicationNode != null)
            {
                manifest.InsertBefore(queries, applicationNode);
            }
            else
            {
                manifest.AppendChild(queries);
            }
        }

        string safetySnapshotAfter = BuildSafetySnapshot(document);
        if (!string.Equals(safetySnapshotBefore, safetySnapshotAfter, StringComparison.Ordinal))
        {
            Debug.LogError("[AndroidManifestPresetUpdater] Se detecto un cambio fuera de <queries>. Se restaura backup para proteger el manifest.");
            try
            {
                File.Copy(backupPath, manifestPath, true);
                AssetDatabase.Refresh();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AndroidManifestPresetUpdater] No se pudo restaurar backup. {ex.Message}");
            }
            return;
        }

        XmlWriterSettings settings = new XmlWriterSettings
        {
            Encoding = new UTF8Encoding(false),
            Indent = false,
            NewLineHandling = NewLineHandling.None,
            OmitXmlDeclaration = false
        };
        using (XmlWriter writer = XmlWriter.Create(manifestPath, settings))
        {
            document.Save(writer);
        }

        AssetDatabase.Refresh();
        Debug.Log($"[AndroidManifestPresetUpdater] Preset aplicado (solo <queries>): {manager.ManifestSimulatorType}. Packages: {queryPackages.Count}. Backup: {Path.GetFileName(backupPath)}");
    }

    private static List<string> BuildQueryPackageList(IBManager manager, string currentPackageName)
    {
        List<string> packages = new List<string>();
        switch (manager.ManifestSimulatorType)
        {
            case IBManager.SimulatorType.Hub:
                if (manager.HubDependentPackages != null)
                {
                    for (int i = 0; i < manager.HubDependentPackages.Count; i++)
                    {
                        packages.Add(manager.HubDependentPackages[i]);
                    }
                }
                break;
            case IBManager.SimulatorType.PackagePart:
                packages.Add(manager.HubPackageName);
                break;
            case IBManager.SimulatorType.Singular:
                break;
        }

        return packages
            .Where(packageName => !string.IsNullOrWhiteSpace(packageName))
            .Select(packageName => packageName.Trim())
            .Where(packageName => !string.Equals(packageName, currentPackageName, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string GetCurrentAndroidPackageName()
    {
#if UNITY_2021_2_OR_NEWER
        return PlayerSettings.GetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android);
#else
        return PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);
#endif
    }

    private static void RemoveQueriesNodes(XmlElement manifest)
    {
        List<XmlNode> toRemove = new List<XmlNode>();
        for (int i = 0; i < manifest.ChildNodes.Count; i++)
        {
            XmlNode child = manifest.ChildNodes[i];
            if (child.NodeType == XmlNodeType.Element && child.Name == "queries")
            {
                toRemove.Add(child);
            }
        }
        for (int i = 0; i < toRemove.Count; i++)
        {
            manifest.RemoveChild(toRemove[i]);
        }
    }

    private static XmlNode FindDirectChild(XmlElement parent, string name)
    {
        for (int i = 0; i < parent.ChildNodes.Count; i++)
        {
            XmlNode child = parent.ChildNodes[i];
            if (child.NodeType == XmlNodeType.Element && child.Name == name)
            {
                return child;
            }
        }
        return null;
    }

    // Snapshot de nodos criticos para asegurar que solo cambiamos <queries>.
    private static string BuildSafetySnapshot(XmlDocument document)
    {
        XmlElement manifest = document.DocumentElement;
        if (manifest == null)
        {
            return string.Empty;
        }

        StringBuilder builder = new StringBuilder();
        AppendNodeOuterXml(builder, FindDirectChild(manifest, "application"));

        for (int i = 0; i < manifest.ChildNodes.Count; i++)
        {
            XmlNode child = manifest.ChildNodes[i];
            if (child.NodeType != XmlNodeType.Element)
            {
                continue;
            }
            if (child.Name == "uses-permission" || child.Name == "uses-feature")
            {
                AppendNodeOuterXml(builder, child);
            }
        }
        return builder.ToString();
    }

    private static void AppendNodeOuterXml(StringBuilder builder, XmlNode node)
    {
        if (node == null)
        {
            return;
        }
        builder.Append(node.OuterXml);
    }
}
