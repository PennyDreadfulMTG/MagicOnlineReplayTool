using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MtgoReplayToolWpf
{
    public class Format
    {
        public Format(String Name)
        {
            this.Name = Name;
            Decks = new List<Deck>();
        }

        public String Name { get; set; }

        public List<Deck> Decks { get; set; }
    }
}
