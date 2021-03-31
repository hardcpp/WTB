using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using HMUI;
using System;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WTB.Views
{
    /// <summary>
    /// Tournament selection screen
    /// </summary>
    internal class TournamentSelect : SDK.UI.ViewController<TournamentSelect>
    {
#pragma warning disable CS0649
        [UIComponent("TournamentList_SettingsButton")]
        private Button m_TournamentList_SettingsButton = null;
        [UIComponent("TournamentList_RefreshButton")]
        private Button m_TournamentList_RefreshButton = null;
        [UIComponent("TournamentList_UpButton")]
        private Button m_TournamentList_UpButton;
        [UIComponent("TournamentList_List")]
        private CustomListTableData m_TournamentList_List = null;
        [UIComponent("TournamentList_DownButton")]
        private Button m_TournamentList_DownButton;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIObject("TournamentInfoBackground")]
        private GameObject m_TournamentInfoBackground = null;
        [UIObject("TournamentInfoPanel")]
        private GameObject m_TournamentInfoPanel;
        [UIComponent("TournamentBanner")]
        private ImageView m_TournamentBanner;
        [UIComponent("TournamentDescription")]
        private TextMeshProUGUI m_TournamentDescription = null;
        [UIComponent("TournamentJoinButton")]
        private Button m_TournamentJoinButton;
        [UIComponent("TournamentMoreInfoButton")]
        private Button m_TournamentMoreInfoButton;
        [UIComponent("TournamentDiscordButton")]
        private Button m_TournamentDiscordButton;
        [UIComponent("TournamentTwitchButton")]
        private Button m_TournamentTwitchButton;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIObject("NoTournamentModal")]
        private GameObject m_NoTournamentModal;
        [UIComponent("NoTournamentModal_RefreshButton")]
        private Button m_NoTournamentModal_RefreshButton = null;
