#pragma warning disable 0649

using Newtonsoft.Json.Linq;
using System;

namespace WTB.Network.Methods
{
    internal class MatchPull : _Method
    {
        internal override string    _MethodName                 => "MatchPull";
        internal override AuthType  _MethodAuth                 => AuthType.Token;
        internal override Type      _MethodResultType           => typeof(MatchPull_Result);
        internal override bool      _MethodShouldRetryOnFailure => true;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        internal int MatchID;
        internal int RPCId;

        protected override JObject SerializeImpl()
        {
            JObject l_Result = new JObject();
            l_Result["MatchID"] = MatchID;
            l_Result["RPCId"]   = RPCId;

            return l_Result;
        }
    }

    internal class MatchPull_Result : _MethodResult
    {
        internal bool       BackToTournamentSelect;
        internal string     BackMessage;
        internal int        RPCId;
        internal string     RPCState;
        internal JObject    RPCData;

        protected override void DeserializeImpl(JObject p_Data)
        {
            BackToTournamentSelect  = p_Data["BackToTournamentSelect"].Value<bool>();
            BackMessage             = p_Data["BackMessage"].Value<string>();
            RPCId                   = p_Data["RPCId"].Value<int>();
            RPCState                = p_Data["RPCState"].Value<string>();
            RPCData                 = p_Data["RPCData"] as JObject;
        }
    }
}
