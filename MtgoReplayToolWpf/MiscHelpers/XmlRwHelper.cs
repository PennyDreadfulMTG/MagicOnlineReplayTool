using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml;

namespace MTGOReplayToolWpf
{
    public static class XmlRwHelper
    {
        [Obsolete]
        public static void WriteMatch(Match match, XmlWriter writer)
        {
            writer.WriteStartElement("match");
            writer.WriteAttributeString("id", match.Id.ToString());
            writer.WriteAttributeString("hero", match.Hero);
            writer.WriteAttributeString("villain", match.Villain);
            writer.WriteAttributeString("result", match.Result.ToString());
            writer.WriteAttributeString("hDeck", match.HDeck);
            writer.WriteAttributeString("vDeck", match.VDeck);

            // cards
            writer.WriteStartElement("hCards");

            foreach (var card in match.HCards)
            {
                writer.WriteAttributeString("card", card);
            }

            writer.WriteEndElement();

            writer.WriteStartElement("vCards");

            foreach (var card in match.VCards)
            {
                writer.WriteAttributeString("card", card);
            }

            writer.WriteEndElement();

            // games
            foreach (var game in match.Games)
            {
                WriteGame(game, writer);
            }

            writer.WriteEndElement();
        }

        public static void WriteMatch(NewMatch match, XmlWriter writer)
        {
            writer.WriteStartElement("match");
            writer.WriteAttributeString("id", match.Id);
            writer.WriteAttributeString("hero", match.Hero);
            writer.WriteAttributeString("villain", match.Villain);
            writer.WriteAttributeString("result", match.Result.ToString());
            writer.WriteAttributeString("hDeck", match.HDeck);
            writer.WriteAttributeString("vDeck", match.VDeck);
            writer.WriteAttributeString("date", match.Date.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("manuallyEdited", match.ManuallyEdited.ToString(CultureInfo.InvariantCulture));

            // cards
            writer.WriteStartElement("hCards");

            foreach (var card in match.HCardsPlayed)
            {
                writer.WriteElementString("card", card);
            }

            writer.WriteEndElement();

            writer.WriteStartElement("vCards");

            foreach (var card in match.VCardsPlayed)
            {
                writer.WriteElementString("card", card);
            }

            writer.WriteEndElement();

            writer.WriteStartElement("vCardsPut");

            foreach (var card in match.VCardsPut)
            {
                writer.WriteElementString("card", card);
            }

            writer.WriteEndElement();

            writer.WriteStartElement("hCardsPut");

            foreach (var card in match.HCardsPut)
            {
                writer.WriteElementString("card", card);
            }

            writer.WriteEndElement();

            // games
            foreach (var game in match.Games)
            {
                WriteGame(game, writer);
            }
            writer.WriteEndElement();
        }

        [Obsolete]
        public static void WriteGame(Game game, XmlWriter writer)
        {
            writer.WriteStartElement("game");
            writer.WriteAttributeString("id", game.Id.ToString());
            writer.WriteAttributeString("onDraw", game.OnDraw.ToString());
            writer.WriteAttributeString("preBoard", game.PreBoard.ToString());
            writer.WriteAttributeString("result", game.Result.ToString());
            writer.WriteAttributeString("turn", game.Turn.ToString());
            writer.WriteAttributeString("hMull", game.HMull.ToString());
            writer.WriteAttributeString("vMull", game.VMull.ToString());

            writer.WriteStartElement("hCards");

            foreach (var card in game.HCards)
            {
                writer.WriteElementString("card", card);
            }

            writer.WriteEndElement();

            writer.WriteStartElement("vCards");

            foreach (var card in game.VCards)
            {
                writer.WriteElementString("card", card);
            }

            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        public static void WriteGame(NewGame game, XmlWriter writer)
        {
            writer.WriteStartElement("game");
            writer.WriteAttributeString("id", game.Id.ToString());
            writer.WriteAttributeString("onDraw", game.OnDraw.ToString());
            writer.WriteAttributeString("preBoard", game.PreBoard.ToString());
            writer.WriteAttributeString("result", game.Result.ToString());
            writer.WriteAttributeString("turn", game.Turn.ToString());
            writer.WriteAttributeString("hMull", game.HMull.ToString());
            writer.WriteAttributeString("vMull", game.VMull.ToString());

            writer.WriteStartElement("hCards");

            foreach (var card in game.HCardsPlayed)
            {
                writer.WriteElementString("card", card);
            }

            writer.WriteEndElement();
            
            writer.WriteStartElement("vCards");

            foreach (var card in game.VCardsPlayed)
            {
                writer.WriteElementString("card", card);
            }

            writer.WriteEndElement();

            writer.WriteStartElement("vCardsPut");

            foreach (var card in game.VCardsPut)
            {
                writer.WriteElementString("card", card);
            }

            writer.WriteEndElement();

            writer.WriteStartElement("hCardsPut");

            foreach (var card in game.HCardsPut)
            {
                writer.WriteElementString("card", card);
            }

            writer.WriteEndElement();

            writer.WriteStartElement("gamelog");

            foreach (var entry in game.Gamelog)
            {
                writer.WriteStartElement("gamelogEntry");
                writer.WriteAttributeString("index",entry.Item1.ToString());
                writer.WriteAttributeString("type", entry.Item2.ToString());
                writer.WriteAttributeString("action", entry.Item3);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();

            writer.WriteEndElement();
            
        }

        public static NewGame ReadGame(XmlReader reader)
        {
            var game = new NewGame();

            while (reader.MoveToNextAttribute())
            {
                switch (reader.Name)
                {
                    case "id": game.Id = Convert.ToInt32(reader.Value); break;
                    case "onDraw": game.OnDraw = Convert.ToBoolean(reader.Value); break;
                    case "preBoard": game.PreBoard = Convert.ToBoolean(reader.Value); break;
                    case "turn": game.Turn = Convert.ToInt32(reader.Value); break;
                    case "hMull": game.HMull = Convert.ToInt32(reader.Value); break;
                    case "vMull": game.VMull = Convert.ToInt32(reader.Value); break;
                    case "result": game.Result = Convert.ToInt32(reader.Value); break;
                }
            }

            if (reader.IsEmptyElement)
            {
                return game;
            }


            while (reader.Read())
            {
                if (reader.IsStartElement())
                {
                    switch (reader.Name)
                    {
                        case "hCards": game.HCardsPlayed = ReadCards(reader); break;
                        case "vCards": game.VCardsPlayed = ReadCards(reader); break;
                        case "hCardsPut": game.HCardsPut = ReadCards(reader); break;
                        case "vCardsPut": game.VCardsPut = ReadCards(reader); break;
                        case "gamelog": game.Gamelog = ReadGamelog(reader); break;
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name.Equals("game"))
                {
                    return game;
                }
            }

            return null;
        }

        private static List<Tuple<Int32, Int32, String>> ReadGamelog(XmlReader reader)
        {
            var gamelog = new List<Tuple<Int32, Int32, String>>();

            if (reader.IsEmptyElement)
            {
                return gamelog;
            }

            while (reader.Read())
            {
                if (reader.IsStartElement())
                {
                    var index = 0;
                    var type = 0;
                    var action = String.Empty;

                    while (reader.MoveToNextAttribute())
                    {
                        switch (reader.Name)
                        {
                            case "index": index = Convert.ToInt32(reader.Value); break;
                            case "type": type = Convert.ToInt32(reader.Value); break;
                            case "action": action = reader.Value; break;
                        }
                    }
                    gamelog.Add(new Tuple<Int32, Int32, String>(index, type, action));
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name.Equals("gamelog"))
                {
                    return gamelog;
                }
            }
            return gamelog;
        }

        public static NewMatch ReadMatch(XmlReader reader)
        {
            if (reader.IsEmptyElement) return null;

            var match = new NewMatch(String.Empty);
            while (reader.MoveToNextAttribute())
            {
                switch (reader.Name)
                {
                    case "id": match.Id = reader.Value; break;
                    case "hero": match.Hero = reader.Value; break;
                    case "villain": match.Villain = reader.Value; break;
                    case "result": match.Result = Convert.ToInt32(reader.Value); break;
                    case "hDeck": match.HDeck = reader.Value; break;
                    case "vDeck": match.VDeck = reader.Value; break;
                    case "date": match.Date =  Convert.ToDateTime(reader.Value, CultureInfo.InvariantCulture); break;
                    case "manuallyEdited": match.ManuallyEdited = Convert.ToBoolean(reader.Value, CultureInfo.InvariantCulture); break;
                }
            }

            if (reader.IsEmptyElement)
            {
                return match;
            }
            
            while (reader.Read())
            {
                if (reader.IsStartElement())
                {
                    switch (reader.Name)
                    {
                        case "hCards": match.HCardsPlayed = ReadCards(reader); break;                            
                        case "vCards": match.VCardsPlayed = ReadCards(reader); break;
                        case "hCardsPut": match.HCardsPut = ReadCards(reader); break;
                        case "vCardsPut": match.VCardsPut = ReadCards(reader); break;
                        case "game": match.Games.Add(ReadGame(reader)); break;
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name.Equals("match"))
                {
                    return match;
                }
            }

            return null;
        }

        private static List<String> ReadCards(XmlReader reader)
        {
            var cardList = new List<String>();

            if (reader.IsEmptyElement)
            {
                return cardList;
            }


            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Text)
                {
                    cardList.Add(reader.Value);
                }
                else if (reader.NodeType == XmlNodeType.EndElement && (reader.Name.Equals("hCards") || reader.Name.Equals("vCards") || reader.Name.Equals("vCardsPut") || reader.Name.Equals("hCardsPut")))
                {
                    return cardList;
                }                
            }
            return cardList;
        }

        public static List<NewMatch> ReadAllMatches(XmlReader reader)
        {
            var returnValue = new List<NewMatch>();
            while (reader.Read())
            {
                if (reader.IsStartElement())
                {
                    if (reader.Name.Equals("match"))
                    {
                        try
                        {
                            returnValue.Add(ReadMatch(reader));
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                }
            }

            return returnValue;
        }

        private static void WriteAllMatches(List<NewMatch> allMatches, XmlWriter writer)
        {
            
                writer.WriteStartElement("mort_matches");

                foreach (var match in allMatches)
                {
                    try
                    {
                        WriteMatch(match, writer);
                    }
                    catch (Exception)
                    {
                        // TODO?
                        throw;
                    }
                }

                writer.WriteEndElement();            
        }

        public static String WriteAllMatches(List<NewMatch> allMatches)
        {
            var paths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.gz", SearchOption.TopDirectoryOnly).ToList();
            MoveFilesToOldBin(paths);

            allMatches = NewMatch.CleanMatches(allMatches);
            var myHash = NewMatch.GetMyHashCode(allMatches);

            //Stream stream = File.Open(myHash + ".bin", FileMode.Create);
            //BinaryFormatter bin = new BinaryFormatter();
            //bin.Serialize(stream, allMatches);
            //stream.Close();

            using (var writer = XmlWriter.Create(myHash + ".xml"))
            {
                writer.WriteStartDocument();
                WriteAllMatches(allMatches, writer);
                writer.WriteEndDocument();
                writer.Flush();
                writer.Close();
            }

            using (var xmlFileStream = File.OpenRead(myHash + ".xml"))
            {
                using (var compressedFileStream = File.Create(myHash + ".xml" + ".gz"))
                {
                    using (var compressionStream = new GZipStream(compressedFileStream,
                               CompressionMode.Compress))
                    {
                        xmlFileStream.CopyTo(compressionStream);
                    }
                }
            }
            File.Delete(myHash + ".xml");
            //XmlReader reader = XmlReader.Create(myHash + ".xml");
            //var readMatches = XmlRwHelper.ReadAllMatches(reader);
            //var differenceMatches = allMatches.Where(x => !readMatches.Any(y => y.id.Equals(x.id)));
            //writer = XmlWriter.Create("test" + ".xml");
            //writer.WriteStartDocument();
            //XmlRwHelper.WriteAllMatches(differenceMatches.ToList(), writer);
            //writer.WriteEndDocument();
            //writer.Flush();
            //writer.Close();
            //reader = XmlReader.Create("test" + ".xml");
            //readMatches = XmlRwHelper.ReadAllMatches(reader);

            return myHash + ".xml.gz";
        }

        private const string PATH_OLD_BIN = @"old_bin\";

        public static void MoveFilesToOldBin(List<String> filepaths)
        {
            if (!Directory.Exists(PATH_OLD_BIN))
            {
                Directory.CreateDirectory(PATH_OLD_BIN);
            }

            // delete paths
            foreach (var s in filepaths)
            {
                
                var file = Path.GetFileName(s);
                var moveTo = PATH_OLD_BIN + file;

                if (File.Exists(moveTo))
                {
                    File.Delete(moveTo);
                }

                File.Move(s, moveTo);
           }
        }

    }
}
