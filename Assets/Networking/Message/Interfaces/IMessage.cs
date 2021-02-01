
using System.Collections.Generic;

public interface IMessage
{
    MessageType Type { get; }
    byte Code { get; }
    int? SubCode { get; }
    Dictionary<byte, object> Parameters { get; }
}
