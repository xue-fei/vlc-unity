using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace VLC
{
    [RequireComponent(typeof(Image))]
    public class MediaPlayerUGUI : MonoBehaviour
    {
        private Image image;
        public string videoPath;
        public bool autoPlay = false;
        private VLCPlayer player;
        private Texture2D texture;
        private uint width = 0;
        private uint height = 0;
        private AspectRatioFitter aspectRatio;
        private float progress;

        private void Awake()
        {
            Loom.Initialize();

            image = GetComponent<Image>();
            aspectRatio = GetComponent<AspectRatioFitter>();
            if (aspectRatio == null)
            {
                aspectRatio = gameObject.AddComponent<AspectRatioFitter>();
            }

            player = new VLCPlayer();
            player.Init(width, height, videoPath);
        }

        // Start is called before the first frame update
        void Start()
        {
            if (autoPlay)
            {
                Play();
            }
        }

        public void Play()
        {
            player.Play();
            StartCoroutine(GetSize());
        }

        public void SetPosition(float progress)
        {
            player?.SetPosition(progress);
        }

        public float GetProgress()
        {
            return player.GetProgress();
        }

        public void Pause()
        {
            player.Pause();
        }

        public void Stop()
        {
            player.Stop();
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
                        image.material = new Material(Shader.Find("Custom/SingleShader"));
                        image.material.mainTexture = texture;
                        image.SetMaterialDirty();
                        image.SetNativeSize();
                    }
                }
                else
                {
                    if (width > 0 && height > 0 && img != null)
                    {
                        player.GetProgress(OnProgress);
                        texture.LoadRawTextureData(img);
                        texture.Apply(false);
                        image.material.mainTexture = texture;
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
                    image.SetNativeSize();
                    aspectRatio.aspectRatio = (float)width / (float)height;
                });
                if (width > 0 && height > 0)
                {
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
                yield return new WaitForSeconds(0.2f);
            }
            if (player.GetSize() == 0)
            {
                player.GetSize((w, h) =>
                {
                    width = w;
                    height = h;
                    image.SetNativeSize();
                    aspectRatio.aspectRatio = (float)width / (float)height;
                });
                player.SetFormat();
                Loom.RunAsync(() =>
                {
                    player.Stop();
                    player.Play();
                });
                image.sprite = null;
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
}