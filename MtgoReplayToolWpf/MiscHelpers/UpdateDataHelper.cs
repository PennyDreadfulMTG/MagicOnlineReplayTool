using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using MtgoReplayToolWpf.DataGridData;
using MtgoReplayToolWpf.DataGridViewModels;
using MtgoReplayToolWpf.MiscHelpers;
using MTGOReplayToolWpf;

namespace MtgoReplayToolWpf
{
    public class UpdateDataHelper
    {
        public static void UpdateUi(UpdateData data, DataGridViewModelCollection viewModelCollection, ComboBox boxPlayer, ComboBox boxDeck, ComboBox boxVsPlayer, ComboBox boxVsDeck)
        {
            // update combobox
            var boxPlayerItem = boxPlayer.Text;
            var boxDeckItem = boxDeck.Text;
            var boxVsPlayerItem = boxVsPlayer.Text;
            var boxVsDeckItem = boxVsDeck.Text;

            boxPlayer.Items.Clear();
            foreach (var kvp in data.PlayerList) boxPlayer.Items.Add(kvp.Key);

            boxDeck.Items.Clear();
            foreach (var kvp in data.DeckList) boxDeck.Items.Add(kvp.Key);

            boxVsPlayer.Items.Clear();
            foreach (var kvp in data.PlayerList) boxVsPlayer.Items.Add(kvp.Key);

            boxVsDeck.Items.Clear();
            foreach (var kvp in data.DeckList) boxVsDeck.Items.Add(kvp.Key);

            boxDeck.Text = boxDeckItem;
            boxPlayer.Text = boxPlayerItem;
            boxVsDeck.Text = boxVsDeckItem;
            boxVsPlayer.Text = boxVsPlayerItem;

            viewModelCollection.DataGridDecksViewModel.SetSource(data.SourceData.DataGridDecksData);
            viewModelCollection.DataGridMatchupViewModel.SetSource(data.SourceData.DataGridMatchupData);
            viewModelCollection.DataGridMullViewModel.SetSource(data.SourceData.DataGridMullData);
            viewModelCollection.DataGridPlayersViewModel.SetSource(data.SourceData.DataGridPlayersData);
            viewModelCollection.DataGridTimeChartViewModel.SetSource(data.SourceData.DataGridTimeChartData);
            viewModelCollection.DataGridTurnsViewModel.SetSource(data.SourceData.DataGridTurnsData);
            viewModelCollection.DataGridVsDecksViewModel.SetSource(data.SourceData.DataGridVsDecksData);
            viewModelCollection.DataGridVsPlayersViewModel.SetSource(data.SourceData.DataGridVsPlayersData);
            viewModelCollection.DataGridGameListViewModel.SetSource(data.SourceData.DataGridGamesData);
            if (CommandLineArgsHelper.HasDeckDefEditor())
            {
                viewModelCollection.DataGridDeckDefinitionsViewModel.SetSource(data.SourceData.DataGridDeckDefinitionsData);
            }
            if (CommandLineArgsHelper.HasDeckDuplicates())
            {
                viewModelCollection.DataGridDuplicatesViewModel.SetSource(data.SourceData.DataGridDuplicatesData);
            }
        }

        public static UpdateData UpdateStats(
            List<NewMatch> allMatches,
            Dictionary<String, Format> decks,
            String boxPlayer,
            String boxDeck,
            String boxVsPlayer,
            String boxVsDeck,
            String boxFormat,
            String startText,
            String endText,
            String sigmaText,
            String boxOnThePlay,
            String boxPostboard,
            String filter)
        {
            return UpdateDataHelper.UpdateStats(allMatches,
                decks,
                boxPlayer,
                boxDeck,
                boxVsPlayer,
                boxVsDeck,
                boxFormat,
                startText,
                endText,
                sigmaText,
                boxOnThePlay,
                boxPostboard,
                null,
                string.Empty,
                filter);
        }


