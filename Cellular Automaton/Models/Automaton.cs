using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cellular_Automaton.Common;

namespace Cellular_Automaton.Models
{
    public class Automaton
    {
        ////////////////////////////////////////////
        #region Properties and ivars
        ////////////////////////////////////////////

        public string Name { get; set; }

        /// <summary>
        /// The number of columns across the life grid
        /// </summary>
        public int Columns  {  get; private set; }

        /// <summary>
        /// The number of rows down the life grid
        /// </summary>
        public int Rows { get; private set; }


        /// <summary>
        /// 
        /// Array of Cells - used as data context in binding to the UI.
        /// 
        /// </summary>
        private Cell[,] _cellGrid = null;
        public Cell[,] CellGrid
        {
            get {
                if (_cellGrid != null)
                    return _cellGrid;
                else
                    throw (new System.InvalidOperationException("CellGrid Get"));
            }
        }

        /// <summary>
        /// CellBirths
        /// 
        /// Counts the number of cell births. Fires change notification.
        /// </summary>
        private int _cellBirths = 0;
        public int CellBirths
        {
            get  { return _cellBirths; }
            protected set  {
                _cellBirths = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(Constants.PropertyChangedNameCellBirths));
            }
        }

        /// <summary>
        /// CellDeaths
        /// 
        /// Counts the number of cell deaths. Fires change notification.
        /// </summary>
        private int _cellDeaths = 0;
        public int CellDeaths
        {
            get  { return _cellDeaths; }
            protected set {
                _cellDeaths = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(Constants.PropertyChangedNameCellDeaths));
            }
        }


        /// <summary>
        /// PeakPopulation
        /// 
        /// Tracks the maximum population of cells on the grid. Fires change notification.
        /// </summary>
        private int _peakPopulation = 0;
        public int PeakPopulation
        {
            get { return _peakPopulation; }
            protected set  {
                _peakPopulation = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(Constants.PropertyChangedNamePeakPopulation));
            }
        }

        /// <summary>
        /// Population
        /// 
        /// Tracks the current population of the grid at the close of each tick.
        /// Fires change notification.
        /// </summary>
        private int _population = 0;
        public int Population
        {
            get { return _population; }
            protected set {
                _population = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Population"));
            }
        }

        /// <summary>
        /// Evaluated
        /// 
        /// True if the model has been evaluated at least once, meaning that the data
        /// in _startingGrid is valid
        /// </summary>
        bool _evaluated = false;
        public bool Evaluated { get { return _evaluated; } }


        // holds the starting grid state so we can revert on demand
        private bool[,] _startingGrid = null;

        // holds the working grid, which is two cells larger than
        // the cell grid on each dimension, with the edge cells
        // being set up to wrap correctly according to the 
        // grid type. See BuildWorkGrid()
        private Cell[,] _workGrid = null;

        // holds the state of the last calculate grid, used to 
        // detect a halt
        private bool[,] _lastGrid = null;


        #endregion


        ////////////////////////////////////////////
        #region Public
        ////////////////////////////////////////////

        public event PropertyChangedEventHandler PropertyChanged;


        public Automaton()
        {
            InitArrays(AutomatonSettings.defaults.gridWidth, AutomatonSettings.defaults.gridHeight);
        }

        public Automaton(int rows, int columns)
        {
            InitArrays(rows, columns);
        }

        public void Reset()
        {
            _evaluated = false;
            CellBirths = 0;
            CellDeaths = 0;
            Population = 0;
            PeakPopulation = 0;

            for (int row = 0; row < this.Rows; row++) {
                for (int col = 0; col < this.Columns; col++)
                    _cellGrid[row, col].Alive = _startingGrid[row, col];
            }
        }


        /// <summary>
        /// Called by the LifeSim class to iterate through the array and apply the game
        /// rules once.
        /// </summary>
        //TODO //////////////////
        public void Evaluate()
        {
            
            bool[,] temp = new bool[Rows, Columns];

            int population = 0;

            if (!_evaluated) {
                _evaluated = true;
                for (int row = 0; row < Rows; row++) {
                    for (int col = 0; col < Columns; col++) {
                        _startingGrid[row, col] = _lastGrid[row, col] = _cellGrid[row, col].Alive;
                        if (_startingGrid[row, col])
                            _peakPopulation++;
                    }
                }
                PeakPopulation = _peakPopulation;
            }

            for (int row = 0; row < Rows; row++) {
                for (int col = 0; col < Columns; col++) {
                    _lastGrid[row, col] = _cellGrid[row, col].Alive;
                    int adj = (row + col) % 4; //CountAdjacent(row, col);
                    if (_cellGrid[row, col].Alive) {
                        if (adj == 2 || adj == 3)
                            temp[row, col] = true;
                        else
                            CellDeaths++;
                    } else {
                        if (adj == 3) {
                            temp[row, col] = true;
                            CellBirths++;
                        }
                    }
                }
            }


            for (int row = 0; row < Rows; row++) {
                for (int col = 0; col < Columns ; col++) {
                    if (temp[row, col] == true) {
                        _cellGrid[row, col].Alive = true;
                        population++;
                    } else
                        _cellGrid[row, col].Alive = false;
                }
            }

            Population = population;
            if (_peakPopulation < population)
                PeakPopulation = population;
            
            //if (AreEqualGrids(_cellGrid, _lastGrid))
            //    _evoHalted = true;
            
             
        }

        #endregion Public


        ////////////////////////////////////////////
        #region Private
        ////////////////////////////////////////////

        /// <summary>
        /// InitArrays(int, int)
        /// 
        /// Creates Cell array and assign values to related private members
        /// 
        /// </summary>
        /// <param name="columns">int, the width (columns) of the grid to be created</param>
        /// <param name="rows">int, the height (rows) of the grid to be created</param>
        private void InitArrays(int rows, int columns)
        {
            if (columns <= 0 || rows <= 0)
                throw (new System.ArgumentOutOfRangeException("InitArrays"));

            this.Columns = columns;
            this.Rows = rows;

            _cellGrid = new Cell[this.Rows, this.Columns];

            for (int row = 0; row < this.Rows; row++) {
                for (int col = 0; col < this.Columns; col++)
                    _cellGrid[row, col] = new Cell();
            }

            _startingGrid = new bool[this.Rows, this.Columns];
            _lastGrid = new bool[this.Rows, this.Columns];
        }

        #endregion
    }
}
