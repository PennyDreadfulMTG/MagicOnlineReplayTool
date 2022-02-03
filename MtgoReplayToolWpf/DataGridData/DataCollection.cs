using System.Collections.Generic;

namespace MtgoReplayToolWpf.DataGridData
{
    public class DataCollection
    {
        public List<DecksData> DataGridDecksData { get; set; }

        public List<DecksData> DataGridPlayersData { get; set; }

        public List<DecksData> DataGridVsDecksData { get; set; }

        public List<DecksData> DataGridVsPlayersData { get; set; }

        public List<MullData> DataGridMullData { get; set; }

        public List<MullData> DataGridTurnsData { get; set; }

        public List<TimeSeriesData> DataGridTimeChartData { get; set; }

        public List<CardsData> DataGridMatchupData { get; set; }

        public List<DuplicateDecksData> DataGridDuplicatesData { get; set; }

        public List<DeckDefinitionData> DataGridDeckDefinitionsData { get; set; }

        public List<GameData> DataGridGamesData { get; set; }

        public DataCollection()
        {
            DataGridDecksData = new List<DecksData>();

            DataGridPlayersData = new List<DecksData>();

            DataGridVsDecksData = new List<DecksData>();

            DataGridVsPlayersData = new List<DecksData>();

            DataGridMullData = new List<MullData>();

            DataGridTurnsData = new List<MullData>();

            DataGridTimeChartData = new List<TimeSeriesData>();

            DataGridMatchupData = new List<CardsData>();

            DataGridDuplicatesData = new List<DuplicateDecksData>();

            DataGridDeckDefinitionsData = new List<DeckDefinitionData>();

            DataGridGamesData = new List<GameData>();
        }
    }
}
