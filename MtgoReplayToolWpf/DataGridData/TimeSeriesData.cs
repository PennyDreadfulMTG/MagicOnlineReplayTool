using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MtgoReplayToolWpf.DataGridData
{
    public class TimeSeriesData
    {
        public List<KeyValuePair<DateTime, Double>> TimeSeriesList;

        public TimeSeriesData()
        {
        }

        public TimeSeriesData(List<KeyValuePair<DateTime, Double>> winRate)
        {
            TimeSeriesList = winRate;
        }
    }
}
