using BS_Utils.Utilities;
using HMUI;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WTB.SDK.UI.DataSource
{
    /// <summary>
    /// Song entry list source
    /// </summary>
    internal class SongList : MonoBehaviour, TableView.IDataSource
    {
        /// <summary>
        /// Cell template
        /// </summary>
        private LevelListTableCell m_SongListTableCellInstance;
        /// <summary>
        /// Default cover image
        /// </summary>
        private Sprite m_DefaultCover = null;
        /// <summary>
        /// Song preview player
        /// </summary>
        private SongPreviewPlayer m_SongPreviewPlayer;
        /// <summary>
        /// Cover cache
        /// </summary>
        private static Dictionary<string, Sprite> CoverCache = new Dictionary<string, Sprite>();
        /// <summary>
        /// Audio clip cache
        /// </summary>
        private static Dictionary<string, AudioClip> AudioClipCache = new Dictionary<string, AudioClip>();
        /// <summary>
        /// API client
        /// </summary>
        private static Network.APIClient APIClient = new Network.APIClient("", TimeSpan.FromMinutes(1), false);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Table view instance
        /// </summary>
        internal TableView TableViewInstance;
        /// <summary>
        /// Data
        /// </summary>
        internal List<Entry> Data = new List<Entry>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Play preview audio ?
        /// </summary>
        internal bool PlayPreviewAudio = false;
        /// <summary>
        /// Preview volume
        /// </summary>
        internal float PreviewAudioVolume = 1f;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On cover fetched event
        /// </summary>
        internal event Action<int, Entry> OnCoverFetched;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Song entry
        /// </summary>
        internal class Entry
        {
            /// <summary>
            /// Was init
            /// </summary>
            private bool m_WasInit = false;

            ////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////

            /// <summary>
            /// Title prefix
            /// </summary>
            internal string TitlePrefix = "";
            /// <summary>
            /// Hover hint text
            /// </summary>
            internal string HoverHint = null;
            /// <summary>
            /// Beat saver map
            /// </summary>
            internal BeatSaverSharp.Beatmap BeatSaver_Map = null;
            /// <summary>
            /// Custom level instance
            /// </summary>
            internal CustomPreviewBeatmapLevel CustomLevel = null;
            /// <summary>
            /// Cover
            /// </summary>
            internal Sprite Cover;
            /// <summary>
            /// Custom data
            /// </summary>
            internal object CustomData = null;
            /// <summary>
            /// Is invalid
            /// </summary>
            internal bool Invalid => (CustomLevel == null && BeatSaver_Map == null);

            ////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////

            /// <summary>
            /// Init the entry
            /// </summary>
            internal void Init()
            {
                if (m_WasInit)
                    return;

                if (CustomLevel == null && BeatSaver_Map != null && !BeatSaver_Map.Partial)
                {
                    var l_LocalLevel = SongCore.Loader.GetLevelByHash(BeatSaver_Map.Hash.ToUpper());
                    if (l_LocalLevel != null && SongCore.Loader.CustomLevels.ContainsKey(l_LocalLevel.customLevelPath))
                        CustomLevel = l_LocalLevel;
                }

                m_WasInit = true;
            }
            /// <summary>
            /// Get entry level hash
            /// </summary>
            /// <returns></returns>
            internal string GetLevelHash()
            {
                if (BeatSaver_Map != null && BeatSaver_Map.Hash != null)
                    return BeatSaver_Map.Hash.ToLower();
                else if (CustomLevel != null && CustomLevel.levelID.StartsWith("custom_level_"))
                    return CustomLevel.levelID.Replace("custom_level_", "").ToLower();

                return "";
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Called when the script is being destroyed.
        /// </summary>
        public void OnDestroy()
        {
            /// Bind event
            TableViewInstance.didSelectCellWithIdxEvent -= DidSelectCellWithIdxEvent;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Init
        /// </summary>
        internal void Init()
        {
            /// Find preview player
            m_SongPreviewPlayer = Resources.FindObjectsOfTypeAll<SongPreviewPlayer>().FirstOrDefault();

            /// Bind event
            TableViewInstance.didSelectCellWithIdxEvent -= DidSelectCellWithIdxEvent;
            TableViewInstance.didSelectCellWithIdxEvent += DidSelectCellWithIdxEvent;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Build cell
        /// </summary>
        /// <param name="p_TableView">Table view instance</param>
        /// <param name="p_Index">Cell index</param>
        /// <returns></returns>
        public TableCell CellForIdx(TableView p_TableView, int p_Index)
        {
            LevelListTableCell l_Cell = GetTableCell();

            TextMeshProUGUI l_Text      = l_Cell.GetField<TextMeshProUGUI, LevelListTableCell>("_songNameText");
            TextMeshProUGUI l_SubText   = l_Cell.GetField<TextMeshProUGUI, LevelListTableCell>("_songAuthorText");
            l_Cell.GetField<Image, LevelListTableCell>("_favoritesBadgeImage").gameObject.SetActive(false);

            var l_HoverHint = l_Cell.gameObject.GetComponent<HoverHint>();
            if (l_HoverHint == null || !l_HoverHint)
            {
                l_HoverHint = l_Cell.gameObject.AddComponent<HoverHint>();
                l_HoverHint.SetField("_hoverHintController", Resources.FindObjectsOfTypeAll<HoverHintController>().First());
            }

            if (l_Cell.gameObject.GetComponent<LocalizedHoverHint>())
                GameObject.Destroy(l_Cell.gameObject.GetComponent<LocalizedHoverHint>());

            var l_SongEntry = Data[p_Index];
            l_SongEntry.Init();

            if ((l_SongEntry.BeatSaver_Map != null && !l_SongEntry.BeatSaver_Map.Partial) || l_SongEntry.CustomLevel != null)
            {
                var l_HaveSong = l_SongEntry.CustomLevel != null && SongCore.Loader.CustomLevels.ContainsKey(l_SongEntry.CustomLevel.customLevelPath);

                string l_MapName        = "";
                string l_MapAuthor      = "";
                string l_MapSongAuthor  = "";
                float  l_Duration       = 0f;
                float  l_BPM            = 0f;

                if (l_SongEntry.CustomLevel != null)
                {
                    l_MapName       = l_SongEntry.CustomLevel.songName;
                    l_MapAuthor     = l_SongEntry.CustomLevel.levelAuthorName;
                    l_MapSongAuthor = l_SongEntry.CustomLevel.songAuthorName;
                    l_BPM           = l_SongEntry.CustomLevel.beatsPerMinute;
                    l_Duration      = l_SongEntry.CustomLevel.songDuration;
                }
                else
                {
                    l_Duration = -1;
                    if (l_SongEntry.BeatSaver_Map.Metadata.Characteristics.Count > 0)
                    {
                        var l_FirstChara = l_SongEntry.BeatSaver_Map.Metadata.Characteristics.FirstOrDefault();
                        var l_DiffCount = l_FirstChara.Difficulties.Count(x => x.Value != null);

                        if (l_DiffCount > 0)
                        {
                            var l_FirstDiff = l_FirstChara.Difficulties.LastOrDefault(x => x.Value != null);
                            l_Duration = l_FirstDiff.Value.Length;
                        }
                    }

                    l_MapName       = l_SongEntry.BeatSaver_Map.Name;
                    l_MapAuthor     = l_SongEntry.BeatSaver_Map.Metadata.LevelAuthorName;
                    l_MapSongAuthor = l_SongEntry.BeatSaver_Map.Metadata.SongAuthorName;
                    l_BPM           = l_SongEntry.BeatSaver_Map.Metadata.BPM;
                }

                string l_Title      = l_SongEntry.TitlePrefix + (l_SongEntry.TitlePrefix.Length != 0 ? " " : "") + (l_HaveSong ? "<#7F7F7F>" : "") + l_MapName;
                string l_SubTitle   = l_MapAuthor + " [" + l_MapSongAuthor + "]";

                if (l_Title.Length > (28 + (l_HaveSong ? "<#7F7F7F>".Length : 0)))
                    l_Title = l_Title.Substring(0, 28 + (l_HaveSong ? "<#7F7F7F>".Length : 0)) + "...";
                if (l_SubTitle.Length > 28)
                    l_SubTitle = l_SubTitle.Substring(0, 28) + "...";

                l_Text.text     = l_Title;
                l_SubText.text  = l_SubTitle;

                var l_BPMText = l_Cell.GetField<TextMeshProUGUI, LevelListTableCell>("_songBpmText");
                l_BPMText.gameObject.SetActive(true);
                l_BPMText.text = ((int)l_BPM).ToString();

                var l_DurationText = l_Cell.GetField<TextMeshProUGUI, LevelListTableCell>("_songDurationText");
                l_DurationText.gameObject.SetActive(true);
                l_DurationText.text = l_Duration >= 0.0 ? $"{Math.Floor((double)l_Duration / 60):N0}:{Math.Floor((double)l_Duration % 60):00}" : "--";

                l_Cell.transform.Find("BpmIcon").gameObject.SetActive(true);

                if (l_SongEntry.Cover != null)
                    l_Cell.GetField<Image, LevelListTableCell>("_coverImage").sprite = l_SongEntry.Cover;
                else if (CoverCache.TryGetValue(l_SongEntry.GetLevelHash(), out var l_Cover))
                {
                    l_SongEntry.Cover = l_Cover;
                    l_Cell.GetField<Image, LevelListTableCell>("_coverImage").sprite = l_SongEntry.Cover;

                    OnCoverFetched?.Invoke(l_Cell.idx, l_SongEntry);
                }
                else
                {
                    l_Cell.GetField<Image, LevelListTableCell>("_coverImage").sprite = m_DefaultCover;

                    if (l_HaveSong)
                    {
                        var l_CoverTask = l_SongEntry.CustomLevel.GetCoverImageAsync(CancellationToken.None);
                        _ = l_CoverTask.ContinueWith(p_CoverTaskResult =>
                        {
                            if (l_Cell.idx >= Data.Count || l_SongEntry != Data[l_Cell.idx])
                                return;

                            HMMainThreadDispatcher.instance.Enqueue(() =>
                            {
                                /// Update infos
                                l_SongEntry.Cover = p_CoverTaskResult.Result;

                                /// Cache cover
                                if (!CoverCache.ContainsKey(l_SongEntry.GetLevelHash()))
                                    CoverCache.Add(l_SongEntry.GetLevelHash(), l_SongEntry.Cover);

                                if (l_Cell.idx < Data.Count && l_SongEntry == Data[l_Cell.idx])
                                {
                                    l_Cell.GetField<Image, LevelListTableCell>("_coverImage").sprite = l_SongEntry.Cover;
                                    l_Cell.RefreshVisuals();

                                    OnCoverFetched?.Invoke(l_Cell.idx, l_SongEntry);
                                }
                            });
                        });
                    }
                    else if (l_SongEntry.BeatSaver_Map != null)
                    {
                        /// Fetch cover
                        if (l_SongEntry.BeatSaver_Map.CoverURL.ToLower().StartsWith("http"))
                        {
                            var l_CoverTask = APIClient.GetAsync(l_SongEntry.BeatSaver_Map.CoverURL, CancellationToken.None, true);
                            _ = l_CoverTask.ContinueWith(p_CoverTaskResult =>
                            {
                                if (l_Cell.idx >= Data.Count || l_SongEntry != Data[l_Cell.idx])
                                    return;

                                if (l_CoverTask.IsCanceled
                                 || l_CoverTask.Status == TaskStatus.Canceled
                                 || l_CoverTask.Result == null
                                 || !l_CoverTask.Result.IsSuccessStatusCode)
                                    return;

                                HMMainThreadDispatcher.instance.Enqueue(() =>
                                {
                                    var l_Texture = new Texture2D(2, 2);

                                    if (l_Texture.LoadImage(p_CoverTaskResult.Result.BodyBytes))
                                    {
                                        l_SongEntry.Cover = Sprite.Create(l_Texture, new Rect(0, 0, l_Texture.width, l_Texture.height), new Vector2(0.5f, 0.5f), 100);

                                        /// Cache cover
                                        if (!CoverCache.ContainsKey(l_SongEntry.GetLevelHash()))
                                            CoverCache.Add(l_SongEntry.GetLevelHash(), l_SongEntry.Cover);

                                        if (l_Cell.idx < Data.Count && l_SongEntry == Data[l_Cell.idx])
                                        {
                                            l_Cell.GetField<Image, LevelListTableCell>("_coverImage").sprite = l_SongEntry.Cover;
                                            l_Cell.RefreshVisuals();

                                            OnCoverFetched?.Invoke(l_Cell.idx, l_SongEntry);
                                        }
                                    }
                                });
                            });
                        }
                        else
                        {
                            var l_CoverTask = l_SongEntry.BeatSaver_Map.CoverImageBytes();
                            _ = l_CoverTask.ContinueWith(p_CoverTaskResult =>
                            {
                                if (l_Cell.idx >= Data.Count || l_SongEntry != Data[l_Cell.idx])
                                    return;

                                HMMainThreadDispatcher.instance.Enqueue(() =>
                                {
                                    var l_Texture = new Texture2D(2, 2);

                                    if (l_Texture.LoadImage(p_CoverTaskResult.Result))
                                    {
                                        l_SongEntry.Cover = Sprite.Create(l_Texture, new Rect(0, 0, l_Texture.width, l_Texture.height), new Vector2(0.5f, 0.5f), 100);

                                        /// Cache cover
                                        if (!CoverCache.ContainsKey(l_SongEntry.GetLevelHash()))
                                            CoverCache.Add(l_SongEntry.GetLevelHash(), l_SongEntry.Cover);

                                        if (l_Cell.idx < Data.Count && l_SongEntry == Data[l_Cell.idx])
                                        {
                                            l_Cell.GetField<Image, LevelListTableCell>("_coverImage").sprite = l_SongEntry.Cover;
                                            l_Cell.RefreshVisuals();

                                            OnCoverFetched?.Invoke(l_Cell.idx, l_SongEntry);
                                        }
                                    }
                                });
                            });
                        }
                    }
                }
            }
            else if (l_SongEntry.BeatSaver_Map != null && l_SongEntry.BeatSaver_Map.Partial)
            {
                l_Text.text     = "Loading from WTB...";
                l_SubText.text  = "";

                l_Cell.GetField<TextMeshProUGUI, LevelListTableCell>("_songBpmText").gameObject.SetActive(false);
                l_Cell.GetField<TextMeshProUGUI, LevelListTableCell>("_songDurationText").gameObject.SetActive(false);
                l_Cell.transform.Find("BpmIcon").gameObject.SetActive(false);

                l_Cell.GetField<Image, LevelListTableCell>("_coverImage").sprite = m_DefaultCover;
            }
            else
            {
                l_Text.text     = "<#FF0000>Invalid song";
                l_SubText.text  = l_SongEntry.CustomLevel != null ? l_SongEntry.CustomLevel.levelID.Replace("custom_level_", "") : "";

                l_Cell.GetField<TextMeshProUGUI, LevelListTableCell>("_songBpmText").gameObject.SetActive(false);
                l_Cell.GetField<TextMeshProUGUI, LevelListTableCell>("_songDurationText").gameObject.SetActive(false);
                l_Cell.transform.Find("BpmIcon").gameObject.SetActive(false);

                l_Cell.GetField<Image, LevelListTableCell>("_coverImage").sprite = m_DefaultCover;
            }

            if (!string.IsNullOrEmpty(l_SongEntry.HoverHint))
            {
                l_HoverHint.enabled = true;
                l_HoverHint.text    = l_SongEntry.HoverHint;
            }
            else
                l_HoverHint.enabled = false;

            return l_Cell;
        }
        /// <summary>
        /// Get cell size
        /// </summary>
        /// <returns></returns>
        public float CellSize()
        {
            return 8.5f;
        }
        /// <summary>
        /// Get number of cell
        /// </summary>
        /// <returns></returns>
        public int NumberOfCells()
        {
            return Data.Count;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Stop preview music if any
        /// </summary>
        internal void StopPreviewMusic()
        {
            if (m_SongPreviewPlayer != null && m_SongPreviewPlayer)
                m_SongPreviewPlayer.CrossfadeToDefault();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When a cell is selected
        /// </summary>
        /// <param name="p_Table">Table instance</param>
        /// <param name="p_Row">Row index</param>
        internal void DidSelectCellWithIdxEvent(TableView p_Table, int p_Row)
        {
            if (m_SongPreviewPlayer == null || !m_SongPreviewPlayer || !PlayPreviewAudio || p_Row > Data.Count)
                return;

            /// Fetch song entry
            var l_SongRowData = Data[p_Row];

            /// Hide if invalid song
            if (l_SongRowData == null || l_SongRowData.Invalid)
                return;

            if (l_SongRowData.CustomLevel != null && SongCore.Loader.CustomLevels.ContainsKey(l_SongRowData.CustomLevel.customLevelPath))
            {
                if (AudioClipCache.TryGetValue(l_SongRowData.GetLevelHash(), out var l_AudioClip))
                    m_SongPreviewPlayer.CrossfadeTo(l_AudioClip, l_SongRowData.CustomLevel.previewStartTime, l_SongRowData.CustomLevel.previewDuration, false);
                else
                {
                    l_SongRowData.CustomLevel.GetPreviewAudioClipAsync(CancellationToken.None).ContinueWith(x =>
                    {
                        if (x.IsCompleted && x.Status == TaskStatus.RanToCompletion)
                        {
                            HMMainThreadDispatcher.instance.Enqueue(() =>
                            {
                                if (!AudioClipCache.ContainsKey(l_SongRowData.GetLevelHash()))
                                    AudioClipCache.Add(l_SongRowData.GetLevelHash(), x.Result);

                                m_SongPreviewPlayer.CrossfadeTo(x.Result, l_SongRowData.CustomLevel.previewStartTime, l_SongRowData.CustomLevel.previewDuration, false);
                            });
                        }
                    });
                }
            }
            else
            {
                /// Stop preview music if any
                m_SongPreviewPlayer.CrossfadeToDefault();
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get new table cell or reuse old one
        /// </summary>
        /// <returns></returns>
        private LevelListTableCell GetTableCell()
        {
            LevelListTableCell l_Cell = (LevelListTableCell)TableViewInstance.DequeueReusableCellForIdentifier("BSP_SongList_Cell");
            if (!l_Cell)
            {
                if (m_SongListTableCellInstance == null)
                    m_SongListTableCellInstance = Resources.FindObjectsOfTypeAll<LevelListTableCell>().First(x => (x.name == "LevelListTableCell"));

                l_Cell = Instantiate(m_SongListTableCellInstance);
            }

            l_Cell.SetField("_notOwned", false);
            l_Cell.reuseIdentifier = "BSP_SongList_Cell";

            if (m_DefaultCover == null)
                m_DefaultCover = l_Cell.GetField<Image, LevelListTableCell>("_coverImage").sprite;

            return l_Cell;
        }
    }
}
