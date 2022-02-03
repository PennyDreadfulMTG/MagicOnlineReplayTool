using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using MtgoReplayToolWpf.DataGridData;
using MtgoReplayToolWpf.DataModel;
using MTGOReplayToolWpf;

namespace MtgoReplayToolWpf.DataGridViewModels
{
    public class DataGridDuplicatesViewModel : DataGridViewModelBase
    {

        public ObservableCollection<DuplicateDecksData> Data { get; set; }

        public ICollectionView DataView { get; set; }

        public IList<DataGridCellInfo> DataGridSelectedCells { get; set; }

        public Dictionary<String, Format> Decks { get; }

        public MainData AllMatches { get; }

        internal void MergeDecklists()
        {
            foreach (var cell in DataGridSelectedCells)
            {
                if (cell.Column.Header.Equals("FirstDeck"))
                {
                    if (cell.Item is DuplicateDecksData data)
                    {
                        DeckHelper.MergeDecks(new List<String> { data.FirstDeck, data.SecondDeck }, Decks, AllMatches.Matches, "_");
                        AllMatches.HasUnsavedChanges = true;
                    }
                }
            }
        }

        public DataGridDuplicatesViewModel(Dictionary<String, Format> decks, MainData allMatches)
        {
            Data = new ObservableCollection<DuplicateDecksData>();
            DataView = CollectionViewSource.GetDefaultView(Data);
            Decks = decks;
            AllMatches = allMatches;
        }

        public DataGridDuplicatesViewModel()
        {
        }

        internal void DataGridDuplicatesSelectedCellsChanged(Object sender, SelectedCellsChangedEventArgs e)
        {
            DataGridSelectedCells = (sender as DataGrid)?.SelectedCells;
        }

        internal void SetSource(List<DuplicateDecksData> dataGridDuplicatesData)
        {
            Data.Clear();
            dataGridDuplicatesData.ForEach(x => Data.Add((DuplicateDecksData)x));
            DataView.Refresh();
        }
    }
}
