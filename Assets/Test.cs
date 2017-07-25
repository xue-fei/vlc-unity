using Net.Media;
using System;
using System.Runtime.InteropServices;
using UnityEngine; 
public class Test : MonoBehaviour
{
    //视频宽
    public int width;
    //视频高
    public int height;
    public Texture2D texture;
    IntPtr libvlc_instance_t;
    IntPtr libvlc_media_player_t;
    IntPtr handle;

    private MediaPlayer.VideoLockCB _videoLockCB;
    private MediaPlayer.VideoUnlockCB _videoUnlockCB;
    private MediaPlayer.VideoDisplayCB _videoDisplayCB;

    // Use this for initialization
    void Start()
    {
        _videoLockCB += VideoLockCallBack;
        _videoUnlockCB += VideoUnlockCallBack;
        _videoDisplayCB += VideoDiplayCallBack;

        texture = new Texture2D(640, 480, TextureFormat.RGBA32, false);

        handle = new IntPtr(110);
         
        libvlc_instance_t = MediaPlayer.Create_Media_Instance();
        
        libvlc_media_player_t = MediaPlayer.Create_MediaPlayer(libvlc_instance_t, handle);
        //"file:///"+Application.streamingAssetsPath+"/test.mp4");
        bool ready = MediaPlayer.NetWork_Media_Play(libvlc_instance_t, libvlc_media_player_t, "rtsp://127.0.0.1:8554/1");
        Debug.Log(ready);
        
        width = MediaPlayer.GetMediaWidth(libvlc_media_player_t);
        height = MediaPlayer.GetMediaHeight(libvlc_media_player_t);
        MediaPlayer.SetFormart(libvlc_media_player_t, "ARGB32", width, height, width * 4);
        MediaPlayer.SetCallbacks(libvlc_media_player_t, _videoLockCB, _videoUnlockCB, _videoDisplayCB, IntPtr.Zero);

        Debug.Log(MediaPlayer.MediaPlayer_IsPlaying(libvlc_media_player_t));
    }


    // Update is called once per frame
    void Update()
    {

    }

    public static byte[] buffer = new byte[1024 * 4];
    private static IntPtr VideoLockCallBack(IntPtr opaque, IntPtr planes)
    {
        Lock();
        Marshal.Copy(planes, buffer, 0, 1024 * 4);
        Debug.Log("Lock");
        return IntPtr.Zero;
    }
    private static void VideoUnlockCallBack(IntPtr opaque, IntPtr picture, IntPtr planes)
    {
        Unlock();
        Debug.Log("Unlock");
    }
    private static void VideoDiplayCallBack(IntPtr opaque, IntPtr picture)
    {
        if (Islock())
        {
            Debug.Log("Islock");
            //fwrite(buffer, sizeof buffer, 1, fp);  
        }
    }


    public static bool flag = false;
    public static void Lock()
    {
        flag = true;
    }
    public static void Unlock()
    {
        flag = false;
    }
    public static bool Islock()
    {
        return false;
    }

    private void OnDestroy()
    {

    }

    private void OnApplicationQuit()
    {
        try
        { 
            Debug.Log(MediaPlayer.MediaPlayer_IsPlaying(libvlc_media_player_t));

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