        public static UpdateData UpdateStats(
            List<NewMatch> allMatches,
            Dictionary<String, Format> decks,
            String boxPlayer,
            String boxDeck,
            String boxVsPlayer,
            String boxVsDeck,
            String boxFormat,
            String startText,
            String endText,
            String sigmaText,
            String boxOnThePlay,
            String boxPostboard,
            BackgroundWorker backgroundWorker,
            String progressString,
            String filter)
        {
            var data = new UpdateData();
            data.SourceData = new DataCollection();
            String player = boxPlayer;
            String deck = boxDeck;
            String vsPlayer = boxVsPlayer;
            String vsDeck = boxVsDeck;
            String format = boxFormat;
            Boolean? onThePlay = null;
            Boolean? postboard = null;

            switch (boxOnThePlay)
            {
                case "Yes":
                    onThePlay = true;
                    break;
                case "No":
                    onThePlay = false;
                    break;
            }

            switch (boxPostboard)
            {
                case "Yes":
                    postboard = true;
                    break;
                case "No":
                    postboard = false;
                    break;
            }

            if (!Int32.TryParse(sigmaText, out Int32 sigma))
            {
                sigma = 50;
            }

            if (allMatches == null)
            {
                return data;
            }

            var vsDecks = new Dictionary<String, DecksData>();
            var vsPlayers = new Dictionary<String, DecksData>();
            var asPlayers = new Dictionary<String, DecksData>();
            var asDecks = new Dictionary<String, DecksData>();
            var players = new Dictionary<String, Int32>();
            var deckRecords = new Dictionary<String, Int32>();
            var cards = new Dictionary<String, Int32[]>();

            var history = new List<KeyValuePair<DateTime, Double>>();
            var mull = new Int32[16];
            var turns = new Int32[22];
            var gamesWon = 0;
            var gamesNotWon = 0;
            //var matrix = new Dictionary<Tuple<String, String>, Int32>();
            // get filter
            var start = GetMatchIdFromTime(startText, DateTime.MinValue);
            var end = GetMatchIdFromTime(endText, DateTime.MaxValue);

            var garbage = false;

            var myGames = new List<GameData>();
            var progress = 0;
            progressString += Environment.NewLine + "    Analyzing Matches";
            foreach (var m in allMatches)
            {
                progress++;
                if (m.Date < start) continue;
                if (m.Date > end) continue;

                if (!m.HDeck.StartsWith(format) && !m.VDeck.StartsWith(format)) continue;

                //garbage data
                if (m.Games.Count <= 1)
                {
                    if (garbage) continue;
                    garbage = true;
                }
                else garbage = false;

                if (!players.ContainsKey(m.Hero)) players.Add(m.Hero, 1);
                else players[m.Hero]++;
                if (!players.ContainsKey(m.Villain)) players.Add(m.Villain, 1);
                else players[m.Villain]++;
                if (!deckRecords.ContainsKey(m.HDeck)) deckRecords.Add(m.HDeck, 1);
                else deckRecords[m.HDeck]++;
                if (!deckRecords.ContainsKey(m.VDeck)) deckRecords.Add(m.VDeck, 1);
                else deckRecords[m.VDeck]++;

                // TODO MATRIX
                //if ((m.VDeck != "unknown") && (m.HDeck != "unknown"))
                //{
                //    if (!matrix.ContainsKey(new Tuple<String, String>(m.VDeck, m.HDeck))) matrix.Add(new Tuple<String, String>(m.VDeck, m.HDeck), 1);
                //    else matrix[new Tuple<String, String>(m.VDeck, m.HDeck)]++;
                //}

                var hFilter = 0;
                var vFilter = 0;
                // apply filter
                if (m.Hero == player || player == "") hFilter++;
                if (m.HDeck == deck || deck == "") hFilter++;
                if (m.Villain == vsPlayer || vsPlayer == "") hFilter++;
                if (m.VDeck == vsDeck || vsDeck == "") hFilter++;

                if (m.Hero == vsPlayer || vsPlayer == "") vFilter++;
                if (m.HDeck == vsDeck || vsDeck == "") vFilter++;
                if (m.Villain == player || player == "") vFilter++;
                if (m.VDeck == deck || deck == "") vFilter++;
                
                if (hFilter == 4)
                {
                    AnalyzeMatch(onThePlay, postboard, vsDecks, vsPlayers, asPlayers, asDecks, cards, history, mull, turns, ref gamesWon, ref gamesNotWon, myGames, m, false);
                }
                if (vFilter == 4)
                {
                    AnalyzeMatch(onThePlay, postboard, vsDecks, vsPlayers, asPlayers, asDecks, cards, history, mull, turns, ref gamesWon, ref gamesNotWon, myGames, m, true);
                }

                backgroundWorker?.ReportProgress(0, progressString + $" - {progress} / {allMatches.Count()}");
            }

            progressString += $" - {allMatches.Count()} / {allMatches.Count()}";

            //update combobox
            var myPlayersList = players.ToList();
            var myDecksList = deckRecords.ToList();
            myPlayersList.Sort((pair2, pair1) => pair1.Value.CompareTo(pair2.Value));
            myDecksList.Sort((pair2, pair1) => pair1.Value.CompareTo(pair2.Value));
            data.DeckList = myDecksList;
            data.PlayerList = myPlayersList;

            // print
            // M, MW, MW%, G, GW, GW%, D, DW, DW%, P, PW, PW%, PRE, PREW, PREW%, POST, POSTW, POSTW%

            progressString += Environment.NewLine + "    Preparing data: AsDeck.";
            backgroundWorker?.ReportProgress(0, progressString);
            data.SourceData.DataGridDecksData = GetSourceList(asDecks);

            progressString += Environment.NewLine + "    Preparing data: Cards.";
            backgroundWorker?.ReportProgress(0, progressString);
            data.SourceData.DataGridMatchupData = GetCardsData(cards, gamesWon, gamesNotWon);

            progressString += Environment.NewLine + "    Preparing data: Mulligan.";
            backgroundWorker?.ReportProgress(0, progressString);
            data.SourceData.DataGridMullData = GetMullData(mull, "Mulligan to");

            progressString += Environment.NewLine + "    Preparing data: AsPlayer.";
            backgroundWorker?.ReportProgress(0, progressString);
            data.SourceData.DataGridPlayersData = GetSourceList(asPlayers);

            progressString += Environment.NewLine + "    Preparing data: Timeseries.";
            backgroundWorker?.ReportProgress(0, progressString);
            data.SourceData.DataGridTimeChartData = GetTimeSeriesdata(history, sigma);

            progressString += Environment.NewLine + "    Preparing data: Turns.";
            backgroundWorker?.ReportProgress(0, progressString);
            data.SourceData.DataGridTurnsData = GetMullData(turns, "Game ended on Turn");

            progressString += Environment.NewLine + "    Preparing data: VsDeck.";
            backgroundWorker?.ReportProgress(0, progressString);
            data.SourceData.DataGridVsDecksData = GetSourceList(vsDecks);

            progressString += Environment.NewLine + "    Preparing data: VsPlayer.";
            backgroundWorker?.ReportProgress(0, progressString);
            data.SourceData.DataGridVsPlayersData = GetSourceList(vsPlayers);

            data.SourceData.DataGridGamesData = myGames.Distinct().ToList();


            Dictionary<String, Format> relevantDecks;
            if (String.IsNullOrEmpty(format))
            {
                relevantDecks = decks;
            }
            else
            {
                relevantDecks = new Dictionary<String, Format>();
                relevantDecks.Add(format, decks[format]);
            }

            if (CommandLineArgsHelper.HasDeckDefEditor())
            {
                if (backgroundWorker != null)
                {
                    progressString += Environment.NewLine + "    Preparing data: Deck.";
                    backgroundWorker?.ReportProgress(0, progressString);
                }
                data.SourceData.DataGridDeckDefinitionsData = GetDecksData(relevantDecks, filter);
            }

            if (CommandLineArgsHelper.HasDeckDuplicates())
            {
                if (backgroundWorker != null)
                {
                    progressString += Environment.NewLine + "    Preparing data: Duplicates.";
                    backgroundWorker?.ReportProgress(0, progressString);
                }
                data.SourceData.DataGridDuplicatesData = GetDuplicatesData(relevantDecks);
            }

            #region matrix
            //i = 0;
            //dataGridView4.RowCount = myDecksList.Count;
            //dataGridView4.ColumnCount = myDecksList.Count;
            //foreach (var kvp in myDecksList)
            //{
            //    var j = 0;
            //    if (kvp.Key == "unknown") continue;
            //    dataGridView4.Rows[i].Cells[0].Value = kvp.Key;
            //    dataGridView4.Columns[i + 1].Name = kvp.Key;
            //    foreach (var kvp2 in myDecksList)
            //    {
            //        if (kvp2.Key == "unknown") continue;
            //        j++;
            //        var a = 0;
            //        var b = 0;
            //        if (matrix.ContainsKey(new Tuple<String, String>(kvp.Key, kvp2.Key))) a = matrix[new Tuple<String, String>(kvp.Key, kvp2.Key)];
            //        if (matrix.ContainsKey(new Tuple<String, String>(kvp2.Key, kvp.Key))) b = matrix[new Tuple<String, String>(kvp2.Key, kvp.Key)];
            //        dataGridView4.Rows[i].Cells[j].Value = a + b;
            //    }
            //    i++;
            //}
            #endregion

            //#region game list
            //if (myMatches.Count == 0)
            //{
            //    label1.Text = "No Data! Maybe wrong filters?";
            //    return;
            //}
            //dataGridView8.RowCount = 1 + myMatches.Count() * 3;
            //dataGridView8.ColumnCount = 12;
            //dataGridView8.Columns[0].Name = "Match ID";
            //dataGridView8.Columns[1].Name = "Game ID";
            //dataGridView8.Columns[2].Name = "hero";
            //dataGridView8.Columns[3].Name = "hDeck";
            //dataGridView8.Columns[4].Name = "villain";
            //dataGridView8.Columns[5].Name = "vDeck";
            //dataGridView8.Columns[6].Name = "Result";
            //dataGridView8.Columns[7].Name = "preboard";
            //dataGridView8.Columns[8].Name = "onDraw";
            //dataGridView8.Columns[9].Name = "hMull";
            //dataGridView8.Columns[10].Name = "vMull";
            //dataGridView8.Columns[11].Name = "Last Turn";

            //for (var k = 0; k < 12; k++) dataGridView8.Columns[k].Width = 75;
            //dataGridView8.Columns[3].Width = 175;
            //dataGridView8.Columns[5].Width = 175;
            //dataGridView8.Columns[2].Width = 100;
            //dataGridView8.Columns[4].Width = 100;


            //i = 1;
            //myMatches = myMatches.Distinct().ToList();
            //foreach (var m in myMatches)
            //{
            //    foreach (var g in m.Games)
            //    {
            //        dataGridView8.Rows[i].Cells[0].Value = m.Id;
            //        dataGridView8.Rows[i].Cells[1].Value = g.Id;
            //        dataGridView8.Rows[i].Cells[2].Value = m.Hero;
            //        dataGridView8.Rows[i].Cells[3].Value = m.HDeck;
            //        dataGridView8.Rows[i].Cells[4].Value = m.Villain;
            //        dataGridView8.Rows[i].Cells[5].Value = m.VDeck;
            //        dataGridView8.Rows[i].Cells[6].Value = g.Result;
            //        dataGridView8.Rows[i].Cells[7].Value = g.PreBoard;
            //        dataGridView8.Rows[i].Cells[8].Value = g.OnDraw;
            //        dataGridView8.Rows[i].Cells[9].Value = g.HMull;
            //        dataGridView8.Rows[i].Cells[10].Value = g.VMull;
            //        dataGridView8.Rows[i].Cells[11].Value = g.Turn;
            //        i++;
            //    }
            //}

            //DataGridViewButtonColumn bnCol = new DataGridViewButtonColumn();
            //bnCol.Text = "View Game";
            //bnCol.UseColumnTextForButtonValue = true;

            //dataGridView8.Columns.Add(bnCol);


            //#endregion

            return data;
        }

