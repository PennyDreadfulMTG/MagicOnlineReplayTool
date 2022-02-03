using MtgoReplayToolWpf.DataModel;

namespace MtgoReplayToolWpf.DataGridViewModels
{
    public class DataGridViewModelCollection
    {
        public DataGridDecksViewModel DataGridDecksViewModel { get; set; }

        public DataGridDecksViewModel DataGridPlayersViewModel { get; set; }

        public DataGridDecksViewModel DataGridVsDecksViewModel { get; set; }

        public DataGridDecksViewModel DataGridVsPlayersViewModel { get; set; }

        public DataGridMullViewModel DataGridMullViewModel { get; set; }

        public DataGridMullViewModel DataGridTurnsViewModel { get; set; }

        public DataGridTimeChartViewModel DataGridTimeChartViewModel { get; set; }

        public DataGridMatchupViewModel DataGridMatchupViewModel { get; set; }

        public DataGridDuplicatesViewModel DataGridDuplicatesViewModel { get; set; }

        public DataGridDeckDefinitionsViewModel DataGridDeckDefinitionsViewModel { get; set; }

        public DataGridGameListViewModel DataGridGameListViewModel { get; set; }

        public DataGridViewModelCollection(MainData data)
        {
            DataGridDecksViewModel = new DataGridDecksViewModel();

            DataGridPlayersViewModel = new DataGridDecksViewModel();

            DataGridVsDecksViewModel = new DataGridDecksViewModel();

            DataGridVsPlayersViewModel = new DataGridDecksViewModel();

            DataGridMullViewModel = new DataGridMullViewModel();

            DataGridTurnsViewModel = new DataGridMullViewModel();

            DataGridTimeChartViewModel = new DataGridTimeChartViewModel();

            DataGridMatchupViewModel = new DataGridMatchupViewModel();

            DataGridDuplicatesViewModel = new DataGridDuplicatesViewModel();

            DataGridDeckDefinitionsViewModel = new DataGridDeckDefinitionsViewModel();

            DataGridGameListViewModel = new DataGridGameListViewModel(data);
        }
    }
}
