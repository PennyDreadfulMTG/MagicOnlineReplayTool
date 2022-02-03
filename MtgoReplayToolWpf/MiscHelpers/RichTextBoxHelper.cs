using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace MtgoReplayToolWpf.MiscHelpers
{
    public static class RichTextBoxHelper
    {
        public static void FillWithShortLog(RichTextBox richTextBox, List<Tuple<Int32, Int32, String>> subStrings)
        {
            richTextBox.Document.Blocks.Clear();
            foreach (var myString in subStrings)
            {
                var paragraph = new Paragraph();
                paragraph.FontFamily = new FontFamily("Arial");
                paragraph.KeepTogether = true;
                paragraph.KeepWithNext = true;
                paragraph.Margin = new Thickness(0);

                switch (myString.Item2)
                {
                    case 0:
                        paragraph.TextAlignment = TextAlignment.Left;
                        paragraph.Foreground = Brushes.Black;
                        paragraph.FontWeight = FontWeights.Bold;
                        break;
                    case 1:
                        paragraph.TextAlignment = TextAlignment.Left;
                        paragraph.Foreground = Brushes.DarkRed;
                        break;

                    case 2:
                        paragraph.TextAlignment = TextAlignment.Left;
                        paragraph.Foreground = Brushes.Blue;
                        paragraph.FontStyle = FontStyles.Italic;
                        break;
                    case 3:
                        paragraph.TextAlignment = TextAlignment.Left;
                        paragraph.Foreground = Brushes.Blue;
                        break;
                    case 4:
                        paragraph.TextAlignment = TextAlignment.Left;
                        paragraph.Foreground = Brushes.DarkRed;
                        paragraph.FontStyle = FontStyles.Italic;
                        break;
                }
                paragraph.Inlines.Add(myString.Item3);
                richTextBox.Document.Blocks.Add(paragraph);
            }
        }

        public static void FillWithFullLog(RichTextBox richTextBox, String gameString)
        {
            richTextBox.Document.Blocks.Clear();
            var paragraph = new Paragraph();
            var cleanString = Regex.Replace(gameString, @"\p{C}+", String.Empty);
            paragraph.Inlines.Add(cleanString);
            richTextBox.Document.Blocks.Add(paragraph);
        }
    }
}
