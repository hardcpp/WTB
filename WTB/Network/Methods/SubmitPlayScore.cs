#pragma warning disable 0649

using Newtonsoft.Json.Linq;
using System;

namespace WTB.Network.Methods
{
    internal class SubmitPlayScore : _Method
    {
        internal override string    _MethodName                 => "SubmitPlayScore";
        internal override AuthType  _MethodAuth                 => AuthType.Token;
        internal override Type      _MethodResultType           => typeof(SubmitPlayScore_Result);
        internal override bool      _MethodShouldRetryOnFailure => true;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        internal int    TournamentID;
        internal string Song;
        internal int    Score;
        internal float  Accuracy;
        internal int    CutCount;
        internal int    TotalNoteCount;
        internal int    MaxCombo;

        protected override JObject SerializeImpl()
        {
            JObject l_Result = new JObject();
            l_Result["TournamentID"]    = TournamentID;
            l_Result["Song"]            = Song;
            l_Result["Score"]           = Score;
            l_Result["Accuracy"]        = Accuracy;
            l_Result["CutCount"]        = CutCount;
            l_Result["TotalNoteCount"]  = TotalNoteCount;
            l_Result["MaxCombo"]        = MaxCombo;

            return l_Result;
        }
    }

    internal class SubmitPlayScore_Result : _MethodResult
    {
        internal bool   BackToTournamentSelect;
        internal string BackMessage;
        internal string Song;

        protected override void DeserializeImpl(JObject p_Data)
        {
            BackToTournamentSelect  = p_Data["BackToTournamentSelect"].Value<bool>();
            BackMessage             = p_Data["BackMessage"].Value<string>();
            Song                    = p_Data["Song"].Value<string>();
        }
    }
}
