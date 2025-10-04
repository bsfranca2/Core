namespace Bsfranca2.Messaging.Contracts;

public interface IMessageProcessor
{
    Task ProcessAsync(object message, Type messageType, MessageContext context, CancellationToken cancellationToken = default);
}

public class MessageContext
{
    public string MessageId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string QueueName { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object?> Headers { get; set; } = new();
    public int RetryCount { get; set; }
}