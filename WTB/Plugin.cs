using BeatSaberMarkupLanguage.MenuButtons;
using IPA;

namespace WTB
{
    /// <summary>
    /// Main plugin class
    /// </summary>
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        /// <summary>
        /// Plugin instance
        /// </summary>
        internal static Plugin Instance { get; private set; }
        /// <summary>
        /// Plugin version
        /// </summary>
        internal static SemVer.Version Version => IPA.Loader.PluginManager.GetPluginFromId("WTB").Version;
        /// <summary>
        /// Plugin name
        /// </summary>
        public static string Name => "WTB";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Called when the plugin is first loaded by IPA (either when the game starts or when the plugin is enabled if it starts disabled).
        /// </summary>
        /// <param name="p_Logger">Logger instance</param>
        [Init]
        public void Init(IPA.Logging.Logger p_Logger)
        {
            ///ServicePointManager.DefaultConnectionLimit = int.MaxValue;
#if DEBUG
            /// Allow untrusted SSL for debug
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (message, cert, chain, sslPolicyErrors) => true;
#endif

            /// Set instance
            Instance = this;

            /// Setup logger
            Logger.log = p_Logger;

            /// Init config
            Config.Init();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [OnEnable]
        public void OnEnable()
        {
            /// Init method handlers
            if (Network.Methods._MethodResultHandlerAttribute.s_Handlers.Count == 0)
                Network.Methods._MethodResultHandlerAttribute.InitHandlers();

            SDK.Game.Logic.Init();

            /// Register mod button
            MenuButtons.instance.RegisterButton(new MenuButton("WTB", "Walk those brackets !", OnModButtonPressed, true));
        }
        [OnDisable]
        public void OnDisable()
        {
            /// Empty
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When the mod button is pressed
        /// </summary>
        private void OnModButtonPressed()
        {
            Views.ViewFlowCoordinator.Instance().Present(true);
        }
    }
}
