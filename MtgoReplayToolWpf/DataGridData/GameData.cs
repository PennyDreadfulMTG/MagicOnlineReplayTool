using MTGOReplayToolWpf;
using System;
using System.Collections.Generic;

namespace MtgoReplayToolWpf.DataGridData
{
    public class GameData: IEquatable<GameData>
    {
        public NewGame Game { get; set; }

        public NewMatch Match { get; set; }

        public String MatchId => Match.Id;

        public Int32 MatchResult => Match.Result;

        public String Hero => Match.Hero;

        public String Villain => Match.Villain;

        public String HeroDeck => Match.HDeck;

        public String VillainDeck => Match.VDeck;

        public DateTime Date => Match.Date;

        public Int32 GameId => Game.Id;

        public Int32 GameResult => Game.Result;

        public Int32 HeroMull => Game.HMull;

        public Int32 VillainMull => Game.VMull;

        public Boolean PreBoard => Game.PreBoard;

        public Int32 Turn => Game.Turn;

        public Boolean HeroOnPlay => Game.OnDraw;

        public List<String> HeroCardsPlayed => Game.HCardsPlayed;

        public List<String> VillainCardsPlayed => Game.VCardsPlayed;

        public List<String> HeroCardsPut => Game.HCardsPut;

        public List<String> VillainCardsPut => Game.VCardsPut;

        public List<Tuple<Int32, Int32, String>> Gamelog => Game.Gamelog;

        public Boolean ManuallyEdited => Match.ManuallyEdited;

        public GameData()
        {
            Game = new NewGame();
            Match = new NewMatch(String.Empty);
        }

        public GameData(NewGame game, NewMatch match)
        {
            Game = game;
            Match = match;
        }

        public override Int32 GetHashCode()
        {
            return (this.MatchId.GetHashCode() * 97) + this.GameId;
        }

        public Boolean Equals(GameData other)
        {
            return this.GetHashCode().Equals(other.GetHashCode());
        }
    }
}
