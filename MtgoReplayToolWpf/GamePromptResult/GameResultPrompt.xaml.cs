using MtgoReplayToolWpf.MiscHelpers;
using System;
using System.Windows;
using System.Windows.Media;

namespace MtgoReplayToolWpf.GamePromptResult
{
    /// <summary>
    /// Interaction logic for GameResultPrompt.xaml
    /// </summary>
    public partial class GameResultPrompt : Window
    {
        private GameResultPromptViewModel viewModel;

        private Boolean _full;

        private Boolean _skipSettings;

        public GameResultPrompt(GameResultPromptViewModel viewModel)
        {
            InitializeComponent();

            this.viewModel = viewModel;
            DataContext = viewModel;
            RichTextBox.FontFamily = new FontFamily("Arial");
            RichTextBox.FontSize = 13.0;

            RichTextBoxHelper.FillWithShortLog(RichTextBox, viewModel.subStrings);

            switch (viewModel.result)
            {
                case -1:
                    MortGuessLabel.Content = "MORT thinks " + viewModel.Villain + " has won.";
                    break;
                case 0:
                    MortGuessLabel.Content = "MORT has no clue who has won.";
                    break;
                case 1:
                    MortGuessLabel.Content = "MORT thinks " + viewModel.Hero + " has won.";
                    break;
            } 
        }

        

        private void HeroWonButton_OnClick(Object sender, RoutedEventArgs e)
        {
            viewModel.result = 1;
            DialogResult = true;
        }

        private void ConfirmButton_OnClick(Object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void VillainWonButton_OnClick(Object sender, RoutedEventArgs e)
        {
            viewModel.result = -1;
            DialogResult = true;
        }

        private void DrawButton_OnClick(Object sender, RoutedEventArgs e)
        {
            viewModel.result = 0;
            DialogResult = true;
        }

        private void ToggleFullLogButton_OnClick(Object sender, RoutedEventArgs e)
        {
            if (_full)
            {
                RichTextBoxHelper.FillWithShortLog(RichTextBox, viewModel.subStrings);
            }
            else
            {
                RichTextBoxHelper.FillWithFullLog(RichTextBox, viewModel.gameString);
            }
            _full = !_full;
        }

        private void Skip_Settings_OnClick(Object sender, RoutedEventArgs e)
        {
            _skipSettings = !_skipSettings;

            SkipSettingsGrid.Visibility = _skipSettings ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Skipp_all_OnClick(Object sender, RoutedEventArgs e)
        {
            viewModel.SkipSetting = GameResultPromptViewModel.SkipTypes.Ignore;
            viewModel.result = 0;
            DialogResult = true;
        }

        private void Take_Mort_Guess_OnClick(Object sender, RoutedEventArgs e)
        {
            viewModel.SkipSetting = GameResultPromptViewModel.SkipTypes.Guess;
            DialogResult = true;
        }
    }
}
