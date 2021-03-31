#pragma warning disable 0649

using Newtonsoft.Json.Linq;
using System;

namespace WTB.Network.Methods
{
    internal class MatchJoin : _Method
    {
        internal override string _MethodName => "MatchJoin";
        internal override AuthType _MethodAuth => AuthType.Token;
        internal override Type _MethodResultType => typeof(MatchJoin_Result);
        internal override bool _MethodShouldRetryOnFailure => false;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        internal int TournamentID;

        protected override JObject SerializeImpl()
        {
            JObject l_Result = new JObject();
            l_Result["TournamentID"] = TournamentID;

            return l_Result;
        }
    }

    internal class MatchJoin_Result : _MethodResult
    {
        internal bool BackToTournamentSelect;
        internal string BackMessage;
        internal int MatchID;
        internal string MatchName;
        protected override void DeserializeImpl(JObject p_Data)
        {
            BackToTournamentSelect = p_Data["BackToTournamentSelect"].Value<bool>();
            BackMessage            = p_Data["BackMessage"].Value<string>();
            MatchID                = p_Data["MatchID"].Value<int>();
            MatchName              = p_Data["MatchName"].Value<string>();
        }
    }
}
