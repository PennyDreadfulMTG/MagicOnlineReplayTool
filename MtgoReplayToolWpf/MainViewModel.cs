using System;
using System.Collections.Generic;
using MTGOReplayToolWpf;
using System.Threading.Tasks;
using MtgoReplayToolWpf.DataGridViewModels;
using MtgoReplayToolWpf.DataModel;
using MtgoReplayToolWpf.DeckDefinitions;
using MtgoReplayToolWpf.MiscHelpers;

namespace MtgoReplayToolWpf
{
    public class MainViewModel
    {
        public MainData Data { get; private set; }

        public MainWindow MainWindow { get; }

        public Uploader Uploader { get; private set; }

        public Int32 Progress { get; set; }

        public Int32 ProgressMax { get; set; }

        public Boolean ProgressActive { get; set; }

        public DataGridViewModelCollection DataGridViewModelCollection { get; set; }

        public Dictionary<String, Format> Decks { get; private set; }

        public MainViewModel(MainWindow mainWindow)
        {
            Data = new MainData();
            MainWindow = mainWindow;
            UiHelper.MainWindow = mainWindow;
            var loader = new DataLoader(MainWindow);
            loader.OnFinishedLoading += LoaderOnOnFinishedLoading;
            
            Uploader = new Uploader(MainWindow.ProgressBar);

            DataGridViewModelCollection = new DataGridViewModelCollection(Data);

            MainWindow.DataGridDecks.DataContext = DataGridViewModelCollection.DataGridDecksViewModel;
            MainWindow.DataGridPlayers.DataContext = DataGridViewModelCollection.DataGridPlayersViewModel;
            MainWindow.DataGridVsDecks.DataContext = DataGridViewModelCollection.DataGridVsDecksViewModel;
            MainWindow.DataGridVsPlayers.DataContext = DataGridViewModelCollection.DataGridVsPlayersViewModel;
            MainWindow.DataGridMulligan.DataContext = DataGridViewModelCollection.DataGridMullViewModel;
            MainWindow.DataGridTurns.DataContext = DataGridViewModelCollection.DataGridTurnsViewModel;
            MainWindow.DataGridTimeChart.DataContext = DataGridViewModelCollection.DataGridTimeChartViewModel;
            MainWindow.DataGridTimeChart.TimeSeriesLineChart.DataContext = DataGridViewModelCollection.DataGridTimeChartViewModel;
            MainWindow.DataGridMatchup.DataContext = DataGridViewModelCollection.DataGridMatchupViewModel;
            MainWindow.DataGridDuplicates.DataContext = DataGridViewModelCollection.DataGridDuplicatesViewModel;
            MainWindow.DataGridGameList.DataContext = DataGridViewModelCollection.DataGridGameListViewModel;

            loader.Start();
        }

        internal async void LearnDecks()
        {
            MainWindow.SetLock(true);
            var format = MainWindow.ComboBoxFormat.Text;
            if (!Decks.ContainsKey(format))
            {
                format = "Modern";
            }
            var result = await ThreadingHelper.StartSTATask(() =>
            {
                var viewModel = new DeckListClassificationViewModel(Decks[format]);
                var learnWindow = new DeckListClassificationView(viewModel);
                learnWindow.ShowDialog();
                return true;
            });
            MainWindow.SetLock(false);
        }

        internal async void SaveData()
        {
            MainWindow.SetLock(true);
            var result = await ThreadingHelper.StartSTATask(() =>
            {
                Data.SaveData();
                return true;
            });
            MainWindow.SetLock(false);
        }

        public async void ReclassifyDecks()
        {
            MainWindow.SetLock(true);
            var result = await ThreadingHelper.StartSTATask(() =>
            {
                Data.Matches = NewMatch.CleanMatches(Data.Matches);
                Data.Matches = NewMatch.GetDecks(Data.Matches, new List<NewMatch>(), Decks);
                UiHelper.UpdateProgress(MainWindow.ProgressBar, 0, 1.0);
                return true;
            });
            MainWindow.SetLock(false);
        }

        public async void UploadData()
        {
            MainWindow.SetLock(true);
            var result = await ThreadingHelper.StartSTATask(() =>
            {
                Uploader.StartUpload();
                return true;
            });
            MainWindow.SetLock(false);
        }

