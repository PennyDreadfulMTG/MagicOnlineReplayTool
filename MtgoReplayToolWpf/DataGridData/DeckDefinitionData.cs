using System;
using System.Collections.Generic;

namespace MtgoReplayToolWpf.DataGridData
{
    public class DeckDefinitionData
    {
        public String Name { get; set; }
        public Int32 Size { get; set; }
        public Double Homogeneity { get; set; }
        public List<DeckListData> DeckLists { get; set; }

        public DeckDefinitionData(String name, Int32 size, Double homogeneity, List<DeckListData> deckLists)
        {
            Name = name;
            Size = size;
            Homogeneity = homogeneity;
            DeckLists = deckLists;
        }

        public DeckDefinitionData()
        {
        }
    }
}
