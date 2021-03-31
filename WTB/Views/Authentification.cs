using BeatSaberMarkupLanguage.Attributes;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WTB.Views
{
    /// <summary>
    /// Authentification view
    /// </summary>
    public class Authentification : SDK.UI.ViewController<Authentification>
    {
#pragma warning disable CS0414
        [UIObject("background")]
        private GameObject m_Background = null;
        [UIComponent("MessageText")]
        private TextMeshProUGUI m_MessageText = null;
        [UIComponent("OpenWebSite")]
        private Button m_OpenWebSiteButton = null;
#pragma warning restore CS0414

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Server tick rate interval
        /// </summary>
        private float m_ServerTickRate;
        /// <summary>
        /// Should request Auth again
        /// </summary>
        private bool m_RequestAuth = false;
        /// <summary>
        /// Request Auth timeout
        /// </summary>
        private float m_RequestAuthTimeout = 0f;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get view content description XML
        /// </summary>
        /// <returns></returns>
        protected override string GetViewContentDescription()
        {
            string BSML_RESOURCE_RAW = "<vertical child-control-height='false' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:schemaLocation='https://monkeymanboy.github.io/BSML-Docs/ https://raw.githubusercontent.com/monkeymanboy/BSML-Docs/gh-pages/BSMLSchema.xsd'> <horizontal> <text text=' ' font-size='5.5' align='Center'/> </horizontal> <horizontal id='background' bg='round-rect-panel' pad='8' size-delta-x='110'> <text id='MessageText' text='Checking plugin version...' font-size='5.5' align='Center'/> </horizontal> <horizontal> <primary-button id='OpenWebSite' text='Open website' /> </horizontal></vertical>";
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

            /// Bind events
            m_OpenWebSiteButton.onClick.AddListener(OnOpenWebSitePressed);
        }
        /// <summary>
        /// On view activation
        /// </summary>
        protected override void OnViewActivation()
        {
            /// Start connection
            Network.ServerConnection.Start();

            /// Update message
            m_MessageText.text = "Checking plugin version...";

            /// Update button
            m_OpenWebSiteButton.interactable = false;

            /// Send our plugin version & game version
            Network.ServerConnection.SendQuery(new Network.Methods.Config()
            {
                Version     = Assembly.GetExecutingAssembly().GetName().Version.ToString(3),
                GameVersion = IPA.Utilities.UnityGame.GameVersion.SemverValue.ToString()
            });
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Called at each frame
        /// </summary>
        public void Update()
        {
            /// Should re-query auth
            if (m_RequestAuth)
            {
                m_RequestAuthTimeout -= UnityEngine.Time.deltaTime;

                /// Request if timer expired
                if (m_RequestAuthTimeout < 0)
                {
                    /// Send Authenticate query
                    Network.ServerConnection.SendQuery(new Network.Methods.Authenticate()
                    {
                        Token           = Config.UserToken,
                        ScoreSaberID    = SDK.Game.UserPlatform.GetUserID(),
                    });

                    /// Wait next answer
                    m_RequestAuth = false;
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On open web site pressed
        /// </summary>
        private void OnOpenWebSitePressed()
        {
            /// Update button
            m_OpenWebSiteButton.interactable = false;

            /// Update message
            if (m_RequestAuth)
                m_MessageText.text = "Website opened in your default browser\nWaiting for authorization...";

            /// Open browser
            Process.Start(StaticConfig.WebSite + "?ssid=" + SDK.Game.UserPlatform.GetUserID());
        }
        /// <summary>
        /// On Config method result
        /// </summary>
        /// <param name="p_Result">Result data</param>
        [Network.Methods._MethodResultHandler]
        static private void OnConfigResult(Network.Methods.Config_Result p_Result)
        {
            if (!CanBeUpdated)
                return;

#if DEBUG
            Logger.log?.Debug("OnConfigResult IsUpdated : "         + p_Result.IsUpdated);
            Logger.log?.Debug("OnConfigResult LastVersion : "       + p_Result.LastVersion);
            Logger.log?.Debug("OnConfigResult NetworkTickRate : " + p_Result.NetworkTickRate);
#endif

            if (p_Result.IsUpdated)
            {
                /// Update message
                Instance.m_MessageText.text = "Version OK, connecting to the server...";

                /// Store server tick rate
                Instance.m_ServerTickRate = p_Result.NetworkTickRate;

                /// Send Authenticate query
                Network.ServerConnection.SendQuery(new Network.Methods.Authenticate()
                {
                    Token           = Config.UserToken,
                    ScoreSaberID    = SDK.Game.UserPlatform.GetUserID(),
                });
            }
            else
                /// Update message
                Instance.m_MessageText.text = "Please update WTB plugin !";
        }
        /// <summary>
        /// On Authenticate method result
        /// </summary>
        /// <param name="p_Result">Result data</param>
        [Network.Methods._MethodResultHandler]
        static private void OnAuthenticateResult(Network.Methods.Authenticate_Result p_Result)
        {
            if (!CanBeUpdated)
                return;

#if DEBUG
            Logger.log?.Debug("OnAuthenticateResult IsValid : " + p_Result.IsValid);
            Logger.log?.Debug("OnAuthenticateResult NewToken : " + p_Result.NewToken);
#endif

            if (p_Result.IsValid)
            {
                /// Update message
                Instance.m_MessageText.text = "Token ok !";

                /// Set configuration for network
                Network.ServerConnection.SetConfiguration(Config.UserToken, Instance.m_ServerTickRate);

                /// Go to tournament selection
                ViewFlowCoordinator.Instance().SwitchToTournamentSelect();
            }
            else if (p_Result.IsDeclined)
            {
                /// Update message
                Instance.m_MessageText.text = "Auth declined !";
            }
            else
            {
                /// Store new token, if the API sent a new one
                if (p_Result.NewToken != "")
                {
                    Config.UserToken = p_Result.NewToken;

                    /// Send Authenticate query
                    Network.ServerConnection.SendQuery(new Network.Methods.Authenticate()
                    {
                        Token           = Config.UserToken,
                        ScoreSaberID    = SDK.Game.UserPlatform.GetUserID(),
                    });
                }
                else
                {
                    /// Update message
                    if (!Instance.m_OpenWebSiteButton.interactable)
                    {
                        /// Update button
                        Instance.m_OpenWebSiteButton.interactable = true;
                        Instance.m_MessageText.text = "Please click on the button below to authorize your game...";
                    }
                    else
                        Instance.m_MessageText.text = "Website opened in your default browser\nWaiting for authorization...";

                    /// Setup re-query
                    Instance.m_RequestAuthTimeout = 2.5f;
                    Instance.m_RequestAuth        = true;
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Make a sha1 hash
        /// </summary>
        /// <param name="p_Input">Input string</param>
        /// <returns></returns>
        static string Hash(string p_Input)
        {
            using (SHA1Managed l_SHA1 = new SHA1Managed())
            {
                var l_Hash      = l_SHA1.ComputeHash(Encoding.UTF8.GetBytes(p_Input));
                var l_Builder   = new StringBuilder(l_Hash.Length * 2);

                foreach (byte l_Byte in l_Hash)
                    l_Builder.Append(l_Byte.ToString("x2"));

                return l_Builder.ToString();
            }
        }
        /// <summary>
        /// Reverse a string
        /// </summary>
        /// <param name="p_String">String to reverse</param>
        /// <returns></returns>
        static string Reverse(string p_String)
        {
            char[] l_Chars = p_String.ToCharArray();
            Array.Reverse(l_Chars);
            return new string(l_Chars);
        }
    }
}
