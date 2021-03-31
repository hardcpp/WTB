#pragma warning disable 0649

using Newtonsoft.Json.Linq;
using System;

namespace WTB.Network.Methods
{
    internal class Authenticate : _Method
    {
        internal override string _MethodName => "Authenticate";
        internal override AuthType _MethodAuth => AuthType.None;
        internal override Type _MethodResultType => typeof(Authenticate_Result);
        internal override bool _MethodShouldRetryOnFailure => false;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        internal string Token;
        internal string ScoreSaberID;

        protected override JObject SerializeImpl()
        {
            JObject l_Result = new JObject();
            l_Result["Token"]           = Token;
            l_Result["ScoreSaberID"]    = ScoreSaberID;

            return l_Result;
        }
    }

    internal class Authenticate_Result : _MethodResult
    {
        internal bool IsValid;
        internal string NewToken;
        internal bool IsDeclined;

        protected override void DeserializeImpl(JObject p_Data)
        {
            IsValid             = p_Data["IsValid"].Value<bool>();
            NewToken            = p_Data["NewToken"].Value<string>();
            IsDeclined          = p_Data["IsDeclined"].Value<bool>();
        }
    }
}
