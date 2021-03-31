using BeatSaberMarkupLanguage.Attributes;
using UnityEngine;
using UnityEngine.UI;

namespace WTB.Views
{
    /// <summary>
    /// Connection error view
    /// </summary>
    internal class ConnectionError : SDK.UI.ViewController<ConnectionError>
    {
#pragma warning disable CS0649
        [UIObject("background")]
        private GameObject m_Background = null;
        [UIComponent("BackButton")]
        private Button m_BackButton;
        [UIComponent("RetryButton")]
        private Button m_RetryButton;
#pragma warning restore CS0649

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get view content description XML
        /// </summary>
        /// <returns></returns>
        protected override string GetViewContentDescription()
        {
            string BSML_RESOURCE_RAW = "<vertical child-control-height='false' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:schemaLocation='https://monkeymanboy.github.io/BSML-Docs/ https://raw.githubusercontent.com/monkeymanboy/BSML-Docs/gh-pages/BSMLSchema.xsd'> <horizontal> <text text=' ' font-size='5.5' align='Center'/> </horizontal> <horizontal id='background' bg='round-rect-panel' pad='8' size-delta-x='110'> <text text='Connection error...' font-size='5.5' align='Center'/> </horizontal> <horizontal> <text text=' ' font-size='5.5' align='Center'/> </horizontal> <horizontal horizontal-fit='PreferredSize' spacing='3'> <button text='Back to game' id='BackButton'></button> <text text=' ' font-size='5.5' align='Center'/> <primary-button text='Retry' id='RetryButton'></primary-button> </horizontal></vertical>";
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
            m_BackButton.onClick.AddListener(Back);
            m_RetryButton.onClick.AddListener(Retry);

            /// Update opacity
            SDK.UI.Backgroundable.SetOpacity(m_Background, 0.5f);
        }
        /// <summary>
        /// On view activation
        /// </summary>
        protected override void OnViewActivation()
        {
            /// Stop network connection
            Network.ServerConnection.Stop();

            /// Display pending message
            if (HasPendingMessage)
                ShowMessageModal();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Back button pressed
        /// </summary>
        internal void Back()
        {
            ViewFlowCoordinator.Instance().Dismiss();
        }
        /// <summary>
        /// Retry button pressed
        /// </summary>
        internal void Retry()
        {
            ViewFlowCoordinator.Instance().SwitchToAuthentification();
        }
    }
}
