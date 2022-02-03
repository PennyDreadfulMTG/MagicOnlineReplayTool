
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using MtgoReplayToolWpf.DataGridData;
using MtgoReplayToolWpf.DataModel;

namespace MtgoReplayToolWpf.DataGridViewModels
{
    public class DataGridGameListViewModel : DataGridViewModelBase
    {
        public ObservableCollection<GameData> Data { get; set; }

        public ICollectionView DataView { get; set; }

        public MainData AllMatches { get; set; }

        public DataGridGameListViewModel(MainData allMatches)
        {
            Data = new ObservableCollection<GameData>();
            DataView = CollectionViewSource.GetDefaultView(Data);
            AllMatches = allMatches;
        }

        internal void SetSource(List<GameData> dataGridGameData)
        {
            Data.Clear();
            dataGridGameData.ForEach(x => Data.Add(x));
            DataView.SortDescriptions.Clear();
            DataView.Refresh();
        }
    }
}
