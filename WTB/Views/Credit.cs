using BeatSaberMarkupLanguage.Attributes;
using System.Diagnostics;
using UnityEngine;

namespace WTB.Views
{
    /// <summary>
    /// Credit view
    /// </summary>
    internal class Credit : SDK.UI.ViewController<Credit>
    {
        private static readonly string s_CreditsStr = "<line-height=125%><u><b>WTB Team :</b></u>"
            + "\n" + "- <color=#008eff><b>HardCPP#1985</b></color> <i>Plugin & API Design & Coordinator IA</i>"
            + "\n" + "- <color=#008eff><b>OMDN | Krixs#1106</b></color> <i>WebSite & API</i>"
            + "\n" + "- <color=#008eff><b>Curze#6815</b></color> <i>WebSite</i>"
            //+ "\n" + "- <color=#008eff><b>Ingenium#0412</b></color> <i>WebSite</i>"
            + "\n" + "- <color=#008eff><b>Luhhky#3944</b></color> <i>Arts & Logo design</i>"
            //+ "\n" + "- <color=#008eff><b>Vred#0001</b></color> <i>Video editing</i>"
            + "\n"

            + "\n" + "<u><b>Special thanks to :</b></u>"
            + "\n" + "- <color=#008eff><b>Moon</b></color> <i>for the original idea</i>"
            + "\n"
            + "\n";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

#pragma warning disable CS0414
        [UIObject("Background")]
        private GameObject m_Background = null;
        [UIComponent("Credits")]
        private HMUI.TextPageScrollView m_Credits = null;
#pragma warning restore CS0414

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get view content description XML
        /// </summary>
        /// <returns></returns>
        protected override string GetViewContentDescription()
        {
            string BSML_RESOURCE_RAW = "<vertical child-control-height='false'> <horizontal bg='panel-top' pad-left='15' pad-right='15' horizontal-fit='PreferredSize'> <text text='Credits' align='Center' font-size='4.2'/> </horizontal> <horizontal bg='round-rect-panel' id='Background' min-width='110' min-height='55' spacing='0' pad='0'> <text-page id='Credits' text=''></text-page> </horizontal> <horizontal> <primary-button text='Website' min-width='54' on-click='click-btn-website'></primary-button> <primary-button text='Discord' min-width='54' on-click='click-btn-discord'></primary-button> </horizontal></vertical>";
            return BSML_RESOURCE_RAW;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override void OnViewCreation()
        {
            SDK.UI.Backgroundable.SetOpacity(m_Background, 0.5f);
            m_Credits.SetText(s_CreditsStr);
            m_Credits.UpdateVerticalScrollIndicator(0);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Go to website
        /// </summary>
        [UIAction("click-btn-website")]
        private void OnWebSitePressed()
        {
            Process.Start(StaticConfig.WebSite);
            ShowMessageModal("URL opened in your desktop browser.");
        }
        /// <summary>
        /// Go to discord
        /// </summary>
        [UIAction("click-btn-discord")]
        private void OnDiscordPressed()
        {
            Process.Start(StaticConfig.Discord);
            ShowMessageModal("URL opened in your desktop browser.");
        }
    }
}
