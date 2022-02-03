using MTGOReplayToolWpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace MtgoReplayToolWpf
{
    public class DataLoader
    {
        public DataLoader(MainWindow mainWindow)
        {
            MainWindow = mainWindow;

            LoadWindow = new LoadWindow
            {
                Width = 500,
                Height = 80,
                Title = "MORT is loading. Please wait.",
                Name = "LoadWindow",
                WindowStyle = WindowStyle.SingleBorderWindow
            };
            LoadWindow.Show();

            LoadWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };

            LoadWorker.DoWork += loadWorker_DoWork;
            LoadWorker.ProgressChanged += loadWorker_ProgressChanged;
            LoadWorker.RunWorkerCompleted += loadWorker_RunWorkerCompleted;
        }

        internal void Start()
        {
            LoadWorker.RunWorkerAsync();
        }

        private MainWindow MainWindow { get; set; }
        
        private LoadWindow LoadWindow { get; set; }

        private List<NewMatch> Matches { get; set; }

        private Dictionary<String, Format> Decks { get; set; }

        private BackgroundWorker LoadWorker { get; set; }

        public delegate void FinishedLoadingHandler(List<NewMatch> loadedMatches, UpdateData data, Dictionary<String, Format> decks);

        public event FinishedLoadingHandler OnFinishedLoading;

        private void loadWorker_ProgressChanged(Object sender, ProgressChangedEventArgs e)
        {
            try
            {
                LoadWindow.Label.Content = "MORT is loading. Please wait. " + Environment.NewLine + e.UserState.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
            }
            
        }

        private async void loadWorker_RunWorkerCompleted(Object sender, RunWorkerCompletedEventArgs e)
        {
            await Task.Run(() => OnFinishedLoading(Matches, Data, Decks));
            MainWindow.Show();
            MainWindow.IsEnabled = true;
            MainWindow.Visibility = Visibility.Visible;
            LoadWindow.Close();
        }

        private void loadWorker_DoWork(Object sender, DoWorkEventArgs e)
        {
            try
            {
                var progressString = "Loading Decks... ";
                Decks = DeckHelper.CrawlDecks(LoadWorker, ref progressString);

                progressString += Environment.NewLine + "Loading Data... ";
                LoadBinaries(LoadWorker, ref progressString);

                progressString += Environment.NewLine + "Clean Matches... ";
                LoadWorker.ReportProgress(0, progressString);
                Matches = NewMatch.CleanMatches(Matches);

                //LoadWorker.ReportProgress(0, "Identify Decks... ");
                ////Matches = NewMatch.GetDecks(Matches, Decks);

                progressString += Environment.NewLine + "Update Stats... ";
                LoadWorker.ReportProgress(0, progressString);
                Data = UpdateDataHelper.UpdateStats(Matches, Decks, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty, LoadWorker, progressString, "4 Pierakor ;)");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        public UpdateData Data { get; set; }

        private void LoadBinaries(BackgroundWorker backgroundWorker, ref String progressString)
        {
            //read all .bins
            progressString += Environment.NewLine + "    Loading old binaries.";
            backgroundWorker.ReportProgress(0, progressString);
            Matches = new List<NewMatch>();
            var binPaths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.bin", SearchOption.TopDirectoryOnly).ToList();
            var paths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.gz", SearchOption.TopDirectoryOnly).ToList();
            var obsolete = new List<String>();
            foreach (var s in binPaths)
            {
                Stream stream = File.Open(s, FileMode.Open);
                try
                {
                    //deseralize
                    var bin = new BinaryFormatter();
                    Matches.AddRange((List<NewMatch>)bin.Deserialize(stream));
                }
                catch (Exception)
                {
                    obsolete.Add(s);
                }
                finally
                {
                    stream.Close();
                }
            }

            // read all XMLs

            progressString += Environment.NewLine + "    Loading XML files.";
            backgroundWorker.ReportProgress(0, progressString);

            var progress = 0;

            foreach (var s in paths)
            {
                var currentFileName = Path.GetFileName(s);
                var newFileName = currentFileName.Remove(currentFileName.Length - 3);
                if (!newFileName.EndsWith(".xml"))
                {
                    newFileName += ".xml";
                }

                using (var originalFileStream = File.OpenRead(s))
                {
                    using (var decompressedFileStream = File.Create(newFileName))
                    {
                        using (var decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                        {
                            decompressionStream.CopyTo(decompressedFileStream);
                        }
                    }
                }

                using (var reader = XmlReader.Create(newFileName))
                {
                    Matches.AddRange(XmlRwHelper.ReadAllMatches(reader));
                }

                File.Delete(newFileName);

                progress++;
                backgroundWorker.ReportProgress(0, progressString + $" - {progress} / {paths.Count()}");
            }

            Matches = NewMatch.DeNullify(Matches);

            progressString += Environment.NewLine + "    Write XML file.";
            backgroundWorker.ReportProgress(0, progressString);
            // XmlWrite all matches
            if (paths.Count + binPaths.Count > 1)
            {
                var myHash = XmlRwHelper.WriteAllMatches(Matches);
            }

            XmlRwHelper.MoveFilesToOldBin(binPaths);
        }
    }
}