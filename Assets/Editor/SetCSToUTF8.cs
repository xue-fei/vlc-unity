using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

/// <summary>
/// CS文件转化为UTF-8格式的工具
/// </summary> 
public class SetCSToUTF8 : EditorWindow
{
    /// <summary>
    /// 文件路径
    /// </summary>
    private string csPath = "";

    /// <summary>
    /// 文件后缀
    /// </summary>
    private string suffix = ".cs";

    [MenuItem("工具/代码转UTF-8格式")]
    static void Init()
    {
        SetCSToUTF8 window = (SetCSToUTF8)EditorWindow.GetWindow(typeof(SetCSToUTF8), false, "代码格式转换", true);
        window.Show();
    }

    void OnGUI()
    {
        EditorGUILayout.Space();
        GUILayout.Label("鼠标点击代码文件夹");
        EditorGUILayout.Space();
        GUILayout.Label("文件夹路径: ");
        csPath = GUILayout.TextField(csPath);
        Object[] objs = Selection.GetFiltered(typeof(Object), SelectionMode.TopLevel);
        csPath = Application.dataPath + "/../" + AssetDatabase.GetAssetPath(objs[0]) + "/";
        GUILayout.Label("文件后缀: ");
        suffix = GUILayout.TextField(suffix);

        if (GUILayout.Button("CS文件转UTF-8格式"))
        {
            Conversion();
        }
    }

    private void Conversion()
    {
        if (csPath.Equals(string.Empty))
        {
            return;
        }

        if (!Directory.Exists(csPath))
        {
            Debug.LogError("找不到文件夹路径！");
            return;
        }

        string[] files = Directory.GetFiles(csPath, "*", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            if (!file.EndsWith(suffix)) continue;
            string strTempPath = file.Replace(@"\", "/");
            Debug.Log("文件路径：" + strTempPath);
            ConvertFileEncoding(strTempPath, new UTF8Encoding(true, true));
        }
        AssetDatabase.Refresh();
        Debug.Log("格式转换完成！");
    }

    /// <summary>
    /// 文件编码转换
    /// </summary>
    /// <param name="sourceFile">源文件</param> 
    /// <param name="targetEncoding">目标编码</param>
    private static void ConvertFileEncoding(string sourceFile, Encoding targetEncoding)
    {
        string fileString = File.ReadAllText(sourceFile, Encoding.Default);
        File.WriteAllText(sourceFile, fileString, targetEncoding);
    }
}