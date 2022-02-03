using System;
using System.Collections.Generic;

namespace MtgoReplayToolWpf.DataGridData
{
    public class DeckListData
    {
        public String Name { get; set; }
        public DateTime Date { get; set; }
        public Double Homogeneity { get; set; }
        public List<String> Cards { get; set; }

        public DeckListData(String name, DateTime date, Double homogeneity, List<String> cards)
        {
            Name = name;
            Date = date;
            Homogeneity = homogeneity;
            Cards = cards;
        }

        public DeckListData()
        {
        }
    }
}