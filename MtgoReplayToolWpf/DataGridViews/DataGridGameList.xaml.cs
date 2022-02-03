using MtgoReplayToolWpf.DataGridData;
using MtgoReplayToolWpf.DataGridViewModels;
using MtgoReplayToolWpf.GameEditing;
using System;

using System.Windows;
using System.Windows.Controls;


namespace MtgoReplayToolWpf.DataGridViews
{
    /// <summary>
    /// Interaction logic for DataGridGameList.xaml
    /// </summary>
    public partial class DataGridGameList
    {
        public DataGridGameList()
        {
            InitializeComponent();
        }

        private void MenuItem_Click_Edit(Object sender, RoutedEventArgs e)
        {
            //Get the clicked MenuItem
            var menuItem = (MenuItem)sender;

            //Get the ContextMenu to which the menuItem belongs
            var contextMenu = (ContextMenu)menuItem.Parent;

            //Find the placementTarget
            var item = (DataGrid)contextMenu.PlacementTarget;

            //Get the underlying item, that you cast to your object that is bound
            //to the DataGrid (and has subject and state as property)
            var itemToShow = (GameData)item.SelectedCells[0].Item;
            var viewModel = (this.DataContext as DataGridGameListViewModel);
            var gameEditingViewModel = new GameEditingViewModel(viewModel.AllMatches, itemToShow.Match, itemToShow.Game);

            var gameEditingView = new GameEditingView(gameEditingViewModel);

            gameEditingView.ShowDialog();
        }
    }
}
