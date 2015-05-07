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
    public abstract class Rule : INotifyPropertyChanged
    {
        private static long RuleCount = 0;

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
        public abstract bool EvaluateNeighbours(bool[] neighbourhood);

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
            return this.Name;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Rule(string name, int neighbourhood) {
            Rule.RuleCount++;
            this.Name = name;
            this.NeighbourhoodCount = neighbourhood;
        }

        protected Rule() { }
    }

    static class RulesExtensions
    {
        public static int EvaluatedCellIndex(this bool[] neighbourhood) {
            switch (neighbourhood.GetLength(0)) {
                case 5:
                    return 3;
                case 9:
                    return 5;
                case 25:
                    return 13;
                default:
                    Debug.Assert(false, "Unspecified neighbourhood environment count");
                    throw new Exception("Unspecified neighbourhood environment count");
            }
        }
    }


    public sealed class CountRule : Rule 
    {
        private int[] _allowedAdjecentCount = {2 , 3};
        /// <summary>
        /// How many alive cells causes Rule to mark evaluated cell as alive
        /// </summary>
        public int[] AllowedAdjecentCount {
            get { return _allowedAdjecentCount; }
            set { _allowedAdjecentCount = value; }
        }


        public override bool EvaluateNeighbours(bool[] neighbourhood) {
            int adjecent = this.CountAdjecent(neighbourhood);
            foreach (int allowedCount in this.AllowedAdjecentCount) {
                if (allowedCount == adjecent)
                    return true;
            }

            return false;
        }

        private int CountAdjecent(bool[] neighbourhood) {
            int evaluatedCell = neighbourhood.EvaluatedCellIndex();
            int i = 0, counter = 0;
            foreach (bool state in neighbourhood) {
                if (state && i != evaluatedCell)
                    counter++;
                i++;
            }

            return counter;
        }
    }

    public sealed class PositionRule : Rule
    {
        private bool[] _allowedNeighbourhood;
        public bool[] AllowedNeighbourhood {
            get {
                Debug.Assert(_allowedNeighbourhood != null);

                if (_allowedNeighbourhood == null)
                    _allowedNeighbourhood = new bool[this.NeighbourhoodCount];
                else if (_allowedNeighbourhood.GetLength(0) != this.NeighbourhoodCount)
                    throw new Exception("Nonmatching  neighbourhood environment count");

                return _allowedNeighbourhood;
            }

            set {
                if (value.GetLength(0) != this.NeighbourhoodCount)
                    throw new Exception("Invalid  neighbourhood environment count");
                _allowedNeighbourhood = value;
            }
        }

        public override bool EvaluateNeighbours(bool[] neighbourhood) 
        {

            return false;
        }


    }



    //public enum RulePatterns
    //{
    //    Diagonal = 0,
    //    Border
    //}

    //public sealed class PatternRule : Rule
    //{

    //}

}
