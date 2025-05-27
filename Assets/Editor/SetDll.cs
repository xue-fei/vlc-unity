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
        buildTarget = BuildTarget.Android;
        suffix = ".so";
        string[] files = Directory.GetFiles(dllPath, "*", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            if (!file.EndsWith(suffix)) continue;
            string strTempPath = file.Replace(@"\", "/");
            //Debug.Log("文件路径：" + strTempPath);
            string[] strTemp = strTempPath.Split("Assets");
            string assetPath = "Assets/" + strTemp[1];
            PluginImporter plugin = AssetImporter.GetAtPath(assetPath) as PluginImporter;
            plugin.SetCompatibleWithEditor(false);
            plugin.SetCompatibleWithAnyPlatform(false);
            plugin.SetCompatibleWithPlatform(BuildTarget.Android, false);
            plugin.SetCompatibleWithPlatform(BuildTarget.iOS, false);
            plugin.SetCompatibleWithPlatform(BuildTarget.WebGL, false);
            plugin.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows, false);
            plugin.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows64, false);
            plugin.SetCompatibleWithPlatform(BuildTarget.StandaloneLinux64, false);
            plugin.SetCompatibleWithPlatform(BuildTarget.StandaloneOSX, false);

            if (buildTarget == BuildTarget.StandaloneWindows)
            {
                plugin.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows, true);
                plugin.SetPlatformData(BuildTarget.StandaloneWindows, "CPU", "x86");
            }
            if (buildTarget == BuildTarget.StandaloneWindows64)
            {
                plugin.SetCompatibleWithEditor(true);
                plugin.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows64, true);
                plugin.SetPlatformData(BuildTarget.StandaloneWindows64, "CPU", "x86_64");
            }
            if (buildTarget == BuildTarget.StandaloneLinux64)
            {
                plugin.SetCompatibleWithEditor(true);
                plugin.SetCompatibleWithPlatform(BuildTarget.StandaloneLinux64, true);
                plugin.SetPlatformData(BuildTarget.StandaloneLinux64, "CPU", "x86_64");
            }
            if (buildTarget == BuildTarget.Android)
            {
                plugin.SetCompatibleWithPlatform(BuildTarget.Android, true);
                if (assetPath.Contains("arm64-v8a"))
                {
                    plugin.SetPlatformData(BuildTarget.Android, "CPU", "ARM64");
                }
                if (assetPath.Contains("armeabi-v7a"))
                {
                    plugin.SetPlatformData(BuildTarget.Android, "CPU", "ARMv7");
                }
            }
            plugin.SaveAndReimport();
        }
        AssetDatabase.Refresh();
    }
}