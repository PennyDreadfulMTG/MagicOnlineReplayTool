using MtgoReplayToolWpf.DataGridViewModels;
using MtgoReplayToolWpf.MiscHelpers;

namespace MtgoReplayToolWpf.DataGridViews
{
    /// <summary>
    /// Interaction logic for DataGridDeckDefinitions.xaml
    /// </summary>
    public partial class DataGridDeckDefinitionsViewer
    {
        public DataGridDeckDefinitionsViewer()
        {
            InitializeComponent();
            if (!CommandLineArgsHelper.HasDeckDefEditor())
            {
                this.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void DataGridDecklists_SelectedCellsChanged(object sender, System.Windows.Controls.SelectedCellsChangedEventArgs e)
        {
            (this.DataContext as DataGridDeckDefinitionsViewModel)?.DataGridDecklistsSelectedCellsChanged(sender, e);
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            (this.DataContext as DataGridDeckDefinitionsViewModel)?.ReclassifySelectedDecks();
        }

        private void Button_Click_All(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            (this.DataContext as DataGridDeckDefinitionsViewModel)?.ReclassifyAllDecks();
        }

        private void Button_Click_Reset(System.Object sender, System.Windows.RoutedEventArgs e)
        {
            (this.DataContext as DataGridDeckDefinitionsViewModel)?.ResetManuallyEditedSelectedDecks();
        }

        private void DataGridDeckDefinitions_SelectedCellsChanged(System.Object sender, System.Windows.Controls.SelectedCellsChangedEventArgs e)
        {
            (this.DataContext as DataGridDeckDefinitionsViewModel)?.DataGridDeckDefinitionsSelectedCellsChanged(sender, e);
        }
    }
}
