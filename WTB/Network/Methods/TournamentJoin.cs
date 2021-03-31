#pragma warning disable 0649

using Newtonsoft.Json.Linq;
using System;

namespace WTB.Network.Methods
{
    internal class TournamentJoin : _Method
    {
        internal override string _MethodName => "TournamentJoin";
        internal override AuthType _MethodAuth => AuthType.Token;
        internal override Type _MethodResultType => typeof(TournamentJoin_Result);
        internal override bool _MethodShouldRetryOnFailure => false;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        internal int ID;

        protected override JObject SerializeImpl()
        {
            JObject l_Result = new JObject();
            l_Result["ID"] = ID;

            return l_Result;
        }
    }

    internal class TournamentJoin_Result : _MethodResult
    {
        internal bool   CanJoin;
        internal string ErrorMessage;
        internal string JoinType;
        internal UInt32 EndTime;

        protected override void DeserializeImpl(JObject p_Data)
        {
            CanJoin         = p_Data["CanJoin"].Value<bool>();
            ErrorMessage    = p_Data["ErrorMessage"].Value<string>();
            JoinType        = p_Data["JoinType"].Value<string>();
            EndTime         = p_Data["EndTime"].Value<UInt32>();
        }
    }
}
