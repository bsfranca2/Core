using Bsfranca2.Messaging.Contracts;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using RabbitMQ.Client;

namespace Bsfranca2.Messaging.Providers.RabbitMQ;

internal sealed class InfrastructureInitializationService(
    RabbitMQConnectionFactory connectionFactory,
    IInfrastructureManager infrastructureManager,
    ILogger<InfrastructureInitializationService> logger
) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogDebug("Initializing messaging infrastructure...");

        try
        {
            await using IConnection connection = await connectionFactory.CreateDedicatedConnectionAsync("Infrastructure-Setup");
            await using IChannel channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await infrastructureManager.EnsureInfrastructureAsync(channel, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize messaging infrastructure");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogDebug("Infrastructure service stopping (no action required)");
        return Task.CompletedTask;
    }
}