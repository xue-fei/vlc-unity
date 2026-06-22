// Editor/AudioRecorderUIEditor.cs
// 必须放在 Assets/Editor/ 目录下（或任意 Editor 文件夹内）。
// 在 Inspector 面板上提供：开始/停止按钮、实时计时、文件路径一键打开。

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using Audio;

[CustomEditor(typeof(AudioRecorderUI))]
public class AudioRecorderUIEditor : Editor
{
    // 控制 Inspector 每帧刷新，用于实时显示录制时长
    private double _lastRepaintTime;

    // 折叠状态
    private bool _showHelp = false;

    public override void OnInspectorGUI()
    {
        // 绘制默认字段（recorder、customFileName、maxDurationSeconds、只读状态）
        DrawDefaultInspector();

        EditorGUILayout.Space(6);
        DrawSeparator();
        EditorGUILayout.Space(4);

        var ui = (AudioRecorderUI)target;
        var recorder = GetRecorder(ui);
        bool isPlaying = Application.isPlaying;
        bool isRecording = isPlaying && recorder != null && recorder.IsRecording;

        // ---- 状态栏 ----
        DrawStatusBar(recorder, isPlaying, isRecording);

        EditorGUILayout.Space(4);

        // ---- 主控按钮 ----
        using (new EditorGUILayout.HorizontalScope())
        {
            // 开始按钮
            GUI.enabled = isPlaying && !isRecording;
            Color oldColor = GUI.backgroundColor;
            GUI.backgroundColor = isPlaying && !isRecording ? new Color(0.4f, 0.9f, 0.4f) : Color.gray;
            if (GUILayout.Button(EditorGUIUtility.IconContent("d_PlayButton"), GUILayout.Height(32), GUILayout.Width(36)))
            {
                ui.StartRecording();
            }
            GUI.backgroundColor = oldColor;

            GUILayout.Space(4);

            // 停止按钮
            GUI.enabled = isPlaying && isRecording;
            GUI.backgroundColor = isPlaying && isRecording ? new Color(0.95f, 0.4f, 0.4f) : Color.gray;
            if (GUILayout.Button(EditorGUIUtility.IconContent("d_PreMatQuad"), GUILayout.Height(32), GUILayout.Width(36)))
            {
                ui.StopRecording();
            }
            GUI.backgroundColor = oldColor;
            GUI.enabled = true;

            GUILayout.Space(4);

            // 打开文件夹按钮
            string lastPath = GetLastPath(ui);
            GUI.enabled = !string.IsNullOrEmpty(lastPath) && File.Exists(lastPath);
            if (GUILayout.Button(new GUIContent(" 打开目录", EditorGUIUtility.IconContent("d_FolderOpened Icon").image),
                    GUILayout.Height(32)))
            {
                EditorUtility.RevealInFinder(lastPath);
            }
            GUI.enabled = true;
        }

        EditorGUILayout.Space(4);

        // ---- 最后保存路径 ----
        string saved = GetLastPath(ui);
        if (!string.IsNullOrEmpty(saved))
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("上次保存", GUILayout.Width(60));
            EditorGUILayout.SelectableLabel(saved, EditorStyles.miniTextField, GUILayout.Height(18));
            EditorGUILayout.EndHorizontal();
        }

        // ---- 帮助折叠 ----
        EditorGUILayout.Space(2);
        _showHelp = EditorGUILayout.Foldout(_showHelp, "使用说明", true);
        if (_showHelp)
        {
            EditorGUILayout.HelpBox(
                "1. 将此脚本挂到任意 GameObject。\n" +
                "2. recorder 字段留空时，运行后自动挂载到 Main Camera。\n" +
                "3. 运行时点击 ▶ 开始录制，点击 ■ 停止并保存为 WAV。\n" +
                "4. WAV 文件默认保存到 Application.persistentDataPath，\n" +
                "   也可在 AudioRecorder.saveDirectory 中自定义路径。\n" +
                "5. 点击 打开目录 可直接在系统文件管理器中定位文件。",
                MessageType.Info);
        }

        // ---- 实时刷新（录制中每 0.1 秒重绘一次）----
        if (isRecording)
        {
            double now = EditorApplication.timeSinceStartup;
            if (now - _lastRepaintTime > 0.1)
            {
                _lastRepaintTime = now;
                Repaint();
            }
        }
    }

    // ---- 辅助 ----

    private void DrawStatusBar(AudioRecorder recorder, bool isPlaying, bool isRecording)
    {
        string statusText;
        MessageType msgType;

        if (!isPlaying)
        {
            statusText = "请进入运行模式后操作";
            msgType = MessageType.None;
        }
        else if (recorder == null)
        {
            statusText = "未找到 AudioRecorder，运行后将自动创建";
            msgType = MessageType.Warning;
        }
        else if (isRecording)
        {
            float sec = recorder.RecordedSeconds;
            int m = (int)sec / 60;
            int s = (int)sec % 60;
            int ms = (int)((sec - (int)sec) * 10);
            statusText = $"● 录制中  {m:00}:{s:00}.{ms}";
            msgType = MessageType.None;
        }
        else
        {
            statusText = "待机";
            msgType = MessageType.None;
        }

        if (isRecording)
        {
            // 录制中用红色高亮样式
            var style = new GUIStyle(EditorStyles.helpBox)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.9f, 0.2f, 0.2f) }
            };
            EditorGUILayout.LabelField(statusText, style, GUILayout.Height(22));
        }
        else
        {
            if (msgType == MessageType.None)
                EditorGUILayout.LabelField(statusText, EditorStyles.centeredGreyMiniLabel);
            else
                EditorGUILayout.HelpBox(statusText, msgType);
        }
    }

    private static void DrawSeparator()
    {
        var rect = EditorGUILayout.GetControlRect(false, 1f);
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.3f));
    }

    private static AudioRecorder GetRecorder(AudioRecorderUI ui)
    {
        // 优先用字段，其次场景查找
        var so = new SerializedObject(ui);
        var prop = so.FindProperty("recorder");
        if (prop.objectReferenceValue != null)
            return prop.objectReferenceValue as AudioRecorder;
        return Object.FindObjectOfType<AudioRecorder>();
    }

    private static string GetLastPath(AudioRecorderUI ui)
    {
        var so = new SerializedObject(ui);
        var prop = so.FindProperty("_lastSavedPath");
        return prop?.stringValue ?? "";
    }
}
#endif