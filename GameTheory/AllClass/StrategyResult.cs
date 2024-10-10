using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameTheory.AllClass
{
    public class StrategyResult
    {
        public string CriterionName { get; set; } // Название критерия (Вальда, Гурвица и т.д.)
        public string Strategy { get; set; } // Название выбранной стратегии
        public double Value { get; set; } // Значение выбранной стратегии (для диаграммы)
    }
}
