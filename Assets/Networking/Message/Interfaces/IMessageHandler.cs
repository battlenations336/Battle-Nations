
public interface IMessageHandler
{
    MessageType Type { get; }
    byte Code { get; }
    int? SubCode { get; }
    bool HandleMessage(IMessage message);
}