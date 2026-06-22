using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using AOT;
using Debug = UnityEngine.Debug;

namespace VLC
{
    public class VLCPlayer
    {
        private IntPtr _libvlc;
        private IntPtr _media;
        private IntPtr _mediaPlayer;
        private IntPtr _event_manager;
        private libvlc_video_lock_cb _videoLock;
        private libvlc_video_unlock_cb _videoUnlock;
        private libvlc_video_display_cb _videoDisplay;
        private uint _width = 0;
        private uint _height = 0;
        private uint _channels = 3;
        private IntPtr _imageIntPtr;
        private byte[] _imageData;
        /// <summary>
        /// 视频长度(毫秒)
        /// </summary>
        private float length = 0;
        private GCHandle _gcHandle;
        private bool _update = false;
        // 事件列表
        List<libvlc_event_e> events = new List<libvlc_event_e>();

        // ====== 音频回调相关 ======
        // 委托必须持有引用，防止 GC 回收
        private libvlc_audio_play_cb _audioPlayCb;
        private libvlc_audio_pause_cb _audioPauseCb;
        private libvlc_audio_resume_cb _audioResumeCb;
        private libvlc_audio_flush_cb _audioFlushCb;
        private libvlc_audio_drain_cb _audioDrainCb;
        private libvlc_audio_set_volume_cb _audioVolumeCb;

        // 环形缓冲区（float PCM，解码线程写、Unity音频线程读）
        private const int RING_BUFFER_SAMPLES = 44100 * 2 * 4; // 4 秒 @ 44100 stereo
        private readonly float[] _audioRingBuffer = new float[RING_BUFFER_SAMPLES];
        private int _audioWritePos = 0;
        private int _audioReadPos = 0;
        private int _audioAvailable = 0;
        private readonly object _audioLock = new object();

        // 音频参数（由 AudioSetFormat 回调填入）
        private int _audioSampleRate = 44100;
        private int _audioChannels = 2;
        private bool _audioParamsReady = false;

        // 供外部查询"是否有新音频参数可以建 AudioClip"
        public bool AudioParamsReady => _audioParamsReady;
        public int AudioSampleRate => _audioSampleRate;
        public int AudioChannels => _audioChannels;

        #region 公开函数

        public void Init(uint width, uint height, string url)
        {
#if UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
            LibVLC.XInitThreads();
#endif
            _width = width;
            _height = height;
            _gcHandle = GCHandle.Alloc(this);
            string[] args1 =
                {
                    "--no-ignore-config",
                    "--no-xlib",
                    "--no-video-title-show",
                    "--no-osd",
                    "--video-filter=adjust",
                    "--network-caching=300",
                };
            _libvlc = LibVLC.libvlc_new(args1.Length, args1);
            if (_libvlc == IntPtr.Zero)
            {
                Debug.LogError("Failed creat libvlc instance...");
                return;
            }
            // 本地文件 如 file:///G:/MyProject/vlc-unity/Assets/StreamingAssets/test.mp4
            bool local = File.Exists(url);
            if (local)
            {
                _media = LibVLC.libvlc_media_new_path(_libvlc, url);
            }
            else
            {
                _media = LibVLC.libvlc_media_new_location(_libvlc, url);
            }
            string[] args2 =
                {
                    ":avcodec-hw=any",
                    ":vout=direct3d11",
                    //":directx-use-sysmem",
                    //":directx-overlay",
                    //":spect-show-original",
                    ":avcodec-threads=124"
                    //捕捉屏幕的相关参数
                    //":screen-fps=30",
                    //":screen-width=1920",
                    //":screen-width=1080",
                    //":video-filter=transform",
                    //":transform-type=hflip",
                    //":transform-type=vflip",
                };
            LibVLC.libvlc_media_add_option(_media, args2);
            if (_media == IntPtr.Zero)
            {
                Debug.LogError("Failed creat media instance...");
                return;
            }
            _mediaPlayer = LibVLC.libvlc_media_player_new(_libvlc);
            _event_manager = LibVLC.libvlc_media_player_event_manager(_mediaPlayer);
            attachEvents(_event_manager);
            LibVLC.libvlc_media_player_set_media(_mediaPlayer, _media);
            LibVLC.libvlc_media_parse_with_options(_media, libvlc_media_parse_flag_t.libvlc_media_parse_network, 10000);
            //LibVLC.libvlc_media_release(_media);

            _videoLock = VideoLock;
            _videoUnlock = VideoUnlock;
            _videoDisplay = VideoDisplay;

            LibVLC.libvlc_video_set_callbacks(_mediaPlayer, _videoLock, _videoUnlock, _videoDisplay, GCHandle.ToIntPtr(_gcHandle));

            // ====== 注册音频回调 ======
            _audioPlayCb = AudioPlay;
            _audioPauseCb = AudioPause;
            _audioResumeCb = AudioResume;
            _audioFlushCb = AudioFlush;
            _audioDrainCb = AudioDrain;
            _audioVolumeCb = AudioSetVolumeCb;

            // 固定格式：有符号16位小端，44100Hz，2声道
            // libvlc 会把解码出的音频重采样/转格式到此规格后再回调
            LibVLC.libvlc_audio_set_format(_mediaPlayer, "S16N", 44100, 2);
            LibVLC.libvlc_audio_set_callbacks(_mediaPlayer,
                _audioPlayCb, _audioPauseCb, _audioResumeCb,
                _audioFlushCb, _audioDrainCb,
                GCHandle.ToIntPtr(_gcHandle));
            LibVLC.libvlc_audio_set_volume_callback(_mediaPlayer, _audioVolumeCb);
        }

