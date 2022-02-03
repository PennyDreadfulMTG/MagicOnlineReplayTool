using MtgoReplayToolWpf;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MTGOReplayToolWpf
{
    [Serializable]
    public class Match
    {
        public Int32 Id;
        public Int32 Result;
        public String Hero, Villain;
        public List<String> HCards, VCards;
        public String HDeck, VDeck;
        public List<Game> Games = new List<Game>();

        public Match(Int32 match)
        {
            Id = match;
            HCards = new List<String>();
            VCards = new List<String>();
        }
    }

    [Serializable]
    public class NewMatch : IEquatable<NewMatch>
    {
        public String Id { get; set; }
        public Int32 Result { get; set; }
        public String Hero { get; set; } = "";
        public String Villain { get; set; } = "";
        public List<String> HCardsPlayed { get; set; }
        public List<String> VCardsPlayed { get; set; }
        public List<String> HCardsPut { get; set; }
        public List<String> VCardsPut { get; set; }
        public String HDeck { get; set; } = "";
        public String VDeck { get; set; } = "";
        public List<NewGame> Games { get; set; } = new List<NewGame>();
        public DateTime Date { get; set; }
        public Boolean ManuallyEdited { get; set; }

        public NewMatch(String match)
        {
            Id = match;
            HCardsPlayed = new List<String>();
            VCardsPlayed = new List<String>();
            HCardsPut = new List<String>();
            VCardsPut = new List<String>();
            ManuallyEdited = false;
        }

        public NewMatch(NewMatch match)
        {
            Id = match.Id;
            Result = match.Result;
            Hero = match.Hero;
            Villain = match.Villain;
            HCardsPlayed = match.HCardsPlayed;
            VCardsPlayed = match.VCardsPlayed;
            HCardsPut = match.HCardsPut;
            VCardsPut = match.VCardsPut;
            HDeck = match.HDeck;
            VDeck = match.VDeck;
            Date = match.Date;
            ManuallyEdited = match.ManuallyEdited;
            Games = new List<NewGame>();
            match.Games.ForEach(x => Games.Add(new NewGame(x)));
        }

        internal void AddGame(NewGame game)
        {
            if (Games.Exists(x => x.Id == game.Id)) return;
            Games.Add(game);
            VCardsPlayed.AddRange(game.VCardsPlayed);
            HCardsPlayed.AddRange(game.HCardsPlayed);
            VCardsPut.AddRange(game.VCardsPut);
            HCardsPut.AddRange(game.HCardsPut);
            VCardsPlayed = VCardsPlayed.Distinct().ToList();
            HCardsPlayed = HCardsPlayed.Distinct().ToList();
            VCardsPut = VCardsPut.Distinct().ToList();
            HCardsPut = HCardsPut.Distinct().ToList();
        }

        internal void GetDecks(Dictionary<String, Format> formats)
        {
            var cards = new List<String>(VCardsPlayed);
            cards.AddRange(VCardsPut);
            cards = cards.Distinct().ToList(); 
            var vDecks = DeckHelper.CompareDecks(formats, cards, this.Date);

            cards = new List<String>(HCardsPlayed);
            cards.AddRange(HCardsPut);
            cards = cards.Distinct().ToList();
            var hDecks = DeckHelper.CompareDecks(formats, cards, this.Date);

            if (hDecks.Count > 0 && vDecks.Count > 0)
            {
                HDeck = hDecks[0].Item2 + " - " + hDecks[0].Item3;
                VDeck = vDecks[0].Item2 + " - " + vDecks[0].Item3;

                if (hDecks[0].Item2 != vDecks[0].Item2)
                {
                    var secondaryHeroDeck = hDecks.First(x => x.Item2.Equals(vDecks[0].Item2));
                    var secondaryVillainDeck = vDecks.First(x => x.Item2.Equals(hDecks[0].Item2));

                    if (secondaryHeroDeck.Item1 + vDecks[0].Item1 > secondaryVillainDeck.Item1 + hDecks[0].Item1)
                    {
                        HDeck = secondaryHeroDeck.Item2 + " - " + secondaryHeroDeck.Item3;
                    }
                    else
                    {
                        VDeck = secondaryVillainDeck.Item2 + " - " + secondaryVillainDeck.Item3;
                    }
                }
            }
            else
            {
                if (hDecks.Count == 0)
                {
                    HDeck = "Unknown";
                }
                else
                {
                    HDeck = hDecks[0].Item2 + " - " + hDecks[0].Item3;
                }


                if (vDecks.Count == 0)
                {
                    VDeck = "Unknown";
                }
                else
                {
                    VDeck = vDecks[0].Item2 + " - " + vDecks[0].Item3;
                }
            }
        }
        
        internal void GetResult()
        {
            Result = 0;
            foreach (var g in Games)
            {
                Result += g.Result; 
            }
        }

        internal Int32 GetGames(Boolean won, Boolean onDraw)
        {
            var counter = 0;
            foreach (var g in Games)
            {
                if (g.OnDraw == onDraw)
                {
                    if (won)
                    {
                        if (g.Result > 0) counter++;
                    }
                    else
                    {
                        if (g.Result < 0) counter++;
                    }
                }
                    
            }
            return counter;
        }

        internal Boolean CheckMatch()
        {
            if (HDeck.Any(c => Char.IsControl(c))) return false;
            if (VDeck.Any(c => Char.IsControl(c))) return false;
            if (Hero.Any(c => Char.IsControl(c))) return false;
            if (Villain.Any(c => Char.IsControl(c))) return false;
            if (Id.Any(c => Char.IsControl(c))) return false;
            if (Games.Count.Equals(0)) return false;
            if (HCardsPlayed.Any(card => card.Any(c => Char.IsControl(c)))) return false;
            if (VCardsPlayed.Any(card => card.Any(c => Char.IsControl(c)))) return false;
            if (Games.Any(g => g.Gamelog.Any(t => t.Item3.Any(c => Char.IsControl(c))))) return false;
            return true;
        }

        public static List<NewMatch> CleanMatches(List<NewMatch> allMatches)
        {
            allMatches = allMatches.Distinct(new MyComparer<NewMatch>()).ToList();
            allMatches = SortMatches(allMatches);
            var deleteList = new List<NewMatch>();

            foreach (var match in allMatches)
            {
                if (!match.CheckMatch())
                {
                    deleteList.Add(match);
                }
            }

            foreach (var match in deleteList)
            {
                allMatches.Remove(match);
            }

            return allMatches;
        }

        public static List<NewMatch> SortMatches(List<NewMatch> unsorted)
        {
            unsorted.Sort((x, y) => x.Date.CompareTo(y.Date));
            return unsorted;
        }

        public static List<NewMatch> DeNullify(List<NewMatch> myMatches)
        {
            foreach (var match in myMatches)
            {
                if (match.HDeck == null) match.HDeck = String.Empty;
                if (match.VDeck == null) match.VDeck = String.Empty;

                foreach (var game in match.Games)
                {
                    if (game.HCardsPlayed == null) game.HCardsPlayed = new List<String>();
                    if (game.VCardsPlayed == null) game.VCardsPlayed = new List<String>();
                    if (game.Gamelog == null) game.Gamelog = new List<Tuple<Int32, Int32, String>>();
                }

            }

            return myMatches;
        }

        public static String GetMyHashCode(List<NewMatch> myList)
        {
            myList = SortMatches(myList);
            var hash = 19;
            foreach (var m in myList)
            {
                hash = 31 * hash + (Int32)m.Date.Ticks;
            }

            if (hash < 0) hash = hash * -1;
            return hash.ToString();
        }

        public static List<NewMatch> GetDecks(List<NewMatch> allMatches, List<NewMatch> oldMatches, Dictionary<String, Format> formats)
        {
            UiHelper.Maximum = allMatches.Count;
            UiHelper.Progress = 0.0;

            allMatches.AsParallel().ForAll((m) => 
            {
                if (!oldMatches.Any(x => x.Id.Equals(m.Id)))
                {
                    // todo account for manually edited somehow?
                    m.GetDecks(formats);
                    m.GetResult();
                }

                lock (UiHelper.MainWindow)
                {
                    UiHelper.AddProgress(1.0);
                }
            });
            
            return allMatches;
        }

        public Boolean Equals(NewMatch other)
        {
            return
            (
                this.Date.Equals(other.Date)
                && GamesEquals(other.Games)
                && this.HDeck.Equals(other.HDeck)
                && this.Hero.Equals(other.Hero)
                && this.Id.Equals(other.Id)
                && this.Result.Equals(other.Result)
                && this.VDeck.Equals(other.VDeck)
                && this.Villain.Equals(other.Villain)
            );
        }

        private Boolean GamesEquals(List<NewGame> otherGames)
        {
            if (this.Games.Count != otherGames.Count)
            {
                return false;
            }

            foreach (var game in this.Games)
            {
                var otherGame = otherGames.FirstOrDefault(x => x.Id.Equals(game.Id));

                if (otherGame == null || !otherGame.Equals(game))
                {
                    return false;
                }
            }

            return true;
        }
    }

    [Serializable]
    public class NewGame: IEquatable<NewGame>
    {
        public Int32 Id { get; set; }
        public Int32 Result { get; set; }
        public Int32 HMull { get; set; }
        public Int32 VMull { get; set; }
        public Boolean PreBoard { get; set; }
        public Int32 Turn { get; set; }
        // true if villain is on the draw!
        public Boolean OnDraw { get; set; }
        public List<String> HCardsPlayed { get; set; } = new List<String>();
        public List<String> VCardsPlayed { get; set; } = new List<String>();
        public List<String> HCardsPut { get; set; } = new List<String>();
        public List<String> VCardsPut { get; set; } = new List<String>();
        public List<Tuple<Int32, Int32, String>> Gamelog { get; set; } = new List<Tuple<Int32, Int32, String>>();
        // 0 System Message
        // 1 action by hero
        // 2 attacks by hero
        // 3 action by villain
        // 4 attacks by villain

        public NewGame()
        {

        }

        public NewGame(Int32 gameId, Int32 result, Boolean onDraw, Int32 turn, Boolean preBoard, Int32 hMull, Int32 vMull, List<String> hCardsPlayed, List<String> vCardsPlayed, List<String> hCardsPut, List<String> vCardsPut,  List<Tuple<Int32,Int32,String>> gamelog)
        {
            Id = gameId;
            Result = result;
            OnDraw = onDraw;
            Turn = turn;
            PreBoard = preBoard;
            HMull = hMull;
            VMull = vMull;
            Gamelog = gamelog;
            HCardsPlayed = hCardsPlayed;
            VCardsPlayed = vCardsPlayed;
            HCardsPut = hCardsPut;
            VCardsPut = vCardsPut;
        }

        public NewGame(NewGame game)
        {
            this.Id = game.Id;
            this.Result = game.Result;
            this.HMull = game.HMull;
            this.VMull = game.VMull;
            this.PreBoard = game.PreBoard;
            this.Turn = game.Turn;
            this.OnDraw = game.OnDraw;
            this.HCardsPlayed = new List<String>(game.HCardsPlayed);
            this.VCardsPlayed = new List<String>(game.VCardsPlayed);
            this.HCardsPut = new List<String>(game.HCardsPut);
            this.VCardsPut = new List<String>(game.VCardsPut);
            this.Gamelog = new List<Tuple<Int32, Int32, String>>(game.Gamelog);
        }

        public Boolean Equals(NewGame other)
        {
            return
            (
                this.HMull.Equals(other.HMull)
                && this.Id.Equals(other.Id)
                && this.OnDraw.Equals(other.OnDraw)
                && this.PreBoard.Equals(other.PreBoard)
                && this.Result.Equals(other.Result)
                && this.Turn.Equals(other.Turn)
                && this.VMull.Equals(other.VMull)
            );
        }
    }

    [Serializable]
    public class Game
    {
        public Int32 Id;
        public Int32 Result;
        public Int32 HMull, VMull;
        public Boolean PreBoard;
        public Int32 Turn;
        public Boolean OnDraw;
        public List<String> HCards;
        public List<String> VCards;
    }
}

