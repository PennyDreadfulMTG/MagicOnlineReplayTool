using System;
using System.Collections.Generic;

namespace MtgoReplayToolWpf.GamePromptResult
{
    public class GameResultPromptViewModel
    {
        public List<Tuple<Int32, Int32, String>> subStrings;
        public Int32 result;
        public String gameString;
        public SkipTypes SkipSetting = SkipTypes.None;

        public enum SkipTypes
        {
            None,
            Ignore,
            Guess
        }

        public GameResultPromptViewModel(List<Tuple<int, int, string>> subStrings, string hero, string villain, int result, string gameString)
        {
            this.subStrings = subStrings;
            Hero = hero;
            Villain = villain;
            this.result = result;
            this.gameString = gameString;
        }

        public String Hero { get; set; }

        public String Villain { get; set; }

        public String HeroButtonText => Hero + " has won";

        public String VillainButtonText => Villain + " has won";

    }
}
