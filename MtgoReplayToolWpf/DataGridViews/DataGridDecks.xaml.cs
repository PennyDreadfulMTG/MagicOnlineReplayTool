using MtgoReplayToolWpf.DataGridViewModels;

namespace MtgoReplayToolWpf.DataGridViews
{
    /// <summary>
    /// Interaction logic for DataGridDecks.xaml
    /// </summary>
    public partial class DataGridDecks
    {
        public DataGridDecks()
        {
            InitializeComponent();
        }

        private void DataGrid_SelectedCellsChanged(System.Object sender, System.Windows.Controls.SelectedCellsChangedEventArgs e)
        {
            (this.DataContext as DataGridDecksViewModel)?.DataGridSelectedCellsChanged(sender, e);
        }

        //private void MenuItem_Click_Rename(System.Object sender, System.Windows.RoutedEventArgs e)
        //{
        //    (this.DataContext as DataGridDecksViewModel)?.Rename
        //}

        private void MenuItem_Click_Merge(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            (this.DataContext as DataGridDecksViewModel)?.MergeDecklists();
        }
    }
}
