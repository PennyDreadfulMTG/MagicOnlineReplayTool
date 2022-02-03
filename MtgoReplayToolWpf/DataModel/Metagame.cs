using MTGOReplayToolWpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MtgoReplayToolWpf.DataModel
{
    public class Metagame
    {
        public List<DataPoint> MetaData { get; set; }

        public Metagame(MainData mainData)
        {
            MetaData = new List<DataPoint>();

            var submittingPlayers = new List<String>();
            var submittingPlayersWithMatches = new List<Tuple<String, Int32>>();

            var heroes = mainData.Matches.GroupBy(x => x.Hero);
            var villains = mainData.Matches.GroupBy(x => x.Villain);
            
            var matchesToDo = new List<NewMatch>(mainData.Matches/*.Where(x => x.HDeck.StartsWith("Modern") || x.VDeck.StartsWith("Modern"))*/);
            var minDate = matchesToDo.Min(x => x.Date);
            
            while (matchesToDo.Count > 0)
            {
                var match = matchesToDo.First();

                var heroMatches = heroes.FirstOrDefault(x => x.Key.Equals(match.Hero))?.Count() ?? 0 + villains.FirstOrDefault(x => x.Key.Equals(match.Hero))?.Count() ?? 0;
                var villainMatches = heroes.FirstOrDefault(x => x.Key.Equals(match.Villain))?.Count() ?? 0 + villains.FirstOrDefault(x => x.Key.Equals(match.Villain))?.Count() ?? 0;

                var submittingPlayer = match.Hero;
                if (heroMatches < villainMatches)
                {
                    submittingPlayer = match.Villain;
                }

                submittingPlayers.Add(submittingPlayer);
                submittingPlayersWithMatches.Add(new Tuple<String, Int32>(submittingPlayer, Math.Max(heroMatches, villainMatches)));
                matchesToDo.RemoveAll(x => x.Hero.Equals(submittingPlayer) || x.Villain.Equals(submittingPlayer));                
            }

            submittingPlayersWithMatches.Sort((x, y) => x.Item2.CompareTo(y.Item2));
            submittingPlayersWithMatches.Reverse();
            var str = submittingPlayersWithMatches.Where(x => x.Item2 >= 50).Select(x => x.Item1).Aggregate((x, y) => x + ", " + y);

            matchesToDo = new List<NewMatch>(mainData.Matches.Where(x => x.HDeck.StartsWith("Modern") || x.VDeck.StartsWith("Modern")));

            while (matchesToDo.Count > 0)
            {
                var match = matchesToDo.ElementAt(0);
                matchesToDo.RemoveAt(0);

                if (submittingPlayers.Contains(match.Hero))
                {
                    if (submittingPlayers.Contains(match.Villain))
                    {
                        continue;
                    }
                    else
                    {
                        var dataPoint = new DataPoint();
                        dataPoint.Date = match.Date;
                        dataPoint.Deck = match.VDeck;
                        MetaData.Add(dataPoint);
                    }
                }
                else
                {
                    if (submittingPlayers.Contains(match.Villain))
                    {
                        var dataPoint = new DataPoint();
                        dataPoint.Date = match.Date;
                        dataPoint.Deck = match.HDeck;
                        MetaData.Add(dataPoint); 
                    }
                    else
                    {
                        throw new Exception($"Neither player marked as submitting player: {match.Hero} - {match.Villain}");
                    }
                }
            }

            var json = JsonConvert.SerializeObject(MetaData);
        }
    }

    public class DataPoint
    {
        public DateTime Date { get; set; }

        public String Deck { get; set; }
    }
}
