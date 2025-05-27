using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EditorTool : Editor
{
    [MenuItem("Tools/打开沙盒文件夹", false, 0)]
    static void OpenPersistentDataPath()
    {
        System.Diagnostics.Process.Start(@Application.persistentDataPath);
    }
}