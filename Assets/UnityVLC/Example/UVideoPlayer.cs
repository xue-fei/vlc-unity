using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using VLC;

public class UVideoPlayer : MonoBehaviour
{
    public Image image;
    public string videoPath;
    private VLCPlayer player;
    public Texture2D texture;
    private uint width = 0;
    private uint height = 0;

    public Button btnPlay;

    // Start is called before the first frame update
    void Start()
    {
        Loom.Initialize();
        player = new VLCPlayer(width, height, videoPath);
        StartCoroutine(Play());
    }

    IEnumerator Play()
    {
        yield return new WaitForSeconds(1f);
        player.Play();
        StartCoroutine(GetSize());
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
                    image.material.mainTexture = texture;
                    image.SetMaterialDirty();
                }
            }
            else
            {
                if (width > 0 && height > 0)
                {
                    VLCPlayer.GetProgress(OnProgress);
                    texture.LoadRawTextureData(img);
                    texture.Apply(false);
                    //image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                }
            }
        }
    }

    private void OnProgress(float progress, string time)
    {
        //text.text = time;
    }

    IEnumerator GetSize()
    {
        float time = Time.time;
        while (player.GetSize() == -1)
        {
            player.GetSize((w, h) =>
            {
                width = w;
                height = h;
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
                player.Stop();
                Debug.LogWarning("无法播放");
                break;
            }
            yield return new WaitForSeconds(0.1f);
        }
        if (player.GetSize() == 0)
        {
            player.GetSize((w, h) =>
            {
                width = w;
                height = h;
            });
            player.SetFormat();
            player.Play();
        }
        yield return null;
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