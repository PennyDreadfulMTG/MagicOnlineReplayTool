using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MtgTop8Crawler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public String DataPath => AppDomain.CurrentDomain.BaseDirectory + @"\Data\";

        public String RootUrl { get; private set; }

        public Boolean IsRunning { get; set; }

        public CancellationTokenSource CancellationTokenSource { get; set; }
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!IsRunning)
            {
                
                RootUrl = TextBoxUrl.Text;
                LabelLog.Tag = 0;

                StartStopButton.Content = "Stop the Crawl";

                CancellationTokenSource = new CancellationTokenSource();
                var token = CancellationTokenSource.Token;

                StartStuffAsync(token);
                IsRunning = true;
            }
            else
            {
                StartStopButton.Content = "Start the Crawl";
                CancellationTokenSource.Cancel();
                IsRunning = false;
            }

            //var thread = new Thread(() => );
            //thread.SetApartmentState(ApartmentState.STA);
            //thread.
            //thread.Start();
        }

        private async void StartStuffAsync(CancellationToken token)
        {
            await Task.Run(() => CrawlRootAsync(RootUrl, token), token);
        }

        private void CrawlRootAsync(String rootUrl, CancellationToken token)
        {
            const String liveLinksFileName = "liveLinks.txt";
            const String deadLinksFileName = "deadLinks.txt";
            const String forceUrlListFilename = "forceUrlList.txt";

            GitPull();

            Stack<String> liveUrlStack = ReadLinksFromFile(DataPath, liveLinksFileName);
            Stack<String> deadUrlStack = ReadLinksFromFile(DataPath, deadLinksFileName);
            var forceUrlList = ReadLinksFromFile(DataPath, forceUrlListFilename);
            var urlWhitelist = new List<String>();
            urlWhitelist.Add(rootUrl);

            var startTime = DateTime.Now;
            var startCountDeadUrls = deadUrlStack.Count;

            liveUrlStack.Push(rootUrl);

            foreach (var url in forceUrlList)
            {
                liveUrlStack.Push(url);
            }
            forceUrlList.Clear();
            WriteToFile(forceUrlList, DataPath, forceUrlListFilename);

            while (liveUrlStack.Count > 0 && !token.IsCancellationRequested)
            {
                var elapsedTime = DateTime.Now - startTime;
                var linksPerMinute = (deadUrlStack.Count - startCountDeadUrls) / elapsedTime.TotalMinutes;
                
                this.Dispatcher.Invoke(() =>
                {
                    LabelLog.Content = "Downloads: " + ((Int32)LabelLog.Tag) + Environment.NewLine
                            + "Downloads/min: " + (((Int32)LabelLog.Tag)/ elapsedTime.TotalMinutes) + Environment.NewLine
                            + "Links: " + deadUrlStack.Count + Environment.NewLine
                            + "Links/min: " + linksPerMinute + Environment.NewLine
                            + "Links TODO: " + liveUrlStack.Count;
                });

                var url = liveUrlStack.Pop();

                if (!urlWhitelist.Contains(url))
                {
                    if (deadUrlStack.Contains(url))
                    {
                        continue;
                    }
                    else
                    {
                        deadUrlStack.Push(url);
                    }
                }
                else
                {
                    urlWhitelist.Remove(url);
                }

                try
                {
                    var newUrls = CrawlAndExtractUrl(url, rootUrl);

                    newUrls.ForEach(x =>
                    {
                        if (!liveUrlStack.Contains(x) && (!deadUrlStack.Contains(x) || urlWhitelist.Contains(x)))
                        {
                            liveUrlStack.Push(x);
                        }
                    });
                }
                catch (Exception exception)
                {
                    if (!(exception is WebException))
                    throw;
                }

                Thread.Sleep(10000);
                WriteToFile(liveUrlStack, DataPath, liveLinksFileName);
                WriteToFile(deadUrlStack, DataPath, deadLinksFileName);
            }
            GitPush();
        }

        private void GitPull()
        {
            Process git;
            if (!Directory.Exists(DataPath))
                git = Process.Start("git", $"clone https://github.com/PennyDreadfulMTG/MORT-Decks.git {DataPath}");
            else
                git = Process.Start(new ProcessStartInfo("git", "pull") { WorkingDirectory = DataPath });
            git.WaitForExit();

        }

        private void GitPush()
        {
            Process.Start(new ProcessStartInfo("git", "add .") { WorkingDirectory = DataPath }).WaitForExit();
            Process.Start(new ProcessStartInfo("git", $"commit -m \"{LabelLog.Tag} new decks\"") { WorkingDirectory = DataPath }).WaitForExit();
            Process.Start(new ProcessStartInfo("git", "push") { WorkingDirectory = DataPath }).WaitForExit();

        }

        private void WriteToFile(Stack<String> urlStack, String path, String fileName)
        {
            var sortedList = urlStack.ToList();
            sortedList.Sort();
            File.WriteAllLines(path + fileName, sortedList);
        }

        private Stack<String> ReadLinksFromFile(String path, String fileName)
        {
            if (File.Exists(path + fileName))
            {
                var array = File.ReadAllLines(path + fileName);
                return new Stack<String>(array);
            }

            return new Stack<String>();
        }

        private List<String> CrawlAndExtractUrl(String url, String rootUrl)
        {
            var myWebRequest = WebRequest.Create(url);
            var myWebResponse = myWebRequest.GetResponse();
            var responseStream = myWebResponse.GetResponseStream();

            var sreader = new StreamReader(responseStream);
            var responseString = sreader.ReadToEnd();
            var links = GetLinks(responseString, url, rootUrl);
            var linksToFollow = links.Where(x => x.Contains("f=") || !x.Contains("mtgtop8.com/event?"));

            GetDeck(links.Where(x => x.Contains("mtgo?")).ToList(), responseString);

            return linksToFollow.ToList();
        }

        
        private void GetDeck(List<String> links, String responseString)
        {
            Regex regexLinkDate = new Regex("\\d\\d/\\d\\d/\\d\\d");
            
            var matchesDate = regexLinkDate.Matches(responseString);

            var latestDate  = new DateTime();

            foreach (var match in matchesDate)
            {
                try
                {
                    var date = new DateTime(2000 + Convert.ToInt32(match.ToString().Substring(6, 2)), Convert.ToInt32(match.ToString().Substring(3, 2)), Convert.ToInt32(match.ToString().Substring(0, 2)));
                    if (date.CompareTo(latestDate) >= 0)
                    {
                        latestDate = date;
                    }
                }
                catch
                {
                    continue;
                }
            }

            foreach (var link in links)
            {
                DownloadDeck(link, latestDate);
            }
        }

        private void DownloadDeck(String link, DateTime date)
        {
            var myWebRequest = WebRequest.Create(link);
            var myWebResponse = myWebRequest.GetResponse();
            var responseStream = myWebResponse.GetResponseStream();

            var sreader = new StreamReader(responseStream);
            var responseString = sreader.ReadToEnd();

            var format = "FormatUnknown";
            Regex formatRegex = new Regex("(?<=f=).+?(?=_)");
            var formatMatches = formatRegex.Matches(link);
            if (formatMatches.Count == 1)
            {
                format = formatMatches[0].Value;
            }

            var deckName = "DeckUnknown";
            Regex deckRegex = new Regex($"(?<={format}_).+?(?=_by)");
            var deckNameMatches = deckRegex.Matches(link);
            if (deckNameMatches.Count == 1)
            {
                deckName = deckNameMatches[0].Value;
            }

            this.Dispatcher.Invoke(() =>
            {
                LabelLog.Tag = 1 + (Int32)LabelLog.Tag;
            });

            var formatPath = DataPath + @"\decks\" + format + @"\";

            if (!Directory.Exists(formatPath))
            {
                Directory.CreateDirectory(formatPath);
            }

            File.WriteAllText(formatPath + GetHashString(link) + ".dec",
                date.ToString("s", CultureInfo.InvariantCulture) + Environment.NewLine
                + format + Environment.NewLine
                + deckName + Environment.NewLine
                + responseString);
        }

        public static byte[] GetHash(string inputString)
        {
            HashAlgorithm algorithm = MD5.Create();  //or use SHA256.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public static string GetHashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        private List<String> GetLinks(String responseString, String currentRootUrl, String rootUrl)
        {
            String myRootUrl = currentRootUrl;
            Int32 index = currentRootUrl.IndexOf("?");
            if (index > 0)
            {
                myRootUrl = currentRootUrl.Substring(0, index);
            }

            Regex regexLink = new Regex("href=.*?>");

            ISet<String> newLinks = new HashSet<String>();
            foreach (var match in regexLink.Matches(responseString))
            {
                var matchString = match.ToString();

                if (matchString.Contains("\"") || matchString.Contains(":"))
                {
                    continue;
                }

                if (matchString.Length > 8)
                {
                    var extension = matchString.Substring(5, matchString.Length - 6);
                    if (extension.Length > 1)
                    {
                        if (extension.StartsWith("?"))
                        {
                            var newLink = myRootUrl + extension;
                            if (!newLinks.Contains(newLink))
                                newLinks.Add(newLink);
                        }
                        else
                        {
                            var newLink = rootUrl + "/" + extension;
                            if (!newLinks.Contains(newLink))
                                newLinks.Add(newLink);
                        }
                    }
                }
            }

            return newLinks.ToList();
        }
    }
}
