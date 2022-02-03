using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using MtgoReplayToolWpf.DataGridData;

namespace MtgoReplayToolWpf.DataGridViewModels
{
    public class DataGridDeckDefinitionsViewModel : DataGridViewModelBase
    {
        private DataGridCellInfo _dataGridDeckDefinitionsCurrentCell;
        private DataGridCellInfo _dataGridDeckListsCurrentCell;

        public ObservableCollection<DeckDefinitionData> DataDeckDefinitions { get; set; }

        public ObservableCollection<DeckListData> DataDeckLists { get; set; }

        public ObservableCollection<String> DataDeckCards { get; set; }

        public ICollectionView DataViewDeckDefinitions { get; set; }

        public ICollectionView DataViewDeckLists { get; set; }

        public ICollectionView DataViewDeckCards { get; set; }

        public String DeckDefinitionFilter { get; set; }

        public DataGridCellInfo DataGridDeckDefinitionsCurrentCell
        {
            get => _dataGridDeckDefinitionsCurrentCell;
            set 
            {
                if (value.IsValid)
                {
                    _dataGridDeckDefinitionsCurrentCell = value;
                    DataGridDeckDefinitionsCurrentCellChanged();
                }
            }
        }
        
        public DataGridCellInfo DataGridDeckListsCurrentCell
        {
            get => _dataGridDeckListsCurrentCell;
            set
            {
                if (value.IsValid)
                {
                    _dataGridDeckListsCurrentCell = value;
                    DataGridDeckListsCurrentCellChanged();
                }
            }
        }
        
        public IList<DataGridCellInfo> DataGridDeckListsSelectedCells { get; set; }

        public IList<DataGridCellInfo> DataGridDeckDefinitionsSelectedCells { get; set; }

        public Dictionary<String, Format> Decks { get; }

        public String Deck { get; set; }

        public DataGridDeckDefinitionsViewModel(Dictionary<String, Format> decks)
        {
            DataDeckDefinitions = new ObservableCollection<DeckDefinitionData>();
            DataViewDeckDefinitions = CollectionViewSource.GetDefaultView(DataDeckDefinitions);


            DataDeckLists = new ObservableCollection<DeckListData>();
            DataViewDeckLists = CollectionViewSource.GetDefaultView(DataDeckLists);


            DataDeckCards = new ObservableCollection<String>();
            DataViewDeckCards = CollectionViewSource.GetDefaultView(DataDeckCards);
            Decks = decks;
        }

        internal void SetSource(List<DeckDefinitionData> dataGridDeckDefinitionsData)
        {
            DataDeckDefinitions.Clear();
            dataGridDeckDefinitionsData.ForEach(x => DataDeckDefinitions.Add(x));
            DataViewDeckDefinitions.SortDescriptions.Clear();
            //DataView.SortDescriptions.Add(new SortDescription("Size", ListSortDirection.Descending));
            DataViewDeckDefinitions.Refresh();
        }

        public DataGridDeckDefinitionsViewModel()
        {

        }

        private void SetSourceDataGridDecklists(List<DeckListData> sourceList)
        {
            DataDeckLists.Clear();
            sourceList.ForEach(x => DataDeckLists.Add(x));
            DataViewDeckLists.SortDescriptions.Clear();
            //DataView.SortDescriptions.Add(new SortDescription("Size", ListSortDirection.Descending));
            DataViewDeckLists.Refresh();
        }

        private void SetSourceDataGridCards(List<String> sourceList)
        {
            DataDeckCards.Clear();
            sourceList.ForEach(x => DataDeckCards.Add(x));
            DataViewDeckCards.SortDescriptions.Clear();
            //DataView.SortDescriptions.Add(new SortDescription("Size", ListSortDirection.Descending));
            DataViewDeckCards.Refresh();
        }

        private void DataGridDeckDefinitionsCurrentCellChanged()
        {
            if (DataGridDeckDefinitionsCurrentCell.Item is DeckDefinitionData data)
            {
                SetSourceDataGridDecklists(data.DeckLists);
            }
        }

        private void DataGridDeckListsCurrentCellChanged()
        {
            if (DataGridDeckListsCurrentCell.Item is DeckListData data)
            {
                SetSourceDataGridCards(data.Cards);
            }
        }

        public void DataGridDecklistsSelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            DataGridDeckListsSelectedCells = (sender as DataGrid)?.SelectedCells;
        }

