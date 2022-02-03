using System;

namespace MtgoReplayToolWpf.MiscHelpers
{
    public static class CommandLineArgsHelper
    {
        public static Boolean HasDeckDefEditor()
        {
            return App.Args.Contains("-deckdefeditor");
        }

        public static Boolean HasDeckDuplicates()
        {
            return App.Args.Contains("-deckduplicates");
        }
    }
}
