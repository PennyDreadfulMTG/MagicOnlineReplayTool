using MtgoReplayToolWpf.MiscHelpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace MtgoReplayToolWpf
{
    /// <summary>
    /// Interaction logic for FormatSelector.xaml
    /// </summary>
    public partial class FormatSelector : Window
    {
        private GithubHelper.Asset[] Assets;
        private List<GithubHelper.Asset> Installed;

        public FormatSelector()
        {
            InitializeComponent();
            
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            var formatWorker = new BackgroundWorker();
            formatWorker.DoWork += GetFormats_DoWork;
            formatWorker.RunWorkerCompleted += GetFormats_RunWorkerCompleted;
            formatWorker.RunWorkerAsync();
        }

        private void GetFormats_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            FormatsList.ItemsSource = Assets;
            FormatsList.SelectedItemsOverride = Installed;
        }

        private void GetFormats_DoWork(object sender, DoWorkEventArgs e)
        {
            var assets = GithubHelper.FetchReleaseAssets("PennyDreadfulMTG", "MORT-Decks").Where(a => a.Name.EndsWith(".zip"));
            Assets = assets.ToArray();
            Installed = Assets.Where(a => File.Exists(Path.Combine(DeckHelper.DeckPath, a.Name))).ToList();
        }

        private void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            var installerWorker = new BackgroundWorker();
            installerWorker.DoWork += InstallerWorker_DoWork;
            installerWorker.RunWorkerCompleted += InstallerWorker_RunWorkerCompleted;
            InstallButton.Visibility = Visibility.Hidden;
            CancelButton.Visibility = Visibility.Hidden;
            progress.Visibility = Visibility.Visible;
            installerWorker.RunWorkerAsync();
        }

        private void InstallerWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void InstallerWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            foreach (var item in Installed)
            {
                if (!item.Exists(DeckHelper.DeckPath))
                {
                    var zip = item.Download(DeckHelper.DeckPath);
                    DeckHelper.Unzip(zip);
                }
            }

            var uninstall = Assets.Where(a => !Installed.Contains(a) && a.Exists(DeckHelper.DeckPath));
            foreach (var item in uninstall)
            {
                File.Delete(Path.Combine(DeckHelper.DeckPath, item.Name));
                Directory.Delete(Path.Combine(DeckHelper.DeckPath, Path.GetFileNameWithoutExtension(item.Name)), true);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