#pragma warning restore CS0649

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Current tournament list page
        /// </summary>
        private int m_CurrentPage = 1;
        /// <summary>
        /// Has more tournaments
        /// </summary>
        private bool m_HasMorePage = false;
        /// <summary>
        /// Current selected tournament informations
        /// </summary>
        private Network.Methods.TournamentInfo_Result m_CurrentTournamentInfo = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get view content description XML
        /// </summary>
        /// <returns></returns>
        protected override string GetViewContentDescription()
        {
            string BSML_RESOURCE_RAW = "<horizontal> <vertical> <horizontal pad='0'> <icon-button id='TournamentList_SettingsButton' icon='WTB.Assets.SettingsIcon.png' stroke-type='Clean' pad='1'/> <text text='Tournament selection' font-size='5.5' align='Center'/> <icon-button id='TournamentList_RefreshButton' icon='WTB.Assets.RefreshIcon.png' stroke-type='Clean' pad='1'/> </horizontal> <page-button id='TournamentList_UpButton' direction='Up'></page-button> <list id='TournamentList_List' expand-cell='true'> </list> <page-button id='TournamentList_DownButton' direction='Down'></page-button> </vertical> <vertical id='TournamentInfoPanel' anchor-min-y='0' anchor-max-y='1' spacing='0'> <vertical id='TournamentInfoBackground' bg='round-rect-panel' min-width='80' min-height='70' child-align='UpperCenter' anchor-pos-y='12'> <image id='TournamentBanner' ignore-layout='false' size-delta-x='80' size-delta-y='23' min-height='23' /> <horizontal pref-width='10' pad-left='2' pad-right='2' pref-height='30' spacing='0'> <text id='TournamentDescription' text='Description' font-size='3' align='Top'/> </horizontal> </vertical> <vertical> <horizontal pad='0'> <primary-button id='TournamentJoinButton' text='Join' min-width='80'></primary-button> </horizontal> <horizontal pad='0'> <button id='TournamentMoreInfoButton' min-width='27' text='More info'></button> <button id='TournamentDiscordButton' min-width='27' text='Discord'></button> <button id='TournamentTwitchButton' min-width='27' text='Twitch'></button> </horizontal> </vertical> </vertical> <modal id='NoTournamentModal' show-event='ShowNoTournamentModal' hide-event='CloseNoTournamentModal,CloseAllModals' move-to-center='true' size-delta-y='30' size-delta-x='48'> <vertical pad='0'> <text text='No tournament found' font-size='5.5' align='Center'/> <primary-button text='Refresh' id='NoTournamentModal_RefreshButton'></primary-button> </vertical> </modal></horizontal>";
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
            m_TournamentList_UpButton.transform.localScale      = Vector3.one * 0.6f;
            m_TournamentList_DownButton.transform.localScale    = Vector3.one * 0.6f;

            /// Scale down button
            m_TournamentList_SettingsButton.transform.localScale = Vector3.one * 0.7f;
            m_TournamentList_RefreshButton.transform.localScale = Vector3.one * 0.7f;

            /// Bind events
            m_TournamentList_SettingsButton.onClick.AddListener(() => ViewFlowCoordinator.Instance().SwitchToSettings());
            m_TournamentList_RefreshButton.onClick.AddListener(OnTournamentListRefreshPressed);
            m_NoTournamentModal_RefreshButton.onClick.AddListener(OnTournamentListRefreshPressed);
            m_TournamentList_UpButton.onClick.AddListener(OnTournamentPageUpPressed);
            m_TournamentList_List.tableView.didSelectCellWithIdxEvent += OnTournamentSelected;
            m_TournamentList_DownButton.onClick.AddListener(OnTournamentPageDownPressed);
            m_TournamentJoinButton.onClick.AddListener(OnTournamentJoinPressed);
            m_TournamentMoreInfoButton.onClick.AddListener(OnTournamentMoreInfoPressed);
            m_TournamentDiscordButton.onClick.AddListener(OnTournamentDiscordPressed);
            m_TournamentTwitchButton.onClick.AddListener(OnTournamentTwitchPressed);

            /// Setup tournament description text
            m_TournamentDescription.overflowMode = TextOverflowModes.Truncate;
            m_TournamentDescription.enableWordWrapping = true;

            /// Change opacity
            SDK.UI.Backgroundable.SetOpacity(m_TournamentInfoBackground, 0.5f);
            SDK.UI.ModalView.SetOpacity(m_NoTournamentModal, 0.75f);
        }
        /// <summary>
        /// On view activation
        /// </summary>
        protected override void OnViewActivation()
        {
            /// Hide tournament info panel
            m_TournamentInfoPanel.SetActive(false);

            /// Go back to page 1
            m_CurrentPage = 1;
            m_HasMorePage = false;

            /// Clear old list
            ClearTournamentList();

            /// Show loading modal
            ShowLoadingModal();

            /// Query tournament list
            Network.ServerConnection.SendQuery(new Network.Methods.TournamentList()
            {
                Page = m_CurrentPage
            });
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Show no tournament modal
        /// </summary>
        private void ShowNoTournamentModal()
        {
            HideLoadingModal();
            HideMessageModal();

            ShowModal("ShowNoTournamentModal");
        }
        /// <summary>
        /// Hide the no tournament modal
        /// </summary>
        private void HideNoTournamentModal() => CloseModal("CloseNoTournamentModal");

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On tournament list refresh button pressed
        /// </summary>
        private void OnTournamentListRefreshPressed()
        {
            /// Go back to page 1
            m_CurrentPage = 1;
            m_HasMorePage = false;

            /// Update UI
            m_TournamentList_UpButton.interactable    = m_CurrentPage != 1;
            m_TournamentList_DownButton.interactable  = m_HasMorePage;

            /// Clear old list
            ClearTournamentList();

            /// Update modals
            HideNoTournamentModal();
            ShowLoadingModal();

            /// Query tournament list
            Network.ServerConnection.SendQuery(new Network.Methods.TournamentList()
            {
                Page = m_CurrentPage
            });
        }
        /// <summary>
        /// On Config method result
        /// </summary>
        /// <param name="p_Result">Result data</param>
        [Network.Methods._MethodResultHandler]
        static private void OnTournamentList(Network.Methods.TournamentList_Result p_Result)
        {
            if (!CanBeUpdated)
                return;

            /// Update list
            Instance.m_CurrentPage = p_Result.Page;
            Instance.m_HasMorePage = p_Result.HasMore;
            Instance.ClearTournamentList();

            /// Add new items
            foreach (var l_Current in p_Result.Tournaments)
                Instance.m_TournamentList_List.data.Add(new TournamentListEntry(l_Current.ID, l_Current.Name, l_Current.State, l_Current.LogoSmall));

            /// Display items
            Instance.m_TournamentList_List.tableView.ReloadData();

            /// Update UI
            Instance.m_TournamentList_UpButton.interactable     = Instance.m_CurrentPage != 1;
            Instance.m_TournamentList_DownButton.interactable   = Instance.m_HasMorePage;

            /// Hide loading modal
            Instance.HideLoadingModal();

            /// If no tournament, show error
            if (p_Result.Tournaments.Count == 0)
                Instance.ShowNoTournamentModal();
        }
        /// <summary>
        /// Go to previous tournament page
        /// </summary>
        private void OnTournamentPageUpPressed()
        {
            /// Underflow check
            if (m_CurrentPage < 2)
                return;

            /// Decrement current page
            m_CurrentPage--;

            /// Show loading
            ShowLoadingModal();

            /// Query tournament list
            Network.ServerConnection.SendQuery(new Network.Methods.TournamentList()
            {
                Page = m_CurrentPage
            });
        }
        /// <summary>
        /// Go to next tournament page
        /// </summary>
        private void OnTournamentPageDownPressed()
        {
            /// Overflow check
            if (!m_HasMorePage)
                return;

            /// Increment current page
            m_CurrentPage++;

            /// Show loading
            ShowLoadingModal();

            /// Query tournament list
            Network.ServerConnection.SendQuery(new Network.Methods.TournamentList()
            {
                Page = m_CurrentPage
            });
        }
        /// <summary>
        /// When a tournament is selected
        /// </summary>
        /// <param name="p_TableView">Table view instance</param>
        /// <param name="p_Row">Row index</param>
        private void OnTournamentSelected(TableView p_TableView, int p_Row)
        {
            /// Fetch tournament entry
            TournamentListEntry l_Tournament = m_TournamentList_List.data[p_Row] as TournamentListEntry;

            /// Show loading
            ShowLoadingModal();

            /// Query tournament informations
            Network.ServerConnection.SendQuery(new Network.Methods.TournamentInfo()
            {
                ID = l_Tournament.ID
            });
        }
        /// <summary>
        /// On tournament info method result
        /// </summary>
        /// <param name="p_Result">Result data</param>
        [Network.Methods._MethodResultHandler]
        static private void OnTournamentInfo(Network.Methods.TournamentInfo_Result p_Result)
        {
            if (!CanBeUpdated)
                return;

            /// If it can join
            if (!p_Result.HasExpired)
            {
                /// Store tournament info
                Instance.m_CurrentTournamentInfo = p_Result;

                /// Update UI
                var l_Description = p_Result.Description;

                if (l_Description.Trim().Length == 0)
                    l_Description = "<align=\"center\"><alpha=#AA><i>No description...</i></align>";

                Instance.m_TournamentDescription.SetText(l_Description);

                /// Build texture
                var l_Banner = new Texture2D(2, 2);
                if (l_Banner.LoadImage(Convert.FromBase64String(p_Result.Banner)))
                {
                    l_Banner = SDK.Unity.Texture2D.ResampleAndCrop(l_Banner, (int)Instance.m_TournamentBanner.rectTransform.sizeDelta.x * 10, (int)Instance.m_TournamentBanner.rectTransform.sizeDelta.y * 10, 0.32f);

                    var l_Sprite = Sprite.Create(l_Banner, new Rect(0, 0, l_Banner.width, l_Banner.height), Vector2.zero, 100f);
                    Instance.m_TournamentBanner.sprite = l_Sprite;
                }

                /// Propagate changes
                Instance.m_TournamentJoinButton.interactable      = Instance.m_CurrentTournamentInfo != null ? Instance.m_CurrentTournamentInfo.CanJoin               : false;
                Instance.m_TournamentMoreInfoButton.interactable  = Instance.m_CurrentTournamentInfo != null ? Instance.m_CurrentTournamentInfo.MoreInfoLink != ""    : false;
                Instance.m_TournamentDiscordButton.interactable   = Instance.m_CurrentTournamentInfo != null ? Instance.m_CurrentTournamentInfo.DiscordLink != ""     : false;
                Instance.m_TournamentTwitchButton.interactable    = Instance.m_CurrentTournamentInfo != null ? Instance.m_CurrentTournamentInfo.TwitchLink != ""      : false;

                /// Show tournament info panel
                Instance.m_TournamentInfoPanel.SetActive(true);

                /// Hide loading modal
                Instance.HideLoadingModal();
            }
            /// If expired
            else if (p_Result.HasExpired)
            {
                /// Hide tournament info panel
                Instance.m_TournamentInfoPanel.SetActive(false);
                /// Show error message
                Instance.SetMessageModal_PendingMessage("This tournament has expired !");
                /// Refresh tournament list
                Instance.OnTournamentListRefreshPressed();
            }
        }
        /// <summary>
        /// Go to tournament
        /// </summary>
        private void OnTournamentJoinPressed()
        {
            /// Show loading modal
            ShowLoadingModal();

            /// Query join
            Network.ServerConnection.SendQuery(new Network.Methods.TournamentJoin()
            {
                ID = m_CurrentTournamentInfo.ID
            });
        }
        /// <summary>
        /// On tournament join method result
        /// </summary>
        /// <param name="p_Result">Result data</param>
        [Network.Methods._MethodResultHandler]
        static private void OnTournamentJoin(Network.Methods.TournamentJoin_Result p_Result)
        {
            if (!CanBeUpdated)
                return;

            /// If we can join
            if (p_Result.CanJoin)
            {
                /// Should join qualifiers room ?
                if (p_Result.JoinType == "Qualifiers")
                {
                    /// Update selected tournament for qualifiers
                    ViewFlowCoordinator.Instance().qualifiers.SetSelectedTournament(Instance.m_CurrentTournamentInfo.ID, Instance.m_CurrentTournamentInfo.Name, SDK.Misc.Time.FromUnixTime(p_Result.EndTime));
                    /// Change active view
                    ViewFlowCoordinator.Instance().SwitchToQualifiers();

                    return;
                }
                /// Should join match room ?
                else if (p_Result.JoinType == "Match")
                {
                    /// Update selected tournament for match
                    ViewFlowCoordinator.Instance().match.SetSelectedTournament(Instance.m_CurrentTournamentInfo.ID, Instance.m_CurrentTournamentInfo.Name);
                    /// Change active view
                    ViewFlowCoordinator.Instance().SwitchToMatch();

                    return;
                }
            }
            /// If we can't join, refresh list and display and error
            else
            {
                /// Hide tournament info panel
                Instance.m_TournamentInfoPanel.SetActive(false);
                /// Show error message
                Instance.SetMessageModal_PendingMessage("This tournament has expired !");
                /// Refresh tournament list
                Instance.OnTournamentListRefreshPressed();
            }
        }
        /// <summary>
        /// Go to tournament info page
        /// </summary>
        private void OnTournamentMoreInfoPressed()
        {
            if (m_CurrentTournamentInfo.MoreInfoLink == "")
                return;

            Process.Start(m_CurrentTournamentInfo.MoreInfoLink);

            ShowMessageModal("URL opened in your desktop browser.");
        }
        /// <summary>
        /// Go to tournament discord page
        /// </summary>
        private void OnTournamentDiscordPressed()
        {
            if (m_CurrentTournamentInfo.DiscordLink == "")
                return;

            Process.Start(m_CurrentTournamentInfo.DiscordLink);

            ShowMessageModal("URL opened in your desktop browser.");
        }
        /// <summary>
        /// Go to tournament twitch page
        /// </summary>
        private void OnTournamentTwitchPressed()
        {
            if (m_CurrentTournamentInfo.TwitchLink == "")
                return;

            Process.Start(m_CurrentTournamentInfo.TwitchLink);

            ShowMessageModal("URL opened in your desktop browser.");
        }
        /// <summary>
        /// Clear tournament list
        /// </summary>
        private void ClearTournamentList()
        {
            /// Clear selection and items, then refresh the list
            m_TournamentList_List.tableView.ClearSelection();
            m_TournamentList_List.data.Clear();
            m_TournamentList_List.tableView.ReloadData();

            /// Update UI
            m_TournamentList_UpButton.interactable      = m_CurrentPage != 1;
            m_TournamentList_DownButton.interactable    = m_HasMorePage;
            m_TournamentJoinButton.interactable         = false;
            m_TournamentMoreInfoButton.interactable     = false;
            m_TournamentDiscordButton.interactable      = false;
            m_TournamentTwitchButton.interactable       = false;
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Tournament list entry
    /// </summary>
    internal class TournamentListEntry : CustomListTableData.CustomCellInfo
    {
        /// <summary>
        /// Tournament ID
        /// </summary>
        internal readonly int ID;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_ID">Tournament ID</param>
        /// <param name="p_Text">Title</param>
        /// <param name="p_SubText">Caption</param>
        /// <param name="p_LogoSmall">Base64 logo</param>
        internal TournamentListEntry(int p_ID, string p_Text, string p_SubText, string p_LogoSmall)
            : base(p_Text, p_SubText, null)
        {
            ID = p_ID;

            var l_Texture = new Texture2D(2, 2);

            if (l_Texture.LoadImage(Convert.FromBase64String(p_LogoSmall)))
                this.icon = Sprite.Create(l_Texture, new Rect(0, 0, l_Texture.width, l_Texture.height), new Vector2(0.5f, 0.5f), 100);
            else
                this.icon = null;
        }
    }
}
