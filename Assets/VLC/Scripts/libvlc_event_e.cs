using System;
using System.Runtime.InteropServices;

namespace VLC
{
    public enum libvlc_event_e : int
    {
        libvlc_MediaMetaChanged =0 , 
        libvlc_MediaSubItemAdded , 
        libvlc_MediaDurationChanged , 
        libvlc_MediaParsedChanged ,
        libvlc_MediaFreed = 4,
        libvlc_MediaStateChanged = 5,
        libvlc_MediaSubItemTreeAdded = 6, 
        libvlc_MediaThumbnailGenerated , 
        libvlc_MediaAttachedThumbnailsFound , 
        libvlc_MediaPlayerMediaChanged = 256,
        libvlc_MediaPlayerNothingSpecial , 
        libvlc_MediaPlayerOpening , 
        libvlc_MediaPlayerBuffering , 
        libvlc_MediaPlayerPlaying ,
        libvlc_MediaPlayerPaused , 
        libvlc_MediaPlayerStopped , 
        libvlc_MediaPlayerForward , 
        libvlc_MediaPlayerBackward ,
        libvlc_MediaPlayerStopping , 
        libvlc_MediaPlayerEncounteredError , 
        libvlc_MediaPlayerTimeChanged , 
        libvlc_MediaPlayerPositionChanged ,
        libvlc_MediaPlayerSeekableChanged , 
        libvlc_MediaPlayerPausableChanged , 
        libvlc_MediaPlayerTitleChanged = 271,
        libvlc_MediaPlayerSnapshotTaken = 272, 
        libvlc_MediaPlayerLengthChanged ,
        libvlc_MediaPlayerVout , 
        libvlc_MediaPlayerScrambledChanged = 275,
        libvlc_MediaPlayerESAdded = 276, 
        libvlc_MediaPlayerESDeleted , 
        libvlc_MediaPlayerESSelected ,
        libvlc_MediaPlayerCorked , 
        libvlc_MediaPlayerUncorked , 
        libvlc_MediaPlayerMuted , 
        libvlc_MediaPlayerUnmuted ,
        libvlc_MediaPlayerAudioVolume , 
        libvlc_MediaPlayerAudioDevice ,  
        libvlc_MediaPlayerChapterChanged , 
        libvlc_MediaPlayerRecordChanged , 
        libvlc_MediaListItemAdded = 512,
        libvlc_MediaListWillAddItem , 
        libvlc_MediaListItemDeleted , 
        libvlc_MediaListWillDeleteItem , 
        libvlc_MediaListEndReached ,
        libvlc_MediaListViewItemAdded = 768, 
        libvlc_MediaListViewWillAddItem , 
        libvlc_MediaListViewItemDeleted , 
        libvlc_MediaListViewWillDeleteItem ,
        libvlc_MediaListPlayerPlayed = 1024, 
        libvlc_MediaListPlayerNextItemSet , 
        libvlc_MediaListPlayerStopped , 
        MediaDiscovererStarted = 1280,
        MediaDiscovererStopped = 1281,
        libvlc_RendererDiscovererItemAdded = 1282,
        libvlc_RendererDiscovererItemDeleted , 
        libvlc_VlmMediaAdded = 1536,
        libvlc_VlmMediaRemoved = 1537,
        libvlc_VlmMediaChanged = 1538,
        libvlc_VlmMediaInstanceStarted = 1539,
        libvlc_VlmMediaInstanceStopped = 1540,
        libvlc_VlmMediaInstanceStatusInit = 1541,
        libvlc_VlmMediaInstanceStatusOpening = 1542,
        libvlc_VlmMediaInstanceStatusPlaying = 1543,
        libvlc_VlmMediaInstanceStatusPause = 1544,
        libvlc_VlmMediaInstanceStatusEnd = 1545,
        libvlc_VlmMediaInstanceStatusError = 1546,
    }
 
    [StructLayout(LayoutKind.Sequential)]
    public struct libvlc_event_t
    { 
        public libvlc_event_e type;
 
        public IntPtr p_obj;

        public EventUnion Union;

        [StructLayout(LayoutKind.Explicit)]
        public struct EventUnion
        {
            [FieldOffset(0)]
            public media_meta_changed media_meta_changed;
            [FieldOffset(0)]
            public media_subitem_added media_subitem_added;
            [FieldOffset(0)]
            public media_duration_changed media_duration_changed;
            [FieldOffset(0)]
            public media_parsed_changed media_parsed_changed;
            [FieldOffset(0)]
            public media_freed media_freed;
            [FieldOffset(0)]
            public media_state_changed media_state_changed;
            [FieldOffset(0)]
            public media_subitemtree_added MediaSubItemTreeAdded;

            // mediaplayer
            [FieldOffset(0)]
            public media_player_buffering media_player_buffering;
            [FieldOffset(0)]
            public media_player_chapter_changed MediaPlayerChapterChanged;
            [FieldOffset(0)]
            public media_player_position_changed MediaPlayerPositionChanged;
            [FieldOffset(0)]
            public media_player_time_changed MediaPlayerTimeChanged;
            [FieldOffset(0)]
            public media_player_title_changed MediaPlayerTitleChanged;
            [FieldOffset(0)]
            public media_player_seekable_changed MediaPlayerSeekableChanged;
            [FieldOffset(0)]
            public media_player_pausable_changed MediaPlayerPausableChanged;
            [FieldOffset(0)]
            public media_player_scrambled_changed MediaPlayerScrambledChanged;
            [FieldOffset(0)]
            public media_player_vout_changed MediaPlayerVoutChanged;

