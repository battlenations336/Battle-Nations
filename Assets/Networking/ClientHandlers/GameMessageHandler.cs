using System.Collections.Generic;
using UnityEngine;

public abstract class GameMessageHandler : MonoBehaviour
{
    public GameMessage message;

    protected virtual void OnEnable()
    {
        message.Subscribe(this);
    }

    protected virtual void OnDisable()
    {
        message.Unsubscribe(this);
    }

    public void HandleMessage(Dictionary<byte, object> parameters, string debugMessage, int returnCode)
    {
        OnHandleMessage(parameters, debugMessage, returnCode);
    }

    protected abstract void OnHandleMessage(Dictionary<byte, object> parameters, string debugMessage, int returnCode);
}