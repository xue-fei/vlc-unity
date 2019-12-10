using System;
using System.Text;
using System.Runtime.InteropServices;

namespace VLC
{
    //定义替代变量
    using libvlc_media_t = IntPtr;
    using libvlc_media_player_t = IntPtr;
    using libvlc_instance_t = IntPtr;
    using Debug = UnityEngine.Debug;

    public class VLCPlayer
    {
        #region 全局变量 
        /// <summary>
        /// vlc库启动参数配置
        /// </summary>
        private static string pluginPath = @UnityEngine.Application.streamingAssetsPath + "/plugins/";

        private static string plugin_arg = "--plugin-path=" + pluginPath;
        //用于播放节目时，转录节目
        private static string program_arg = "--sout=#duplicate{dst=display,dst=std{access=file,mux=flv,dst=" +
            @UnityEngine.Application.streamingAssetsPath + "/record.flv}}";
        //    + "--vout-filter=transform,--transform-type=hflip";
        //https://www.cnblogs.com/waimai/p/3342739.html  , program_arg
        //private static string program_arg = "--network-caching=1000"; 
        //private static string[] arguments = { "-I", "dummy", "--no-ignore-config", "--no-video-title", plugin_arg }; , "--avcodec-hw=any"
        private static string[] arguments = {
            "-I",
            "dummy",
            "--no-ignore-config",
            "--no-video-title",
            "--verbose=4", 
            //"--ffmpeg-hw",
            //"--video-filter=transform",
            "--transform-type={hflip,vflip}",
            //"--transform-type=vflip",
            //plugin_arg 
        };

        #endregion

