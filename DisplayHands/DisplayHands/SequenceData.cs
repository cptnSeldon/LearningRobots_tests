using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace DisplayHands
{
    class SequenceData : INotifyPropertyChanged
    {

        #region ATTRIBUTES
        private string sequenceFromApp;
        public String SequenceFromApp
        {
            get { return sequenceFromApp;  }
            set { sequenceFromApp = value; OnPropertyChanged("Sequence updated"); }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion ATTRIBUTES


        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
