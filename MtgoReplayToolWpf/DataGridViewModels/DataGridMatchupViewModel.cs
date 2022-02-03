using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using MtgoReplayToolWpf.DataGridData;

namespace MtgoReplayToolWpf.DataGridViewModels
{
    public class DataGridMatchupViewModel : DataGridViewModelBase
    {
        public ObservableCollection<CardsData> Data { get; set; }

        public ICollectionView DataView { get; set; }

        public DataGridMatchupViewModel()
        {
            Data = new ObservableCollection<CardsData>();
            DataView = CollectionViewSource.GetDefaultView(Data);
        }

        internal void SetSource(List<CardsData> dataGridMatchupData)
        {
            Data.Clear();
            dataGridMatchupData.ForEach(x => Data.Add(x));
            DataView.SortDescriptions.Clear();
            DataView.SortDescriptions.Add(new SortDescription("Games", ListSortDirection.Descending));
            DataView.Refresh();
        }
    }
}
