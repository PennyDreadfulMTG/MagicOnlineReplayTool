using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using MtgoReplayToolWpf.DataGridData;
using MtgoReplayToolWpf.DataModel;

namespace MtgoReplayToolWpf.DataGridViewModels
{
    public class DataGridDecksViewModel : DataGridViewModelBase
    {
        public ObservableCollection<DecksData> Data { get; set; }

        public String Header { get; set; }

        public ICollectionView DataView { get; set; }

        public IList<DataGridCellInfo> DataGridSelectedCells { get; private set; }

        public Dictionary<String, Format> Decks { get; }
        
        public MainData AllMatches { get; }

        public DataGridDecksViewModel(Dictionary<String, Format> decks, String header, MainData allMatches)
        {
            Header = header;
            AllMatches = allMatches;
            Data = new ObservableCollection<DecksData>();
            DataView = CollectionViewSource.GetDefaultView(Data);
            Decks = decks;
        }

        public DataGridDecksViewModel()
        {
        }

        internal void DataGridSelectedCellsChanged(Object sender, SelectedCellsChangedEventArgs e)
        {
            DataGridSelectedCells = (sender as DataGrid)?.SelectedCells;
        }

        internal void MergeDecklists()
        {
            var decksToMergeList = new List<String>();
            var playersToMergeList = new List<String>();

            foreach (var cell in DataGridSelectedCells)
            {
                if (cell.Column.Header != null && cell.Column.Header.Equals("Matches"))
                {
                    if (cell.Item is DecksData data)
                    {
                        if (Header == "Deck")
                        {

                            decksToMergeList.Add(data.Names);

                        }
                        else
                        {

                            playersToMergeList.Add(data.Names);

                        }
                    }
                }
            }

            if (decksToMergeList.Count > 1)
            {
                //if (AllMatches.HasUnsavedChanges)
                //{
                //    MessageBox.Show("Cannot merge decks while you have unsaved changes.", "Merge Decks", MessageBoxButton.OK);
                //    return;                    
                //}

                var message = "Are you sure you want to merge the following decks? This will change your deck definitions and is not reversible!" + Environment.NewLine;
                decksToMergeList.ForEach(x => message += Environment.NewLine + x);
                if (MessageBox.Show(message, "Merge Decks", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    DeckHelper.MergeDecks(decksToMergeList, Decks, AllMatches.Matches, " - ");
                    AllMatches.HasUnsavedChanges = true;
                }
            }
        }

        internal void SetSource(List<DecksData> dataGridDecksData)
        {
            Data.Clear();
            dataGridDecksData.ForEach(x => Data.Add(x));
            DataView.SortDescriptions.Clear();
            DataView.SortDescriptions.Add(new SortDescription("Matches", ListSortDirection.Descending));
            DataView.Refresh();
        }
    }
}