        private static void AnalyzeMatch(
            Boolean? onThePlay,
            Boolean? postboard,
            Dictionary<String, DecksData> vsDecks,
            Dictionary<String, DecksData> vsPlayers,
            Dictionary<String, DecksData> asPlayers,
            Dictionary<String, DecksData> asDecks,
            Dictionary<String, Int32[]> cards,
            List<KeyValuePair<DateTime, Double>> history,
            Int32[] mull,
            Int32[] turns,
            ref Int32 gamesWon,
            ref Int32 gamesNotWon,
            List<GameData> myGames,
            NewMatch m,
            Boolean invert)
        {
            var hero = m.Hero;
            var villain = m.Villain;
            var hDeck = m.HDeck;
            var vDeck = m.VDeck;
            var matchResult = m.Result;

            if (invert)
            {
                hero = m.Villain;
                villain = m.Hero;
                hDeck = m.VDeck;
                vDeck = m.HDeck;
                matchResult = -1 * m.Result;
            }

            if (!vsDecks.ContainsKey(vDeck)) vsDecks.Add(vDeck, new DecksData("Deck", vDeck));
            if (!vsPlayers.ContainsKey(villain)) vsPlayers.Add(villain, new DecksData("Player", villain));
            if (!asDecks.ContainsKey(hDeck)) asDecks.Add(hDeck, new DecksData("Deck", hDeck));
            if (!asPlayers.ContainsKey(hero)) asPlayers.Add(hero, new DecksData("Player", hero));

            vsDecks[vDeck].Matches++;
            vsPlayers[villain].Matches++;
            asDecks[hDeck].Matches++;
            asPlayers[hero].Matches++;

            if (matchResult > 0)
            {
                vsDecks[vDeck].MatchWon++;
                vsPlayers[villain].MatchWon++;
                asDecks[hDeck].MatchWon++;
                asPlayers[hero].MatchWon++;

                history.Add(new KeyValuePair<DateTime, Double>(m.Date, 1));
            }
            else
            {
                history.Add(new KeyValuePair<DateTime, Double>(m.Date, 0));
            }
            foreach (var g in m.Games)
            {
                var onDraw = g.OnDraw;
                var hMull = g.HMull;
                var vMull = g.VMull;
                var hCardsPlayed = g.HCardsPlayed;
                var hCardsPut = g.HCardsPut;
                var gameResult = g.Result;

                if (invert)
                {
                    onDraw = !g.OnDraw;
                    hMull = g.VMull;
                    vMull = g.HMull;
                    hCardsPlayed = g.VCardsPlayed;
                    hCardsPut = g.VCardsPut;
                    gameResult = -1 * g.Result;
                }

                if (postboard.HasValue && postboard == g.PreBoard)
                {
                    continue;
                }

                if (onThePlay.HasValue && onThePlay != onDraw)
                {
                    continue;
                }

                myGames.Add(new GameData(g, m));

                // mull
                mull[hMull]++;
                if (g.Turn > 10) turns[10]++;
                else turns[g.Turn]++;

                vsDecks[vDeck].Games++;
                vsPlayers[villain].Games++;
                asDecks[hDeck].Games++;
                asPlayers[hero].Games++;

                if (gameResult > 0)
                {
                    gamesWon++;
                    vsDecks[vDeck].GamesWon++;
                    vsPlayers[villain].GamesWon++;
                    asDecks[hDeck].GamesWon++;
                    asPlayers[hero].GamesWon++;

                    mull[8 + hMull]++;
                    if (g.Turn > 10) turns[21]++;
                    else turns[g.Turn + 11]++;
                }
                else
                {
                    gamesNotWon++;
                }

                if (!onDraw)
                {
                    vsDecks[vDeck].GamesDraw++;
                    vsPlayers[villain].GamesDraw++;
                    asDecks[hDeck].GamesDraw++;
                    asPlayers[hero].GamesDraw++;

                    if (gameResult > 0)
                    {
                        vsDecks[vDeck].GamesWonDraw++;
                        vsPlayers[villain].GamesWonDraw++;
                        asDecks[hDeck].GamesWonDraw++;
                        asPlayers[hero].GamesWonDraw++;
                    }
                }
                else
                {
                    vsDecks[vDeck].GamesPlay++;
                    vsPlayers[villain].GamesPlay++;
                    asDecks[hDeck].GamesPlay++;
                    asPlayers[hero].GamesPlay++;

                    if (gameResult > 0)
                    {
                        vsDecks[vDeck].GamesWonPlay++;
                        vsPlayers[villain].GamesWonPlay++;
                        asDecks[hDeck].GamesWonPlay++;
                        asPlayers[hero].GamesWonPlay++;
                    }
                }

                if (g.PreBoard)
                {
                    vsDecks[vDeck].GamesPre++;
                    vsPlayers[villain].GamesPre++;
                    asDecks[hDeck].GamesPre++;
                    asPlayers[hero].GamesPre++;

                    if (gameResult > 0)
                    {
                        vsDecks[vDeck].GamesWonPre++;
                        vsPlayers[villain].GamesWonPre++;
                        asDecks[hDeck].GamesWonPre++;
                        asPlayers[hero].GamesWonPre++;
                    }
                }
                else
                {
                    vsDecks[vDeck].GamesPost++;
                    vsPlayers[villain].GamesPost++;
                    asDecks[hDeck].GamesPost++;
                    asPlayers[hero].GamesPost++;

                    if (gameResult > 0)
                    {
                        vsDecks[vDeck].GamesWonPost++;
                        vsPlayers[villain].GamesWonPost++;
                        asDecks[hDeck].GamesWonPost++;
                        asPlayers[hero].GamesWonPost++;
                    }
                }

                foreach (var card in hCardsPlayed.Distinct())
                {
                    if (!cards.ContainsKey(card)) cards.Add(card, new Int32[2] { 1, 0 });
                    else cards[card][0]++;
                    if (gameResult > 0) cards[card][1]++;
                }

                foreach (var card in hCardsPut.Distinct())
                {
                    var cardString = "put [" + card + "] into play";
                    if (!cards.ContainsKey(cardString)) cards.Add(cardString, new Int32[2] { 1, 0 });
                    else cards[cardString][0]++;
                    if (gameResult > 0) cards[cardString][1]++;
                }
            }
        }

