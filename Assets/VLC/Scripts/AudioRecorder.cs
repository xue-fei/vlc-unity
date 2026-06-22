// AudioRecorder.cs
// 核心录制组件。
//
// 原理：
//   Unity 的 OnAudioFilterRead 回调在音频 DSP 链的末端触发，
//   data[] 包含当前帧混音后的所有声音（BGM + 音效 + VLC 音频等）。
//   将此 GameObject 上的 AudioListener 捕获的最终混音数据写入 WAV。
//
// 使用步骤：
//   1. 把此脚本挂到场景中带有 AudioListener 的摄像机 GameObject 上。
//   2. 调用 StartRecording() 开始，StopRecording() 结束。
//   3. 录制完成后在 OnRecordingSaved 回调中获取文件路径。

using System;
using System.IO;
using UnityEngine;
using Audio;

namespace Audio
{
    [RequireComponent(typeof(AudioListener))]
    public class AudioRecorder : MonoBehaviour
    {
        // ---- 公开配置 ----

        [Tooltip("录制文件保存目录，默认为 Application.persistentDataPath")]
        public string saveDirectory = "";

        [Tooltip("文件名前缀")]
        public string filePrefix = "Recording";

        [Tooltip("最大录制时长（秒），0 = 不限制")]
        public float maxDurationSeconds = 0f;

        // ---- 事件 ----

        /// <summary>录制完成时触发，参数为 WAV 文件完整路径。</summary>
        public event Action<string> OnRecordingSaved;

        /// <summary>录制出错时触发。</summary>
        public event Action<string> OnRecordingError;

        // ---- 只读状态 ----

        public bool IsRecording { get; private set; }
        public float RecordedSeconds => IsRecording ? _recordedSamples / (float)(_sampleRate * _channels) : 0f;
        public string CurrentFilePath { get; private set; }

        // ---- 内部状态 ----

        private WavWriter _writer;
        private int _sampleRate;
        private int _channels;
        private long _recordedSamples;
        private long _maxSamples;        // 0 = 无限

        // OnAudioFilterRead 在音频线程调用，用 volatile bool 控制开关避免锁竞争
        private volatile bool _capturing;

        // ---- 公开方法 ----

        /// <summary>
        /// 开始录制。
        /// </summary>
        /// <param name="fileName">
        /// 自定义文件名（不含扩展名）。留空则自动用时间戳命名。
        /// </param>
        public void StartRecording(string fileName = "")
        {
            if (IsRecording)
            {
                Debug.LogWarning("[AudioRecorder] 已在录制中，请先调用 StopRecording()。");
                return;
            }

            try
            {
                // 确定保存路径
                string dir = string.IsNullOrEmpty(saveDirectory)
                    ? Application.persistentDataPath
                    : saveDirectory;
                Directory.CreateDirectory(dir);

                string name = string.IsNullOrEmpty(fileName)
                    ? $"{filePrefix}_{DateTime.Now:yyyyMMdd_HHmmss}"
                    : fileName;

                CurrentFilePath = Path.Combine(dir, name + ".wav");

                // 获取 Unity 音频系统参数
                _sampleRate = AudioSettings.outputSampleRate;
                switch (AudioSettings.speakerMode)
                {
                    case AudioSpeakerMode.Mono: _channels = 1; break;
                    case AudioSpeakerMode.Stereo: _channels = 2; break;
                    case AudioSpeakerMode.Quad: _channels = 4; break;
                    case AudioSpeakerMode.Surround: _channels = 5; break;
                    case AudioSpeakerMode.Mode5point1: _channels = 6; break;
                    case AudioSpeakerMode.Mode7point1: _channels = 8; break;
                    default: _channels = 2; break;
                }

                _maxSamples = maxDurationSeconds > 0f
                    ? (long)(maxDurationSeconds * _sampleRate * _channels)
                    : 0L;
                _recordedSamples = 0L;

                _writer = new WavWriter(CurrentFilePath, _channels, _sampleRate);
                IsRecording = true;
                _capturing = true;  // 音频线程开始捕获

                Debug.Log($"[AudioRecorder] 开始录制 → {CurrentFilePath}  ({_sampleRate}Hz × {_channels}ch)");
            }
            catch (Exception ex)
            {
                IsRecording = false;
                _capturing = false;
                OnRecordingError?.Invoke(ex.Message);
                Debug.LogError($"[AudioRecorder] 启动失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 停止录制并保存文件。
        /// </summary>
        public void StopRecording()
        {
            if (!IsRecording) return;

            _capturing = false;   // 先停止音频线程写入
            IsRecording = false;

            try
            {
                _writer?.Close();
                _writer = null;
                Debug.Log($"[AudioRecorder] 录制完成 ({RecordedSecondsSnapshot():F1}s) → {CurrentFilePath}");
                OnRecordingSaved?.Invoke(CurrentFilePath);
            }
            catch (Exception ex)
            {
                OnRecordingError?.Invoke(ex.Message);
                Debug.LogError($"[AudioRecorder] 保存失败: {ex.Message}");
            }
        }

        // ---- Unity 消息 ----

        /// <summary>
        /// OnAudioFilterRead 在 Unity 音频 DSP 线程调用（非主线程）。
        /// data 是交错 PCM float（-1~1），channels 是声道数。
        /// 注意：不能修改 data，否则会影响实际播放。
        /// </summary>
        void OnAudioFilterRead(float[] data, int channels)
        {
            if (!_capturing || _writer == null) return;

            // 检查是否超过最大时长
            if (_maxSamples > 0 && _recordedSamples + data.Length >= _maxSamples)
            {
                // 只写剩余部分
                int remaining = (int)(_maxSamples - _recordedSamples);
                if (remaining > 0)
                {
                    float[] last = new float[remaining];
                    Array.Copy(data, last, remaining);
                    _writer.Write(last);
                    _recordedSamples += remaining;
                }
                _capturing = false;
                // 在主线程完成文件写入（StopRecording 通过 _pendingStop 触发）
                _pendingStop = true;
                return;
            }

            _writer.Write(data);
            _recordedSamples += data.Length;
        }

        // 超时停止需要回到主线程执行（文件操作不能在音频线程做）
        private volatile bool _pendingStop;

        void Update()
        {
            if (_pendingStop)
            {
                _pendingStop = false;
                Debug.Log("[AudioRecorder] 已达最大时长，自动停止。");
                StopRecording();
            }
        }

        void OnDestroy()
        {
            if (IsRecording) StopRecording();
        }

        void OnApplicationQuit()
        {
            if (IsRecording) StopRecording();
        }

        // ---- 私有工具 ----

        private float RecordedSecondsSnapshot()
        {
            if (_sampleRate == 0 || _channels == 0) return 0f;
            return _recordedSamples / (float)(_sampleRate * _channels);
        }
    }
}