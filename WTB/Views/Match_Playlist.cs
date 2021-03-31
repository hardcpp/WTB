using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WTB.Views
{
    /// <summary>
    /// Match playlist view
    /// </summary>
    internal class Match_Playlist : SDK.UI.ViewController<Match_Playlist>
    {
        /// <summary>
        /// Total pick ban line per page
        /// </summary>
        private static int SCORES_PER_PAGE = 10;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

#pragma warning disable CS0649
        [UIObject("Title")]
        private GameObject m_Title = null;
        [UIObject("Background")]
        private GameObject m_Background = null;
        [UIComponent("PlayListUpButton")]
        private Button m_PlayListUpButton;
        [UIObject("PlayList")]
        private GameObject m_PlayListView = null;
        private SDK.UI.DataSource.SimpleTextList m_PlayList = null;
        [UIComponent("PlayListDownButton")]
        private Button m_PlayListDownButton;
#pragma warning restore CS0649

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Pending title
        /// </summary>
        private string m_PendingTitle = null;
        /// <summary>
        /// Current score list page
        /// </summary>
        private int m_CurrentPage = 1;
        /// <summary>
        /// Has more scores
        /// </summary>
        private bool m_HasMorePage = false;
        /// <summary>
        /// Data
        /// </summary>
        private List<(bool, string, float, string, string, float)> m_Data = new List<(bool, string, float, string, string, float)>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get view content description XML
        /// </summary>
        /// <returns></returns>
        protected override string GetViewContentDescription()
        {
            string BSML_RESOURCE_RAW = "<vertical child-control-height='false' spacing='0' pad='0'> <horizontal bg='panel-top' pad-left='15' pad-right='15' horizontal-fit='PreferredSize'> <text id='Title' text='Scores' align='Center' font-size='4.2'/> </horizontal> <horizontal spacing='0' pad='0'> <vertical min-width='110' spacing='0' pad='0'> <page-button id='PlayListUpButton' direction='Up'></page-button> <vertical id='Background' bg='round-rect-panel' min-width='110' pref-height='55' spacing='0' pad='2' pad-top='1'> <list id='PlayList'> </list> </vertical> <page-button id='PlayListDownButton' direction='Down'></page-button> </vertical> </horizontal></vertical>";
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

            /// Scale down up & down button
            m_PlayListUpButton.transform.localScale   = Vector3.one * 0.5f;
            m_PlayListDownButton.transform.localScale = Vector3.one * 0.5f;

            /// Prepare play list list
            var l_BSMLTableView = m_PlayListView.GetComponentInChildren<BSMLTableView>();
            l_BSMLTableView.SetDataSource(null, false);
            GameObject.DestroyImmediate(m_PlayListView.GetComponentInChildren<CustomListTableData>());
            m_PlayList = l_BSMLTableView.gameObject.AddComponent<SDK.UI.DataSource.SimpleTextList>();
            m_PlayList.TableViewInstance = l_BSMLTableView;
            l_BSMLTableView.SetDataSource(m_PlayList, false);

            /// Bind events
            m_PlayListUpButton.onClick.AddListener(OnScorePageUpPressed);
            m_PlayListDownButton.onClick.AddListener(OnScorePageDownPressed);
        }
        /// <summary>
        /// On view activation
        /// </summary>
        protected override void OnViewActivation()
        {
            /// Rebuild list
            SetData(m_Data);

            /// Process pending title
            if (m_PendingTitle != null)
            {
                SetTitle(m_PendingTitle);
                m_PendingTitle = null;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Load play list
        /// </summary>
        /// <param name="p_Data">New data set</param>
        internal void SetData(List<(bool, string, float, string, string, float)> p_Data)
        {
            /// Clear previous scores
            ClearDisplayedData();

            /// Store data
            m_Data = p_Data;

            /// Reset page
            m_CurrentPage = (m_CurrentPage * SCORES_PER_PAGE) >= p_Data.Count ? 1 : m_CurrentPage;
            m_HasMorePage = p_Data.Count > SCORES_PER_PAGE;

            /// Update UI
            if (UICreated)
            {
                m_PlayListUpButton.interactable   = m_CurrentPage != 1;
                m_PlayListDownButton.interactable = m_HasMorePage;
            }

            /// Rebuild list
            RebuildList();
        }
        /// <summary>
        /// Set title
        /// </summary>
        /// <param name="p_Title">New title</param>
        internal void SetTitle(string p_Title)
        {
            if (UICreated)
                m_Title.GetComponentInChildren<TextMeshProUGUI>().text = p_Title;
            else
                m_PendingTitle = p_Title;
        }
        /// <summary>
        /// Go to previous play list page
        /// </summary>
        private void OnScorePageUpPressed()
        {
            /// Underflow check
            if (m_CurrentPage < 2)
                return;

            /// Decrement current page
            m_CurrentPage--;

            /// Clear previous scores
            ClearDisplayedData();

            /// Rebuild list
            RebuildList();
        }
        /// <summary>
        /// Go to next play list page
        /// </summary>
        private void OnScorePageDownPressed()
        {
            /// Overflow check
            if (!m_HasMorePage)
                return;

            /// Increment current page
            m_CurrentPage++;

            /// Clear previous scores
            ClearDisplayedData();

            /// Rebuild list
            RebuildList();
        }
        /// <summary>
        /// Rebuild list
        /// </summary>
        private void RebuildList()
        {
            if (!UICreated)
                return;

            /// Clear old entries
            ClearDisplayedData();

            for (int l_I = (m_CurrentPage - 1) * SCORES_PER_PAGE; l_I < m_Data.Count && l_I < (m_CurrentPage * SCORES_PER_PAGE); ++l_I)
            {
                m_PlayList.Data.Add(BuildLineString(
                    m_Data[l_I].Item1,
                    m_Data[l_I].Item2,
                    m_Data[l_I].Item3,
                    m_Data[l_I].Item4,
                    m_Data[l_I].Item5,
                    m_Data[l_I].Item6
                ));
            }

            /// Refresh
            m_PlayList.TableViewInstance.ReloadData();
        }
        /// <summary>
        /// Clear the pick ban list
        /// </summary>
        private void ClearDisplayedData()
        {
            if (!UICreated)
                return;

            m_PlayList.TableViewInstance.ClearSelection();
            m_PlayList.Data.Clear();
            m_PlayList.TableViewInstance.ReloadData();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Build play list line
        /// </summary>
        /// <param name="p_IsTieBreaker">Is tie breaker map</param>
        /// <param name="p_TeamAName">Team A name</param>
        /// <param name="p_TeamAAcc">Team A score</param>
        /// <param name="p_MapName">Name of the map</param>
        /// <param name="p_TeamBName">Team B name</param>
        /// <param name="p_TeamBAcc">Team B score</param>
        /// <returns>Built  score line</returns>
        private (string, string) BuildLineString(bool p_IsTieBreaker, string p_TeamAName, float p_TeamAAcc, string p_MapName, string p_TeamBName, float p_TeamBAcc)
        {
            /// Prepare number formatter
            var l_NumberFormat = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            l_NumberFormat.NumberGroupSeparator = " ";

            /// Result line
            string l_Text = "";

            /// Fake line height
            l_Text += "<line-height=1%>";

            if (p_TeamAAcc >= 0f)
                l_Text += string.Format("<align=\"left\"><color={0}>{1} (<size=90%><color=#f6d031>{2:#,0.##}%</color></size>)", p_TeamAAcc >= p_TeamBAcc ? "green" : "red", p_TeamAName, p_TeamAAcc * 100f);

            /// Line break
            l_Text += "\n";
            /// Fake line height
            l_Text += "<line-height=1%>";

            if (p_TeamBAcc >= 0f)
                l_Text += string.Format("<align=\"right\"><color={0}>(<size=90%><color=#f6d031>{1:#,0.##}%</color></size>) {2}", p_TeamBAcc >= p_TeamAAcc ? "green" : "red", p_TeamBAcc * 100f, p_TeamBName);

            /// Line break
            l_Text += "\n";
            /// Restore line height
            l_Text += "<line-height=100%>";
            l_Text += "<align=\"center\">" + (p_IsTieBreaker ? "<color=yellow>" : "<color=white>") + (p_MapName.Length > 30 ? p_MapName.Substring(0, 27) + "..." : p_MapName);

            ////////////////////////////////////////////////////////////////////////////

            string l_HoverHint = null;/* "";
            /// Fake line height
            l_HoverHint += "<line-height=1%>";

            /// Left
            l_HoverHint += string.Format("<align=\"left\"><color={0}>{1} (<size=90%><color=#f6d031>{2:#,0.##}%</color></size>)", p_TeamAAcc >= p_TeamBAcc ? "green" : "red", p_TeamAName, p_TeamAAcc * 100f);

            /// Line break
            l_Text += "\n";
            /// Fake line height
            l_Text += "<line-height=1%>";

            /// Right
            l_HoverHint += string.Format("<align=\"right\"><color={0}>(<size=90%><color=#f6d031>{1:#,0.##}%</color></size>) {2}", p_TeamBAcc >= p_TeamAAcc ? "green" : "red", p_TeamBAcc * 100f, p_TeamBName);
            */
            return (l_Text, l_HoverHint);
        }
    }
}
