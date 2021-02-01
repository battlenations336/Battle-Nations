using ExitGames.Client.Photon;
using GameCommon;
using GameCommon.Functions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AuthenticationView : MonoBehaviour
{

    public InputField CreateUserName;
    public InputField CreatePassword;
    public InputField LoginUserName;
    public InputField LoginPassword;

    string loginName;
    string password;

    public void SetAuthenticationValues(string _loginName, string _password)
    {
        loginName = _loginName;
        password = _password;
    }

    public void SendResponseRequest()
    {
        OperationRequest request = new OperationRequest() { OperationCode = 1, Parameters = new Dictionary<byte, object>() { { (byte)PhotonEngine.instance.SubCodeParameterCode, 1 } } };
        Debug.Log("Sending Request for Response");
        PhotonEngine.instance.SendRequest(request);
    }

    public void SendEventRequest()
    {
        OperationRequest request = new OperationRequest() { OperationCode = (byte)MessageOperationCode.Login, Parameters = new Dictionary<byte, object>() { { (byte)PhotonEngine.instance.SubCodeParameterCode, MessageSubCode.LoginUserPass } } };
        Debug.Log("Sending Request for Event");
        PhotonEngine.instance.SendRequest(request);
    }

    public void SendLoginRequest()
    {
        string gameVersion = GameVersion.Current();
        OperationRequest request = new OperationRequest() { OperationCode = (byte)MessageOperationCode.Login, Parameters = new Dictionary<byte, object>() { { (byte)PhotonEngine.instance.SubCodeParameterCode, MessageSubCode.LoginUserPass } } };
        request.Parameters.Add((byte)MessageParameterCode.LoginName, loginName);
        request.Parameters.Add((byte)MessageParameterCode.Password, password);
        request.Parameters.Add((byte)MessageParameterCode.Object, MessageSerializerService.SerializeObjectOfType(gameVersion));
        Debug.Log("Sending Request for Login");
        PhotonEngine.instance.SendRequest(request);

    }

    public void SendNewAccountRequest()
    {
        string gameVersion = GameVersion.Current();
        OperationRequest request = new OperationRequest() { OperationCode = (byte)MessageOperationCode.Login, Parameters = new Dictionary<byte, object>() { { (byte)PhotonEngine.instance.SubCodeParameterCode, MessageSubCode.LoginNewAccount } } };
        request.Parameters.Add((byte)MessageParameterCode.LoginName, loginName);
        request.Parameters.Add((byte)MessageParameterCode.Password, password);
        request.Parameters.Add((byte)MessageParameterCode.Object, MessageSerializerService.SerializeObjectOfType(gameVersion));
        Debug.Log("Sending Request for New Account");
        PhotonEngine.instance.SendRequest(request);

    }
}
