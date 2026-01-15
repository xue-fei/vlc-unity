using UnityEditor;
using UnityEngine;

namespace VLC
{
    [CustomEditor(typeof(MediaPlayerUGUI))]
    public class MediaPlayerUGUIEditor : Editor
    {
        private MediaPlayerUGUI mediaPlayer;
        private float currentProgress;
        private bool isDragging = false;

        private void OnEnable()
        {
            mediaPlayer = (MediaPlayerUGUI)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (Application.isPlaying && mediaPlayer != null)
            {
                if (!isDragging)
                {
                    currentProgress = mediaPlayer.GetProgress();
                }
                EditorGUI.BeginChangeCheck();
                float newProgress = EditorGUILayout.Slider("Progress", currentProgress, 0f, 1f);
                if (EditorGUI.EndChangeCheck())
                {
                    currentProgress = newProgress;
                    isDragging = true;
                }
                if (isDragging && currentProgress != mediaPlayer.GetProgress())
                {
                    mediaPlayer.SetPosition(currentProgress);
                    isDragging = false;
                }
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Play"))
                {
                    mediaPlayer.Play();
                }
                if (GUILayout.Button("Pause"))
                {
                    mediaPlayer.Pause();
                }
                if (GUILayout.Button("Stop"))
                {
                    mediaPlayer.Stop();
                }
                GUILayout.EndHorizontal();
            }
        }
    }
}