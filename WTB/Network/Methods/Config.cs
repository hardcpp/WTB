#pragma warning disable 0649

using Newtonsoft.Json.Linq;
using System;

namespace WTB.Network.Methods
{
    internal class Config : _Method
    {
        internal override string _MethodName => "Config";
        internal override AuthType _MethodAuth => AuthType.None;
        internal override Type _MethodResultType => typeof(Config_Result);
        internal override bool _MethodShouldRetryOnFailure => true;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        internal string Version;
        internal string GameVersion;

        protected override JObject SerializeImpl()
        {
            JObject l_Result = new JObject();
            l_Result["Version"]     = Version;
            l_Result["GameVersion"] = GameVersion;

            return l_Result;
        }
    }

    internal class Config_Result : _MethodResult
    {
        internal bool IsUpdated;
        internal string LastVersion;
        internal float NetworkTickRate;

        protected override void DeserializeImpl(JObject p_Data)
        {
            IsUpdated       = p_Data["IsUpdated"].Value<bool>();
            LastVersion     = p_Data["LastVersion"].Value<string>();
            NetworkTickRate = p_Data["NetworkTickRate"].Value<float>();
        }
    }
}
