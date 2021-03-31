#pragma warning disable 0649

using Newtonsoft.Json.Linq;
using System;

namespace WTB.Network.Methods
{
    internal class MatchInput : _Method
    {
        internal override string _MethodName => "MatchInput";
        internal override AuthType _MethodAuth => AuthType.Token;
        internal override Type _MethodResultType => null;
        internal override bool _MethodShouldRetryOnFailure => true;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        internal int MatchID;
        internal string InputType;
        internal JArray InputData;

        protected override JObject SerializeImpl()
        {
            JObject l_Result = new JObject();
            l_Result["MatchID"]     = MatchID;
            l_Result["InputType"]   = InputType;
            l_Result["InputData"]   = InputData;

            return l_Result;
        }
    }
}
