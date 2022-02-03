using MtgoReplayToolWpf.MiscHelpers;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MtgoReplayToolWpf.GameEditing
{
    /// <summary>
    /// Interaction logic for GameEditingView.xaml
    /// </summary>
    public partial class GameEditingView : Window
    {
        private readonly GameEditingViewModel viewModel;

        public GameEditingView(GameEditingViewModel viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            DataContext = viewModel;

            RichTextBox.FontFamily = new FontFamily("Arial");
            RichTextBox.FontSize = 13.0;

            RichTextBoxHelper.FillWithShortLog(RichTextBox, viewModel.Game.Gamelog);

            // todo on close
            this.Closing += GameEditingView_Closing;
        }

        private void GameEditingView_Closing(Object sender, CancelEventArgs e)
        {
            if (viewModel.HasChanges())
            {
                var message = "You have edited the data of the game and match. Do you want to commit those changes (YES) or exit without commiting (NO)?" + Environment.NewLine;
                var messageBoxResult = MessageBox.Show(message, "Commit Changes", MessageBoxButton.YesNoCancel);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    viewModel.CommitChanges();
                }
                else if (messageBoxResult == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
