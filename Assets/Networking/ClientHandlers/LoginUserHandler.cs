using BNR;
using ExitGames.Client.Photon;
using GameCommon;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.ClientHandlers
{
    public class LoginUserHandler : GameMessageHandler
    {
        protected override void OnHandleMessage(Dictionary<byte, object> parameters, string debugMessage, int returnCode)
        {
            if(returnCode == (short)ReturnCode.OK)
            {
                // Successful Login
                RequestProfile();
            }
            else
            {
                Debug.LogFormat("{0} - {1}", this.name, debugMessage);
                MainMenu.instance.PasswordEntry.SetError(debugMessage);
            }
        }

        public void RequestProfile()
        {
            OperationRequest request = new OperationRequest() { OperationCode = (byte)MessageOperationCode.Login, Parameters = new Dictionary<byte, object>() { { (byte)PhotonEngine.instance.SubCodeParameterCode, MessageSubCode.LoadProfile } } };
            Debug.Log("Sending Request for Profile");
            PhotonEngine.instance.SendRequest(request);
        }

    }
}
