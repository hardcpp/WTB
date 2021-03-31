using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.Parser;

namespace WTB.Views
{
    /// <summary>
    /// Settings view
    /// </summary>
    public class Settings : SDK.UI.ViewController<Settings>
    {
#pragma warning disable CS0649
        [UIComponent("SubmitScoresToggle")]
        private ToggleSetting m_SubmitScoresToggle;
        [UIComponent("PlayMapPreviewToggle")]
        private ToggleSetting m_PlayMapPreviewToggle;
        [UIComponent("PreviewVolumeIncrement")]
        private IncrementSetting m_PreviewVolumeIncrement;
#pragma warning restore CS0649

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get view content description XML
        /// </summary>
        /// <returns></returns>
        protected override string GetViewContentDescription()
        {
            string BSML_RESOURCE_RAW = "<vertical child-control-height='false' spacing='0' pad='0'> <horizontal bg='panel-top' pad-left='15' pad-right='15' horizontal-fit='PreferredSize'> <text text='Settings' align='Center' font-size='4.2'/> </horizontal> <horizontal> <text text='Submit scores on ScoreSaber' align='Center'/> </horizontal> <horizontal> <bool-setting id='SubmitScoresToggle'></bool-setting> </horizontal> <horizontal> <text text='Play map preview audio' align='Center'/> </horizontal> <horizontal> <bool-setting id='PlayMapPreviewToggle'></bool-setting> </horizontal> <horizontal> <text text='Preview volume' align='Center'/> </horizontal> <horizontal> <increment-setting id='PreviewVolumeIncrement' min='0' max='1' increment='0.05'/> </horizontal> <horizontal min-height='40'> </horizontal></vertical>";
            return BSML_RESOURCE_RAW;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            var l_Event     = new BSMLAction(this, this.GetType().GetMethod(nameof(Settings.OnSettingChanged),  System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public));
            var l_Formatter = new BSMLAction(this, this.GetType().GetMethod(nameof(Settings.FNPercentage),      System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public));

            SDK.UI.ToggleSetting.Setup(m_SubmitScoresToggle,        l_Event,                Config.SubmitScores,        true);
            SDK.UI.ToggleSetting.Setup(m_PlayMapPreviewToggle,      l_Event,                Config.SongPreview,         true);
            SDK.UI.IncrementSetting.Setup(m_PreviewVolumeIncrement, l_Event, l_Formatter,   Config.SongPreviewVolume,   true);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On setting changed
        /// </summary>
        /// <param name="p_Value">New value</param>
        public void OnSettingChanged(object p_Value)
        {
            /// Update config
            Config.SubmitScores         = m_SubmitScoresToggle.Value;
            Config.SongPreview          = m_PlayMapPreviewToggle.Value;
            Config.SongPreviewVolume    = m_PreviewVolumeIncrement.Value;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On percentage setting changes
        /// </summary>
        /// <param name="p_Value">New value</param>
        /// <returns></returns>
        public string FNPercentage(float p_Value)
        {
            return System.Math.Round(p_Value * 100f, 2) + " %";
        }
    }
}