        private void LoaderOnOnFinishedLoading(List<NewMatch> loadedMatches, UpdateData data, Dictionary<String, Format> decks)
        {
            Decks = decks;
            Data.LoadData(loadedMatches);
#if DEBUG
            Metagame metagame = new Metagame(Data);
#endif   
            MainWindow.Dispatcher.Invoke(() =>
            {
                DataGridViewModelCollection.DataGridDecksViewModel = new DataGridDecksViewModel(Decks, "Deck", Data);
                DataGridViewModelCollection.DataGridPlayersViewModel = new DataGridDecksViewModel(Decks, "Player", Data);
                DataGridViewModelCollection.DataGridVsDecksViewModel = new DataGridDecksViewModel(Decks, "Deck", Data);
                DataGridViewModelCollection.DataGridVsPlayersViewModel = new DataGridDecksViewModel(Decks, "Player", Data);

                MainWindow.DataGridDecks.DataContext = DataGridViewModelCollection.DataGridDecksViewModel;
                MainWindow.DataGridPlayers.DataContext = DataGridViewModelCollection.DataGridPlayersViewModel;
                MainWindow.DataGridVsDecks.DataContext = DataGridViewModelCollection.DataGridVsDecksViewModel;
                MainWindow.DataGridVsPlayers.DataContext = DataGridViewModelCollection.DataGridVsPlayersViewModel;

                if (CommandLineArgsHelper.HasDeckDefEditor())
                {
                    DataGridViewModelCollection.DataGridDeckDefinitionsViewModel = new DataGridDeckDefinitionsViewModel(Decks);
                    MainWindow.DataGridDefinitions.DataContext = DataGridViewModelCollection.DataGridDeckDefinitionsViewModel;
                }

                if (CommandLineArgsHelper.HasDeckDuplicates())
                { 
                    DataGridViewModelCollection.DataGridDuplicatesViewModel = new DataGridDuplicatesViewModel(Decks, Data);
                    MainWindow.DataGridDuplicates.DataContext = DataGridViewModelCollection.DataGridDuplicatesViewModel;
                }

                foreach (var format in Decks.Keys)
                {
                    MainWindow.ComboBoxFormat.Items.Add(format);
                }
            });            

            UpdateUi(data);
        }

        public void UpdateUi(UpdateData data)
        {
            if (!MainWindow.Dispatcher.CheckAccess())
            {
                MainWindow.Dispatcher.Invoke(new UpdateUiDelegate(UpdateUi), data);
            }
            else
            {
                UpdateDataHelper.UpdateUi(data, DataGridViewModelCollection, MainWindow.ComboBoxPlayer, MainWindow.ComboBoxDeck, MainWindow.ComboBoxOpponent, MainWindow.ComboBoxOpponentsDeck);
                MainWindow.DataGridTimeChart.ChartWinOverTime.InvalidateVisual();
            }
        }

        public async void UpdateAll()
        {
            MainWindow.SetLock(true);

            var player = MainWindow.ComboBoxPlayer.Text;
            var deck = MainWindow.ComboBoxDeck.Text;
            var vsDeck = MainWindow.ComboBoxOpponentsDeck.Text;
            var vsPlayer = MainWindow.ComboBoxOpponent.Text;
            var format = MainWindow.ComboBoxFormat.Text;
            var start = MainWindow.ComboBoxStart.Text;
            var end = MainWindow.ComboBoxEnd.Text;
            var sigma = MainWindow.DataGridTimeChart.TextBoxFilterSize.Text;
            var onThePlay = MainWindow.ComboBoxOnThePlay.Text;
            var postboard = MainWindow.ComboBoxPostboard.Text;
            var filter = DataGridViewModelCollection.DataGridDeckDefinitionsViewModel.DeckDefinitionFilter;            

            var data = await Task.Run(() => UpdateDataHelper.UpdateStats(Data.Matches, Decks, player, deck, vsPlayer, vsDeck, format, start, end, sigma, onThePlay, postboard, filter));
            UpdateUi(data);

            MainWindow.SetLock(false);
        }

        public delegate void UpdateUiDelegate(UpdateData data);

        public async void ScanMtgoFolder()
        {
            MainWindow.SetLock(true);

            var paths = await Task.Run(() => ScanConvertHelper.FindMtgoFolders());
            var filterSize = MainWindow.DataGridTimeChart.TextBoxFilterSize.Text;
            var result = await ThreadingHelper.StartSTATask(() => ScanConvertHelper.ScanConvert(paths, MainWindow.ProgressBar, Data.Matches, Decks, filterSize));

            //var result = await Task.Run(() => ScanConvertHelper.ScanConvert(paths, MainWindow.ProgressBar, AllMatches, Decks));
            Data.Matches = result.Item2;
            UpdateUi(result.Item1);

            MainWindow.SetLock(false);
        }

        public async void ScanFolder(String dialogSelectedPath)
        {
            MainWindow.SetLock(true);

            //var result = await Task.Run(() => ScanConvertHelper.ScanConvert(new[]{dialogSelectedPath}, MainWindow.ProgressBar, AllMatches, Decks));
            var filterSize = MainWindow.DataGridTimeChart.TextBoxFilterSize.Text;
            var result = await ThreadingHelper.StartSTATask(() => ScanConvertHelper.ScanConvert(new[] { dialogSelectedPath }, MainWindow.ProgressBar, Data.Matches, Decks, filterSize));

            Data.Matches = result.Item2;
            UpdateUi(result.Item1);

            MainWindow.SetLock(false);
        }
    }
}