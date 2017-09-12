using Net.Media;
using System;
using System.Drawing;
using System.Drawing.Imaging;
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

    private const int _width = 1024;
    private const int _height = 576;
    private const int _pixelBytes = 4;
    private const int _pitch = 1024 * _pixelBytes;
    private IntPtr _buff = IntPtr.Zero;

    public float fireRate = 1F;
    private float nextFire = 0.0F;
    bool ready = false;
    // Use this for initialization
    void Start()
    {
        if (_videoLockCB == null)
            _videoLockCB = new MediaPlayer.VideoLockCB(VideoLockCallBack);
        if (_videoUnlockCB == null)
            _videoUnlockCB = new MediaPlayer.VideoUnlockCB(VideoUnlockCallBack);
        if (_videoDisplayCB == null)
            _videoDisplayCB = new MediaPlayer.VideoDisplayCB(VideoDiplayCallBack);

        texture = new Texture2D(1024, 576, TextureFormat.RGBA32, false);

        handle = new IntPtr(110);

        libvlc_instance_t = MediaPlayer.Create_Media_Instance();

        libvlc_media_player_t = MediaPlayer.Create_MediaPlayer(libvlc_instance_t, handle);

        MediaPlayer.SetFormart(libvlc_media_player_t, "RV32", _width, _height, _width * 4);
        MediaPlayer.SetCallbacks(libvlc_media_player_t, _videoLockCB, _videoUnlockCB, _videoDisplayCB, IntPtr.Zero);

        //"file:///"+Application.streamingAssetsPath+"/test.mp4");
        //bool ready = MediaPlayer.NetWork_Media_Play(libvlc_instance_t, libvlc_media_player_t, "rtsp://127.0.0.1:8554/1");
        ready = MediaPlayer.NetWork_Media_Play(libvlc_instance_t, libvlc_media_player_t, "file:///" + Application.streamingAssetsPath + "/test.mp4");
        Debug.Log(ready);

        width = MediaPlayer.GetMediaWidth(libvlc_media_player_t);
        height = MediaPlayer.GetMediaHeight(libvlc_media_player_t);
        

        Debug.Log(MediaPlayer.MediaPlayer_IsPlaying(libvlc_media_player_t));
    }


    // Update is called once per frame
    void Update()
    { 
        if (ready && Time.time > nextFire)
        {
            nextFire = Time.time + fireRate;
            //Debug.Log(Islock());
            if (Islock())
            {
                Debug.Log(_buff.ToInt32());
                using (Bitmap bmp = new Bitmap(_width, _height, PixelFormat.Format32bppPArgb))
                {
                    Rectangle rect = new Rectangle(0, 0, _width, _height);
                    BitmapData bp = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat);
                    CopyMemory(bp.Scan0, _buff, (uint)(_height * _pitch));
                    bmp.UnlockBits(bp);
                    //bmp.Save("c:\\vlc.bmp"); 
                }
            }
        }
    }

    private void OnGUI()
    {
        if(GUI.Button(new Rect(0,0,100,100),"Take"))
        {
          Debug.Log (MediaPlayer.TakeSnapShot(libvlc_media_player_t, @Application.streamingAssetsPath, "testa.jpg"));
        }
    }

    private IntPtr VideoLockCallBack(IntPtr opaque, IntPtr planes)
    {
        Lock();
        _buff = Marshal.AllocHGlobal(_pitch * _height);
        Marshal.WriteIntPtr(planes, _buff);//初始化
        Debug.Log("Lock");
        return IntPtr.Zero;
    }
    private void VideoUnlockCallBack(IntPtr opaque, IntPtr picture, IntPtr planes)
    {
        Unlock();
        Debug.Log("Unlock");
    }
    private void VideoDiplayCallBack(IntPtr opaque, IntPtr picture)
    {
        if (Islock())
        {
            Debug.Log("Islock");
            //fwrite(buffer, sizeof buffer, 1, fp);  
        }
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
