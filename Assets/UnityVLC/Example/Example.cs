using System.Collections;
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
    private uint width = 0;
    /// <summary>
    /// 视频高
    /// </summary>
    private uint height = 0;
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
        Loom.Initialize();
        string videoPath = "https://img.qunliao.info:443/4oEGX68t_9505974551.mp4";
        //string videoPath = "rtmp://222.222.65.164:10090/hls/EasyGBS1310810000200000011513108100001310731362?sign=PIagfVc7R";
        //string videoPath = "http://devimages.apple.com.edgekey.net/streaming/examples/bipbop_4x3/gear2/prog_index.m3u8";
        //string videoPath = "http://39.134.115.163:8080/PLTV/88888910/224/3221225632/index.m3u8";
        //string videoPath = "http://demo-videos.qnsdk.com/bbk-H265-50fps.mp4";
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
                    Debug.LogWarning("_width:" + width + " _height:" + height);
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

    IEnumerator GetSize()
    {
        float time = Time.time;
        while(player.GetSize()==-1)
        {
            player.GetSize((w,h)=>{
                     width = w;
                     height =h;
                });
            if(width!=0&&height!=0)
            {
                Debug.LogWarning(" _width:" + width + " _height:" + height);
                player.SetFormat();
                break;
            }
            if(Time.time -time>=5f)
            {
                player.Stop();
                break;
            }
        }
        yield return null;
    }

    private void OnCtrl(Button button)
    {
        if (button.name == btnStart.name)
        {
            if (!player.IsPlaying())
            {
                player.Play();
                StartCoroutine(GetSize());
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
                width = 0;
                height = 0;
            }
        }
    }

    private void OnDestroy()
    {
        if (player.IsPlaying())
        {
            player.Stop();
        }
        player?.Dispose();
    }
}