using BNR;
using GameCommon;
using GameCommon.SerializedObjects;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.ClientHandlers
{
    public class AttackHandler : GameMessageHandler
    {
        protected override void OnHandleMessage(Dictionary<byte, object> parameters, string debugMessage, int returnCode)
        {
            if (returnCode == (short)ReturnCode.OK)
            {
                Debug.LogFormat("Attacked opponent");
                var attackDamage = MessageSerializerService.DeserializeObjectOfType<AttackDamage>(parameters[(byte)MessageParameterCode.Object]);
                BattleMapCtrl.instance.Update_PlayerAttack(attackDamage);
                Debug.LogFormat("Sending Request for attack, {0} {1}", GameData.Player.Name, attackDamage.BaseDamage);
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
