using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cellular_Automaton.Common;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace Cellular_Automaton.Models
{
    public enum RulesLogicalOperator {
        Conjunction = 0, //and
        Disjunction, //or
        ExclusiveDisjunction, //xor (either-or)
        AlternativeDenial //not both
    };

    public class Automaton : INotifyPropertyChanged
    {
        ////////////////////////////////////////////
        #region Properties and ivars
        ////////////////////////////////////////////

        public int NeighbourhoodEnvironment = 4;

        public RulesLogicalOperator LogicalOperator { get; set; }

        private ObservableCollection<Rule> _rules = new ObservableCollection<Rule>();
        public ObservableCollection<Rule> Rules {
            get { return _rules; }
            set {
                Debug.Assert(value != null);
                _rules = value;
            }
        }

        private string _name = "";
        public string Name {
            get {
                return _name;
            }
            set {
                _name = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(Constants.PropertyChangedNameAutomatonName));
            }
        }

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


        private Random _random = new Random();


        #endregion


        ////////////////////////////////////////////
        #region Public
        ////////////////////////////////////////////

        public event PropertyChangedEventHandler PropertyChanged;


        public Automaton()
        {
            InitArrays(AutomatonSettings.defaults.gridWidth, AutomatonSettings.defaults.gridHeight);
            this.Rules.Add(new Rule("Life rule", 4, RuleType.Count));
        }

        public Automaton(int rows, int columns)
        {
            InitArrays(rows, columns);
        }

        public override string ToString() 
        {
            return this.Name;
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

        public void SetNewConfiguration(AutomatonConfiguration config) 
        {
            _evaluated = false;
            CellBirths = 0;
            CellDeaths = 0;
            Population = 0;
            PeakPopulation = 0;

            Debug.Assert(config.Rows == this.Rows && config.Columns == this.Columns);
            _startingGrid = config.Grid;

            for (int row = 0; row < this.Rows; row++) {
                for (int col = 0; col < this.Columns; col++)
                    _cellGrid[row, col].Alive = _startingGrid[row, col];
            }
        }

        public bool[,] GetCurrentWorkGridConfiguration() 
        {
            Debug.Assert(_cellGrid != null);
            bool[,] grid = new bool[Rows, Columns];
            for (int i = 0; i < Rows; i++) {
                for (int j = 0; j < Columns; j++) {
                    grid[i, j] = _cellGrid[i, j].Alive;
                }
            }
            return grid;
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
                    int adj = _random.Next(0, 5);
                    adj = CountAdjacent(row, col); //_random.Next(0,5); // CountAdjacent(row, col); //

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

            //bool[] results = new bool[Rules.Count];
            //int tempCounter = 0;
            //bool[,] neighb;
            //bool[] neighbourhood;
            //bool makeAlive = false;

            //for (int row = 0; row < Rows; row++) {
            //    for (int col = 0; col < Columns; col++) {
            //        _lastGrid[row, col] = _cellGrid[row, col].Alive;

            //        makeAlive = false;
            //        neighb = GetNeighbourhood(row, col);
            //        neighbourhood = new bool[neighb.GetLength(0) * neighb.GetLength(1)];

            //        tempCounter = 0;
            //        for (int k = 0; k < neighb.GetLength(0); k++) {
            //            for (int l = 0; l < neighb.GetLength(1); l++) {
            //                neighbourhood[tempCounter] = neighb[k, l];
            //                tempCounter++;
            //            }
            //        }

            //        tempCounter = 0;
            //        foreach (Rule rule in Rules) {
            //            results[tempCounter] = rule.EvaluateNeighbours(neighbourhood);
            //            tempCounter++;
            //        }

            //        foreach (bool b in results) {
            //            makeAlive = (b || makeAlive);
            //        }

            //        if (_cellGrid[row, col].Alive) {
            //            if (makeAlive)
            //                temp[row, col] = true;
            //            else
            //                CellDeaths++;
            //        } else {
            //            if (makeAlive) {
            //                temp[row, col] = true;
            //                CellBirths++;
            //            }
            //        }
            //    }
            //}


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

            this.BuildWorkGrid();

            _startingGrid = new bool[this.Rows, this.Columns];
            _lastGrid = new bool[this.Rows, this.Columns];
        }


        /// <summary>
        /// BuildWorkGrid()
        /// 
        /// The work grid is two cells larger in each dimension, with the edge cells being
        /// used for controlling how the model wraps. This method builds the work grid off
        /// of the _cellGrid taking the GridType into account.
        /// </summary>
        private void BuildWorkGrid()
        {
            //_workGrid = new Cell[this.Rows + 4, this.Columns + 4];
            //for (int row = 0 ; row < this.Rows + 2; row++) {
            //    for (int col = 0 ; col < this.Columns + 2; col++) {
            //        _workGrid[row, col] = new Cell();
            //        if(row-2 > 0 && col-2 > 0)
            //            _workGrid[row, col] = _cellGrid[row - 2, col - 2];
            //    }
            //}

            //return;

            _workGrid = new Cell[this.Rows + 2, this.Columns + 2];
            for (int row = 0; row < this.Rows + 2; row++) {
                for (int col = 0; col < this.Columns + 2; col++) {
                    // Handle the corner conditions. A corner cell can only be
                    // alive in the case of a torus. In all other grid types it
                    // will be dead by one of the other edges. In the case of a
                    // torus it wraps to the opposite corner.
                    if (row == 0 && col == 0) {
                        _workGrid[row, col] = new Cell();
                    } else if (row == 0 && col == this.Columns + 1) {
                        _workGrid[row, col] = new Cell();
                    } else if (row == this.Rows + 1 && col == this.Columns + 1) {
                        _workGrid[row, col] = new Cell();
                    } else if (row == this.Rows + 1 && col == 0) {
                        _workGrid[row, col] = new Cell();
                    }
                        // Handle the non-corner edges. They are dead in the
                        // finite case, or the case where they lie along the top/bottom
                        // in an x cylinder grid, or the left/right in a y cylinder grid.
                        // Otherwise they wrap to the cell on the opposite side.
                      else if (row == 0) {
                        _workGrid[row, col] = new Cell();
                    } else if (row == this.Rows + 1) {
                        _workGrid[row, col] = new Cell();
                    } else if (col == 0) {
                        _workGrid[row, col] = new Cell();
                    } else if (col == this.Columns + 1) {
                        _workGrid[row, col] = new Cell();
                    } else
                        _workGrid[row, col] = _cellGrid[row - 1, col - 1];
                }
            }
        }


        private bool[,] GetNeighbourhood(int row, int col) 
        {
            int size = -1;
            if (NeighbourhoodEnvironment == 4) {
                size = 3;
            } else if (NeighbourhoodEnvironment == 8) {
                size = 3;
            } else if (NeighbourhoodEnvironment == 24) {
                size = 5;
            }

            Debug.Assert(size > 0 && size%2!=0);
            bool[,] neighbourhood = new bool[size, size];

            for (int i = -(size/2), x=0 ; i < neighbourhood.GetLength(0)/2 ; i++, x++) {
                for(int j = -(size/2), y=0 ; j < neighbourhood.GetLength(1)/2 ; j++, y++) {
                    if (row + i > 0 && col + i > 0 && row + i < _workGrid.GetLength(0) && col + i < _workGrid.GetLength(1)) {
                        bool alive = _workGrid[row + i, col + j].Alive;
                        neighbourhood[x, y] = alive;
                    }
                }
            }

            return neighbourhood;
        }


        /// <summary>
        /// CountAdjacent(int, int)
        /// 
        /// This function counts neighbors using the work grid, which returns
        /// the correct values for edge cells depending on the grid type. The work
        /// grid is two cells larger in both dimensions, with the outer cells used
        /// for correct wrapping of neighbor checks, as set up in BuildWorkGrid().
        /// The incoming coordinates are in the _cellGrid space, so this function
        /// shifts them down and right 1 cell to get to _workGrid space.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        private int CountAdjacent(int row, int column) {
            int count = 0;

            row++;
            column++;

            // upper left
            if (_workGrid[row - 1, column - 1].Alive)
                count++;

            if (_workGrid[row - 1, column].Alive)
                count++;

            // upper right
            if (_workGrid[row - 1, column + 1].Alive)
                count++;

            // left
            if (_workGrid[row, column - 1].Alive)
                count++;

            // right
            if (_workGrid[row, column + 1].Alive)
                count++;

            // lower left
            if (_workGrid[row + 1, column - 1].Alive)
                count++;

            // lower middle
            if (_workGrid[row + 1, column].Alive)
                count++;

            // lower right
            if (_workGrid[row + 1, column + 1].Alive)
                count++;

            return count;
        }


        #endregion
    }
}
