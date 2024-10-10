using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameTheory.AllClass
{
    public class CriterionResult
    {
        public string CriterionName { get; set; }
        public int OptimalStrategyIndex { get; set; }

        public CriterionResult(string name, int index)
        {
            CriterionName = name;
            OptimalStrategyIndex = index;
        }
    }

}
