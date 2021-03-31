using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using HMUI;
using Newtonsoft.Json.Linq;
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
    /// Match view controller
    /// </summary>
    public class Match : SDK.UI.ViewController<Match>, IProgress<double>
    {
#pragma warning disable CS0649
        [UIComponent("TournamentNameText")]
        private TextMeshProUGUI m_TournamentNameText = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIObject("MessageFrame")]
        private GameObject m_MessageFrame;
        [UIObject("MessageFrame_Background")]
        private GameObject m_MessageFrame_Background;
        [UIComponent("MessageFrame_Text")]
        private TextMeshProUGUI m_MessageFrame_Text;
        private string m_MessageFrame_RemainingTimePrefix = "";
        private DateTime? m_MessageFrame_RemainingTimeEnd = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIObject("SongFrame")]
        private GameObject m_SongFrame;
        [UIComponent("SongFrame_UpButton")]
        private Button m_SongFrame_UpButton;
        [UIObject("SongFrame_List")]
        private GameObject m_SongFrame_ListView = null;
        private SDK.UI.DataSource.SongList m_SongFrame_List = null;
        [UIComponent("SongFrame_DownButton")]
        private Button m_SongFrame_DownButton;
        [UIObject("SongFrame_InfoPanel")]
        private GameObject m_SongFrame_InfoPanel;
        private SDK.UI.LevelDetail m_SongFrame_InfoPanel_Detail;
        [UIComponent("SongFrame_RemainingTimeText")]
        private TextMeshProUGUI m_SongFrame_RemainingTimeFrameText;
        private string m_SongFrame_RemainingTimePrefix = "";
        private DateTime m_SongFrame_RemainingTimeEnd = DateTime.Now;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIObject("SongDetailFrame")]
        private GameObject m_SongDetailFrame;
        [UIObject("SongDetailFrame_Panel")]
        private GameObject m_SongDetailFrame_Panel;
        private SDK.UI.LevelDetail m_SongDetailFrame_Detail;
        private DateTime m_SongDetailFrame_RemainingTimeEnd = DateTime.Now;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIObject("SongPlayDetailFrame")]
        private GameObject m_SongPlayDetailFrame;
        [UIObject("SongPlayDetailFrame_Panel")]
        private GameObject m_SongPlayDetailFrame_Panel;
        private SDK.UI.LevelResults m_SongPlayDetailFrame_Detail;
        private DateTime m_SongPlayDetailFrame_RemainingTimeEnd = DateTime.Now;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIObject("ConfirmPickBanMessageModal")]
        private GameObject m_ConfirmPickBanMessageModal;
        [UIComponent("ConfirmPickBanMessageModal_Text")]
        private TextMeshProUGUI m_ConfirmPickBanMessageModal_Text;
        [UIComponent("ConfirmPickBanMessageModal_YesButton")]
        private Button m_ConfirmPickBanMessageModal_YesButton;
