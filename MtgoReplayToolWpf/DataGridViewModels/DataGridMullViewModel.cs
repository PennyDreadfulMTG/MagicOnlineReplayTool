using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using MtgoReplayToolWpf.DataGridData;

namespace MtgoReplayToolWpf.DataGridViewModels
{
    public class DataGridMullViewModel : DataGridViewModelBase
    {
        public ObservableCollection<MullData> Data { get; set; }

        public ICollectionView DataView { get; set; }

        public DataGridMullViewModel()
        {
            Data = new ObservableCollection<MullData>();
            DataView = CollectionViewSource.GetDefaultView(Data);
        }

        internal void SetSource(List<MullData> dataGridMullData)
        {
            Data.Clear();
            dataGridMullData.ForEach(x => Data.Add(x));
            DataView.SortDescriptions.Clear();
            DataView.SortDescriptions.Add(new SortDescription("Number", ListSortDirection.Descending));
            DataView.Refresh();
        }
    }
}
