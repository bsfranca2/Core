using Bsfranca2.Messaging.Contracts;

using MediatR;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bsfranca2.Messaging.Providers.Mediatr;

public class MediatRMessageProcessor(
    IServiceProvider serviceProvider,
    ILogger<MediatRMessageProcessor> logger
) : IMessageProcessor
{
    public async Task ProcessAsync(object message, Type messageType, MessageContext context,
        CancellationToken cancellationToken = default)
    {
        using IServiceScope scope = serviceProvider.CreateScope();

        Type adapterType = typeof(MediatREventAdapter<>).MakeGenericType(messageType);

        if (Activator.CreateInstance(adapterType, message) is not INotification notification)
        {
            logger.LogWarning("Could not create MediatR notification adapter for event type {EventType}", messageType.Name);
            return;
        }

        try
        {
            IMediator scopedMediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            await scopedMediator.Publish(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process message {MessageId} of type {EventType} via MediatR",
                context.MessageId, messageType.Name);
            throw;
        }
    }
}