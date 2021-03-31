using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WTB.Views
{
    /// <summary>
    /// Match pick ban list
    /// </summary>
    internal class Match_PickBanList : SDK.UI.ViewController<Match_PickBanList>
    {
        /// <summary>
        /// Total pick ban line per page
        /// </summary>
        private static int PICKBAN_PER_PAGE = 10;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

#pragma warning disable CS0649
        [UIObject("background")]
        private GameObject m_Background = null;
        [UIComponent("PickBanUpButton")]
        private Button m_PickBanUpButton;
        [UIObject("PickBanList")]
        private GameObject m_PickBanListView = null;
        private SDK.UI.DataSource.SimpleTextList m_PickBanList = null;
        [UIComponent("PickBanDownButton")]
        private Button m_PickBanDownButton;
#pragma warning restore CS0649

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

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
        private List<(bool, string, string)> m_Data = new List<(bool, string, string)>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get view content description XML
        /// </summary>
        /// <returns></returns>
        protected override string GetViewContentDescription()
        {
            string BSML_RESOURCE_RAW = "<vertical child-control-height='false' spacing='0' pad='0'> <horizontal bg='panel-top' pad-left='15' pad-right='15' horizontal-fit='PreferredSize'> <text text='Pick &amp; Bans' align='Center' font-size='4.2'/> </horizontal> <horizontal spacing='0' pad='0'> <vertical min-width='110' spacing='0' pad='0'> <page-button id='PickBanUpButton' direction='Up'></page-button> <vertical id='background' bg='round-rect-panel' min-width='110' pref-height='55' spacing='0' pad='2' pad-top='1'> <list id='PickBanList'> </list> </vertical> <page-button id='PickBanDownButton' direction='Down'></page-button> </vertical> </horizontal></vertical>";
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
            m_PickBanUpButton.transform.localScale = Vector3.one * 0.5f;
            m_PickBanDownButton.transform.localScale = Vector3.one * 0.5f;

            /// Prepare pick ban list
            var l_BSMLTableView = m_PickBanListView.GetComponentInChildren<BSMLTableView>();
            l_BSMLTableView.SetDataSource(null, false);
            GameObject.DestroyImmediate(m_PickBanListView.GetComponentInChildren<CustomListTableData>());
            m_PickBanList = l_BSMLTableView.gameObject.AddComponent<SDK.UI.DataSource.SimpleTextList>();
            m_PickBanList.TableViewInstance = l_BSMLTableView;
            l_BSMLTableView.SetDataSource(m_PickBanList, false);

            /// Bind events
            m_PickBanUpButton.onClick.AddListener(OnScorePageUpPressed);
            m_PickBanDownButton.onClick.AddListener(OnScorePageDownPressed);
        }
        /// <summary>
        /// On view activation
        /// </summary>
        protected override void OnViewActivation()
        {
            /// Rebuild list
            SetData(m_Data);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Load pick ban
        /// </summary>
        /// <param name="p_Data">New data set</param>
        internal void SetData(List<(bool, string, string)> p_Data)
        {
            /// Clear previous scores
            ClearDisplayedData();

            /// Store data
            m_Data = p_Data;

            /// Reset page
            m_CurrentPage = (m_CurrentPage * PICKBAN_PER_PAGE) >= p_Data.Count ? 1 : m_CurrentPage;
            m_HasMorePage = p_Data.Count > PICKBAN_PER_PAGE;

            /// Update UI
            if (UICreated)
            {
                m_PickBanUpButton.interactable   = m_CurrentPage != 1;
                m_PickBanDownButton.interactable = m_HasMorePage;
            }

            /// Rebuild list
            RebuildList();
        }
        /// <summary>
        /// Go to previous pick ban page
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
        /// Go to next pick ban page
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

            for (int l_I = (m_CurrentPage - 1) * PICKBAN_PER_PAGE; l_I < m_Data.Count && l_I < (m_CurrentPage * PICKBAN_PER_PAGE); ++l_I)
                m_PickBanList.Data.Add(BuildLineString(m_Data[l_I].Item1, m_Data[l_I].Item2, m_Data[l_I].Item3));

            /// Refresh
            m_PickBanList.TableViewInstance.ReloadData();
        }
        /// <summary>
        /// Clear the pick ban list
        /// </summary>
        private void ClearDisplayedData()
        {
            if (!UICreated)
                return;

            m_PickBanList.TableViewInstance.ClearSelection();
            m_PickBanList.Data.Clear();
            m_PickBanList.TableViewInstance.ReloadData();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Build pick ban line
        /// </summary>
        /// <param name="p_IsBan">Is a ban</param>
        /// <param name="p_PlayerName">Player name</param>
        /// <param name="p_MapName">Name of the map</param>
        /// <returns>Built pick ban line</returns>
        private (string, string) BuildLineString(bool p_IsBan, string p_PlayerName, string p_MapName)
        {
            /// Result line
            string l_Line = "<align=\"left\">" + (p_IsBan ? "<color=red>Ban</color>" : "<color=green>Pick</color>");

            l_Line += "<pos=8%> | ";
            l_Line += "<u>" + p_PlayerName + "</u>";
            l_Line += "<pos=40%> | ";
            l_Line += "<color=#c4c4c4>" + p_MapName;

            return (l_Line, null);
        }
    }
}
