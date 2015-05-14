using Cellular_Automaton.Common;
using Cellular_Automaton.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace Cellular_Automaton.Controllers
{

    public class AutomatonController : INotifyPropertyChanged
    {
        ////////////////////////////////////////////
        #region Properties and ivars
        ////////////////////////////////////////////

        private Automaton _automaton;
        public Automaton CurrentAutomaton
        {
            get  { return _automaton; }
            set { _automaton = value; }
        }


        private TimeSpan _timerSpan = AutomatonSettings.defaults.timerInterval;
        public TimeSpan timerSpan
        {
            get  { 
                if(_timerSpan == null)
                    _timerSpan = AutomatonSettings.defaults.timerInterval;
                return _timerSpan; 
            }
            set  {
                _timerSpan = value;
                _timer.Interval = _timerSpan;
            }
        }

        private int _timerInterval = 0;
        public int TimerInterval
        {
            get {
                return _timerInterval;
            }
            set {
                _timerInterval = value;
                this.timerSpan = new TimeSpan(_timerInterval);
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(Constants.PropertyChangedNameTimerInterval));
            }
        }

        private int _generation = 0;
        public int Generation
        {
            get  {  return _generation; }
            protected set {
                _generation = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Generation"));
            }
        }


        private bool _isPaused = true;
        public bool IsPaused
        {
            get { return _isPaused; }
            set { _isPaused = value; }
        }


        private DispatcherTimer _timer = null;


        #endregion 


        ////////////////////////////////////////////
        #region Public
        ////////////////////////////////////////////

        public event PropertyChangedEventHandler PropertyChanged;


        public AutomatonController()
        {
            _automaton = new Automaton();
            _timer = new DispatcherTimer();
            _timerSpan = this.timerSpan;
            _timer.Interval = _timerSpan;
            _timer.Tick += new EventHandler(TimerEvent);
            _timer.Start();
        }

        public AutomatonController(Automaton automaton) 
        {
            _automaton = automaton;
            _timer = new DispatcherTimer();
            _timerSpan = this.timerSpan;
            _timer.Interval = _timerSpan;
            _timer.Tick += new EventHandler(TimerEvent);
            _timer.Start();
        }


        public void NewModel() {
            if (!_isPaused)
                _isPaused = true;

            _automaton = new Automaton();
            this.Generation = 0;
        }

        public void Reset()
        {
            if (!_isPaused)
                IsPaused = true;

            _automaton.Reset();
            this.Generation = 0;
        }

        public void SetNewConfiguration(AutomatonConfiguration config) 
        {
            this.IsPaused = true;
            this.CurrentAutomaton.SetNewConfiguration(config);
        }

        #endregion


        ////////////////////////////////////////////
        #region Private
        ////////////////////////////////////////////

        private void TimerEvent(Object sender, EventArgs e)
        {
            if (this != null && !this.IsPaused) { //
                _automaton.Evaluate();
                this.Generation++;
            } else if (this == null)
                throw (new System.InvalidOperationException("TimerEvent"));
        }

        #endregion

    }
}
