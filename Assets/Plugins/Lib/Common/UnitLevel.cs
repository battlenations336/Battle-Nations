using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNR
{
    public class UnitLevel
    {
        public string Name { get; set; }
        public double Level { get; set; }
        public int Cost { get; set; }

        public UnitLevel(string name, double level, int cost)
        {
            Name = name;
            Level = level;
            Cost = cost;
        }
    }
}
