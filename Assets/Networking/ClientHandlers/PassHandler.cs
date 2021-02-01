using GameCommon;
using GameCommon.SerializedObjects;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.ClientHandlers
{
    public class PassHandler : GameMessageHandler
    {
        protected override void OnHandleMessage(Dictionary<byte, object> parameters, string debugMessage, int returnCode)
        {
            if (returnCode == (short)ReturnCode.OK)
            {
                Debug.LogFormat("Player passed");
            }
            else
            {
                // Show the error dialog
                // ShowError(response.DebugMessage);
                Debug.LogFormat("Pass error {0}", debugMessage);
                BattleMapCtrl.instance.OpponentSurrendered();
            }
        }
    }
}
