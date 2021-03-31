using System.Net;
using System.Net.Http;

namespace WTB.SDK.Network
{
    /// <summary>
    /// API Response class
    /// </summary>
    internal class APIResponse
    {
        /// <summary>
        /// Result code
        /// </summary>
        internal readonly HttpStatusCode StatusCode;
        /// <summary>
        /// Reason phrase
        /// </summary>
        internal readonly string ReasonPhrase;
        /// <summary>
        /// Is success
        /// </summary>
        internal readonly bool IsSuccessStatusCode;
        /// <summary>
        /// Response bytes
        /// </summary>
        internal readonly byte[] BodyBytes;
        /// <summary>
        /// Response string
        /// </summary>
        internal readonly string BodyString;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Reply">Reply status</param>
        /// <param name="p_Body">Reply body</param>
        internal APIResponse(HttpResponseMessage p_Reply, byte[] p_BodyBytes, string p_BodyString)
        {
            StatusCode          = p_Reply.StatusCode;
            ReasonPhrase        = p_Reply.ReasonPhrase;
            IsSuccessStatusCode = p_Reply.IsSuccessStatusCode;
            BodyBytes           = p_BodyBytes;
            BodyString          = p_BodyString;

            ///foreach (var l_Header in p_Reply.RequestMessage.Headers)
            ///{
            ///    Logger.log.Debug(l_Header.Key);
            ///    foreach (var l_Value in l_Header.Value)
            ///        Logger.log.Debug("    " + l_Value);
            ///}

            p_Reply.Dispose();
        }
    }
}
