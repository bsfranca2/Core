using Bsfranca2.Messaging.Configurations;

using Microsoft.Extensions.Logging;

namespace Bsfranca2.Messaging.Providers.RabbitMQ;

internal static partial class RabbitMQInfrastructureManagerLogs
{
    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Setting up RabbitMQ infrastructure...")]
    internal static partial void LogSetupInfrastructureStart(ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "RabbitMQ infrastructure setup completed successfully.")]
    internal static partial void LogSetupInfrastructureComplete(ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "A critical error occurred while setting up RabbitMQ infrastructure.")]
    internal static partial void LogSetupInfrastructureError(ILogger logger, Exception exception);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Declared exchange {ExchangeName} of type {ExchangeType}.")]
    internal static partial void LogDeclareExchange(ILogger logger, string exchangeName, ExchangeType exchangeType);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Failed to declare exchange {ExchangeName}.")]
    internal static partial void LogDeclareExchangeError(ILogger logger, Exception exception, string exchangeName);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Declared queue {QueueName} with {MessageCount} messages and {ConsumerCount} consumers.")]
    internal static partial void LogDeclareQueue(ILogger logger, string queueName, uint messageCount, uint consumerCount);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Failed to declare queue {QueueName}.")]
    internal static partial void LogDeclareQueueError(ILogger logger, Exception exception, string queueName);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Bound queue {QueueName} to exchange {ExchangeName} with routing key {RoutingKey}.")]
    internal static partial void LogCreateBinding(ILogger logger, string queueName, string exchangeName, string routingKey);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Failed to bind queue {QueueName} to exchange {ExchangeName} with routing key {RoutingKey}.")]
    internal static partial void LogCreateBindingError(ILogger logger, Exception exception, string queueName, string exchangeName, string routingKey);
}