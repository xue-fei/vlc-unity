using Net.Media;
using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class Test : MonoBehaviour
{
    //视频宽
    public int width = 1024;
    //视频高
    public int height = 768;
    public float length = 0;

    public Texture2D m_texture;
    public Material matVideo;
    public Slider slider;
    public Text text;
    IntPtr libvlc_instance_t;
    IntPtr libvlc_media_player_t;

    private VideoLockCB _videoLockCB;
    private VideoUnlockCB _videoUnlockCB;
    private VideoDisplayCB _videoDisplayCB;

    private int _pixelBytes = 3;
    private int _pitch;
    private IntPtr _buff = IntPtr.Zero;

    string snapShotpath;
    // Use this for initialization
    void Start()
    {
        Application.targetFrameRate = 30;
        Loom.Initialize();
        snapShotpath = "file:///" + Application.streamingAssetsPath;

        _videoLockCB += VideoLockCallBack;

        _videoUnlockCB += VideoUnlockCallBack;

        _videoDisplayCB += VideoDiplayCallBack;

        libvlc_instance_t = MediaPlayer.Create_Media_Instance();

        libvlc_media_player_t = MediaPlayer.Create_MediaPlayer(libvlc_instance_t);
        //湖南卫视直播地址
        //string videoPath = "rtmp://58.200.131.2:1935/livetv/hunantv";
        //本地视频地址
        string videoPath = "file:///" + Application.streamingAssetsPath + "/test.mp4";
        bool state = MediaPlayer.SetLocation(libvlc_instance_t, libvlc_media_player_t, videoPath);
        Debug.Log("state:" + state);
        width = MediaPlayer.GetMediaWidth(libvlc_media_player_t);
        Debug.Log("width: " + width);
        height = MediaPlayer.GetMediaHeight(libvlc_media_player_t);
        Debug.Log("height: " + height);
        length = MediaPlayer.GetMediaLength(libvlc_media_player_t);
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

        matVideo.mainTexture = m_texture;

        MediaPlayer.SetCallbacks(libvlc_media_player_t, _videoLockCB, _videoUnlockCB, _videoDisplayCB, IntPtr.Zero);
        MediaPlayer.SetFormart(libvlc_media_player_t, "RV24", width, height, _pitch);

        bool ready = MediaPlayer.MediaPlayer_Play(libvlc_media_player_t);
        Debug.Log("ready:" + ready);
        bool isPlaying = MediaPlayer.MediaPlayer_IsPlaying(libvlc_media_player_t);
        Debug.Log("isPlaying:" + isPlaying); 
        slider.onValueChanged.AddListener(ChangValue);
    }

    private void OnDrag(PointerEventData obj)
    {
        throw new NotImplementedException();
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 100, 100), "Take"))
        {
            Debug.Log("snapShotpath:" + snapShotpath);
            Debug.Log("@snapShotpath:" + @snapShotpath);
            byte[] bs = m_texture.EncodeToJPG();
            File.WriteAllBytes(Application.streamingAssetsPath + "/test.jpg", bs);
        }
    }

    private IntPtr VideoLockCallBack(IntPtr opaque, IntPtr planes)
    {
        Lock();
        Marshal.WriteIntPtr(planes, 0, _buff);
        Loom.QueueOnMainThread(() =>
        {
            m_texture.LoadRawTextureData(_buff, _buff.ToInt32());
            m_texture.Apply();
            GetProgress();
        });
        return IntPtr.Zero;
    }

    private void VideoDiplayCallBack(IntPtr opaque, IntPtr picture)
    {

    }

    private void VideoUnlockCallBack(IntPtr opaque, IntPtr picture, IntPtr planes)
    {
        Unlock();
    }

    bool obj = false;
    private void Lock()
    {
        obj = true;
    }
    private void Unlock()
    {
        obj = false;
    }
    private bool Islock()
    {
        return obj;
    }

    long len;
    private void ChangValue(float value)
    {
        MediaPlayer.SetPosition(libvlc_media_player_t, value);
        text.text = GetHMS((int)(length*value));
    }

    private void GetProgress()
    {
        len = MediaPlayer.GetPosition(libvlc_media_player_t);
        text.text = GetHMS((int)len);
        slider.value = len / length;
    }

    private void OnDestroy()
    {

    }

    private void OnApplicationQuit()
    {
        try
        {
            if (MediaPlayer.MediaPlayer_IsPlaying(libvlc_media_player_t))
            {
                MediaPlayer.MediaPlayer_Stop(libvlc_media_player_t);
            }

            MediaPlayer.Release_MediaPlayer(libvlc_media_player_t);

            MediaPlayer.Release_Media_Instance(libvlc_instance_t);

            GC.Collect();
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    string GetHMS(int l)
    {
        TimeSpan ts = new TimeSpan(0, 0, 0, 0, l);

        return (ts.Hours.ToString("00") + ":" + ts.Minutes.ToString("00") + ":"
                + ts.Seconds.ToString("00"));
    }
}