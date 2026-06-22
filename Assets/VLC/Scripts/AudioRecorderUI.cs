// AudioRecorderUI.cs
// 录制控制器——无 UGUI 依赖，所有操作通过 Inspector 面板按钮完成。
// 运行时也可直接调用公开方法。

using System.IO;
using UnityEngine;
using Audio;

public class AudioRecorderUI : MonoBehaviour
{
    [Header("录制目标")]
    [Tooltip("挂有 AudioRecorder 的 GameObject（含 AudioListener）；留空自动寻找")]
    public AudioRecorder recorder;

    [Header("录制设置")]
    [Tooltip("文件名（不含扩展名），留空则自动使用时间戳")]
    public string customFileName = "";

    [Tooltip("最大录制时长（秒），0 = 不限制")]
    public float maxDurationSeconds = 0f;

    [Header("只读状态（运行时更新）")]
    [SerializeField, Tooltip("当前是否正在录制")]
    private bool _isRecording = false;

    [SerializeField, Tooltip("已录制时长（秒）")]
    private float _recordedSeconds = 0f;

    [SerializeField, Tooltip("上次保存的文件路径")]
    private string _lastSavedPath = "";

    // ---- Unity 消息 ----

    void Start()
    {
        EnsureRecorder();

        if (recorder != null)
        {
            recorder.maxDurationSeconds = maxDurationSeconds;
            recorder.OnRecordingSaved += OnSaved;
            recorder.OnRecordingError += OnError;
        }
    }

    void Update()
    {
        if (recorder == null) return;
        _isRecording = recorder.IsRecording;
        _recordedSeconds = recorder.RecordedSeconds;
    }

    void OnDestroy()
    {
        if (recorder != null)
        {
            recorder.OnRecordingSaved -= OnSaved;
            recorder.OnRecordingError -= OnError;
        }
    }

    // ---- 公开控制接口（Editor 按钮 / 代码均可调用）----

    /// <summary>开始录制。</summary>
    public void StartRecording()
    {
        EnsureRecorder();
        if (recorder == null) return;
        recorder.maxDurationSeconds = maxDurationSeconds;
        recorder.StartRecording(customFileName);
        _isRecording = true;
    }

    /// <summary>停止录制并保存 WAV 文件。</summary>
    public void StopRecording()
    {
        if (recorder == null) return;
        recorder.StopRecording();
        _isRecording = false;
    }

    /// <summary>直接把一个 AudioClip 保存为 WAV，不依赖录制流程。</summary>
    public void SaveClip(AudioClip clip, string fileName = "")
    {
        if (clip == null) { Debug.LogWarning("[AudioRecorderUI] clip 为空"); return; }

        EnsureRecorder();
        string dir = (recorder != null && !string.IsNullOrEmpty(recorder.saveDirectory))
            ? recorder.saveDirectory
            : Application.persistentDataPath;
        string name = string.IsNullOrEmpty(fileName)
            ? $"Clip_{System.DateTime.Now:yyyyMMdd_HHmmss}"
            : fileName;
        string path = Path.Combine(dir, name + ".wav");

        WavWriter.Save(path, clip);
        _lastSavedPath = path;
        Debug.Log($"[AudioRecorderUI] AudioClip 已保存 → {path}");
    }

    // ---- 私有 ----

    private void EnsureRecorder()
    {
        if (recorder != null) return;

        recorder = FindObjectOfType<AudioRecorder>();
        if (recorder != null) return;

        var cam = Camera.main;
        if (cam != null)
        {
            recorder = cam.gameObject.AddComponent<AudioRecorder>();
            Debug.Log("[AudioRecorderUI] 已自动挂载 AudioRecorder 到主摄像机。");
        }
        else
        {
            Debug.LogError("[AudioRecorderUI] 找不到带 AudioListener 的 GameObject，请手动拖入 recorder。");
        }
    }

    private void OnSaved(string path)
    {
        _lastSavedPath = path;
        _isRecording = false;
        _recordedSeconds = 0f;
        Debug.Log($"[AudioRecorderUI] 已保存：{path}");
    }

    private void OnError(string msg)
    {
        _isRecording = false;
        Debug.LogError($"[AudioRecorderUI] 错误：{msg}");
    }
}