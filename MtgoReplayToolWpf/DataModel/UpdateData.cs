using System;
using System.Collections.Generic;
using MtgoReplayToolWpf.DataGridData;

namespace MtgoReplayToolWpf
{
    public class UpdateData
    {
        public List<KeyValuePair<String, Int32>> PlayerList { get; set; }

        public List<KeyValuePair<String, Int32>> DeckList { get; set; }

        public DataCollection SourceData { get; set; }

        public List<KeyValuePair<DateTime, Double>> TimeSeriesList { get; set; }

    }
}