#pragma warning restore CS0649

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private static string RPC_STATE_WAITING_USERS       = "waiting_users";
        private static string RPC_STATE_WAITING_COORIDNATOR = "waiting_coordinator";
        private static string RPC_STATE_PICK_BAN_MAP        = "pick_ban";
        private static string RPC_STATE_LOADING_MAP         = "loading_map";
        private static string RPC_STATE_PLAYING             = "playing";
        private static string RPC_STATE_PLAY_SUMMARY        = "play_summary";
        private static string RPC_STATE_ENDED               = "ended";

        private static string RPC_STATE_PICK_BAN_MAP__EVENT__DO_PICK_BAN    = "pick_ban.do_pick_ban";
        private static string RPC_STATE_LOADING_MAP__EVENT__MAP_LOADED      = "loading_map.map_loaded";
        private static string RPC_STATE_LOADING_MAP__EVENT__READY           = "loading_map.ready";
        private static string RPC_STATE_PLAYING__EVENT__SCORE               = "playing.score";
        private static string RPC_STATE_PLAY_SUMMARY__EVENT__ASK_REPLAY     = "play_summary.ask_replay";

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
        /// Match name
        /// </summary>
        private string m_MatchName = "";
        /// <summary>
        /// Match ID
        /// </summary>
        private int m_MatchID;
        /// <summary>
        /// RPC Id
        /// </summary>
        private int m_RPCId = 0;
        /// <summary>
        /// RPC state
        /// </summary>
        private string m_RPCState = "";
        /// <summary>
        /// RPC Data
        /// </summary>
        private JObject m_RPCData = null;
        /// <summary>
        /// Network controller
        /// </summary>
        private Match_Network m_NetworkController = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private class SongEntry
        {
            internal Network.Methods.Playlist_Result.SongEntry Raw = null;
            internal BeatSaverSharp.Beatmap BeatMap = null;
        }

        /// <summary>
        /// BeatSaver API
        /// </summary>
        private readonly BeatSaverSharp.BeatSaver m_BeatSaver = new BeatSaverSharp.BeatSaver(new BeatSaverSharp.HttpOptions("WTB", "1.0.0", TimeSpan.FromSeconds(10), StaticConfig.BeatSaverBaseURL));
        /// <summary>
        /// Map name cache
        /// </summary>
        private readonly ConcurrentDictionary<string, string> m_MapNameCache = new ConcurrentDictionary<string, string>();
        /// <summary>
        /// Map author name cache
        /// </summary>
        private readonly ConcurrentDictionary<string, string> m_MapAuthorNameCache = new ConcurrentDictionary<string, string>();
        /// <summary>
        /// Current song list page
        /// </summary>
        private int m_CurrentPage = 1;
        /// <summary>
        /// All song list
        /// </summary>
        private readonly List<SongEntry> m_Songs = new List<SongEntry>();
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

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private bool m_LoadingMap_Loading = false;
        private bool m_LoadingMap_Loaded = false;
        private bool m_LoadingMap_Downloading = false;
        private float m_LoadingMap_DownloadingProgress = 0f;
        private int m_LoadingMap_DownloadingRemainingTry = 3;
        private bool m_LoadingMap_Ready = false;
        private string m_LoadingMap_ToLoad_Hash = "";
        private IBeatmapLevel m_LoadingMap_LoadedLevel = null;
        private BeatmapCharacteristicSO m_LoadingMap_LoadedLevelCharacteristic = null;
        private string m_LoadingMap_ToLoad_Difficulty = "";
        private Sprite m_LoadingMap_LevelCover = null;
        /// <summary>
        /// Download cancel token
        /// </summary>
        private CancellationTokenSource m_DownloadCancelTokenSource;

        private bool m_PlaySummary_ReplayRequested = false;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private bool m_PlayingMap_LoadingStarted = false;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Preview song player
        /// </summary>
        private SongPreviewPlayer m_SongPreviewPlayer;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Match ID getter
        /// </summary>
        internal int MatchID => m_MatchID;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get view content description XML
        /// </summary>
        /// <returns></returns>
        protected override string GetViewContentDescription()
        {
            string BSML_RESOURCE_RAW = "<vertical child-control-height='false' spacing='0' pad='0'> <horizontal bg='panel-top' pad-left='15' pad-right='15' horizontal-fit='PreferredSize'> <text id='TournamentNameText' align='Center' font-size='4.2'/> </horizontal> <horizontal id='MessageFrame' spacing='0' pad-top='-1'> <horizontal> <text text=' ' font-size='5.5' align='Center'/> </horizontal> <horizontal id='MessageFrame_Background' bg='round-rect-panel' pad='8'> <text id='MessageFrame_Text' text='Waiting for coordinator...' font-size='4.5' align='Center'/> </horizontal> <horizontal> <text text=' ' font-size='5.5' align='Center'/> </horizontal> </horizontal> <horizontal id='SongFrame' spacing='0' pad-top='1'> <vertical pad-right='3'> <page-button id='SongFrame_UpButton' direction='Up'></page-button> <list id='SongFrame_List' expand-cell='true'> </list> <page-button id='SongFrame_DownButton' direction='Down'></page-button> </vertical> <vertical pad-left='3' pad-top='9' id='SongFrame_InfoPanel' spacing='0' size-delta-x='110' size-delta-y='110' min-width='80'> </vertical> <horizontal ignore-layout='true'> <text id='SongFrame_RemainingTimeText' font-size='4' align='Center'/> </horizontal> </horizontal> <horizontal id='SongDetailFrame' spacing='0' pad-top='1'> <vertical pref-height='55'> <text text=' ' /> </vertical> <vertical pad-top='2' id='SongDetailFrame_Panel' spacing='0' size-delta-x='110' size-delta-y='110' min-width='80'> </vertical> <vertical pref-height='55'> <text text=' ' /> </vertical> </horizontal> <horizontal id='SongPlayDetailFrame' spacing='0' pad-top='-15'> <vertical pref-height='55'> </vertical> <vertical id='SongPlayDetailFrame_Panel' spacing='0' size-delta-x='110' size-delta-y='80' min-width='80'> </vertical> <vertical pref-height='55'> </vertical> </horizontal> <modal id='ConfirmPickBanMessageModal' show-event='ConfirmPickBanMessageModal' hide-event='CloseConfirmPickBanMessageModal,CloseAllModals' move-to-center='true' size-delta-y='20' size-delta-x='90'> <vertical pad='0'> <text text='' id='ConfirmPickBanMessageModal_Text' font-size='4' align='Center'/> <horizontal> <primary-button text='Yes' id='ConfirmPickBanMessageModal_YesButton'></primary-button> <button text='No' click-event='CloseConfirmPickBanMessageModal'></button> </horizontal> </vertical> </modal></vertical>";
            return BSML_RESOURCE_RAW;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override void OnViewCreation()
        {
            /// Bind events
            m_ConfirmPickBanMessageModal_YesButton.onClick.AddListener(OnDoPickBanPressed);

            /// Song panel
            if (true)
            {
                /// Scale down up & down button
                m_SongFrame_UpButton.transform.localScale   = Vector3.one * 0.6f;
                m_SongFrame_DownButton.transform.localScale = Vector3.one * 0.6f;

                /// Prepare song list
                var l_BSMLTableView = m_SongFrame_ListView.GetComponentInChildren<BSMLTableView>();
                l_BSMLTableView.SetDataSource(null, false);
                GameObject.DestroyImmediate(m_SongFrame_ListView.GetComponentInChildren<CustomListTableData>());
                m_SongFrame_List = l_BSMLTableView.gameObject.AddComponent<SDK.UI.DataSource.SongList>();
                m_SongFrame_List.TableViewInstance  = l_BSMLTableView;
                m_SongFrame_List.PlayPreviewAudio   = Config.SongPreview;
                m_SongFrame_List.PreviewAudioVolume = Config.SongPreviewVolume;
                m_SongFrame_List.Init();
                l_BSMLTableView.SetDataSource(m_SongFrame_List, false);

                /// Bind events
                m_SongFrame_UpButton.onClick.AddListener(OnSongPageUpPressed);
                m_SongFrame_List.OnCoverFetched                              += OnSongCoverFetched;
                m_SongFrame_List.TableViewInstance.didSelectCellWithIdxEvent += OnSongSelected;
                m_SongFrame_DownButton.onClick.AddListener(OnSongPageDownPressed);

                /// Init music preview details panel
                m_SongFrame_InfoPanel_Detail = new SDK.UI.LevelDetail(m_SongFrame_InfoPanel.transform);
                m_SongFrame_InfoPanel_Detail.SetPracticeButtonEnabled(false);
                m_SongFrame_InfoPanel_Detail.SetPlayButtonAction(OnSongPlayPressed);

                /// Move text
                var l_RectTransform = m_SongFrame_RemainingTimeFrameText.gameObject.transform.parent as RectTransform;
                l_RectTransform.anchorMin = new Vector2(0.48f, 0.86f);
            }

            /// Song Detail Frame
            if (true)
            {
                m_SongDetailFrame_Detail = new SDK.UI.LevelDetail(m_SongDetailFrame_Panel.transform);
                m_SongDetailFrame_Detail.SetPracticeButtonEnabled(false);
                m_SongDetailFrame_Detail.SetPlayButtonInteractable(false);
                m_SongDetailFrame_Detail.SetPlayButtonAction(OnReadyPressed);
            }

            /// Song play details frame
            if (true)
            {
                m_SongPlayDetailFrame_Detail = new SDK.UI.LevelResults(m_SongPlayDetailFrame_Panel.transform);
                m_SongPlayDetailFrame_Detail.SetButtonInteractable(false);
                m_SongPlayDetailFrame_Detail.SetButtonAction(OnReplayButton);
            }

            /// Update opacity
            SDK.UI.Backgroundable.SetOpacity(m_MessageFrame_Background, 0.5f);
            SDK.UI.ModalView.SetOpacity(m_ConfirmPickBanMessageModal, 0.85f);

            /// Find song preview object
            m_SongPreviewPlayer = Resources.FindObjectsOfTypeAll<SongPreviewPlayer>().FirstOrDefault();
        }
        /// <summary>
        /// On view activation
        /// </summary>
        protected override void OnViewActivation()
        {
            /// Update tournament name
            m_TournamentNameText.text = m_TournamentName + " | " + m_MatchName;

            /// Reset RPC ID
            m_RPCId = 0;

            /// Refresh settings
            m_SongFrame_List.PlayPreviewAudio   = Config.SongPreview;
            m_SongFrame_List.PreviewAudioVolume = Config.SongPreviewVolume;

            /// Initial join stuff
            if (!m_DontRefreshOnActivation)
            {
                /// Switch UI
                SwitchUI(RPC_STATE_WAITING_USERS);
                /// Update UI
                SetMessageFrameText("Joining the match...");

                ViewFlowCoordinator.Instance().matchPickBanList.SetData(new List<(bool, string, string)>());
                ViewFlowCoordinator.Instance().matchPlaylist.SetData(new List<(bool, string, float, string, string, float)>());
                ViewFlowCoordinator.Instance().matchPlaylist.SetTitle("Scores");

                /// Destroy existing network controller
                if (Instance.m_NetworkController != null)
                {
                    DestroyImmediate(Instance.m_NetworkController.gameObject);
                    Instance.m_NetworkController = null;
                }

                Network.ServerConnection.SendQuery(new Network.Methods.MatchJoin()
                {
                    TournamentID = m_TournamentID
                });
            }

            m_DontRefreshOnActivation = false;
        }
        /// <summary>
        /// On view deactivation
        /// </summary>
        protected override void OnViewDeactivation()
        {
            /// Stop preview music if any
            m_SongFrame_List.StopPreviewMusic();
            m_SongPreviewPlayer.CrossfadeToDefault();

            /// Cancel download if any
            if (m_DownloadCancelTokenSource != null)
            {
                m_DownloadCancelTokenSource.Cancel();
                m_DownloadCancelTokenSource = null;
            }

            /// Destroy existing network controller
            if (!m_DontRefreshOnActivation && m_NetworkController != null)
            {
                if (m_NetworkController && m_NetworkController.gameObject)
                    GameObject.Destroy(m_NetworkController.gameObject);

                m_NetworkController = null;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Called every frame if the script is enabled.
        /// </summary>
        public void Update()
        {
            if (m_NetworkController != null && m_NetworkController)
            {
                /// If new RPC data
                if (m_RPCId != m_NetworkController.RPCId)
                {
                    /// Update last ID
                    m_RPCId = m_NetworkController.RPCId;
                    /// Switch UI if needed
                    SwitchUI(m_NetworkController.RPCState);
                    /// Handle new RPC date
                    HandleRPCData(m_NetworkController.RPCData);
                }
            }

            if (m_SongFrame.activeSelf)
            {
                var l_Diff = (int)(m_SongFrame_RemainingTimeEnd - DateTime.Now).TotalSeconds;
                var l_Text = (m_SongFrame_RemainingTimePrefix != "" ? (m_SongFrame_RemainingTimePrefix + " - ") : "") + "<color=yellow><b>Remaining time ";

                if (l_Diff > 0)
                {
                    var l_Minutes = l_Diff / 60;
                    var l_Seconds = (l_Diff - (l_Minutes * 60));

                    l_Text += l_Minutes + ":" + l_Seconds.ToString().PadLeft(2, '0');
                }
                else
                    l_Text += "0:00";

                if (l_Text != m_SongFrame_RemainingTimeFrameText.text)
                    m_SongFrame_RemainingTimeFrameText.text = l_Text;
            }

            if (m_MessageFrame.activeSelf && m_MessageFrame_RemainingTimeEnd != null)
            {
                var l_Diff = (int)(m_MessageFrame_RemainingTimeEnd.Value - DateTime.Now).TotalSeconds;
                var l_Text = m_MessageFrame_RemainingTimePrefix + "\n<color=yellow><b>Remaining time ";

                if (l_Diff > 0)
                {
                    var l_Minutes = l_Diff / 60;
                    var l_Seconds = (l_Diff - (l_Minutes * 60));

                    l_Text += l_Minutes + ":" + l_Seconds.ToString().PadLeft(2, '0');
                }
                else
                    l_Text += "0:00";

                if (l_Text != m_MessageFrame_Text.text)
                    m_MessageFrame_Text.text = l_Text;
            }

            if (m_RPCState == RPC_STATE_LOADING_MAP)
            {
                var l_Diff        = (int)(m_SongDetailFrame_RemainingTimeEnd - DateTime.Now).TotalSeconds;
                var l_TimeText    = "";
                var l_CounterText = " (" + (m_RPCData["StateCounter"]?.Value<int>() ?? 0) + "/" + (m_RPCData["StateCounterMax"]?.Value<int>() ?? 0) + ")";

                if (l_Diff > 0)
                {
                    var l_Minutes = l_Diff / 60;
                    var l_Seconds = (l_Diff - (l_Minutes * 60));
                    l_TimeText += l_Minutes + ":" + l_Seconds.ToString().PadLeft(2, '0');
                }
                else
                    l_TimeText += "0:00";

                if (m_LoadingMap_Loading)
                {
                    string l_Message = "Loading map " + l_CounterText + "...";

                    if (m_LoadingMap_Downloading)
                        l_Message = string.Format("Downloading map (try {0}/{1})\n{2}%", 1 + (2 - m_LoadingMap_DownloadingRemainingTry), 3, Mathf.Round(m_LoadingMap_DownloadingProgress * 100f));

                    l_Message += "\n<color=yellow><b>Remaining time " + l_TimeText;

                    SetMessageFrameText(l_Message);
                }
                else if (m_MessageFrame.activeSelf)
                    m_MessageFrame.SetActive(false);

                if (m_SongDetailFrame.activeSelf)
                {
                    var l_State = m_RPCData["State"]?.Value<string>() ?? "loading";
                    if (l_State == "loading")
                    {
                        m_SongDetailFrame_Detail.SetPlayButtonText("Waiting for players " + l_CounterText + " <color=yellow><b>" + l_TimeText);
                        m_SongDetailFrame_Detail.SetPlayButtonInteractable(false);
                    }
                    else if (l_State == "ready_check")
                    {
                        m_SongDetailFrame_Detail.SetPlayButtonText("Set ready ! " + l_CounterText + " <color=yellow><b>" + l_TimeText);
                        m_SongDetailFrame_Detail.SetPlayButtonInteractable(!m_LoadingMap_Ready);
                    }
                }
            }

            if (m_RPCState == RPC_STATE_PLAY_SUMMARY)
            {
                int  l_RemainingCredits  = 0;
                bool l_CanAskReplay      = false;
                bool l_ReplayAsked       = m_RPCData.ContainsKey("ReplayAsked") ? m_RPCData["ReplayAsked"].Value<bool>() : false;

                if (m_RPCData.ContainsKey("ReplayCredits"))
                {
                    var l_Credits       = m_RPCData["ReplayCredits"] as JArray;
                    var l_MyPlayerID    = SDK.Game.UserPlatform.GetUserID();

                    foreach (JObject l_CurrentCredit in l_Credits)
                    {
                        if (l_CurrentCredit["Leader"]?.Value<string>() == l_MyPlayerID)
                        {
                            l_RemainingCredits  = l_CurrentCredit["Credit"]?.Value<int>() ?? 0;
                            l_CanAskReplay      = true;
                            break;
                        }
                    }
                }

                var l_Diff          = (int)(m_SongPlayDetailFrame_RemainingTimeEnd - DateTime.Now).TotalSeconds;
                var l_TimeText      =   (l_ReplayAsked
                                            ?
                                                "Replay asked!"
                                            :
                                                "Ask replay! " + (l_CanAskReplay ? (l_RemainingCredits > 0 ? "no replay remaining" : (l_RemainingCredits + " replay remaining")) : "(Team leader only)")
                                        )
                                        + " <color=yellow><b>(";

                if (l_CanAskReplay && !m_PlaySummary_ReplayRequested)
                    m_SongPlayDetailFrame_Detail.SetButtonInteractable(l_RemainingCredits > 0);
                else
                    m_SongPlayDetailFrame_Detail.SetButtonInteractable(false);

                if (l_Diff > 0)
                {
                    var l_Minutes = l_Diff / 60;
                    var l_Seconds = (l_Diff - (l_Minutes * 60));
                    l_TimeText += l_Minutes + ":" + l_Seconds.ToString().PadLeft(2, '0');
                }
                else
                    l_TimeText += "0:00";

                l_TimeText += ")";

                m_SongPlayDetailFrame_Detail.SetButtonText(l_TimeText);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set selected tournament
        /// </summary>
        /// <param name="p_TournamentID">ID of the tournament</param>
        /// <param name="p_TournamentName"></param>
        internal void SetSelectedTournament(int p_TournamentID, string p_TournamentName)
        {
            /// Store tournament ID
            m_TournamentID = p_TournamentID;

            /// Update tournament name
            m_TournamentName = p_TournamentName;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Switch UI
        /// </summary>
        /// <param name="p_RPCState">RPC State</param>
        private void SwitchUI(string p_RPCState)
        {
            if (m_RPCState == p_RPCState)
                return;

            m_RPCState = p_RPCState;

            ViewFlowCoordinator.Instance().HideLeftScreen();
            ViewFlowCoordinator.Instance().HideRightScreen();

            /// Close all modal except loading one
            HideMessageModal();

            /// Hide all panels
            m_MessageFrame.SetActive(false);
            m_SongFrame.SetActive(false);
            m_SongFrame_InfoPanel_Detail.SetActive(false);
            m_SongDetailFrame.SetActive(false);
            m_SongPlayDetailFrame.SetActive(false);
            m_ParserParams.EmitEvent("CloseConfirmPickBanMessageModal");

            /// Stop preview music if any
            m_SongFrame_List.StopPreviewMusic();
            if (m_SongPreviewPlayer)
                m_SongPreviewPlayer.CrossfadeToDefault();

            if (p_RPCState == RPC_STATE_WAITING_USERS || p_RPCState == RPC_STATE_WAITING_COORIDNATOR || p_RPCState == RPC_STATE_ENDED)
                m_MessageFrame.SetActive(true);
            else if (p_RPCState == RPC_STATE_PICK_BAN_MAP)
            {
                ViewFlowCoordinator.Instance().ShowMatchPickBanList();
            }
            else if (p_RPCState == RPC_STATE_LOADING_MAP)
            {
                m_LoadingMap_Loading                  = false;
                m_LoadingMap_Loaded                   = false;
                m_LoadingMap_Downloading              = false;
                m_LoadingMap_DownloadingProgress      = 0f;
                m_LoadingMap_DownloadingRemainingTry  = 3;
                m_LoadingMap_Ready                    = false;
                m_LoadingMap_LoadedLevel              = null;
                m_LoadingMap_LevelCover               = null;
                m_LoadingMap_ToLoad_Difficulty        = "";

                m_PlayingMap_LoadingStarted = false;

                ViewFlowCoordinator.Instance().ShowMatchPlaylist();
            }
            else if (p_RPCState == RPC_STATE_PLAYING)
            {
                m_MessageFrame.SetActive(true);
                ViewFlowCoordinator.Instance().ShowMatchPlaylist();
            }
            else if (p_RPCState == RPC_STATE_PLAY_SUMMARY)
            {
                m_PlaySummary_ReplayRequested = false;
                m_SongPlayDetailFrame.SetActive(true);
                ViewFlowCoordinator.Instance().ShowMatchPlaylist();
            }
        }
        /// <summary>
        /// Set message frame text
        /// </summary>
        /// <param name="p_Message">Message to show</param>
        /// <param name="p_RemainingTimeEnd">Remaining end time</param>
        private void SetMessageFrameText(string p_Message, DateTime? p_RemainingTimeEnd = null)
        {
            m_MessageFrame_RemainingTimePrefix  = p_Message;
            m_MessageFrame_RemainingTimeEnd     = p_RemainingTimeEnd;

            if (m_MessageFrame_Text.text != p_Message)
                m_MessageFrame_Text.text  = p_Message;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Handle RPC data
        /// </summary>
        /// <param name="p_Data">New RPC data</param>
        private void HandleRPCData(JObject p_Data, bool p_Force = false)
        {
            m_RPCData = p_Data;

            if (m_RPCState == RPC_STATE_WAITING_USERS || m_RPCState == RPC_STATE_WAITING_COORIDNATOR || m_RPCState == RPC_STATE_ENDED)
            {
                DateTime? l_Timeout = null;

                if (p_Data.ContainsKey("Timeout") && p_Data["Timeout"].Value<string>() != "-1")
                    l_Timeout = SDK.Misc.Time.FromUnixTime(p_Data["Timeout"]?.Value<Int64>() ?? 0);

                SetMessageFrameText(p_Data["Message"]?.Value<string>() ?? "Waiting for coordinator...", l_Timeout);

                HideLoadingModal();
            }
            else if (m_RPCState == RPC_STATE_PICK_BAN_MAP)
            {
                string l_CurrentPlayerTurn      = p_Data["CurrentPlayerTurn"]?.Value<string>() ?? "";
                string l_CurrentPlayerTurnName  = p_Data["CurrentPlayerTurnName"]?.Value<string>() ?? "Someone";
                bool   l_IsBan                  = (p_Data["CurrentActionType"]?.Value<string>() ?? "") != "pick";

                if (l_CurrentPlayerTurn == SDK.Game.UserPlatform.GetUserID())
                {
                    /// Update remaining time
                    m_SongFrame_RemainingTimeEnd    = SDK.Misc.Time.FromUnixTime((p_Data["CurrentActionEndTime"]?.Value<Int64>() ?? (uint)0));
                    m_SongFrame_RemainingTimePrefix = "Please " + (l_IsBan ? "<color=red><b>ban</b></color>" : "<color=green><b>pick</b></color>") + " a map";

                    /// Change UI
                    m_MessageFrame.SetActive(false);
                    m_SongFrame.SetActive(true);
                    m_SongFrame_InfoPanel_Detail.SetActive(false);
                    m_SongFrame_InfoPanel_Detail.SetPlayButtonText(l_IsBan ? "Ban" : "Pick");

                    /// Clear on song list
                    m_Songs.Clear();
                    m_SelectedSong = null;
                    m_SelectedSongCharacteristic = null;
                    m_CurrentPage = 1;

                    /// Parser song list
                    if (p_Data.ContainsKey("Maps"))
                    {
                        JArray l_Maps = p_Data["Maps"] as JArray;
                        foreach (JObject l_MapObject in l_Maps)
                        {
                            m_Songs.Add(new SongEntry()
                            {
                                Raw = new Network.Methods.Playlist_Result.SongEntry()
                                {
                                    Hash        = l_MapObject["Hash"]?.Value<string>()       ?? "",
                                    Mode        = l_MapObject["GameMode"]?.Value<string>()   ?? "",
                                    Difficulty  = l_MapObject["Difficulty"]?.Value<string>() ?? ""
                                }
                            });
                        }
                    }

                    /// Rebuild song list
                    RebuildSongList();

                    /// Show action required message
                    SetMessageModal_PendingMessage("Please " + (l_IsBan ? "<color=red><b>ban</b></color>" : "<color=green><b>pick</b></color>") + " a map");
                }
                else
                {
                    m_SongFrame.SetActive(false);
                    m_SongFrame_InfoPanel_Detail.SetActive(false);
                    m_MessageFrame.SetActive(true);

                    if (m_SongPreviewPlayer != null && m_SongPreviewPlayer)
                        m_SongPreviewPlayer.CrossfadeToDefault();

                    SetMessageFrameText("<u>" + l_CurrentPlayerTurnName + "</u> is " + (l_IsBan ? "<color=red><b>banning</b></color>" : "<color=green><b>picking</b></color>") + " a map",
                                        SDK.Misc.Time.FromUnixTime(p_Data["CurrentActionEndTime"]?.Value<Int64>() ?? 0));
                }

                /// Update pick/ban list on right screen
                ViewFlowCoordinator.Instance().matchPickBanList.SetData(GeneratePickBanList());

                HideLoadingModal();
            }
            else if (m_RPCState == RPC_STATE_LOADING_MAP)
            {
                m_LoadingMap_ToLoad_Hash            = p_Data["MapToLoad"]?["Hash"]?.Value<string>() ?? "";
                m_SongDetailFrame_RemainingTimeEnd  = SDK.Misc.Time.FromUnixTime(p_Data["StateEndTime"]?.Value<Int64>() ?? 0);

                if (!m_LoadingMap_Loaded)
                {
                    m_SongDetailFrame.SetActive(false);
                    m_MessageFrame.SetActive(true);
                    SetMessageFrameText("");

                    if (p_Force || !m_LoadingMap_Loading)
                    {
                        m_LoadingMap_Loading = true;

                        var l_LocalSong = SongCore.Loader.GetLevelByHash(m_LoadingMap_ToLoad_Hash);
                        if (l_LocalSong != null && SongCore.Loader.CustomLevels.ContainsKey(l_LocalSong.customLevelPath))
                        {
                            SDK.Game.Level.LoadSong(l_LocalSong.levelID, (p_BeatMapLevel) =>
                            {
                                m_LoadingMap_Loading        = false;
                                m_LoadingMap_Loaded         = true;
                                m_LoadingMap_LoadedLevel    = p_BeatMapLevel;

                                /// Send query
                                Network.ServerConnection.SendQuery(new Network.Methods.MatchInput()
                                {
                                    MatchID   = m_MatchID,
                                    InputType = RPC_STATE_LOADING_MAP__EVENT__MAP_LOADED,
                                    InputData = new JArray() { }
                                });

                                m_LoadingMap_LoadedLevel.GetCoverImageAsync(CancellationToken.None).ContinueWith(x =>
                                {
                                    HMMainThreadDispatcher.instance.Enqueue(() =>
                                    {
                                        if (!CanBeUpdated)
                                            return;

                                        m_LoadingMap_LevelCover = x.Result;
                                        HandleRPCData(m_RPCData, true);
                                    });
                                });
                            });
                        }
                        else if (m_LoadingMap_DownloadingRemainingTry > 0)
                        {
                            m_LoadingMap_DownloadingRemainingTry--;
                            m_LoadingMap_Downloading = true;

                            var l_Task = m_BeatSaver.Hash(m_LoadingMap_ToLoad_Hash);
                            l_Task.ContinueWith(x => {
                                OnBeatmapPopulated(x, m_LoadingMap_ToLoad_Hash, x.Result);

                                if (x.Result == null || x.Result.Partial)
                                {
                                    m_LoadingMap_Downloading = false;

                                    HandleRPCData(m_RPCData, true);
                                    return;
                                }

                                if (m_DownloadCancelTokenSource != null)
                                {
                                    m_DownloadCancelTokenSource.Cancel();
                                    m_DownloadCancelTokenSource = null;
                                }

                                m_DownloadCancelTokenSource = new CancellationTokenSource();

                                /// Start downloading
                                SDK.Game.BeatSaver.DownloadSong(x.Result, m_DownloadCancelTokenSource.Token, this).ContinueWith((y) =>
                                {
                                    if (y.IsCanceled || y.Status == TaskStatus.Canceled)
                                        return;

                                    if (y.Result)
                                    {
                                        /// Bind callback
                                        SongCore.Loader.SongsLoadedEvent += OnDownloadedSongLoaded;
                                        /// Refresh loaded songs
                                        SongCore.Loader.Instance.RefreshSongs(false);
                                    }
                                    else
                                    {
                                        /// Show error message
                                        HMMainThreadDispatcher.instance.Enqueue(() => {
                                            if (!CanBeUpdated)
                                                return;

                                            m_LoadingMap_Downloading = false;

                                            HandleRPCData(m_RPCData, true);
                                        });
                                    }
                                });
                            });
                        }
                    }
                }
                else
                {
                    m_MessageFrame.SetActive(false);
                    m_SongDetailFrame.SetActive(true);

                    ViewFlowCoordinator.Instance().ShowGamePlaySetup();

                    m_LoadingMap_ToLoad_Difficulty = p_Data["MapToLoad"]?["Difficulty"]?.Value<string>() ?? "";

                    if (!m_SongDetailFrame_Detail.FromSongCore( SongCore.Loader.GetLevelByHash(m_LoadingMap_ToLoad_Hash),
                                                                m_LoadingMap_LevelCover,
                                                                p_Data["MapToLoad"]?["GameMode"]?.Value<string>() ?? "",
                                                                m_LoadingMap_ToLoad_Difficulty,
                                                                out m_LoadingMap_LoadedLevelCharacteristic))
                    {
                        /// Hide song info panel
                        m_SongFrame_InfoPanel_Detail.SetActive(false);

                        /// Show message
                        SetMessageModal_PendingMessage("Ooops, the requested difficulty doesn't seem to exist !");
                        ShowMessageModal();

                        return;
                    }

                    /// Load the song clip to get duration
                    m_LoadingMap_LoadedLevel.GetPreviewAudioClipAsync(CancellationToken.None).ContinueWith(x =>
                    {
                        if (x.IsCompleted && x.Status == TaskStatus.RanToCompletion)
                        {
                            HMMainThreadDispatcher.instance.Enqueue(() =>
                            {
                                if (!CanBeUpdated)
                                    return;

                                var l_Duration = x.Result.length;

                                m_SongDetailFrame_Detail.Time = l_Duration;
                                m_SongDetailFrame_Detail.NPS  = (float)m_SongDetailFrame_Detail.Notes / l_Duration;

                                /// Start preview song
                                if (m_SongPreviewPlayer)
                                    m_SongPreviewPlayer.CrossfadeTo(x.Result, m_LoadingMap_LoadedLevel.previewStartTime, m_LoadingMap_LoadedLevel.previewDuration, false);
                            });
                        }
                    });
                }

                if (p_Data.ContainsKey("PlaylistWithScores"))
                    ViewFlowCoordinator.Instance().matchPlaylist.SetData(GeneratePlaylistWithScores());
                if (p_Data.ContainsKey("PlaylistWithScoresTitle"))
                    ViewFlowCoordinator.Instance().matchPlaylist.SetTitle(p_Data["PlaylistWithScoresTitle"].Value<string>());

                if (p_Data.ContainsKey("ReadyPlayers") && p_Data["ReadyPlayers"].Type == JTokenType.Array)
                {
                    var l_ReadyPlayers = p_Data["ReadyPlayers"] as JArray;
                    m_LoadingMap_Ready = l_ReadyPlayers.Any(x => x.Value<string>() == SDK.Game.UserPlatform.GetUserID());
                    m_SongDetailFrame_Detail.SetPlayButtonInteractable(!m_LoadingMap_Ready);
                }

                HideLoadingModal();
            }
            else if (m_RPCState == RPC_STATE_PLAYING)
            {
                m_NetworkController.MapStartTime = SDK.Misc.Time.FromUnixTime(p_Data["MapStartTime"]?.Value<Int64>() ?? 0);
                DateTime? l_Timeout = null;

                if (p_Data.ContainsKey("Timeout") && p_Data["Timeout"].Value<string>() != "-1")
                    l_Timeout = SDK.Misc.Time.FromUnixTime(p_Data["Timeout"]?.Value<Int64>() ?? 0);

                if (p_Data.ContainsKey("PlaylistWithScores"))
                    ViewFlowCoordinator.Instance().matchPlaylist.SetData(GeneratePlaylistWithScores());
                if (p_Data.ContainsKey("PlaylistWithScoresTitle"))
                    ViewFlowCoordinator.Instance().matchPlaylist.SetTitle(p_Data["PlaylistWithScoresTitle"].Value<string>());

                HideLoadingModal();

                if (!m_PlayingMap_LoadingStarted)
                {
                    SetMessageFrameText("Loading the map...");

                    m_PlayingMap_LoadingStarted = true;

                    var l_LocalSong = SongCore.Loader.GetLevelByHash(m_LoadingMap_ToLoad_Hash);
                    SDK.Game.Level.LoadSong(l_LocalSong.levelID, (p_BeatMapLevel) =>
                    {
                        BeatmapDifficulty l_Difficulty = SDK.Game.Level.SerializedToDifficulty(m_LoadingMap_ToLoad_Difficulty);

                        /// Fetch game settings
                        var l_PlayerData     = Resources.FindObjectsOfTypeAll<PlayerDataModel>().First().playerData;
                        var l_PlayerSettings = l_PlayerData.playerSpecificSettings;
                        var l_ColorScheme    = l_PlayerData.colorSchemesSettings.overrideDefaultColors ? l_PlayerData.colorSchemesSettings.GetSelectedColorScheme() : null;

                        /// Prevent refresh on activation
                        m_DontRefreshOnActivation = true;

                        HMMainThreadDispatcher.instance.Enqueue(() => SetMessageFrameText("Waiting for players to finish the map...", l_Timeout));

                        SDK.Game.Level.PlaySong(p_BeatMapLevel, m_LoadingMap_LoadedLevelCharacteristic, l_Difficulty, l_PlayerData.overrideEnvironmentSettings, l_ColorScheme, new GameplayModifiers(
                            false, false, GameplayModifiers.EnergyType.Bar, true, false, false, GameplayModifiers.EnabledObstacleType.All, false, false, false, false, GameplayModifiers.SongSpeed.Normal, false, false, false, false, false
                            ), l_PlayerSettings,
                        (p_Transition, p_LevelPlayResult, p_Difficulty) =>
                        {
                            /// Prepare data
                            int l_Score             = p_LevelPlayResult.rawScore;
                            int l_MaxCombo          = p_LevelPlayResult.maxCombo;
                            int l_CutCount          = p_LevelPlayResult.goodCutsCount;
                            int l_TotalNoteCount    = p_Difficulty.beatmapData.cuttableNotesType;
                            int l_MaxScore          = SDK.Game.Level.GetMaxScore(p_Difficulty);
                            float l_Accuracy        = SDK.Game.Level.GetScorePercentage(l_MaxScore, l_Score);

                            /// Send query
                            Network.ServerConnection.SendQuery(new Network.Methods.MatchInput()
                            {
                                MatchID   = m_MatchID,
                                InputType = RPC_STATE_PLAYING__EVENT__SCORE,
                                InputData = new JArray() { l_Score, l_Accuracy, l_CutCount, l_MaxCombo, l_TotalNoteCount }
                            });
                        });
                    });
                }
                else
                    SetMessageFrameText("Waiting for players to finish the map...", l_Timeout);
            }
            else if (m_RPCState == RPC_STATE_PLAY_SUMMARY)
            {
                if (p_Data.ContainsKey("Timeout") && p_Data["Timeout"].Value<string>() != "-1")
                    m_SongPlayDetailFrame_RemainingTimeEnd = SDK.Misc.Time.FromUnixTime(p_Data["Timeout"]?.Value<Int64>() ?? 0);

                if (p_Data.ContainsKey("PlaylistWithScores"))
                    ViewFlowCoordinator.Instance().matchPlaylist.SetData(GeneratePlaylistWithScores());
                if (p_Data.ContainsKey("PlaylistWithScoresTitle"))
                    ViewFlowCoordinator.Instance().matchPlaylist.SetTitle(p_Data["PlaylistWithScoresTitle"].Value<string>());

                if (p_Data.ContainsKey("MapHash")
                    && p_Data.ContainsKey("GameMode")
                    && p_Data.ContainsKey("Difficulty")
                    && p_Data.ContainsKey("Scores"))
                {
                    var l_Scores  = p_Data["Scores"] as JObject;
                    var l_MyScore = l_Scores.Values().SingleOrDefault((x) => (x as JObject).ContainsKey("ScoreSaberID") && x["ScoreSaberID"].Value<string>() == SDK.Game.UserPlatform.GetUserID()) as JObject;

                    if (l_MyScore != null)
                    {
                        m_SongPlayDetailFrame_Detail.Score          = l_MyScore["Score"]?.Value<int>() ?? -1;
                        m_SongPlayDetailFrame_Detail.Accuracy       = l_MyScore["Acc"]?.Value<float>() * 100f ?? -1f;
                        m_SongPlayDetailFrame_Detail.CutCount       = l_MyScore["CutCount"]?.Value<int>() ?? -1;
                        m_SongPlayDetailFrame_Detail.MaxCombo       = l_MyScore["MaxCombo"]?.Value<int>() ?? -1;
                        m_SongPlayDetailFrame_Detail.CuttableCount  = l_MyScore["NoteCount"]?.Value<int>() ?? -1;
                    }
                    else
                    {
                        m_SongPlayDetailFrame_Detail.Score          = -1;
                        m_SongPlayDetailFrame_Detail.Accuracy       = -1f;
                        m_SongPlayDetailFrame_Detail.CutCount       = -1;
                        m_SongPlayDetailFrame_Detail.MaxCombo       = -1;
                        m_SongPlayDetailFrame_Detail.CuttableCount  = -1;
                    }

                    m_SongPlayDetailFrame_Detail.Name           = GetMapName(p_Data["MapHash"].Value<string>());
                    m_SongPlayDetailFrame_Detail.AuthorNameText = GetMapAuthorName(p_Data["MapHash"].Value<string>());

                    var l_CoverTask = GetMapCover(p_Data["MapHash"].Value<string>());
                    if (l_CoverTask != null)
                    {
                        _ = l_CoverTask.ContinueWith(p_CoverTaskResult =>
                        {
                            if (m_RPCState == RPC_STATE_PLAY_SUMMARY)
                                HMMainThreadDispatcher.instance.Enqueue(() => m_SongPlayDetailFrame_Detail.Cover = p_CoverTaskResult.Result ?? SongCore.Loader.defaultCoverImage);
                        });
                    }

                    var l_Charac = SongCore.Loader.beatmapCharacteristicCollection.GetBeatmapCharacteristicBySerializedName(p_Data["GameMode"].Value<string>());

                    if (l_Charac != null)
                        m_SongPlayDetailFrame_Detail.Characteristic = l_Charac.icon;

                    m_SongPlayDetailFrame_Detail.Difficulty = SDK.Game.Level.SerializedToDifficultyName(p_Data["Difficulty"].Value<string>());
                }

                HideLoadingModal();
            }
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
        /// Rebuild song list
        /// </summary>
        /// <returns></returns>
        private void RebuildSongList()
        {
            /// Clear selection and items, then refresh the list
            m_SongFrame_List.TableViewInstance.ClearSelection();
            m_SongFrame_List.Data.Clear();

            /// Append all songs
            if (m_Songs != null)
            {
                for (int l_I = (m_CurrentPage - 1) * 7; l_I < (m_CurrentPage * 7); ++l_I)
                {
                    if (l_I >= m_Songs.Count)
                        break;

                    var l_SongEntry = m_Songs[l_I];
                    var l_SongHash = l_SongEntry.Raw.Hash.ToLower();
                    var l_LocalSong = SongCore.Loader.GetLevelByHash(l_SongHash);

                    /// If the map is already downloaded
                    if (l_LocalSong != null && SongCore.Loader.CustomLevels.ContainsKey(l_LocalSong.customLevelPath))
                    {
                        m_SongFrame_List.Data.Add(new SDK.UI.DataSource.SongList.Entry()
                        {
                            CustomLevel = l_LocalSong,
                            CustomData = l_SongEntry
                        });
                    }
                    /// Fetch information from beat saver
                    else
                    {
                        var l_RowEntry = new SDK.UI.DataSource.SongList.Entry();
                        l_RowEntry.CustomData = l_SongEntry;

                        if (l_SongEntry.BeatMap == null)
                        {
                            l_SongEntry.BeatMap = new BeatSaverSharp.Beatmap(m_BeatSaver, null, l_SongHash);
                            l_RowEntry.BeatSaver_Map = l_SongEntry.BeatMap;

                            _ = l_SongEntry.BeatMap.Populate().ContinueWith((x) =>
                            {
                                if (x.Status != TaskStatus.RanToCompletion)
                                    l_RowEntry.BeatSaver_Map = null;

                                HMMainThreadDispatcher.instance.Enqueue(() =>
                                {
                                    /// Refresh cells
                                    if (CanBeUpdated)
                                    {
                                        m_SongFrame_List.TableViewInstance.RefreshCellsContent();
                                        OnSongSelected(m_SongFrame_List.TableViewInstance, m_SelectedSongIndex);
                                    }
                                });
                            });

                        }
                        else
                            l_RowEntry.BeatSaver_Map = l_SongEntry.BeatMap;

                        m_SongFrame_List.Data.Add(l_RowEntry);
                    }
                }
            }

            /// Refresh the list
            m_SongFrame_List.TableViewInstance.ReloadData();

            /// Update UI
            m_SongFrame_UpButton.interactable   = m_CurrentPage != 1;
            m_SongFrame_DownButton.interactable = m_Songs != null ? (m_Songs.Count > (m_CurrentPage * 7)) : false;

            /// Select first one
            if (m_SongFrame_List.Data.Count > 0)
            {
                m_SongFrame_List.TableViewInstance.SelectCellWithIdx(0);
                m_SongFrame_List.DidSelectCellWithIdxEvent(m_SongFrame_List.TableViewInstance, 0);
                OnSongSelected(m_SongFrame_List.TableViewInstance, 0);
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
            m_SelectedSong               = null;
            m_SelectedSongCharacteristic = null;
            m_SelectedSongIndex          = p_Row;

            /// Hide if invalid song
            if (p_Row < 0
                || p_Row >= m_SongFrame_List.Data.Count
                || m_SongFrame_List.Data[p_Row].Invalid
                || (p_TableView != null && m_SongFrame_List.Data[p_Row].BeatSaver_Map != null && m_SongFrame_List.Data[p_Row].BeatSaver_Map.Partial))
            {
                /// Hide song info panel
                m_SongFrame_InfoPanel_Detail.SetActive(false);
                /// Stop preview music if any
                m_SongFrame_List.StopPreviewMusic();

                return;
            }

            /// Fetch song entry
            var l_SongRowData = m_SongFrame_List.Data[p_Row];

            /// Show song info panel
            m_SongFrame_InfoPanel_Detail.SetActive(true);

            var l_GameMode   = (l_SongRowData.CustomData as SongEntry).Raw.Mode;
            var l_Difficulty = (l_SongRowData.CustomData as SongEntry).Raw.Difficulty;

            if (l_SongRowData.CustomLevel != null && SongCore.Loader.CustomLevels.ContainsKey(l_SongRowData.CustomLevel.customLevelPath))
            {
                if (!m_SongFrame_InfoPanel_Detail.FromSongCore(l_SongRowData.CustomLevel, l_SongRowData.Cover, l_GameMode, l_Difficulty, out m_SelectedSongCharacteristic))
                {
                    /// Hide song info panel
                    m_SongFrame_InfoPanel_Detail.SetActive(false);

                    /// Show message
                    SetMessageModal_PendingMessage("Ooops, the requested difficulty doesn't seem to exist !");
                    ShowMessageModal();

                    return;
                }

                /// Store selected song
                m_SelectedSong = l_SongRowData;
            }
            else
            {
                if (!m_SongFrame_InfoPanel_Detail.FromBeatSaver(l_SongRowData.BeatSaver_Map, l_SongRowData.Cover, l_GameMode, l_Difficulty, out m_SelectedSongCharacteristic))
                {
                    /// Hide song info panel
                    m_SongFrame_InfoPanel_Detail.SetActive(false);

                    /// Show message
                    SetMessageModal_PendingMessage("Ooops, the requested difficulty doesn't seem to exist !");
                    ShowMessageModal();

                    return;
                }

                /// Store selected song
                m_SelectedSong = l_SongRowData;
            }
        }
        /// <summary>
        /// On song cover fetched
        /// </summary>
        /// <param name="p_RowData">Row data</param>
        private void OnSongCoverFetched(int p_Index, SDK.UI.DataSource.SongList.Entry p_RowData)
        {
            if (m_SelectedSongIndex != p_Index)
                return;

            OnSongSelected(m_SongFrame_List.TableViewInstance, m_SelectedSongIndex);
        }
        /// <summary>
        /// On song play button pressed
        /// </summary>
        private void OnSongPlayPressed()
        {
            if (m_SelectedSong == null || m_SelectedSongCharacteristic == null)
                return;

            if (m_RPCState == RPC_STATE_PICK_BAN_MAP)
            {
                bool l_IsBan = (m_RPCData["CurrentActionType"]?.Value<string>() ?? "") != "pick";

                var l_LocalSong = m_SelectedSong.CustomLevel;

                m_ConfirmPickBanMessageModal_Text.text  = l_IsBan ? "Do you really want to <color=red><b>ban</b></color>\n" : "Do you really want to <color=green><b>pick</b></color>\n";
                m_ConfirmPickBanMessageModal_Text.text += "<u>" + (l_LocalSong != null ? l_LocalSong.songName : m_SelectedSong.BeatSaver_Map.Metadata.SongName) + "</u>\n";

                m_ParserParams.EmitEvent("ConfirmPickBanMessageModal");
            }
        }
        /// <summary>
        /// Do pick/ban
        /// </summary>
        private void OnDoPickBanPressed()
        {
            if (m_SelectedSong == null || m_SelectedSongCharacteristic == null)
                return;

            /// Update UI
            m_ParserParams.EmitEvent("CloseConfirmPickBanMessageModal");
            ShowLoadingModal();

            /// Send query
            Network.ServerConnection.SendQuery(new Network.Methods.MatchInput()
            {
                MatchID   = m_MatchID,
                InputType = RPC_STATE_PICK_BAN_MAP__EVENT__DO_PICK_BAN,
                InputData = new JArray()
                {
                    m_SelectedSong.GetLevelHash()
                }
            });
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On ready pressed
        /// </summary>
        private void OnReadyPressed()
        {
            if (m_RPCState != RPC_STATE_LOADING_MAP || (m_RPCData["State"]?.Value<string>() ?? "loading") != "ready_check")
                return;

            /// Update state
            m_LoadingMap_Ready = true;

            /// Send query
            Network.ServerConnection.SendQuery(new Network.Methods.MatchInput()
            {
                MatchID   = m_MatchID,
                InputType = RPC_STATE_LOADING_MAP__EVENT__READY,
                InputData = new JArray() { }
            });
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On replay pressed
        /// </summary>
        private void OnReplayButton()
        {
            if (m_RPCState != RPC_STATE_PLAY_SUMMARY)
                return;

            /// Update state
            m_PlaySummary_ReplayRequested = true;
            m_SongPlayDetailFrame_Detail.SetButtonInteractable(false);

            /// Send query
            Network.ServerConnection.SendQuery(new Network.Methods.MatchInput()
            {
                MatchID   = m_MatchID,
                InputType = RPC_STATE_PLAY_SUMMARY__EVENT__ASK_REPLAY,
                InputData = new JArray() { }
            });
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When the match join received
        /// </summary>
        /// <param name="p_Result">Result</param>
        [Network.Methods._MethodResultHandler]
        static private void OnMatchJoin(Network.Methods.MatchJoin_Result p_Result)
        {
            /// Avoid refresh if not active view anymore
            if (!CanBeUpdated)
                return;

            /// Should back to tournament select
            if (p_Result.BackToTournamentSelect)
            {
                /// Go back to tournament select with a message
                ViewFlowCoordinator.Instance().tournamentSelect.SetMessageModal_PendingMessage(p_Result.BackMessage);
                ViewFlowCoordinator.Instance().SwitchToTournamentSelect();

                return;
            }

            /// Update state and UI
            Instance.m_MatchID                    = p_Result.MatchID;
            Instance.m_MatchName                  = p_Result.MatchName;
            Instance.m_TournamentNameText.text    = Instance.m_TournamentName + " | " + p_Result.MatchName;

            /// Switch UI
            Instance.SwitchUI(RPC_STATE_WAITING_USERS);
            /// Update UI
            Instance.SetMessageFrameText("Waiting for coordinator...");

            /// Create network controller
            Instance.m_NetworkController = (new GameObject()).AddComponent<Match_Network>();

            /// Make GameObject to stay on scene change
            GameObject.DontDestroyOnLoad(Instance.m_NetworkController.gameObject);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get map name
        /// </summary>
        /// <param name="p_Hash">Hash of the map</param>
        /// <returns></returns>
        private string GetMapName(string p_Hash)
        {
            if (m_MapNameCache.TryGetValue(p_Hash.ToLower(), out var l_Name))
                return l_Name;

            var l_LocalSong = SongCore.Loader.GetLevelByHash(p_Hash);
            if (l_LocalSong != null && SongCore.Loader.CustomLevels.ContainsKey(l_LocalSong.customLevelPath))
            {
                m_MapNameCache.TryAdd(p_Hash.ToLower(), l_LocalSong.songName);
                return l_LocalSong.songName;
            }
            else
            {
                var l_Task = m_BeatSaver.Hash(p_Hash);
                l_Task.ContinueWith(x => OnBeatmapPopulated(x, p_Hash, x.Result));
            }

            return p_Hash.ToLower();
        }
        /// <summary>
        /// Get map name
        /// </summary>
        /// <param name="p_Hash">Hash of the map</param>
        /// <returns></returns>
        private string GetMapAuthorName(string p_Hash)
        {
            if (m_MapAuthorNameCache.TryGetValue(p_Hash.ToLower(), out var l_Name))
                return l_Name;

            var l_LocalSong = SongCore.Loader.GetLevelByHash(p_Hash);
            if (l_LocalSong != null && SongCore.Loader.CustomLevels.ContainsKey(l_LocalSong.customLevelPath))
            {
                m_MapAuthorNameCache.TryAdd(p_Hash.ToLower(), l_LocalSong.songName);
                return l_LocalSong.songAuthorName;
            }
            else
            {
                var l_Task = m_BeatSaver.Hash(p_Hash);
                l_Task.ContinueWith(x => OnBeatmapPopulated(x, p_Hash, x.Result));
            }

            return "";
        }
        /// <summary>
        /// Get map name
        /// </summary>
        /// <param name="p_Hash">Hash of the map</param>
        /// <returns></returns>
        private Task<Sprite> GetMapCover(string p_Hash)
        {
            var l_LocalSong = SongCore.Loader.GetLevelByHash(p_Hash);
            if (l_LocalSong != null && SongCore.Loader.CustomLevels.ContainsKey(l_LocalSong.customLevelPath))
                return l_LocalSong.GetCoverImageAsync(CancellationToken.None);

            return Task.FromResult<Sprite>(null);
        }
        /// <summary>
        /// When a beatmap get fully loaded
        /// </summary>
        /// <param name="p_Task">Task instance</param>
        /// <param name="p_Hash">Hash of the song</param>
        /// <param name="p_BeatMap">Beatmap instance</param>
        private static void OnBeatmapPopulated(Task p_Task, string p_Hash, BeatSaverSharp.Beatmap p_BeatMap)
        {
            if (p_Task.Status != TaskStatus.RanToCompletion)
                return;

            /// Avoid refresh if not active view anymore
            if (!CanBeUpdated)
                return;

            if (p_BeatMap == null || p_BeatMap.Partial)
            {
                Instance.m_MapNameCache.TryAdd(p_Hash.ToLower(), p_Hash.ToLower());
                return;
            }

            Instance.m_MapNameCache.TryAdd(p_Hash.ToLower(), p_BeatMap.Metadata.SongName);
            Instance.m_MapAuthorNameCache.TryAdd(p_Hash.ToLower(), p_BeatMap.Metadata.SongAuthorName);

            if (Instance.m_RPCState == RPC_STATE_PICK_BAN_MAP)
            {
                HMMainThreadDispatcher.instance.Enqueue(() =>
                {
                    /// Avoid refresh if not active view anymore
                    if (!CanBeUpdated)
                        return;

                    ViewFlowCoordinator.Instance().matchPickBanList.SetData(Instance.GeneratePickBanList());
                });
            }
            else if (Instance.m_RPCState == RPC_STATE_LOADING_MAP || Instance.m_RPCState == RPC_STATE_PLAYING || Instance.m_RPCState == RPC_STATE_PLAY_SUMMARY)
            {
                HMMainThreadDispatcher.instance.Enqueue(() =>
                {
                    /// Avoid refresh if not active view anymore
                    if (!CanBeUpdated)
                        return;

                    ViewFlowCoordinator.Instance().matchPlaylist.SetData(Instance.GeneratePlaylistWithScores());
                });
            }

            if (Instance.m_RPCState == RPC_STATE_PLAY_SUMMARY)
            {
                HMMainThreadDispatcher.instance.Enqueue(() =>
                {
                    /// Avoid refresh if not active view anymore
                    if (!CanBeUpdated)
                        return;

                    Instance.HandleRPCData(Instance.m_RPCData);
                });
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On download progress reported
        /// </summary>
        /// <param name="p_Value"></param>
        void IProgress<double>.Report(double p_Value)
        {
            /// Avoid refresh if not active view anymore
            if (!CanBeUpdated)
                return;

            m_LoadingMap_DownloadingProgress = (float)p_Value;
        }
        /// <summary>
        /// When a downloaded song is downloaded
        /// </summary>
        /// <param name="p_Loader">Loader instance</param>
        /// <param name="p_Maps">All loaded songs</param>
        private static void OnDownloadedSongLoaded(SongCore.Loader p_Loader, ConcurrentDictionary<string, CustomPreviewBeatmapLevel> p_Maps)
        {
            /// Remove callback
            SongCore.Loader.SongsLoadedEvent -= OnDownloadedSongLoaded;

            /// Avoid refresh if not active view anymore
            if (!CanBeUpdated)
                return;

            HMMainThreadDispatcher.instance.Enqueue(() =>
            {
                if (!CanBeUpdated)
                    return;

                Instance.m_LoadingMap_Downloading = false;
                Instance.HandleRPCData(Instance.m_RPCData, true);
            });
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Generate pick ban list
        /// </summary>
        /// <returns></returns>
        private List<(bool, string, string)> GeneratePickBanList()
        {
            List<(bool, string, string)> l_PickBanList = new List<(bool, string, string)>();

            if (m_RPCState != RPC_STATE_PICK_BAN_MAP)
                return l_PickBanList;

            JArray l_Bans = m_RPCData.ContainsKey("Bans") ? m_RPCData["Bans"] as JArray : null;
            JArray l_Picks = m_RPCData.ContainsKey("Picks") ? m_RPCData["Picks"] as JArray : null;

            if (l_Bans != null && l_Picks != null)
            {
                int l_TurnCount = l_Bans.Count + l_Picks.Count;
                int l_BanIt = 0;
                int l_PickIt = 0;
                for (int l_I = 0; l_I < l_TurnCount; ++l_I)
                {
                    bool l_IsInBanTurn = ((l_I / 2) % 2) == 0;
                    int l_RelOffset = l_IsInBanTurn ? l_BanIt++ : l_PickIt++;

                    if (l_IsInBanTurn && l_RelOffset < l_Bans.Count)
                        l_PickBanList.Add((true, l_Bans[l_RelOffset]["UserName"]?.Value<string>() ?? "?", GetMapName(l_Bans[l_RelOffset]["Map"]?["Hash"]?.Value<string>() ?? "?")));
                    else if (!l_IsInBanTurn && l_RelOffset < l_Picks.Count)
                        l_PickBanList.Add((false, l_Picks[l_RelOffset]["UserName"]?.Value<string>() ?? "?", GetMapName(l_Picks[l_RelOffset]["Map"]?["Hash"]?.Value<string>() ?? "?")));
                }
            }

            return l_PickBanList;
        }
        /// <summary>
        /// Parse play list with scores array
        /// </summary>
        /// <returns></returns>
        private List<(bool, string, float, string, string, float)> GeneratePlaylistWithScores()
        {
            List<(bool, string, float, string, string, float)> l_Result = new List<(bool, string, float, string, string, float)>();

            if ((m_RPCState == RPC_STATE_LOADING_MAP || m_RPCState == RPC_STATE_PLAYING || m_RPCState == RPC_STATE_PLAY_SUMMARY) && m_RPCData.ContainsKey("PlaylistWithScores"))
            {
                var l_Data = m_RPCData["PlaylistWithScores"] as JArray;

                for (int l_I = 0; l_I < l_Data.Count; ++l_I)
                {
                    var l_Row = l_Data[l_I];

                    l_Result.Add((
                        l_I == (l_Data.Count - 1),
                        l_Row["LeftName"].Value<string>(),
                        l_Row["LeftAcc"].Value<float>(),
                        Instance.GetMapName(l_Row["Hash"].Value<string>()),
                        l_Row["RightName"].Value<string>(),
                        l_Row["RightAcc"].Value<float>()
                    ));
                }
            }

            return l_Result;
        }
    }
}
