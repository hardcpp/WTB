using BeatSaberMarkupLanguage.Attributes;
using UnityEngine;

namespace WTB.Views
{
    /// <summary>
    /// ChangeLog view
    /// </summary>
    internal class ChangeLog : SDK.UI.ViewController<ChangeLog>
    {
#pragma warning disable CS0414
        [UIObject("Background")]
        private GameObject m_Background = null;
        [UIComponent("ChangeLog")]
        private HMUI.TextPageScrollView m_ChangeLog = null;
#pragma warning restore CS0414

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get view content description XML
        /// </summary>
        /// <returns></returns>
        protected override string GetViewContentDescription()
        {
            string BSML_RESOURCE_RAW = "<vertical child-control-height='false'> <horizontal bg='panel-top' pad-left='15' pad-right='15' horizontal-fit='PreferredSize'> <text text='ChangeLog' align='Center' font-size='4.2'/> </horizontal> <horizontal bg='round-rect-panel' id='Background' min-width='110' min-height='65' spacing='0' pad='0'> <text-page id='ChangeLog' text=''></text-page> </horizontal></vertical>";
            return BSML_RESOURCE_RAW;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override void OnViewCreation()
        {
            /// Change opacity
            SDK.UI.Backgroundable.SetOpacity(m_Background, 0.5f);
        }
        /// <summary>
        /// On view activation
        /// </summary>
        protected override void OnViewActivation()
        {
            /// Show loading animation
            ShowLoadingModal();

            /// Query change log
            Network.ServerConnection.SendQuery(new Network.Methods.ChangeLog()
            {

            });
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On ChangeLog method result
        /// </summary>
        /// <param name="p_Result">Result data</param>
        [Network.Methods._MethodResultHandler]
        static private void OnChangeLog(Network.Methods.ChangeLog_Result p_Result)
        {
            if (!CanBeUpdated)
                return;

            /// Store the change log
            Instance.m_ChangeLog.SetText("<line-height=125%>" + p_Result.Content);

            /// Hide loading
            Instance.HideLoadingModal();
        }
    }
}
