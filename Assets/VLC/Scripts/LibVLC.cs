using System;
using System.Runtime.InteropServices;

namespace VLC
{
    /// <summary>
    /// https://www.videolan.org/developers/vlc/doc/doxygen/html/group__libvlc.html
    /// http://nightlies.videolan.org/
    /// </summary>
    public static class LibVLC
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        private const string pluginName = "libvlc";
#elif UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
        private const string pluginName = "libvlc.so";
#elif UNITY_ANDROID && !UNITY_EDITOR
        private const string pluginName = "libvlc.so";
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        private const string pluginName = "libvlc";
#endif
        [DllImport("libX11", CallingConvention = CallingConvention.Cdecl)]
        public static extern int XInitThreads();

        [DllImport(pluginName)]
        internal static extern IntPtr libvlc_get_version();

        [DllImport(pluginName)]
        internal static extern IntPtr libvlc_new(int argc, params string[] args);

        [DllImport(pluginName)]
        internal static extern void libvlc_release(IntPtr libvlc_instance);

        [DllImport(pluginName)]
        internal static extern int libvlc_media_tracks_get(IntPtr media, out IntPtr ppTracks);

        [DllImport(pluginName)]
        internal static extern void libvlc_media_tracks_release(IntPtr tracks, int i_count);

        [DllImport(pluginName)]
        internal static extern IntPtr libvlc_media_player_new(IntPtr libvlc_instance);

        [DllImport(pluginName)]
        internal static extern IntPtr libvlc_media_new_path(IntPtr libvlc_instance, char[] path);

        [DllImport(pluginName)]
        internal static extern IntPtr libvlc_media_new_path(IntPtr libvlc_instance, string path);

        [DllImport(pluginName)]
        internal static extern IntPtr libvlc_media_new_location(IntPtr libvlc_instance, string path);

        [DllImport(pluginName)]
        internal static extern void libvlc_media_add_option(IntPtr media, params string[] args);

        [DllImport(pluginName)]
        internal static extern libvlc_state_t libvlc_media_get_state(IntPtr media);

        [DllImport(pluginName)]
        internal static extern libvlc_state_t libvlc_media_player_get_state(IntPtr mediaPlayer);

        [DllImport(pluginName)]
        internal static extern IntPtr libvlc_media_player_new_from_media(IntPtr media);

        [DllImport(pluginName)]
        internal static extern IntPtr libvlc_media_player_event_manager(IntPtr mediaPlayer);

        [DllImport(pluginName)]
        internal static extern int libvlc_event_attach(IntPtr p_event_manager, libvlc_event_e i_event_type, libvlc_callback_t f_callback, IntPtr user_data);

        [DllImport(pluginName)]
        internal static extern void libvlc_event_detach(IntPtr p_event_manager, libvlc_event_e i_event_type, libvlc_callback_t f_callback, IntPtr p_user_data);

        [DllImport(pluginName)]
        internal static extern bool libvlc_media_player_can_pause(IntPtr mediaPlayer);

        [DllImport(pluginName)]
        internal static extern void libvlc_media_player_pause(IntPtr mediaPlayer);

        [DllImport(pluginName)]
        internal static extern int libvlc_media_player_play(IntPtr mediaPlayer);

        [DllImport(pluginName)]
        internal static extern int libvlc_media_player_stop(IntPtr mediaPlayer);

        [DllImport(pluginName)]
        internal static extern int libvlc_media_player_stop_async(IntPtr mediaPlayer);

        [DllImport(pluginName)]
        internal static extern Int64 libvlc_media_get_duration(IntPtr media);

        [DllImport(pluginName)]
        internal static extern Int64 libvlc_media_player_get_time(IntPtr mediaPlayer);

        [DllImport(pluginName)]
        internal static extern int libvlc_media_player_set_position(IntPtr mediaPlayer, float f_pos, bool b_fast);

        [DllImport(pluginName)]
        internal static extern void libvlc_media_player_release(IntPtr mediaPlayer);

