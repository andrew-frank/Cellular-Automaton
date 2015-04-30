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

    public class AutomatonController
    {
        ////////////////////////////////////////////
        #region Properties and ivars
        ////////////////////////////////////////////

        private Automaton _automaton;
        /// <summary>
        /// Holds a reference to the simulation model
        /// </summary>
        /// 
        public Automaton CurrentAutomaton
        {
            get  { return _automaton; }
        }


        private TimeSpan _timerInterval = AutomatonSettings.defaults.timerInterval;
        /// <summary>
        /// Holds the timer interval, set to default on start
        /// </summary>
        public TimeSpan TimerInterval
        {
            get  { return _timerInterval; }
            set  {
                _timerInterval = value;
                _timer.Interval = _timerInterval;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(Constants.PropertyChangedNameTimerInterval));
            }
        }


        private bool _isPaused = true;
        /// <summary>
        /// Holds the simulation run state: false if running, true if paused. 
        /// </summary>
        /// 
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
            _timer.Interval = _timerInterval;
            _timer.Tick += new EventHandler(TimerEvent);
            _timer.Start();
        }

        public void NewModel()
        {
            if (!_isPaused)
                _isPaused = true;

            _automaton = new Automaton();
        }

        public void ResetSim()
        {
            if (!_isPaused)
                IsPaused = true;

            _automaton.Reset();
        }

        #endregion


        ////////////////////////////////////////////
        #region Private
        ////////////////////////////////////////////

        /// <summary>
        /// TimerEvent(Object, EventArgs)
        /// 
        /// This function handles the timer tick. If the game is in the unpaused state it calls
        /// the LifeModel.Evaluate() method to iterate the model. It then increments the
        /// generation count. It's a static method so we pass in the 'this' pointer for the
        /// LifeSim instance that owns the timer in its Tag property. Useful little things, Tags.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        private void TimerEvent(Object sender, EventArgs e)
        {

            if (this != null) { //&& !this.IsPaused
                //if (!_currentAutomaton._lm.EvolutionHalted || ALSettings.Default.HaltOnStability == false) {
                    _automaton.Evaluate();
                    //this.Generation++;
                //} else {
                //    _currentAutomaton.IsPaused = true;
                //    if (_currentAutomaton._uiCallback != null) {
                //        _currentAutomaton._uiCallback();
                //    }
                //}
            } else if (this == null)
                throw (new System.InvalidOperationException("TimerEvent"));
        }


        #endregion


    }
}
