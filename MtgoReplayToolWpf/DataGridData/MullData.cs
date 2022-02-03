using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MtgoReplayToolWpf.DataGridData
{
    public class MullData
    {
        public String Header { get; set; }
        public Int32 Number { get; set; }
        public Double PrevalenceAt { get; set; }
        public Double WinrateAt { get; set; }
        public Double PrevalenceAtLower { get; set; }
        public Double WinrateAtLower { get; set; }
        public Double PrevalenceAtHigher { get; set; }
        public Double WinrateAtHigher { get; set; }
        public Double GamesAt { get; set; }
        public Double GamesWonAt { get; set; }
        public Double GamesAtLower { get; set; }
        public Double GamesWonAtLower { get; set; }
        public Double GamesAtHigher { get; set; }
        public Double GamesWonAtHigher { get; set; }

        public MullData(String header, Double[] source)
        {
            Header = Header;
            Number = (Int32) source[0];
            PrevalenceAt = source[1];
            WinrateAt = source[2];
            PrevalenceAtHigher = source[3];
            WinrateAtHigher = source[4];
            PrevalenceAtLower = source[5];
            WinrateAtLower = source[6];
            GamesAt = source[7];
            GamesWonAt = source[8];
            GamesAtHigher = source[9];
            GamesWonAtHigher = source[10];
            GamesAtLower = source[11];
            GamesWonAtLower = source[12];
        }

        public MullData()
        {
        }
    }
}
