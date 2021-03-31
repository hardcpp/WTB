using HMUI;
using System.Linq;
using UnityEngine;

namespace WTB.Views
{
    /// <summary>
    /// UI flow coordinator
    /// </summary>
    internal class ViewFlowCoordinator : SDK.UI.ViewFlowCoordinator<ViewFlowCoordinator>
    {
        internal ConnectionError    connectionError  = null;
        internal Authentification   authentification = null;
        internal Settings           settings         = null;
        internal TournamentSelect   tournamentSelect = null;
        internal Credit             credit           = null;
        internal ChangeLog          changeLog        = null;
        internal Qualifiers         qualifiers       = null;
        internal Match              match            = null;
        internal Match_PickBanList  matchPickBanList = null;
        internal Match_Playlist     matchPlaylist    = null;
        internal ScoreBoard         scoreBoard       = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Title
        /// </summary>
        internal override string Title => "Walk those brackets";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When the Unity GameObject and all children are ready
        /// </summary>
        internal ViewFlowCoordinator()
        {
            if (connectionError == null)    connectionError     = CreateViewController<ConnectionError>();
            if (authentification == null)   authentification    = CreateViewController<Authentification>();
            if (settings == null)           settings            = CreateViewController<Settings>();
            if (tournamentSelect == null)   tournamentSelect    = CreateViewController<TournamentSelect>();
            if (credit == null)             credit              = CreateViewController<Credit>();
            if (changeLog == null)          changeLog           = CreateViewController<ChangeLog>();
            if (qualifiers == null)         qualifiers          = CreateViewController<Qualifiers>();
            if (match == null)              match               = CreateViewController<Match>();
            if (matchPickBanList == null)   matchPickBanList    = CreateViewController<Match_PickBanList>();
            if (matchPlaylist == null)      matchPlaylist       = CreateViewController<Match_Playlist>();
            if (scoreBoard == null)         scoreBoard          = CreateViewController<ScoreBoard>();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get initial views controller
        /// </summary>
        /// <returns>(Middle, Left, Right)</returns>
        protected override sealed (ViewController, ViewController, ViewController) GetInitialViewsController() => (authentification, null, null);
        /// <summary>
        /// On back button pressed
        /// </summary>
        /// <param name="p_TopViewController">Current top view controller</param>
        /// <returns>True if the event is catched, false if we should dismiss the flow coordinator</returns>
        protected override sealed bool OnBackButtonPressed(ViewController p_TopViewController)
        {
            /// If we are in qualifiers, we switch back to tournament list
            if (p_TopViewController == settings || p_TopViewController == qualifiers || p_TopViewController == match)
            {
                SwitchToTournamentSelect();
                return true;
            }

            /// Stop connection
            Network.ServerConnection.Stop();

            return false;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Switch to authentification view
        /// </summary>
        internal void SwitchToAuthentification() => ChangeView(authentification);
        /// <summary>
        /// Switch to connection error view
        /// </summary>
        internal void SwitchToConnectionError() => ChangeView(connectionError);
        /// <summary>
        /// Switch to settings view
        /// </summary>
        internal void SwitchToSettings() => ChangeView(settings);
        /// <summary>
        /// Switch to tournament select view
        /// </summary>
        internal void SwitchToTournamentSelect() => ChangeView(tournamentSelect, credit, changeLog);
        /// <summary>
        /// Switch to qualifiers view
        /// </summary>
        internal void SwitchToQualifiers()
        {
            var l_GamePlaySetupViewController = Resources.FindObjectsOfTypeAll<GameplaySetupViewController>().First();

            if (l_GamePlaySetupViewController)
                l_GamePlaySetupViewController.Setup(false, true, true, GameplaySetupViewController.GameplayMode.SinglePlayer);

            ChangeView(qualifiers, l_GamePlaySetupViewController);
        }
        /// <summary>
        /// Switch to match view
        /// </summary>
        internal void SwitchToMatch() => ChangeView(match);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Show the score board
        /// </summary>
        internal void ShowGamePlaySetup()
        {
            var l_GamePlaySetupViewController = Resources.FindObjectsOfTypeAll<GameplaySetupViewController>().First();

            if (l_GamePlaySetupViewController)
                l_GamePlaySetupViewController.Setup(false, true, true, GameplaySetupViewController.GameplayMode.SinglePlayer);

            SetLeftScreenViewController(l_GamePlaySetupViewController, ViewController.AnimationType.None);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Show the score board
        /// </summary>
        internal void ShowScoreBoard() => SetRightScreenViewController(scoreBoard, ViewController.AnimationType.None);
        /// <summary>
        /// Show match pick ban list
        /// </summary>
        internal void ShowMatchPickBanList() => SetRightScreenViewController(matchPickBanList, ViewController.AnimationType.None);
        /// <summary>
        /// Show match playlist
        /// </summary>
        internal void ShowMatchPlaylist() => SetRightScreenViewController(matchPlaylist, ViewController.AnimationType.None);
    }
}
