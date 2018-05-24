using Net.Media;
using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

public class Test : MonoBehaviour
{
    //视频宽
    public int width;
    //视频高
    public int height;
    public Texture2D texture;
    public Material mat;
    IntPtr libvlc_instance_t;
    IntPtr libvlc_media_player_t;
    IntPtr handle;

    private VideoLockCB _videoLockCB;
    private VideoUnlockCB _videoUnlockCB;
    private VideoDisplayCB _videoDisplayCB;

    private const int _width = 1024;
    private const int _height = 576;
    private const int _pixelBytes = 4;
    private const int _pitch = 1024 * _pixelBytes;
    private IntPtr _buff = IntPtr.Zero;

    bool ready = false;

    string snapShotpath;
    // Use this for initialization
    void Start()
    { 
        snapShotpath = "file:///" + Application.streamingAssetsPath;

        _videoLockCB += VideoLockCallBack;

        _videoUnlockCB += VideoUnlockCallBack;

        _videoDisplayCB += VideoDiplayCallBack;

        texture = new Texture2D(1024, 576, TextureFormat.RGBA32, false);
        mat.mainTexture = texture;
        _buff = Marshal.AllocHGlobal(_pitch * _height);
        handle = new IntPtr(1);

        libvlc_instance_t = MediaPlayer.Create_Media_Instance();

        libvlc_media_player_t = MediaPlayer.Create_MediaPlayer(libvlc_instance_t, handle);

        MediaPlayer.SetCallbacks(libvlc_media_player_t, _videoLockCB, _videoUnlockCB, _videoDisplayCB, IntPtr.Zero);

        //"file:///"+Application.streamingAssetsPath+"/test.mp4");rtmp://live.hkstv.hk.lxdns.com/live/hks
        //bool ready = MediaPlayer.NetWork_Media_Play(libvlc_instance_t, libvlc_media_player_t, "rtsp://127.0.0.1:8554/1");
        ready = MediaPlayer.NetWork_Media_Play(libvlc_instance_t, libvlc_media_player_t, "rtmp://live.hkstv.hk.lxdns.com/live/hks");
        Debug.Log(ready);

        width = MediaPlayer.GetMediaWidth(libvlc_media_player_t);
        height = MediaPlayer.GetMediaHeight(libvlc_media_player_t);
        MediaPlayer.SetFormart(libvlc_media_player_t, "ARGB", _width, _height, _width * 4);

        Debug.Log(MediaPlayer.MediaPlayer_IsPlaying(libvlc_media_player_t));
        Debug.Log(Application.dataPath);
    }


    // Update is called once per frame
    void Update()
    {
        if (Islock())
        {
            texture.LoadRawTextureData(_buff, _buff.ToInt32());
            texture.Apply();
        }
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 100, 100), "Take"))
        { 
            //vlc截图未解决 用Unity保存帧图，画面是上下反转左右反转的
            //Debug.Log(MediaPlayer.TakeSnapShot(libvlc_media_player_t, snapShotpath, "testa.jpg",1024,576)); 
            byte[] bs = texture.EncodeToJPG();
            File.WriteAllBytes(Application.streamingAssetsPath + "/test.jpg", bs); 
        }

    }

    private IntPtr VideoLockCallBack(IntPtr opaque, IntPtr planes)
    {
        Lock();
        //初始化  
        Marshal.WriteIntPtr(planes, _buff);
        return IntPtr.Zero;
    }
    private void VideoUnlockCallBack(IntPtr opaque, IntPtr picture, IntPtr planes)
    {
        Unlock(); 
    }
    private void VideoDiplayCallBack(IntPtr opaque, IntPtr picture)
    { 

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

    private void OnDestroy()
    {

    }

    [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
    private static extern void CopyMemory(IntPtr Destination, IntPtr Source, uint Length);

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
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }
}
