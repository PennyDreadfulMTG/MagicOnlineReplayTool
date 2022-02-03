using System;
using System.Collections.Generic;

namespace MtgoReplayToolWpf.DataModel
{
    public class DeckMatrix
    {
        Format Format;

        Dictionary<String, Dictionary<String, Double>> matrix = new Dictionary<String, Dictionary<String, Double>>();

        Dictionary<String, Double> UnclassifiedDecksScores = new Dictionary<String, Double>();

        public DeckMatrix(Format format)
        {
            Format = format;

            //Int32 maxSize = 2000;
            UiHelper.Maximum = Format.Decks.Count;
            UiHelper.Progress = 0;

            foreach (var notClassifiedDeck in Format.Decks)
            {
                //if (matrix.Keys.Count > maxSize)
                //{
                //    break;
                //}
                UiHelper.AddProgress(1);

                if (notClassifiedDeck.ManuallyEdited)
                {
                    continue;
                }

                if (!matrix.ContainsKey(notClassifiedDeck.Filepath))
                {
                    matrix.Add(notClassifiedDeck.Filepath, new Dictionary<String, Double>());
                }

                if (!UnclassifiedDecksScores.ContainsKey(notClassifiedDeck.Filepath))
                {
                    UnclassifiedDecksScores.Add(notClassifiedDeck.Filepath, 0.0);
                }

                foreach (var classifiedDeck in Format.Decks)
                {
                    if (!classifiedDeck.ManuallyEdited)
                    {
                        continue;
                    } 
                    
                    SetValue(notClassifiedDeck.Filepath, classifiedDeck.Filepath, classifiedDeck.GetDice(notClassifiedDeck));                    
                }
            }
        }

        internal List<Tuple<Double, Deck>> GetBestMatchesForDeck(Deck deckToClassify, Int32 numberOfDecksToReturn)
        {
            var list = new List<Tuple<Double, Deck>>();
            if (deckToClassify != null)
            { 
                var row = matrix[deckToClassify.Filepath];
            
                foreach (var classifiedDeck in row.Keys)
                {
                    list.Add(new Tuple<Double, Deck>(row[classifiedDeck], Format.Decks.Find(x => x.Filepath.Equals(classifiedDeck))));
                }

                list.Sort((x, y) => y.Item1.CompareTo(x.Item1));
            }
            while (list.Count < numberOfDecksToReturn)
            {
                list.Add(new Tuple<Double, Deck>(0.0, Deck.GetDummy()));
            }
            return list.GetRange(0, numberOfDecksToReturn);
        }

        public Double GetValue(String firstDeck, String secondDeck)
        {
            return matrix[firstDeck][secondDeck];
        }

        public void SetValue(String firstDeck, String secondDeck, Double value)
        {
            matrix[firstDeck][secondDeck] = value;
            if (UnclassifiedDecksScores[firstDeck] < value)
            {
                UnclassifiedDecksScores[firstDeck] = value;
            }
        }

        public void SetManualClassification(Deck deck, String name)
        {
            deck.Name = name;
            deck.ManuallyEdited = true;

            DeckHelper.WriteDeckToFile(deck.Filepath, Format.Name, deck.Name, deck.Date, deck.Definition, true);
            
            matrix.Remove(deck.Filepath);
            UnclassifiedDecksScores.Remove(deck.Filepath);

            foreach (var notClassifiedDeck in Format.Decks)
            {
                if (notClassifiedDeck.ManuallyEdited)
                {
                    continue;
                }
                
                SetValue(notClassifiedDeck.Filepath, deck.Filepath, deck.GetDice(notClassifiedDeck));
            }
        }

        public Deck GetAnyDeck()
        {
            var enumerator = matrix.Keys.GetEnumerator();
            enumerator.MoveNext();
            var current = enumerator.Current;
            return Format.Decks.Find(x => x.Filepath.Equals(current));
        }

        public Deck GetWorstMatchedDeck()
        {
            var worstMatchTotal = 1.0;
            var worstDeck = String.Empty;

            foreach (var unclassifiedDeck in UnclassifiedDecksScores.Keys)
            {
                if (UnclassifiedDecksScores[unclassifiedDeck] < worstMatchTotal)
                {
                    worstMatchTotal = UnclassifiedDecksScores[unclassifiedDeck];
                    worstDeck = unclassifiedDeck;
                }
            }

            return Format.Decks.Find(x => x.Filepath.Equals(worstDeck));
        }

        public Deck GetBestMatchedDeck()
        {
            var bestDeck = String.Empty;
            var bestMatchTotal = 0.0;
                
            foreach (var unclassifiedDeck in UnclassifiedDecksScores.Keys)
            {
                if (UnclassifiedDecksScores[unclassifiedDeck] > bestMatchTotal)
                {
                    bestMatchTotal = UnclassifiedDecksScores[unclassifiedDeck];
                    bestDeck = unclassifiedDeck;
                }
            }            

            return Format.Decks.Find(x => x.Filepath.Equals(bestDeck));
        }
    }
}
