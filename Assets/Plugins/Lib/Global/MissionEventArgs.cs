using GameCommon;
using System;

namespace BNR
{
    public class MissionEventArgs : EventArgs
    {
        public string MissionId;
        public MissionState State;

        public MissionEventArgs(string MissionId, MissionState State)
        {
            this.MissionId = MissionId;
            this.State = State;
        }
    }
}
