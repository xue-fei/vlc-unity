using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VLC;

public class Example : MonoBehaviour
{
    private VLCPlayer player;
    /// <summary>
    /// 视频宽
    /// </summary>
    private int width = 1024;
    /// <summary>
    /// 视频高
    /// </summary>
    private int height = 768;
    /// <summary>
    /// 视频长度(毫秒)
    /// </summary>
    private float length = 0;
    public RawImage rawImage;
    private Texture2D texture;
    public Slider slider;
    public Text text;
    public Button btnStart;
    public Button btnPause;
    public Button btnStop;

    // Use this for initialization
    void Start()
    {
        //湖南卫视直播地址
        //string videoPath = "rtmp://58.200.131.2:1935/livetv/hunantv";
        //大兔子
        //string videoPath = "http://223.110.242.130:6610/gitv/live1/G_CCTV-1-HQ/1.m3u8";
        //本地视频
        string videoPath = @"file:///" + Application.streamingAssetsPath + "/test.mp4";
        //捕捉屏幕
        //string videoPath = "screen://";
        player = new VLCPlayer(width, height, videoPath);

        player.OnProgress += OnProgress;
        length = player.GetMediaLength();
        Debug.Log("length:" + length);

        btnStart.onClick.AddListener(delegate ()
        {
            OnCtrl(btnStart);
        });
        btnPause.onClick.AddListener(delegate ()
        {
            OnCtrl(btnPause);
        });
        btnStop.onClick.AddListener(delegate ()
        {
            OnCtrl(btnStop);
        });

        UnityAction<BaseEventData> drag = new UnityAction<BaseEventData>(OnDrag);
        EventTrigger.Entry myDrag = new EventTrigger.Entry();
        myDrag.eventID = EventTriggerType.Drag;
        myDrag.callback.AddListener(drag);
        EventTrigger eventTrigger = slider.gameObject.AddComponent<EventTrigger>();
        eventTrigger.triggers.Add(myDrag);
    }

    byte[] img;
    private void Update()
    {
        if (player != null && player.GetVideoImage(out img))
        {
            if (texture == null)
            {
                if ((width <= 0 || height <= 0) && player.VideoTrack != null)
                {
                    width = (int)player.VideoTrack.Value.i_width;
                    height = (int)player.VideoTrack.Value.i_height;
                }
                if (width > 0 && height > 0)
                {
                    texture = new Texture2D(width, height, TextureFormat.RGB24, false, false);
                    rawImage.texture = texture;
                }
            }
            else
            {
                GetProgress();
                texture.LoadRawTextureData(img);
                texture.Apply(false);
            }
        }
    }

    private void OnProgress(float progress, string time)
    {
        slider.value = progress;
        text.text = time;
    }

    void OnDrag(BaseEventData data)
    {
        player.SetProgress(slider.value);
    }

    private void OnCtrl(Button button)
    {
        if (button.name == btnStart.name)
        {
            if (!player.IsPlaying())
            {
                player.Play();
            }
        }
        if (button.name == btnPause.name)
        {
            if (player.IsPlaying())
            {
                btnPause.transform.Find("Text").GetComponent<Text>().text = "继续";
                player.Pause();
            }
            if (!player.IsPlaying())
            {
                btnPause.transform.Find("Text").GetComponent<Text>().text = "暂停";
                player.Play();
            }
        }
        if (button.name == btnStop.name)
        {
            if (player.IsPlaying())
            {
                player.Stop();
            }
        }
    }

    /// <summary>
    /// 获取播放进度 
    /// </summary>
    private void GetProgress()
    {
        long len = player.GetPosition();
        string time = GetHMS((int)len);
        float progress = len / length;
        slider.value = progress;
        text.text = time;
    }

    private string GetHMS(int length)
    {
        TimeSpan ts = new TimeSpan(0, 0, 0, 0, length);

        return (ts.Hours.ToString("00") + ":" + ts.Minutes.ToString("00") + ":"
                + ts.Seconds.ToString("00"));
    }

    private void OnDestroy()
    {
        player?.Dispose();
    }
}