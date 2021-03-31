#pragma warning disable 0649

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace WTB.Network.Methods
{
    internal class ScoreBoard : _Method
    {
        internal override string    _MethodName                 => "ScoreBoard";
        internal override AuthType  _MethodAuth                 => AuthType.Token;
        internal override Type      _MethodResultType           => typeof(ScoreBoard_Result);
        internal override bool      _MethodShouldRetryOnFailure => false;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        internal int    TournamentID;
        internal string Song;
        internal bool   IsMapScoreBoard;
        internal bool   FocusSelf;
        internal int    Page;

        protected override JObject SerializeImpl()
        {
            JObject l_Result = new JObject();
            l_Result["TournamentID"]    = TournamentID;
            l_Result["Song"]            = Song;
            l_Result["IsMapScoreBoard"] = IsMapScoreBoard;
            l_Result["FocusSelf"]       = FocusSelf;
            l_Result["Page"]            = Page;

            return l_Result;
        }
    }

    internal class ScoreBoard_Result : _MethodResult
    {
        internal class ScoreEntry
        {
            internal string Name;
            internal float  Accuracy;
            internal int    Score;
            internal int    CutCount;
            internal int    TotalNoteCount;
            internal int    MaxCombo;
            internal bool   Qualified;

            internal void Deserialize(JObject p_Data)
            {
                Name            = p_Data["Name"].Value<string>();
                Accuracy        = p_Data["Accuracy"].Value<float>();
                Score           = p_Data["Score"].Value<int>();
                CutCount        = p_Data["CutCount"].Value<int>();
                TotalNoteCount  = p_Data["TotalNoteCount"].Value<int>();
                MaxCombo        = p_Data["MaxCombo"].Value<int>();
                Qualified       = p_Data["Qualified"].Value<bool>();
            }
        }

        internal int                Page;
        internal bool               HasMore;
        internal int                MyRank;
        internal IList<ScoreEntry>  Scores;

        protected override void DeserializeImpl(JObject p_Data)
        {
            Page    = p_Data["Page"].Value<int>();
            HasMore = p_Data["HasMore"].Value<bool>();
            MyRank  = p_Data["MyRank"].Value<int>();

            var l_Scores = new List<ScoreEntry>();
            foreach (JObject l_Current in p_Data["Scores"])
            {
                var l_Score = new ScoreEntry();
                l_Score.Deserialize(l_Current);
                l_Scores.Add(l_Score);
            }

            Scores = l_Scores;
        }
    }
}
