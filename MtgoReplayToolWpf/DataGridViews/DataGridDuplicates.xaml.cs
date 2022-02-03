using MtgoReplayToolWpf.DataGridViewModels;
using MtgoReplayToolWpf.MiscHelpers;

namespace MtgoReplayToolWpf.DataGridViews
{
    /// <summary>
    /// Interaction logic for DataGridDecks.xaml
    /// </summary>
    public partial class DataGridDuplicates
    {
        public DataGridDuplicates()
        {
            InitializeComponent();
            if (!CommandLineArgsHelper.HasDeckDuplicates())
            { 
                this.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void Button_Click(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            (this.DataContext as DataGridDuplicatesViewModel)?.MergeDecklists();
        }

        private void DataGrid_SelectedCellsChanged(System.Object sender, System.Windows.Controls.SelectedCellsChangedEventArgs e)
        {
            (this.DataContext as DataGridDuplicatesViewModel)?.DataGridDuplicatesSelectedCellsChanged(sender, e);
        }
    }
}
