
using BNR;
using ExitGames.Client.Photon;
using GameCommon;
using GameCommon.SerializedObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PhotonEngine : MonoBehaviour, IPhotonPeerListener
{
    protected List<GameMessage> eventMessageList = new List<GameMessage>();
    protected List<GameMessage> responseMessageList = new List<GameMessage>();
    public string ServerAddress;
    public string ApplicationName;
    public byte SubCodeParameterCode;
    public bool UseEncryption;
    public static PhotonEngine instance;

    public EngineState State { get; protected set; }

    public PhotonPeer Peer { get; protected set; }

    public int Ping { get; protected set; }

    private void Awake()
    {
        if ((UnityEngine.Object)PhotonEngine.instance == (UnityEngine.Object)null)
            PhotonEngine.instance = this;
        else if ((UnityEngine.Object)PhotonEngine.instance != (UnityEngine.Object)this)
            UnityEngine.Object.Destroy((UnityEngine.Object)this.gameObject);
        UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)this.gameObject);
        this.Initialize();
    }

    protected void Initialize()
    {
        this.State = EngineState.DisconnectedState;
        Application.runInBackground = true;
        this.GatherMessageHandlers();
        this.Peer = new PhotonPeer((IPhotonPeerListener)this, ConnectionProtocol.Tcp);
    }

    private void Start()
    {
        Debug.Log((object)string.Format("Connecting to {0}", (object)this.ServerAddress));
        this.ConnectToServer(this.ServerAddress, this.ApplicationName);
    }

    private void FixedUpdate()
    {
        this.Ping = this.Peer.RoundTripTime;
        this.State.OnUpdate();
    }

    private void OnApplicationQuit()
    {
        this.Disconnect();
        PhotonEngine.instance = (PhotonEngine)null;
    }

    public void SaveGame()
    {
        OperationRequest request = new OperationRequest()
        {
            OperationCode = 1,
            Parameters = new Dictionary<byte, object>()
      {
        {
          PhotonEngine.instance.SubCodeParameterCode,
          (object) MessageSubCode.SaveProfile
        },
        {
          (byte) 5,
          MessageSerializerService.SerializeObjectOfType<Profile>(GameData.Player.GetProfile())
        }
      }
        };
        Debug.Log((object)"Sending Request for Save Profile");
        PhotonEngine.instance.SendRequest(request);
    }

    public void Disconnect()
    {
        if (this.Peer != null && this.Peer.PeerState == PeerStateValue.Connected)
            this.Peer.Disconnect();
        this.State = EngineState.DisconnectedState;
    }

    public void ConnectToServer(string serverAddress, string applicationName)
    {
        if (this.State != EngineState.DisconnectedState)
            return;
        this.Peer.Connect(serverAddress, applicationName);
        this.State = EngineState.WaitingToConnectState;
    }

    public void GatherMessageHandlers()
    {
        foreach (GameMessage gameMessage in Resources.LoadAll<GameMessage>("GameMessages"))
        {
            if (gameMessage.messageType == MessageType.Async)
                this.eventMessageList.Add(gameMessage);
            else if (gameMessage.messageType == MessageType.Response)
                this.responseMessageList.Add(gameMessage);
        }
    }

    public void DebugReturn(DebugLevel level, string message)
    {
        Debug.Log((object)string.Format("Debug Return: {0} - {1}", (object)level, (object)message));
    }

    public void OnEvent(EventData eventData)
    {
        Event message = new Event(eventData.Code, (int?)eventData.Parameters[this.SubCodeParameterCode], eventData.Parameters);
        IEnumerable<GameMessage> source = this.eventMessageList.Where<GameMessage>((Func<GameMessage, bool>)(h =>
        {
            if (h.code != (MessageOperationCode)message.Code)
                return false;
            int subCode1 = (int)h.subCode;
            int? subCode2 = message.SubCode;
            int valueOrDefault = subCode2.GetValueOrDefault();
            return subCode1 == valueOrDefault & subCode2.HasValue;
        }));
        if (source == null || source.Count<GameMessage>() == 0)
            Debug.Log((object)string.Format("Attempted to handle event code:{0} - subCode:{1}", (object)message.Code, (object)message.SubCode));
        foreach (GameMessage gameMessage in source)
            gameMessage.Notify(message.Parameters, "", 0);
    }

    public void OnOperationResponse(OperationResponse operationResponse)
    {
        Response message = new Response(operationResponse.OperationCode, (int?)operationResponse.Parameters[this.SubCodeParameterCode], operationResponse.Parameters, operationResponse.DebugMessage, operationResponse.ReturnCode);
        IEnumerable<GameMessage> source = this.responseMessageList.Where<GameMessage>((Func<GameMessage, bool>)(h =>
        {
            if (h.code != (MessageOperationCode)message.Code)
                return false;
            int subCode1 = (int)h.subCode;
            int? subCode2 = message.SubCode;
            int valueOrDefault = subCode2.GetValueOrDefault();
            return subCode1 == valueOrDefault & subCode2.HasValue;
        }));
        if (source == null || source.Count<GameMessage>() == 0)
            Debug.Log((object)string.Format("Attempted to handle response code:{0} - subCode:{1}", (object)message.Code, (object)message.SubCode));
        foreach (GameMessage gameMessage in source)
            gameMessage.Notify(message.Parameters, message.DebugMessage, (int)message.ReturnCode);
    }

    public void OnStatusChanged(StatusCode statusCode)
    {
        Debug.Log((object)string.Format("Switching Status to {0}", (object)statusCode.ToString()));
        switch (statusCode)
        {
            case StatusCode.SecurityExceptionOnConnect:
            case StatusCode.ExceptionOnConnect:
            case StatusCode.Disconnect:
            case StatusCode.Exception:
            case StatusCode.ExceptionOnReceive:
            case StatusCode.TimeoutDisconnect:
            case StatusCode.DisconnectByServer:
            case StatusCode.DisconnectByServerUserLimit:
            case StatusCode.DisconnectByServerLogic:
            case StatusCode.EncryptionFailedToEstablish:
                this.State = EngineState.DisconnectedState;
                break;
            case StatusCode.Connect:
                if (this.UseEncryption)
                {
                    this.Peer.EstablishEncryption();
                    this.State = EngineState.ConnectingState;
                    break;
                }
                this.State = EngineState.ConnectedState;
                break;
            case StatusCode.EncryptionEstablished:
                this.State = EngineState.ConnectedState;
                break;
            default:
                this.State = EngineState.DisconnectedState;
                break;
        }
        if (this.State != EngineState.DisconnectedState)
            return;
        MenuConfig.Error = "Disconnected from server";
        SceneManager.LoadScene("Menu");
    }

    public void SendRequest(OperationRequest request)
    {
        this.State.SendRequest(request, true, (byte)0, this.UseEncryption);
    }

    public void SendRequest(
      MessageOperationCode code,
      MessageSubCode subCode,
      params object[] parameters)
    {
        OperationRequest request = new OperationRequest()
        {
            OperationCode = (byte)code,
            Parameters = new Dictionary<byte, object>()
      {
        {
          (byte) 0,
          (object) subCode
        }
      }
        };
        for (int index = 0; index < parameters.Length; index += 2)
        {
            if (!(parameters[index] is MessageParameterCode))
                throw new ArgumentException(string.Format("Parameter {0} is not a MessageParameterCode", (object)index));
            request.Parameters.Add((byte)parameters[index], parameters[index + 1]);
        }
        this.SendRequest(request);
    }
}
