using MtgoReplayToolWpf.DataModel;
using System;
using System.ComponentModel;
using System.Linq;

namespace MtgoReplayToolWpf.DeckDefinitions
{
    public class DeckListClassificationViewModel : INotifyPropertyChanged
    {
        public DeckMatrix DeckMatrix { get; set; }

        public Deck DeckToClassify { get; set; }

        public Deck BestMatch { get; set; }

        public Deck SecondBestMatch { get; set; }

        public Deck ThirdBestMatch { get; set; }

        public Deck FourthBestMatch { get; set; }

        public Double BestMatchDice { get; set; }

        public Double SecondBestMatchDice { get; set; }

        public Double ThirdBestMatchDice { get; set; }

        public Double FourthBestMatchDice { get; set; }

        public DeckDifference BestMatchDeckDifference { get; set; }

        public DeckDifference SecondBestMatchDeckDifference { get; set; }

        public DeckDifference ThirdBestMatchDeckDifference { get; set; }

        public DeckDifference FourthBestMatchDeckDifference { get; set; }

        public String DeckList { get; set; }

        public String DeckName { get; set; }

        public DeckListClassificationViewModel(Format format)
        {
            DeckMatrix = new DeckMatrix(format);

            NextDeck(string.Empty);
        }

        private void NextDeck(String nextDeckType)
        {
            while (true)
            {
                switch (nextDeckType)
                {
                    case "Best Matched Deck":
                        DeckToClassify = DeckMatrix.GetBestMatchedDeck();
                        break;
                    case "Worst Matched Deck":
                        DeckToClassify = DeckMatrix.GetWorstMatchedDeck();
                        break;
                    default:
                        DeckToClassify = DeckMatrix.GetAnyDeck();
                        break;
                }
                
                var bestMatches = DeckMatrix.GetBestMatchesForDeck(DeckToClassify, 4);
                
                BestMatch = bestMatches[0].Item2;
                SecondBestMatch = bestMatches[1].Item2;
                ThirdBestMatch = bestMatches[2].Item2;
                FourthBestMatch = bestMatches[3].Item2;

                BestMatchDice = bestMatches[0].Item1;
                SecondBestMatchDice = bestMatches[1].Item1;
                ThirdBestMatchDice = bestMatches[2].Item1;
                FourthBestMatchDice = bestMatches[3].Item1;

                if (BestMatchDice >= .96)
                {
                    DeckMatrix.SetManualClassification(DeckToClassify, BestMatch.Name);
                }
                else
                {
                    break;
                }
            }

            if (DeckToClassify != null)
            {
                DeckName = DeckToClassify.Name;
                DeckList = DeckToClassify.Name + Environment.NewLine + DeckToClassify.Date.ToString() + Environment.NewLine +
                    DeckToClassify.Definition.Select(kvp => Convert.ToInt32(kvp.Value).ToString() + ' ' + kvp.Key).ToList().Aggregate((x, y) => x + Environment.NewLine + y);
            }
            else
            {
                DeckName = "No data";
                DeckList = "No data";

                DeckToClassify = Deck.GetDummy();
            }

            BestMatchDeckDifference = new DeckDifference(DeckToClassify, BestMatch);
            SecondBestMatchDeckDifference = new DeckDifference(DeckToClassify, SecondBestMatch);
            ThirdBestMatchDeckDifference = new DeckDifference(DeckToClassify, ThirdBestMatch);
            FourthBestMatchDeckDifference = new DeckDifference(DeckToClassify, FourthBestMatch);

            RaisePropertyChanged(nameof(DeckToClassify));
            RaisePropertyChanged(nameof(DeckName));
            RaisePropertyChanged(nameof(DeckList));
            RaisePropertyChanged(nameof(BestMatch));
            RaisePropertyChanged(nameof(SecondBestMatch));
            RaisePropertyChanged(nameof(ThirdBestMatch));
            RaisePropertyChanged(nameof(FourthBestMatch));
            RaisePropertyChanged(nameof(BestMatchDice));
            RaisePropertyChanged(nameof(SecondBestMatchDice));
            RaisePropertyChanged(nameof(ThirdBestMatchDice));
            RaisePropertyChanged(nameof(FourthBestMatchDice));
            RaisePropertyChanged(nameof(BestMatchDeckDifference));
            RaisePropertyChanged(nameof(SecondBestMatchDeckDifference));
            RaisePropertyChanged(nameof(ThirdBestMatchDeckDifference));
            RaisePropertyChanged(nameof(FourthBestMatchDeckDifference));
        }

        public void ClassifyDeck(String deckName, String nextDeck)
        {
            if (DeckToClassify != null && DeckToClassify.Filepath != String.Empty)
            {
                DeckMatrix.SetManualClassification(DeckToClassify, deckName);
            }
            NextDeck(nextDeck);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
