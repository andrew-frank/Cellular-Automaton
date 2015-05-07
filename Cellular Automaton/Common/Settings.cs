using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Cellular_Automaton.Common
{
    public class AutomatonSettings
    {
        private static AutomatonSettings _defaults;
        public static AutomatonSettings defaults
        {
            get { 
                if(_defaults==null)
                    _defaults = new AutomatonSettings();
                return _defaults;
            }

            set  { _defaults = value; }
        }

        public TimeSpan timerInterval = new TimeSpan(TimeSpan.TicksPerSecond/2);

        public int gridWidth = 40;
        public int gridHeight = 40;

        public Color GridBackground = Colors.Black;
        public Color ActiveCellColor = Colors.White;
    }
}