        void attachEvents(IntPtr eventManager)
        {
            events.Add(libvlc_event_e.libvlc_MediaPlayerOpening);
            events.Add(libvlc_event_e.libvlc_MediaPlayerBuffering);
            events.Add(libvlc_event_e.libvlc_MediaPlayerESAdded);
            events.Add(libvlc_event_e.libvlc_MediaPlayerEncounteredError);
            events.Add(libvlc_event_e.libvlc_MediaPlayerPlaying);
            events.Add(libvlc_event_e.libvlc_MediaPlayerPaused);
            events.Add(libvlc_event_e.libvlc_MediaPlayerStopped);
            events.Add(libvlc_event_e.libvlc_MediaPlayerPositionChanged);
            events.Add(libvlc_event_e.libvlc_MediaPlayerTimeChanged);
            events.Add(libvlc_event_e.libvlc_MediaPlayerLengthChanged);
            events.Add(libvlc_event_e.libvlc_MediaPlayerMediaChanged);
            IntPtr userData = GCHandle.ToIntPtr(_gcHandle);
            // 订阅事件
            foreach (libvlc_event_e e in events)
            {
                LibVLC.libvlc_event_attach(eventManager, e, handleEvents, userData);
            }
        }

        void detachEvents(IntPtr eventManager)
        {
            IntPtr userData = GCHandle.ToIntPtr(_gcHandle);
            foreach (libvlc_event_e e in events)
            {
                LibVLC.libvlc_event_detach(eventManager, e, handleEvents, userData);
            }
        }

