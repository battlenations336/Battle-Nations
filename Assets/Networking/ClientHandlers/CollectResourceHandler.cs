using BNR;
using GameCommon;
using GameCommon.SerializedObjects.Packets;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.ClientHandlers
{
    public class CollectResourceHandler : GameMessageHandler
    {
        protected override void OnHandleMessage(Dictionary<byte, object> parameters, string debugMessage, int returnCode)
        {
            if (returnCode == (short)ReturnCode.OK)
            {
                var storage = MessageSerializerService.DeserializeObjectOfType<GameCommon.SerializedObjects.Storage>(parameters[(byte)MessageParameterCode.Object]);
                Debug.LogFormat("Resource collected: {0}", storage.ToString());
                //GameData.Player.UpdateStorage(storage);
            }
            else
            {
                // Show the error dialog
                // ShowError(response.DebugMessage);
                Debug.LogFormat("Collection error {0}", debugMessage);
            }
        }
    }
}
