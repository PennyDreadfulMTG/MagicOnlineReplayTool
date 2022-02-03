using System;
using System.Linq;
using System.Collections.Generic;

namespace MtgoReplayToolWpf
{
    public class Deck
    {
        public Deck(String name, Dictionary<String, Double> definition, Int32 sampleSize, DateTime date, String filepath, String format, Boolean manuallyEdited)
        {
            Name = name;
            Definition = definition;
            SampleSize = sampleSize;
            Date = date;
            Filepath = filepath;
            Format = format;
            ManuallyEdited = manuallyEdited;
        }

        public String Name { get; set; }

        public String Format { get; set; }

        public Int32 SampleSize { get; set; }

        public Dictionary<String, Double> Definition { get; set; }

        public DateTime Date { get; set; }

        public String Filepath { get; set; }

        public Boolean ManuallyEdited { get; set; }

        internal double GetDice(Deck deckList)
        {
            Double union = 0.0;
            Double here = 0.0;
            Double there = 0.0;
            Definition.ToList().ForEach(x =>
                {
                    here += x.Value;
                    if (deckList.Definition.TryGetValue(x.Key, out Double value))
                    {
                        union += Math.Min(value, x.Value);
                    }
                }
            );

            deckList.Definition.ToList().ForEach(x =>
                {
                    there += x.Value;
                }
            );

            return 2 * union / (here + there);
        }

        public override string ToString()
        {
            return Name;
        }

        public string ToFullString()
        {
            var myString = Name + Environment.NewLine + Environment.NewLine;
            Definition.ToList().ForEach(x => myString = myString + x.Key + " " + x.Value.ToString("#.##") + Environment.NewLine);
            return myString;
        }

        internal void AddDeckList(Deck deckList)
        {
            deckList.Definition.ToList().ForEach(x =>
                {
                    if (Definition.TryGetValue(x.Key, out Double value))
                    {
                        Definition[x.Key] = (value * SampleSize + x.Value) / (SampleSize + 1); 
                    }
                    else
                    {
                        Definition.Add(x.Key, x.Value / (SampleSize + 1));
                    }
                });
            SampleSize++;
        }

        internal static Deck GetDummy()
        {
            return new Deck("Dummy", new Dictionary<String, Double>(), 1, DateTime.Now, String.Empty, String.Empty, false);
        }
    }

    public class DeckDifference
    {
        public String Union { get; set; }

        public String Here { get; set; }

        public String There { get; set; }

        public DeckDifference(Deck deck, Deck otherDeck)
        {
            deck.Definition.ToList().ForEach(x =>
            {
                if (!otherDeck.Definition.TryGetValue(x.Key, out var value))
                {
                    value = 0.0;
                }

                Double union = Math.Min(x.Value, value);
                Double here = x.Value - value;
                Double there = value - x.Value;
                if (union > 0.0)
                {
                    Union += $"{union} {x.Key}{Environment.NewLine}";
                }

                if (here > 0.0)
                {
                    Here += $"{here} {x.Key}{Environment.NewLine}";
                }

                if (there > 0.0)
                {
                    There += $"{there} {x.Key}{Environment.NewLine}";
                }
            });

            otherDeck.Definition.ToList().ForEach(x =>
            {
                if (!deck.Definition.ContainsKey(x.Key))
                {
                    There += $"{x.Value} {x.Key}{Environment.NewLine}";
                }
            });
        }
    }
}
