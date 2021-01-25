
using System;

namespace BNR
{
    public class HurryEventArgs : EventArgs
    {
        public BuildingEntity Building;
        public ArmyUnit ArmyUnit;

        public HurryEventArgs(BuildingEntity building)
        {
            this.Building = building;
        }

        public HurryEventArgs(ArmyUnit unit)
        {
            this.ArmyUnit = unit;
        }
    }
}
