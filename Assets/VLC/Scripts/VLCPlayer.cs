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
        private bool _locked = true;
        private bool _update = false;
        // 事件列表
        List<libvlc_event_e> events = new List<libvlc_event_e>();

        #region 公开函数

        public void Init(uint width, uint height, string url)
        {
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
                };
            _libvlc = LibVLC.libvlc_new(args1.Length, args1);
            if (_libvlc == IntPtr.Zero)
            {
                Debug.LogError("Failed creat libvlc instance...");
                return;
            }
            // 本地文件 如 file:///G:/MyProject/vlc-unity/Assets/StreamingAssets/test.mp4
            if (File.Exists(url))
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
        }

        void attachEvents(IntPtr eventManager)
        {
            events.Add(libvlc_event_e.libvlc_MediaPlayerOpening);
            events.Add(libvlc_event_e.libvlc_MediaPlayerBuffering);
            events.Add(libvlc_event_e.libvlc_MediaPlayerPlaying);
            events.Add(libvlc_event_e.libvlc_MediaPlayerPaused);
            events.Add(libvlc_event_e.libvlc_MediaPlayerStopped);
            events.Add(libvlc_event_e.libvlc_MediaPlayerPositionChanged);
            events.Add(libvlc_event_e.libvlc_MediaPlayerTimeChanged);
            events.Add(libvlc_event_e.libvlc_MediaPlayerLengthChanged);
            events.Add(libvlc_event_e.libvlc_MediaPlayerMediaChanged);
            // 订阅事件
            foreach (libvlc_event_e e in events)
            {
                LibVLC.libvlc_event_attach(eventManager, e, handleEvents, IntPtr.Zero);
            }
        }

        void detachEvents(IntPtr eventManager)
        {
            foreach (libvlc_event_e e in events)
            {
                LibVLC.libvlc_event_detach(eventManager, e, handleEvents, IntPtr.Zero);
            }
        }

        [MonoPInvokeCallback(typeof(libvlc_callback_t))]
        public static void handleEvents(libvlc_event_t e, IntPtr userData)
        {
            switch (e.type)
            {
                case libvlc_event_e.libvlc_MediaPlayerOpening:
                    //Debug.LogWarning("libvlc_MediaPlayerOpening");
                    break;
                case libvlc_event_e.libvlc_MediaPlayerBuffering:
                    //Debug.LogWarning("libvlc_MediaPlayerBuffering");
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

        public bool GetVideoImage(out byte[] imageData)
        {
            imageData = null;
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
            instance._locked = true;
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
            instance._locked = false;
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
            if (_width == 0 || _height == 0)
            {
                code = -1;
                return code;
            }
            Debug.LogWarning("code:" + code + " _width:" + _width + " _height:" + _height);
            action?.Invoke(_width, _height);
            return code;
        }

        public void SetFormat()
        {
            LibVLC.libvlc_video_set_format(_mediaPlayer, "RV24", _width, _height, _width * _channels);
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
                if (0 != LibVLC.libvlc_media_player_play(_mediaPlayer))
                {
                    return false;
                }
                length = GetMediaLength();
                Debug.Log("length:" + length);
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
    }
}