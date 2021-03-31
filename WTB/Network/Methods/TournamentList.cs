#pragma warning disable 0649

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace WTB.Network.Methods
{
    internal class TournamentList : _Method
    {
        internal override string _MethodName => "TournamentList";
        internal override AuthType _MethodAuth => AuthType.Token;
        internal override Type _MethodResultType => typeof(TournamentList_Result);
        internal override bool _MethodShouldRetryOnFailure => false;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        internal int Page;

        protected override JObject SerializeImpl()
        {
            JObject l_Result = new JObject();
            l_Result["Page"] = Page;

            return l_Result;
        }
    }

    internal class TournamentList_Result : _MethodResult
    {
        internal class TournamentEntry
        {
            internal int ID;
            internal string Name;
            internal string LogoSmall;
            internal string State;

            internal void Deserialize(JObject p_Data)
            {
                ID          = p_Data["ID"].Value<int>();
                Name        = p_Data["Name"].Value<string>();
                LogoSmall   = p_Data["LogoSmall"].Value<string>();
                State       = p_Data["State"].Value<string>();
            }
        }

        internal int Page;
        internal bool HasMore;
        internal IList<TournamentEntry> Tournaments;

        protected override void DeserializeImpl(JObject p_Data)
        {
            Page    = p_Data["Page"].Value<int>();
            HasMore = p_Data["HasMore"].Value<bool>();

            var l_Tournaments = new List<TournamentEntry>();
            foreach (JObject l_Current in p_Data["Tournaments"])
            {
                var l_Tournament = new TournamentEntry();
                l_Tournament.Deserialize(l_Current);

                l_Tournaments.Add(l_Tournament);
            }

            Tournaments = l_Tournaments;
        }
    }
}
