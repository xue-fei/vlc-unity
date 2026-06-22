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

        // ---- 音频 ----
        private AudioSource _audioSource;
        private AudioClip _audioClip;
        private bool _audioClipCreated = false;

        private void Awake()
        {
            Loom.Initialize();

            image = GetComponent<Image>();
            aspectRatio = GetComponent<AspectRatioFitter>();
            if (aspectRatio == null)
            {
                aspectRatio = gameObject.AddComponent<AspectRatioFitter>();
            }

            // ---- 初始化 AudioSource ----
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
                _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.spatialBlend = 0f;
            _audioSource.loop = true;

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
        }

        public void SetPosition(float progress)
        {
            player?.SetPosition(progress);
        }

        public float GetProgress()
        {
            if (player != null)
            {
                return player.GetProgress();
            }
            else
            {
                return 0;
            }
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
            // ---- 首次拿到音频参数时建流式 AudioClip ----
            if (!_audioClipCreated && player != null && player.AudioParamsReady)
            {
                _audioClipCreated = true;
                _audioClip = AudioClip.Create(
                    "VLCAudio",
                    player.AudioSampleRate,
                    player.AudioChannels,
                    player.AudioSampleRate,
                    true,
                    OnAudioRead,
                    OnAudioSetPosition);
                _audioSource.clip = _audioClip;
                _audioSource.Play();
            }

            if (player != null && player.GetVideoImage(out img, out width, out height))
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
                        aspectRatio.aspectRatio = (float)width / (float)height;
                        image.sprite = null;
                    }
                    else
                    {
                        Debug.LogWarning("here");
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

        // ---- 音频线程回调 ----
        private void OnAudioRead(float[] data)
        {
            player?.ReadAudioData(data);
        }

        private void OnAudioSetPosition(int newPosition) { }

        /// <summary>通过 AudioSource.volume 控制音量（0~1）</summary>
        public void SetVolume(float normalizedVolume)
        {
            if (_audioSource != null)
                _audioSource.volume = Mathf.Clamp01(normalizedVolume);
        }

        private void OnDestroy()
        {
            _audioSource?.Stop();
            if (_audioClip != null)
            {
                Destroy(_audioClip);
                _audioClip = null;
            }
            if (player != null)
            {
                if (player.IsPlaying()) player.Stop();
                player.Dispose();
                player = null;
            }
        }
    }
}