        [MonoPInvokeCallback(typeof(libvlc_callback_t))]
        public static void handleEvents(libvlc_event_t e, IntPtr userData)
        {
            GCHandle handle = GCHandle.FromIntPtr(userData);
            VLCPlayer instance = (VLCPlayer)handle.Target;
            if (instance == null) return;

            switch (e.type)
            {
                case libvlc_event_e.libvlc_MediaPlayerOpening:
                    //Debug.LogWarning("libvlc_MediaPlayerOpening");
                    break;
                case libvlc_event_e.libvlc_MediaPlayerBuffering:
                    //Debug.LogWarning("libvlc_MediaPlayerBuffering");
                    break;
                case libvlc_event_e.libvlc_MediaPlayerESAdded:
                    // 此时视频流已发现，可安全获取尺寸 
                    LibVLC.libvlc_video_get_size(instance._mediaPlayer, 0, ref instance._width, ref instance._height);
                    if (instance._width > 0 && instance._height > 0)
                    {
                        LibVLC.libvlc_video_set_format(instance._mediaPlayer, "RV24", instance._width, instance._height, instance._width * 3);
                    }
                    break;
                case libvlc_event_e.libvlc_MediaPlayerEncounteredError:
                    Debug.LogWarning("视频加载失败");
                    break;
                case libvlc_event_e.libvlc_MediaPlayerPlaying:
                    //Debug.LogWarning("libvlc_MediaPlayerPlaying");
                    break;
                case libvlc_event_e.libvlc_MediaPlayerPaused:
                    //Debug.LogWarning("libvlc_MediaPlayerPaused");
                    break;
                case libvlc_event_e.libvlc_MediaPlayerStopped:
                    //Debug.LogWarning("libvlc_MediaPlayerStopped");
                    break;
                case libvlc_event_e.libvlc_MediaPlayerPositionChanged:
                    //Debug.LogWarning("libvlc_MediaPlayerPositionChanged");
                    break;
                case libvlc_event_e.libvlc_MediaPlayerTimeChanged:
                    //Debug.LogWarning("libvlc_MediaPlayerTimeChanged");
                    break;
                case libvlc_event_e.libvlc_MediaPlayerLengthChanged:
                    //Debug.LogWarning("libvlc_MediaPlayerLengthChanged");
                    break;
                default:
                    //Debug.LogWarning(e.type);
                    break;
            }
        }

        public bool GetVideoImage(out byte[] imageData, out uint width, out uint height)
        {
            imageData = null;
            width = _width;
            height = _height;
            if (_update)
            {
                imageData = _imageData;
                _update = false;
                return true;
            }
            return false;
        }

        [MonoPInvokeCallback(typeof(libvlc_video_format_cb))]
        public static IntPtr VideoFormat(IntPtr opaque, string chroma, uint width, uint height, uint pitches, uint lines)
        {
            Debug.LogWarning(" " + chroma + " " + width + " " + height);
            return (IntPtr)1;
        }

        [MonoPInvokeCallback(typeof(libvlc_video_cleanup_cb))]
        public static void VideoClean(IntPtr opaque)
        {

        }

        static int defaultWidth = 1920;
        static int defaultHeight = 1080;

        [MonoPInvokeCallback(typeof(libvlc_video_lock_cb))]
        public static IntPtr VideoLock(IntPtr opaque, ref IntPtr planes)
        {
            // 通过 opaque 获取实例
            GCHandle handle = GCHandle.FromIntPtr(opaque);
            VLCPlayer instance = (VLCPlayer)handle.Target;
            if (instance._imageIntPtr == IntPtr.Zero)
            {
                if (instance._width == 0 || instance._height == 0)
                {
                    instance._imageIntPtr = Marshal.AllocHGlobal((int)(defaultWidth * instance._channels * defaultHeight));
                }
                else
                {
                    instance._imageIntPtr = Marshal.AllocHGlobal((int)(instance._width * instance._channels * instance._height));
                }
            }
            planes = instance._imageIntPtr;
            return instance._imageIntPtr;
        }

        [MonoPInvokeCallback(typeof(libvlc_video_unlock_cb))]
        public static void VideoUnlock(IntPtr opaque, IntPtr picture, ref IntPtr planes)
        {
            GCHandle handle = GCHandle.FromIntPtr(opaque);
            VLCPlayer instance = (VLCPlayer)handle.Target;
        }

        [MonoPInvokeCallback(typeof(libvlc_video_display_cb))]
        public static void VideoDisplay(IntPtr opaque, IntPtr picture)
        {
            GCHandle handle = GCHandle.FromIntPtr(opaque);
            VLCPlayer instance = (VLCPlayer)handle.Target;
            if (!instance._update)
            {
                instance._imageData = new byte[instance._width * instance._channels * instance._height];
                Marshal.Copy(picture, instance._imageData, 0, (int)(instance._width * instance._channels * instance._height));
                instance._update = true;
            }
        }

