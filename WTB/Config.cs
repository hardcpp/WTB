namespace WTB
{
    /// <summary>
    /// Config class helper
    /// </summary>
    internal static class Config
    {
        /// <summary>
        /// Config instance
        /// </summary>
        private static SDK.Config.INIConfig m_Config = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Server URL
        /// </summary>
        internal static string ServerURL {
            get { return m_Config.GetString("WTB", "ServerURL", "https://wtb.omedan.com/api/plugins/wtb/", true);   }
            set {        m_Config.SetString("WTB", "ServerURL", value);                                             }
        }
        /// <summary>
        /// Should dump all network call in log file
        /// </summary>
        internal static bool DebugNetwork {
            get { return m_Config.GetBool("WTB", "DebugNetwork", false, false);         }
            set {        m_Config.SetBool("WTB", "DebugNetwork", value);                }
        }
        /// <summary>
        /// UserToken
        /// </summary>
        internal static string UserToken {
            get { return m_Config.GetString("WTB", "UserToken", "", true);              }
            set {        m_Config.SetString("WTB", "UserToken", value);                 }
        }
        /// <summary>
        /// SubmitScores
        /// </summary>
        internal static bool SubmitScores {
            get { return m_Config.GetBool("WTB", "SubmitScores", true, true);           }
            set {        m_Config.SetBool("WTB", "SubmitScores", value);                }
        }
        /// <summary>
        /// SongPreview
        /// </summary>
        internal static bool SongPreview {
            get { return m_Config.GetBool("WTB", "SongPreview", true, true);            }
            set {        m_Config.SetBool("WTB", "SongPreview", value);                 }
        }
        /// <summary>
        /// SongPreviewVolume
        /// </summary>
        internal static float SongPreviewVolume {
            get { return m_Config.GetFloat("WTB", "SongPreviewVolume", 0.3f, true);     }
            set {        m_Config.SetFloat("WTB", "SongPreviewVolume", value);          }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Init config
        /// </summary>
        internal static void Init() => m_Config = new SDK.Config.INIConfig("WTB");
    }
}