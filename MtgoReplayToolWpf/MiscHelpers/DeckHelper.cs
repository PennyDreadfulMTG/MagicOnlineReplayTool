using MathNet.Numerics.Distributions;
using MtgoReplayToolWpf.MiscHelpers;
using MTGOReplayToolWpf;
using Newtonsoft.Json;
using Sentry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace MtgoReplayToolWpf
{
    public static class DeckHelper
    {
        public static readonly string DeckPath = AppDomain.CurrentDomain.BaseDirectory + "decks";

        public static Dictionary<string, Format> CrawlDecks(BackgroundWorker backgroundWorker, ref string progressString)
        {
            var returnValue = new Dictionary<string, Format>();
            var folders = Directory.GetDirectories(DeckPath);

            foreach (var format in folders)
            {
                var formatName = Path.GetFileName(format);
                if (formatName == "hidden") continue;
                var files = Directory.GetFiles(format, "*.dec");
                returnValue.Add(formatName, new Format(formatName));
                progressString += Environment.NewLine + $"    Loading format: {formatName}";
                var progress = 0;
                var copyOfProgressString = progressString;
                var decks = files.AsParallel().Select(file => {
                    var deck = GetDeckFromFile(file);
                    lock (backgroundWorker)
                    {
                        progress++;
                        backgroundWorker.ReportProgress(0, copyOfProgressString + $" - {progress} / {files.Count()}");
                    }
                    return deck;
                });

                decks = decks.Where(x => x.Definition.Keys.Count > 0);

                returnValue[formatName].Decks.AddRange(decks);

                progressString += $" - {files.Count()} / {files.Count()}";
                backgroundWorker.ReportProgress(0, progressString);
            }

            return returnValue;
        }

        public static void UpdateZips(BackgroundWorker backgroundWorker, ref string progressString)
        {
            var assets = GithubHelper.FetchReleaseAssets("PennyDreadfulMTG", "MORT-Decks");
            var hashes = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(assets.First(a => a.name == "hashes.json").GetContents());

            var zips = Directory.GetFiles(DeckPath, "*.zip");
            var progress = 0;
            foreach (var zip in zips)
            {
                string zipName = Path.GetFileName(zip);
                if (!hashes.ContainsKey(zipName))
                    continue;

                string localHash;

                using (FileStream fs = new FileStream(zip, FileMode.Open))
                using (BufferedStream bs = new BufferedStream(fs))
                {
                    using (SHA1Managed sha1 = new SHA1Managed())
                    {
                        byte[] hash = sha1.ComputeHash(bs);
                        StringBuilder formatted = new StringBuilder(2 * hash.Length);
                        foreach (byte b in hash)
                        {
                            formatted.AppendFormat("{0:X2}", b);
                        }
                        localHash = formatted.ToString();
                    }
                }
                progress++;
                if (!hashes[zipName]["sha1"].Equals(localHash, StringComparison.InvariantCultureIgnoreCase))
                {
                    progressString += Environment.NewLine + $"    Updating format: {zipName}";
                    string copyOfProgressString = progressString;
                    backgroundWorker.ReportProgress(progress, copyOfProgressString);
                    assets.First(a => a.name == zipName).Download(DeckPath);
                    string dirName = Path.Combine(DeckPath, Path.GetFileNameWithoutExtension(zip));
                    using (var archive = ZipFile.OpenRead(zip))
                    {
                        foreach (var deck in archive.Entries)
                        {
                            deck.ExtractToFile(Path.Combine(dirName, deck.Name), true);
                        }

                    }

                }
                
            }
        }

        private static void MergeDecksInternal(List<String> decksToMerge, Dictionary<string, Format> decks, List<NewMatch> matches, String separator)
        {
            var newFormatName = string.Empty;
            var newDeckName = string.Empty;
            var largestDeckSize = 0;
            var listOfAllDecks = new List<Deck>();

            foreach (var deck in decksToMerge)
            {
                if (deck.Equals("Unknown"))
                {
                    continue;
                }

                var index = deck.IndexOf(separator);
                var deckName = deck.Substring(index + separator.Length, deck.Length - index - separator.Length);
                var formatName = deck.Substring(0, index);

                var format = decks[formatName];
                var listOfDecks = format.Decks.Where(x => x.Name.Equals(deckName));

                if (listOfDecks.Count() > largestDeckSize)
                {
                    newFormatName = formatName;
                    newDeckName = deckName;
                    largestDeckSize = listOfDecks.Count();
                }

                listOfAllDecks.AddRange(listOfDecks);
            }

            foreach (var deck in listOfAllDecks)
            {
                deck.Name = newDeckName;
                WriteDeckToFile(deck.Filepath, newFormatName, deck.Name, deck.Date, deck.Definition.Select(kvp => Convert.ToInt32(kvp.Value).ToString() + ' ' + kvp.Key).ToList(), deck.ManuallyEdited);
            }

            var matchesToReclassify = matches.Where(x => decksToMerge.Contains(x.HDeck) || decksToMerge.Contains(x.VDeck));
            matchesToReclassify = NewMatch.GetDecks(matchesToReclassify.ToList(), new List<NewMatch>(), decks);
        }

        internal async static void MergeDecks(List<String> decksToMerge, Dictionary<String, Format> decks, List<NewMatch> matches, String separator)
        {
            UiHelper.LockUi();
            var result = await ThreadingHelper.StartSTATask(() =>
            {
                MergeDecksInternal(decksToMerge, decks, matches, separator);
                return true;
            });
            UiHelper.UnlockUi();
        }

        
        public static List<Tuple<Double, String, String>> CompareDecks(Dictionary<String, Format> formats, List<String> seenCards, DateTime ownDate)
        {
            var resultDecks = new List<Tuple<Double, String, String>>();

            foreach (var format in formats.ToList())
            {
                foreach (var deck in format.Value.Decks)
                {
                    var counter = 0.0;

                    foreach (var card in seenCards)
                    {
                        if (deck.Definition.TryGetValue(card, out Double value))
                        {
                            if (value > 4)
                            {
                                value = 4;
                            }

                            counter += value;
                        }
                    }

                    var timeDifference = ownDate.Subtract(deck.Date);
                    if (timeDifference.TotalDays > 365)
                    {
                        counter *= 1 / (1 + (Math.Abs(timeDifference.TotalDays) - 365) / 365);
                    }

                    resultDecks.Add(new Tuple<Double, String, String>(counter, format.Key, deck.Name));
                }
            }

            resultDecks = resultDecks.OrderByDescending(x => x.Item1).ToList();

            if (resultDecks[0].Item1 <= 9 || resultDecks[0].Item1 <= seenCards.Count * 1.5)
            {
                resultDecks.Clear();
            }

            return resultDecks;
        }

        public static Deck GetDeckFromFile(String path)
        {
            var line = String.Empty;
            var definition = new Dictionary<String, Double>();
            var date = new DateTime();
            var format = String.Empty;
            var deckName = String.Empty;
            var manuallyEdited = false;

            try
            {
                using (var sr = new StreamReader(path))
                {
                    // Date
                    if (!sr.EndOfStream)
                    {
                        line = sr.ReadLine();
                        date = DateTime.ParseExact(line, "s", CultureInfo.InvariantCulture);
                    }

                    // Format
                    if (!sr.EndOfStream)
                    {
                        format = sr.ReadLine();
                    }

                    // Deckname
                    if (!sr.EndOfStream)
                    {
                        deckName = sr.ReadLine();
                        if (deckName.StartsWith("***"))
                        {
                            manuallyEdited = true;
                            deckName = deckName.Remove(0, 3);
                        }
                    }

                    while (!sr.EndOfStream)
                    {
                        line = sr.ReadLine();
                        if (line.Equals("")) continue;
                        var index = line.IndexOf(" ");
                        if (index > 0)
                        {
                            var key = line.Substring(index + 1, line.Length - index - 1);
                            if (key == "Sideboard") 
                                continue;
                            var value = Convert.ToDouble(line.Substring(0, index));
                            if (definition.ContainsKey(key))
                            {
                                definition[key] += value;
                            }
                            else
                            {

                                definition.Add(key, value);
                            }
                        }                        
                    }
                }
            }
            catch (Exception e)
            {
                SentrySdk.CaptureException(e);
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }

            return new Deck(deckName, definition, 1, date, path, format, manuallyEdited);
        }

        internal static void WriteDeckToFile(String filepath, String format, String deckName, DateTime date, Dictionary<String, Double> definition, Boolean manuallyEdited)
        {
            WriteDeckToFile(filepath, format, deckName, date, definition.Select(kvp => Convert.ToInt32(kvp.Value).ToString() + ' ' + kvp.Key).ToList(), manuallyEdited);
        }

        internal static void WriteDeckToFile(String path, String format, String deck, DateTime date, List<String> cards, Boolean manuallyEdited)
        {
            if (manuallyEdited)
            {
                deck = "***" + deck;
            }

            File.WriteAllText(path,
                date.ToString("s", CultureInfo.InvariantCulture) + Environment.NewLine
                + format + Environment.NewLine
                + deck + Environment.NewLine
                + cards.Aggregate((agg, next) => agg + Environment.NewLine + next));
        }
    }
}
