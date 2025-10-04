namespace Bsfranca2.Messaging.Contracts;

internal interface IEventHandlerWrapper
{
    Task HandleAsync(object message, CancellationToken cancellationToken);
}