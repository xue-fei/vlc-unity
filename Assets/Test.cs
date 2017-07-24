using Net.Media;
using System;
using UnityEngine;

public class Test : MonoBehaviour
{
    public Texture2D texture;
    IntPtr libvlc_instance_t;
    IntPtr libvlc_media_player_t;
    IntPtr handle;
    // Use this for initialization
    void Start()
    {
        texture = new Texture2D(640, 480, TextureFormat.YUY2, false);

        handle = new IntPtr(110);
         
        libvlc_instance_t = MediaPlayer.Create_Media_Instance();
        
        libvlc_media_player_t = MediaPlayer.Create_MediaPlayer(libvlc_instance_t, texture.GetNativeTexturePtr());
        
        bool ready = MediaPlayer.NetWork_Media_Play(libvlc_instance_t, libvlc_media_player_t, "file:///D:\\MyProject\\VLC\\Assets\\StreamingAssets\\test.mp4");
        Debug.Log(ready);
        
        Debug.Log(MediaPlayer.MediaPlayer_IsPlaying(libvlc_media_player_t));
    }


    // Update is called once per frame
    void Update()
    {

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
