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
        AlternativeDenial, //not both (nand)
        JoinDenial //nor
    };

    public class Automaton : INotifyPropertyChanged
    {
        ////////////////////////////////////////////
        #region Properties and ivars
        ////////////////////////////////////////////

        public static long AutomatonCount = 0;


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

        private string _name = "New automaton #" + Automaton.AutomatonCount;
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

        public int Columns  {  get; private set; }

        public int Rows { get; private set; }


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


        //true = model has been evaluated at least once==_startingGrid is valid
        bool _evaluated = false;
        public bool Evaluated { get { return _evaluated; } }


        private bool[,] _startingGrid = null;


        #endregion


        ////////////////////////////////////////////
        #region Public
        ////////////////////////////////////////////

        public event PropertyChangedEventHandler PropertyChanged;


        public Automaton()
        {
            InitArrays(AutomatonSettings.defaults.gridWidth, AutomatonSettings.defaults.gridHeight);
            Automaton.AutomatonCount++;
        }

        public Automaton(Automaton automaton) 
        {
            Automaton.AutomatonCount++;
            this.Rows = automaton.Rows;
            this.Columns = automaton.Columns;
            this.Name = automaton.Name;
            this.LogicalOperator = automaton.LogicalOperator;
            this.Rules = new ObservableCollection<Rule>();
            foreach (Rule rule in automaton.Rules) {
                this.Rules.Add(new Rule(rule));
            }
            this.NeighbourhoodEnvironment = automaton.NeighbourhoodEnvironment;
            InitArrays(automaton.Rows, automaton.Columns);
        }

        public Automaton(int rows, int columns)
        {
            Automaton.AutomatonCount++;
            InitArrays(rows, columns);
        }

        public override string ToString() {
            string logic = "";
            switch (this.LogicalOperator) {
                case RulesLogicalOperator.Conjunction:
                    logic = "Conjunction";
                    break;
                case RulesLogicalOperator.Disjunction:
                    logic = "Disjunction";
                    break;
                case RulesLogicalOperator.AlternativeDenial:
                    logic = "Alternative Denial";
                    break;
                case RulesLogicalOperator.JoinDenial:
                    logic = "Join Denial";
                    break;
                case RulesLogicalOperator.ExclusiveDisjunction:
                    logic = "Exclusive Disjunction";
                    break;
                default:
                    Debug.Assert(false, "Wrong logical operator");
                    break;
            }
            return this.Name + "    (" + this.NeighbourhoodEnvironment + "-point, " + logic + ")"; 
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

            if (config.Rows == this.Rows && config.Columns == this.Columns) {
                _startingGrid = config.Grid;
                for (int row = 0; row < this.Rows; row++) {
                    for (int col = 0; col < this.Columns; col++)
                        _cellGrid[row, col].Alive = _startingGrid[row, col];
                }

            } else {
                this.InitArrays(config.Rows, config.Columns);
                int xMax = Math.Min(config.Rows, this.Rows);
                int yMax = Math.Min(config.Columns, this.Columns);
                for (int row = 0; row < xMax ; row++) {
                    for (int col = 0; col < yMax ; col++)
                        _cellGrid[row, col].Alive = _startingGrid[row, col] = config.Grid[row, col];
                }
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


        public void Evaluate()
        {
            bool[,] temp = new bool[Rows, Columns];

            int population = 0;

            if (!_evaluated) {
                _evaluated = true;
                for (int row = 0; row < Rows; row++) {
                    for (int col = 0; col < Columns; col++) {
                        _startingGrid[row, col] = _cellGrid[row, col].Alive;
                        if (_startingGrid[row, col])
                            _peakPopulation++;
                    }
                }
                PeakPopulation = _peakPopulation;
            }

            bool[] results = new bool[Rules.Count];
            int tempCounter = 0;
            bool[,] neighb;
            bool[] neighbourhood;
            bool makeAlive = false;

            for (int row = 0; row < Rows; row++) {
                for (int col = 0; col < Columns; col++) {
                    makeAlive = false;
                    neighb = GetNeighbourhood(row, col);
                    neighbourhood = RulesExtensions.neighbourhood2dTo1dArray(this.NeighbourhoodEnvironment, neighb);

                    tempCounter = 0;
                    foreach (Rule rule in Rules) {
                        results[tempCounter] = rule.EvaluateNeighbours(neighbourhood);
                        tempCounter++;
                    }

                    bool firstRuleEval = true;
                    switch (this.LogicalOperator) {
                        case RulesLogicalOperator.Disjunction:
                            foreach (bool b in results) {
                                if (firstRuleEval) makeAlive = b;
                                else makeAlive = (b || makeAlive);
                                firstRuleEval = false;
                            }
                            break;
                        case RulesLogicalOperator.Conjunction:
                            foreach (bool b in results) {
                                if (firstRuleEval) makeAlive = b;
                                else makeAlive = (b && makeAlive);
                                firstRuleEval = false;
                            }
                            break;
                        case RulesLogicalOperator.AlternativeDenial:
                            foreach (bool b in results) {
                                if (firstRuleEval) makeAlive = b;
                                else makeAlive = (b && makeAlive);
                                firstRuleEval = false;
                            }
                            makeAlive = !makeAlive;
                            break;
                        case RulesLogicalOperator.ExclusiveDisjunction:
                            foreach (bool b in results) {
                                if (firstRuleEval) makeAlive = b;
                                else makeAlive = (b ^ makeAlive);
                                firstRuleEval = false;
                            }
                            break;
                        case RulesLogicalOperator.JoinDenial:
                            foreach (bool b in results) {
                                if (firstRuleEval) makeAlive = b;
                                else makeAlive = (b || makeAlive);
                                firstRuleEval = false;
                            }
                            makeAlive = !makeAlive;
                            break;
                        default:
                            foreach (bool b in results) {
                                if (firstRuleEval) makeAlive = b;
                                else makeAlive = (b || makeAlive);
                                firstRuleEval = false;
                            }
                            Debug.Assert(false, "Unspecified logical operator");
                            break;
                    }

                    if (_cellGrid[row, col].Alive) {
                        if (makeAlive)
                            temp[row, col] = true;
                        else
                            CellDeaths++;
                    } else {
                        if (makeAlive) {
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
        }


        #endregion Public

        ////////////////////////////////////////////
        #region Private
        ////////////////////////////////////////////

        private void InitArrays(int rows, int columns)
        {
            if (columns <= 0 || rows <= 0)
                throw (new System.ArgumentOutOfRangeException("Automaton: InitArrays()"));

            this.Columns = columns;
            this.Rows = rows;

            _cellGrid = new Cell[this.Rows, this.Columns];

            for (int row = 0; row < this.Rows; row++) {
                for (int col = 0; col < this.Columns; col++)
                    _cellGrid[row, col] = new Cell();
            }

            _startingGrid = new bool[this.Rows, this.Columns];
        }


        private bool[,] GetNeighbourhood(int row, int col) 
        {
            int size = RulesExtensions.gridWidthForNeighbourhoodCount(this.NeighbourhoodEnvironment);
            Debug.Assert(size > 0 && size%2!=0);

            bool[,] neighbourhood = new bool[size, size];
            for (int i = -(size/2), x=0 ; i <= neighbourhood.GetLength(0)/2 ; i++, x++) {
                for(int j = -(size/2), y=0 ; j <= neighbourhood.GetLength(1)/2 ; j++, y++) {
                    if (row + i >= 0 && col + j >= 0 && row + i < _cellGrid.GetLength(0) && col + j < _cellGrid.GetLength(1))
                        neighbourhood[x, y] = _cellGrid[row + i, col + j].Alive;
                }
            }

            return neighbourhood;
        }


        #endregion
    }
}