        private static List<DuplicateDecksData> GetDuplicatesData(Dictionary<String, Format> decks)
        {
            var returnValue = new List<DuplicateDecksData>();
            var decksByName = GetDecksByName(decks);
            var aggregateDecksByName = new List<Deck>();

            foreach (var deck in decksByName)
            {
                var firstDeck = deck.Value.First();
                var aggregateDeck = new Deck(firstDeck.Name, new Dictionary<String, Double>(firstDeck.Definition), 1, firstDeck.Date, firstDeck.Filepath, firstDeck.Format, firstDeck.ManuallyEdited);
                deck.Value.ForEach((x) =>
                    {
                        aggregateDeck.AddDeckList(x);
                    }
                );

                aggregateDeck.Name = deck.Key;

                aggregateDecksByName.Add(aggregateDeck);
            }

            foreach (var deck in aggregateDecksByName)
            {
                foreach (var otherDeck in aggregateDecksByName)
                {
                    if (deck.Name.CompareTo(otherDeck.Name) >= 0)
                    {
                        continue;
                    }

                    if (!otherDeck.Name.StartsWith(deck.Name.Substring(0, 3)))
                    {
                        continue;
                    }

                    var dice = deck.GetDice(otherDeck);

                    if (dice > 0.5)
                    {
                        returnValue.Add(new DuplicateDecksData(deck.Name, otherDeck.Name, dice));
                    }
                }
            }

            return returnValue;
        }

