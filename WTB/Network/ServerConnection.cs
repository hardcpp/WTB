using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WTB.SDK.Network;

namespace WTB.Network
{
    /// <summary>
    /// Server connection class
    /// </summary>
    internal class ServerConnection
    {
        /// <summary>
        /// API Client
        /// </summary>
        private static APIClient m_APIClient = null;
        /// <summary>
        /// Is the plugin started
        /// </summary>
        private static bool m_Started = false;
        /// <summary>
        /// User token for authed queries
        /// </summary>
        private static string m_UserToken = "";
        /// <summary>
        /// Update interval
        /// </summary>
        private static float m_NetworkTickRate = -1;
        /// <summary>
        /// Request cancel token
        /// </summary>
        private static CancellationTokenSource m_CancelTokenSource;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// RPC query type
        /// </summary>
        struct RPCQuery
        {
            [Newtonsoft.Json.JsonProperty("jsonrpc")]
            internal string jsonrpc;
            [Newtonsoft.Json.JsonProperty("id")]
            internal int id;
            [Newtonsoft.Json.JsonProperty("method")]
            internal string method;
            [Newtonsoft.Json.JsonProperty("params")]
            internal JObject parameters;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Start the connection
        /// </summary>
        internal static void Start()
        {
            if (m_Started)
                return;

            Logger.log?.Debug($"[Network][ServerConnection.Start] Starting server connection");

            /// Create API client if needed
            if (m_APIClient == null)
                m_APIClient = new APIClient(Config.ServerURL, TimeSpan.FromSeconds(10), true);

            m_CancelTokenSource = new CancellationTokenSource();

            m_Started = true;
        }
        /// <summary>
        /// Stop the connection
        /// </summary>
        internal static void Stop()
        {
            if (!m_Started)
                return;

            Logger.log?.Debug($"[Network][ServerConnection.Stop] Closing server connection");

            if (m_CancelTokenSource != null)
            {
                m_CancelTokenSource.Cancel();
                m_CancelTokenSource = null;
            }

            m_UserToken         = "";
            m_NetworkTickRate   = -1;
            m_Started           = false;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set configuration
        /// </summary>
        /// <param name="p_UserToken">User token for authed queries</param>
        /// <param name="p_NetworkTickRate">Network tick rate for message queue pulling</param>
        internal static void SetConfiguration(string p_UserToken, float p_NetworkTickRate)
        {
            m_UserToken         = p_UserToken;
            m_NetworkTickRate   = p_NetworkTickRate;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Send query to the server
        /// </summary>
        /// <typeparam name="T">Query type</typeparam>
        /// <param name="p_Query">Query data</param>
        internal static void SendQuery<T>(T p_Query) where T : Methods._Method
        {
            try
            {
                /// Build the query
                string l_RPCQueryStr = BuildQuery(p_Query);

                if (l_RPCQueryStr == null)
                    return;

#if DEBUG
                System.Diagnostics.Stopwatch l_Timer = null;
                if (Config.DebugNetwork)
                {
                    l_Timer = new System.Diagnostics.Stopwatch();
                    l_Timer.Start();
                    Logger.log?.Debug("[Network][ServerConnection.SendQuery] Sent query :");
                    Logger.log?.Debug(l_RPCQueryStr);
                }
#endif

                /// Send query
                var l_Query = m_APIClient.PostAsync(new StringContent(l_RPCQueryStr, Encoding.UTF8, "application/json"), m_CancelTokenSource.Token, !p_Query._MethodShouldRetryOnFailure);
                l_Query.ContinueWith(p_Task =>
                {
                    if (p_Task.IsCanceled
                     || p_Task.Status == TaskStatus.Canceled
                     || (!p_Query._MethodShouldRetryOnFailure && p_Task.Result == null))
                        return;

                    if (p_Task.Result == null)
                    {
                        HandleError("Connection error\n" + "Unable to reach WTB server!");
                        return;
                    }

                    try
                    {
                        if (p_Task.IsCompleted)
                        {
                            /// If code is different than 200, log it
                            if (p_Task.Result.StatusCode != HttpStatusCode.OK)
                            {
                                Logger.log?.Error($"[Network][ServerConnection.SendQuery] Query \"{p_Query._MethodName}\" invalid reply, code {p_Task.Result.StatusCode}");

                                HandleError("Connection error\n" + p_Task.Result.ReasonPhrase);

                                return;
                            }

                            /// Get JSON response
                            JObject l_JSON = JObject.Parse(p_Task.Result.BodyString);
#if DEBUG
                            if (Config.DebugNetwork)
                            {
                                l_Timer.Stop();
                                Logger.log?.Debug("[Network][ServerConnection.SendQuery] Query execution time " + l_Timer.ElapsedMilliseconds + "ms");
                                Logger.log?.Debug(JsonConvert.SerializeObject(l_JSON, Formatting.Indented));
                            }
#endif

                            /// Result object
                            Methods._MethodResult l_Result = null;

                            /// Reply not OK
                            if (l_JSON.ContainsKey("error"))
                            {
                                Logger.log?.Error($"[Network][ServerConnection.SendQuery] Query \"{p_Query._MethodName}\" invalid reply, json error code {l_JSON["error"]}");

                                HandleError("Connection error\n" + "Server replied an invalid response, please send us WTB latest log !");

                                return;
                            }
                            /// Reply OK
                            else if (p_Query._MethodResultType != null)
                            {
                                l_Result = (Methods._MethodResult)Activator.CreateInstance(p_Query._MethodResultType);

                                if (!l_Result.Deserialize(l_JSON["result"] as JObject))
                                {
                                    Logger.log?.Error($"[Network][ServerConnection.SendQuery] Query \"{p_Query._MethodName}\" invalid reply format");

                                    HandleError("Internal plugin error, please send us WTB latest log !");

                                    return;
                                }
                            }

                            if (p_Query._MethodResultType != null && Methods._MethodResultHandlerAttribute.s_Handlers.TryGetValue(p_Query._MethodResultType, out var l_Handler))
                                HMMainThreadDispatcher.instance.Enqueue(() => l_Handler.Call((Methods._MethodResult)l_Result));
                            else if (p_Query._MethodResultType != null)
                            {
                                Logger.log?.Debug($"[Network][ServerConnection.SendQuery] Query \"{p_Query._MethodName}\" has no handler !");

                                HandleError("Internal plugin error, please send us WTB latest log !");
                                return;
                            }
                        }
                        else
                        {
                            Logger.log?.Error("[Network][ServerConnection.SendQuery] Failed : ");
                            Logger.log?.Error(p_Task.Exception);
                        }
                    }
                    catch (Exception l_Exception)
                    {
                        Logger.log?.Error("[Network][ServerConnection.SendQuery] Exception");
                        Logger.log?.Error(l_Exception);

                        HandleError("Internal plugin error, please send us WTB latest log !");
                    }
                });
            }
            catch (System.Exception p_Exception)
            {
                Logger.log?.Error("[Network][ServerConnection.SendQuery] Failed : ");
                Logger.log?.Error(p_Exception);

                HandleError("Connection error\n" + "Server replied an invalid response, please send us WTB latest log !");
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Build query
        /// </summary>
        /// <typeparam name="T">Query type</typeparam>
        /// <param name="p_Query">Query data</param>
        /// <param name="p_Attribute">Result attribute</param>
        /// <returns></returns>
        private static string BuildQuery<T>(T p_Query) where T : Methods._Method
        {
            /// Build RPC 2.0 query
            RPCQuery l_RPCQuery = new RPCQuery();
            l_RPCQuery.jsonrpc      = "2.0";
            l_RPCQuery.id           = 1;
            l_RPCQuery.method       = p_Query._MethodName;
            l_RPCQuery.parameters   = p_Query.Serialize();

            /// Convert to JObject
            JObject l_JSONQuery = JObject.FromObject(l_RPCQuery);

            /// Add token to the query if needed
            if (p_Query._MethodAuth == Methods._Method.AuthType.Token)
                l_JSONQuery["params"]["UserToken"] = m_UserToken;

#if DEBUG
            return JsonConvert.SerializeObject(l_JSONQuery, Formatting.Indented);
#else
            return JsonConvert.SerializeObject(l_JSONQuery, Formatting.None);
#endif
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Handle a error
        /// </summary>
        /// <param name="p_ErrorMessage">Error message to display</param>
        private static void HandleError(string p_ErrorMessage)
        {
            Logger.log.Error("ERRRROR " + p_ErrorMessage);
            /// Show connection error
            HMMainThreadDispatcher.instance.Enqueue(() =>
            {
                Views.ViewFlowCoordinator.Instance().connectionError.SetMessageModal_PendingMessage(p_ErrorMessage);
                Views.ViewFlowCoordinator.Instance().SwitchToConnectionError();
            });
        }
    }
}
