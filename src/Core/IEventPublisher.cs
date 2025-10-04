namespace Bsfranca2.Core;

public interface IEventPublisher
{
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class, IEvent;
    Task PublishAsync(object @event, Type eventType, CancellationToken cancellationToken = default);
}
