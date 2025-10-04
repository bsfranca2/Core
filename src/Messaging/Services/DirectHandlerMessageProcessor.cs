using System.Diagnostics;

using Bsfranca2.Messaging.Contracts;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bsfranca2.Messaging.Services;

public class DirectHandlerMessageProcessor(
    IServiceProvider serviceProvider,
    ILogger<DirectHandlerMessageProcessor> logger
) : IMessageProcessor
{
    public async Task ProcessAsync(
        object message,
        Type messageType,
        MessageContext context,
        CancellationToken cancellationToken = default)
    {
        using IServiceScope scope = serviceProvider.CreateScope();

        Type wrapperImplementationType = typeof(EventHandlerWrapper<>).MakeGenericType(messageType);

        if (scope.ServiceProvider.GetService(wrapperImplementationType) is not IEventHandlerWrapper handlerWrapper)
        {
            DirectHandlerMessageProcessorLogs.LogNoHandlerWrapperFound(logger, messageType.Name);
            return;
        }

        string handlerTypeName = wrapperImplementationType.Name;
        Stopwatch stopwatch = Stopwatch.StartNew();
        try
        {
            DirectHandlerMessageProcessorLogs.LogExecutingHandlerWrapper(logger, handlerTypeName, messageType.Name,
                context.MessageId);

            await handlerWrapper.HandleAsync(message, cancellationToken);

            stopwatch.Stop();
            DirectHandlerMessageProcessorLogs.LogHandlerWrapperCompleted(logger, handlerTypeName, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            DirectHandlerMessageProcessorLogs.LogHandlerWrapperFailed(logger, ex, handlerTypeName, messageType.Name,
                stopwatch.ElapsedMilliseconds, context.MessageId);
            throw;
        }
    }
}

internal static partial class DirectHandlerMessageProcessorLogs
{
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "No handler wrapper found for event type {EventType}")]
    public static partial void LogNoHandlerWrapperFound(ILogger logger, string eventType);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Executing handler wrapper {HandlerType} for event {EventType} from message {MessageId}")]
    public static partial void LogExecutingHandlerWrapper(ILogger logger, string handlerType, string eventType, string messageId);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Handler wrapper {HandlerType} completed in {ElapsedMs}ms")]
    public static partial void LogHandlerWrapperCompleted(ILogger logger, string handlerType, long elapsedMs);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Handler wrapper {HandlerType} failed for event {EventType} after {ElapsedMs}ms. MessageId: {MessageId}")]
    public static partial void LogHandlerWrapperFailed(ILogger logger, Exception exception, string handlerType, string eventType,
        long elapsedMs, string messageId);
}