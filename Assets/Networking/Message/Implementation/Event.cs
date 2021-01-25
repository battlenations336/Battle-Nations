using System.Collections.Generic;

public class Event : IMessage
{
    protected readonly byte _code;
    protected readonly int? _subCode;
    protected readonly Dictionary<byte, object> _parameters;

    public MessageType Type
    {
        get
        {
            return MessageType.Async;
        }
    }

    public byte Code { get { return _code; } }

    public int? SubCode { get { return _subCode; } }

    public Dictionary<byte, object> Parameters { get { return _parameters; } }

    public Event(byte code, int? subCode, Dictionary<byte, object> parameters)
    {
        _code = code;
        _subCode = subCode;
        _parameters = parameters;
    }
}
