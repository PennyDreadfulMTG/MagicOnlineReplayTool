using MTGOReplayToolWpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace MtgoReplayToolWpf.DataModel
{
    public class MainData : INotifyPropertyChanged
    {
        private List<NewMatch> _matches;
        private Boolean _hasUnsavedChanges;

        public Boolean HasUnsavedChanges
        {
            get => _hasUnsavedChanges;
            set
            {
                _hasUnsavedChanges = value;
                RaisePropertyChanged(nameof(HasUnsavedChanges));
                RaisePropertyChanged(nameof(HasNoUnsavedChanges));
            }
        }

        public Boolean HasNoUnsavedChanges => !HasUnsavedChanges;

        public List<NewMatch> Matches
        {
            get => _matches;
            set
            {
                _matches = value;
                HasUnsavedChanges = true;
                RaisePropertyChanged(nameof(HasUnsavedChanges));
                RaisePropertyChanged(nameof(HasNoUnsavedChanges));
            }
        }

        public void LoadData(List<NewMatch> loadedMatches)
        {
            _matches = loadedMatches;
            HasUnsavedChanges = false;
            RaisePropertyChanged(nameof(HasUnsavedChanges));
            RaisePropertyChanged(nameof(HasNoUnsavedChanges));
        }

        public void SaveData()
        {
            XmlRwHelper.WriteAllMatches(Matches);
            HasUnsavedChanges = false;
            RaisePropertyChanged(nameof(HasUnsavedChanges));
            RaisePropertyChanged(nameof(HasNoUnsavedChanges));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
