using System;

namespace WTB.SDK.Misc
{
    /// <summary>
    /// Time helper
    /// </summary>
    internal static class Time
    {
        /// <summary>
        /// Unix Epoch
        /// </summary>
        private static readonly DateTime s_UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get UnixTimestamp
        /// </summary>
        /// <returns>Unix timestamp</returns>
        internal static Int64 UnixTimeNow()
        {
            return (Int64)(DateTime.UtcNow - s_UnixEpoch).TotalSeconds;
        }
        /// <summary>
        /// Convert DateTime to UnixTimestamp
        /// </summary>
        /// <param name="p_DateTime">The DateTime to convert</param>
        /// <returns></returns>
        internal static Int64 ToUnixTime(DateTime p_DateTime)
        {
            return (Int64)p_DateTime.ToUniversalTime().Subtract(s_UnixEpoch).TotalSeconds;
        }
        /// <summary>
        /// Convert UnixTimestamp to DateTime
        /// </summary>
        /// <param name="p_TimeStamp"></param>
        /// <returns></returns>
        internal static DateTime FromUnixTime(Int64 p_TimeStamp)
        {
            return s_UnixEpoch.AddSeconds(p_TimeStamp).ToLocalTime();
        }
    }
}
