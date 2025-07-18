using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VLC;

public class Example : MonoBehaviour
{
    public string videoPath;
    private VLCPlayer player = null;
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
    public AspectRatioFitter aspectRatio;
    public InputField inputField;
    public Slider sliderVolume;

    // Use this for initialization
    void Start()
    {
        Loom.Initialize();
        //videoPath = "https://img.qunliao.info:443/4oEGX68t_9505974551.mp4";
        //videoPath = "http://devimages.apple.com.edgekey.net/streaming/examples/bipbop_4x3/gear2/prog_index.m3u8";
        // videoPath = "http://39.134.115.163:8080/PLTV/88888910/224/3221225632/index.m3u8";
        // videoPath = "http://demo-videos.qnsdk.com/bbk-H265-50fps.mp4";
        // videoPath = "rtsp://127.0.0.1:8554/stream";
        //本地视频
        // videoPath = @"file:///" + Application.streamingAssetsPath + "/test.mp4";
        //捕捉屏幕
        // videoPath = "screen://";

        btnStart.onClick.AddListener(delegate ()
        {
            if (string.IsNullOrEmpty(inputField.text))
            {
                return;
            }
            if (!string.Equals(videoPath, inputField.text))
            {
                if (player != null)
                {
                    Dispose();
                }
                videoPath = inputField.text;
                player = new VLCPlayer();
                player.Init(width, height, inputField.text);
            }
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

        sliderVolume.onValueChanged.AddListener(OnVolume);
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
                if (width > 0 && height > 0)
                {
                    player.GetProgress(OnProgress);
                    texture.LoadRawTextureData(img);
                    texture.Apply(false);
                }
            }
        }
    }

    private void OnProgress(float progress, string time)
    {
        text.text = time;
        slider.value = progress;
    }

    void OnDrag(BaseEventData data)
    {
        player.SetPosition(slider.value);
    }

    IEnumerator GetSize()
    {
        float time = Time.time;
        bool timeout = false;
        while (!timeout)
        {
            player.GetSize((w, h) =>
            {
                width = w;
                height = h;
                aspectRatio.aspectRatio = (float)width / (float)height;
            });
            if (width > 0 && height > 0)
            {
                Debug.LogWarning(" _width:" + width + " _height:" + height);
                player.SetFormat();
                player.Play();
                break;
            }
            if (Time.time - time >= 5f)
            {
                timeout = true; 
                Dispose(); 
                Debug.LogWarning("无法播放");
                break;
            }
            Debug.Log(Time.time);
            yield return new WaitForSeconds(0.5f);
        }
        if (player != null && player.GetSize() == 0)
        {
            player.GetSize((w, h) =>
            {
                width = w;
                height = h;
                aspectRatio.aspectRatio = (float)width / (float)height;
            });
            player.SetFormat();
            player.Stop();
            player.Play();
            yield break;
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
                Dispose();
            }
        }
    }

    private void OnVolume(float volume)
    {
        player.SetVolume((int)volume);
    }

    private void Dispose()
    {
        if (player != null)
        {
            if (player.IsPlaying())
            {
                player.Stop();
            }
            player.Dispose();
            player = null;
            if (texture != null)
            {
                DestroyImmediate(texture);
            }
            width = 0;
            height = 0;
            videoPath = "";
        }
    }

    private void OnDestroy()
    {
        Dispose();
    }
}