        #region 公开函数
        /// <summary>
        /// 创建VLC播放资源索引
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public static libvlc_instance_t Create_Media_Instance()
        {
            libvlc_instance_t libvlc_instance = IntPtr.Zero;
            IntPtr argvPtr = IntPtr.Zero;

            try
            {
                if (arguments.Length == 0 ||
                    arguments == null)
                {
                    return IntPtr.Zero;
                }

                //将string数组转换为指针
                argvPtr = StrToIntPtr(arguments);
                if (argvPtr == null || argvPtr == IntPtr.Zero)
                {
                    return IntPtr.Zero;
                }

                //设置启动参数
                libvlc_instance = LibVLC.libvlc_new(arguments.Length, argvPtr);
                if (libvlc_instance == null || libvlc_instance == IntPtr.Zero)
                {
                    return IntPtr.Zero;
                }

                return libvlc_instance;
            }
            catch
            {
                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// 释放VLC播放资源索引
        /// </summary>
        /// <param name="libvlc_instance">VLC 全局变量</param>
        public static void Release_Media_Instance(libvlc_instance_t libvlc_instance)
        {
            try
            {
                if (libvlc_instance != IntPtr.Zero ||
                    libvlc_instance != null)
                {
                    LibVLC.libvlc_release(libvlc_instance);
                }

                libvlc_instance = IntPtr.Zero;
            }
            catch (Exception)
            {
                libvlc_instance = IntPtr.Zero;
            }
        }

        /// <summary>
        /// 创建VLC播放器
        /// </summary>
        /// <param name="libvlc_instance">VLC 全局变量</param>
        /// <param name="handle">VLC MediaPlayer需要绑定显示的窗体句柄</param>
        /// <returns></returns>
        public static libvlc_media_player_t Create_MediaPlayer(libvlc_instance_t libvlc_instance)
        {
            libvlc_media_player_t libvlc_media_player = IntPtr.Zero;

            try
            {
                if (libvlc_instance == IntPtr.Zero ||
                    libvlc_instance == null)
                {
                    return IntPtr.Zero;
                }

                //创建播放器
                libvlc_media_player = LibVLC.libvlc_media_player_new(libvlc_instance);
                if (libvlc_media_player == null || libvlc_media_player == IntPtr.Zero)
                {
                    return IntPtr.Zero;
                }

                //设置播放窗口            
                //SafeNativeMethods.libvlc_media_player_set_hwnd(libvlc_media_player, (int)handle);

                return libvlc_media_player;
            }
            catch
            {
                LibVLC.libvlc_media_player_release(libvlc_media_player);

                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// 释放媒体播放器
        /// </summary>
        /// <param name="libvlc_media_player">VLC MediaPlayer变量</param>
        public static void Release_MediaPlayer(libvlc_media_player_t libvlc_media_player)
        {
            try
            {
                if (libvlc_media_player != IntPtr.Zero ||
                    libvlc_media_player != null)
                {
                    if (LibVLC.libvlc_media_player_is_playing(libvlc_media_player))
                    {
                        LibVLC.libvlc_media_player_stop(libvlc_media_player);
                    }

                    LibVLC.libvlc_media_player_release(libvlc_media_player);
                }

                libvlc_media_player = IntPtr.Zero;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                libvlc_media_player = IntPtr.Zero;
            }
        }

        /// <summary>
        /// 设置文件路径
        /// </summary>
        /// <param name="libvlc_instance"></param>
        /// <param name="libvlc_media_player"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static bool SetLocation(libvlc_instance_t libvlc_instance, libvlc_media_player_t libvlc_media_player, string url)
        {
            IntPtr pMrl = IntPtr.Zero;
            libvlc_media_t libvlc_media = IntPtr.Zero;

            try
            {
                if (url == null ||
                    libvlc_instance == IntPtr.Zero ||
                    libvlc_instance == null ||
                    libvlc_media_player == IntPtr.Zero ||
                    libvlc_media_player == null)
                {
                    return false;
                }
                Debug.Log("url:" + url);
                pMrl = StrToIntPtr(url);
                if (pMrl == null || pMrl == IntPtr.Zero)
                {
                    return false;
                }

                //播放网络文件
                libvlc_media = LibVLC.libvlc_media_new_location(libvlc_instance, pMrl);

                string[] arguments =
                {
                    ":avcodec-hw=any", 
                    //":vout=direct3d11",
                    //":directx-use-sysmem",
                    //":directx-overlay",
                    //":spect-show-original",
                    //":avcodec-threads=124" 
                    //捕捉屏幕的相关参数
                    //":screen-fps=30",
                    //":screen-width=1920",
                    //":screen-width=1080",
                    //":video-filter=transform",
                    //":transform-type=hflip",
                    //":transform-type=vflip",
                };
                AddOption(libvlc_media, arguments);

                if (libvlc_media == null || libvlc_media == IntPtr.Zero)
                {
                    return false;
                }

                LibVLC.libvlc_media_parse(libvlc_media);
                //long duration = SafeNativeMethods.libvlc_media_get_duration(libvlc_media);
                //Debug.Log("视频长度: " + duration / 1000f + "秒");

                //将Media绑定到播放器上
                LibVLC.libvlc_media_player_set_media(libvlc_media_player, libvlc_media);

                //释放libvlc_media资源
                LibVLC.libvlc_media_release(libvlc_media);

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                //释放libvlc_media资源
                if (libvlc_media != IntPtr.Zero)
                {
                    LibVLC.libvlc_media_release(libvlc_media);
                }
                libvlc_media = IntPtr.Zero;

                return false;
            }
        }

        public static void AddOption(libvlc_media_player_t libvlc_media_player, string[] arguments)
        {
            for (int i = 0; i < arguments.Length; i++)
            {
                IntPtr pMrl = Marshal.StringToHGlobalAnsi(arguments[i]);
                LibVLC.libvlc_media_add_option(libvlc_media_player, pMrl);
                Marshal.FreeHGlobal(pMrl);
            }
        }

        /// <summary>
        /// 播放
        /// </summary>
        /// <param name="libvlc_instance">VLC 全局变量</param>
        /// <param name="libvlc_media_player">VLC MediaPlayer变量</param>
        /// <param name="url">网络视频URL，支持http、rtp、udp等格式的URL播放</param>
        /// <returns></returns>
        public static bool MediaPlayer_Play(libvlc_media_player_t libvlc_media_player)
        {
            try
            {
                if (libvlc_media_player == IntPtr.Zero || libvlc_media_player == null)
                {
                    return false;
                }

                if (0 != LibVLC.libvlc_media_player_play(libvlc_media_player))
                {
                    return false;
                }

                //休眠指定时间
                //Thread.Sleep(500);

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }
        }

        public static void SetFormart(libvlc_media_player_t libvlc_media_player, string chroma, int width, int height, int pitch)
        {
            LibVLC.libvlc_video_set_format(libvlc_media_player, StrToIntPtr(chroma), width, height, pitch);
        }

        public static int GetMediaWidth(libvlc_media_player_t libvlc_media_player)
        {
            int width = LibVLC.libvlc_video_get_width(libvlc_media_player);
            return width;
        }

        public static int GetMediaHeight(libvlc_media_player_t libvlc_media_player)
        {
            int height = LibVLC.libvlc_video_get_height(libvlc_media_player);
            return height;
        }

        public static long GetMediaLength(libvlc_media_player_t libvlc_media_player)
        {
            libvlc_media_t libvlc_media = GetMedia(libvlc_media_player);
            long length = 0;
            if (libvlc_media != IntPtr.Zero)
            {
                length = LibVLC.libvlc_media_get_duration(libvlc_media);
            }
            LibVLC.libvlc_media_release(libvlc_media);
            return length;
        }

        /// <summary>
        /// 暂停或恢复视频
        /// </summary>
        /// <param name="libvlc_media_player">VLC MediaPlayer变量</param>
        /// <returns></returns>
        public static bool MediaPlayer_Pause(libvlc_media_player_t libvlc_media_player)
        {
            try
            {
                if (libvlc_media_player == IntPtr.Zero ||
                    libvlc_media_player == null)
                {
                    return false;
                }

                if (LibVLC.libvlc_media_player_can_pause(libvlc_media_player))
                {
                    LibVLC.libvlc_media_player_pause(libvlc_media_player);

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }
        }

        /// <summary>
        /// 停止播放
        /// </summary>
        /// <param name="libvlc_media_player">VLC MediaPlayer变量</param>
        /// <returns></returns>
        public static bool MediaPlayer_Stop(libvlc_media_player_t libvlc_media_player)
        {
            try
            {
                if (libvlc_media_player == IntPtr.Zero ||
                    libvlc_media_player == null)
                {
                    return false;
                }

                LibVLC.libvlc_media_player_stop(libvlc_media_player);

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }
        }

        /// <summary>
        /// 快进
        /// </summary>
        /// <param name="libvlc_media_player">VLC MediaPlayer变量</param>
        /// <returns></returns>
        public static bool MediaPlayer_Forward(libvlc_media_player_t libvlc_media_player)
        {
            double time = 0;

            try
            {
                if (libvlc_media_player == IntPtr.Zero ||
                    libvlc_media_player == null)
                {
                    return false;
                }

                if (LibVLC.libvlc_media_player_is_seekable(libvlc_media_player))
                {
                    time = LibVLC.libvlc_media_player_get_time(libvlc_media_player) / 1000.0;
                    if (time == -1)
                    {
                        return false;
                    }

                    LibVLC.libvlc_media_player_set_time(libvlc_media_player, (Int64)((time + 30) * 1000));

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }
        }

        /// <summary>
        /// 快退
        /// </summary>
        /// <param name="libvlc_media_player">VLC MediaPlayer变量</param>
        /// <returns></returns>
        public static bool MediaPlayer_Back(libvlc_media_player_t libvlc_media_player)
        {
            double time = 0;

            try
            {
                if (libvlc_media_player == IntPtr.Zero ||
                    libvlc_media_player == null)
                {
                    return false;
                }

                if (LibVLC.libvlc_media_player_is_seekable(libvlc_media_player))
                {
                    time = LibVLC.libvlc_media_player_get_time(libvlc_media_player) / 1000.0;
                    if (time == -1)
                    {
                        return false;
                    }

                    if (time - 30 < 0)
                    {
                        LibVLC.libvlc_media_player_set_time(libvlc_media_player, (Int64)(1 * 1000));
                    }
                    else
                    {
                        LibVLC.libvlc_media_player_set_time(libvlc_media_player, (Int64)((time - 30) * 1000));
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }
        }

        public static Int64 GetPosition(libvlc_media_player_t libvlc_media_player)
        {
            return LibVLC.libvlc_media_player_get_time(libvlc_media_player);
        }

        public static void SetPosition(libvlc_media_player_t libvlc_media_player, float posf)
        {
            LibVLC.libvlc_media_player_set_position(libvlc_media_player, posf);
        }

        /// <summary>
        /// VLC MediaPlayer是否在播放
        /// </summary>
        /// <param name="libvlc_media_player">VLC MediaPlayer变量</param>
        /// <returns></returns>
        public static bool MediaPlayer_IsPlaying(libvlc_media_player_t libvlc_media_player)
        {
            try
            {
                if (libvlc_media_player == IntPtr.Zero ||
                    libvlc_media_player == null)
                {
                    return false;
                }

                return LibVLC.libvlc_media_player_is_playing(libvlc_media_player);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }
        }

        /// <summary>
        /// 录制快照
        /// </summary>
        /// <param name="libvlc_media_player">VLC MediaPlayer变量</param>
        /// <param name="path">快照要存放的路径</param>
        /// <param name="name">快照保存的文件名称</param>
        /// <returns></returns>
        public static bool TakeSnapShot(libvlc_media_player_t libvlc_media_player, string path, string name, int width, int height)
        {
            try
            {
                string snap_shot_path = null;

                if (libvlc_media_player == IntPtr.Zero ||
                    libvlc_media_player == null)
                {
                    Debug.LogError("HERE1");
                    return false;
                }

                snap_shot_path = path + "\\" + name;
                snap_shot_path = snap_shot_path.Replace('/', '\\');
                //snap_shot_path = @"D:\\1.jpg";
                Debug.LogError("snap_shot_path:" + snap_shot_path);

                int code = LibVLC.libvlc_video_take_snapshot(libvlc_media_player, 1, snap_shot_path.ToCharArray(), width, height);
                Debug.LogError("code:" + code);
                if (0 == code)
                {
                    Debug.LogError("HERE2");
                    return true;
                }
                else
                {
                    Debug.LogError("HERE3");
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }
        }

        /// <summary>
        /// 获取信息
        /// </summary>
        /// <param name="libvlc_media_player"></param>
        /// <returns></returns>
        public static libvlc_media_t GetMedia(libvlc_media_player_t libvlc_media_player)
        {
            libvlc_media_t media = IntPtr.Zero;

            try
            {
                if (libvlc_media_player == IntPtr.Zero ||
                    libvlc_media_player == null)
                {
                    return media;
                }

                media = LibVLC.libvlc_media_player_get_media(libvlc_media_player);
                if (media == IntPtr.Zero || media == null)
                {
                    return media;
                }
                else
                {
                    return media;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return media;
            }
        }

        /// <summary>
        /// 获取已经显示的图片数
        /// </summary>
        /// <param name="libvlc_media_player"></param>
        /// <returns></returns>
        public static int GetDisplayedPictures(libvlc_media_player_t libvlc_media_player)
        {
            libvlc_media_t media = IntPtr.Zero;
            libvlc_media_stats_t media_stats = new libvlc_media_stats_t();
            try
            {
                if (libvlc_media_player == IntPtr.Zero ||
                    libvlc_media_player == null)
                {
                    return 0;
                }

                media = LibVLC.libvlc_media_player_get_media(libvlc_media_player);
                if (media == IntPtr.Zero || media == null)
                {
                    return 0;
                }

                if (1 == LibVLC.libvlc_media_get_stats(media, ref media_stats))
                {
                    return media_stats.i_displayed_pictures;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// 设置全屏
        /// </summary>
        /// <param name="libvlc_media_player"></param>
        /// <param name="isFullScreen"></param>
        public static bool SetFullScreen(libvlc_media_player_t libvlc_media_player, int isFullScreen)
        {
            try
            {
                if (libvlc_media_player == IntPtr.Zero ||
                    libvlc_media_player == null)
                {
                    return false;
                }

                LibVLC.libvlc_set_fullscreen(libvlc_media_player, isFullScreen);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 设置回调
        /// </summary>
        /// <param name="libvlc_media_player"></param>
        /// <param name="lockcb"></param>
        /// <param name="unlockcb"></param>
        /// <param name="displaycb"></param>
        /// <param name="opaque"></param>
        public static void SetCallbacks(libvlc_media_player_t libvlc_media_player, VideoLock lockcb, VideoUnlock unlockcb, VideoDisplay displaycb, IntPtr opaque)
        {
            try
            {
                LibVLC.libvlc_video_set_callbacks(libvlc_media_player, lockcb, unlockcb, displaycb, opaque);
                Debug.Log("SetCallbacks");
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        public static string GetVersion()
        {
            return Marshal.PtrToStringAnsi(LibVLC.libvlc_get_version());
        }
        #endregion

        #region 私有函数
        //将string []转换为IntPtr
        public static IntPtr StrToIntPtr(string[] args)
        {
            try
            {
                IntPtr ip_args = IntPtr.Zero;

                PointerToArrayOfPointerHelper argv = new PointerToArrayOfPointerHelper();
                argv.pointers = new IntPtr[args.Length];

                for (int i = 0; i < args.Length; i++)
                {
                    argv.pointers[i] = Marshal.StringToHGlobalAnsi(args[i]);
                }

                int size = Marshal.SizeOf(typeof(PointerToArrayOfPointerHelper));
                ip_args = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(argv, ip_args, false);

                return ip_args;
            }
            catch (Exception)
            {
                return IntPtr.Zero;
            }
        }

        //将string转换为IntPtr
        private static IntPtr StrToIntPtr(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                {
                    return IntPtr.Zero;
                }

                IntPtr pMrl = IntPtr.Zero;
                byte[] bytes = Encoding.UTF8.GetBytes(url);

                pMrl = Marshal.AllocHGlobal(bytes.Length + 1);
                Marshal.Copy(bytes, 0, pMrl, bytes.Length);
                Marshal.WriteByte(pMrl, bytes.Length, 0);

                return pMrl;
            }
            catch (Exception)
            {
                return IntPtr.Zero;
            }
        }

        //数组转换为指针
        internal struct PointerToArrayOfPointerHelper
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
            public IntPtr[] pointers;
        }
        #endregion

    }
}