#pragma warning disable 0649

using Newtonsoft.Json.Linq;
using System;

namespace WTB.Network.Methods
{
    internal class TournamentInfo : _Method
    {
        internal override string _MethodName => "TournamentInfo";
        internal override AuthType _MethodAuth => AuthType.Token;
        internal override Type _MethodResultType => typeof(TournamentInfo_Result);
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

    internal class TournamentInfo_Result : _MethodResult
    {
        internal int ID;
        internal bool HasExpired;
        internal string Name;
        internal string Banner;
        internal string Description;
        internal string MoreInfoLink;
        internal string TwitchLink;
        internal string DiscordLink;
        internal bool CanJoin;

        protected override void DeserializeImpl(JObject p_Data)
        {
            ID          = p_Data["ID"].Value<int>();
            HasExpired  = p_Data["HasExpired"].Value<bool>();
            Name        = p_Data["Name"].Value<string>();
            Banner      = p_Data["Banner"].Value<string>();
            Description = p_Data["Description"].Value<string>();
            MoreInfoLink= p_Data["MoreInfoLink"].Value<string>();
            TwitchLink  = p_Data["TwitchLink"].Value<string>();
            DiscordLink = p_Data["DiscordLink"].Value<string>();
            CanJoin     = p_Data["CanJoin"].Value<bool>();
        }
    }
}
