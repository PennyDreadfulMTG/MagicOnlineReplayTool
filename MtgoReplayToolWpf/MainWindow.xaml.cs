using MtgoReplayToolWpf.MiscHelpers;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace MtgoReplayToolWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {            
            InitializeComponent();
            MainViewModel = new MainViewModel(this);
            this.DataContext = MainViewModel;
            Visibility = Visibility.Hidden;
            IsEnabled = false;
            Hide();
            if (!CommandLineArgsHelper.HasDeckDefEditor())
            {
                ButtonLearnDecks.Visibility = Visibility.Collapsed;
            }
            this.Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(Object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MainViewModel.Data.HasUnsavedChanges)
            {
                var message = "You have edited your match data. Do you want to commit those changes (YES) or exit without commiting (NO)?" + Environment.NewLine;
                var messageBoxResult = MessageBox.Show(message, "Commit Changes", MessageBoxButton.YesNoCancel);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    MainViewModel.Data.SaveData();
                }
                else if (messageBoxResult == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        public MainViewModel MainViewModel { get; set; }

        internal void ShowMessage(String message)
        {
            Label.Content = message;
        }

        private void ButtonUpdateGrid_OnClick(Object sender, RoutedEventArgs e)
        {
            MainViewModel.UpdateAll();
        }

        private void ButtonScanMtgoReplayFolder_OnClick(Object sender, RoutedEventArgs e)
        {
            MainViewModel.ScanMtgoFolder();
        }

        private void ButtonOpenCustomReplayFolder_OnClick(Object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    MainViewModel.ScanFolder(dialog.SelectedPath);
                }
            }
        }
        private void ButtonSaveData_OnClick(Object sender, RoutedEventArgs e)
        {
            MainViewModel.SaveData();
        }
        
        public void SetLock(Boolean value)
        {
            IsEnabled = !value;
            ProgressBar.IsIndeterminate = value;
        }

        private void ButtonReclassifyDecks_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel.ReclassifyDecks();
        }

        private void ButtonUploadData_OnClick(object sender, RoutedEventArgs e)
        {
            MainViewModel.UploadData();
        }

        private void ButtonLearnDecks_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel.LearnDecks();
        }
    }
}