        public int GetSize(Action<uint, uint> action = null)
        {
            int code = LibVLC.libvlc_video_get_size(_mediaPlayer, 0, ref _width, ref _height);
            if (_width > 0 && _height > 0)
            {
                length = GetMediaLength();
                Debug.Log("length:" + length);

                Debug.LogWarning($"视频尺寸: {_width}x{_height}");
                action?.Invoke(_width, _height);
                return 0;
            }
            return -1;
        }

        /// <summary>
        /// 播放
        /// </summary>
        /// <returns></returns>
        public bool Play()
        {
            try
            {
                if (_mediaPlayer == IntPtr.Zero || _mediaPlayer == null)
                {
                    return false;
                }
                //LibVLC.libvlc_media_player_set_hwnd(_mediaPlayer, (System.IntPtr)0);

                LibVLC.libvlc_video_set_format(_mediaPlayer, "RV24", _width, _height, _width * _channels);

                if (0 != LibVLC.libvlc_media_player_play(_mediaPlayer))
                {
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }
        }

        /// <summary>
        /// 暂停或恢复视频
        /// </summary>
        /// <returns></returns>
        public bool Pause()
        {
            try
            {
                if (_mediaPlayer == IntPtr.Zero ||
                    _mediaPlayer == null)
                {
                    return false;
                }

                if (LibVLC.libvlc_media_player_can_pause(_mediaPlayer))
                {
                    LibVLC.libvlc_media_player_pause(_mediaPlayer);

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }
        }

        /// <summary>
        /// 停止播放
        /// </summary>
        /// <returns></returns>
        public bool Stop()
        {
            try
            {
                if (_mediaPlayer == IntPtr.Zero ||
                    _mediaPlayer == null)
                {
                    return false;
                }
                LibVLC.libvlc_media_player_stop(_mediaPlayer);
                //VLC4.0或更高版本
                //LibVLC.libvlc_media_player_stop_async(_mediaPlayer);
                //LibVLC.CloseLibrary(lib);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }
        }

        /// <summary>
        /// 是否在播放
        /// </summary>
        /// <returns></returns>
        public bool IsPlaying()
        {
            try
            {
                if (_mediaPlayer == IntPtr.Zero ||
                    _mediaPlayer == null)
                {
                    return false;
                }
                return LibVLC.libvlc_media_player_is_playing(_mediaPlayer);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }
        }

        public long GetMediaLength()
        {
            long length = 0;
            if (_media != IntPtr.Zero)
            {
                length = LibVLC.libvlc_media_get_duration(_media);
            }
            return length;
        }

        public Int64 GetPosition()
        {
            return LibVLC.libvlc_media_player_get_time(_mediaPlayer);
        }

        public void SetPosition(float posf)
        {
            LibVLC.libvlc_media_player_set_position(_mediaPlayer, posf, false);
        }

        long len;
        string time;
        float progress;
        public void GetProgress(Action<float, string> action = null)
        {
            len = GetPosition();
            time = GetHMS((int)len);
            progress = len / length;
            if (action != null)
            {
                action(progress, time);
            }
        }

        public float GetProgress()
        {
            return (float)len / length;
        }

        /// <summary>
        /// 设置音量
        /// </summary>
        /// <param name="volume">0-100</param>
        public void SetVolume(int volume)
        {
            LibVLC.libvlc_audio_set_volume(_mediaPlayer, volume);
        }

        public string GetVersion()
        {
            return Marshal.PtrToStringAnsi(LibVLC.libvlc_get_version());
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            if (_event_manager != IntPtr.Zero)
            {
                detachEvents(_event_manager);
            }
            if (_mediaPlayer != IntPtr.Zero)
            {
                LibVLC.libvlc_media_player_release(_mediaPlayer);
            }
            if (_media != IntPtr.Zero)
            {
                LibVLC.libvlc_media_release(_media);
            }
            if (_libvlc != IntPtr.Zero)
            {
                LibVLC.libvlc_release(_libvlc);
            }
            _mediaPlayer = IntPtr.Zero;
            _media = IntPtr.Zero;
            _libvlc = IntPtr.Zero;
        }

        private string GetHMS(int length)
        {
            TimeSpan ts = new TimeSpan(0, 0, 0, 0, length);

            return (ts.Hours.ToString("00") + ":" + ts.Minutes.ToString("00") + ":"
                    + ts.Seconds.ToString("00"));
        }

        #endregion

        // ====================================================
        // 音频回调（libvlc 解码线程调用，需 MonoPInvokeCallback）
        // ====================================================

        /// <summary>
        /// libvlc 每次解码好一批 PCM 后调用此函数。
        /// 把 S16 数据转换为 float 并写入环形缓冲区。
        /// </summary>
        [MonoPInvokeCallback(typeof(libvlc_audio_play_cb))]
        public static void AudioPlay(IntPtr opaque, IntPtr samples, uint count, long pts)
        {
            GCHandle handle = GCHandle.FromIntPtr(opaque);
            VLCPlayer instance = (VLCPlayer)handle.Target;
            if (instance == null) return;

            // count = 每声道采样数，总 short 个数 = count * channels
            int totalSamples = (int)(count * instance._audioChannels);

            lock (instance._audioLock)
            {
                for (int i = 0; i < totalSamples; i++)
                {
                    // S16N：每个 short 2 字节，范围 -32768~32767 → float -1~1
                    short s = Marshal.ReadInt16(samples, i * 2);
                    float f = s / 32768.0f;

                    instance._audioRingBuffer[instance._audioWritePos] = f;
                    instance._audioWritePos = (instance._audioWritePos + 1) % RING_BUFFER_SAMPLES;

                    if (instance._audioAvailable < RING_BUFFER_SAMPLES)
                        instance._audioAvailable++;
                    else
                        // 缓冲区满时丢弃最旧数据（移动读指针）
                        instance._audioReadPos = (instance._audioReadPos + 1) % RING_BUFFER_SAMPLES;
                }

                // 首次进来时记录格式已就绪（格式在 libvlc_audio_set_format 中已固定）
                if (!instance._audioParamsReady)
                {
                    instance._audioSampleRate = 44100;
                    instance._audioChannels = 2;
                    instance._audioParamsReady = true;
                }
            }
        }

        [MonoPInvokeCallback(typeof(libvlc_audio_pause_cb))]
        public static void AudioPause(IntPtr opaque, long pts) { }

        [MonoPInvokeCallback(typeof(libvlc_audio_resume_cb))]
        public static void AudioResume(IntPtr opaque, long pts) { }

        /// <summary>
        /// seek 或停止时清空环形缓冲区，防止播旧数据。
        /// </summary>
        [MonoPInvokeCallback(typeof(libvlc_audio_flush_cb))]
        public static void AudioFlush(IntPtr opaque, long pts)
        {
            GCHandle handle = GCHandle.FromIntPtr(opaque);
            VLCPlayer instance = (VLCPlayer)handle.Target;
            if (instance == null) return;
            lock (instance._audioLock)
            {
                instance._audioWritePos = 0;
                instance._audioReadPos = 0;
                instance._audioAvailable = 0;
            }
        }

        [MonoPInvokeCallback(typeof(libvlc_audio_drain_cb))]
        public static void AudioDrain(IntPtr opaque) { }

        [MonoPInvokeCallback(typeof(libvlc_audio_set_volume_cb))]
        public static void AudioSetVolumeCb(IntPtr opaque, float volume, bool mute) { }

        // ====================================================
        // 供 Unity 音频线程（OnAudioRead）调用
        // ====================================================

        /// <summary>
        /// Unity AudioClip PCMReaderCallback 中调用此方法填充 data[]。
        /// 缓冲区不足时补零（静音），避免爆音。
        /// </summary>
        public void ReadAudioData(float[] data)
        {
            lock (_audioLock)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    if (_audioAvailable > 0)
                    {
                        data[i] = _audioRingBuffer[_audioReadPos];
                        _audioReadPos = (_audioReadPos + 1) % RING_BUFFER_SAMPLES;
                        _audioAvailable--;
                    }
                    else
                    {
                        data[i] = 0f; // 欠载静音
                    }
                }
            }
        }
    }
}