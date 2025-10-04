using RabbitMQ.Client;

namespace Bsfranca2.Messaging.Contracts;

public interface IInfrastructureManager
{
    Task EnsureInfrastructureAsync(IChannel channel, CancellationToken cancellationToken = default);
}