        private static List<TimeSeriesData> GetTimeSeriesdata(List<KeyValuePair<DateTime, Double>> history, Int32 sigma)
        {
            var accuracy = 6;

            List<KeyValuePair<DateTime, Double>> copyHistory = new List<KeyValuePair<DateTime, Double>>(history);
            copyHistory.ForEach(x =>
            {
                if (x.Key.CompareTo(new DateTime(2014, 1, 1)) < 0)
                {
                    history.Remove(x);
                }
            });

            if (history.Count > 500)
            {
                Int32 factor = Convert.ToInt32(Math.Ceiling(history.Count / 500.0));
                Int32 newSize = Convert.ToInt32(Math.Floor((Double)history.Count / factor));
                sigma = sigma / factor;
                copyHistory = new List<KeyValuePair<DateTime, Double>>(history);
                history = new List<KeyValuePair<DateTime, Double>>();

                for (int i = 0; i < newSize; i++)
                {
                    var sum = 0.0;
                    for (int j = 0; j < factor; j++)
                    {
                        sum += copyHistory.ElementAt(i * factor + j).Value;
                    }
                    var newDateTicks = (copyHistory.ElementAt(i * factor).Key.Ticks + copyHistory.ElementAt((i + 1) * factor - 1).Key.Ticks) / 2;
                    var newDate = new DateTime(newDateTicks);
                    history.Add(new KeyValuePair<DateTime, Double>(newDate, sum / factor));
                }
            }

            if (history.Count > 0)
            {
                if (sigma < 1)
                {
                    sigma = 1;
                }
                else if (sigma > history.Count)
                {
                    sigma = history.Count;
                }

                var filterSize = sigma * accuracy;
                var halfFilterSize = (Int32)(filterSize * 0.5);

                var winRate = new List<KeyValuePair<DateTime, Double>>();
                var filter = new Double[filterSize];

                for (int i = 0; i < filter.Length; i++)
                {
                    filter[i] = Math.Exp(-1 * Math.Pow(i - halfFilterSize, 2) / (2 * Math.Pow(sigma, 2))) / (Math.Sqrt(2 * Math.PI) * sigma);
                }

                for (int i = 0; i < history.Count; i++)
                {
                    var filterSum = 0.0;
                    var normalSum = 0.0;

                    for (int j = 0; j < filter.Length; j++)
                    {
                        var x = i + j - halfFilterSize;
                        if (x >= 0 && x < history.Count)
                        {
                            normalSum += 1.0 * filter[j];
                            filterSum += history.ElementAt(x).Value * filter[j];
                        }
                    }

                    winRate.Add(new KeyValuePair<DateTime, Double>(history.ElementAt(i).Key/*.ToShortDateString()*/ /*String.Empty*/, filterSum / normalSum));
                }

                return new List<TimeSeriesData> { new TimeSeriesData(winRate) };
            }

            return new List<TimeSeriesData>();
        }