            // medialist
            [FieldOffset(0)]
            public media_list_item_added MediaListItemAdded;
            [FieldOffset(0)]
            public media_list_will_add_item MediaListWillAddItem;
            [FieldOffset(0)]
            public media_list_item_deleted MediaListItemDeleted;
            [FieldOffset(0)]
            public media_list_will_delete_item MediaListWillDeleteItem;
            [FieldOffset(0)]
            public media_list_player_next_item_set MediaListPlayerNextItemSet;

            // mediaplayer
            [FieldOffset(0)]
            public media_player_snapshot_taken MediaPlayerSnapshotTaken;
            [FieldOffset(0)]
            public media_player_length_changed MediaPlayerLengthChanged;
            [FieldOffset(0)]
            public vlm_media_event VlmMediaEvent;
            [FieldOffset(0)]
            public media_player_media_changed MediaPlayerMediaChanged;
            [FieldOffset(0)]
            public es_changed EsChanged;
            [FieldOffset(0)]
            public volume_changed MediaPlayerVolumeChanged;
            [FieldOffset(0)]
            public audio_device_changed AudioDeviceChanged;
            // renderer discoverer
            [FieldOffset(0)]
            public renderer_discoverer_item_added RendererDiscovererItemAdded;
            [FieldOffset(0)]
            public renderer_discoverer_item_deleted RendererDiscovererItemDeleted;
        }

        #region Media
        [StructLayout(LayoutKind.Sequential)]
        public struct media_meta_changed
        {
            public libvlc_meta_t meta_type;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct media_subitem_added
        {
            public IntPtr new_child;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct media_duration_changed
        {
            public long new_duration;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct media_parsed_changed
        {
            public int new_status;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct media_freed
        {
            public IntPtr md;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct media_state_changed
        {
            public libvlc_state_t new_state;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct media_subitemtree_added
        {
            public IntPtr item;
        }
        #endregion

        #region MediaPlayer
        [StructLayout(LayoutKind.Sequential)]
        public struct media_player_buffering
        {
            public float new_cache;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct media_player_chapter_changed
        {
            public int new_chapter;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct media_player_position_changed
        {
            public float new_position;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct media_player_time_changed
        {
            public long new_time;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct media_player_title_changed
        {
            public int new_title;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct media_player_seekable_changed
        {
            public int new_seekable;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct media_player_pausable_changed
        {
            public int new_pausable;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct media_player_scrambled_changed
        {
            public int new_scrambled;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct media_player_vout_changed
        {
            public int new_count;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct media_player_snapshot_taken
        {
            public IntPtr psz_filename;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct media_player_length_changed
        {
            public long new_length;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct es_changed
        {
            public tracktype type;
            public int id;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct audio_device_changed
        {
            public IntPtr device;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct media_player_media_changed
        {
            public IntPtr new_media;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct volume_changed
        {
            public float volume;
        }
        #endregion

        #region MediaList
    
        [StructLayout(LayoutKind.Sequential)]
        public struct media_list_item_added
        {
            public IntPtr item;
            public int index;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct media_list_will_add_item
        {
            public IntPtr item;
            public int index;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct media_list_item_deleted
        {
            public IntPtr item;
            public int index;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct media_list_will_delete_item
        {
            public IntPtr item;
            public int index;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct media_list_player_next_item_set
        {
            public IntPtr item;
        }
    
        [StructLayout(LayoutKind.Sequential)]
        public struct vlm_media_event
        {
            public IntPtr psz_media_name;
            public IntPtr psz_instance_name;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct renderer_discoverer_item_added
        {
            public IntPtr item;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct renderer_discoverer_item_deleted
        {
            public IntPtr item;
        }
    
        #endregion

    }

    public enum libvlc_meta_t : int
    {
        libvlc_meta_Title,
        libvlc_meta_Artist,
        libvlc_meta_Genre,
        libvlc_meta_Copyright,
        libvlc_meta_Album,
        libvlc_meta_TrackNumber,
        libvlc_meta_Description,
        libvlc_meta_Rating,
        libvlc_meta_Date,
        libvlc_meta_Setting,
        libvlc_meta_URL,
        libvlc_meta_Language,
        libvlc_meta_NowPlaying,
        libvlc_meta_Publisher,
        libvlc_meta_EncodedBy,
        libvlc_meta_ArtworkURL,
        libvlc_meta_TrackID
    }

    /// <summary>
    /// Media track type such as Audio, Video or Text
    /// </summary>
    public enum tracktype
    {
        /// <summary>
        /// Unknown track
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// Audio track
        /// </summary>
        Audio = 0,

        /// <summary>
        /// Video track
        /// </summary>
        Video = 1,

        /// <summary>
        /// Text track
        /// </summary>
        Text = 2
    }
}