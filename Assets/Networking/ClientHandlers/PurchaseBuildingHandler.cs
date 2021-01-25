using BNR;
using GameCommon;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.ClientHandlers
{
    public class PurchaseBuildingHandler : GameMessageHandler
    {
        protected override void OnHandleMessage(Dictionary<byte, object> parameters, string debugMessage, int returnCode)
        {
            if (returnCode == (short)ReturnCode.OK)
            {
                var purchase = MessageSerializerService.DeserializeObjectOfType<GameCommon.SerializedObjects.Packets.BuildEntity>(parameters[(byte)MessageParameterCode.Object]);
//                if (GameData.Compositions.ContainsKey(purchase.Name))
//                    PlayerMap.instance.CompletePurchase(purchase.Name, purchase.Id, new Vector3(purchase.PositionX, purchase.PositionY, 0), purchase.Flip, purchase.InitialSetup);
//                GameData.Player.UpdateStorage(purchase.Storage);
                Debug.LogFormat("Purchase completed {0}", purchase.Name);
            }
            else
            {
                // Show the error dialog
                // ShowError(response.DebugMessage);
                Debug.LogFormat("Purchase error {0}", debugMessage);
            }
        }
    }
}
