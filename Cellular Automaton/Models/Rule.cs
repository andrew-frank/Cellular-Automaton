using Cellular_Automaton.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cellular_Automaton.Models
{
    public enum RuleType
    {
        Count = 0,
        Match
    }

    public class Rule : INotifyPropertyChanged
    {
        private static long RuleCount = 0;
        //////////////////////////////////


        private RuleType _type = RuleType.Count;
        public RuleType Type {
            get { return _type; }
            set {
                _type = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(Constants.PropertyChangedNameRuleType));
            }
        }


        private bool _ruleResult = true;
        public bool RuleResult { get { return _ruleResult; } set { _ruleResult = value; } }


        private int _neighbourhoodCount = 4;
        public int NeighbourhoodCount {
            get { return _neighbourhoodCount; }
            set {
                Debug.Assert(value == 4 || value == 8 || value == 24);
                if (value == 4 || value == 8 || value == 24)
                    _neighbourhoodCount = value;
            }
        }



        /// <summary>
        /// Reads from and neighbourhood array state of the evaluated cell, its neighbours and
        /// applies the rule returning the new state of the middle cell
        /// </summary>
        /// <param name="neighbourhood">Array of bool states of neighbourhood (evaluated cell + neighbours)</param>
        /// <returns>New state of the middle cell</returns>
        public bool EvaluateNeighbours(bool[] neighbourhood) {
            switch (this.Type) {
                case RuleType.Count:
                    return EvaluateCountNeighbours(neighbourhood);
                case RuleType.Match:
                    return EvaluateMatchNeighbours(neighbourhood);
                default:
                    Debug.Assert(false, "Invalid type");
                    return false;
            }
        }

        private string _name;
        public string Name
        {
            get { 
                if(_name == null)
                    return "Unnamed rule #" + Rule.RuleCount;
                return _name;
            }

            set {
                _name = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(Constants.PropertyChangedNameRuleName));
            }
        }

        public override string ToString() {
            string type = "Count";
            if (this.Type == RuleType.Match)
                type = "Match";
            return this.Name + "  Type: " + type;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Rule(string name, int neighbourhood, RuleType type) {
            Rule.RuleCount++;
            this.Name = name;
            this.NeighbourhoodCount = neighbourhood;
            this.Type = type;
        }

        public Rule(Rule rule) 
        {
            Rule.RuleCount++;
            this.Name = rule.Name;
            this.NeighbourhoodCount = rule.NeighbourhoodCount;
            this.AllowedAdjecentCount = new int[rule.AllowedAdjecentCount.GetLength(0)];
            int i = 0;
            foreach (int x in rule.AllowedAdjecentCount) {
                this.AllowedAdjecentCount[i] = x;
                i++;
            }

            i = 0;
            this.AllowedNeighbourhood = new bool[rule.AllowedNeighbourhood.GetLength(0)];
            foreach (bool x in rule.AllowedNeighbourhood) {
                this.AllowedNeighbourhood[i] = x;
                i++;
            }

            this.Type = rule.Type;
        }

        protected Rule() { }


        #region Count rule
        private int[] _allowedAdjecentCount = { 2, 3 };
        /// <summary>
        /// How many alive cells causes Rule to mark evaluated cell as alive
        /// </summary>
        public int[] AllowedAdjecentCount {
            get { return _allowedAdjecentCount; }
            set { _allowedAdjecentCount = value; }
        }

        public bool EvaluateCountNeighbours(bool[] neighbourhood) {
            int adjecent = this.CountAdjecent(neighbourhood);
            foreach (int allowedCount in this.AllowedAdjecentCount) {
                if (allowedCount == adjecent)
                    return RuleResult; //true;
            }

            return !RuleResult;
        }

        private int CountAdjecent(bool[] neighbourhood) {
            int i = 0, counter = 0;
            foreach (bool state in neighbourhood) {
                if (state && isNeighbourValidAtIndex(i))
                    counter++;
                i++;
            }

            return counter;
        }


        #endregion

        #region Match rule

        private bool[] _allowedNeighbourhood;
        public bool[] AllowedNeighbourhood {
            get {
                if (_allowedNeighbourhood == null)
                    _allowedNeighbourhood = new bool[RulesExtensions.neighbourhood1DArrLengthFromNeighbourhoodCount(this.NeighbourhoodCount)];
                else if (_allowedNeighbourhood.GetLength(0) != RulesExtensions.neighbourhood1DArrLengthFromNeighbourhoodCount(this.NeighbourhoodCount))
                    throw new Exception("Nonmatching  neighbourhood environment count");

                return _allowedNeighbourhood;
            }

            set {
                if (value.GetLength(0) != RulesExtensions.neighbourhood1DArrLengthFromNeighbourhoodCount(this.NeighbourhoodCount))
                    throw new Exception("Invalid  neighbourhood environment count");
                _allowedNeighbourhood = value;
            }
        }

        public bool EvaluateMatchNeighbours(bool[] neighbourhood) {
            int i = 0;
            foreach (bool state in neighbourhood) {
                if (state != _allowedNeighbourhood[i]) {
                    if (isNeighbourValidAtIndex(i))
                        return RuleResult;//false;
                }
                i++;
            }
            return !RuleResult; //true;
        }

        private bool isNeighbourValidAtIndex(int index)  {
            switch (this.NeighbourhoodCount) {
                case 4:
                case 8:
                    if (index == 4)
                        return false;
                    break;
                case 24:
                    if (index == 12)
                        return false;
                    break;
                default:
                    Debug.Assert(false, "Unspecified neighbourhood environment count");
                    throw new Exception("Unspecified neighbourhood environment count");
            }

            if (this.NeighbourhoodCount != 4)
                return true;
            return !(index == 0 || index == 2 || index == 6 || index == 8);
        }

        #endregion
    }

    public static class RulesExtensions
    {
        public static int neighbourhood1DArrLengthFromNeighbourhoodCount(int neighbourhoodCount) {
            switch (neighbourhoodCount) {
                case 4:
                case 8:
                    return 9;
                case 24:
                    return 25;
                default:
                    Debug.Assert(false, "Wrong neighbourhood count");
                    return -1;
            }
        }

        public static bool[] neighbourhood2dTo1dArray(int neighbourhoodCount, bool[,] neighbourhood) {
            bool[] arr = new bool[neighbourhood.GetLength(0) * neighbourhood.GetLength(1)];

            int tempCounter = 0;
            int gridWidth = RulesExtensions.gridWidthForNeighbourhoodCount(neighbourhoodCount);

            for (int k = 0; k < neighbourhood.GetLength(0); k++) {
                for (int l = 0; l < neighbourhood.GetLength(1); l++) {
                    arr[tempCounter] = neighbourhood[k, l];
                    tempCounter++;
                }
            }
            return arr;
        }

        public static bool[] cellGridTo1dBoolArr(int neighbourhoodCount, Cell[,] grid) {
            bool[,] arr1 = (RulesExtensions.cellGridToBool2dArr(neighbourhoodCount, grid));
            return RulesExtensions.neighbourhood2dTo1dArray(neighbourhoodCount, arr1);
        }

        public static bool[,] cellGridToBool2dArr(int neighbourhoodCount , Cell[,] grid) {
            int gridWidth = RulesExtensions.gridWidthForNeighbourhoodCount(neighbourhoodCount);
            bool[,] arr = new bool[gridWidth, gridWidth];
            for (int i = 0; i < grid.GetLength(0); i++) {
                for (int j = 0; j < grid.GetLength(1); j++) {
                    arr[i, j] = grid[i, j].Alive;
                }
            }
            return arr;
        }


        public static bool[,] neighbourhood1dTo2dArray(int neighbourhoodCount, bool[] neighbourhood) {
            int width = RulesExtensions.gridWidthForNeighbourhoodCount(neighbourhoodCount);
            bool[,] arr = new bool[width,width];
            int tempCounter = 0;
            for (int i = 0; i < width; i++) {
                for (int j = 0; j < width; j++) {
                    arr[i, j] = neighbourhood[tempCounter];
                    tempCounter++;
                }
            }

            return arr;
        }

        public static int gridWidthForNeighbourhoodCount(int neighbourhoodCount) {
            switch (neighbourhoodCount) {
                case 4:
                case 8:
                    return 3;
                case 24:
                    return 5;
                default:
                    Debug.Assert(false, "Wrong neighbourhood count");
                    break;
            }
            return -1;
        }
    }
}
