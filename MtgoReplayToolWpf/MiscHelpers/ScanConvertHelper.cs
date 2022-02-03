using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using MTGOReplayToolWpf;
using ParsingHelper = MTGOReplayToolWpf.ParsingHelper;

namespace MtgoReplayToolWpf
{
    public class ScanConvertHelper
    {
        public static String[] FindMtgoFolders()
        {
            var path = Environment.GetEnvironmentVariable("LocalAppData") + "\\Apps";
            var paths = Directory.GetFiles(path, "mtgo_game_history", SearchOption.AllDirectories);
            for (var i = 0; i < paths.Length; i++)
            {
                paths[i] = paths[i].Remove(paths[i].Length - 18);
            }

            return paths;
        }

        static void ReadAndProcess(List<String> files, List<String> newFiles, ProgressBar progressBar, List<NewMatch> allMatches)
        {            
            if (!Directory.Exists("replays"))
            {
                Directory.CreateDirectory("replays");
            }
            
            for (var i = 0; i < files.Count(); i++)
            {
                var destination = @"replays\" + Path.GetFileName(files[i]);
                if (!File.Exists(destination))
                {
                    File.Copy(files[i], destination);
                }

                UiHelper.UpdateProgress(progressBar, 0.5*i/files.Count, 1.0);
                var newFile = newFiles.Contains(files[i]);
                allMatches = ParsingHelper.ProcessMatch(files[i], allMatches, newFile);
            }
        }

        public static Tuple<UpdateData, List<NewMatch>> ScanConvert(String[] paths, ProgressBar progressBar, List<NewMatch> allMatches, Dictionary<String, Format> formats, String filterSize)
        {
            var oldMatches = new List<NewMatch>(allMatches);

            var files = new List<String>();
            var newFiles = new List<String>();

            foreach (var path in paths)
            {
                files.AddRange(Directory.GetFiles(path, "match*.dat"));
                newFiles.AddRange(Directory.GetFiles(path, "match_gamelog*.dat"));                
            }

            // todo returns all matches
            ReadAndProcess(files, newFiles, progressBar, allMatches);

            allMatches = NewMatch.CleanMatches(allMatches);
            allMatches = NewMatch.GetDecks(allMatches, oldMatches, formats);
            var Data = UpdateDataHelper.UpdateStats(allMatches, formats, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty, filterSize, String.Empty, String.Empty, String.Empty);
            
            UiHelper.UpdateProgress(progressBar, 1, 0);
            return new Tuple<UpdateData, List<NewMatch>>(Data, allMatches);
        }

         // defines a delegate type

    }
}
