#pragma warning disable 0649

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace WTB.Network.Methods
{
    internal class Playlist : _Method
    {
        internal override string    _MethodName                 => "Playlist";
        internal override AuthType  _MethodAuth                 => AuthType.Token;
        internal override Type      _MethodResultType           => typeof(Playlist_Result);
        internal override bool      _MethodShouldRetryOnFailure => false;

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

    internal class Playlist_Result : _MethodResult
    {
        internal class SongEntry
        {
            internal string Hash;
            internal string Mode;
            internal string Difficulty;

            internal void Deserialize(JObject p_Data)
            {
                Hash        = p_Data["Hash"].Value<string>();
                Mode        = p_Data["Mode"].Value<string>();
                Difficulty  = p_Data["Difficulty"].Value<string>();
            }
        }

        internal bool               BackToTournamentSelect;
        internal string             BackMessage;
        internal IList<SongEntry>   Songs;

        protected override void DeserializeImpl(JObject p_Data)
        {
            BackToTournamentSelect = p_Data["BackToTournamentSelect"].Value<bool>();
            BackMessage            = p_Data["BackMessage"].Value<string>();

            var l_Songs = new List<SongEntry>();
            foreach (JObject l_Current in p_Data["Songs"])
            {
                var l_Song = new SongEntry();
                l_Song.Deserialize(l_Current);
                l_Songs.Add(l_Song);
            }

            Songs = l_Songs;
        }
    }
}
