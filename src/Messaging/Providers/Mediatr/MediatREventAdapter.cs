using Bsfranca2.Core;

using MediatR;

namespace Bsfranca2.Messaging.Providers.Mediatr;

public class MediatREventAdapter<TEvent>(TEvent @event) : INotification
    where TEvent : IEvent
{
    public TEvent Event { get; } = @event ?? throw new ArgumentNullException(nameof(@event));
}