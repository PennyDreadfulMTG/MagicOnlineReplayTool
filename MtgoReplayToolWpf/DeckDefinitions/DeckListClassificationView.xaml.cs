using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MtgoReplayToolWpf.DeckDefinitions
{
    /// <summary>
    /// Interaction logic for DeckListClassificationView.xaml
    /// </summary>
    public partial class DeckListClassificationView : Window
    {
        public DeckListClassificationViewModel ViewModel { get; }

        public DeckListClassificationView(DeckListClassificationViewModel viewModel)
        {
            ViewModel = viewModel;
            this.DataContext = ViewModel;
            InitializeComponent();
        }

        private void NewDeckNameButton_Click(Object sender, RoutedEventArgs e)
        {
            ViewModel.ClassifyDeck(NewNameTextBox.Text, NextDeckComboBox.SelectionBoxItem.ToString());
        }

        private void BestMatchButton_Click(Object sender, RoutedEventArgs e)
        {
            ViewModel.ClassifyDeck(ViewModel.BestMatch.Name, NextDeckComboBox.SelectionBoxItem.ToString());
        }

        private void SecondBestMatchButton_Click(Object sender, RoutedEventArgs e)
        {
            ViewModel.ClassifyDeck(ViewModel.SecondBestMatch.Name, NextDeckComboBox.SelectionBoxItem.ToString());
        }

        private void ThirdBestMatchButton_Click(Object sender, RoutedEventArgs e)
        {
            ViewModel.ClassifyDeck(ViewModel.ThirdBestMatch.Name, NextDeckComboBox.SelectionBoxItem.ToString());
        }

        private void FourthBestMatchButton_Click(Object sender, RoutedEventArgs e)
        {
            ViewModel.ClassifyDeck(ViewModel.FourthBestMatch.Name, NextDeckComboBox.SelectionBoxItem.ToString());
        }
    }
}
