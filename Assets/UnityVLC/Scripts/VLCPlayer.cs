using System;
using System.Runtime.InteropServices;

namespace VLC
{
    using Debug = UnityEngine.Debug;

    public class VLCPlayer
    {
        private IntPtr _libvlc;
        private IntPtr _media;
        private IntPtr _mediaPlayer;

        private libvlc_video_lock_cb _videoLock;
        private libvlc_video_unlock_cb _videoUnlock;
        private libvlc_video_display_cb _videoDisplay;
        private int _tracks;
        private IntPtr _tracksIntPtr;
        private libvlc_video_track_t? _videoTrack = null;
        private int _width = 1024;
        private int _height = 576;
        private int _channels = 3;
        private IntPtr _imageIntPtr;
        private byte[] _imageData;
        /// <summary>
        /// 视频长度(毫秒)
        /// </summary>
        private float length = 0;
        private GCHandle _gcHandle;
        private bool _locked = true;
        private bool _update = false;
        private volatile bool _cancel = false;
        /// <summary>
        /// 视频播放进度
        /// </summary>
        public Action<float, string> OnProgress;

        #region 公开函数

        public VLCPlayer(int width, int height, string url)
        {
            Debug.Log("Playing: " + url);
            _width = width;
            _height = height;
            _gcHandle = GCHandle.Alloc(this);
            string[] args1 =
                {
                    "--no-ignore-config",
                    "--no-xlib",
                    "--no-video-title-show",
                    "--no-osd"
                };
            _libvlc = LibVLC.libvlc_new(args1.Length, args1);
            if (_libvlc == IntPtr.Zero)
            {
                Debug.LogError("Failed creat libvlc instance...");
                return;
            }
            _media = LibVLC.libvlc_media_new_location(_libvlc, url);
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
            LibVLC.libvlc_media_player_set_media(_mediaPlayer, _media);
            LibVLC.libvlc_media_parse_with_options(_media, libvlc_media_parse_flag_t.libvlc_media_parse_local, 200);
            //LibVLC.libvlc_media_parse(_media);
            //LibVLC.libvlc_media_parse_async(_media);
            _videoLock = VideoLock;
            _videoUnlock = VideoUnlock;
            _videoDisplay = VideoDisplay;
            LibVLC.libvlc_video_set_callbacks(_mediaPlayer, _videoLock, _videoUnlock, _videoDisplay, GCHandle.ToIntPtr(_gcHandle));
            LibVLC.libvlc_video_set_format(_mediaPlayer, "RV24", (uint)_width, (uint)_height, (uint)_width * (uint)_channels);
            System.Threading.Thread.Sleep(300);
            //LibVLC.libvlc_media_player_play(_mediaPlayer);
            System.Threading.Thread t = new System.Threading.Thread(TrackReaderThread);
            t.Start();
        }

        private void TrackReaderThread()
        {
            int _trackGetAttempts = 0;
            while (_trackGetAttempts < 10 && _cancel == false)
            {
                libvlc_video_track_t? track = GetVideoTrack();

                if (track.HasValue && track.Value.i_width > 0 && track.Value.i_height > 0)
                {
                    _videoTrack = track;
                    if (_width <= 0 || _height <= 0)
                    {
                        _width = (int)_videoTrack.Value.i_width;
                        _height = (int)_videoTrack.Value.i_height;
                        LibVLC.libvlc_video_set_format(_mediaPlayer, "RV24", _videoTrack.Value.i_width, _videoTrack.Value.i_height, (uint)_width * (uint)_channels);
                        //LibVLC.libvlc_media_player_play(_mediaPlayer);
                    }
                    break;
                }
                _trackGetAttempts++;
                System.Threading.Thread.Sleep(500);
            }

            if (_trackGetAttempts >= 10)
            {
                Debug.LogError("Maximum attempts of getting video track reached, maybe opening failed?");
            }
        }

        public libvlc_video_track_t? VideoTrack
        {
            get
            {
                return _videoTrack;
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

        private IntPtr VideoLock(IntPtr opaque, ref IntPtr planes)
        {
            _locked = true;
            if (_imageIntPtr == IntPtr.Zero)
            {
                _imageIntPtr = Marshal.AllocHGlobal(_width * _channels * _height);
            }
            planes = _imageIntPtr;
            return _imageIntPtr;
        }

        private void VideoUnlock(IntPtr opaque, IntPtr picture, ref IntPtr planes)
        {
            _locked = false;
        }

        private void VideoDisplay(IntPtr opaque, IntPtr picture)
        {
            if (!_update)
            {
                _imageData = new byte[_width * _channels * _height];
                Marshal.Copy(picture, _imageData, 0, _width * _channels * _height);
                _update = true;
            }
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

                if (0 != LibVLC.libvlc_media_player_play(_mediaPlayer))
                {
                    return false;
                }
                //休眠指定时间
                //Thread.Sleep(500); 
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

                LibVLC.libvlc_media_player_stop_async(_mediaPlayer);

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

        public libvlc_video_track_t? GetVideoTrack()
        {
            IntPtr tracks;
            libvlc_video_track_t? videoTrack = null;
            int tracksInt = LibVLC.libvlc_media_tracks_get(_media, out tracks);
            _tracks = tracksInt;
            _tracksIntPtr = tracks;
            for (int i = 0; i < tracksInt; i++)
            {
                IntPtr mtrackptr = Marshal.ReadIntPtr(tracks, i * IntPtr.Size);
                libvlc_media_track_t mtrack = Marshal.PtrToStructure<libvlc_media_track_t>(mtrackptr);
                if (mtrack.i_type == libvlc_track_type_t.libvlc_track_video)
                {
                    videoTrack = Marshal.PtrToStructure<libvlc_video_track_t>(mtrack.media);
                }
            }
            return videoTrack;
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

        public void GetProgress()
        {
            long len = GetPosition();
            string time = GetHMS((int)len);
            float progress = len / length;
            if (OnProgress != null)
            {
                OnProgress(progress, time);
            }
        }

        public void SetProgress(float value)
        {
            SetPosition(value);
            string time = GetHMS((int)(length * value));
            if (OnProgress != null)
            {
                OnProgress(value, time);
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