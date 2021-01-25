using BNR;
using GameCommon;
using GameCommon.SerializedObjects;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.ClientHandlers
{
    public class DamageHandler : GameMessageHandler
    {
        protected override void OnHandleMessage(Dictionary<byte, object> parameters, string debugMessage, int returnCode)
        {
            if (returnCode == (short)ReturnCode.OK)
            {
                var attackDamage = MessageSerializerService.DeserializeObjectOfType<AttackDamage>(parameters[(byte)MessageParameterCode.Object]);
                BattleMapCtrl.instance.Update_EnemyAttack(attackDamage);
                Debug.LogFormat("Damage incoming from opponent, {0} {1}", GameData.Player.Name, attackDamage.BaseDamage);
            }
            else
            {
                // Show the error dialog
                // ShowError(response.DebugMessage);
                Debug.LogFormat("Damage error {0}", debugMessage);
            }
        }
    }
}
