using BNR;
using GameCommon;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.ClientHandlers
{
    public class ChangeBuildingStateHandler : GameMessageHandler
    {
        protected override void OnHandleMessage(Dictionary<byte, object> parameters, string debugMessage, int returnCode)
        {
            if (returnCode == (short)ReturnCode.OK)
            {
                var buildingUpdate = MessageSerializerService.DeserializeObjectOfType<GameCommon.SerializedObjects.Packets.BuildingUpdate>(parameters[(byte)MessageParameterCode.Object]);

                Debug.LogFormat("State changed: {0} : {1} -> {2}", buildingUpdate.Name, buildingUpdate.OldState, buildingUpdate.NewState);
            }
            else
            {
                // Show the error dialog
                // ShowError(response.DebugMessage);
                Debug.LogFormat("State change error {0}", debugMessage);
            }
        }
    }
}