        private static List<DecksData> GetSourceList(Dictionary<String, DecksData> data)
        {
            List<DecksData> returnList = new List<DecksData>();
            var total = new DecksData();
            foreach (var s in data.Keys)
            {
                total.Header = data[s].Header;
                total.Names = "Total";
                total.Matches += data[s].Matches;
                total.MatchWon += data[s].MatchWon;
                total.Games += data[s].Games;
                total.GamesWon += data[s].GamesWon;
                total.GamesPre += data[s].GamesPre;
                total.GamesWonPre += data[s].GamesWonPre;
                total.GamesPost += data[s].GamesPost;
                total.GamesWonPost += data[s].GamesWonPost;
                total.GamesDraw += data[s].GamesDraw;
                total.GamesWonDraw += data[s].GamesWonDraw;
                total.GamesPlay += data[s].GamesPlay;
                total.GamesWonPlay += data[s].GamesWonPlay;

                returnList.Add(data[s]);
            }

            returnList.Add(total);
            return returnList;
        }

        private static List<MullData> GetMullData(Int32[] stats, String header)
        {
            var sourceList = new List<Double[]>();
            var dataList = new List<MullData>();
            var total = 0.0;
            var size = stats.Length / 2;

            for (var j = 0; j < size; j++)
            {
                sourceList.Add(new Double[13]);
                total += stats[size - j - 1];
                sourceList.ElementAt(j)[0] = size - j - 1;
                sourceList.ElementAt(j)[7] = stats[size - j - 1];
                sourceList.ElementAt(j)[8] = stats[2 * size - 1 - j];
            }

            for (var j = 0; j < size; j++)
            {
                if (j == 0)
                {
                    sourceList.ElementAt(0)[9] = sourceList.ElementAt(0)[7];
                    sourceList.ElementAt(0)[10] = sourceList.ElementAt(0)[8];
                    sourceList.ElementAt(size - 1)[11] = sourceList.ElementAt(size - 1)[7];
                    sourceList.ElementAt(size - 1)[12] = sourceList.ElementAt(size - 1)[8];
                }
                else
                {
                    sourceList.ElementAt(j)[9] = sourceList.ElementAt(j - 1)[9] + sourceList.ElementAt(j)[7];
                    sourceList.ElementAt(j)[10] = sourceList.ElementAt(j - 1)[10] + sourceList.ElementAt(j)[8];
                    sourceList.ElementAt(size - j - 1)[11] = sourceList.ElementAt(size - j)[11] + sourceList.ElementAt(size - j - 1)[7];
                    sourceList.ElementAt(size - j - 1)[12] = sourceList.ElementAt(size - j)[12] + sourceList.ElementAt(size - j - 1)[8];
                }

                sourceList.ElementAt(j)[1] = 100.0 * sourceList.ElementAt(j)[7] / total;
                sourceList.ElementAt(j)[2] = 100.0 * sourceList.ElementAt(j)[8] / sourceList.ElementAt(j)[7];
                sourceList.ElementAt(j)[3] = 100.0 * sourceList.ElementAt(j)[9] / total;
                sourceList.ElementAt(j)[4] = 100.0 * sourceList.ElementAt(j)[10] / sourceList.ElementAt(j)[9];
                sourceList.ElementAt(size - j - 1)[5] = 100.0 * sourceList.ElementAt(size - j - 1)[11] / total;
                sourceList.ElementAt(size - j - 1)[6] = 100.0 * sourceList.ElementAt(size - j - 1)[12] / sourceList.ElementAt(size - j - 1)[11];
            }

            sourceList.ForEach(x => dataList.Add(new MullData("Mulligan to", x)));

            return dataList;
        }

