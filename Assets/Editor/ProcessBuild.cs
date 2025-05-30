﻿using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class ProcessBuild
{
    [MenuItem("工具/打开沙盒文件夹")]
    static void OpenPersistentDataPath()
    {
        System.Diagnostics.Process.Start(@Application.persistentDataPath);
    }

    [PostProcessBuildAttribute(88)]
    public static void onPostProcessBuild(BuildTarget buildTarget, string path)
    {
        //|| buildTarget == BuildTarget.StandaloneWindows
        if (buildTarget == BuildTarget.StandaloneWindows64
            || buildTarget == BuildTarget.StandaloneLinux64)
        {

        }
        else
        {
            Debug.LogError("Target is not Windows PostProcessBuild will not run");
            return;
        }

        #region 拷贝vlc的plugins相关文件
        FileInfo fileInfo = new FileInfo(path);
        Debug.Log(fileInfo.FullName);
        Debug.Log(fileInfo.Name);
        Debug.Log(fileInfo.Extension);
        string dataDirStr = path.Replace(fileInfo.Extension, "") + "_Data/";
        //删除自动打包出的x86_64文件夹
        DirectoryInfo x86_64 = new DirectoryInfo(dataDirStr + "Plugins/x86_64");
        if (x86_64.Exists)
        {
            x86_64.Delete(true);
        }
        //删除自动打包出的x86文件夹
        DirectoryInfo x86 = new DirectoryInfo(dataDirStr + "Plugins/x86");
        if (x86.Exists)
        {
            x86.Delete(true);
        }
        string disDirStr = dataDirStr + "Plugins/";
        DirectoryInfo di = new DirectoryInfo(disDirStr);
        foreach (FileInfo fi in di.GetFiles())
        {
            fi.Delete();
        }
        if (buildTarget == BuildTarget.StandaloneWindows64)
        {
            Debug.LogWarning("Win_x86_64");
            Copy(Application.dataPath + "/Plugins/Win/x86_64/", disDirStr);
        }
        //if (buildTarget == BuildTarget.StandaloneWindows)
        //{
        //    Debug.LogWarning("Win_x86");
        //    Copy(Application.dataPath + "/Plugins/Win/x86/", disDirStr);
        //}
        if (buildTarget == BuildTarget.StandaloneLinux64)
        {
            Debug.LogWarning("Linux_x86_64");
            Copy(Application.dataPath + "/Plugins/Linux/x86_64/", disDirStr);
        }
        #endregion
    }

    public static void Copy(string sourceDirectory, string targetDirectory)
    {
        DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
        DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

        CopyAll(diSource, diTarget);
    }

    public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
    {
        Directory.CreateDirectory(target.FullName);

        foreach (FileInfo fi in source.GetFiles())
        {
            if (fi.Extension != ".meta")
            {
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }
        }

        foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
        {
            DirectoryInfo nextTargetSubDir =
                target.CreateSubdirectory(diSourceSubDir.Name);
            CopyAll(diSourceSubDir, nextTargetSubDir);
        }
    }
}