using GameCommon;
using GameCommon.SerializedObjects;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.ClientHandlers
{
    public class FirstBattleTurnHandler : GameMessageHandler
    {
        protected override void OnHandleMessage(Dictionary<byte, object> parameters, string debugMessage, int returnCode)
        {
            if (returnCode == (short)ReturnCode.OK)
            {
                Debug.LogFormat("Joined battle first");
                var opponent = MessageSerializerService.DeserializeObjectOfType<Opponent>(parameters[(byte)MessageParameterCode.Object]);

                BattleMapCtrl.instance.SetOpponent(opponent, true);
            }
            else
            {
                // Show the error dialog
                // ShowError(response.DebugMessage);
                Debug.LogFormat("Attack error {0}", debugMessage);
            }
        }
    }
}
