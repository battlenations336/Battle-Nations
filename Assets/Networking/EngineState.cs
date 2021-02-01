using ExitGames.Client.Photon;
using System;

public class EngineState
{
    public static EngineState DisconnectedState = new EngineState() { OnUpdate = () => { }, SendRequest = (r, s, c, e) => { } };
    public static EngineState WaitingToConnectState = new EngineState() { OnUpdate = DoUpdate, SendRequest = DoSend };
    public static EngineState ConnectingState = new EngineState() { OnUpdate = DoUpdate, SendRequest = DoSend };
    public static EngineState ConnectedState = new EngineState() { OnUpdate = DoUpdate, SendRequest = DoSend };
    public Action OnUpdate { get; set; }
    public Action<OperationRequest, bool, byte, bool> SendRequest { get; set; }

    protected EngineState() { }

    protected static void DoUpdate() { PhotonEngine.instance.Peer.Service(); }
    protected static void DoSend(OperationRequest request, bool sendReliable, byte channelId, bool encrypt) { PhotonEngine.instance.Peer.OpCustom(request, sendReliable, channelId, encrypt); }
}
