namespace WTB
{
    /// <summary>
    /// StaticConfig class helper
    /// </summary>
    internal static class StaticConfig
    {
#if DEBUG
        /// <summary>
        /// Website URL
        /// </summary>
        internal static readonly string WebSite = "https://127.0.0.1:8000/";
#else
        /// <summary>
        /// Website URL
        /// </summary>
        internal static readonly string WebSite = "https://wtb.omedan.com/";
#endif
        /// <summary>
        /// Beat saver endpoint
        /// </summary>
        internal static readonly string BeatSaverEndPoint = WebSite + "api/";
        /// <summary>
        /// Beat saver endpoint
        /// </summary>
        internal static readonly string BeatSaverBaseURL = WebSite.TrimEnd('/');
        /// <summary>
        /// Discord invitation link
        /// </summary>
        internal static readonly string Discord = "https://discord.gg/d759hd3";
    }
}
