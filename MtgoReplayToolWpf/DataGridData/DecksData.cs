using MtgoReplayToolWpf.MiscHelpers;
using System;

namespace MtgoReplayToolWpf.DataGridData
{
    public class DecksData
    {
        public String Header { get; set; }
        public String Names { get; set; }

        public Double MatchWin => 100.0 * MatchWon / Matches;
        public Double GameWin => 100.0 * GamesWon / Games;
        public Double GameWinDraw => 100.0 * GamesWonDraw / GamesDraw;
        public Double GameWinPlay => 100.0 * GamesWonPlay / GamesPlay;
        public Double GameWinPre => 100.0 * GamesWonPre / GamesPre;
        public Double GameWinPost => 100.0 * GamesWonPost / GamesPost;

        public Int32 Matches { get; set; }
        public Int32 MatchWon { get; set; }
        public Int32 Games { get; set; }
        public Int32 GamesWon { get; set; }
        public Int32 GamesDraw { get; set; }
        public Int32 GamesWonDraw { get; set; }
        public Int32 GamesPlay { get; set; }
        public Int32 GamesWonPlay { get; set; }
        public Int32 GamesPre { get; set; }
        public Int32 GamesWonPre { get; set; }
        public Int32 GamesPost { get; set; }
        public Int32 GamesWonPost { get; set; }

        public Double ConfidenceDraw => 100 * StatisticsHelper.TwoProportionZTest(GamesWonDraw, GamesDraw, GamesWonPlay, GamesPlay);
        public Double ConfidencePlay => 100 - ConfidenceDraw;
        public Double ConfidencePre => 100 * StatisticsHelper.TwoProportionZTest(GamesWonPre, GamesPre, GamesWonPost, GamesPost);
        public Double ConfidencePost => 100 - ConfidencePre;

        public DecksData()
        {
        }

        public DecksData(String header, String name)
        {
            Header = header;
            Names = name;
        }
    }
}