        [DllImport(pluginName)]
        internal static extern void libvlc_media_release(IntPtr media);

        [DllImport(pluginName)]
        internal static extern void libvlc_media_player_set_hwnd(IntPtr mediaPlayer, IntPtr drawable);

        [DllImport(pluginName)]
        internal static extern void libvlc_media_player_set_media(IntPtr mediaPlayer, IntPtr media);

        [DllImport(pluginName)]
        internal static extern void libvlc_video_set_format(IntPtr mediaPlayer, string chroma, uint width, uint height, uint pitch);

        [DllImport(pluginName)]
        internal static extern void libvlc_video_set_format_callbacks(IntPtr mediaPlayer, libvlc_video_format_cb setup, libvlc_video_cleanup_cb cleanup);

        [DllImport(pluginName)]
        internal static extern void libvlc_video_set_callbacks(IntPtr mediaPlayer, libvlc_video_lock_cb _lock, libvlc_video_unlock_cb _unlock, libvlc_video_display_cb _display, IntPtr _opaque);

        [DllImport(pluginName)]
        internal static extern int libvlc_video_get_size(IntPtr mediaPlayer, uint num, ref uint width, ref uint height);

        [DllImport(pluginName)]
        internal static extern bool libvlc_media_player_is_playing(IntPtr mediaPlayer);

        [DllImport(pluginName)]
        internal static extern void libvlc_media_player_set_pause(IntPtr mediaPlayer, int do_pause);

        [DllImport(pluginName)]
        internal static extern int libvlc_media_parse_with_options(IntPtr mediaPlayer, libvlc_media_parse_flag_t parse_flag, int timeout);

        //LibVLC 4.0.0 or later
        [DllImport(pluginName)]
        internal static extern int libvlc_media_parse_request(IntPtr mediaPlayer, libvlc_media_parse_flag_t parse_flag, int timeout);

        [DllImport(pluginName)]
        internal static extern int libvlc_audio_set_volume(IntPtr mediaPlayer, int i_volume);
    }

    public enum libvlc_media_parse_flag_t
    {
        libvlc_media_parse_local,
        libvlc_media_parse_network,
        libvlc_media_fetch_local,
        libvlc_media_fetch_network,
        libvlc_media_do_interact
    }

    public enum libvlc_state_t
    {
        libvlc_NothingSpecial,
        libvlc_Opening,
        libvlc_Buffering,
        libvlc_Playing,
        libvlc_Paused,
        libvlc_Stopped,
        libvlc_Ended,
        libvlc_Error
    }

    public enum libvlc_track_type_t
    {
        libvlc_track_unknown = -1,
        libvlc_track_audio = 0,
        libvlc_track_video = 1,
        libvlc_track_text = 2
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct libvlc_media_track_t
    {
        public uint i_codec;
        public uint i_original_fourcc;
        public int i_id;
        public libvlc_track_type_t i_type;
        public int i_profile;
        public int i_level;
        public IntPtr media;
        public uint i_bitrate;
        public IntPtr psz_language;
        public IntPtr psz_description;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct libvlc_video_track_t
    {
        public uint i_height;
        public uint i_width;
        public uint i_sar_num;
        public uint i_sar_den;
        public uint i_frame_rate_num;
        public uint i_frame_rate_den;
        public uint i_orientation;
        public uint i_projection;
        public IntPtr pose;
        public uint i_multiview;
    }

    public delegate IntPtr libvlc_video_format_cb(IntPtr opaque, string chroma, uint width, uint height, uint pitches, uint lines);
    public delegate IntPtr libvlc_video_lock_cb(IntPtr opaque, ref IntPtr planes);
    public delegate void libvlc_video_cleanup_cb(IntPtr opaque);
    public delegate void libvlc_video_display_cb(IntPtr opaque, IntPtr picture);
    public delegate void libvlc_video_unlock_cb(IntPtr opaque, IntPtr picture, ref IntPtr planes);
    public delegate void libvlc_callback_t(libvlc_event_t p_event, IntPtr p_data);
}