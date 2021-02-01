
using GameCommon;
using System;

namespace BNR
{
    public class EncounterEventArgs : EventArgs
    {
        public string WorldId;
        public EncounterArmy Encounter;
        public TransMode Mode;
        public bool ForceStart;

        public EncounterEventArgs(
          string WorldId,
          EncounterArmy Encounter,
          TransMode mode,
          bool forceStart)
        {
            this.WorldId = WorldId;
            this.Encounter = Encounter;
            this.Mode = mode;
            this.ForceStart = forceStart;
        }
    }
}
