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
    private uint width = 1024;
    /// <summary>
    /// 视频高
    /// </summary>
    private uint height = 576;
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
        //http://devimages.apple.com.edgekey.net/streaming/examples/bipbop_4x3/gear2/prog_index.m3u8
        string videoPath = "http://39.134.115.163:8080/PLTV/88888910/224/3221225632/index.m3u8";
        //本地视频
        //string videoPath = @"file:///" + Application.streamingAssetsPath + "/test.mp4";
        //捕捉屏幕
        //string videoPath = "screen://";
        player = new VLCPlayer(width, height, videoPath);

        VLCPlayer.OnProgress += OnProgress;

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
                if (width > 0 && height > 0)
                {
                    texture = new Texture2D((int)width, (int)height, TextureFormat.RGB24, false, false);
                    rawImage.texture = texture;
                }
            }
            else
            {
                VLCPlayer.GetProgress();
                texture.LoadRawTextureData(img);
                texture.Apply(false);
            }
        }
    }

    private void OnProgress(float progress, string time)
    {
        text.text = time;
    }

    void OnDrag(BaseEventData data)
    {
        player.SetPosition(slider.value);
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

    private void OnDestroy()
    {
        player?.Dispose();
    }
}