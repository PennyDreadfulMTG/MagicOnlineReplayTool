using MtgoReplayToolWpf.GamePromptResult;
using Sentry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;

namespace MTGOReplayToolWpf
{
    public static class ParsingHelper
    {
        public static GameResultPromptViewModel.SkipTypes Skip = GameResultPromptViewModel.SkipTypes.None;

        public static List<NewMatch> ProcessMatch(String matchPath, List<NewMatch> allMatches, Boolean newFile)
        {
            var file = String.Empty;

            // read file
            try
            {
                using (var sr = new StreamReader(matchPath))
                {
                    file = sr.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                SentrySdk.CaptureException(e);
                MessageBox.Show("The file could not be read:" + Environment.NewLine + e.Message);
            }

            try
            {
                // file[12] == 4 game log 3 chat log
                //if (file[12] != 4) continue;
                if (file.Length < 50) return allMatches;
                var date = File.GetLastWriteTime(matchPath);
                String hero, villain;
                String matchId;

                var start = IndexOf(file,"@P@P", 0) + 4;
                if (start < 4) return allMatches;
                var end = IndexOf(file, " joined", start);
                if (end < 0) return allMatches;
                hero = file.Substring(start, end - start);
                start = IndexOf(file, "@P@P", end) + 4;
                if (start < 4) return allMatches;
                end = IndexOf(file, " joined", start);
                villain = file.Substring(start, end - start);

                if (villain.Equals(hero)) return allMatches;

                if (hero.CompareTo(villain) < 0)
                {
                    //if (!villain.Equals(player)) continue;
                    var s = villain;
                    villain = hero;
                    hero = s;
                }

                if (villain.Contains("\0"))
                {
                    // debug
                    var breakpoint = 0;
                    breakpoint++;
                }

                var gameId = 0;
                if (newFile)
                {
                    matchId = file.Substring(3, 36);
                }
                else
                {
                    matchId = file.Substring(3, 9);
                    gameId = Convert.ToInt32(file.Substring(15, 9));
                }

                if (allMatches.Exists(x => x.Id == matchId))
                {
                    if (newFile) return allMatches;
                    var myMatch = allMatches.Find(x => x.Id == matchId);
                    if (myMatch.Games.Exists(x => x.Id == gameId)) return allMatches;
                }
           
                var seperator = new List<Int32>();
                var gameResults = new List<Int32>();
                var regMatch = Regex.Match(file, "(" + "@P" + hero + "|" + "@P" + villain + ") chooses to play");
                while (regMatch.Success)
                {
                    seperator.Add(regMatch.Index);
                    if (regMatch.Value.Contains(hero))
                    {
                        gameResults.Add(-1);
                    }
                    else
                    {
                        gameResults.Add(1);
                    }
                    regMatch = regMatch.NextMatch();
                }

                seperator.Add(file.Length);

                if (gameResults.Count > 0)
                {
                    gameResults.RemoveAt(0);
                    gameResults.Add(0);
                }

                // results logic
                if (gameResults.Count == 2)
                {
                    gameResults[1] = gameResults[0];
                }

                NewMatch match;
                if (allMatches.Exists(x => x.Id == matchId))
                {
                    match = allMatches.Find(x => x.Id == matchId);
                }
                else
                {
                    match = new NewMatch(matchId);
                    match.Hero = hero;
                    match.Villain = villain;
                    match.Date = date;
                    allMatches.Add(match);
                }

                //iterate games
                for (var j = 0; j < gameResults.Count; j++)
                {
                    var gameString = file.Substring(seperator[j], seperator[j + 1] - seperator[j]);
                    NewGame game = null;
                
                    if (newFile) game = ProcessGame(gameString, j, hero, villain, gameResults[j]);
                    else game = ProcessGame(file, gameId, hero, villain);

                    if (game != null)
                    {
                        match.AddGame(game);
                    }
                }
                
            }
            catch (Exception e)
            {
                SentrySdk.CaptureException(e);
                MessageBox.Show("An Exception occured while trying to process file: " + matchPath + Environment.NewLine + e.Message);
            }

            return allMatches;
        }

        public static NewGame ProcessGame(String gameString, Int32 gameId, String hero, String villain, Int32 result = 0)
        {
            // who is onDraw
            Boolean onDraw;
            var start = 0;
            var end = gameString.IndexOf(" chooses to play first");
            if (end < 0)
            {
                end = gameString.IndexOf(" chooses to play last");
                if (end < 0)
                {
                    onDraw = true;
                }
                else
                {
                    start = 0;
                    var playerOnDraw = gameString.Substring(start, end - start);
                    if (playerOnDraw.Equals(hero)) onDraw = false;
                    else if (playerOnDraw.Equals(villain)) onDraw = true;
                    else
                    {
                        onDraw = true;
                    }
                }
            }
            else
            {
                start = gameString.LastIndexOf("@P", end) + 2;
                var playerOnDraw = gameString.Substring(start, end - start);
                if (playerOnDraw.Equals(hero)) onDraw = true;
                else if (playerOnDraw.Equals(villain)) onDraw = false;
                else
                {
                    onDraw = true;
                }
            }

            // last turn
            var turn = 0;
            start = gameString.LastIndexOf("@PTurn ") + 7;
            end = IndexOf(gameString, ":", start);
            if (start >= 7 && end > 0) turn = Convert.ToInt32(gameString.Substring(start, end - start));
            // pre board?
            var preBoard = gameId == 0;
            if (gameId > 5) preBoard = gameString.IndexOf(hero + " rolled a ") > 0;
            // mulls
            var vMull = GetMull(gameString, villain);
            var hMull = GetMull(gameString, hero);

            #region regex
            var regMatch = Regex.Match("", "");
            var subStrings = new List<Tuple<Int32, Int32, String>>(); // idx, length, type
            var cardsPlayed = new List<String>[2] { new List<String>(), new List<String>() };
            var cardsPut = new List<String>[2] { new List<String>(), new List<String>() };
            var players = new String[2] { hero, villain };
            for (var i = 0; i <= 1; i++)
            {
                regMatch = Regex.Match(gameString, players[i] + " chooses to play (first|last).");
                while (regMatch.Success)
                {
                    subStrings.Add(new Tuple<Int32, Int32, String>(regMatch.Index, i * 2 + 1, regMatch.Value));
                    regMatch = regMatch.NextMatch();
                }

                regMatch = Regex.Match(gameString, players[i] + " mulligans to (6|5|4|3|2|1) cards.");
                while (regMatch.Success)
                {
                    subStrings.Add(new Tuple<Int32, Int32, String>(regMatch.Index, i * 2 + 1, regMatch.Value));
                    regMatch = regMatch.NextMatch();
                }

                regMatch = Regex.Match(gameString, players[i] + " plays ");
                while (regMatch.Success)
                {
                    start = regMatch.Index + regMatch.Length + 2;
                    end = IndexOf(gameString, "@", start + 1);
                    cardsPlayed[i].Add(gameString.Substring(start, end - start));
                    subStrings.Add(new Tuple<Int32, Int32, String>(regMatch.Index, i * 2 + 1, players[i] + " plays " + gameString.Substring(start, end - start)));
                    regMatch = regMatch.NextMatch();
                }
                #region casts
                regMatch = Regex.Match(gameString, players[i] + " casts ");
                while (regMatch.Success)
                {
                    start = regMatch.Index + regMatch.Length + 2;
                    end = IndexOf(gameString, "@", start + 1);
                    if (end - start >= 39)
                    {
                        var tempString = gameString.Substring(start, 39);
                        if (tempString.Equals("card face down using an alternate cost."))
                        {
                            end = start + 39;
                        }
                    }
                    var spell = gameString.Substring(start, end - start);
                    cardsPlayed[i].Add(spell);
                    start = IndexOf(gameString, "]", end);
                    end = IndexOf(gameString, "targeting", start);
                    if (end > 0 && end - start < 25)
                    {
                        if (gameString.Substring(end + 10, 2).Equals("@["))
                        {
                            start = IndexOf(gameString, "@[", start + 1) + 2;
                            end = IndexOf(gameString, "@", start + 1);
                            var target = gameString.Substring(start, end - start);
                            subStrings.Add(new Tuple<Int32, Int32, String>(regMatch.Index, i * 2 + 1, players[i] + " casts " + spell + " targeting " + target + "."));
                            regMatch = regMatch.NextMatch();
                        }
                        else
                        {
                            start = end + 10;
                            end = gameString.IndexOfAny(new Char[2] { '.', ' ' }, start + 1);
                            var target = gameString.Substring(start, end - start);
                            subStrings.Add(new Tuple<Int32, Int32, String>(regMatch.Index, i * 2 + 1, players[i] + " casts " + spell + " targeting " + target + "."));
                            regMatch = regMatch.NextMatch();
                        }
                    }
                    else
                    {
                        subStrings.Add(new Tuple<Int32, Int32, String>(regMatch.Index, i * 2 + 1, players[i] + " casts " + spell + "."));
                        regMatch = regMatch.NextMatch();
                    }
                }
                #endregion

                #region put into play
                regMatch = Regex.Match(gameString, players[i] + " puts @\\[.*?] onto the battlefield");
                while (regMatch.Success)
                {
                    start = IndexOf(gameString, "@[", regMatch.Index) + 2;
                    end = IndexOf(gameString, "@", start + 1);
                    var spell = gameString.Substring(start, end - start);
                    cardsPut[i].Add(spell);
                    subStrings.Add(new Tuple<Int32, Int32, String>(regMatch.Index, i * 2 + 1, players[i] + " puts " + spell + " onto the battlefield."));
                    regMatch = regMatch.NextMatch();
                }

                regMatch = Regex.Match(gameString, players[i] + " puts @\\[.*?] into play");
                while (regMatch.Success)
                {
                    start = IndexOf(gameString, "@[", regMatch.Index) + 2;
                    end = IndexOf(gameString, "@", start + 1);
                    var spell = gameString.Substring(start, end - start);
                    cardsPut[i].Add(spell);
                    subStrings.Add(new Tuple<Int32, Int32, String>(regMatch.Index, i * 2 + 1, players[i] + " puts " + spell + " into play."));
                    regMatch = regMatch.NextMatch();
                }
                #endregion

                #region activated
                regMatch = Regex.Match(gameString, players[i] + " activates an ability of ");
                while (regMatch.Success)
                {
                    start = regMatch.Index + regMatch.Length + 2;
                    end = IndexOf(gameString, "@", start + 1);
                    var spell = gameString.Substring(start, end - start);
                    start = IndexOf(gameString, "]", end);
                    end = IndexOf(gameString, "targeting", start);
                    if (end > 0 && end - start < 25)
                    {
                        if (gameString.Substring(end + 10, 2).Equals("@["))
                        {
                            start = IndexOf(gameString, "@[", start + 1) + 2;
                            end = IndexOf(gameString, "@", start + 1);
                            var target = gameString.Substring(start, end - start);
                            subStrings.Add(new Tuple<Int32, Int32, String>(regMatch.Index, i * 2 + 1, players[i] + " activates " + spell + " targeting " + target + "."));
                            regMatch = regMatch.NextMatch();
                        }
                        else
                        {
                            start = end + 10;
                            end = gameString.IndexOfAny(new Char[2] { '.', ' ' }, start + 1);
                            var target = gameString.Substring(start, end - start);
                            subStrings.Add(new Tuple<Int32, Int32, String>(regMatch.Index, i * 2 + 1, players[i] + " activates " + spell + " targeting " + target + "."));
                            regMatch = regMatch.NextMatch();
                        }
                    }
                    else
                    {
                        subStrings.Add(new Tuple<Int32, Int32, String>(regMatch.Index, i * 2 + 1, players[i] + " activates " + spell + "."));
                        regMatch = regMatch.NextMatch();
                    }
                }
                #endregion

                #region attacks
                regMatch = Regex.Match(gameString, players[i] + " is being attacked by");
                while (regMatch.Success)
                {
                    var attackers = "";
                    var done = false;
                    start = regMatch.Index + regMatch.Length;
                    while (!done)
                    {
                        start = IndexOf(gameString, "[", start) + 1;
                        end = IndexOf(gameString, "@", start);
                        attackers = attackers + gameString.Substring(start, end - start);
                        start = IndexOf(gameString, "]", start) + 1;
                        if (start >= gameString.Length - 1) done = true;
                        else if (gameString.Substring(start, 1).Equals(","))
                        {
                            attackers = attackers + ", ";
                        }
                        else { done = true; }
                    }
                    subStrings.Add(new Tuple<Int32, Int32, String>(regMatch.Index, (i + 1) * 2, players[i] + " is being attacked by " + attackers + "."));
                    regMatch = regMatch.NextMatch();

                }
                #endregion

                regMatch = Regex.Match(gameString, "Turn [0-9]+: " + players[i]);
                while (regMatch.Success)
                {
                    subStrings.Add(new Tuple<Int32, Int32, String>(regMatch.Index, 0, regMatch.Value));
                    regMatch = regMatch.NextMatch();
                }

                regMatch = Regex.Match(gameString, "(" + players[i] + " has conceded from the game.|" + players[i] + " has run out of time and has lost the match.|" + players[i] + " loses because of drawing a card with an empty library)");
                while (regMatch.Success)
                {
                    subStrings.Add(new Tuple<Int32, Int32, String>(regMatch.Index, 0, regMatch.Value));
                    regMatch = regMatch.NextMatch();
                }


            }
            #region blocks
            regMatch = Regex.Match(gameString, Regex.Escape("]") + " blocks @");
            while (regMatch.Success)
            {
                start = gameString.LastIndexOf("[", regMatch.Index) + 1;
                end = IndexOf(gameString, "@", start);
                var blocker = gameString.Substring(start, end - start);
                start = regMatch.Index + regMatch.Length + 1;
                end = IndexOf(gameString, "@", start);
                var attacker = gameString.Substring(start, end - start);
                subStrings.Add(new Tuple<Int32, Int32, String>(regMatch.Index, 0, blocker + " blocks " + attacker + "."));
                regMatch = regMatch.NextMatch();
            }
            #endregion

            subStrings.Sort((pair2, pair1) => pair2.Item1.CompareTo(pair1.Item1));
            #endregion


            // result
            regMatch = Regex.Match(gameString, " has run out of time and has lost the match");
            end = regMatch.Index;
            if (end > 0)
            {
                start = gameString.LastIndexOf("@P", end) + 2;
                var loser = gameString.Substring(start, end - start);
                if (loser.Equals(hero)) result = -10;
                if (loser.Equals(villain)) result = 10;
            }
            else
            {
                regMatch = Regex.Match(gameString, "( has conceded from the game| loses because of drawing a card with an empty library)");
                end = regMatch.Index;
                if (end > 0)
                {
                    start = gameString.LastIndexOf("@P", end) + 2;
                    var loser = gameString.Substring(start, end - start);
                    if (loser.Equals(hero)) result = -1;
                    if (loser.Equals(villain)) result = 1;
                }
                else if (result == 0)
                {
                    var lastSpell = gameString.LastIndexOf(" casts ");
                    var lastAttack = gameString.LastIndexOf(" is being attacked by");
                    var lastTrigger = gameString.LastIndexOf(" puts triggered ability");
                    var lastActivated = gameString.LastIndexOf(" activates an ability");
                    var lastAction = Math.Max(Math.Max(lastSpell, lastTrigger), lastActivated);
                    if (lastAction > lastAttack)
                    {
                        start = gameString.LastIndexOf("@P", lastAction) + 2;
                        var winner = gameString.Substring(start, lastAction - start);
                        if (winner.Equals(hero)) result = 1;
                        if (winner.Equals(villain)) result = -1;
                        if (Skip == GameResultPromptViewModel.SkipTypes.None)
                        {
                            var viewModel = new GameResultPromptViewModel(subStrings, hero, villain, result, gameString);
                            var prompt = new GameResultPrompt(viewModel);
                            var promptResult = prompt.ShowDialog();
                            result = viewModel.result;
                            Skip = viewModel.SkipSetting;
                        }
                        else if (Skip == GameResultPromptViewModel.SkipTypes.Ignore)
                        {
                            return null;
                        }
                    }
                    else
                    {
                        if (lastAttack > 0)
                        {
                            start = gameString.LastIndexOf("@P", lastAttack) + 2;
                            var loser = gameString.Substring(start, lastAttack - start);
                            if (loser.Equals(hero)) result = -1;
                            if (loser.Equals(villain)) result = 1;
                        }
                    }
                }

            }

            return new NewGame(gameId, result, onDraw, turn, preBoard, hMull, vMull, cardsPlayed[0], cardsPlayed[1], cardsPut[0], cardsPut[1], subStrings);
        }

        private static Int32 GetMull(String gameString, String player)
        {
            var mull = 7;
            var start = gameString.LastIndexOf(player + " mulligans to ");
            var end = -1;
            if (start > 0)
            {
                start = start + 14 + player.Length;
                end = gameString.IndexOf(' ', start);
                if (end - start == 1)
                {
                    mull = Convert.ToInt32(gameString.Substring(start, 1));
                }
                else
                {
                    var numberAsWord = gameString.Substring(start, end - start);

                    switch (numberAsWord)
                    {
                        case "one":
                            mull = 1;
                            break;
                        case "two":
                            mull = 2;
                            break;
                        case "three":
                            mull = 3;
                            break;
                        case "four":
                            mull = 4;
                            break;
                        case "five":
                            mull = 5;
                            break;
                        case "six":
                            mull = 6;
                            break;
                        default:
                            mull = 7;
                            break;
                    }
                }
            }

            return mull;
        }

        public static Int32 IndexOf(String source, String target, Int32 start)
        {
            if (start >= 0 && start < source.Length)
            {
                return source.IndexOf(target, start);
            }
            return -1;
        }
    }
}
