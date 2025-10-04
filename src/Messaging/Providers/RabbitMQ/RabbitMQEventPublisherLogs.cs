using Bsfranca2.Messaging.Configurations;

using Microsoft.Extensions.Logging;

namespace Bsfranca2.Messaging.Providers.RabbitMQ;

internal static partial class RabbitMQEventPublisherLogs
{
    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Publishing event {EventType} to exchange {Exchange} with routing key {RoutingKey}")]
    internal static partial void LogPublishingEvent(ILogger logger, string eventType, string exchange, string routingKey);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Successfully published event {EventType} to exchange {Exchange}")]
    internal static partial void LogPublishedEventSuccess(ILogger logger, string eventType, string exchange);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Failed to publish event {EventType} to exchange {Exchange} with routing key {RoutingKey}")]
    internal static partial void LogPublishEventError(ILogger logger, Exception exception, string eventType, string exchange,
        string routingKey);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "RabbitMQ connection disposed")]
    internal static partial void LogDisposed(ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Error during RabbitMQ connection disposal")]
    internal static partial void LogDisposeError(ILogger logger, Exception exception);
}