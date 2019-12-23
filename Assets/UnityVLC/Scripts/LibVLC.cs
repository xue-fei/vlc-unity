using System;
using System.Runtime.InteropServices;
using System.Security;

namespace VLC
{
    //定义替代变量
    using libvlc_media_t = IntPtr;
    using libvlc_media_player_t = IntPtr;
    using libvlc_instance_t = IntPtr;
    using Debug = UnityEngine.Debug;

    #region 导入库函数
    [SuppressUnmanagedCodeSecurity]
    internal static class LibVLC
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        private const string pluginName = "libvlc";
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        private const string pluginName = "__Internal";
#endif

        /// <summary>
        /// 创建一个libvlc实例，它是引用计数的
        /// </summary>
        /// <param name="argc"></param>
        /// <param name="argv"></param>
        /// <returns></returns>
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern libvlc_instance_t libvlc_new(int argc, IntPtr argv);

        /// <summary>
        /// 释放libvlc实例
        /// </summary>
        /// <param name="libvlc_instance"></param>
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void libvlc_release(libvlc_instance_t libvlc_instance);

        /// <summary>
        /// 获取libvlc的版本
        /// </summary>
        /// <returns></returns>
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern IntPtr libvlc_get_version();

        /// <summary>
        /// 从视频来源(例如http、rtsp)构建一个libvlc_meida
        /// </summary>
        /// <param name="libvlc_instance"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern libvlc_media_t libvlc_media_new_location(libvlc_instance_t libvlc_instance, IntPtr path);

        /// <summary>
        /// 从本地文件路径构建一个libvlc_media
        /// </summary>
        /// <param name="libvlc_instance"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern libvlc_media_t libvlc_media_new_path(libvlc_instance_t libvlc_instance, IntPtr path);

        /// <summary>
        /// 释放libvlc_media
        /// </summary>
        /// <param name="libvlc_media_inst"></param>
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void libvlc_media_release(libvlc_media_t libvlc_media_inst);

        /// <summary>
        /// 创建一个空的播放器
        /// </summary>
        /// <param name="libvlc_instance"></param>
        /// <returns></returns>
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern libvlc_media_player_t libvlc_media_player_new(libvlc_instance_t libvlc_instance);

        /// <summary>
        /// 从libvlc_media构建播放器
        /// </summary>
        /// <param name="libvlc_media"></param>
        /// <returns></returns>
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern libvlc_media_player_t libvlc_media_player_new_from_media(libvlc_media_t libvlc_media);

        /// <summary>
        /// 释放播放器资源
        /// </summary>
        /// <param name="libvlc_mediaplayer"></param>
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void libvlc_media_player_release(libvlc_media_player_t libvlc_mediaplayer);

        /// <summary>
        /// 将视频(libvlc_media)绑定到播放器上
        /// </summary>
        /// <param name="libvlc_media_player"></param>
        /// <param name="libvlc_media"></param>
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void libvlc_media_player_set_media(libvlc_media_player_t libvlc_media_player, libvlc_media_t libvlc_media);

        /// <summary>
        /// 参数设置
        /// </summary>
        /// <param name="libvlc_media_player"></param>
        /// <param name="options"></param>
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void libvlc_media_add_option(libvlc_media_player_t libvlc_media_player, IntPtr options);

        /// <summary>
        /// 获取播放信息
        /// </summary>
        /// <param name="libvlc_media_player"></param>
        /// <param name="libvlc_media"></param>
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void libvlc_media_tracks_get(libvlc_media_player_t libvlc_media_player, libvlc_media_t libvlc_media);

        /// <summary>
        /// 设置编码
        /// </summary>
        /// <param name="libvlc_media_player"></param>
        /// <param name="chroma"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="pitch"></param>
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void libvlc_video_set_format(libvlc_media_player_t libvlc_media_player, IntPtr chroma, int width, int height, int pitch);

        /// <summary>
        /// 视频每一帧的数据信息
        /// </summary>
        /// <param name="libvlc_mediaplayer"></param>
        /// <param name="lockCB"></param>
        /// <param name="unlockCB"></param>
        /// <param name="displayCB"></param>
        /// <param name="opaque"></param>
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void libvlc_video_set_callbacks(libvlc_media_player_t libvlc_mediaplayer, VideoLock lockCB, VideoUnlock unlockCB, VideoDisplay displayCB, IntPtr opaque);

        /// <summary>
        /// 设置图像输出的窗口
        /// </summary>
        /// <param name="libvlc_mediaplayer"></param>
        /// <param name="drawable"></param>
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void libvlc_media_player_set_hwnd(libvlc_media_player_t libvlc_mediaplayer, Int32 drawable);

        /// <summary>
        /// 播放器播放
        /// </summary>
        /// <param name="libvlc_mediaplayer"></param>
        /// <returns></returns>
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern int libvlc_media_player_play(libvlc_media_player_t libvlc_mediaplayer);

        /// <summary>
        /// 播放器暂停
        /// </summary>
        /// <param name="libvlc_mediaplayer"></param>
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void libvlc_media_player_pause(libvlc_media_player_t libvlc_mediaplayer);

        /// <summary>
        /// 播放器停止
        /// </summary>
        /// <param name="libvlc_mediaplayer"></param>
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void libvlc_media_player_stop(libvlc_media_player_t libvlc_mediaplayer);

        /// <summary>
        /// 解析视频资源的媒体信息(如时长等)
        /// </summary>
        /// <param name="libvlc_media"></param>
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void libvlc_media_parse(libvlc_media_t libvlc_media);

        /// <summary>
        /// 获取视频宽度
        /// </summary>
        /// <param name="libvlc_mediaplayer"></param>
        /// <returns></returns>
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern Int32 libvlc_video_get_width(libvlc_media_player_t libvlc_mediaplayer);

