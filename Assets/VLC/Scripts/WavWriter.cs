// WavWriter.cs
// WAV 文件写入工具——支持边录边写（流式）和一次性写入两种模式。

using System;
using System.IO;
using UnityEngine;

namespace Audio
{
    /// <summary>
    /// 流式 WAV 写入器。
    /// 使用方式：
    ///   1. new WavWriter(path, channels, sampleRate)
    ///   2. Write(float[]) —— 可多次调用
    ///   3. Close() —— 补写文件头，完成文件
    /// </summary>
    public class WavWriter : IDisposable
    {
        private FileStream _fs;
        private BinaryWriter _bw;
        private int _channels;
        private int _sampleRate;
        private long _dataChunkSizePos; // 记录 data chunk size 字段的文件偏移
        private long _riffChunkSizePos;
        private int _totalSamples;     // 写入的总 float 样本数（含所有声道）
        private bool _closed;

        public string FilePath { get; private set; }

        public WavWriter(string filePath, int channels, int sampleRate)
        {
            FilePath = filePath;
            _channels = channels;
            _sampleRate = sampleRate;

            _fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            _bw = new BinaryWriter(_fs);

            WriteHeader();
        }

        /// <summary>写入 PCM float 数据（-1 ~ 1，交错排列）。线程安全：加锁保护。</summary>
        public void Write(float[] data)
        {
            if (_closed || data == null || data.Length == 0) return;
            lock (_bw)
            {
                foreach (float sample in data)
                {
                    // float → 16-bit PCM
                    short s = (short)Mathf.Clamp(Mathf.RoundToInt(sample * 32767f), short.MinValue, short.MaxValue);
                    _bw.Write(s);
                }
                _totalSamples += data.Length;
            }
        }

        /// <summary>完成录制，补写 RIFF/data 尺寸字段。</summary>
        public void Close()
        {
            if (_closed) return;
            _closed = true;

            lock (_bw)
            {
                int dataByteCount = _totalSamples * 2; // 每样本 2 字节（16-bit）

                // 修正 data chunk size
                _fs.Seek(_dataChunkSizePos, SeekOrigin.Begin);
                _bw.Write(dataByteCount);

                // 修正 RIFF chunk size = 文件总长 - 8
                _fs.Seek(_riffChunkSizePos, SeekOrigin.Begin);
                _bw.Write((int)(_fs.Length - 8));

                _bw.Flush();
                _fs.Close();
            }
        }

        public void Dispose() => Close();

        // ---- WAV 文件头（44 字节） ----
        // 占位写入，Close() 时回填正确尺寸
        private void WriteHeader()
        {
            int byteRate = _sampleRate * _channels * 2;
            int blockAlign = _channels * 2;

            // RIFF chunk
            _bw.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            _riffChunkSizePos = _fs.Position;
            _bw.Write(0);                    // 占位，Close() 时回填
            _bw.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

            // fmt  chunk
            _bw.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
            _bw.Write(16);                   // PCM chunk size
            _bw.Write((short)1);             // AudioFormat = PCM
            _bw.Write((short)_channels);
            _bw.Write(_sampleRate);
            _bw.Write(byteRate);
            _bw.Write((short)blockAlign);
            _bw.Write((short)16);            // BitsPerSample

            // data chunk
            _bw.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            _dataChunkSizePos = _fs.Position;
            _bw.Write(0);                    // 占位，Close() 时回填
        }

        // ---- 一次性工具方法（无需实例化）----

        /// <summary>
        /// 把 AudioClip 直接保存为 WAV 文件（非流式）。
        /// </summary>
        public static void Save(string filePath, AudioClip clip)
        {
            if (clip == null) throw new ArgumentNullException(nameof(clip));
            float[] data = new float[clip.samples * clip.channels];
            clip.GetData(data, 0);
            using var writer = new WavWriter(filePath, clip.channels, clip.frequency);
            writer.Write(data);
        }
    }
}