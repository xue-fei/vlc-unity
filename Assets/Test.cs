using Net.Media;
using System;
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
    // Use this for initialization
    void Start()
    {
        texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

        handle = new IntPtr(110);
         
        libvlc_instance_t = MediaPlayer.Create_Media_Instance();
        
        libvlc_media_player_t = MediaPlayer.Create_MediaPlayer(libvlc_instance_t, handle);
        
        bool ready = MediaPlayer.NetWork_Media_Play(libvlc_instance_t, libvlc_media_player_t, "file:///"+Application.streamingAssetsPath+"/test.mp4");
        Debug.Log(ready);
        
        width = MediaPlayer.GetMediaWidth(libvlc_media_player_t);
        height = MediaPlayer.GetMediaHeight(libvlc_media_player_t);
        MediaPlayer.SetFormart(libvlc_media_player_t, "ARGB32", width, height, width * 4);

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
