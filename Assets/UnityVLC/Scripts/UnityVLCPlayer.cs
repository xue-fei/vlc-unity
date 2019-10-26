using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace VLC
{
    public class UnityVLCPlayer : MonoBehaviour
    {
        private IntPtr _libvlc_instance_t;
        private IntPtr _libvlc_media_player_t;
        private int _pixelBytes = 3;
        private int _pitch;
        private IntPtr _buff = IntPtr.Zero;
        private VideoLock _videoLock;
        private VideoUnlock _videoUnlock;
        private VideoDisplay _videoDisplay;
        //视频宽
        private int width = 1024;
        //视频高
        private int height = 768;
        private float length = 0;
        private Texture2D m_texture;

        public Action<float, string> OnProgress;

        public void Init()
        {
            Loom.Initialize();
            _videoLock += OnVideoLock;
            _videoDisplay += OnVideoDiplay;
            _videoUnlock += OnVideoUnlock;
            _libvlc_instance_t = VLCPlayer.Create_Media_Instance();
            _libvlc_media_player_t = VLCPlayer.Create_MediaPlayer(_libvlc_instance_t);
        }

        /// <summary>
        /// 设置视频路径 本地/网络
        /// </summary>
        public void SetLocation(string path, Material material)
        {
            Debug.Log("path:" + path);
            bool state = VLCPlayer.SetLocation(_libvlc_instance_t, _libvlc_media_player_t, path);
            Debug.Log("state:" + state);
            width = VLCPlayer.GetMediaWidth(_libvlc_media_player_t);
            Debug.Log("width: " + width);
            height = VLCPlayer.GetMediaHeight(_libvlc_media_player_t);
            Debug.Log("height: " + height);
            length = VLCPlayer.GetMediaLength(_libvlc_media_player_t);
            Debug.Log("length: " + length);
            //网络地址不晓得怎么拿到视频宽高
            if (width == 0 && height == 0)
            {
                width = 1024;
                height = 576;
            }
            m_texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            Debug.Log(m_texture.GetRawTextureData().Length);
            _pitch = width * _pixelBytes;
            _buff = Marshal.AllocHGlobal(height * _pitch);
            material.mainTexture = m_texture;
            VLCPlayer.SetCallbacks(_libvlc_media_player_t, _videoLock, _videoUnlock, _videoDisplay, IntPtr.Zero);
            VLCPlayer.SetFormart(_libvlc_media_player_t, "RV24", width, height, _pitch);

            bool isPlaying = VLCPlayer.MediaPlayer_IsPlaying(_libvlc_media_player_t);
            Debug.Log("isPlaying:" + isPlaying);
        }

        /// <summary>
        /// 播放
        /// </summary>
        public void Play()
        {
            bool ready = VLCPlayer.MediaPlayer_Play(_libvlc_media_player_t);
            Debug.Log("ready:" + ready);
        }

        /// <summary>
        /// 暂停
        /// </summary>
        public void Pause()
        {
            if (VLCPlayer.MediaPlayer_IsPlaying(_libvlc_media_player_t))
            {
                VLCPlayer.MediaPlayer_Pause(_libvlc_media_player_t);
            }
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            if (VLCPlayer.MediaPlayer_IsPlaying(_libvlc_media_player_t))
            {
                VLCPlayer.MediaPlayer_Stop(_libvlc_media_player_t);
            }
        }

        public bool IsPlaying()
        {
            return VLCPlayer.MediaPlayer_IsPlaying(_libvlc_media_player_t);
        }

        /// <summary>
        /// 录制快照
        /// </summary>
        public void TakeSnapShot(string filePath)
        {
            byte[] data = m_texture.EncodeToJPG();
            File.WriteAllBytes(filePath, data);
        }

        private IntPtr OnVideoLock(IntPtr opaque, IntPtr planes)
        {
            Marshal.WriteIntPtr(planes, 0, _buff);
            Loom.QueueOnMainThread(() =>
            {
                m_texture.LoadRawTextureData(_buff, _buff.ToInt32());
                m_texture.Apply();
                GetProgress();
            });
            return IntPtr.Zero;
        }

        private void OnVideoDiplay(IntPtr opaque, IntPtr picture)
        {

        }

        private void OnVideoUnlock(IntPtr opaque, IntPtr picture, IntPtr planes)
        {

        }

        public void SetProgress(float value)
        {
            VLCPlayer.SetPosition(_libvlc_media_player_t, value);
            string time = GetHMS((int)(length * value));
            if (OnProgress != null)
            {
                OnProgress(value, time);
            }
        }

        private void GetProgress()
        {
            long len = VLCPlayer.GetPosition(_libvlc_media_player_t);
            string time = GetHMS((int)len);
            float progress = len / length;
            if (OnProgress != null)
            {
                OnProgress(progress, time);
            }
        }

        private string GetHMS(int length)
        {
            TimeSpan ts = new TimeSpan(0, 0, 0, 0, length);

            return (ts.Hours.ToString("00") + ":" + ts.Minutes.ToString("00") + ":"
                    + ts.Seconds.ToString("00"));
        }

        private void OnDestroy()
        {
            if (VLCPlayer.MediaPlayer_IsPlaying(_libvlc_media_player_t))
            {
                VLCPlayer.MediaPlayer_Stop(_libvlc_media_player_t);
            }
            VLCPlayer.Release_MediaPlayer(_libvlc_media_player_t);
            VLCPlayer.Release_Media_Instance(_libvlc_instance_t); 
        } 
    }
}