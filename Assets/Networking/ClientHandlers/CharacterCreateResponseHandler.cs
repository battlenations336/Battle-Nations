using GameCommon;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCreateResponseHandler : GameMessageHandler
{
    protected override void OnHandleMessage(Dictionary<byte, object> parameters, string debugMessage, int returnCode)
    {
        if (returnCode == (short)ReturnCode.OK)
        {
            // Show the login screen since it was successful
            Debug.LogFormat("Character Created Successfully");
        }
        else
        {
            // Show the error dialog
            // ShowError(response.DebugMessage);
            Debug.LogFormat("{0}", debugMessage);
        }
    }
}
