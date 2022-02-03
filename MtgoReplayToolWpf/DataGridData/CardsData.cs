using System;

namespace MtgoReplayToolWpf.DataGridData
{
    public class CardsData
    {
        public String Name { get; set; }
        public Int32 Games { get; set; }
        public Int32 GamesWon { get; set; }
        public Double GameWin { get; set; }
        public Double Correlation { get; set; }

        public CardsData(String cardName, Int32[] data, Double correlation)
        {
            Name = cardName;

            Games = data[0];

            GamesWon = data[1];

            GameWin = (Double)data[1]/data[0];

            Correlation = correlation;
        }

        public CardsData()
        {
        }
    }
}