        /// <summary>
        /// 获取视频高度
        /// </summary>
        /// <param name="libvlc_mediaplayer"></param>
        /// <returns></returns>
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern Int32 libvlc_video_get_height(libvlc_media_player_t libvlc_mediaplayer);

        /// <summary>
        /// 返回视频的时长(毫秒)(必须先调用libvlc_media_parse之后，该函数才会生效)
        /// </summary>
        /// <param name="libvlc_media"></param>
        /// <returns></returns>
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern Int64 libvlc_media_get_duration(libvlc_media_t libvlc_media);

        /// <summary>
        /// 当前播放时间
        /// </summary>
        /// <param name="libvlc_mediaplayer"></param>
        /// <returns></returns>
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern Int64 libvlc_media_player_get_time(libvlc_media_player_t libvlc_mediaplayer);

        /// <summary>
        /// 设置播放时间
        /// </summary>
        /// <param name="libvlc_mediaplayer"></param>
        /// <param name="time"></param>
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void libvlc_media_player_set_time(libvlc_media_player_t libvlc_mediaplayer, Int64 time);

        /// <summary>
        /// 获取音量
        /// </summary>
        /// <param name="libvlc_media_player"></param>
        /// <returns></returns>
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern int libvlc_audio_get_volume(libvlc_media_player_t libvlc_media_player);

        /// <summary>
        /// 设置音量
        /// </summary>
        /// <param name="libvlc_media_player"></param>
        /// <param name="volume"></param>
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void libvlc_audio_set_volume(libvlc_media_player_t libvlc_media_player, int volume);

        /// <summary>
        /// 设置全屏
        /// </summary>
        /// <param name="libvlc_media_player"></param>
        /// <param name="isFullScreen"></param>
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void libvlc_set_fullscreen(libvlc_media_player_t libvlc_media_player, int isFullScreen);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern int libvlc_get_fullscreen(libvlc_media_player_t libvlc_media_player);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void libvlc_toggle_fullscreen(libvlc_media_player_t libvlc_media_player);

        /// <summary>
        /// 判断播放时是否在播放
        /// </summary>
        /// <param name="libvlc_media_player"></param>
        /// <returns></returns>
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern bool libvlc_media_player_is_playing(libvlc_media_player_t libvlc_media_player);

        /// <summary>
        /// 判断播放时是否能够Seek
        /// </summary>
        /// <param name="libvlc_media_player"></param>
        /// <returns></returns>
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern bool libvlc_media_player_is_seekable(libvlc_media_player_t libvlc_media_player);

        /// <summary>
        /// 判断播放时是否能够Pause
        /// </summary>
        /// <param name="libvlc_media_player"></param>
        /// <returns></returns>
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern bool libvlc_media_player_can_pause(libvlc_media_player_t libvlc_media_player);

        /// <summary>
        /// 判断播放器是否可以播放
        /// </summary>
        /// <param name="libvlc_media_player"></param>
        /// <returns></returns>
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern int libvlc_media_player_will_play(libvlc_media_player_t libvlc_media_player);

        /// <summary>
        /// 进行快照
        /// </summary>
        /// <param name="libvlc_media_player"></param>
        /// <param name="num"></param>
        /// <param name="filepath"></param>
        /// <param name="i_width"></param>
        /// <param name="i_height"></param>
        /// <returns></returns>
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern int libvlc_video_take_snapshot(libvlc_media_player_t libvlc_media_player, int num, char[] filepath, int i_width, int i_height);

        /// <summary>
        /// 获取Media信息
        /// </summary>
        /// <param name="libvlc_media_player"></param>
        /// <returns></returns>
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern libvlc_media_t libvlc_media_player_get_media(libvlc_media_player_t libvlc_media_player);

        /// <summary>
        /// 获取媒体信息
        /// </summary>
        /// <param name="libvlc_media"></param>
        /// <param name="lib_vlc_media_stats"></param>
        /// <returns></returns>
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern int libvlc_media_get_stats(libvlc_media_t libvlc_media, ref libvlc_media_stats_t lib_vlc_media_stats);

        /// <summary>
        /// 设置播放进度
        /// </summary>
        /// <param name="libvlc_media"></param>
        /// <param name="posf"></param>
        /// <returns></returns>
        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern int libvlc_media_player_set_position(libvlc_media_t libvlc_media, float posf);
    }
    #endregion

    #region 结构体
    public struct libvlc_media_stats_t
    {
        /* Input */
        public int i_read_bytes;
        public float f_input_bitrate;

        /* Demux */
        public int i_demux_read_bytes;
        public float f_demux_bitrate;
        public int i_demux_corrupted;
        public int i_demux_discontinuity;

        /* Decoders */
        public int i_decoded_video;
        public int i_decoded_audio;

        /* Video Output */
        public int i_displayed_pictures;
        public int i_lost_pictures;

        /* Audio output */
        public int i_played_abuffers;
        public int i_lost_abuffers;

        /* Stream output */
        public int i_sent_packets;
        public int i_sent_bytes;
        public float f_send_bitrate;
    }

    internal struct libvlc_media_track_t
    {
        public uint i_codec;
        public uint i_original_fourcc;
        public int i_id;
        public libvlc_track_type_t i_type;
        public int i_profile;
        public int i_level;
    }
    #endregion

    internal enum libvlc_track_type_t
    {
        libvlc_track_unknown,
        libvlc_track_audio,
        libvlc_track_video,
        libvlc_track_text
    }

    public delegate IntPtr VideoLock(IntPtr opaque, IntPtr planes);
    public delegate void VideoDisplay(IntPtr opaque, IntPtr picture);
    public delegate void VideoUnlock(IntPtr opaque, IntPtr picture, IntPtr planes);
}