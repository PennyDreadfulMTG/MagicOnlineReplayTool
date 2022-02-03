using System;

namespace MtgoReplayToolWpf.DataGridData
{
    public class DuplicateDecksData
    {
        public String FirstDeck { get; set; }

        public String SecondDeck { get; set; }

        public Double Homogeneity { get; set; }

        public DuplicateDecksData(String firstDeck, String secondDeck, Double homogeneity)
        {
            FirstDeck = firstDeck;
            SecondDeck = secondDeck;
            Homogeneity = homogeneity;
        }

        public DuplicateDecksData()
        {

        }
    }
}
