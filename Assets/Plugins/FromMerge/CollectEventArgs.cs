
using System;

namespace BNR
{
  public class CollectEventArgs : EventArgs
  {
    public BuildingEntity Building;
    public ArmyUnit ArmyUnit;

    public CollectEventArgs(BuildingEntity building)
    {
      this.Building = building;
    }

    public CollectEventArgs(ArmyUnit unit)
    {
      this.ArmyUnit = unit;
    }
  }
}