        private static List<CardsData> GetCardsData(Dictionary<String, Int32[]> stats, Int32 gamesWon, Int32 gamesNotWon)
        {
            var resultList = new List<CardsData>();
            var keys = stats.Keys.ToList();

            foreach (var key in keys)
            {
                var meanCast = (Double)stats[key][0] / (gamesWon + gamesNotWon);
                var meanWin = (Double)gamesWon / (gamesWon + gamesNotWon);
                var castMinusMean = 1.0 - meanCast;
                var notCastMinusMean = 0.0 - meanCast;
                var wonMinusMean = 1.0 - meanWin;
                var notWonMinusMean = 0.0 - meanWin;
                var timesCastWon = stats[key][1];
                var timesCastNotWon = stats[key][0] - stats[key][1];
                var timesNotCastWon = gamesWon - timesCastWon;
                var timesNotCastNotWon = gamesNotWon - timesCastNotWon;

                var numerator = castMinusMean * wonMinusMean * timesCastWon;
                numerator += castMinusMean * notWonMinusMean * timesCastNotWon;
                numerator += notCastMinusMean * wonMinusMean * timesNotCastWon;
                numerator += notCastMinusMean * notWonMinusMean * timesNotCastNotWon;

                var denominatorCast = castMinusMean * castMinusMean * stats[key][0];
                denominatorCast += notCastMinusMean * notCastMinusMean * (timesNotCastNotWon + timesNotCastWon);

                var denominatorWon = wonMinusMean * wonMinusMean * gamesWon;
                denominatorWon += notWonMinusMean * notWonMinusMean * gamesNotWon;

                var correlation = numerator / (Math.Sqrt(denominatorCast) * Math.Sqrt(denominatorWon));

                resultList.Add(new CardsData(key, stats[key], correlation));
            }

            if (resultList.Count > 100)
            {
                resultList = resultList.OrderByDescending(x => x.Games).ToList();
                resultList = resultList.GetRange(0, 100);
            }

            return resultList;
        }

