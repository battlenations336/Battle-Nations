using BNR;
using ExitGames.Client.Photon;
using GameCommon;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.ClientHandlers
{
    public class AccountCreationHandler : GameMessageHandler
    {
        protected override void OnHandleMessage(Dictionary<byte, object> parameters, string debugMessage, int returnCode)
        {
            if (returnCode == (short)ReturnCode.OK)
            {
                // Show the login screen since it was successful
                Debug.LogFormat("Account Created Successfully");
                // Successful Login
                //SceneManager.LoadScene("Menu");
                MainMenu.instance.NewAccountCreated();

            }
            else
            {
                // Show the error dialog
                // ShowError(response.DebugMessage);
                MenuConfig.Error = debugMessage;
                Debug.LogFormat("{0}", debugMessage);
                SceneManager.LoadScene("Menu");
            }
        }
    }
}
