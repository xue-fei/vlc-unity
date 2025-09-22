using UnityEditor; 

namespace VLC
{
    [CustomEditor(typeof(MediaPlayerUGUI))]
    public class MediaPlayerUGUIEditor : Editor
    {
        MediaPlayerUGUI mediaPlayer;
        float progress;

        private void OnEnable()
        {
            mediaPlayer = (MediaPlayerUGUI)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI(); 
            progress = EditorGUILayout.Slider(progress, 0.0f, 1.0f);
            mediaPlayer.SetPosition(progress);
            //progress = mediaPlayer.GetProgress();
        }
    }
}