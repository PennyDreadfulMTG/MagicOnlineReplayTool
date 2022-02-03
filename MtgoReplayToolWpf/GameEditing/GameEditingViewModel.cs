using MtgoReplayToolWpf.DataModel;
using MTGOReplayToolWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MtgoReplayToolWpf.GameEditing
{
    public class GameEditingViewModel
    {
        public MainData MainData { get; }

        public NewGame Game { get; set; }

        public NewMatch Match { get; set; }

        public NewGame OriginalGame { get; set; }

        public NewMatch OriginalMatch { get; set; }

        public GameEditingViewModel(MainData mainData, NewMatch match, NewGame game)
        {
            MainData = mainData;
            OriginalGame = game;
            OriginalMatch = match;
            Match = new NewMatch(OriginalMatch);
            Game = Match.Games.Find(x => x.Id.Equals(OriginalGame.Id));
        }

        internal Boolean HasChanges()
        {
            return !Match.Equals(OriginalMatch);
        }

        internal void CommitChanges()
        {
            if (!MainData.Matches.Remove(OriginalMatch))
            {
                throw new Exception($"Something went wrong when trying to remove OriginalMatch {OriginalMatch.Id}.");
            }
            Match.ManuallyEdited = true;
            MainData.Matches.Add(Match);
            MainData.HasUnsavedChanges = true;
        }
    }
}
