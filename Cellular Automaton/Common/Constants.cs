using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cellular_Automaton.Common
{
    public static class Constants
    {
        #region Property Changed Names

        public static readonly string PropertyChangedNameTimerInterval = "timerSpan";

        public static readonly string PropertyChangedNameCellBirths = "CellBirths";
        public static readonly string PropertyChangedNameCellDeaths = "CellDeaths";
        public static readonly string PropertyChangedNamePeakPopulation = "PeakPopulation";
        public static readonly string PropertyChangedNamePopulation = "Population";

        public static readonly string PropertyChangedNameIsAlive = "Alive";

        public static readonly string PropertyChangedNameRuleName = "RuleName";
        public static readonly string PropertyChangedNameRuleType = "RuleType";

        public static readonly string PropertyChangedNameAutomatonConfigName = "AutomatonConfigName";

        public static readonly string PropertyChangedNameAutomatonName = "Name";

        #endregion
    }
}
