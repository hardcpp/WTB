using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using HMUI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WTB.Views
{
    /// <summary>
    /// Qualifiers view controller
    /// </summary>
    public class Qualifiers : SDK.UI.ViewController<Qualifiers>, IProgress<double>
    {
#pragma warning disable CS0649
        [UIComponent("QualifiersTitle")]
        private TextMeshProUGUI m_QualifiersTitle;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIComponent("SongUpButton")]
        private Button m_SongUpButton;
        [UIObject("SongList")]
        private GameObject m_SongListView = null;
        private SDK.UI.DataSource.SongList m_SongList = null;
        [UIComponent("SongDownButton")]
        private Button m_SongDownButton;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIObject("SongInfoPanel")]
        private GameObject m_SongInfoPanel;
        private SDK.UI.LevelDetail m_SongInfo_Detail;
#pragma warning restore CS0649

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private class SongEntry
        {
            internal Network.Methods.Playlist_Result.SongEntry Raw = null;
            internal BeatSaverSharp.Beatmap BeatMap = null;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Should refresh at activation
        /// </summary>
        private bool m_DontRefreshOnActivation = false;
        /// <summary>
        /// ID of the tournament
        /// </summary>
        private int m_TournamentID;
        /// <summary>
        /// Name of the tournament
        /// </summary>
        private string m_TournamentName;
        /// <summary>
        /// End time
        /// </summary>
        private DateTime m_EndTime;
        /// <summary>
        /// BeatSaver API
        /// </summary>
        private readonly BeatSaverSharp.BeatSaver m_BeatSaver = new BeatSaverSharp.BeatSaver(new BeatSaverSharp.HttpOptions("WTB", "1.0.0", TimeSpan.FromSeconds(10), StaticConfig.BeatSaverBaseURL));
        /// <summary>
        /// Current song list page
        /// </summary>
        private int m_CurrentPage = 1;
        /// <summary>
        /// All song list
        /// </summary>
        private List<SongEntry> m_Songs;
        /// <summary>
        /// Selected song index
        /// </summary>
        private int m_SelectedSongIndex = 0;
        /// <summary>
        /// Selected song
        /// </summary>
        private SDK.UI.DataSource.SongList.Entry m_SelectedSong = null;
        /// <summary>
        /// Selected song characteristic
        /// </summary>
        private BeatmapCharacteristicSO m_SelectedSongCharacteristic = null;
        /// <summary>
        /// Download cancel token
        /// </summary>
        private CancellationTokenSource m_DownloadCancelTokenSource;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get view content description XML
        /// </summary>
        /// <returns></returns>
        protected override string GetViewContentDescription()
        {
            string BSML_RESOURCE_RAW = "<vertical child-control-height='false' spacing='0' pad='0'> <horizontal bg='panel-top' pad-left='15' pad-right='15' horizontal-fit='PreferredSize'> <text id='QualifiersTitle' align='Center' font-size='4.2'/> </horizontal> <horizontal spacing='0' pad-top='1'> <vertical pad-right='3'> <page-button id='SongUpButton' direction='Up'></page-button> <list id='SongList' expand-cell='true'> </list> <page-button id='SongDownButton' direction='Down'></page-button> </vertical> <vertical pad-left='3' pad-top='7' id='SongInfoPanel' spacing='0' size-delta-x='110' size-delta-y='110' min-width='80'> </vertical> </horizontal></vertical>";
            return BSML_RESOURCE_RAW;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override void OnViewCreation()
        {
            /// Scale down up & down button
            m_SongUpButton.transform.localScale     = Vector3.one * 0.6f;
            m_SongDownButton.transform.localScale   = Vector3.one * 0.6f;

            /// Prepare song list
            var l_BSMLTableView = m_SongListView.GetComponentInChildren<BSMLTableView>();
            l_BSMLTableView.SetDataSource(null, false);
            GameObject.DestroyImmediate(m_SongListView.GetComponentInChildren<CustomListTableData>());
            m_SongList = l_BSMLTableView.gameObject.AddComponent<SDK.UI.DataSource.SongList>();
            m_SongList.TableViewInstance    = l_BSMLTableView;
            m_SongList.PlayPreviewAudio     = Config.SongPreview;
            m_SongList.PreviewAudioVolume   = Config.SongPreviewVolume;
            m_SongList.Init();
            l_BSMLTableView.SetDataSource(m_SongList, false);

            /// Bind events
            m_SongUpButton.onClick.AddListener(OnSongPageUpPressed);
            m_SongList.OnCoverFetched                               += OnSongCoverFetched;
            m_SongList.TableViewInstance.didSelectCellWithIdxEvent  += OnSongSelected;
            m_SongDownButton.onClick.AddListener(OnSongPageDownPressed);

            /// Init music preview details panel
            m_SongInfo_Detail = new SDK.UI.LevelDetail(m_SongInfoPanel.transform);
            m_SongInfo_Detail.SetPracticeButtonEnabled(false);
            m_SongInfo_Detail.SetPlayButtonAction(OnSongPlayPressed);
        }
        /// <summary>
        /// On view activation
        /// </summary>
        protected override void OnViewActivation()
        {
            /// Prevent refresh if requested
            if (m_DontRefreshOnActivation)
            {
                m_DontRefreshOnActivation = false;
                return;
            }

            /// Hide song info panel
            m_SongInfo_Detail.SetActive(false);

            /// Go back to page 1
            m_CurrentPage = 1;

            /// Update UI
            m_SongUpButton.interactable = m_CurrentPage != 1;
            m_SongDownButton.interactable = m_Songs != null ? (m_Songs.Count > (m_CurrentPage * 7)) : false;

            /// Refresh settings
            m_SongList.PlayPreviewAudio     = Config.SongPreview;
            m_SongList.PreviewAudioVolume   = Config.SongPreviewVolume;

            /// Clear old list
            m_Songs = null;
            RebuildSongList();

            /// Show loading modal
            ShowLoadingModal();

            /// Query tournament qualifier playlist
            Network.ServerConnection.SendQuery(new Network.Methods.Playlist()
            {
                TournamentID = m_TournamentID
            });
        }
        /// <summary>
        /// On view deactivation
        /// </summary>
        protected override void OnViewDeactivation()
        {
            /// Stop preview music if any
            m_SongList.StopPreviewMusic();

            /// Cancel download if any
            if (m_DownloadCancelTokenSource != null)
            {
                m_DownloadCancelTokenSource.Cancel();
                m_DownloadCancelTokenSource = null;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Called every frame if the script is enabled.
        /// </summary>
        public void Update()
        {
            var l_Diff = (int)(m_EndTime - DateTime.Now).TotalSeconds;
            var l_Text = m_TournamentName + " | <color=yellow><b>End in ";

            if (l_Diff > 0)
            {
                const int s_Day     = 24 * 60 * 60;
                const int s_Hour    = 60 * 60;
                const int s_Minute  = 60;

                var l_Days      = l_Diff / s_Day;
                var l_Hours     = (l_Diff - ((l_Days * s_Day))) / s_Hour;
                var l_Minutes   = (l_Diff - ((l_Days * s_Day) + (l_Hours * s_Hour))) / s_Minute;
                var l_Seconds   = (l_Diff - ((l_Days * s_Day) + (l_Hours * s_Hour) + (l_Minutes * s_Minute)));

                if (l_Days > 0)     l_Text += l_Days    + "d ";
                if (l_Hours > 0)    l_Text += l_Hours   + "h ";
                if (l_Minutes > 0)  l_Text += l_Minutes + "m ";

                l_Text += l_Seconds.ToString().PadLeft(2, '0') + "s";
            }
            else
                l_Text += "00s";

            if (l_Text != m_QualifiersTitle.text)
                m_QualifiersTitle.text = l_Text;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set selected tournament
        /// </summary>
        /// <param name="p_TournamentID">ID of the tournament</param>
        /// <param name="p_TournamentName"></param>
        internal void SetSelectedTournament(int p_TournamentID, string p_TournamentName, DateTime p_EndTime)
        {
            /// Store tournament ID
            m_TournamentID = p_TournamentID;

            /// Update tournament name
            m_TournamentName = "Qualifiers : " + p_TournamentName;

            /// End time
            m_EndTime = p_EndTime;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Go to previous song page
        /// </summary>
        private void OnSongPageUpPressed()
        {
            /// Underflow check
            if (m_CurrentPage < 2)
                return;

            /// Decrement current page
            m_CurrentPage--;

            /// Rebuild song list
            RebuildSongList();
        }
        /// <summary>
        /// Go to next song page
        /// </summary>
        private void OnSongPageDownPressed()
        {
            /// Increment current page
            m_CurrentPage++;

            /// Rebuild song list
            RebuildSongList();
        }
        /// <summary>
        /// When the qualifiers playlist received
        /// </summary>
        /// <param name="p_Result">Result</param>
        [Network.Methods._MethodResultHandler]
        static private void OnPlaylist(Network.Methods.Playlist_Result p_Result)
        {
            if (!CanBeUpdated)
                return;

            /// Should back to tournament select
            if (p_Result.BackToTournamentSelect)
            {
                /// Hide loading modal
                Instance.HideLoadingModal();

                /// Go back to tournament select with a message
                ViewFlowCoordinator.Instance().tournamentSelect.SetMessageModal_PendingMessage(p_Result.BackMessage);
                ViewFlowCoordinator.Instance().SwitchToTournamentSelect();

                return;
            }

            /// Store new song list
            Instance.m_Songs = new List<SongEntry>();

            /// Add songs
            foreach (var l_Current in p_Result.Songs)
            {
                Instance.m_Songs.Add(new SongEntry()
                {
                    Raw = l_Current
                });
            }

            /// Rebuild song list
            Instance.RebuildSongList();

            /// Hide loading modal
            Instance.HideLoadingModal();
        }
        /// <summary>
        /// Rebuild song list
        /// </summary>
        /// <returns></returns>
        private void RebuildSongList(int p_SongToFocus = 0)
        {
            /// Clear selection and items, then refresh the list
            m_SongList.TableViewInstance.ClearSelection();
            m_SongList.Data.Clear();

            /// Append all songs
            if (m_Songs != null)
            {
                for (int l_I = (m_CurrentPage - 1) * 7; l_I < (m_CurrentPage * 7); ++l_I)
                {
                    if (l_I >= m_Songs.Count)
                        break;

                    var l_SongEntry = m_Songs[l_I];
                    var l_SongHash  = l_SongEntry.Raw.Hash.ToLower();
                    var l_LocalSong = SongCore.Loader.GetLevelByHash(l_SongHash);

                    /// If the map is already downloaded
                    if (l_LocalSong != null && SongCore.Loader.CustomLevels.ContainsKey(l_LocalSong.customLevelPath))
                    {
                        m_SongList.Data.Add(new SDK.UI.DataSource.SongList.Entry()
                        {
                            CustomLevel = l_LocalSong,
                            CustomData  = l_SongEntry
                        });
                    }
                    /// Fetch information from beat saver
                    else
                    {
                        var l_RowEntry = new SDK.UI.DataSource.SongList.Entry();
                        l_RowEntry.CustomData = l_SongEntry;

                        if (l_SongEntry.BeatMap == null)
                        {
                            l_SongEntry.BeatMap         = new BeatSaverSharp.Beatmap(m_BeatSaver, null, l_SongHash);
                            l_RowEntry.BeatSaver_Map    = l_SongEntry.BeatMap;

                            _ = l_SongEntry.BeatMap.Populate().ContinueWith((x) =>
                            {
                                if (x.Status != TaskStatus.RanToCompletion)
                                    l_RowEntry.BeatSaver_Map = null;

                                HMMainThreadDispatcher.instance.Enqueue(() =>
                                {
                                    /// Refresh cells
                                    if (CanBeUpdated)
                                    {
                                        m_SongList.TableViewInstance.RefreshCellsContent();
                                        OnSongSelected(m_SongList.TableViewInstance, m_SelectedSongIndex);
                                    }
                                });
                            });

                        }
                        else
                            l_RowEntry.BeatSaver_Map = l_SongEntry.BeatMap;

                        m_SongList.Data.Add(l_RowEntry);
                    }
                }
            }

            /// Refresh the list
            m_SongList.TableViewInstance.ReloadData();

            /// Update UI
            m_SongUpButton.interactable   = m_CurrentPage != 1;
            m_SongDownButton.interactable = m_Songs != null ? (m_Songs.Count > (m_CurrentPage * 7)) : false;

            /// Select first one
            if (p_SongToFocus < m_SongList.Data.Count)
            {
                m_SongList.TableViewInstance.SelectCellWithIdx(p_SongToFocus);
                m_SongList.DidSelectCellWithIdxEvent(m_SongList.TableViewInstance, p_SongToFocus);
                OnSongSelected(m_SongList.TableViewInstance, p_SongToFocus);
            }
        }
        /// <summary>
        /// When a song is selected
        /// </summary>
        /// <param name="p_TableView">Source table</param>
        /// <param name="p_Row">Selected row</param>
        private void OnSongSelected(TableView p_TableView, int p_Row)
        {
            /// Unselect previous song
            m_SelectedSong                  = null;
            m_SelectedSongCharacteristic    = null;
            m_SelectedSongIndex             = p_Row;

            /// Hide if invalid song
            if (p_Row < 0
                || p_Row >= m_SongList.Data.Count
                || m_SongList.Data[p_Row].Invalid
                || (p_TableView != null && m_SongList.Data[p_Row].BeatSaver_Map != null && m_SongList.Data[p_Row].BeatSaver_Map.Partial))
            {
                /// Hide song info panel
                m_SongInfo_Detail.SetActive(false);
                /// Stop preview music if any
                m_SongList.StopPreviewMusic();
                /// Hide score board
                ViewFlowCoordinator.Instance().HideRightScreen();

                return;
            }

            /// Fetch song entry
            var l_SongRowData = m_SongList.Data[p_Row];

            /// Show song info panel
            m_SongInfo_Detail.SetActive(true);

            var l_GameMode      = (l_SongRowData.CustomData as SongEntry).Raw.Mode;
            var l_Difficulty    = (l_SongRowData.CustomData as SongEntry).Raw.Difficulty;

            if (l_SongRowData.CustomLevel != null && SongCore.Loader.CustomLevels.ContainsKey(l_SongRowData.CustomLevel.customLevelPath))
            {
                if (!m_SongInfo_Detail.FromSongCore(l_SongRowData.CustomLevel, l_SongRowData.Cover, l_GameMode, l_Difficulty, out m_SelectedSongCharacteristic))
                {
                    /// Hide song info panel
                    m_SongInfo_Detail.SetActive(false);

                    /// Show message
                    ShowMessageModal("Ooops, the requested difficulty doesn't seem to exist !");

                    return;
                }

                /// Store selected song
                m_SelectedSong = l_SongRowData;

                /// Update play button
                m_SongInfo_Detail.SetPlayButtonText("Play");
            }
            else
            {
                if (!m_SongInfo_Detail.FromBeatSaver(l_SongRowData.BeatSaver_Map, l_SongRowData.Cover, l_GameMode, l_Difficulty, out m_SelectedSongCharacteristic))
                {
                    /// Hide song info panel
                    m_SongInfo_Detail.SetActive(false);

                    /// Show message
                    ShowMessageModal("Ooops, the requested difficulty doesn't seem to exist !");

                    return;
                }

                /// Store selected song
                m_SelectedSong = l_SongRowData;

                /// Update play button
                m_SongInfo_Detail.SetPlayButtonText("Download");
            }

            /// Show score board
            ViewFlowCoordinator.Instance().ShowScoreBoard();
            ViewFlowCoordinator.Instance().scoreBoard.LoadScore(m_TournamentID, (l_SongRowData.CustomData as SongEntry).Raw.Hash);
        }
        /// <summary>
        /// On song cover fetched
        /// </summary>
        /// <param name="p_RowData">Row data</param>
        private void OnSongCoverFetched(int p_Index, SDK.UI.DataSource.SongList.Entry p_RowData)
        {
            if (m_SelectedSongIndex != p_Index)
                return;

            OnSongSelected(m_SongList.TableViewInstance, m_SelectedSongIndex);
        }
        /// <summary>
        /// On song play button pressed
        /// </summary>
        private void OnSongPlayPressed()
        {
            if (m_SelectedSong == null || m_SelectedSongCharacteristic == null)
                return;

            var l_LocalSong = SongCore.Loader.GetLevelByHash((m_SelectedSong.CustomData as SongEntry).Raw.Hash.ToLower());

            /// If the song doesn't exist
            if (l_LocalSong == null || !SongCore.Loader.CustomLevels.ContainsKey(l_LocalSong.customLevelPath))
            {
                /// Show download modal
                ShowLoadingModal("Downloading", true);

                if (m_DownloadCancelTokenSource != null)
                {
                    m_DownloadCancelTokenSource.Cancel();
                    m_DownloadCancelTokenSource = null;
                }

                m_DownloadCancelTokenSource = new CancellationTokenSource();

                /// Start downloading
                SDK.Game.BeatSaver.DownloadSong(m_SelectedSong.BeatSaver_Map, m_DownloadCancelTokenSource.Token, this).ContinueWith((x) =>
                {
                    if (x.IsCanceled || x.Status == TaskStatus.Canceled)
                        return;

                    /// Bind callback
                    SongCore.Loader.SongsLoadedEvent += OnDownloadedSongLoaded;
                    /// Refresh loaded songs
                    SongCore.Loader.Instance.RefreshSongs(false);
                });

                return;
            }

            /// Stop preview music
            m_SongList.StopPreviewMusic();

            /// Show loading modal
            ShowLoadingModal();

            SDK.Game.Level.LoadSong("custom_level_" + (m_SelectedSong.CustomData as SongEntry).Raw.Hash.ToUpper(), (p_LoadedLevel) =>
            {
                BeatmapDifficulty l_Difficulty = SDK.Game.Level.SerializedToDifficulty((m_SelectedSong.CustomData as SongEntry).Raw.Difficulty);

                /// Hide loading modal
                HideLoadingModal();

                /// Fetch game settings
                var l_PlayerData        = Resources.FindObjectsOfTypeAll<PlayerDataModel>().First().playerData;
                var l_PlayerSettings    = l_PlayerData.playerSpecificSettings;
                var l_ColorScheme       = l_PlayerData.colorSchemesSettings.overrideDefaultColors ? l_PlayerData.colorSchemesSettings.GetSelectedColorScheme() : null;

                /// Prevent refresh on activation
                m_DontRefreshOnActivation = true;

                if (!Config.SubmitScores)
                    BS_Utils.Gameplay.ScoreSubmission.DisableSubmission("WTB");

                SDK.Game.Level.PlaySong(p_LoadedLevel, m_SelectedSongCharacteristic, l_Difficulty, l_PlayerData.overrideEnvironmentSettings, l_ColorScheme, new GameplayModifiers(
                    false, false, GameplayModifiers.EnergyType.Bar, true, false, false, GameplayModifiers.EnabledObstacleType.All, false, false, false, false, GameplayModifiers.SongSpeed.Normal, false, false, false, false, false
                    ), l_PlayerSettings,
                (p_Transition, p_LevelPlayResult, p_Difficulty) =>
                {
                    if (p_LevelPlayResult.levelEndAction == LevelCompletionResults.LevelEndAction.Restart)
                    {
                        OnSongPlayPressed();
                        return;
                    }

                    if (p_LevelPlayResult.levelEndStateType == LevelCompletionResults.LevelEndStateType.None)
                        return;

                    /// Prepare data
                    int l_Score             = p_LevelPlayResult.rawScore;
                    int l_MaxCombo          = p_LevelPlayResult.maxCombo;
                    int l_CutCount          = p_LevelPlayResult.goodCutsCount;
                    int l_TotalNoteCount    = p_Difficulty.beatmapData.cuttableNotesType;
                    int l_MaxScore          = SDK.Game.Level.GetMaxScore(p_Difficulty);
                    float l_Accuracy        = SDK.Game.Level.GetScorePercentage(l_MaxScore, l_Score);

                    /// Show loading modal
                    ShowLoadingModal("Uploading score");

                    /// Send result
                    Network.ServerConnection.SendQuery(new Network.Methods.SubmitPlayScore()
                    {
                        TournamentID    = m_TournamentID,
                        Song            = (m_SelectedSong.CustomData as SongEntry).Raw.Hash,
                        Score           = l_Score,
                        Accuracy        = l_Accuracy,
                        CutCount        = l_CutCount,
                        MaxCombo        = l_MaxCombo,
                        TotalNoteCount  = l_TotalNoteCount
                    });
                });
            });
        }
        /// <summary>
        /// On download progress reported
        /// </summary>
        /// <param name="p_Value"></param>
        void IProgress<double>.Report(double p_Value)
        {
            /// Avoid refresh if not active view anymore
            if (!CanBeUpdated)
                return;

            SetLoadingModal_DownloadProgress("Downloading", (float)p_Value);
        }
        /// <summary>
        /// When a downloaded song is downloaded
        /// </summary>
        /// <param name="p_Loader">Loader instance</param>
        /// <param name="p_Maps">All loaded songs</param>
        private void OnDownloadedSongLoaded(SongCore.Loader p_Loader, ConcurrentDictionary<string, CustomPreviewBeatmapLevel> p_Maps)
        {
            /// Remove callback
            SongCore.Loader.SongsLoadedEvent -= OnDownloadedSongLoaded;

            /// Avoid refresh if not active view anymore
            if (!CanBeUpdated)
                return;

            /// Refresh the list
            m_SongList.TableViewInstance.ReloadData();

            /// Rebuild song list
            RebuildSongList(m_SelectedSongIndex);

            /// Hide loading modal
            HideLoadingModal();
        }
        /// <summary>
        /// On submit score result
        /// </summary>
        /// <param name="p_Result">Result</param>
        [Network.Methods._MethodResultHandler]
        static private void OnSubmitScoreResult(Network.Methods.SubmitPlayScore_Result p_Result)
        {
            if (!CanBeUpdated)
                return;

            /// Should back to tournament select
            if (p_Result.BackToTournamentSelect)
            {
                /// Hide loading modal
                Instance.HideLoadingModal();

                /// Go back to tournament select with a message
                ViewFlowCoordinator.Instance().tournamentSelect.SetMessageModal_PendingMessage(p_Result.BackMessage);
                ViewFlowCoordinator.Instance().SwitchToTournamentSelect();

                return;
            }

            /// Hide loading modal
            Instance.HideLoadingModal();

            /// Prepare score board
            ViewFlowCoordinator.Instance().scoreBoard.SetSelectedModifiers(true, true);
            ViewFlowCoordinator.Instance().scoreBoard.LoadScore(Instance.m_TournamentID, p_Result.Song);
        }
    }
}
