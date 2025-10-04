using Microsoft.Extensions.Logging;

using RabbitMQ.Client;

namespace Bsfranca2.Messaging.Providers.RabbitMQ;

internal static partial class RabbitMQConnectionFactoryLogs
{
    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Created RabbitMQ connection for {ConnectionType}. Endpoint: {Endpoint}")]
    internal static partial void LogConnectionCreated(ILogger logger, string connectionType, AmqpTcpEndpoint endpoint);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Failed to create RabbitMQ connection for {ConnectionType}. ConnectionString: {ConnectionString}")]
    internal static partial void LogConnectionCreationFailed(ILogger logger, Exception ex, string connectionType, string connectionString);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "RabbitMQ connection {ConnectionType} shutdown: {ReplyText} (Code: {ReplyCode})")]
    internal static partial void LogConnectionShutdown(ILogger logger, string connectionType, string replyText, ushort replyCode);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "RabbitMQ connection {ConnectionType} blocked: {Reason}")]
    internal static partial void LogConnectionBlocked(ILogger logger, string connectionType, string reason);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "RabbitMQ connection {ConnectionType} unblocked")]
    internal static partial void LogConnectionUnblocked(ILogger logger, string connectionType);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "RabbitMQ connection {ConnectionType} recovery succeeded")]
    internal static partial void LogRecoverySucceeded(ILogger logger, string connectionType);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "RabbitMQ connection {ConnectionType} callback exception in {Detail}")]
    internal static partial void LogCallbackException(ILogger logger, Exception ex, string connectionType,
        IDictionary<string, object> detail);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "RabbitMQ connections disposed")]
    internal static partial void LogConnectionsDisposed(ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Error disposing RabbitMQ connections")]
    internal static partial void LogDisposeError(ILogger logger, Exception ex);
}