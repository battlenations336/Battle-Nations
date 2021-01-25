using GameCommon;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.ClientHandlers
{
    public class JoinBattleQueueHandler : GameMessageHandler
    {
        protected override void OnHandleMessage(Dictionary<byte, object> parameters, string debugMessage, int returnCode)
        {
            if (returnCode == (short)ReturnCode.OK)
            {
                Debug.LogFormat("Joined Queue");
                BattleMapCtrl.instance.BattleQueueJoined();
            }
            else
            {
                // Show the error dialog
                // ShowError(response.DebugMessage);
                Debug.LogFormat("Join Queue error {0}", debugMessage);
            }
        }
    }
}
