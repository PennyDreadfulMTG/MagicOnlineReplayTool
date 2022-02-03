using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MtgoReplayToolWpf.DataGridViews
{
    public class IndexToDateLabelConverter : IMultiValueConverter
    {
        public Object Convert(Object[] values, Type targetType, Object parameter, CultureInfo culture)
        {
            var index = values[0] as Double?;
            var labelList = values[1] as List<String>;

            if (index != null && labelList != null)
            {
                if (index % 1 == 0 && index < labelList.Count)
                {
                    try
                    {
                        return labelList.ElementAt((Int32)index);
                    }
                    catch
                    {
                        return null;
                    }
                }
            }

            return null;
        }

        public Object[] ConvertBack(Object value, Type[] targetTypes, Object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
