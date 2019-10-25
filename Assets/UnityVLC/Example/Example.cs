using System;
using UnityEngine;
using UnityEngine.UI;
using VLC;
using Debug = UnityEngine.Debug;

public class Example : MonoBehaviour
{
    UnityVLCPlayer unityVLCPlayer;
    public Material matVideo;
    public Slider slider;
    public Text text;
    public Button btnStart;
    public Button btnPause;
    public Button btnStop;

    // Use this for initialization
    void Start()
    {
        unityVLCPlayer = gameObject.AddComponent<UnityVLCPlayer>();
        unityVLCPlayer.Init();
        unityVLCPlayer.OnProgress += OnProgress;
        //湖南卫视直播地址
        //string videoPath = "rtmp://58.200.131.2:1935/livetv/hunantv";
        //本地视频地址
        string videoPath = "file:///" + Application.streamingAssetsPath + "/test.mp4";

        unityVLCPlayer.SetLocation(videoPath, matVideo);
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

        slider.onValueChanged.AddListener(ChangValue);
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 100, 100), "Take"))
        {
            string fileName = DateTime.Now.ToFileTime() + ".jpg";
            unityVLCPlayer.TakeSnapShot(Application.streamingAssetsPath + "/" + fileName);
        }
    }

    private void OnProgress(float progress, string time)
    {
        slider.value = progress;
        text.text = time;
    }

    private void ChangValue(float value)
    {
        unityVLCPlayer.SetProgress(value);
    }

    private void OnCtrl(Button button)
    {
        if (button.name == btnStart.name)
        {
            if (!unityVLCPlayer.IsPlaying())
            {
                unityVLCPlayer.Play(); ;
            }
        }
        if (button.name == btnPause.name)
        {
            if (unityVLCPlayer.IsPlaying())
            {
                btnPause.transform.Find("Text").GetComponent<Text>().text = "继续";
                unityVLCPlayer.Pause();
            }
            if (!unityVLCPlayer.IsPlaying())
            {
                btnPause.transform.Find("Text").GetComponent<Text>().text = "暂停";
                unityVLCPlayer.Play();
            }
        }
        if (button.name == btnStop.name)
        {
            if (unityVLCPlayer.IsPlaying())
            {
                unityVLCPlayer.Stop();
            }
        }
    }
}