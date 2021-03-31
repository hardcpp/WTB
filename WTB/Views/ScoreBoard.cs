using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using HMUI;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace WTB.Views
{
    /// <summary>
    /// Score Board view
    /// </summary>
    internal class ScoreBoard : SDK.UI.ViewController<ScoreBoard>
    {
        /// <summary>
        /// Total score line per page
        /// </summary>
        private static int SCORE_PER_PAGE = 10;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

#pragma warning disable CS0649
        [UIObject("TypeSegmentPanel")]
        private GameObject m_TypeSegmentPanel;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIObject("SegmentPanel")]
        private GameObject m_SegmentPanel;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIObject("background")]
        private GameObject m_Background = null;
        [UIComponent("ScoreUpButton")]
        private Button m_ScoreUpButton;
        [UIObject("ScoreList")]
        private GameObject m_ScoreListView = null;
        private SDK.UI.DataSource.SimpleTextList m_ScoreList = null;
        [UIObject("LoadingIndicator")]
        private GameObject m_LoadingIndicator;
        [UIComponent("ScoreDownButton")]
        private Button m_ScoreDownButton;
#pragma warning restore CS0649

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Current tournament ID
        /// </summary>
        private int m_CurrentTournamentID;
        /// <summary>
        /// Current song
        /// </summary>
        private string m_CurrentSongHash;
        /// <summary>
        /// Is the map ranking
        /// </summary>
        private bool m_IsMapRanking = true;
        /// <summary>
        /// Current score type
        /// </summary>
        private bool m_FocusSelf = false;
        /// <summary>
        /// Current score list page
        /// </summary>
        private int m_CurrentPage = 1;
        /// <summary>
        /// Has more scores
        /// </summary>
        private bool m_HasMorePage = false;
        /// <summary>
        /// Type segment control
        /// </summary>
        private TextSegmentedControl m_TypeSegmentControl = null;
        /// <summary>
        /// Focus segment control
        /// </summary>
        private IconSegmentedControl m_FocusSegmentControl = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get view content description XML
        /// </summary>
        /// <returns></returns>
        protected override string GetViewContentDescription()
        {
            string BSML_RESOURCE_RAW = "<vertical child-control-height='false' spacing='0' pad='0'> <horizontal bg='panel-top' pad-left='15' pad-right='15' horizontal-fit='PreferredSize'> <text text='ScoreBoard' align='Center' font-size='4.2'/> </horizontal> <horizontal id='TypeSegmentPanel' pad-top='2' pref-height='7' pad-left='8' pad-right='8'> </horizontal> <horizontal spacing='0' pad='0'> <vertical min-width='12' pref-height='10' spacing='0' pad='0' pad-right='5'> <horizontal id='SegmentPanel' min-width='12' min-height='30' > </horizontal> </vertical> <vertical min-width='90' spacing='0' pad='0'> <page-button id='ScoreUpButton' direction='Up'></page-button> <vertical id='background' bg='round-rect-panel' min-width='90' pref-height='55' spacing='0' pad='2' pad-top='1'> <list id='ScoreList'> <loading id='LoadingIndicator' source='#LoadingIndicator' active='false'></loading> </list> </vertical> <page-button id='ScoreDownButton' direction='Down'></page-button> </vertical> </horizontal></vertical>";
            return BSML_RESOURCE_RAW;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override void OnViewCreation()
        {
            /// Update background color
            SDK.UI.Backgroundable.SetOpacity(m_Background, 0.5f);

            /// Create type selector
            m_TypeSegmentControl = SDK.UI.TextSegmentedControl.Create(m_TypeSegmentPanel.transform as RectTransform, false, new string[] { "Map ranking", "Tournament ranking" });

            /// Scale down up & down button
            m_ScoreUpButton.transform.localScale    = Vector3.one * 0.5f;
            m_ScoreDownButton.transform.localScale  = Vector3.one * 0.5f;

            /// Prepare score list
            var l_BSMLTableView = m_ScoreListView.GetComponentInChildren<BSMLTableView>();
            l_BSMLTableView.SetDataSource(null, false);
            GameObject.DestroyImmediate(m_ScoreListView.GetComponentInChildren<CustomListTableData>());
            m_ScoreList = l_BSMLTableView.gameObject.AddComponent<SDK.UI.DataSource.SimpleTextList>();
            m_ScoreList.TableViewInstance = l_BSMLTableView;
            l_BSMLTableView.SetDataSource(m_ScoreList, false);

            /// Create score type selector
            m_FocusSegmentControl = SDK.UI.VerticalIconSegmentedControl.Create(m_SegmentPanel.transform as RectTransform, true);
            var l_GlobalIcon = Resources.FindObjectsOfTypeAll<Sprite>().First(x => x.name == "GlobalIcon");
            var l_PlayerIcon = Resources.FindObjectsOfTypeAll<Sprite>().First(x => x.name == "PlayerIcon");

            /// Set icons
            if (l_GlobalIcon && l_PlayerIcon)
            {
                m_FocusSegmentControl.SetData(new List<IconSegmentedControl.DataItem>() {
                        new IconSegmentedControl.DataItem(l_GlobalIcon, "Global"),
                        new IconSegmentedControl.DataItem(l_PlayerIcon, "Me")
                }.ToArray());
            }

            /// Bind events
            m_TypeSegmentControl.didSelectCellEvent  += OnScoreTypeChanged;
            m_FocusSegmentControl.didSelectCellEvent += OnScoreFocusChanged;
            m_ScoreUpButton.onClick.AddListener(OnScorePageUpPressed);
            m_ScoreDownButton.onClick.AddListener(OnScorePageDownPressed);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set selected modifiers
        /// </summary>
        /// <param name="p_MapRanking">Focus map ranking ?</param>
        /// <param name="p_FocusSelf">Focus self ?</param>
        internal void SetSelectedModifiers(bool p_MapRanking, bool p_FocusSelf)
        {
            if (!UICreated)
            {
                Logger.log?.Error("[ScoreBoard] Set selected modifiers called before UI creation!");
                return;
            }

            m_TypeSegmentControl.SelectCellWithNumber(p_MapRanking ? 0 : 1);
            m_FocusSegmentControl.SelectCellWithNumber(p_FocusSelf ? 1 : 0);

            m_IsMapRanking = p_MapRanking;
            m_FocusSelf    = p_FocusSelf;
        }
        /// <summary>
        /// On score board type changed
        /// </summary>
        /// <param name="p_SegmentControl">Segment control instance</param>
        /// <param name="p_Index">Selected </param>
        private void OnScoreTypeChanged(SegmentedControl p_SegmentControl, int p_Index)
        {
            /// Update type
            m_IsMapRanking = p_Index == 0;

            /// Refresh scores
            LoadScore(m_CurrentTournamentID, m_CurrentSongHash);
        }
        /// <summary>
        /// Load score
        /// </summary>
        /// <param name="p_TournamentID">ID of the tournament</param>
        /// <param name="p_SongHash">Hash of the song</param>
        internal void LoadScore(int p_TournamentID, string p_SongHash)
        {
            /// Clear previous scores
            ClearScoreList();

            /// Show loading animation
            m_LoadingIndicator.SetActive(true);

            /// Store settings
            m_CurrentTournamentID   = p_TournamentID;
            m_CurrentSongHash       = p_SongHash;

            /// Reset page
            m_CurrentPage = 1;
            m_HasMorePage = false;

            /// Update UI
            m_ScoreUpButton.interactable   = m_CurrentPage != 1;
            m_ScoreDownButton.interactable = m_HasMorePage;

            /// Query scores
            Network.ServerConnection.SendQuery(new Network.Methods.ScoreBoard()
            {
                TournamentID    = m_CurrentTournamentID,
                Song            = m_CurrentSongHash,
                Page            = m_CurrentPage,
                FocusSelf       = m_FocusSelf,
                IsMapScoreBoard = m_IsMapRanking
            });
        }
        /// <summary>
        /// Go to previous score page
        /// </summary>
        private void OnScorePageUpPressed()
        {
            /// Underflow check
            if (m_CurrentPage < 2)
                return;

            /// Decrement current page
            m_CurrentPage--;

            /// Clear previous scores
            ClearScoreList();

            /// Show loading animation
            m_LoadingIndicator.SetActive(true);

            /// Query scores
            Network.ServerConnection.SendQuery(new Network.Methods.ScoreBoard()
            {
                TournamentID    = m_CurrentTournamentID,
                Song            = m_CurrentSongHash,
                Page            = m_CurrentPage,
                FocusSelf       = false,
                IsMapScoreBoard = m_IsMapRanking
            });
        }
        /// <summary>
        /// Go to next score page
        /// </summary>
        private void OnScorePageDownPressed()
        {
            /// Overflow check
            if (!m_HasMorePage)
                return;

            /// Increment current page
            m_CurrentPage++;

            /// Clear previous scores
            ClearScoreList();

            /// Show loading animation
            m_LoadingIndicator.SetActive(true);

            /// Query scores
            Network.ServerConnection.SendQuery(new Network.Methods.ScoreBoard()
            {
                TournamentID    = m_CurrentTournamentID,
                Song            = m_CurrentSongHash,
                Page            = m_CurrentPage,
                FocusSelf       = false,
                IsMapScoreBoard = m_IsMapRanking
            });
        }
        /// <summary>
        /// When the score type is changed
        /// </summary>
        /// <param name="p_SegmentControl">Segment control instance</param>
        /// <param name="p_Index">Selected </param>
        private void OnScoreFocusChanged(SegmentedControl p_SegmentControl, int p_Index)
        {
            /// Change type
            m_FocusSelf = p_Index == 1;

            /// Re-query scores
            LoadScore(m_CurrentTournamentID, m_CurrentSongHash);
        }
        /// <summary>
        /// On score list method result
        /// </summary>
        /// <param name="p_Result">Result data</param>
        [Network.Methods._MethodResultHandler]
        static private void OnScoreList(Network.Methods.ScoreBoard_Result p_Result)
        {
            if (!CanBeUpdated)
                return;

            /// Clear data
            Instance.ClearScoreList();

            /// Update page
            Instance.m_CurrentPage = p_Result.Page;
            Instance.m_HasMorePage = p_Result.HasMore;

            /// Update UI
            Instance.m_ScoreUpButton.interactable     = Instance.m_CurrentPage > 1;
            Instance.m_ScoreDownButton.interactable   = Instance.m_HasMorePage;

            /// Build new score list
            int l_CurrentScoreRank = (Instance.m_CurrentPage - 1) * SCORE_PER_PAGE;
            foreach (var l_Score in p_Result.Scores)
            {
                Instance.m_ScoreList.Data.Add(Instance.BuildLineString(
                    ++l_CurrentScoreRank,
                    l_Score.Name,
                    l_Score.Accuracy,
                    l_Score.Score,
                    l_Score.CutCount,
                    l_Score.TotalNoteCount,
                    l_Score.MaxCombo,
                    l_CurrentScoreRank == p_Result.MyRank,
                    l_Score.Qualified)
                );
            }

            /// Hide loading animation
            Instance.m_LoadingIndicator.SetActive(false);

            /// Refresh display
            Instance.m_ScoreList.TableViewInstance.ReloadData();
        }
        /// <summary>
        /// Clear the score list
        /// </summary>
        private void ClearScoreList()
        {
            if (!UICreated)
                return;

            m_ScoreList.TableViewInstance.ClearSelection();
            m_ScoreList.Data.Clear();
            m_ScoreList.TableViewInstance.ReloadData();
        }
        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Build score line
        /// </summary>
        /// <param name="p_Rank">Score rank</param>
        /// <param name="p_Name">Player discord tag</param>
        /// <param name="p_Accurary">Score accuracy</param>
        /// <param name="p_Score">Score</param>
        /// <param name="p_IsSelf">Is me ?</param>
        /// <returns>Built score line</returns>
        private (string, string) BuildLineString(int p_Rank, string p_Name, float p_Accurary, int p_Score, int p_CutCount, int p_TotalNoteCount, int p_MaxCombo, bool p_IsSelf, bool p_Qualified)
        {
            /// Result line
            string l_Text = "";

            if (m_IsMapRanking)
                p_Qualified = false;

            /// Prepare number formatter
            var l_NumberFormat = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            l_NumberFormat.NumberGroupSeparator = " ";

            /// Fake line height
            l_Text += "<line-height=1%>";
            /// Left part
            l_Text += p_Qualified ? "<color=#00B00E>" : "";
            l_Text += p_IsSelf ? (p_Qualified ? "<color=#00FF0E>" : "<color=#008eff>") : "";
            l_Text += string.Format("<align=\"left\">{0}<pos=10%><smallcaps>{1}</smallcaps> - (<size=90%><color=#f6d031>{2:#,0.##}%</color></size>)</align>", p_Rank, p_Name, p_Accurary * 100.0f);
            /// Line break
            l_Text += "\n";
            /// Restore line height
            l_Text += "<line-height=100%>";
            /// Right part
            l_Text += string.Format("<align=\"right\"><mspace=0.35em>{0}</mspace><space=0.2em></align>", p_Score.ToString("#,0", l_NumberFormat));

            ////////////////////////////////////////////////////////////////////////////

            string l_HoverHint = null;
            if (m_IsMapRanking)
            {
                l_HoverHint  = "Notes: " + p_CutCount + "/" + p_TotalNoteCount + " (";
                l_HoverHint += (p_TotalNoteCount - p_CutCount) + " miss " + p_MaxCombo + " max combo)";
            }

            return (l_Text, l_HoverHint);
        }
    }
}
