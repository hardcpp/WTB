#pragma warning disable 0649

using Newtonsoft.Json.Linq;
using System;

namespace WTB.Network.Methods
{
    internal class ChangeLog : _Method
    {
        internal override string _MethodName => "ChangeLog";
        internal override AuthType _MethodAuth => AuthType.None;
        internal override Type _MethodResultType => typeof(ChangeLog_Result);
        internal override bool _MethodShouldRetryOnFailure => false;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        protected override JObject SerializeImpl()
        {
            JObject l_Result = new JObject();
            return l_Result;
        }
    }

    internal class ChangeLog_Result : _MethodResult
    {
        internal string Content;

        protected override void DeserializeImpl(JObject p_Data)
        {
            Content = p_Data["Content"].Value<string>();
        }
    }
}
