using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cellular_Automaton.Models
{
    public class Cell : INotifyPropertyChanged
    {
        private bool _alive;
        public bool Alive {
            get { return _alive; }
            set {
                _alive = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(Common.Constants.PropertyChangedNameIsAlive));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