        internal void DataGridDeckDefinitionsSelectedCellsChanged(Object sender, SelectedCellsChangedEventArgs e)
        {
            DataGridDeckDefinitionsSelectedCells = (sender as DataGrid)?.SelectedCells;
        }

        public async void ReclassifyAllDecks()
        {
            UiHelper.LockUi();
            var result = await ThreadingHelper.StartSTATask(() =>
            {
                var allLists = new List<DeckListData>();
                DataDeckDefinitions.ToList().ForEach((x) => allLists.AddRange(x.DeckLists));
                UiHelper.Maximum = allLists.Count;
                allLists.AsParallel().ForAll(x => ReclassifyDeckDefinition(x));
                return true;
            });
            UiHelper.UnlockUi();
        }

        private void ReclassifyDeckDefinition(DeckListData data)
        {
            var compareDecks = DeckHelper.CompareDecks(Decks, data.Cards.Select(x => x.Substring(x.IndexOf(" ") + 1)).ToList(), data.Date);
            if (compareDecks.Count > 0)
            {
                var percentile = compareDecks.Where(x => x.Item1 > compareDecks[0].Item1 * 0.75);
                var groups = percentile.GroupBy(x => x.Item3);
                var orderedGroups = groups.OrderByDescending(x => x.Count());
                DeckHelper.WriteDeckToFile(data.Name, orderedGroups.ElementAt(0).ElementAt(0).Item2, orderedGroups.ElementAt(0).ElementAt(0).Item3, data.Date, data.Cards, false);
            }
            

            lock (UiHelper.MainWindow)
            {
                UiHelper.AddProgress(1.0);
            }
        }

        public async void ReclassifySelectedDecks()
        {
            UiHelper.LockUi();

            var allDecks = Decks.Values.Select(x => x.Decks).Aggregate(new List<Deck>(), (aggr, inc) =>
            {
                aggr.AddRange(inc);
                return aggr;
            });

            foreach (var cell in DataGridDeckListsSelectedCells)
            {
                if (cell.Column.Header.Equals("Name"))
                {
                    if (cell.Item is DeckListData data)
                    {
                        var deck = allDecks.Where(x => x.Filepath.Equals(data.Name)).FirstOrDefault();
                        if (deck != null)
                        {
                            deck.Name = Deck;
                            deck.ManuallyEdited = true;
                            var result = await ThreadingHelper.StartSTATask(() =>
                            {
                                DeckHelper.WriteDeckToFile(deck.Filepath, deck.Format, deck.Name, deck.Date, deck.Definition, true);
                                return true;
                            });
                        }
                        //var compareDecks = DeckHelper.CompareDecks(Decks, data.Cards.Select(x => x.Substring(x.IndexOf(" ") + 1)).ToList(), data.Date);
                        //var percentile = compareDecks.Where(x => x.Item1 > compareDecks[0].Item1 * 0.75);
                        //var groups = percentile.GroupBy(x => x.Item3);
                        //var orderedGroups = groups.OrderByDescending(x => x.Count());
                        //DeckHelper.WriteDeckToFile(data.Name, orderedGroups.ElementAt(0).ElementAt(0).Item2, orderedGroups.ElementAt(0).ElementAt(0).Item3, data.Date, data.Cards);
                        
                }
                }
            }
                
            UiHelper.UnlockUi();            
        }

        public async void ResetManuallyEditedSelectedDecks()
        {
            UiHelper.LockUi();

            var allDecks = Decks.Values.Select(x => x.Decks).Aggregate(new List<Deck>(), (aggr, inc) =>
            {
                aggr.AddRange(inc);
                return aggr;
            });

            foreach (var cell in DataGridDeckDefinitionsSelectedCells)
            {
                if (cell.Column.Header.Equals("Deck"))
                {
                    if (cell.Item is DeckDefinitionData data)
                    {
                        foreach (var decklistData in data.DeckLists)
                        {
                            var deck = allDecks.Where(x => x.Filepath.Equals(decklistData.Name)).FirstOrDefault();
                            if (deck != null)
                            {
                                deck.ManuallyEdited = !deck.ManuallyEdited;
                                var result = await ThreadingHelper.StartSTATask(() =>
                                {
                                    DeckHelper.WriteDeckToFile(deck.Filepath, deck.Format, deck.Name, deck.Date, deck.Definition, deck.ManuallyEdited);
                                    return true;
                                });
                            }
                        }
                    }
                }
            }

            UiHelper.UnlockUi();
        }
    }
}
