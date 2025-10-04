using Bsfranca2.Core;

using MediatR;

using Microsoft.Extensions.DependencyInjection;

namespace Bsfranca2.Messaging.Providers.Mediatr;

internal class MediatRHandlerAdapter<TEvent>(
    IServiceProvider serviceProvider
) : INotificationHandler<MediatREventAdapter<TEvent>>
    where TEvent : class, IEvent
{
    public async Task Handle(MediatREventAdapter<TEvent> notification, CancellationToken cancellationToken)
    {
        using IServiceScope scope = serviceProvider.CreateScope();

        IEnumerable<IEventHandler<TEvent>> handlers = scope.ServiceProvider.GetServices<IEventHandler<TEvent>>();
        
        IEnumerable<Task> tasks = handlers.Select(handler =>
            handler.HandleAsync(notification.Event, cancellationToken));

        await Task.WhenAll(tasks);
    }
}