        private static Dictionary<String, List<Deck>> GetDecksByName(Dictionary<String, Format> decks)
        {
            var decksByName = new Dictionary<String, List<Deck>>();

            foreach (var format in decks)
            {
                foreach (var deck in format.Value.Decks)
                {
                    if (decksByName.ContainsKey(format.Key + '_' + deck.Name))
                    {
                        decksByName[format.Key + '_' + deck.Name].Add(deck);
                    }
                    else
                    {
                        decksByName.Add(format.Key + '_' + deck.Name, new List<Deck> { deck });
                    }
                }
            }

            return decksByName;
        }

        private static List<DeckDefinitionData> GetDecksData(Dictionary<String, Format> decks, String filter)
        {
            var filteredDecks = ApplyDeckFilter(decks, filter);
            var decksByName = GetDecksByName(filteredDecks);
            var resultList = new List<DeckDefinitionData>();

            foreach (var listOfDecksWithSameName in decksByName)
            {
                var decklistData = new List<DeckListData>();
                var outerHomogeneity = 0.0;

                foreach (var decklist in listOfDecksWithSameName.Value)
                {
                    var homogeneity = 0.0;

                    foreach (var otherDecklist in listOfDecksWithSameName.Value)
                    {
                        homogeneity += decklist.GetDice(otherDecklist);
                    }

                    homogeneity *= (1.0 / listOfDecksWithSameName.Value.Count);
                    outerHomogeneity += homogeneity;

                    var deckListStrings = new List<String>();

                    foreach (var line in decklist.Definition)
                    {
                        deckListStrings.Add(line.Value.ToString() + " " + line.Key);
                    }

                    decklistData.Add(new DeckListData(decklist.Filepath, decklist.Date, homogeneity, deckListStrings));
                }

                outerHomogeneity *= (1.0 / listOfDecksWithSameName.Value.Count);
                resultList.Add(new DeckDefinitionData(listOfDecksWithSameName.Key, listOfDecksWithSameName.Value.Count, outerHomogeneity, decklistData));
            }

            return resultList;
        }

        private static Dictionary<String, Format> ApplyDeckFilter(Dictionary<String, Format> decks, String filter)
        {
            if (filter == null)
            {
                filter = String.Empty;
            }

            var returnValue = new Dictionary<String, Format>();
            
            var stringList = filter.Split(new String[] { ";", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            var filteredCardsList = new List<Tuple<Int32, String>>();
            var excludedCardsList = new List<String>();
            var onlyManuallyEdited = false;
            var onlyNotManuallyEdited = false;

            foreach (var filterEntry in stringList)
            {
                if (filterEntry.StartsWith("!"))
                {
                    excludedCardsList.Add(filterEntry.Substring(1));
                }
                else if (filterEntry.StartsWith("*"))
                {
                    if (filterEntry.Substring(1).StartsWith("yes"))
                    {
                        onlyManuallyEdited = true;
                    }
                    else if (filterEntry.Substring(1).StartsWith("no"))
                    {
                        onlyNotManuallyEdited = true;
                    }
                }
                else
                {
                    var numberAndCard = filterEntry.Split(new Char[]{' '}, 2);
                    if (Int32.TryParse(numberAndCard[0], out var number))
                    {
                        filteredCardsList.Add(new Tuple<Int32, String>(number, numberAndCard[1]));
                    }
                    else
                    {
                        filteredCardsList.Add(new Tuple<Int32, String>(1, filterEntry));
                    }
                }
            }

            foreach (var formatName in decks.Keys)
            {
                var format = new Format(formatName);
                returnValue.Add(formatName, format);
                foreach (var deck in decks[formatName].Decks)
                {
                    if (excludedCardsList.Any(x => deck.Definition.Keys.Contains(x)))
                    {
                        continue;
                    }

                    if (onlyManuallyEdited && !deck.ManuallyEdited)
                    {
                        continue;
                    }

                    if (onlyNotManuallyEdited && deck.ManuallyEdited)
                    {
                        continue;
                    }

                    if (filteredCardsList.All(x => deck.Definition.Keys.Contains(x.Item2) && (deck.Definition[x.Item2] >= x.Item1)))
                    {
                        format.Decks.Add(deck);
                    }
                }
            }

            return returnValue;
        }

        private static DateTime GetMatchIdFromTime(String time, DateTime defaultValue)
        {
            return DateTime.TryParseExact(time, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime value) ? value : defaultValue;
        }
    }
}
