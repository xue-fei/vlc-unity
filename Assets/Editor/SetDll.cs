using System.IO;
using UnityEditor;
using UnityEngine;

public class SetDll : EditorWindow
{
    /// <summary>
    /// 文件后缀
    /// </summary>
    private string suffix = ".dll";

    [MenuItem("工具/Dll设置平台和架构")]
    static void Init()
    {
        SetDll window = (SetDll)EditorWindow.GetWindow(typeof(SetDll), false, "Dll设置平台和架构", true);
        window.Show();
    }

    void OnGUI()
    {
        EditorGUILayout.Space();
        if (GUILayout.Button("Dll设置平台和架构"))
        {
            //SetConfig(Application.dataPath + "/Plugins/Win/x86/", BuildTarget.StandaloneWindows);
            //SetConfig(Application.dataPath + "/Plugins/Win/x86_64/", BuildTarget.StandaloneWindows64);
            SetConfig(Application.dataPath + "/Plugins/Linux/x86_64/", BuildTarget.StandaloneLinux64);
            //SetConfig(Application.dataPath + "/Plugins/Android/arm64-v8a/", BuildTarget.Android);
            //SetConfig(Application.dataPath + "/Plugins/Android/armeabi-v7a/", BuildTarget.Android);
        }
    }

    private void SetConfig(string dllPath, BuildTarget buildTarget)
    {
        if (string.IsNullOrEmpty(dllPath))
        {
            return;
        }

        if (!Directory.Exists(dllPath))
        {
            Debug.LogError("找不到文件夹路径！");
            return;
        }
        if (buildTarget == BuildTarget.Android
            || buildTarget == BuildTarget.StandaloneLinux64)
        {
            suffix = ".so";
        }
        string[] files = Directory.GetFiles(dllPath, "*", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            if (!file.EndsWith(suffix)) continue;
            string strTempPath = file.Replace(@"\", "/");
            //Debug.Log("文件路径：" + strTempPath);
            string[] strTemp = strTempPath.Split("Assets");
            string assetPath = "Assets/" + strTemp[1];
            PluginImporter pluginImporter = AssetImporter.GetAtPath(assetPath) as PluginImporter;
            pluginImporter.SetCompatibleWithEditor(false);
            pluginImporter.SetCompatibleWithAnyPlatform(false);

            pluginImporter.SetCompatibleWithPlatform(BuildTarget.Android, false);
            pluginImporter.SetCompatibleWithPlatform(BuildTarget.iOS, false);
            pluginImporter.SetCompatibleWithPlatform(BuildTarget.WebGL, false);
            pluginImporter.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows, false);
            pluginImporter.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows64, false);
            pluginImporter.SetCompatibleWithPlatform(BuildTarget.StandaloneLinux64, false);
            pluginImporter.SetCompatibleWithPlatform(BuildTarget.StandaloneOSX, false);

            if (buildTarget == BuildTarget.StandaloneWindows)
            {
                pluginImporter.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows, true);
                pluginImporter.SetPlatformData(BuildTarget.StandaloneWindows, "CPU", "x86");
            }
            if (buildTarget == BuildTarget.StandaloneWindows64)
            {
                pluginImporter.SetCompatibleWithEditor(true);
                pluginImporter.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows64, true);
                pluginImporter.SetPlatformData(BuildTarget.StandaloneWindows64, "CPU", "x86_64");
            }
            if (buildTarget == BuildTarget.StandaloneLinux64)
            {
                pluginImporter.SetCompatibleWithEditor(true);
                pluginImporter.SetCompatibleWithPlatform(BuildTarget.StandaloneLinux64, true);
                pluginImporter.SetPlatformData(BuildTarget.StandaloneLinux64, "CPU", "x86_64");
                pluginImporter.SetPlatformData(BuildTarget.StandaloneLinux64, "OS", "Linux");
                pluginImporter.SetPlatformData(BuildTarget.StandaloneLinux64, "Standalone", "Linux64");
            }
            if (buildTarget == BuildTarget.Android)
            {
                pluginImporter.SetCompatibleWithPlatform(BuildTarget.Android, true);
                if (assetPath.Contains("arm64-v8a"))
                {
                    pluginImporter.SetPlatformData(BuildTarget.Android, "CPU", "ARM64");
                }
                if (assetPath.Contains("armeabi-v7a"))
                {
                    pluginImporter.SetPlatformData(BuildTarget.Android, "CPU", "ARMv7");
                }
            }
            EditorUtility.SetDirty(pluginImporter);
            pluginImporter.SaveAndReimport();
        }
        AssetDatabase.Refresh();
    }
}