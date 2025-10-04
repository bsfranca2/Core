using Microsoft.Extensions.Logging;

namespace Bsfranca2.Messaging.Providers.RabbitMQ;

internal static partial class RabbitMQEventConsumerLogs
{
    // Start/Stop
    [LoggerMessage(Level = LogLevel.Debug, Message = "Starting RabbitMQ event consumer...")]
    internal static partial void LogStart(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "RabbitMQ event consumer started for {QueueCount} queues")]
    internal static partial void LogStartComplete(ILogger logger, int queueCount);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to start RabbitMQ event consumer")]
    internal static partial void LogStartFailed(ILogger logger, Exception ex);
    
    [LoggerMessage(Level = LogLevel.Debug, Message = "Stopping RabbitMQ event consumer...")]
    internal static partial void LogStop(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "RabbitMQ event consumer stopped")]
    internal static partial void LogStopComplete(ILogger logger);
    
    [LoggerMessage(Level = LogLevel.Error, Message = "A critical error occurred while stopping the RabbitMQ event consumer.")]
    internal static partial void LogStopFailed(ILogger logger, Exception ex);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Error canceling consumer {ConsumerTag}")]
    internal static partial void LogCancelConsumerError(ILogger logger, Exception ex, string consumerTag);

    // Consuming
    [LoggerMessage(Level = LogLevel.Information, Message = "Started consumer for queue {QueueName} with tag {ConsumerTag}")]
    internal static partial void LogConsumerStartedForQueue(ILogger logger, string queueName, string consumerTag);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to start consumer for queue {QueueName}")]
    internal static partial void LogStartConsumerFailed(ILogger logger, Exception ex, string queueName);

    // Message Processing
    [LoggerMessage(Level = LogLevel.Debug, Message = "Processing message {MessageId} of type {EventType} from queue {QueueName}")]
    internal static partial void LogProcessingMessage(ILogger logger, string messageId, string eventType, string queueName);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Successfully processed message {MessageId} of type {EventType}")]
    internal static partial void LogMessageProcessed(ILogger logger, string messageId, string eventType);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to process message {MessageId} of type {EventType} from queue {QueueName}")]
    internal static partial void LogProcessingFailed(ILogger logger, Exception ex, string messageId, string eventType, string queueName);
    
    [LoggerMessage(Level = LogLevel.Warning, Message = "Unknown event type: {EventType}")]
    internal static partial void LogUnknownEventType(ILogger logger, string eventType);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to deserialize message of type {EventType}")]
    internal static partial void LogDeserializationFailed(ILogger logger, string eventType);
    
    [LoggerMessage(Level = LogLevel.Debug, Message = "Message rejected and will be retried via DLQ")]
    internal static partial void LogMessageRequeued(ILogger logger);
    
    // Dispose
    [LoggerMessage(Level = LogLevel.Debug, Message = "RabbitMQ consumer connection disposed")]
    internal static partial void LogDisposed(ILogger logger);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Error during RabbitMQ consumer disposal")]
    internal static partial void LogDisposeError(ILogger logger, Exception ex);
}