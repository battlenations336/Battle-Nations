using BNR;
using GameCommon;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.ClientHandlers
{    
    public class CompleteMissionHandler : GameMessageHandler
    {
        protected override void OnHandleMessage(Dictionary<byte, object> parameters, string debugMessage, int returnCode)
        {
            if (returnCode == (short)ReturnCode.OK)
            {
                var missionUpdated = MessageSerializerService.DeserializeObjectOfType<GameCommon.SerializedObjects.Packets.UpdateMission>(parameters[(byte)MessageParameterCode.Object]);
                //GameData.Player.UpdateStorage(missionUpdated.Storage);
                
                Debug.LogFormat("Mission updated {0}, {1}", missionUpdated.MissionId, missionUpdated.State);
            }
            else
            {
                // Show the error dialog
                // ShowError(response.DebugMessage);
                Debug.LogFormat("Mission error {0}", debugMessage);
            }
        }
    }
}
