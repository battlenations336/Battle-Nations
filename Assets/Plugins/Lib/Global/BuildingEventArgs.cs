using GameCommon;
using System;

namespace BNR
{
    public class BuildingEventArgs : EventArgs
    {
        public string BuildingType;
        public BuildingState OldState;
        public BuildingState NewState;

        public BuildingEventArgs(string BuildingType, BuildingState OldState, BuildingState NewState)
        {
            this.BuildingType = BuildingType;
            this.OldState = OldState;
            this.NewState = NewState;
        }
    }
}
