using System;
using System.Collections.Generic;
using MtgoReplayToolWpf.DataGridData;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MtgoReplayToolWpf.DataGridViewModels
{
    public class DataGridTimeChartViewModel : DataGridViewModelBase, INotifyPropertyChanged
    {
        private ObservableCollection<KeyValuePair<Int32, Double>> timeSeriesList = new ObservableCollection<KeyValuePair<Int32, Double>>();

        public ObservableCollection<KeyValuePair<Int32, Double>> TimeSeriesList
        {
            get
            {
                return timeSeriesList;
            }

            set
            {
                timeSeriesList = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(TimeSeriesList)));
            }
        }

        public List<String> LabelList { get; set; } 

        public event PropertyChangedEventHandler PropertyChanged;

        internal void SetSource(List<TimeSeriesData> dataGridTimeChartData)
        {
            TimeSeriesList.Clear();
            LabelList = new List<String>();
            var data = dataGridTimeChartData.FirstOrDefault() as TimeSeriesData;
            var lastDate = DateTime.MinValue;
            var tempList = new ObservableCollection<KeyValuePair<Int32, Double>>();
            Int32 counter = 0;
            if (data != null)
            {
                foreach (var kvp in data?.TimeSeriesList)
                {
                    if (kvp.Key.Month != lastDate.Month || kvp.Key.Year != lastDate.Year)
                    {
                        LabelList.Add(kvp.Key.Date.ToShortDateString());
                        lastDate = kvp.Key;
                    }
                    else
                    {
                        LabelList.Add(kvp.Key.Date.ToShortDateString());
                        //LabelList.Add(null);
                    }

                    tempList.Add(new KeyValuePair<Int32, Double>(counter++, kvp.Value));
                }
            }

            TimeSeriesList = new ObservableCollection<KeyValuePair<Int32, Double>>(tempList);
            OnPropertyChanged(nameof(LabelList));
        }

        protected void OnPropertyChanged(String name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
